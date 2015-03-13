using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TwitterLib;

namespace TwitterLib
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Clean out design-time labels
            lblNumFollowing.Text = "";
            lblRateLimit.Text = "";
            lblRemaining.Text = "";

            // Screen name is persisted in app.exe.config
            
            txtScreenName.Text = Properties.Settings.Default.ScreenName;
        }

        void TwitterAPI_RateLimitWake(object sender, EventArgs e)
        {
            MessageBox.Show("Wake");
        }

        private void btnLoadGrid_Click(object sender, EventArgs e)
        {
            int rateLimit;
            int limitRemaining;

            Cursor = Cursors.WaitCursor;
            Peeps peeps = Peeps.PeopleFollowedBy(txtScreenName.Text, out rateLimit, out limitRemaining);

            lblNumFollowing.Text = peeps.Count.ToString();
            lblRateLimit.Text = rateLimit.ToString();
            lblRemaining.Text = limitRemaining.ToString();

            peeps[5].CalcAddlInfo();

            dgvPeeps.DataSource = peeps;
            Cursor = Cursors.Default;
        }

        private void txtScreenName_Validated(object sender, EventArgs e)
        {
            Properties.Settings.Default.ScreenName = txtScreenName.Text;
            Properties.Settings.Default.Save();
        }
    }
}
