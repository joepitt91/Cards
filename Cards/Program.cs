using JoePitt.Cards.Net;
using JoePitt.Cards.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;

namespace JoePitt.Cards
{
    /// <summary>
    /// The central class of Cards.
    /// </summary>
    static class Program
    {
        static public string CardSetPath { get; private set; }
        /// <summary>
        /// The game that is currently being played.
        /// </summary>
        static public Game CurrentGame { get; set; }
        /// <summary>
        /// The Client Communications Handler of the current player.
        /// </summary>
        static public ClientNetworking CurrentPlayer { get; set; }
        /// <summary>
        /// The Leaderboard window.
        /// </summary>
        static public Leaderboard LeaderBoard { get; set; }
        /// <summary>
        /// The Session Key of this instance of Cards.
        /// </summary>
        static public string SessionKey { get; set; }

        static public BinaryFormatter Formatter { get; set; }

        static private int ShownWinners { get; set; }
        static private Waiting waiting;


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            CardSetPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JoePitt\\Cards\\";

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Running baseUI = new Running();
            baseUI.Show();
            waiting = new Waiting();

            try
            {
                if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null)
                {
                    string arg = Uri.UnescapeDataString(AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0].Substring(8));
                    if (File.Exists(arg))
                    {
                        if (arg.EndsWith(".cardset"))
                        {
                            Dealer.InstallCardSet(arg);
                        }
                    }
                }
            }
            catch { }
            Formatter = new BinaryFormatter();
            NewGame:
            if (Setup())
            {
                while (CurrentGame.Playable)
                {
                    LeaderBoard.UpdateScores();
                    CurrentPlayer = CurrentGame.LocalPlayers[0];
                    string fullResponse = "";
                    try
                    {
                        fullResponse = SharedNetworking.Communicate(Program.CurrentPlayer, "GAMEUPDATE");
                    }
                    catch (SocketException)
                    {
                        MessageBox.Show("Your Connection to the Game has been lost. Restarting.", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Restart();
                    }
                    string[] response = fullResponse.Split(' ');

                    switch (response[0])
                    {
                        case "WAITING":
                            CurrentGame.Stage = 'W';
                            break;
                        case "PLAYING":
                            CurrentGame.Stage = 'P';
                            CurrentGame.Round = Convert.ToInt32(response[2]);
                            Play();
                            break;
                        case "VOTING":
                            CurrentGame.Stage = 'V';
                            Vote();
                            break;
                        case "END":
                            CurrentGame.Stage = 'E';
                            if (Replay())
                            {
                                goto NewGame;
                            }
                            else
                            {
                                Exit();
                            }
                            break;
                        default:
                            MessageBox.Show("Unexpected Error! Unknown Game State, Application will exit!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            Exit();
                            break;
                    }
                }
                Application.Restart();
            }
        }

        static public void Init()
        {
            CardSetPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JoePitt\\Cards\\";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DebugUI")]
        static private bool Setup()
        {
            ShownWinners = 0;
            RNGCryptoServiceProvider KeyGen = new RNGCryptoServiceProvider();
            byte[] KeyBytes = new byte[32];
            KeyGen.GetBytes(KeyBytes);
            SessionKey = Convert.ToBase64String(KeyBytes);

            NewGame newGame = new NewGame();
            newGame.ShowDialog();
            newGame.Dispose();
            if (CurrentGame != null)
            {
                LeaderBoard = new Leaderboard();
                return true;
            }
            else return false;
        }

        static private void Play()
        {
            if (CurrentGame.Round > 1 && ShownWinners < (CurrentGame.Round - 1))
            {
                Scores();
            }

            int submitted = 0;
            foreach (ClientNetworking player in CurrentGame.LocalPlayers)
            {
                CurrentPlayer = player;
                string fullResponse = "";
                try
                {
                    fullResponse = SharedNetworking.Communicate(Program.CurrentPlayer, "NEED ANSWER");
                }
                catch (SocketException)
                {
                    MessageBox.Show("Your Connection to the Game has been lost. Restarting.", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Restart();
                }

                string[] responseP = fullResponse.Split(' ');
                switch (responseP[0])
                {
                    case "YES":
                        if (CurrentGame.LocalPlayers.Count > 1)
                        {
                            MessageBox.Show("Pass to " + CurrentPlayer.Owner.Name, "Next Player", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        Gameplay game = new Gameplay();
                        if (game.ShowDialog() == DialogResult.Abort)
                        {
                            break;
                        }
                        submitted++;
                        break;
                    case "NO":
                        break;
                    default:
                        MessageBox.Show("Unexpected Error! Unknown NEED ANSWER Response, Application will exit!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        Exit();
                        break;
                }
            }
            if (CurrentGame.Stage == 'P' && submitted < 1)
            {
                waiting.Update("Waiting for other players to submit their answers...");
                waiting.ShowDialog();
            }
        }

        static private void Vote()
        {
            int voted = 0;
            foreach (ClientNetworking player in CurrentGame.LocalPlayers)
            {
                CurrentPlayer = player;
                string fullResponse = "";
                try
                {
                    fullResponse = SharedNetworking.Communicate(Program.CurrentPlayer, "NEED VOTE");
                }
                catch (SocketException)
                {
                    MessageBox.Show("Your Connection to the Game has been lost. Restarting.", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Restart();
                }
                string[] responseP = fullResponse.Split(' ');
                switch (responseP[0])
                {
                    case "YES":
                        if (CurrentGame.LocalPlayers.Count > 1)
                        {
                            MessageBox.Show("Pass to " + CurrentPlayer.Owner.Name, "Next Player", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        Voting vote = new Voting();
                        if (vote.ShowDialog() == DialogResult.Abort)
                        {
                            break;
                        }
                        voted++;
                        break;
                    case "NO":
                        break;
                    default:
                        MessageBox.Show("Unexpected Error! Unknown NEED VOTE Response, Application will exit!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        Exit();
                        break;
                }
            }
            if (CurrentGame.Stage == 'V' && voted < 1)
            {
                waiting.Update("Waiting for other players to submit their votes...");
                waiting.ShowDialog();
            }
        }

        static private void Scores()
        {
            CurrentPlayer = CurrentGame.LocalPlayers[0];
            string fullResponse = "";
            try
            {
                fullResponse = SharedNetworking.Communicate(Program.CurrentPlayer, "GETWINNER");
            }
            catch (SocketException)
            {
                MessageBox.Show("Your Connection to the Game has been lost. Restarting.", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Restart();
            }
            string[] response = fullResponse.Split(' ');
            try
            {
                byte[] winnersBytes = Convert.FromBase64String(response[0]);
                using (MemoryStream stream = new MemoryStream(winnersBytes))
                {
                    stream.Position = 0;
                    CurrentGame.Winners = (List<Answer>)Formatter.Deserialize(stream);
                }
                string message = "";
                if (CurrentGame.Winners.Count > 1)
                {
                    message = "The winning answers are:" + Environment.NewLine;
                    foreach (Answer winner in CurrentGame.Winners)
                    {
                        message = message + Environment.NewLine + winner.ToString + " (by " + winner.Submitter.Name + ")";
                    }
                }
                else
                {
                    message = "The winning answer is: " + CurrentGame.Winners[0].ToString + " (by " + CurrentGame.Winners[0].Submitter.Name + ")";
                }
                MessageBox.Show(message, "Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShownWinners = CurrentGame.Round - 1;
            }
            catch (FormatException)
            {
                MessageBox.Show("Unexpected Error! Bad response to GETWINNER, Application will exit!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Exit();
            }
        }

        static private bool Replay()
        {
            CurrentPlayer = CurrentGame.LocalPlayers[0];
            string fullResponse = "";
            try
            {
                fullResponse = SharedNetworking.Communicate(Program.CurrentPlayer, "GETWINNER");
            }
            catch (SocketException)
            {
                MessageBox.Show("Your Connection to the Game has been lost. Restarting.", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Restart();
            }
            string[] response = fullResponse.Split(' ');
            try
            {

                byte[] winnersBytes = Convert.FromBase64String(response[0]);
                using (MemoryStream stream = new MemoryStream(winnersBytes))
                {
                    stream.Position = 0;
                    CurrentGame.Winners = (List<Answer>)Formatter.Deserialize(stream);
                }
                string message = "";
                if (CurrentGame.Winners.Count > 1)
                {
                    message = "The winning answers are:" + Environment.NewLine;
                    foreach (Answer winner in CurrentGame.Winners)
                    {
                        message = message + Environment.NewLine + winner.ToString + " (by " + winner.Submitter.Name + ")";
                    }
                }
                else
                {
                    message = "The winning answer is: " + CurrentGame.Winners[0].ToString + " (by " + CurrentGame.Winners[0].Submitter.Name + ")";
                }
                MessageBox.Show(message, "Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShownWinners = CurrentGame.Round;
            }
            catch (FormatException)
            {
                MessageBox.Show("Unexpected Error! Bad response to GETWINNER, Application will exit!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Exit();
            }

            CurrentGame.Playable = false;
            string FinalScore = "The Final Scores are:" + Environment.NewLine;
            int position = 1;
            foreach (Player player in CurrentGame.Players.OrderByDescending(player => player.Score).ThenBy(player => player.Name))
            {
                FinalScore = FinalScore + Environment.NewLine + position + ") " + player.Name + " with " + player.Score + " points";
                position++;
            }
            FinalScore = FinalScore + Environment.NewLine + Environment.NewLine + "Play Again?";
            CurrentGame.Stop();
            if (MessageBox.Show(FinalScore, "Final Results - Cards", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static public void Exit()
        {
            if (CurrentGame != null) { CurrentGame.Stop(); }
            Application.Exit();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            string Error = "Cards will exit due to a fatal error from which it cannot recover. " +
                "If this happens again, please report it to the developer with: " +
                "Where you were in the game, and the below technical details" +
                Environment.NewLine + Environment.NewLine +
                "Error Message: " + ex.Message + Environment.NewLine + Environment.NewLine +
                "Additional Data: " + Environment.NewLine + ex.Data + Environment.NewLine + Environment.NewLine +
                "Stack Trace: " + Environment.NewLine + ex.StackTrace;
            MessageBox.Show(Error, "FATAL ERROR", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            Application.Exit();
        }
    }
}