﻿using JoePitt.Cards.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JoePitt.Cards.UI
{
    /// <summary>
    /// Main Gameplay UI Control.
    /// </summary>
    public partial class Gameplay : Form
    {
        private Card BlackCard;
        private bool Submitted = false;

        /// <summary>
        /// Initalises the main gameplay UI.
        /// </summary>
        public Gameplay()
        {
            InitializeComponent();
            FormClosing += Gameplay_FormClosing;
        }

        /// <summary>
        /// Prevents accidental closing, ends game if closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Gameplay_FormClosing(object sender, FormClosingEventArgs e)
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

        private void Gameplay_Load(object sender, EventArgs e)
        {
            Text = Program.CurrentPlayer.Owner.Name + " - Cards";
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
            if (response[0] == "PLAYING")
            {
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(response[1])))
                {
                    stream.Position = 0;
                    BlackCard = (Card)Program.Formatter.Deserialize(stream);
                }
                txtBlackCard.Text = BlackCard.ToString + Environment.NewLine;
                string blankRegEx = "_{3,}";
                Regex regexr = new Regex(blankRegEx);
                if (regexr.IsMatch(txtBlackCard.Text))
                {
                    string Text = regexr.Replace(txtBlackCard.Text, "[Blank]");
                    txtBlackCard.AccessibleName = "Black Card is " + Text;
                }
                else
                {
                    txtBlackCard.AccessibleName = "Black Card is " + txtBlackCard.Text;
                }
                txtBlackCard.Select(txtBlackCard.Text.Length, 0);

                string cards = "";
                try
                {
                    cards = SharedNetworking.Communicate(Program.CurrentPlayer, "MYCARDS");
                }
                catch (SocketException)
                {
                    MessageBox.Show("Your Connection to the Game has been lost. Restarting.", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Restart();
                }
                try
                {
                    using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(cards)))
                    {
                        stream.Position = 0;
                        Program.CurrentPlayer.Owner.WhiteCards = (List<Card>)Program.Formatter.Deserialize(stream);
                    }
                }
                catch (SerializationException ex)
                {
                    MessageBox.Show("Unexpected Error! Unable to get White Cards (" + ex.Message + "). Application will exit!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Program.Exit();
                }

                foreach (Card playerCard in Program.CurrentPlayer.Owner.WhiteCards)
                {
                    cmbWhiteCard.Items.Add(playerCard.ToString);
                    cmbWhiteCard2.Items.Add(playerCard.ToString);
                }
                if (BlackCard.Needs == 1)
                {
                    txtWhiteCard2.Visible = false;
                    cmbWhiteCard2.Visible = false;
                    picWhiteCard2.Visible = false;
                }

                cmbWhiteCard.SelectedIndex = 0;
                cmbWhiteCard2.SelectedIndex = 1;

                txtRound.Text = Program.CurrentGame.Round + " of " + Program.CurrentGame.Rounds;
            }
            else
            {
                Submitted = true;
                Close();
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

        /// <summary>
        /// Keeps the First White card up to date with the selected card.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WhiteCard_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtWhiteCard.Text = cmbWhiteCard.SelectedItem.ToString();
            if (cmbWhiteCard.SelectedIndex == cmbWhiteCard2.SelectedIndex)
            {
                if (cmbWhiteCard2.SelectedIndex == cmbWhiteCard2.Items.Count - 1)
                {
                    cmbWhiteCard2.SelectedIndex = 0;
                }
                else
                {
                    cmbWhiteCard2.SelectedIndex++;
                }
            }
        }

        /// <summary>
        /// Keeps the Second White Card up to date with the selected card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WhiteCard2_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtWhiteCard2.Text = cmbWhiteCard2.SelectedItem.ToString();
            if (cmbWhiteCard.SelectedIndex == cmbWhiteCard2.SelectedIndex)
            {
                if (cmbWhiteCard.SelectedIndex == cmbWhiteCard.Items.Count - 1)
                {
                    cmbWhiteCard.SelectedIndex = 0;
                }
                else
                {
                    cmbWhiteCard.SelectedIndex++;
                }
            }
        }

        private void ToggleLeaderboard_Click(object sender, EventArgs e)
        {
            if (Program.LeaderBoard.Visible)
            {
                Program.LeaderBoard.Hide();
                Program.LeaderBoard.UpdateScores();
            }
            else
            {
                Program.LeaderBoard.Show();
                Program.LeaderBoard.UpdateScores();
            }
        }

        private void Submit_Click(object sender, EventArgs e)
        {
            Enabled = false;
            Answer myAnswer;
            if (BlackCard.Needs == 1)
            {
                myAnswer = new Answer(Program.CurrentPlayer.Owner, BlackCard, Program.CurrentPlayer.Owner.WhiteCards[cmbWhiteCard.SelectedIndex]);
            }
            else
            {
                myAnswer = new Answer(Program.CurrentPlayer.Owner, BlackCard, Program.CurrentPlayer.Owner.WhiteCards[cmbWhiteCard.SelectedIndex], Program.CurrentPlayer.Owner.WhiteCards[cmbWhiteCard2.SelectedIndex]);
            }
            if (MessageBox.Show("Submit: " + myAnswer.ToString, "Confirm Answer", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {

                string response = "";
                try
                {
                    response = SharedNetworking.Communicate(Program.CurrentPlayer, "SUBMIT " + Convert.ToBase64String(myAnswer.ToByteArray()));
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
                else
                {
                    MessageBox.Show("Unexpected Error! Unable to submit answer. Application will exit!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Program.Exit();
                }
            }
            else
            {
                Enabled = true;
            }
        }

        private void OpenRules_Click(object sender, EventArgs e)
        {
            Rules rules = new Rules();
            rules.ShowDialog();
            rules.Dispose();
        }

        private void OpenLicense_Click(object sender, EventArgs e)
        {
            License license = new License();
            license.ShowDialog();
            license.Dispose();
        }
    }
}
