using JoePitt.Cards.Net;
using System;
using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;
using System.Xml.Linq;

namespace JoePitt.Cards
{
    internal class CrashReport
    {
        public CrashReport(Exception ex)
        {
            XElement rootNode = new XElement("CardsCrashReport");

            XElement environment = new XElement("Environment");
            try
            {
                KeyValuePair<string, string> operatingSystem = GetOSVersionAndCaption();
                environment.Add(new XElement("OperatingSystem", operatingSystem.Key + " (" + operatingSystem.Value + ")"));
            }
            catch {
                environment.Add(new XAttribute("PopulateFailed", true));
            }
            if (Environment.Is64BitOperatingSystem)
            {
                environment.Add(new XElement("OSArchitecture", "64-bit"));
            }
            else
            {
                environment.Add(new XElement("OSArchitecture", "32-bit"));
            }
            environment.Add(new XElement("CPUCount", Environment.ProcessorCount));
            environment.Add(new XElement("CLRVersion", Environment.Version));
            environment.Add(new XElement("CardsVersion", Application.ProductVersion));
            environment.Add(new XElement("RunningFrom", Environment.CurrentDirectory));
            rootNode.Add(environment);

            XElement exception = new XElement("Exception");
            exception.Add(new XElement("Data", ex.Data));
            exception.Add(new XElement("Message", ex.Message));
            exception.Add(new XElement("StackTrace", ex.StackTrace));
            rootNode.Add(exception);

            XElement gameStatus = new XElement("GameStatus");
            if (Program.CurrentGame == null)
            {
                gameStatus.Add(new XElement("GameStarted", false));
            }
            else
            {
                gameStatus.Add(new XElement("GameStarted", true));
                gameStatus.Add(new XElement("Playable", Program.CurrentGame.Playable));
                gameStatus.Add(new XElement("NetworkUp", Program.CurrentGame.NetworkUp));
                gameStatus.Add(new XElement("GameType", Program.CurrentGame.GameType));
                gameStatus.Add(new XElement("Rounds", Program.CurrentGame.Rounds));
                gameStatus.Add(new XElement("BotCount", Program.CurrentGame.BotCount));
                gameStatus.Add(new XElement("PlayerCount", Program.CurrentGame.PlayerCount));
                gameStatus.Add(new XElement("CardsPerPlayer", Program.CurrentGame.CardsPerUser));
                gameStatus.Add(new XElement("Cheats", Program.CurrentGame.Cheats));
                gameStatus.Add(new XElement("NeverHaveI", Program.CurrentGame.NeverHaveI));
                gameStatus.Add(new XElement("RebootingTheUniverse", Program.CurrentGame.RebootingTheUniverse));

                XElement cardSets = new XElement("CardSets");
                try
                {
                    cardSets.Add(new XAttribute("Count", Program.CurrentGame.CardSets.Count));
                    foreach (CardSet cardSet in Program.CurrentGame.CardSets)
                    {
                        XElement xCardSet = new XElement("CardSet");
                        xCardSet.Add(new XAttribute("Name", cardSet.Name));
                        xCardSet.Add(new XAttribute("Version", cardSet.Version));
                        xCardSet.Add(new XAttribute("GUID", cardSet.CardSetGuid));

                        XElement blackCards = new XElement("BlackCard");
                        blackCards.Add(new XAttribute("Count", cardSet.BlackCardCount));
                        foreach (KeyValuePair<string, Card> card in cardSet.BlackCards)
                        {
                            XElement blackCard = new XElement("Card");
                            blackCard.Add(new XAttribute("ID", card.Key));
                            blackCard.Add(new XAttribute("Needs", card.Value.Needs));
                            blackCard.Add(card.Value.ToString);
                            blackCards.Add(blackCard);
                        }
                        xCardSet.Add(blackCards);

                        XElement whiteCards = new XElement("WhiteCard");
                        whiteCards.Add(new XAttribute("Count", cardSet.WhiteCardCount));
                        foreach (KeyValuePair<string, Card> card in cardSet.WhiteCards)
                        {
                            XElement whiteCard = new XElement("Card");
                            whiteCard.Add(new XAttribute("ID", card.Key));
                            whiteCard.Add(card.Value.ToString);
                            whiteCards.Add(whiteCard);
                        }
                        xCardSet.Add(whiteCards);
                        cardSets.Add(xCardSet);
                    }
                }
                catch
                {
                    cardSets.Add(new XAttribute("PopulateFailed", true));
                }
                gameStatus.Add(cardSets);

                if (Program.CurrentGame.GameType == 'H')
                {
                    XElement serverNetworking = new XElement("ServerNetworking");
                    serverNetworking.Add(new XElement("Port", Program.CurrentGame.HostNetwork.Port));
                    serverNetworking.Add(new XElement("IPCount", Program.CurrentGame.HostNetwork.ConnectionStrings.Count));
                    gameStatus.Add(serverNetworking);
                }

                XElement clients = new XElement("LocalClients");
                try
                {
                    clients.Add(new XAttribute("Count", Program.CurrentGame.LocalPlayers.Count));
                    foreach (ClientNetworking player in Program.CurrentGame.LocalPlayers)
                    {
                        XElement client = new XElement("Client");
                        client.Add(new XElement("Name", player.Owner.Name));
                        client.Add(new XElement("HasEstablished", player.Established));
                        client.Add(new XElement("HasDropped", player.Dropped));
                        XElement command = new XElement("Command", player.NextCommand);
                        command.Add(new XAttribute("New", player.NewCommand));
                        client.Add(command);
                        XElement response = new XElement("Response", player.LastResponse);
                        response.Add(new XAttribute("New", player.NewResponse));
                        client.Add(response);
                        clients.Add(client);
                    }
                }
                catch
                {
                    clients.Add(new XAttribute("PopulateFailed", true));
                }
                gameStatus.Add(clients);

                XElement players = new XElement("Players");
                try
                {
                    players.Add(new XAttribute("Count", Program.CurrentGame.Players.Count));
                    foreach (Player player in Program.CurrentGame.Players)
                    {
                        XElement xPlayer = new XElement("Player");
                        xPlayer.Add(new XAttribute("Name", player.Name));
                        xPlayer.Add(new XAttribute("IsBot", player.IsBot));
                        xPlayer.Add(new XAttribute("Score", player.Score));
                        xPlayer.Add(new XAttribute("WhiteCardCount", player.WhiteCards.Count));
                        foreach (Card card in player.WhiteCards)
                        {
                            XElement xCard = new XElement("Card");
                            xCard.Add(new XAttribute("ID", card.Id));
                            xCard.Add(card.ToString);
                            xPlayer.Add(xCard);
                        }
                        players.Add(xPlayer);
                    }
                }
                catch
                {
                    players.Add(new XAttribute("PopulateFailed", true));
                }
                gameStatus.Add(players);

                gameStatus.Add(new XElement("Round", Program.CurrentGame.Round));
                gameStatus.Add(new XElement("Stage", Program.CurrentGame.Stage));
                XElement currentBlack = new XElement("CurrentBlackCard");
                currentBlack.Add(new XAttribute("ID", Program.CurrentGame.CurrentBlackCard));
                currentBlack.Add(new XAttribute("Needs", Program.CurrentGame.GameSet.BlackCards[Program.CurrentGame.GameSet.BlackCardIndex[Program.CurrentGame.CurrentBlackCard]].Needs));
                currentBlack.Add(Program.CurrentGame.GameSet.BlackCards[Program.CurrentGame.GameSet.BlackCardIndex[Program.CurrentGame.CurrentBlackCard]].ToString);
                gameStatus.Add(currentBlack);

                XElement answers = new XElement("Answers");
                try
                {
                    answers.Add(new XAttribute("Count", Program.CurrentGame.Answers.Count));
                    foreach (Answer answer in Program.CurrentGame.Answers)
                    {
                        XElement xAnswer = new XElement("Answer");
                        xAnswer.Add(new XAttribute("Submitter", answer.Submitter.Name));

                        XElement blackCard = new XElement("BlackCard", answer.BlackCard.ToString);
                        blackCard.Add(new XAttribute("ID", answer.BlackCard.Id));
                        blackCard.Add(new XAttribute("Needs", answer.BlackCard.Needs));
                        xAnswer.Add(blackCard);

                        XElement whiteCard1 = new XElement("WhiteCard1", answer.WhiteCard.ToString);
                        whiteCard1.Add(new XAttribute("ID", answer.WhiteCard.Id));
                        xAnswer.Add(whiteCard1);
                        if (answer.BlackCard.Needs == 2)
                        {
                            XElement whiteCard2 = new XElement("WhiteCard2", answer.WhiteCard2.ToString);
                            whiteCard2.Add(new XAttribute("ID", answer.WhiteCard2.Id));
                            xAnswer.Add(whiteCard2);
                        }
                        xAnswer.Add(new XElement("ToString", answer.ToString));
                        answers.Add(xAnswer);
                    }
                }
                catch
                {
                    answers.Add(new XAttribute("PopulateFailed", true));
                }
                gameStatus.Add(answers);

                XElement votes = new XElement("Votes");
                try
                {
                    votes.Add(new XAttribute("Count", Program.CurrentGame.Votes.Count));
                    foreach (Vote vote in Program.CurrentGame.Votes)
                    {
                        XElement xVote = new XElement("Vote");
                        xVote.Add(new XAttribute("Voter", vote.Voter.Name));
                        XElement xAnswer = new XElement("VotedFor");
                        xAnswer.Add(new XAttribute("Submitter", vote.Choice.Submitter.Name));

                        XElement blackCard = new XElement("BlackCard", vote.Choice.BlackCard.ToString);
                        blackCard.Add(new XAttribute("ID", vote.Choice.BlackCard.Id));
                        blackCard.Add(new XAttribute("Needs", vote.Choice.BlackCard.Needs));
                        xAnswer.Add(blackCard);

                        XElement whiteCard1 = new XElement("WhiteCard1", vote.Choice.WhiteCard.ToString);
                        whiteCard1.Add(new XAttribute("ID", vote.Choice.WhiteCard.Id));
                        xAnswer.Add(whiteCard1);
                        if (vote.Choice.BlackCard.Needs == 2)
                        {
                            XElement whiteCard2 = new XElement("WhiteCard2", vote.Choice.WhiteCard2.ToString);
                            whiteCard2.Add(new XAttribute("ID", vote.Choice.WhiteCard2.Id));
                            xAnswer.Add(whiteCard2);
                        }
                        xAnswer.Add(new XElement("ToString", vote.Choice.ToString));
                        xVote.Add(xAnswer);
                        votes.Add(xVote);
                    }
                }
                catch
                {
                    votes.Add(new XAttribute("PopulateFailed", true));
                }
                gameStatus.Add(votes);

                XElement winners = new XElement("Winners");
                try
                {
                    winners.Add(new XAttribute("Count", Program.CurrentGame.Winners.Count));
                    foreach (Answer answer in Program.CurrentGame.Winners)
                    {
                        XElement xAnswer = new XElement("Winner");
                        xAnswer.Add(new XAttribute("Submitter", answer.Submitter.Name));

                        XElement blackCard = new XElement("BlackCard", answer.BlackCard.ToString);
                        blackCard.Add(new XAttribute("ID", answer.BlackCard.Id));
                        blackCard.Add(new XAttribute("Needs", answer.BlackCard.Needs));
                        xAnswer.Add(blackCard);

                        XElement whiteCard1 = new XElement("WhiteCard1", answer.WhiteCard.ToString);
                        whiteCard1.Add(new XAttribute("ID", answer.WhiteCard.Id));
                        xAnswer.Add(whiteCard1);
                        if (answer.BlackCard.Needs == 2)
                        {
                            XElement whiteCard2 = new XElement("WhiteCard2", answer.WhiteCard2.ToString);
                            whiteCard2.Add(new XAttribute("ID", answer.WhiteCard2.Id));
                            xAnswer.Add(whiteCard2);
                        }
                        xAnswer.Add(new XElement("ToString", answer.ToString));
                        winners.Add(xAnswer);
                    }
                }
                catch
                {
                    winners.Add(new XAttribute("PopulateFailed", true));
                }
                gameStatus.Add(winners);
            }
            rootNode.Add(gameStatus);

            XDocument crashReport = new XDocument(rootNode);
            crashReport.Save(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + DateTime.Now.ToString("yyyyMMdd-HHmmss_") + "CardsCrashReport.xml");
        }

        private static KeyValuePair<string, string> GetOSVersionAndCaption()
        {
            KeyValuePair<string, string> kvpOSSpecs = new KeyValuePair<string, string>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem");
            try
            {
                foreach (var os in searcher.Get())
                {
                    var version = os["Version"].ToString();
                    var productName = os["Caption"].ToString();
                    kvpOSSpecs = new KeyValuePair<string, string>(productName, version);
                }
            }
            catch { }

            return kvpOSSpecs;
        }
    }
}
