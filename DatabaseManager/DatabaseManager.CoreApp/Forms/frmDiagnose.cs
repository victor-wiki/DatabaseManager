using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseManager.Model;
using DatabaseManager.Core;
using DatabaseInterpreter.Utility;
using DatabaseInterpreter.Model;
using DatabaseManager.Forms;

namespace DatabaseManager
{
    public partial class frmDiagnose : Form
    {
        public DatabaseType DatabaseType { get; set; }
        public ConnectionInfo ConnectionInfo { get; set; }
        public string Schema { get; set; }

        DbManager dbManager = null;

        public frmDiagnose()
        {
            InitializeComponent();
        }

        private void frmDiagnose_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            if (this.DatabaseType == DatabaseType.Oracle)
            {
                this.rbNotNullWithEmpty.Enabled = false;
                this.rbSelfReferenceSame.Checked = true;
            }

            if (this.DatabaseType == DatabaseType.Oracle || this.DatabaseType == DatabaseType.Postgres || this.DatabaseType == DatabaseType.Sqlite)
            {
                this.tabControl.TabPages.Remove(this.tabForScript);
            }
        }

        public void Init(IObserver<FeedbackInfo> observer)
        {
            this.dbManager = new DbManager();

            dbManager.Subscribe(observer);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            string tabPageName = this.tabControl.SelectedTab.Name;

            if (tabPageName == this.tabForTable.Name)
            {
                this.DiagnoseTable();
            }
            else if (tabPageName == this.tabForScript.Name)
            {
                this.DiagnoseScript();
            }
        }

        private async void DiagnoseTable()
        {
            TableDiagnoseType diagnoseType = TableDiagnoseType.None;

            if (this.rbNotNullWithEmpty.Checked)
            {
                diagnoseType = TableDiagnoseType.NotNullWithEmpty;
            }
            else if (this.rbWithLeadingOrTrailingWhitespace.Checked)
            {
                diagnoseType = TableDiagnoseType.WithLeadingOrTrailingWhitespace;
            }
            else if (this.rbSelfReferenceSame.Checked)
            {
                diagnoseType = TableDiagnoseType.SelfReferenceSame;
            }

            if (diagnoseType == TableDiagnoseType.None)
            {
                MessageBox.Show("Please select a type for table diagnose.");
                return;
            }

            try
            {
                this.btnStart.Enabled = false;

                TableDiagnoseResult result = await dbManager.DiagnoseTable(this.DatabaseType, this.ConnectionInfo, this.Schema, diagnoseType);

                if (result.Details.Count > 0)
                {
                    frmTableDiagnoseResult frmResult = new frmTableDiagnoseResult()
                    {
                        DatabaseType = this.DatabaseType,
                        ConnectionInfo = this.ConnectionInfo
                    };

                    frmResult.LoadResult(result);
                    frmResult.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Diagnosis finished, no invalid data found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHelper.GetExceptionDetails(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnStart.Enabled = true;
            }
        }

        private async void DiagnoseScript()
        {
            ScriptDiagnoseType diagnoseType = ScriptDiagnoseType.None;

            if(this.rbViewColumnAliasWithoutQuotationChar.Checked)
            {
                diagnoseType = ScriptDiagnoseType.ViewColumnAliasWithoutQuotationChar;
            }
            else if (this.rbNameNotMatchForScript.Checked)
            {
                diagnoseType = ScriptDiagnoseType.NameNotMatch;
            }

            if (diagnoseType == ScriptDiagnoseType.None)
            {
                MessageBox.Show("Please select a type for script diagnose.");
                return;
            }

            try
            {
                this.btnStart.Enabled = false;

                List<ScriptDiagnoseResult> results = await dbManager.DiagnoseScript(this.DatabaseType, this.ConnectionInfo, this.Schema, diagnoseType);

                if (results.Count > 0)
                {
                    frmScriptDiagnoseResult frmResult = new frmScriptDiagnoseResult()
                    {
                        DatabaseType = this.DatabaseType,
                        ConnectionInfo = this.ConnectionInfo,
                        DiagnoseType = diagnoseType
                    };

                    frmResult.LoadResults(results);
                    frmResult.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Diagnosis finished, no invalid data found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHelper.GetExceptionDetails(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnStart.Enabled = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
