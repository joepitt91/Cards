using JoePitt.Cards.Net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JoePitt.Cards.UI
{
    /// <summary>
    /// New Game UI.
    /// </summary>
    public partial class NewGame : Form
    {
        /// <summary>
        /// Initalises the New Game UI.
        /// </summary>
        public NewGame()
        {
            InitializeComponent();
            picLogo.ContextMenuStrip = mnuLogo;
            tabsType.SelectedIndexChanged += TabsType_SelectedIndexChanged;
            txtAddPlayerLocal.GotFocus += TxtAddPlayerLocal_GotFocus;
            txtAddPlayerLocal.LostFocus += TxtAddPlayerLocal_LostFocus;
            string displayName = "";
            try { displayName = Dealer.GetPlayerName(); } catch (ArgumentNullException) { }
            if (!string.IsNullOrEmpty(displayName))
            {
                lstPlayersLocal.Items.Add(displayName);
                txtPlayerNameHost.Text = displayName;
                txtPlayerNameJoin.Text = displayName;
            }
        }

        private void TxtAddPlayerLocal_GotFocus(object sender, EventArgs e)
        {
            AcceptButton = btnAddPlayerLocal;
        }

        private void TxtAddPlayerLocal_LostFocus(object sender, EventArgs e)
        {
            AcceptButton = btnStartLocal;
        }

        private void TabsType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabsType.SelectedIndex)
            {
                case 0:
                    AcceptButton = btnStartLocal;
                    break;
                case 1:
                    AcceptButton = btnStartHost;
                    break;
                case 2:
                    AcceptButton = btnStartJoin;
                    break;
            }
        }

        private void NewGame_Load(object sender, EventArgs e)
        {
            // Add Card Sets.
            chkCardSetsHost.Items.Clear();
            chkCardSetsLocal.Items.Clear();
            StringCollection savedCardSets = Properties.Settings.Default.CardSets;
            List<string[]> CardSets = Dealer.GetCardSets();
            foreach (string[] CardSet in CardSets)
            {
                if (CardSet[4] == "OK")
                {
                    bool added = false;
                    foreach (string savedSetStr in savedCardSets)
                    {
                        string guid = savedSetStr.Substring(savedSetStr.LastIndexOf("_") + 1);

                        if (guid == CardSet[0])
                        {
                            chkCardSetsLocal.Items.Add(CardSet[1] + " (" + CardSet[2] + ")" + Environment.NewLine + CardSet[0], true);
                            chkCardSetsHost.Items.Add(CardSet[1] + " (" + CardSet[2] + ")" + Environment.NewLine + CardSet[0], true);
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        chkCardSetsLocal.Items.Add(CardSet[1] + " (" + CardSet[2] + ")" + Environment.NewLine + CardSet[0], false);
                        chkCardSetsHost.Items.Add(CardSet[1] + " (" + CardSet[2] + ")" + Environment.NewLine + CardSet[0], false);
                    }
                }
            }

            numRoundsLocal.Value = Properties.Settings.Default.Rounds;
            numRoundsHost.Value = Properties.Settings.Default.Rounds;

            numCardsLocal.Value = Properties.Settings.Default.Cards;
            numCardsHost.Value = Properties.Settings.Default.Cards;

            chkNeverHaveILocal.Checked = Properties.Settings.Default.NeverHaveI;
            chkNeverHaveIHost.Checked = Properties.Settings.Default.NeverHaveI;

            chkRebootingTheUniverseLocal.Checked = Properties.Settings.Default.Rebooting;
            chkRebootingTheUniverseHost.Checked = Properties.Settings.Default.Rebooting;
        }

        /// <summary>
        /// Start a local game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartLocal_Click(object sender, EventArgs e)
        {
            if (lstPlayersLocal.Items.Count > 0)
            {
                // Prevent Changes
                Enabled = false;
                // Save Settings
                StringCollection cardPacks = new StringCollection();
                foreach (string set in chkCardSetsLocal.CheckedItems)
                {
                    cardPacks.Add(set.Replace(Environment.NewLine, "_"));
                }
                Properties.Settings.Default.CardSets = cardPacks;
                Properties.Settings.Default.Rounds = (int)numRoundsLocal.Value;
                Properties.Settings.Default.Cards = (int)numCardsLocal.Value;
                Properties.Settings.Default.NeverHaveI = chkNeverHaveILocal.Checked;
                Properties.Settings.Default.Rebooting = chkRebootingTheUniverseLocal.Checked;
                Properties.Settings.Default.Save();

                List<string> players = new List<string>();
                foreach (string player in lstPlayersLocal.Items)
                {
                    players.Add(player);
                }
                Program.CurrentGame = new Game('L', players);
                if (Program.CurrentGame.Playable)
                {
                    RNGCryptoServiceProvider PassGen = new RNGCryptoServiceProvider();
                    byte[] PassBytes = new byte[32];
                    PassGen.GetBytes(PassBytes);
                    Program.SessionKey = Convert.ToBase64String(PassBytes);
                    foreach (Player player in Program.CurrentGame.Players)
                    {
                        if (!player.Name.ToLower().StartsWith("[bot]"))
                        {
                            ClientNetworking playerNetwork = new ClientNetworking(player, "127.0.0.1", Program.CurrentGame.HostNetwork.Port)
                            {
                                NextCommand = "JOIN " + player.Name.Replace(' ', '_') + " " + Program.SessionKey,
                                NewCommand = true
                            };
                            while (!playerNetwork.NewResponse)
                            {
                                Application.DoEvents();
                                if (playerNetwork.Dropped)
                                {
                                    MessageBox.Show("Your Connection to the Game has been lost. Restarting.", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    Application.Restart();
                                    break;
                                }
                            }
                            if (playerNetwork.LastResponse == "SUCCESS")
                            {
                                playerNetwork.NewResponse = false;
                                Program.CurrentGame.LocalPlayers.Add(playerNetwork);
                            }
                        }
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Add some players.", "No Players", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddPlayerLocal_Click(object sender, EventArgs e)
        {
            if (txtAddPlayerLocal.Text.Length > 0)
            {
                if (lstPlayersLocal.Items.Contains(txtAddPlayerLocal.Text))
                {
                    MessageBox.Show("Player Name already added.", "Duplicated Player", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    lstPlayersLocal.Items.Add(txtAddPlayerLocal.Text);
                    txtAddPlayerLocal.Text = "";
                    txtAddPlayerLocal.Focus();
                }
            }
        }

        private void RemovePlayerLocal_Click(object sender, EventArgs e)
        {
            if (lstPlayersLocal.SelectedIndex != -1)
            {
                lstPlayersLocal.Items.RemoveAt(lstPlayersLocal.SelectedIndex);
            }
        }

        // Host Game

        /// <summary>
        /// Start hosting a game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "NoName")]
        private void StartHost_Click(object sender, EventArgs e)
        {
            // Prevent Changes
            Enabled = false;

            if (string.IsNullOrEmpty(txtPlayerNameHost.Text))
            {
                MessageBox.Show("NoName");
                Enabled = true;
                return;
            }

            if (txtPlayerNameHost.Text.ToLower().StartsWith("[bot]"))
            {
                MessageBox.Show("Sorry, you cannot be a bot too.", "Invalid Username", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Enabled = true;
                return;
            }

            // Save Settings
            StringCollection cardPacks = new StringCollection();
            foreach (string set in chkCardSetsHost.CheckedItems)
            {
                cardPacks.Add(set.Replace(Environment.NewLine, "_"));
            }
            Properties.Settings.Default.CardSets = cardPacks;
            Properties.Settings.Default.Rounds = (int)numRoundsHost.Value;
            Properties.Settings.Default.Cards = (int)numCardsHost.Value;
            Properties.Settings.Default.NeverHaveI = chkNeverHaveIHost.Checked;
            Properties.Settings.Default.Rebooting = chkRebootingTheUniverseHost.Checked;
            Properties.Settings.Default.Save();

            List<string> players = new List<string>
            {
                txtPlayerNameHost.Text
            };
            int i = 0;
            while (i < (int)numPlayersHost.Value)
            {
                players.Add("FREESLOT");
                i++;
            }
            i = 0;
            while (i < (int)numBotsHost.Value)
            {
                players.Add("[Bot] " + (i + 1));
                i++;
            }
            Program.CurrentGame = new Game('H', players);
            if (Program.CurrentGame.Playable)
            {
                ClientNetworking playerNetwork = new ClientNetworking(Program.CurrentGame.Players[0], "127.0.0.1", Program.CurrentGame.HostNetwork.Port)
                {
                    NextCommand = "JOIN " + txtPlayerNameHost.Text.Replace(' ', '_') + " " + Program.SessionKey,
                    NewCommand = true
                };

                while (!playerNetwork.NewResponse)
                {
                    Application.DoEvents();
                    if (playerNetwork.Dropped)
                    {
                        MessageBox.Show("Your Connection to the Game has been lost. Restarting.", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Restart();
                        break;
                    }
                }

                if (playerNetwork.LastResponse == "SUCCESS")
                {
                    playerNetwork.NewResponse = false;
                    Program.CurrentGame.LocalPlayers.Add(playerNetwork);

                    ConnectionDetails connectionDetails = new ConnectionDetails();
                    connectionDetails.ShowDialog();
                    Close();
                }
                else
                {
                    Enabled = true;
                }
            }
            else
            {
                Enabled = true;
            }
        }

        // Join Game

        /// <summary>
        /// Join a hosted game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartJoin_Click(object sender, EventArgs e)
        {
            Enabled = false;
            string IPv4 = "^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            string IPv6 = "^(?:[a-fA-F0-9]{1,4}:|:){7}[a-fA-F0-9]{1,4}$";

            bool isIPv4 = Regex.IsMatch(txtIPJoin.Text, IPv4);
            bool isIPv6 = Regex.IsMatch(txtIPJoin.Text, IPv6);

            if (isIPv4 || isIPv6)
            {
                List<string> players = new List<string>
                {
                    txtPlayerNameJoin.Text
                };
                Program.CurrentGame = new Game('J', players);
                if (Program.CurrentGame.Join(txtIPJoin.Text, (int)numPortJoin.Value))
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("IP Address is not valid, must be an IPv4 or IPv6 address.", "Bad Host", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Enabled = true;
            }
        }

        // Context Menu Items

        private void RulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Rules rules = new Rules();
            rules.ShowDialog();
            rules.Dispose();
        }

        private void LicenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            License license = new License();
            license.ShowDialog();
            license.Dispose();
        }
    }
}