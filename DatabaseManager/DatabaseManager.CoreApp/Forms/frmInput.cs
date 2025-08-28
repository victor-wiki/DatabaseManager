using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmInput : Form
    {
        public string Content => this.txtContent.Text;

        public frmInput(string title, string defaultContent)
        {
            InitializeComponent();

            this.Text = title;

            if (defaultContent != null)
            {
                this.txtContent.Text = defaultContent;
            }
        }

        private void frmInput_Load(object sender, EventArgs e)
        {
            this.txtContent.Select(this.txtContent.Text.Length, 0);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string content = this.txtContent.Text;

            if (string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Content is required!");
                return;
            }

            this.DialogResult = DialogResult.OK;
        }
       
    }
}
