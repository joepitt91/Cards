using JoePitt.Cards.Net;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Windows.Forms;

namespace JoePitt.Cards.UI
{
    /// <summary>
    /// Leaderboard UI.
    /// </summary>
    public partial class Leaderboard : Form
    {
        /// <summary>
        /// Initalises the Leaderboard UI.
        /// </summary>
        public Leaderboard()
        {
            InitializeComponent();
            FormClosing += Leaderboard_FormClosing;
        }

        /// <summary>
        /// Hodes the UI rather than closing it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Leaderboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.ApplicationExitCall)
            {
                Hide();
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Handles keyboard input for DebugUI calls.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData">The Keys that have been pressed.</param>
        /// <returns>If the key press has been handled.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.Shift | Keys.F1:
                    UI.Debug debugUI = new Debug();
                    debugUI.ShowDialog();
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Update the Leaderboard UI.
        /// </summary>
        public void UpdateScores()
        {
            Dictionary<string, int> scores = new Dictionary<string, int>();
            try
            {
                while (Program.CurrentPlayer == null)
                {
                    Application.DoEvents();
                }
                while (Program.CurrentPlayer.NewCommand)
                {
                    Application.DoEvents();
                }
                string response = SharedNetworking.Communicate(Program.CurrentPlayer, "GAMESCORES");
                string[] scoreValues = response.Split(',');
                foreach (string score in scoreValues)
                {
                    string[] scoreDetails = score.Split(':');
                    scores.Add(scoreDetails[0], Convert.ToInt32(scoreDetails[1]));
                }
                lstLeaderboard.BackColor = System.Drawing.Color.White;
            }
            catch (SocketException)
            {
                lstLeaderboard.BackColor = System.Drawing.Color.Red;
                return;
            }


            int i = 1;
            lstLeaderboard.Items.Clear();
            foreach (KeyValuePair<string, int> p in scores)
            {
                lstLeaderboard.Items.Add(i.ToString());
                lstLeaderboard.Items[i - 1].SubItems.Add(p.Key);
                lstLeaderboard.Items[i - 1].SubItems.Add(p.Value.ToString());
                i++;
            }
        }
    }
}
