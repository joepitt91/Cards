using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace JoePitt.Cards.UI
{
    public partial class ConnectionDetails : Form
    {
        private BackgroundWorker updater = new BackgroundWorker();

        /// <summary>
        /// Displays connection details and players for hosted network games.
        /// </summary>
        public ConnectionDetails()
        {
            InitializeComponent();
            FormClosing += ConnectionDetails_FormClosing;
            updater.DoWork += Updater_DoWork;
            updater.WorkerReportsProgress = true;
            updater.WorkerSupportsCancellation = true;
            updater.ProgressChanged += Updater_ProgressChanged;
        }

        private void Updater_DoWork(object sender, DoWorkEventArgs e)
        {
            while (Program.CurrentGame.Stage == 'W' && !updater.CancellationPending)
            {
                updater.ReportProgress(0);
                Thread.Sleep(3000);
            }
            updater.ReportProgress(100);
        }

        private void Updater_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lstPlayers.Items.Clear();
            foreach (Player player in Program.CurrentGame.Players.
                Where(player => player.Name != "FREESLOT").OrderBy(player => player.Name))
            {
                lstPlayers.Items.Add(player.Name);
            }
            if (e.ProgressPercentage == 100)
            {
                Close();
            }
        }

        private void ConnectionDetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && Program.CurrentGame.Stage == 'W')
            {
                if (MessageBox.Show("Are you sure you want to cancel the game?", "Cancel Game?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    Program.CurrentGame.Stop();
                    updater.CancelAsync();
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }
            else
            {
                updater.CancelAsync();
            }
        }

        private void ConnectionDetails_Load(object sender, EventArgs e)
        {
            int i = 0;
            foreach (Net.ConnectionDetails connection in Program.CurrentGame.HostNetwork.ConnectionStrings)
            {
                lstConnections.Items.Add(connection.IP.ToString());
                lstConnections.Items[i].SubItems.Add(connection.Port.ToString());
                if (connection.InternetRoutable)
                {
                    lstConnections.Items[i].SubItems.Add("Internet");
                    lstConnections.Items[i].SubItems.Add("Not Yet Tested");
                    ThreadPool.QueueUserWorkItem(TestConnection, new List<string> {
                        connection.IP.ToString(),
                        Convert.ToString(connection.Port),
                        Convert.ToString(i) });
                }
                else
                {
                    lstConnections.Items[i].SubItems.Add("Local Network");
                    lstConnections.Items[i].SubItems.Add("Not Applicable");
                }
                i++;
            }
            updater.RunWorkerAsync();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void TestConnection(object state)
        {
            string portCheckID = Program.CurrentGame.HostNetwork.PortCheckID.ToString();
            List<string> parameters = (List<string>)state;
            string address = HttpUtility.UrlEncode(parameters[0]);
            string port = parameters[1];
            string url = "https://portcheck.azurewebsites.net/?IP=" + address +
                "&Port=" + port + "&PortCheckID=" + portCheckID;
            WebClient client = new WebClient();
            string response = "";
            int tries = 0;
            retry:
            try
            {
                lstConnections.Invoke((MethodInvoker)delegate
                {
                    lstConnections.Items[Convert.ToInt32(parameters[2])].SubItems[3].Text = "Testing...";
                });
                try
                {
                    response = client.DownloadString(url);
                }
                catch (WebException) { }

                string resultStr = "";
                bool retry = false;
                switch (response)
                {
                    case "Pass":
                        resultStr = "Successful";
                        break;
                    case "VerifyFailed":
                        resultStr = "Wrong Machine - Manual Forward Required";
                        break;
                    case "ConnectFailed":
                        resultStr = "Failed - Manual Forward Required";
                        break;
                    case "BadIP":
                        resultStr = "Test Failed - IP Misunderstood";
                        if (tries < 4) retry = true;
                        break;
                    case "InvalidPort":
                        resultStr = "Test Failed - Port Out of Range";
                        if (tries < 4) retry = true;
                        break;
                    case "BadPort":
                        resultStr = "Test Failed - Port Misunderstood";
                        if (tries < 4) retry = true;
                        break;
                    case "BadGUID":
                        resultStr = "Test Failed - Client Test ID Misunderstood";
                        if (tries < 4) retry = true;
                        break;
                    case "BadServerGUID":
                        resultStr = "Test Failed - Host Test ID Misunderstood";
                        if (tries < 4) retry = true;
                        break;
                    case "UnknownFail":
                        resultStr = "Test Failed - Reason Unknown";
                        if (tries < 4) retry = true;
                        break;
                    case "NoParams":
                        resultStr = "Test Failed - Parameters Misunderstood";
                        if (tries < 4) retry = true;
                        break;
                    default:
                        resultStr = "Test Failed - Unknown Reason";
                        if (tries < 4) retry = true;
                        break;
                }
                tries++;

                lstConnections.Invoke((MethodInvoker)delegate
                {
                    lstConnections.Items[Convert.ToInt32(parameters[2])].SubItems[3].Text = resultStr;
                    if (retry)
                    {
                        lstConnections.Items[Convert.ToInt32(parameters[2])].SubItems[3].Text = resultStr + " (Retry in 3s)";
                    }
                });
                if (retry)
                {
                    Thread.Sleep(3000);
                    goto retry;
                }
            }
            catch
            { }
        }

        private void TestSelected_Click(object sender, EventArgs e)
        {
            if (lstConnections.SelectedIndices.Count > 0)
            {
                int connectionID = lstConnections.SelectedIndices[0];
                if (connectionID != -1)
                {
                    if (lstConnections.Items[connectionID].SubItems[3].Text == "Not Applicable")
                    {
                        MessageBox.Show("You cannot test a Local Network Address", "Cannot Test",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (lstConnections.Items[connectionID].SubItems[3].Text != "Testing..." ||
                        lstConnections.Items[connectionID].SubItems[3].Text.EndsWith("(Retry in 3s)"))
                    {
                        ThreadPool.QueueUserWorkItem(TestConnection, new List<string> {
                        lstConnections.Items[connectionID].Text,
                        lstConnections.Items[connectionID].SubItems[1].Text,
                        Convert.ToString(connectionID) });
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "uk")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "joepitt")]
        private void CopyToClipboard_Click(object sender, EventArgs e)
        {
            if (lstConnections.SelectedIndices.Count > 0)
            {
                int connectionID = lstConnections.SelectedIndices[0];
                if (connectionID != -1)
                {
                    string clipboardMessage =
                        "Hey, come play a game of Cards by Joe Pitt (https://www.joepitt.co.uk/Project/Cards/) with me" +
                        Environment.NewLine + Environment.NewLine +
                        "IP Address: " + lstConnections.Items[connectionID].Text + Environment.NewLine +
                        "Port: " + lstConnections.Items[connectionID].SubItems[1].Text;
                    Clipboard.Clear();
                    Clipboard.SetText(clipboardMessage);
                    MessageBox.Show("Connection Details Copied to Clipboard, " +
                        "use CTRL+V to paste them into a chat with the other players",
                        "Copied to Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}