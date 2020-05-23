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
using DatabaseInterpreter.Core;
using DatabaseConverter.Core;
using DatabaseInterpreter.Utility;

namespace DatabaseManager
{
    public partial class frmTableCopy : Form, IObserver<FeedbackInfo>
    {
        private ConnectionInfo targetDbConnectionInfo;
        private DbConverter dbConverter = null;
        private bool hasError = false;

        public DatabaseType DatabaseType { get; set; }
        public ConnectionInfo ConnectionInfo { get; set; }
        public Table Table { get; set; }        

        public DatabaseInterpreter.Utility.FeedbackHandler OnFeedback;

        public frmTableCopy()
        {
            InitializeComponent();
        }

        private void frmDbObjectCopy_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            this.txtName.Text = this.Table.Name + "_copy";
        }

        private async void btnExecute_Click(object sender, EventArgs e)
        {
            this.hasError = false;

            await Task.Run(() => this.CopyTable());
        }

        private async Task CopyTable()
        {
            string name = this.txtName.Text.Trim();

            if (!this.ValidateInputs())
            {
                return;
            }

            try
            {
                GenerateScriptMode scriptMode = this.GetGenerateScriptMode();

                bool isTableExisted = false;

                if (scriptMode.HasFlag(GenerateScriptMode.Schema))
                {
                    isTableExisted = await this.IsNameExisted();
                }

                if (isTableExisted)
                {
                    name = name + "_copy";
                }

                SchemaInfo schemaInfo = new SchemaInfo();
                schemaInfo.Tables.Add(this.Table);

                DatabaseType targetDatabaseType = this.rbAnotherDatabase.Checked ? this.ucConnection.DatabaseType : this.DatabaseType;
                ConnectionInfo targetConnectionInfo = this.rbAnotherDatabase.Checked ? this.targetDbConnectionInfo : this.ConnectionInfo;

                DbInterpreterOption sourceOption = new DbInterpreterOption();
                DbInterpreterOption targetOption = new DbInterpreterOption();

                targetOption.TableScriptsGenerateOption.GenerateIdentity = this.chkGenerateIdentity.Checked;

                DbConveterInfo source = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, this.ConnectionInfo, sourceOption) };
                DbConveterInfo target = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(targetDatabaseType, targetConnectionInfo, targetOption) };

                source.TableNameMappings.Add(this.Table.Name, name);

                this.btnExecute.Enabled = false;

                using (this.dbConverter = new DbConverter(source, target))
                {
                    this.dbConverter.Option.RenameTableChildren = isTableExisted || this.rbSameDatabase.Checked;
                    this.dbConverter.Option.GenerateScriptMode = scriptMode;
                    this.dbConverter.Option.BulkCopy = true;
                    this.dbConverter.Option.UseTransaction = true;
                    this.dbConverter.Option.ConvertComputeColumnExpression = true;
                    this.dbConverter.Option.IgnoreNotSelfForeignKey = true;

                    this.dbConverter.Subscribe(this);

                    if (this.DatabaseType == DatabaseType.MySql)
                    {
                        source.DbInterpreter.Option.InQueryItemLimitCount = 2000;
                    }

                    if (this.DatabaseType != targetDatabaseType)
                    {
                        if (targetDatabaseType == DatabaseType.SqlServer)
                        {
                            target.DbOwner = "dbo";
                        }
                        else if (targetDatabaseType == DatabaseType.MySql)
                        {
                            target.DbInterpreter.Option.RemoveEmoji = true;
                        }
                    }
                    else
                    {
                        target.DbOwner = this.Table.Owner;
                    }

                    this.dbConverter.Option.SplitScriptsToExecute = true;

                    await this.dbConverter.Convert(schemaInfo);

                    if (!this.hasError && !this.dbConverter.HasError && !source.DbInterpreter.HasError && !target.DbInterpreter.HasError)
                    {
                        if (!this.dbConverter.CancelRequested)
                        {
                            MessageBox.Show("Copy finished", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Task has been canceled.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.hasError = true;
                this.HandleException(ex);
            }
            finally
            {
                this.btnExecute.Enabled = true;
            }
        }

        private GenerateScriptMode GetGenerateScriptMode()
        {
            GenerateScriptMode scriptMode = GenerateScriptMode.None;

            if (this.chkScriptSchema.Checked)
            {
                scriptMode = scriptMode | GenerateScriptMode.Schema;
            }
            if (this.chkScriptData.Checked)
            {
                scriptMode = scriptMode | GenerateScriptMode.Data;
            }

            return scriptMode;
        }

        private void HandleException(Exception ex)
        {
            string errMsg = ExceptionHelper.GetExceptionDetails(ex);

            LogHelper.LogError(errMsg);

            MessageBox.Show(errMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rbSameDatabase_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void rbAnotherDatabase_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlState();
        }

        private void SetControlState()
        {
            this.ucConnection.Enabled = this.rbAnotherDatabase.Checked;

            this.txtName.Text = this.rbSameDatabase.Checked ? $"{this.Table.Name}_copy" : this.Table.Name;
        }

        private void ucConnection_OnSelectedChanged(object sender, ConnectionInfo connectionInfo)
        {
            this.targetDbConnectionInfo = connectionInfo;
        }

        private bool ValidateInputs()
        {
            string name = this.txtName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name can't be empty.");
                return false;
            }

            if (this.rbAnotherDatabase.Checked)
            {
                if (this.targetDbConnectionInfo == null)
                {
                    MessageBox.Show("Please specify target database connection.");
                    return false;
                }
            }

            GenerateScriptMode scriptMode = this.GetGenerateScriptMode();

            if (scriptMode == GenerateScriptMode.None)
            {
                MessageBox.Show("Please specify the script mode.");
                return false;
            }

            return true;
        }

        private async Task<bool> IsNameExisted()
        {
            DatabaseType databaseType = this.rbAnotherDatabase.Checked ? this.ucConnection.DatabaseType : this.DatabaseType;

            ConnectionInfo connectionInfo = this.rbAnotherDatabase.Checked ? this.targetDbConnectionInfo : this.ConnectionInfo;

            DbInterpreterOption option = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple };

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(databaseType, connectionInfo, option);

            SchemaInfoFilter filter = new SchemaInfoFilter() { TableNames = new string[] { this.txtName.Text.Trim() } };

            var tables = await dbInterpreter.GetTablesAsync(filter);

            if (tables.Count > 0)
            {
                return true;
            }

            return false;
        }

        public void OnNext(FeedbackInfo value)
        {
            this.Feedback(value);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        private void Feedback(FeedbackInfo info)
        {
            if (this.OnFeedback != null)
            {
                this.OnFeedback(info);
            }
        }
    }
}
