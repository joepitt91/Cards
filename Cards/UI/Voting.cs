﻿using JoePitt.Cards.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

namespace JoePitt.Cards.UI
{
    /// <summary>
    /// Voting UI.
    /// </summary>
    public partial class Voting : Form
    {
        private bool Submitted = false;

        /// <summary>
        /// Initalise the Voting UI.
        /// </summary>
        public Voting()
        {
            InitializeComponent();
            FormClosing += Vote_FormClosing;
        }

        /// <summary>
        /// Prevents accidental closing and ends the game if closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vote_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !Submitted)
            {
                if (MessageBox.Show("End game now?", "End Game", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    Program.CurrentGame.Stage = 'E';
                    Program.CurrentGame.Playable = false;
                    DialogResult = DialogResult.Abort;
                }
            }
        }

        /// <summary>
        /// Handles Key Presses and Launch Debug UI if CTRL+SHIFT+F1 is pressed.
        /// </summary>
        /// <param name="msg">System-Generated.</param>
        /// <param name="keyData">The keys that are pressed.</param>
        /// <returns>If the keypress was handled.</returns>
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

        private void Vote_Load(object sender, EventArgs e)
        {
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
            if (response[0] == "VOTING")
            {
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(response[1])))
                {
                    stream.Position = 0;
                    Program.CurrentGame.Answers = (List<Answer>)Program.Formatter.Deserialize(stream);
                }
            }

            Text = Program.CurrentPlayer.Owner.Name + Text;
            foreach (Answer answer in Program.CurrentGame.Answers)
            {
                cmbAnswers.Items.Add(answer.ToString);
            }
            cmbAnswers.SelectedIndex = 0;
        }

        /// <summary>
        /// Keeps the choice card up to date with the selected answer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbAnswers_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtAnswer.Text = Program.CurrentGame.Answers[cmbAnswers.SelectedIndex].ToString;
        }

        private void btnVote_Click(object sender, EventArgs e)
        {
            //cast vote
            Enabled = false;
            if (cmbAnswers.SelectedIndex != -1)
            {
                Vote myVote = new Vote(Program.CurrentPlayer.Owner, Program.CurrentGame.Answers[cmbAnswers.SelectedIndex]);
                byte[] myVoteBytes = myVote.ToByteArray();
                string myVoteBase64 = Convert.ToBase64String(myVoteBytes);
                string response = "";
                try
                {
                    response = SharedNetworking.Communicate(Program.CurrentPlayer, "VOTE " + myVoteBase64);
                }
                catch (SocketException)
                {
                    MessageBox.Show("Your Connection to the Game has been lost. Restarting.", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Restart();
                }
                if (response == "SUBMITTED")
                {
                    Submitted = true;
                    Close();
                }
                else if (response == "ERROR")
                {
                    Submitted = false;
                    Close();
                }
                else
                {
                    MessageBox.Show("Unexpected Error! Unable to submit vote. Application will exit!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Program.Exit();
                    Close();
                }
            }
            else
            {
                MessageBox.Show("Select an answer.", "No Answer Chosen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Enabled = true;
            }
        }
    }
}