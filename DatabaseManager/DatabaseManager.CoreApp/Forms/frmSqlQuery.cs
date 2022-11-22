using DatabaseManager.Helper;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmSqlQuery : Form
    {
        public bool ReadOnly { get; set; }
        public int SplitterDistance { get; set; }
        public bool ShowEditorMessage { get; set; }

        public frmSqlQuery()
        {
            InitializeComponent();          
        }

        private void frmSqlQuery_Load(object sender, EventArgs e)
        {
            
        }

        public void Init()
        {
            this.ucSqlQuery.ReadOnly = this.ReadOnly;
            this.ucSqlQuery.ShowEditorMessage = this.ShowEditorMessage;
            this.ucSqlQuery.SplitterDistance = this.SplitterDistance;
        }

        public void Query(DatabaseObjectDisplayInfo displayInfo)
        {
            this.ucSqlQuery.Editor.AppendText(displayInfo.Content);

            RichTextBoxHelper.Highlighting(this.ucSqlQuery.Editor, displayInfo.DatabaseType, false);

            this.ucSqlQuery.RunScripts(displayInfo);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
