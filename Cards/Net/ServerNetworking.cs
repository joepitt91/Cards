using JoePitt.Cards.Exceptions;
using NATUPNPLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace JoePitt.Cards.Net
{
    /// <summary>
    /// Provides Server Networking for a new game.
    /// </summary>
    internal class ServerNetworking : IDisposable
    {
        /// <summary>
        /// The Default port for games of Cards.
        /// </summary>
        public const int DefaultPort = 60069;
        /// <summary>
        /// The Port this game is being played on.
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// The IP:Port combinations that can be used to connect to this game.
        /// </summary>
        public List<ConnectionDetails> ConnectionStrings { get; private set; }

        /// <summary>
        /// The GUID to be used to verify Port Forwarding.
        /// </summary>
        public Guid PortCheckID { get; private set; }

        private Game Game;
        private IPAddress IPV4ExternalIPAddress;
        private TcpListener tcpListener;
        private List<Socket> ClientTcpClients;
        private bool ListenerUp;


        /// <summary>
        /// Initialise networking for the specified Game.
        /// </summary>
        /// <param name="game">The Game the networking is for.</param>
        public ServerNetworking(Game game)
        {
            Game = game;
            Port = DefaultPort;
            ConnectionStrings = new List<ConnectionDetails>();
            PortCheckID = Guid.NewGuid();
            ClientTcpClients = new List<Socket>();


            if (!DiscoverLocalPort())
            {
                throw new NoFreePortsException();
            }

            DiscoverLocalIPV4Addresses();
            DiscoverExternalIPV4Address();
            DiscoverIPV6Addresses();

            ThreadPool.QueueUserWorkItem(Listen);
        }

        private bool DiscoverLocalPort()
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();
            retest:
            foreach (TcpConnectionInformation tcpConnection in tcpConnections)
            {
                if (tcpConnection.LocalEndPoint.Port == Port)
                {
                    if (Port < DefaultPort + 100)
                    {
                        Port++;
                        goto retest;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void DiscoverLocalIPV4Addresses()
        {
            foreach (NetworkInterface iface in NetworkInterface.GetAllNetworkInterfaces().Where(iface =>
                (iface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                iface.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                iface.GetIPProperties().GatewayAddresses.Count > 0))
            {
                foreach (UnicastIPAddressInformation ip in iface.GetIPProperties().UnicastAddresses.Where(ip =>
                    ip.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    ConnectionStrings.Add(new ConnectionDetails(ip.Address, Port, false, false));
                    if (Game.GameType != 'L')
                    {
                        SetupUPNP(ip.Address);
                    }
                }
            }
        }

        private bool DiscoverExternalIPV4Address()
        {
            try
            {
                string externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(externalIP)[0].ToString();
                IPV4ExternalIPAddress = IPAddress.Parse(externalIP);
                ConnectionStrings.Add(new ConnectionDetails(IPV4ExternalIPAddress, Port, true, false));
                return true;
            }
            catch (WebException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void DiscoverIPV6Addresses()
        {
            foreach (NetworkInterface iface in NetworkInterface.GetAllNetworkInterfaces().Where(iface =>
                (iface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                iface.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                iface.GetIPProperties().GatewayAddresses.Count > 0))
            {
                foreach (UnicastIPAddressInformation ip in iface.GetIPProperties().UnicastAddresses.Where(ip =>
                    ip.Address.AddressFamily == AddressFamily.InterNetworkV6))
                {
                    if (!ip.Address.IsIPv6LinkLocal && !ip.Address.IsIPv6SiteLocal)
                    {
                        ConnectionStrings.Add(new ConnectionDetails(ip.Address, Port, true, false));
                        if (Game.GameType != 'L')
                        {
                            SetupUPNP(ip.Address);
                        }
                    }
                    else if (ip.Address.IsIPv6SiteLocal)
                    {
                        ConnectionStrings.Add(new ConnectionDetails(ip.Address, Port, false, false));
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void SetupUPNP(IPAddress LocalIP)
        {
            UPnPNAT upnpnat = new UPnPNAT();
            IStaticPortMappingCollection mappings = upnpnat.StaticPortMappingCollection;
            if (mappings == null)
            {
                return;
            }

            try
            {
                mappings.Add(Port, "TCP", Port, LocalIP.ToString(), true, "Cards");
            }
            catch { }
        }

        private void Listen(object state)
        {
            tcpListener = new TcpListener(IPAddress.Any, Port);
            try
            {
                tcpListener.Start();
                ListenerUp = true;
                Game.NetworkUp = true;
            }
            catch (SocketException)
            {
                ListenerUp = false;
                Game.NetworkUp = false;
                throw new ListenerStartException();
            }

            while (ListenerUp)
            {
                try
                {
                    if (tcpListener.Pending())
                    {
                        Socket clientSocket = tcpListener.AcceptSocket();
                        ClientTcpClients.Add(clientSocket);
                        ThreadPool.QueueUserWorkItem(ClientLink, clientSocket);
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                catch (SocketException) { }
            }
        }

        private void ClientLink(object myClientObj)
        {
            Socket myClient = (Socket)myClientObj;
            Player thisPlayer = new Player("FAKE", new List<Card>());

            while (myClient.Connected)
            {
                string clientText = SharedNetworking.GetString(myClient);

                if (string.IsNullOrEmpty(clientText))
                {
                    goto drop;
                }

                string serverText = "";
                string[] ClientTexts = clientText.Split(' ');
                switch (ClientTexts[0])
                {
                    /* Get Game Details
                     * EXPECTS
                     * [0]Command
                     * 
                     * RESPONSE
                     * HosterName PlayerInGame FreeSeats GUID_Version:GUID_Version
                     */
                    case "HELLO":
                        serverText = HandleHello();
                        break;

                    /* Try and (Re)Join the game.
                     * EXPECTS:
                     * [0]Command
                     * [1]Username 
                     * [2]Hash of Password
                     * 
                     * RESPONSE:
                     * SUCCESS
                     * FAILED NoSpace
                     */
                    case "JOIN":
                        Player newPlayer = HandleJoin(thisPlayer, ClientTexts);
                        if (newPlayer.Name.StartsWith("FAILED "))
                        {
                            serverText = newPlayer.Name;
                        }
                        else
                        {
                            thisPlayer = newPlayer;
                            serverText = "SUCCESS";
                        }
                        break;

                    /*
                     * Gets the GameSet.
                     * EXPECTS
                     * [0]Command
                     * 
                     * RESPONSE:
                     * Base64 Represenation of GameSet
                     * SETERROR
                     */
                    case "GETCARDS":
                        serverText = HandleGetCards();
                        break;

                    /*
                     * Exits the game
                     * EXPECTS:
                     * [0]Command
                     * 
                     * RESPONSE:
                     * RuleName:Value RuleName:Value RuleName:Value
                     */
                    case "GETRULES":
                        serverText = HandleGetRules();
                        break;

                    /*
                     * Updates the status of the game
                     * EXPECTS:
                     * [0]Command
                     * 
                     * RESPONSE:
                     * WAITING (PreGame)
                     * PLAYING BlackCardBase64 Round
                     * VOTING AnswersBase64
                     * END
                     */
                    case "GAMEUPDATE":
                        serverText = HandleGameUpdate();
                        break;

                    /*
                     * Updates the leaderboard
                     * EXPECTS:
                     * [0]Command
                     * 
                     * RESPONSE:
                     * PlayerName:Score,PlayerName:Score,PlayerName:Score
                     */
                    case "GAMESCORES":
                        serverText = HandleGameScores();
                        break;

                    /*
                     * Gets the players White Cards
                     * EXPECTS:
                     * [0]Command
                     * 
                     * RESPONSE
                     * CardID,CardID,CardID,CardID,CardID,CardID,CardID,CardID,CardID,CardID
                     * JoinFirst
                     */
                    case "MYCARDS":
                        serverText = HandleMyCards(thisPlayer);
                        break;

                    /* Find out if an action is required
                     * EXPECTS
                     * [0]Command
                     * 
                     * RESPONSE
                     * YES
                     * NO
                     */
                    case "NEED":
                        serverText = HandleNeed(thisPlayer, ClientTexts);
                        break;

                    /*
                     * Submits the players answer
                     * EXPECTS:
                     * [0]Command
                     * [1]Base64 Encoded Answer
                     * 
                     * RESPONSE:
                     * SUBMITTED
                     * ERROR 
                     */
                    case "SUBMIT":
                        serverText = HandleSubmit(thisPlayer, ClientTexts);
                        break;

                    /*
                     * Submits the players vote
                     * EXPECTS:
                     * [0]Command
                     * [1]Base64 Vote
                     * 
                     * RESPONSE:
                     * SUBMITTED
                     * ERROR 
                     * 
                     */
                    case "VOTE":
                        serverText = HandleVote(thisPlayer, ClientTexts);
                        break;

                    /*
                     * Updates the winners of the last round.
                     * 
                     * EXPECTS:
                     * [0]Command
                     * 
                     * RETURNS:
                     * [Binary Representation of Game.Winners]
                     */
                    case "GETWINNER":
                        serverText = HandleGetWinner();
                        break;

                    /*
                     * Performs a Special Action (e.g. Rebooting the Universe)
                     * EXPECTS:
                     * [0]Command
                     * [1]SpecialAbility
                     * 
                     * RESPONSE:
                     * SUCCESS
                     * FAILURE
                     * 
                     */
                    case "SPECIAL":
                        serverText = HandleSpecial(thisPlayer, ClientTexts);
                        break;

                    /*
                     * Performs a Cheating Action 
                     * EXPECTS:
                     * [0]Command
                     * [1]SpecialAbility
                     * 
                     * RESPONSE:
                     * SUCCESS
                     * FAILURE
                     * 
                     */
                    case "CHEAT":
                        serverText = HandleCheat(thisPlayer, ClientTexts);
                        break;

                    // Answer PortCheck Requests
                    case "PortCheck":
                        serverText = PortCheckID.ToString();
                        break;

                    /*
                     * Exits the game
                     * EXPECTS:
                     * [0]Command
                     */
                    case "EXIT":
                        goto drop;

                    //Reject HTTP Connections
                    case "GET":
                        goto drop;

                    // Catch All
                    default:
                        serverText = "Shut up meg!";
                        break;
                }

                if (!SharedNetworking.SendString(myClient, serverText))
                {
                    MessageBox.Show("A network error has occurred sending a response from your game host.",
                        "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            drop:
            try
            {
                myClient.SendTimeout = 3000;
                SharedNetworking.SendString(myClient, "BYE");
            }
            catch (SocketException)
            { }
            catch (ObjectDisposedException)
            { }
            myClient.Close();
        }

        private string HandleHello()
        {
            int players_H = 0;
            int maxPlayers_H = Game.PlayerCount;
            foreach (Player player in Game.Players)
            {
                if (player.Name != "FREESLOT" && !player.Name.ToLower().StartsWith("[bot]"))
                {
                    players_H++;
                }
            }
            string cardSets = "";
            foreach (CardSet set in Game.CardSets)
            {
                cardSets = cardSets + set.CardSetGuid.ToString() + "_" + set.Version + ":";
            }
            cardSets = cardSets.Substring(0, cardSets.Length - 1);

            if ((maxPlayers_H - players_H) == 0 && Game.Stage == 'W')
            {
                Game.Stage = 'P';
            }
            return Game.Players[0].Name.Replace(' ', '_') + " " + players_H + " " + (maxPlayers_H - players_H) + " " + cardSets;
        }

        private Player HandleJoin(Player thisPlayer, string[] ClientTexts)
        {
            int playersJoin = 0;
            int maxPlayersJoin = Game.PlayerCount;
            foreach (Player player in Game.Players)
            {
                if (player.Name != "FREESLOT" && !player.Name.ToLower().StartsWith("[bot]"))
                {
                    playersJoin++;
                }
            }

            if (ClientTexts.Length == 3)
            {
                bool found = false;
                foreach (Player player in Game.Players)
                {
                    if (player.Name == ClientTexts[1].Replace('_', ' '))
                    {
                        found = true;
                        if (player.Verify(ClientTexts[2]))
                        {
                            thisPlayer = player;
                            break;
                        }
                        else
                        {
                            thisPlayer.Name = "FAILED BadPass";
                            break;
                        }
                    }
                }
                if (!found)
                {
                    foreach (Player player in Game.Players)
                    {
                        if (player.Name == "FREESLOT")
                        {
                            player.Name = ClientTexts[1].Replace('_', ' ');
                            player.Verify(ClientTexts[2]);
                            found = true;
                            playersJoin++;
                            thisPlayer = player;
                            break;
                        }
                    }
                }
            }
            if ((maxPlayersJoin - playersJoin) == 0 && Game.Stage == 'W')
            {
                Game.Stage = 'P';
            }
            return thisPlayer;
        }

        private string HandleGetCards()
        {
            byte[] bytes = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                Program.Formatter.Serialize(stream, Game.GameSet);
                bytes = stream.ToArray();
            }
            return Convert.ToBase64String(bytes);
        }

        private string HandleGetRules()
        {
            string serverText = "CardsPerPlayer:" + Game.CardsPerUser;
            serverText = serverText + " Rounds:" + Game.Rounds;
            serverText = serverText + " NeverHaveI" + Game.NeverHaveI;
            serverText = serverText + " RebootingTheUniverse" + Game.RebootingTheUniverse;
            if (Game.Cheats)
            {
                serverText = serverText + " Cheats:true";
            }
            return serverText;
        }

        private string HandleGameUpdate()
        {
            switch (Game.Stage)
            {
                case 'W':
                    return "WAITING";
                case 'P':
                    return "PLAYING " + Convert.ToBase64String(Game.GameSet.BlackCards[Game.GameSet.BlackCardIndex[Game.CurrentBlackCard]].ToByteArray()) + " " + Game.Round;
                case 'V':
                    return "VOTING " + Convert.ToBase64String(Game.AnswersToByteArray());
                case 'E':
                    return "END";
                default:
                    return "X";
            }
        }

        private string HandleGameScores()
        {
            string serverText = "";
            foreach (Player player in Game.Players)
            {
                serverText = serverText + player.Name.Replace(' ', '_') + ":" + player.Score + ",";
            }
            serverText = serverText.Substring(0, serverText.Length - 1);
            return serverText;
        }

        private string HandleMyCards(Player thisPlayer)
        {
            if (thisPlayer.Name == "FAKE")
            {
                return "JoinFirst";
            }
            else
            {
                byte[] bytes = new byte[0];
                using (MemoryStream stream = new MemoryStream())
                {
                    Program.Formatter.Serialize(stream, thisPlayer.WhiteCards);
                    bytes = stream.ToArray();
                }
                return Convert.ToBase64String(bytes);
            }
        }

        private string HandleNeed(Player thisPlayer, string[] ClientTexts)
        {
            string serverText = "";
            if (ClientTexts.Length != 2)
            {
                serverText = "Usage: NEED [ANSWER|VOTE]";
            }
            else
            {
                switch (ClientTexts[1])
                {
                    case "ANSWER":
                        serverText = "YES";
                        foreach (Answer answer in Game.Answers)
                        {
                            if (answer.Submitter != null && answer.Submitter.Name == thisPlayer.Name)
                            {
                                serverText = "NO";
                            }
                        }
                        break;
                    case "VOTE":
                        serverText = "YES";
                        foreach (Vote vote in Game.Votes)
                        {
                            if (vote.Voter != null && vote.Voter.Name == thisPlayer.Name)
                            {
                                serverText = "NO";
                            }
                        }
                        break;
                    default:
                        serverText = "Usage: NEED [ANSWER|VOTE]";
                        break;
                }
            }
            return serverText;
        }

        private string HandleSubmit(Player thisPlayer, string[] ClientTexts)
        {
            string serverText = "";
            if (ClientTexts.Length != 2)
            {
                serverText = "Usage: SUBMIT Base64Answer";
            }
            else
            {
                int i = 0;
                foreach (Answer answer in Game.Answers)
                {
                    if (answer.Submitter.Name == thisPlayer.Name)
                    {
                        Game.Answers[i] = new Answer(Convert.FromBase64String(ClientTexts[1]));
                        serverText = "SUBMITTED";
                        break;
                    }
                    i++;
                }
                if (serverText != "SUBMITTED")
                {
                    Game.Answers.Add(new Answer(Convert.FromBase64String(ClientTexts[1])));
                    serverText = "SUBMITTED";
                }
            }
            if (serverText != "SUBMITTED")
            {
                serverText = "ERROR";
            }
            else
            {
                if (Game.Answers.Count == (Game.PlayerCount + Game.BotCount))
                {
                    Game.Answers = Dealer.ShuffleAnswers(Game.Answers);
                    Game.Stage = 'V';
                }
            }
            return serverText;
        }

        private string HandleVote(Player thisPlayer, string[] ClientTexts)
        {
            string serverText = "";
            if (ClientTexts.Length != 2)
            {
                return "Usage: VOTE Base64Vote";
            }
            else
            {
                if (Game.Votes.Count == Game.PlayerCount &&
                    Game.Votes[0].Choice.BlackCard != Game.GameSet.BlackCards[Game.GameSet.BlackCardIndex[Game.CurrentBlackCard]])
                {
                    Game.Winners = new List<Answer>();
                }
                else if (Game.Winners == null)
                {
                    Game.Winners = new List<Answer>();
                }
                int i = 0;
                foreach (Vote vote in Game.Votes)
                {
                    if (vote.Voter.Name == thisPlayer.Name)
                    {
                        byte[] voteIn = Convert.FromBase64String(ClientTexts[1]);
                        using (MemoryStream stream = new MemoryStream(voteIn))
                        {
                            stream.Position = 0;
                            Vote thisVote = (Vote)Program.Formatter.Deserialize(stream);
                            if (thisVote.Choice.BlackCard != Game.GameSet.BlackCards[Game.GameSet.BlackCardIndex[Game.CurrentBlackCard]])
                            {
                                serverText = "ERROR";
                            }
                            else
                            {
                                Game.Votes[i] = thisVote;
                                serverText = "SUBMITTED";
                            }
                        }
                        break;
                    }
                    i++;
                }
                if (serverText != "SUBMITTED")
                {
                    byte[] voteBytes = Convert.FromBase64String(ClientTexts[1]);
                    using (MemoryStream stream = new MemoryStream(voteBytes))
                    {
                        stream.Position = 0;
                        Game.Votes.Add((Vote)Program.Formatter.Deserialize(stream));
                        serverText = "SUBMITTED";
                    }
                }
            }
            if (serverText != "SUBMITTED")
            {
                serverText = "ERROR";
            }
            else
            {
                if (Game.Votes.Count == Game.PlayerCount)
                {
                    Game.Winners = new List<Answer>();
                    Dictionary<string, int> results = new Dictionary<string, int>();
                    foreach (Answer answer in Game.Answers)
                    {
                        results.Add(answer.Submitter.Name + "/" + answer.ToString, 0);
                    }
                    foreach (Vote vote in Game.Votes)
                    {
                        results[vote.Choice.Submitter.Name + "/" + vote.Choice.ToString]++;
                    }
                    List<KeyValuePair<string, int>> myList = results.ToList();
                    myList.Sort(delegate (KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2)
                    {
                        return pair2.Value.CompareTo(pair1.Value);
                    });
                    int winningVotes = myList[0].Value;
                    foreach (KeyValuePair<string, int> answer in myList)
                    {
                        if (answer.Value == winningVotes)
                        {
                            string[] winningAnswer = answer.Key.Split('/');
                            foreach (Answer winner in Game.Answers)
                            {
                                if (winner.Submitter.Name == winningAnswer[0] && winner.ToString == winningAnswer[1])
                                {
                                    Game.Winners.Add(winner);
                                    foreach (Player player in Game.Players)
                                    {
                                        if (player.Name == winner.Submitter.Name)
                                        {
                                            player.Score++;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    Game.NextRound();
                }
            }
            return serverText;
        }

        private string HandleGetWinner()
        {
            return Convert.ToBase64String(Game.WinnersToByteArray());
        }

        private string HandleSpecial(Player thisPlayer, string[] ClientTexts)
        {
            string test = thisPlayer.Name + Game.Stage;
            if (ClientTexts.Length != 2)
            {
                return "Usage: SPECIAL Action" + test;
            }
            else
            {
                return "NOT IMPLEMENTED YET!" + test;
            }
        }

        private string HandleCheat(Player thisPlayer, string[] ClientTexts)
        {
            string test = thisPlayer.Name + Game.Stage;
            if (ClientTexts.Length != 2)
            {
                return "Usage: CHEAT Action" + test;
            }
            else
            {
                return "NOT IMPLEMENTED YET!" + test;
            }
        }

        public void Close()
        {
            ListenerUp = false;
            foreach (Socket thisClient in ClientTcpClients)
            {
                thisClient.Close();
            }
            tcpListener.Stop();
            Game.NetworkUp = false;
            DropUPNP();
            Dispose();
        }

        private void DropUPNP()
        {

            UPnPNAT upnpnat = new UPnPNAT();
            IStaticPortMappingCollection mappings = upnpnat.StaticPortMappingCollection;
            try
            {
                mappings.Remove(Port, "TCP");
            }
            catch (System.Runtime.InteropServices.COMException) { }
            catch (System.IO.FileNotFoundException) { }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ConnectionStrings.Clear();
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ServerNetworking() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
