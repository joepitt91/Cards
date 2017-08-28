using System;
using System.Windows.Forms;

namespace JoePitt.Cards.UI
{
    /// <summary>
    /// Licence UI Control.
    /// </summary>
    public partial class License : Form
    {
        /// <summary>
        /// Initalises the License UI.
        /// </summary>
        public License()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void License_Load(object sender, EventArgs e)
        {
            txtTerms.Select(0, 0);
            txtTerms.AccessibleName = txtTerms.Text;
        }
    }
}
