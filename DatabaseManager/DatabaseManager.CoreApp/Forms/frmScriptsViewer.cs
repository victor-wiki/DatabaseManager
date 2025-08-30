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
using DatabaseManager.Core;
using DatabaseManager.Helper;

namespace DatabaseManager.Forms
{
    public partial class frmScriptsViewer : Form
    {
        public DatabaseType DatabaseType { get; set; }


        public frmScriptsViewer()
        {
            InitializeComponent();

            this.Init();
        }

        private void frmScriptsViewer_Load(object sender, EventArgs e)
        {
            TextEditorHelper.Highlighting(this.txtScripts, this.DatabaseType);
        }

        private void Init()
        {           
            TextEditorHelper.ApplySetting(this.txtScripts, SettingManager.Setting.TextEditorOption);
        }

        public void LoadScripts(string scripts)
        {
            this.txtScripts.Text = scripts;          
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
