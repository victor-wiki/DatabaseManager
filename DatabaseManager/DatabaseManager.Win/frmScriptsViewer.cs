using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseInterpreter.Model;
using DatabaseManager.Helper;

namespace DatabaseManager
{
    public partial class frmScriptsViewer : Form
    {
        public DatabaseType DatabaseType { get; set; }


        public frmScriptsViewer()
        {
            InitializeComponent();
        }

        private void frmScriptsViewer_Load(object sender, EventArgs e)
        {

        }

        public void LoadScripts(string scripts)
        {
            this.txtScripts.AppendText(scripts);

            RichTextBoxHelper.Highlighting(this.txtScripts, this.DatabaseType, false);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
