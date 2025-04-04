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
    public partial class frmEnddatImportDate : Form
    {
        public frmEnddatImportDate()
        {
            InitializeComponent();
        }

        public DateTime Date
        {
            get
            {
                return calEnddatImportDate.SelectionStart;
            }
        }

        private void frmEnddatImportDate_Load(object sender, EventArgs e)
        {
            calEnddatImportDate.MaxDate = DateTime.Now;
        }
    }
}
