using JoePitt.Cards.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace JoePitt.Cards
{
    /// <summary>
    /// A Game of Cards.
    /// </summary>
    [Serializable]
    internal class Game
    {
        /// <summary>
        /// If the game is ready to be played.
        /// </summary>
        public bool Playable { get; set; }
        /// <summary>
        /// Identifies if the game is Local (L), Hosting (H) or Joining (J).
        /// </summary>
        public char GameType { get; private set; }

        //Networking
        /// <summary>
        /// The hosting network provider, for hosted games.
        /// </summary>
        
        public ServerNetworking HostNetwork { get { return HostNetworkField; } private set { HostNetworkField = value; } }
        [NonSerialized]
        private ServerNetworking HostNetworkField;
        /// <summary>
        /// All the client networking proviers for this instance.
        /// </summary>
        public List<ClientNetworking> LocalPlayers { get { return LocalPlayersField; } set { LocalPlayersField = value; } }
        [NonSerialized]
        private List<ClientNetworking> LocalPlayersField;
        /// <summary>
        /// If networking is up and running.
        /// </summary>
        public bool NetworkUp { get; set; }

        //Settings
        /// <summary>
        /// The maximum number of human players.
        /// </summary>
        public int PlayerCount { get; private set; }
        /// <summary>
        /// The number of automated players.
        /// </summary>
        public int BotCount { get; private set; }
        /// <summary>
        /// Human and automated players.
        /// </summary>
        public List<Player> Players { get; private set; }
        /// <summary>
        /// The Card Sets that are in use in the game.
        /// </summary>
        public List<CardSet> CardSets { get; private set; }
        /// <summary>
        /// If cheats are enabled.
        /// </summary>
        public bool Cheats { get; private set; }
        /// <summary>
        /// The number of cards each play should have.
        /// </summary>
        public int CardsPerUser { get; private set; }
        /// <summary>
        /// The number of rounds to play.
        /// </summary>
        public int Rounds { get; private set; }
        /// <summary>
        /// Enable Never Have I - Allowing swapping of cards that are not understood.
        /// </summary>
        public bool NeverHaveI { get; private set; }
        /// <summary>
        /// Enable Rebooting The Universie - Allowing entire hands to be re dealt.
        /// </summary>
        public bool RebootingTheUniverse { get; private set; }

        //Progress
        /// <summary>
        /// The status of the game:
        /// W: Waiting for players to Join.
        /// P: Accepting answers to the current Black Card (Playing ).
        /// V: Accepting votes on the best answers (Voting).
        /// E: End of Game.
        /// </summary>
        public char Stage { get; set; }
        /// <summary>
        /// The, potentially merged, set of cards to use for the game.
        /// </summary>
        public CardSet GameSet { get; private set; }
        /// <summary>
        /// The Index of the current Black Card, from the shuffled CardSet.BlackCardIndex.
        /// </summary>
        public int CurrentBlackCard { get; private set; }
        /// <summary>
        /// The Index of the top White Card, from the shuffled CardSet.WhiteCardIndex.
        /// </summary>
        public int CurrentWhiteCard { get; private set; }
        /// <summary>
        /// The current round of the game.
        /// </summary>
        public int Round { get; set; }
        /// <summary>
        /// Answers that have been submitted for this round.
        /// </summary>
        public List<Answer> Answers { get; set; }
        /// <summary>
        /// Votes that have been submitted for this round.
        /// </summary>
        public List<Vote> Votes { get; private set; }
        /// <summary>
        /// The winning answers from the previous round.
        /// </summary>
        public List<Answer> Winners { get; set; }

        /// <summary>
        /// Initalise a game including the global settings.
        /// </summary>
        /// <param name="type">The Type of game to setup.</param>
        /// <param name="playerNames">List of player names.</param>
        public Game(char type, List<string> playerNames)
        {
            if (playerNames == null) { throw new ArgumentNullException("playerNames"); }
            GameType = type;
            // Settings
            Round = 0;

            if (type != 'J')
            {
                Rounds = Properties.Settings.Default.Rounds;
                CardsPerUser = Properties.Settings.Default.Cards;
                NeverHaveI = Properties.Settings.Default.NeverHaveI;
                RebootingTheUniverse = Properties.Settings.Default.Rebooting;
                Cheats = Properties.Settings.Default.Cheats;

                // Deal First Hand
                PrepareDeck();
                List<Tuple<int, Card>> hands = new List<Tuple<int, Card>>();
                try
                {
                    hands = Dealer.Deal(GameSet, playerNames.Count, CardsPerUser);
                }
                catch (ApplicationException ex)
                {
                    MessageBox.Show("Error, game not started. " + ex.Message, "Game Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Playable = false;
                    return;
                }

                // Setup Players
                Players = new List<Player>();
                int player = 0;
                foreach (string playerName in playerNames)
                {
                    List<Card> hand = new List<Card>();
                    foreach (Tuple<int, Card> card in hands.GetRange(player * CardsPerUser, CardsPerUser))
                    {
                        hand.Add(card.Item2);
                    }
                    Players.Add(new Player(playerName, hand));
                    if (Players[player].Name.ToLower().StartsWith("[bot]"))
                    {
                        BotCount++;
                    }
                    else
                    {
                        PlayerCount++;
                    }
                    player++;
                }

                //get game ready
                CurrentBlackCard = 0;

                HostNetwork = new ServerNetworking(this);
                LocalPlayers = new List<ClientNetworking>();
                Stage = 'W';
                Playable = true;
                NextRound();
            }
            else
            {
                Players = new List<Player>
                {
                    new Player(playerNames[0], new List<Card>())
                };
            }
        }

        /// <summary>
        /// Shutdown the current game.
        /// </summary>
        /// <returns>If the game was stopped.</returns>
        public bool Stop()
        {
            Playable = false;
            if (HostNetwork != null)
            {
                HostNetwork.Close();
            }
            if (LocalPlayers != null)
            {
                foreach (ClientNetworking playerNetwork in LocalPlayers)
                {
                    playerNetwork.NextCommand = "EXIT";
                    playerNetwork.NewCommand = true;
                }
            }
            return true;
        }

        /// <summary>
        /// Join a Remote Game.
        /// </summary>
        /// <param name="address">Hostname, IPv4 or IPv6 Address of game.</param>
        /// <param name="port">TCP Port of Game.</param>
        /// <returns>If Game was joined.</returns>
        public bool Join(string address, int port)
        {
            StartJoin:
            LocalPlayers = new List<ClientNetworking>();
            ClientNetworking playerNetwork;
            playerNetwork = new ClientNetworking(Players[0], address, port);
            LocalPlayers.Add(playerNetwork);
            playerNetwork.NextCommand = "HELLO";
            playerNetwork.NewCommand = true;
            while (!playerNetwork.NewResponse)
            {
                Application.DoEvents();
            }
            string ServerHello = playerNetwork.LastResponse;
            playerNetwork.NewResponse = false;
            string[] ServerDetails = ServerHello.Split(' ');

            if (ServerDetails[3] != "0")
            {
                playerNetwork.NextCommand = "JOIN " + Players[0].Name.Replace(' ', '_') + " " + Program.SessionKey;
                playerNetwork.NewCommand = true;
                while (!playerNetwork.NewResponse)
                {
                    Application.DoEvents();
                }
                string ServerResponse = playerNetwork.LastResponse;
                playerNetwork.NewResponse = false;
                if (ServerResponse == "SUCCESS")
                {
                    Program.CurrentPlayer = LocalPlayers[0];
                    goto SetupGame;
                }
                else
                {
                    string reason = "Unknown Error";
                    if (ServerResponse.StartsWith("FAILED "))
                    {
                        switch (ServerResponse.Split(' ')[1])
                        {
                            case "NoSpace":
                                if (MessageBox.Show("Failed  to Join Game: No space in game.", "Join Failed", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) == DialogResult.Retry)
                                {
                                    goto StartJoin;
                                }
                                else
                                {
                                    playerNetwork.NextCommand = "EXIT";
                                    playerNetwork.NewCommand = true;
                                    while (!playerNetwork.NewResponse)
                                    {
                                        Application.DoEvents();
                                    }
                                    return false;
                                }
                            case "BadPass":
                                reason = "Name in use or Password incorrect";
                                break;
                        }
                    }
                    MessageBox.Show("Failed to Join Game: " + reason, "Failed to Join", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    playerNetwork.NextCommand = "EXIT";
                    playerNetwork.NewCommand = true;
                    while (!playerNetwork.NewResponse)
                    {
                        Application.DoEvents();
                    }
                    return false;
                }
                SetupGame:
                Program.CurrentPlayer.NextCommand = "GETRULES";
                Program.CurrentPlayer.NewCommand = true;
                while (!Program.CurrentPlayer.NewResponse)
                {
                    Application.DoEvents();
                }
                string[] responses = Program.CurrentPlayer.LastResponse.Split(' ');
                Program.CurrentPlayer.NewResponse = false;
                foreach (string setting in responses)
                {
                    string[] settingPair = setting.Split(':');
                    switch (settingPair[0])
                    {
                        case "CardsPerPlayer":
                            CardsPerUser = Convert.ToInt32(settingPair[1]);
                            break;
                        case "Rounds":
                            Rounds = Convert.ToInt32(settingPair[1]);
                            break;
                        case "NeverHaveI":
                            NeverHaveI = Convert.ToBoolean(settingPair[1]);
                            break;
                        case "RebootingTheUniverse":
                            RebootingTheUniverse = Convert.ToBoolean(settingPair[1]);
                            break;
                        case "Cheats":
                            Cheats = Convert.ToBoolean(settingPair[1]);
                            break;
                    }
                }

                Program.CurrentPlayer.NextCommand = "GETCARDS";
                Program.CurrentPlayer.NewCommand = true;
                while (!Program.CurrentPlayer.NewResponse)
                {
                    Application.DoEvents();
                }
                string response = Program.CurrentPlayer.LastResponse;
                Program.CurrentPlayer.NewResponse = false;

                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(response)))
                {
                    stream.Position = 0;
                    GameSet = (CardSet)Program.Formatter.Deserialize(stream);
                }
                Playable = true;
                return true;
            }
            else
            {
                if (MessageBox.Show("Failed to Join Game: No space in game.", "Join Failed", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) == DialogResult.Retry)
                {
                    goto StartJoin;
                }
                else
                {
                    playerNetwork.NextCommand = "EXIT";
                    playerNetwork.NewCommand = true;
                    while (!playerNetwork.NewResponse)
                    {
                        Application.DoEvents();
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Prepare the Card Deck for the game.
        /// </summary>
        /// <returns>If the deck was successfully prepared.</returns>
        private bool PrepareDeck()
        {
            CardSets = new List<CardSet>();
            int sets = 0;
            List<string> MergeList = new List<string>();
            foreach (string[] set in Dealer.GetCardSets())
            {
                // GUID, Name, Version, Path, Hash Status
                foreach (string enabledSet in Properties.Settings.Default.CardSets)
                {
                    string enabledGuid = enabledSet.Substring(enabledSet.IndexOf('_') + 1);
                    if (set[0] == enabledGuid && set[4] == "OK")
                    {
                        CardSets.Add(new CardSet(set[0]));
                        if (sets == 0)
                        {
                            GameSet = new CardSet(set[0]);
                            sets++;
                        }
                        else
                        {
                            MergeList.Add(set[0]);
                        }
                        break;
                    }
                }
            }
            if (sets > 0)
            {
                GameSet.Merge(MergeList);
            }

            if (GameSet != null && GameSet.BlackCards.Count > 0 && GameSet.WhiteCards.Count > 0)
            {
                CurrentWhiteCard = ((PlayerCount + BotCount) * CardsPerUser) + 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Setups up the next round.
        /// </summary>
        /// <returns>if the next round is ready.</returns>
        public bool NextRound()
        {
            if (Round + 1 > Rounds)
            {
                Stage = 'E';
                return false;
            }
            else
            {
                if (Stage != 'W')
                {
                    Stage = 'P';
                }
                if (Round == 0)
                {
                    Winners = new List<Answer>();
                }
                Round++;
                if (CurrentBlackCard + 1 > GameSet.BlackCards.Count)
                {
                    CurrentBlackCard = 1;
                }
                else
                {
                    CurrentBlackCard++;
                }
                if (Round > 1)
                {
                    foreach (Answer answer in Answers)
                    {
                        NextWhiteCard();
                        string remove = answer.WhiteCard.Id;
                        int i = 0;
                        foreach (Card card in answer.Submitter.WhiteCards)
                        {
                            if (card.Id == remove)
                            {
                                answer.Submitter.WhiteCards.RemoveAt(i);
                                break;
                            }
                            i++;
                        }
                        answer.Submitter.WhiteCards.Add(GameSet.WhiteCards[GameSet.WhiteCardIndex[CurrentWhiteCard]]);
                        i = 0;
                        foreach (Player player in Program.CurrentGame.Players)
                        {
                            if (player.Name == answer.Submitter.Name)
                            {
                                Program.CurrentGame.Players[i].WhiteCards = answer.Submitter.WhiteCards;
                                break;
                            }
                            i++;
                        }

                        if (answer.BlackCard.Needs == 2)
                        {
                            NextWhiteCard();
                            remove = answer.WhiteCard2.Id;
                            i = 0;
                            foreach (Card card in answer.Submitter.WhiteCards)
                            {
                                if (card.Id == remove)
                                {
                                    answer.Submitter.WhiteCards.RemoveAt(i);
                                    break;
                                }
                                i++;
                            }
                            answer.Submitter.WhiteCards.Add(GameSet.WhiteCards[GameSet.WhiteCardIndex[CurrentWhiteCard]]);
                            i = 0;
                            foreach (Player player in Program.CurrentGame.Players)
                            {
                                if (player.Name == answer.Submitter.Name)
                                {
                                    Program.CurrentGame.Players[i].WhiteCards = answer.Submitter.WhiteCards;
                                    break;
                                }
                                i++;
                            }
                        }
                    }
                }
                Answers = new List<Answer>();

                foreach (Player player in Players)
                {
                    if (player.IsBot)
                    {
                        Answer botAnswer;
                        if (GameSet.BlackCards[GameSet.BlackCardIndex[CurrentBlackCard]].Needs == 1)
                        {
                            botAnswer = new Answer(player, GameSet.BlackCards[GameSet.BlackCardIndex[CurrentBlackCard]], player.WhiteCards[0]);
                        }
                        else
                        {
                            botAnswer = new Answer(player, GameSet.BlackCards[GameSet.BlackCardIndex[CurrentBlackCard]], player.WhiteCards[0], player.WhiteCards[1]);
                        }
                        Answers.Add(botAnswer);
                    }
                }

                Votes = new List<Vote>();
                return true;
            }
        }

        /// <summary>
        /// Finds the next available White Card.
        /// </summary>
        private void NextWhiteCard()
        {
            // Prevent Double Dealing.
            bool CardUsed = true;
            while (CardUsed)
            {
                bool found = false;
                foreach (Player player in Players)
                {
                    Card whiteCard = GameSet.WhiteCards[GameSet.WhiteCardIndex[CurrentWhiteCard]];
                    foreach (Card card in player.WhiteCards)
                    {
                        if (card.Id == whiteCard.Id)
                        {
                            found = true;
                            if (CurrentWhiteCard + 1 > GameSet.WhiteCardCount)
                            {
                                CurrentWhiteCard = 1;
                            }
                            else
                            {
                                CurrentWhiteCard++;
                            }
                            break;
                        }
                    }
                    if (found) break;
                }
                if (!found) CardUsed = false;
            }
        }

        /// <summary>
        /// Converts the Submitted Answers to a byte array.
        /// </summary>
        /// <returns>The Binary Representation of the Submitted Answers.</returns>
        public byte[] AnswersToByteArray()
        {
            byte[] bytes = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Position = 0;
                Program.Formatter.Serialize(stream, Answers);
                bytes = stream.ToArray();
            }
            return bytes;
        }

        /// <summary>
        /// Converts the Winners list to a byte array.
        /// </summary>
        /// <returns>The Binary Representation of the Winners List.</returns>
        public byte[] WinnersToByteArray()
        {
            byte[] bytes = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                Program.Formatter.Serialize(stream, Winners);
                bytes = stream.ToArray();
            }
            return bytes;
        }
    }
}