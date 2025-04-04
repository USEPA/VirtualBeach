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
    public partial class frmShowColumnMapping : Form
    {
        string _leftCaption;
        string _rightCaption;
        Dictionary<string, string> _dctColMap;

        public frmShowColumnMapping(string leftCaption, string rightCaption, Dictionary<string,string> dctColMap)
        {
            InitializeComponent();
            _leftCaption = leftCaption;
            _rightCaption = rightCaption;
            if (dctColMap != null)
                _dctColMap = new Dictionary<string, string>(dctColMap);
        }

        private void frmShowColumnMapping_Load(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(_leftCaption, typeof(string));
            dt.Columns.Add(_rightCaption, typeof(string));

            foreach (string key in _dctColMap.Keys)
            {
                DataRow dr = dt.NewRow();
                dr[0] = key;
                dr[1] = _dctColMap[key];

                dt.Rows.Add(dr);
            }

            dataGridView1.DataSource = dt;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {            
            DialogResult confirmResult = MessageBox.Show("Are you sure to clear the column mapping?", "Confirm Clear", MessageBoxButtons.YesNo);
            if (confirmResult == System.Windows.Forms.DialogResult.Yes)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Abort;
                this.Close();
            }
        }

        
    }
}
