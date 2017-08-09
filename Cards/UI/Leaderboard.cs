using System;
using System.Data;
using System.Linq;
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
            FormClosing += FrmLeaderboard_FormClosing;
        }

        /// <summary>
        /// Hodes the UI rather than closing it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmLeaderboard_FormClosing(object sender, FormClosingEventArgs e)
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
            int i = 1;
            lstLeaderboard.Items.Clear();
            foreach (Player p in Program.CurrentGame.Players.ToList().OrderByDescending(p => p.Score))
            {
                lstLeaderboard.Items.Add(i.ToString());
                lstLeaderboard.Items[i - 1].SubItems.Add(p.Name);
                lstLeaderboard.Items[i - 1].SubItems.Add(p.Score.ToString());
                i++;
            }
        }
    }
}
