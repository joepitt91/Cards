using System;
using System.Windows.Forms;

namespace JoePitt.Cards.UI
{
    /// <summary>
    /// Rules UI.
    /// </summary>
    public partial class Rules : Form
    {
        /// <summary>
        /// Initalises the Rules UI.
        /// </summary>
        public Rules()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void Rules_Load(object sender, EventArgs e)
        {
            txtRules.Select(0, 0);
            txtRules.AccessibleName = txtRules.Text;
        }
    }
}
