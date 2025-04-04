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
    public partial class frmWorking : Form
    {
        public delegate void CancelWorkerHandler(object sender, EventArgs e);
        public event CancelWorkerHandler Canceled;

        public frmWorking()
        {
            InitializeComponent();
        }

        public frmWorking(string Message, CancelWorkerHandler CancelDelegate)
        {
            InitializeComponent();
            lblMessage.Text = Message;
            Canceled += new CancelWorkerHandler(CancelDelegate);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (Canceled != null)
            {
                Canceled(null, null);
            }
        } 
    }
}
