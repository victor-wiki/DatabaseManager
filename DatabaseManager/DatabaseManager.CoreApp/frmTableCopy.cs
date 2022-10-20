using DatabaseConverter.Core;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmTableCopy : Form, IObserver<FeedbackInfo>
    {
        private ConnectionInfo targetDbConnectionInfo;
        private DbConverter dbConverter = null;

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

            this.SetSchemaControlStates();
        }

        private async void btnExecute_Click(object sender, EventArgs e)
        {
            await this.CopyTable();
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

                DbInterpreterOption sourceOption = new DbInterpreterOption() { ThrowExceptionWhenErrorOccurs = true };
                DbInterpreterOption targetOption = new DbInterpreterOption() { ThrowExceptionWhenErrorOccurs = true };

                targetOption.TableScriptsGenerateOption.GenerateIdentity = this.chkGenerateIdentity.Checked;

                DbConveterInfo source = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, this.ConnectionInfo, sourceOption) };
                DbConveterInfo target = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(targetDatabaseType, targetConnectionInfo, targetOption) };

                if (this.chkOnlyCopyTable.Checked)
                {
                    source.DatabaseObjectType = DatabaseObjectType.Table | DatabaseObjectType.Column;
                }

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
                    this.dbConverter.Option.UseOriginalDataTypeIfUdtHasOnlyOneAttr = SettingManager.Setting.UseOriginalDataTypeIfUdtHasOnlyOneAttr;
                    this.dbConverter.Option.OnlyForTableCopy = true;


                    if (this.cboSchema.Visible)
                    {
                        string targetSchema = string.IsNullOrEmpty(this.cboSchema.Text) ? target.DbInterpreter.DefaultSchema : this.cboSchema.Text;
                        this.dbConverter.Option.SchemaMappings.Add(new SchemaMappingInfo() { SourceSchema = this.Table.Schema, TargetSchema = targetSchema });
                    }

                    this.dbConverter.Subscribe(this);

                    if (this.DatabaseType == DatabaseType.MySql)
                    {
                        source.DbInterpreter.Option.InQueryItemLimitCount = 2000;
                    }

                    this.dbConverter.Option.SplitScriptsToExecute = true;

                    DbConverterResult result = await this.dbConverter.Convert(schemaInfo);

                    if (result.InfoType == DbConverterResultInfoType.Information)
                    {
                        if (!this.dbConverter.CancelRequested)
                        {
                            MessageBox.Show("Table copied.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Task has been canceled.");
                        }
                    }
                    else if (result.InfoType == DbConverterResultInfoType.Warnning)
                    {
                        MessageBox.Show(result.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (result.InfoType == DbConverterResultInfoType.Error) //message shows in main form because it uses Subscribe above
                    {
                        // MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
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

        private void SetControlState()
        {
            this.ucConnection.Enabled = this.rbAnotherDatabase.Checked;
            this.txtName.Text = this.rbSameDatabase.Checked ? $"{this.Table.Name}_copy" : this.Table.Name;

            this.SetSchemaControlStates();
        }

        private void ucConnection_OnSelectedChanged(object sender, ConnectionInfo connectionInfo)
        {
            this.targetDbConnectionInfo = connectionInfo;

            this.SetSchemaControlStates();
        }

        private void SetSchemaControlStates()
        {
            this.cboSchema.Text = "";
            this.cboSchema.Items.Clear();

            var targetDbInterpreter = this.GetTargetDbInterpreter();

            if (targetDbInterpreter != null)
            {
                DatabaseType targetDbType = targetDbInterpreter.DatabaseType;

                this.lblSchema.Visible = this.cboSchema.Visible = targetDbType == DatabaseType.SqlServer || targetDbType == DatabaseType.Postgres;

                this.ShowSchemas();
            }
        }

        private async void ShowSchemas()
        {
            if (this.cboSchema.Visible)
            {
                if(this.rbAnotherDatabase.Checked && !this.ucConnection.ValidateProfile())
                {
                    return;
                }

                try
                {
                    var targetDbSchemas = await this.GetTargetDbInterpreter().GetDatabaseSchemasAsync();

                    foreach (var schema in targetDbSchemas)
                    {
                        this.cboSchema.Items.Add(schema.Name);

                        if (this.Table.Schema == schema.Name)
                        {
                            this.cboSchema.Text = schema.Name;
                        }
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
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

        private DbInterpreter GetTargetDbInterpreter()
        {
            DatabaseType databaseType = this.rbAnotherDatabase.Checked ? this.ucConnection.DatabaseType : this.DatabaseType;

            ConnectionInfo connectionInfo = this.rbAnotherDatabase.Checked ? this.targetDbConnectionInfo : this.ConnectionInfo;

            DbInterpreterOption option = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple };

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(databaseType, connectionInfo, option);

            return dbInterpreter;
        }

        private async Task<bool> IsNameExisted()
        {
            DbInterpreter dbInterpreter = this.GetTargetDbInterpreter();

            SchemaInfoFilter filter = new SchemaInfoFilter() { TableNames = new string[] { this.txtName.Text.Trim() } };

            if (!string.IsNullOrEmpty(this.cboSchema.Text))
            {
                filter.Schema = this.cboSchema.Text;
            }

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
