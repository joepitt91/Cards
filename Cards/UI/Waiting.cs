using JoePitt.Cards.Net;
using System.Threading;
using System.Windows.Forms;

namespace JoePitt.Cards.UI
{
    /// <summary>
    /// Waiting UI.
    /// </summary>
    public partial class Waiting : Form
    {
        bool waiting = true;
        delegate void SetTextCallback(string text);
        delegate void CloseCallback();

        /// <summary>
        /// Initalise Waiting UI.
        /// </summary>
        public Waiting()
        {
            InitializeComponent();
            FormClosing += Waiting_FormClosing;
        }
        
        private void Waiting_Load(object sender, System.EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(KeepUpdated);
        }
        
        private void Waiting_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && waiting)
            {
                Program.CurrentGame.Stage = 'E';
                Program.CurrentGame.Playable = false;
                DialogResult = DialogResult.Abort;
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
        /// Updates the UI every second.
        /// </summary>
        private void KeepUpdated(object state)
        {
            while (waiting)
            {
                Program.CurrentPlayer = Program.CurrentGame.LocalPlayers[0];
                Program.CurrentPlayer.NextCommand = "GAMEUPDATE";
                Program.CurrentPlayer.NewCommand = true;
                while (!Program.CurrentPlayer.NewResponse)
                {
                    Application.DoEvents();
                }
                string[] response = Program.CurrentPlayer.LastResponse.Split(' ');
                Program.CurrentPlayer.NewResponse = false;
                switch (response[0])
                {
                    case "WAITING":
                        Program.CurrentGame.Stage = 'W';
                        break;
                    case "PLAYING":
                        Program.CurrentGame.Stage = 'P';
                        break;
                    case "VOTING":
                        Program.CurrentGame.Stage = 'V';
                        break;
                    case "END":
                        Program.CurrentGame.Stage = 'E';
                        break;
                    default:
                        MessageBox.Show("Unexpected Error! Unknown Game State, Application will exit!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        Program.Exit();
                        break;
                }

                switch (Program.CurrentGame.Stage)
                {
                    case 'W':
                        Update("Waiting for other players to join the game...");
                        break;
                    case 'P':
                        foreach (ClientNetworking player in Program.CurrentGame.LocalPlayers)
                        {
                            Program.CurrentPlayer = player;
                            Program.CurrentPlayer.NextCommand = "NEED ANSWER";
                            Program.CurrentPlayer.NewCommand = true;
                            while (!Program.CurrentPlayer.NewResponse)
                            {
                                Application.DoEvents();
                            }
                            string[] responseP = Program.CurrentPlayer.LastResponse.Split(' ');
                            Program.CurrentPlayer.NewResponse = false;
                            switch (responseP[0])
                            {
                                case "YES":
                                    waiting = false;
                                    break;
                                case "NO":
                                    break;
                                default:
                                    MessageBox.Show("Unexpected Error! Unknown NEED ANSWER Response, Application will exit!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                    Program.Exit();
                                    break;
                            }
                        }
                        Update("Waiting for other players to submit their answers...");
                        break;
                    case 'V':
                        foreach (ClientNetworking player in Program.CurrentGame.LocalPlayers)
                        {
                            Program.CurrentPlayer = player;
                            Program.CurrentPlayer.NextCommand = "NEED VOTE";
                            Program.CurrentPlayer.NewCommand = true;
                            while (!Program.CurrentPlayer.NewResponse)
                            {
                                Application.DoEvents();
                            }
                            string[] responseP = Program.CurrentPlayer.LastResponse.Split(' ');
                            Program.CurrentPlayer.NewResponse = false;
                            switch (responseP[0])
                            {
                                case "YES":
                                    waiting = false;
                                    break;
                                case "NO":
                                    break;
                                default:
                                    MessageBox.Show("Unexpected Error! Unknown NEED VOTE Response, Application will exit!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                    Program.Exit();
                                    break;
                            }
                        }
                        Update("Waiting for other players to submit their votes...");
                        break;
                    default:
                        waiting = false;
                        return;
                }
                Thread.Sleep(1000);
            }
            CloseCallback d = new CloseCallback(Close);
            this.Invoke(d, new object[] { });
            return;
        }

        /// <summary>
        /// Replaces the text of the UI.
        /// </summary>
        /// <param name="message">The new message to show.</param>
        public void Update(string message)
        {
            if (this.txtMessage.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(Update);
                this.Invoke(d, new object[] { message });
            }
            else
            {
                txtMessage.Text = message;
            }
        }
    }
}
