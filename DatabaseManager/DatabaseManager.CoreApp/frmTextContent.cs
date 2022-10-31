using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmTextContent : Form
    {
        public frmTextContent()
        {
            InitializeComponent();
        }

        public frmTextContent(string content)
        {
            InitializeComponent();

            this.txtContent.Text = content;           
        }

        private void frmTextContent_Load(object sender, EventArgs e)
        {
            this.txtContent.Select(0, 0);
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            string content = this.txtContent.Text.Trim();

            if(string.IsNullOrEmpty(content))
            {
                MessageBox.Show("The content is empty.");
                return;
            }

            Clipboard.SetDataObject(content);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
