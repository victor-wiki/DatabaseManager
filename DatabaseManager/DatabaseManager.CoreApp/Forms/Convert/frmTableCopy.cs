using DatabaseConverter.Core;
using DatabaseConverter.Core.Model;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Forms;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.IO;
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
            Image tipImage = IconImageHelper.GetImage(IconChar.InfoCircle, IconImageHelper.TipColor);
          
            this.picNeedPreviewTip.Image = tipImage;

            this.txtName.Text = this.Table.Name + "_copy";

            this.SetSchemaControlStates();
        }

        private async void btnExecute_Click(object sender, EventArgs e)
        {
            Task.Run(() => this.CopyTable());
        }

        private async Task<bool> TestTargetConnection()
        {
            DbInterpreter dbInterpreter = this.GetTargetDbInterpreter();

            try
            {
                using (DbConnection connection = dbInterpreter.CreateConnection())
                {
                    await connection.OpenAsync();

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                return false;
            }
        }

        private async void CopyTable()
        {
            string name = this.txtName.Text.Trim();

            if (!this.ValidateInputs())
            {
                return;
            }

            if (await this.TestTargetConnection() == false)
            {
                return;
            }

            try
            {
                GenerateScriptMode scriptMode = this.GetGenerateScriptMode();

                bool isTableExisted = false;
                bool isDifferentTableName = false;

                Action checkTableName = async () =>
                {
                    if (scriptMode.HasFlag(GenerateScriptMode.Schema))
                    {
                        isTableExisted = await this.IsNameExisted(name);
                    }

                    if (isTableExisted)
                    {
                        name = name + "_copy";
                        isDifferentTableName = true;
                    }
                };

                do
                {
                    checkTableName();
                }
                while (isTableExisted);

                SchemaInfo schemaInfo = new SchemaInfo();
                schemaInfo.Tables.Add(this.Table);

                DatabaseType targetDatabaseType = this.rbAnotherDatabase.Checked ? this.ucConnection.DatabaseType : this.DatabaseType;
                ConnectionInfo targetConnectionInfo = this.rbAnotherDatabase.Checked ? this.targetDbConnectionInfo : this.ConnectionInfo;

                DbInterpreterOption sourceOption = new DbInterpreterOption() { ThrowExceptionWhenErrorOccurs = true };
                DbInterpreterOption targetOption = new DbInterpreterOption() { ThrowExceptionWhenErrorOccurs = true };

                targetOption.TableScriptsGenerateOption.GenerateIdentity = this.chkGenerateIdentity.Checked;

                DbConveterInfo source = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, this.ConnectionInfo, sourceOption) };
                DbConveterInfo target = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(targetDatabaseType, targetConnectionInfo, targetOption) };

                source.DatabaseObjectType = DatabaseObjectType.Table | DatabaseObjectType.Column;

                if (this.chkPrimaryKey.Checked)
                {
                    source.DatabaseObjectType |= DatabaseObjectType.PrimaryKey;
                }

                if (this.chkForeignKey.Checked)
                {
                    source.DatabaseObjectType |= DatabaseObjectType.ForeignKey;
                }

                if (this.chkIndex.Checked)
                {
                    source.DatabaseObjectType |= DatabaseObjectType.Index;
                }

                if (this.chkCheckConstraint.Checked)
                {
                    source.DatabaseObjectType |= DatabaseObjectType.Constraint;
                }

                if (this.chkTrigger.Checked)
                {
                    source.DatabaseObjectType |= DatabaseObjectType.Trigger;
                }

                if (this.chkScriptData.Checked && !target.DbInterpreter.SupportBulkCopy)
                {
                    target.DbInterpreter.Option.ScriptOutputMode = GenerateScriptOutputMode.WriteToString;
                }

                source.TableNameMappings.Add(this.Table.Name, name);

                this.btnExecute.Enabled = false;

                using (this.dbConverter = new DbConverter(source, target))
                {
                    var option = this.dbConverter.Option;

                    option.RenameTableChildren = isTableExisted || this.rbSameDatabase.Checked;
                    option.GenerateScriptMode = scriptMode;
                    option.BulkCopy = true;
                    option.UseTransaction = true;
                    option.ConvertComputeColumnExpression = true;
                    //option.IgnoreNotSelfForeignKey = true;
                    option.UseOriginalDataTypeIfUdtHasOnlyOneAttr = SettingManager.Setting.UseOriginalDataTypeIfUdtHasOnlyOneAttr;
                    option.OnlyForTableCopy = true;
                    option.NeedPreview = this.chkNeedPreview.Checked;

                    if (this.cboDataTypeMappingFile.SelectedIndex >= 0)
                    {
                        ConvertConfigFileInfo configFileInfo = this.cboDataTypeMappingFile.SelectedItem as ConvertConfigFileInfo;

                        option.DataTypeMappingFilePath = configFileInfo.FilePath;
                    }

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

                    DbConvertResult result = await this.dbConverter.Convert(schemaInfo, this.Table.Schema);

                    if (option.NeedPreview)
                    {
                        var translatedSchemaInfo = result.TranslatedSchemaInfo;
                        IEnumerable<TableColumnContentMaxLength> tableColumnContentMaxLengths = null;

                        if (SettingManager.Setting.ShowCurrentColumnContentMaxLength)
                        {
                            DbStatistic statistic = new DbStatistic(this.DatabaseType, this.ConnectionInfo);

                            statistic.Subscribe(this);

                            tableColumnContentMaxLengths = await statistic.GetTableColumnContentLengths(new SchemaInfoFilter() { Schema= this.Table.Schema, TableNames = [this.Table.Name]  });
                        }

                        if (translatedSchemaInfo != null)
                        {
                            this.Invoke(() =>
                            {
                                frmSchemaPreviewer form = new frmSchemaPreviewer(result.TranslatedSchemaInfo, option.SchemaMappings, source.TableNameMappings, tableColumnContentMaxLengths);

                                form.ShowDialog();

                                translatedSchemaInfo = form.SchemaInfo;

                                option.NeedPreview = false;                                

                            });

                            result = await this.dbConverter.Convert(schemaInfo, this.Table.Schema, translatedSchemaInfo);
                        }
                    }

                    if (result.InfoType == DbConvertResultInfoType.Information)
                    {
                        if (!this.dbConverter.CancelRequested)
                        {
                            string msg = "Table copied." + (isDifferentTableName ? $@"{Environment.NewLine}The target table name is ""{name}""." : "");

                            MessageBox.Show(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Task has been canceled.");
                        }
                    }
                    else if (result.InfoType == DbConvertResultInfoType.Warnning)
                    {
                        MessageBox.Show(result.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (result.InfoType == DbConvertResultInfoType.Error) //message shows in main form because it uses Subscribe above
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

        private void ucConnection_ProfileSelectedChanged(object sender, ConnectionInfo connectionInfo)
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

            this.chkNeedPreview.Checked = SettingManager.Setting.NeedPreviewBeforeConvert;
        }

        private async void ShowSchemas()
        {
            if (this.cboSchema.Visible)
            {
                if (this.rbAnotherDatabase.Checked && !this.ucConnection.ValidateProfile())
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

        private async Task<bool> IsNameExisted(string name)
        {
            DbInterpreter dbInterpreter = this.GetTargetDbInterpreter();

            SchemaInfoFilter filter = new SchemaInfoFilter() { TableNames = new string[] { name } };

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

        private void ucConnection_DatabaseTypeSelectedChanged(object sender, EventArgs e)
        {
            this.ShowDataTypeMappingConfigFiles();
        }

        private void ShowDataTypeMappingConfigFiles()
        {
            this.cboDataTypeMappingFile.Items.Clear();
            this.lblDataTypeMappingFileType.Text = "";

            if (this.ucConnection.IsDbTypeSelected())
            {
                DatabaseType sourceDatabaseType = this.DatabaseType;
                DatabaseType targetDatabaseType = this.ucConnection.DatabaseType;

                List<ConvertConfigFileInfo> files = new List<ConvertConfigFileInfo>();

                if(sourceDatabaseType!= targetDatabaseType)
                {
                    string defaultDataTypeMappingFilePath = DataTypeMappingManager.GetDataTypeMappingFilePath(sourceDatabaseType, targetDatabaseType);

                    if(File.Exists(defaultDataTypeMappingFilePath))
                    {
                        files.Add(new ConvertConfigFileInfo() { Name = Path.GetFileNameWithoutExtension(defaultDataTypeMappingFilePath), FilePath = defaultDataTypeMappingFilePath, IsDefault = true });
                    }                    
                }                

                string customMappingFolder = SettingManager.Setting.CustomMappingFolder;

                string customMappingSubFolder = DataTypeMappingManager.GetDataTypeMappingCustomSubFolder(sourceDatabaseType, targetDatabaseType, customMappingFolder);

                DirectoryInfo di = new DirectoryInfo(customMappingSubFolder);

                if (di.Exists)
                {
                    files.AddRange(di.GetFiles().OrderBy(item => item.Name).Select(item => new ConvertConfigFileInfo() { Name = Path.GetFileNameWithoutExtension(item.Name), FilePath = item.FullName }));
                }

                this.cboDataTypeMappingFile.Items.AddRange(files.ToArray());

                if (this.cboDataTypeMappingFile.Items.Count > 0)
                {
                    this.cboDataTypeMappingFile.SelectedIndex = 0;
                }
            }
        }

        private void cboDataTypeMappingFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConvertConfigFileInfo selectedItem = this.cboDataTypeMappingFile.SelectedItem as ConvertConfigFileInfo;

            if (selectedItem != null)
            {
                this.lblDataTypeMappingFileType.Text = $"({(selectedItem.IsDefault ? "Default" : "Custom")})";
            }
            else
            {
                this.lblDataTypeMappingFileType.Text = "";
            }
        }

        private void lblDataTypeMappingFileType_Click(object sender, EventArgs e)
        {
            var selectedItem = this.cboDataTypeMappingFile.SelectedItem;

            if (selectedItem != null)
            {
                ConvertConfigFileInfo configFileInfo = selectedItem as ConvertConfigFileInfo;

                DatabaseType sourceDatabaseType = this.DatabaseType;
                DatabaseType targetDatabaseType = this.ucConnection.DatabaseType;

                frmDataTypeMappingSetting form = new frmDataTypeMappingSetting(true, sourceDatabaseType, targetDatabaseType, configFileInfo.IsDefault, configFileInfo.Name);

                form.ShowDialog();
            }
        }
    }
}
