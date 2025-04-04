using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Prediction
{
    public partial class frmEnddatURL : Form
    {
        private string strEnddatURL = null;
        private string strTimestamp = null;
        private string strTimezone = null;
        private bool bMostRecent = true;

        public frmEnddatURL(string URL, string Timestamp, string Timezone, bool UseTimestamp)
        {
            InitializeComponent();

            if (URL != null && URL != "") { strEnddatURL = URL; }
            else { strEnddatURL = ""; }

            if (Timestamp != null && Timestamp != "") { strTimestamp = Timestamp; }
            else { strTimestamp = ""; }

            if (Timezone != null && Timezone != "") { cbTimezone.SelectedItem = Timezone; }
            else { cbTimezone.SelectedIndex = 0; }

            bMostRecent = UseTimestamp;
        }


        private void frmEnddatURL_Load(object sender, EventArgs e)
        {
            if (strEnddatURL != null)
                txtEnddatURL.Text = strEnddatURL;

            if (strTimestamp != null)
                tbTimestamp.Text = strTimestamp;

            if (strTimezone != null)
                cbTimezone.SelectedItem = strTimezone;

            rbManual.Checked = bMostRecent;
        }

        
        public void Clear()
        {
            strEnddatURL = "";
            cbTimezone.SelectedIndex = 0;
            strTimestamp = "";
        }


        public bool UseTimestamp
        {
            get { return !bMostRecent; }
        }


        public string Timestamp
        {
            get { return strTimestamp; }
        }


        public string Timezone
        {
            get { return strTimezone; }
        }


        public string URL
        {
            get { return strEnddatURL; }
        }


        private void btnOK_Click(object sender, EventArgs e)
        {
            strEnddatURL = txtEnddatURL.Text;
            strTimestamp = tbTimestamp.Text.Trim();
            try { strTimezone = cbTimezone.SelectedItem.ToString(); }
            catch (NullReferenceException) { strTimezone = "UTC"; }
            bMostRecent = rbMostRecent.Checked;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
