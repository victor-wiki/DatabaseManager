using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmDiagnose : Form
    {
        private DatabaseType databaseType { get; set; }
        private ConnectionInfo connectionInfo { get; set; }
        private string schema;
        private DbManager dbManager = null;

        public frmDiagnose(DatabaseType databaseType, ConnectionInfo connectionInfo, string schema)
        {
            InitializeComponent();

            this.databaseType = databaseType;
            this.connectionInfo = connectionInfo;
            this.schema = schema;

            this.dbManager = new DbManager();
        }

        private void frmDiagnose_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            if (this.databaseType == DatabaseType.Oracle)
            {
                this.rbNotNullWithEmpty.Enabled = false;
                this.rbSelfReferenceSame.Checked = true;
            }

            this.rbPrimaryKeyColumnIsNullable.Enabled = this.databaseType == DatabaseType.Sqlite;

            if (this.databaseType == DatabaseType.Oracle || this.databaseType == DatabaseType.Postgres || this.databaseType == DatabaseType.Sqlite)
            {
                this.tabControl.TabPages.Remove(this.tabForScript);
            }
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.dbManager.Subscribe(observer);
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
            else if(this.rbEmptyValueRatherThanNull.Checked)
            {
                diagnoseType = TableDiagnoseType.EmptyValueRatherThanNull;
            }
            else if(this.rbPrimaryKeyColumnIsNullable.Checked)
            {
                diagnoseType = TableDiagnoseType.PrimaryKeyColumnIsNullable;
            }

            if (diagnoseType == TableDiagnoseType.None)
            {
                MessageBox.Show("Please select a type for table diagnose.");
                return;
            }

            try
            {
                this.btnStart.Enabled = false;

                TableDiagnoseResult result = await dbManager.DiagnoseTable(this.databaseType, this.connectionInfo, this.schema, diagnoseType);

                if (result.Details.Count > 0)
                {
                    frmTableDiagnoseResult frmResult = new frmTableDiagnoseResult()
                    {
                        DatabaseType = this.databaseType,
                        ConnectionInfo = this.connectionInfo
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

                List<ScriptDiagnoseResult> results = await dbManager.DiagnoseScript(this.databaseType, this.connectionInfo, this.schema, diagnoseType);

                if (results.Count > 0)
                {
                    frmScriptDiagnoseResult frmResult = new frmScriptDiagnoseResult()
                    {
                        DatabaseType = this.databaseType,
                        ConnectionInfo = this.connectionInfo,
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
