using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace JoePitt.Cards.Net
{
    /// <summary>
    /// Provies Client Networking to join and play a game over a network.
    /// </summary>
    internal class ClientNetworking
    {
        private string Address;
        private int Port;
        /// <summary>
        /// The player who's Connection this is.
        /// </summary>
        public Player Owner { get; private set; }
        /// <summary>
        /// If there is a command waiting to be sent.
        /// </summary>
        public bool NewCommand { get; set; } = false;
        /// <summary>
        /// The next command to be sent.
        /// </summary>
        public string NextCommand { get; set; } = "";
        /// <summary>
        /// If there is a new response waiting to be processed.
        /// </summary>
        public bool NewResponse { get; set; } = false;
        /// <summary>
        /// The last response from the server.
        /// </summary>
        public string LastResponse { get; private set; } = "";


        /// <summary>
        /// Sets up the Player's Networking.
        /// </summary>
        /// <param name="ownerIn">The Player the networking is for.</param>
        /// <param name="serverAddress">The IP Address of the hosted game.</param>
        /// <param name="serverPort">The port of the hosted game.</param>
        public ClientNetworking(Player ownerIn, string serverAddress, int serverPort)
        {
            Owner = ownerIn;
            Address = serverAddress;
            Port = serverPort;
            ThreadPool.QueueUserWorkItem(RunSession);
        }

        private void RunSession(object state)
        {
            TcpClient tcpClient;
            try
            {
                tcpClient = new TcpClient(Address, Port);
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Unable to connect to game (" + ex.Message + ")", "Unable to connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Restart();
                return;
            }

            while (Program.CurrentGame.Playable)
            {
                if (NewCommand)
                {
                retry:
                    if (SharedNetworking.Send(tcpClient, NextCommand))
                    {
                        NewCommand = false;
                        LastResponse = SharedNetworking.ReceiveString(tcpClient);
                        if (LastResponse.Contains(Environment.NewLine))
                        {
                            LastResponse = LastResponse.Replace(Environment.NewLine, "");
                        }
                        if (!string.IsNullOrEmpty(LastResponse))
                        {
                            NewResponse = true;
                        }
                        else
                        {
                            if (MessageBox.Show("A network error has occurred while getting the response to the last command", "Network Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                            {
                                NewResponse = false;
                                NewCommand = true;
                                goto retry;
                            }
                            else
                            {
                                Application.Restart();
                            }
                        }
                    }
                    else
                    {
                        if (MessageBox.Show("A network error has occurred while sending the last command", "Network Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                        {
                            NewResponse = false;
                            NewCommand = true;
                            goto retry;
                        }
                        else
                        {
                            Application.Restart();
                        }
                    }
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
            tcpClient.Close();
        }
    }
}
