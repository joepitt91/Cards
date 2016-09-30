﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
            FormClosing += FrmVote_FormClosing;
        }

        /// <summary>
        /// Prevents accidental closing and ends the game if closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmVote_FormClosing(object sender, FormClosingEventArgs e)
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

        protected override bool ProcessCmdKey(ref Message message, Keys keys)
        {
            switch (keys)
            {
                case Keys.Control | Keys.Shift | Keys.F1:
                    UI.Debug debugUI = new Debug();
                    debugUI.ShowDialog();
                    return false;
            }
            return false;
        }

        private void frmVote_Load(object sender, EventArgs e)
        {
            Program.CurrentPlayer.NextCommand = "GAMEUPDATE";
            Program.CurrentPlayer.NewCommand = true;
            while (!Program.CurrentPlayer.NewResponse)
            {
                Application.DoEvents();
            }
            string[] response = Program.CurrentPlayer.LastResponse.Split(' ');
            Program.CurrentPlayer.NewResponse = false;
            if (response[0] == "VOTING")
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(response[1])))
                {
                    Program.CurrentGame.Answers = (List<Answer>)formatter.Deserialize(stream);
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
            if (cmbAnswers.SelectedIndex != -1)
            {
                Vote myVote = new Vote(Program.CurrentPlayer.Owner, Program.CurrentGame.Answers[cmbAnswers.SelectedIndex]);
                byte[] myVoteBytes = myVote.ToByteArray();
                string myVoteBase64 = Convert.ToBase64String(myVoteBytes);
                Program.CurrentPlayer.NextCommand = "VOTE " + myVoteBase64;
                Program.CurrentPlayer.NewCommand = true;
                while (!Program.CurrentPlayer.NewResponse)
                {
                    Application.DoEvents();
                }
                string response = Program.CurrentPlayer.LastResponse;
                Program.CurrentPlayer.NewResponse = false;
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
            }
        }
    }
}