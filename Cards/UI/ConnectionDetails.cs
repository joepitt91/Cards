using JoePitt.Cards.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JoePitt.Cards.UI
{
    public partial class ConnectionDetails : Form
    {
        /// <summary>
        /// Displays connection details and players for hosted network games.
        /// </summary>
        public ConnectionDetails()
        {
            InitializeComponent();
        }

        private void ConnectionDetails_Load(object sender, EventArgs e)
        {
            int i = 0;
            foreach (Net.ConnectionDetails connection in Program.CurrentGame.HostNetwork.ConnectionStrings)
            {
                lstConnections.Items.Add(connection.IP.ToString());
                lstConnections.Items[i].SubItems.Add(connection.Port.ToString());
                lstConnections.Items[i].SubItems.Add(connection.IP.AddressFamily.ToString() + " - " + connection.InternetRoutable);
                lstConnections.Items[i].SubItems.Add(connection.TestPassed.ToString());
            }
        }

        private void BtnTestSelected_Click(object sender, EventArgs e)
        {

        }
    }
}
