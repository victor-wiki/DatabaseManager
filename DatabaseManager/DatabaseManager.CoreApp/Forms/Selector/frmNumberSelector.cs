using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmNumberSelector : Form
    {
        public decimal InputValue { get; private set; }

        public decimal MinValue
        {
            get { return this.nudValue.Minimum; }
            set { this.nudValue.Minimum = value; }
        }

        public decimal MaxValue
        {
            get { return this.nudValue.Maximum; }
            set { this.nudValue.Maximum = value; }
        }

        public string Title 
        { 
            get { return this.lblTitle.Text; }
            set { this.lblTitle.Text = value; }
        }

        public frmNumberSelector()
        {
            InitializeComponent();
        }

        private void frmNumberSelector_Load(object sender, EventArgs e)
        {

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.InputValue = this.nudValue.Value;

            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
