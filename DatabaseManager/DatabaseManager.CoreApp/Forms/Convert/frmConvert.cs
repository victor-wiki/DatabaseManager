using DatabaseConverter.Core;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmConvert : Form, IObserver<FeedbackInfo>
    {
        private const string DONE = "Convert finished";
        private DatabaseType sourceDatabaseType;
        private ConnectionInfo sourceDbConnectionInfo;
        private ConnectionInfo targetDbConnectionInfo;
        private DbConverter dbConverter = null;
        private bool useSourceConnector = true;
        private CancellationTokenSource cancellationTokenSource;
        private IEnumerable<CheckBox> configCheckboxes;
        private List<SchemaMappingInfo> schemaMappings = new List<SchemaMappingInfo>();

        public frmConvert()
        {
            InitializeComponent();
        }

        public frmConvert(DatabaseType sourceDatabaseType, ConnectionInfo sourceConnectionInfo)
        {
            InitializeComponent();

            this.sourceDatabaseType = sourceDatabaseType;
            this.sourceDbConnectionInfo = sourceConnectionInfo;
            this.useSourceConnector = false;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Init();
        }

        private void Init()
        {
            TextBox.CheckForIllegalCrossThreadCalls = false;
            CheckBox.CheckForIllegalCrossThreadCalls = false;

            Image tipImage = IconImageHelper.GetImage(IconChar.InfoCircle, IconImageHelper.TipColor);
            this.picSchemaMappingTip.Image = tipImage;
            this.picNeedPreviewTip.Image = tipImage;
            this.picContinueWhenErrorOccursTip.Image = tipImage;

            this.btnCopyMessage.Image = IconImageHelper.GetImageByFontType(IconChar.Copy, IconFont.Regular);
            this.btnSaveMessage.Image = IconImageHelper.GetImageByFontType(IconChar.Save, IconFont.Solid);

            this.chkNeedPreview.Checked = SettingManager.Setting.NeedPreviewBeforeConvert;

            if (!this.useSourceConnector)
            {
                int increaseHeight = this.sourceDbProfile.Height;
                this.sourceDbProfile.Visible = false;
                this.btnFetch.Height = this.targetDbProfile.ClientHeight;
                this.targetDbProfile.Top -= increaseHeight;
                this.tvDbObjects.Top -= increaseHeight;
                this.gbConfiguration.Top -= increaseHeight;
                this.tvDbObjects.Height += increaseHeight;
                this.gbConfiguration.Height += increaseHeight;

                if (this.sourceDatabaseType == DatabaseType.MySql || this.sourceDatabaseType == DatabaseType.Oracle)
                {
                    this.btnSetSchemaMappings.Enabled = false;
                    this.chkCreateSchemaIfNotExists.Enabled = false;
                    this.chkCreateSchemaIfNotExists.Checked = false;
                }
            }

            this.cboMode.SelectedIndex = 0;

            this.configCheckboxes = this.gbConfiguration.Controls.Cast<Control>()
                                    .Where(item => item is CheckBox).Cast<CheckBox>().ToArray();

            this.SetControlStateByMode();
        }

        private void btnFetch_Click(object sender, EventArgs e)
        {
            this.Invoke(new Action(this.Fetch));
        }

        private async void Fetch()
        {
            DatabaseType dbType;

            if (this.useSourceConnector)
            {
                dbType = this.sourceDbProfile.DatabaseType;

                if (!this.sourceDbProfile.IsDbTypeSelected())
                {
                    MessageBox.Show("Please select a source database type.");
                    return;
                }

                if (!this.sourceDbProfile.IsProfileSelected())
                {
                    MessageBox.Show("Please select a source database profile.");
                    return;
                }

                if (!this.sourceDbProfile.ValidateProfile())
                {
                    return;
                }
            }
            else
            {
                dbType = this.sourceDatabaseType;
            }

            this.btnFetch.Text = "...";

            try
            {
                await this.tvDbObjects.LoadTree(dbType, this.sourceDbConnectionInfo);
                this.btnExecute.Enabled = true;
            }
            catch (Exception ex)
            {
                this.tvDbObjects.ClearNodes();

                string message = ExceptionHelper.GetExceptionDetails(ex);

                LogHelper.LogError(message);

                MessageBox.Show("Error:" + message);
            }

            this.btnFetch.Text = "Fetch";
        }

        private async void btnExecute_Click(object sender, EventArgs e)
        {
            this.txtMessage.ForeColor = Color.Black;
            this.txtMessage.Text = "";

            this.cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() => this.Convert(), this.cancellationTokenSource.Token);
        }

        private bool ValidateSource(SchemaInfo schemaInfo)
        {
            if (!this.tvDbObjects.HasDbObjectNodeSelected())
            {
                MessageBox.Show("Please select objects from tree.");
                return false;
            }

            if (this.sourceDbConnectionInfo == null)
            {
                MessageBox.Show("Source connection is null.");
                return false;
            }

            return true;
        }

        private void SetGenerateScriptOption(params DbInterpreterOption[] options)
        {
            if (options != null)
            {
                string outputFolder = this.txtOutputFolder.Text.Trim();

                foreach (DbInterpreterOption option in options)
                {
                    if (Directory.Exists(outputFolder))
                    {
                        option.ScriptOutputFolder = outputFolder;
                    }
                }
            }
        }

        private GenerateScriptMode GetGenerateScriptMode()
        {
            GenerateScriptMode scriptMode = GenerateScriptMode.None;

            if (this.cboMode.SelectedIndex == 0 || this.cboMode.SelectedIndex == 2)
            {
                scriptMode = scriptMode | GenerateScriptMode.Schema;
            }

            if (this.cboMode.SelectedIndex == 1 || this.cboMode.SelectedIndex == 2)
            {
                scriptMode = scriptMode | GenerateScriptMode.Data;
            }

            return scriptMode;
        }

        private async Task Convert()
        {
            SchemaInfo schemaInfo = this.tvDbObjects.GetSchemaInfo();

            if (!this.ValidateSource(schemaInfo))
            {
                return;
            }

            if (this.targetDbConnectionInfo == null)
            {
                MessageBox.Show("Target connection info is null.");
                return;
            }

            if (!this.targetDbProfile.ValidateProfile())
            {
                return;
            }

            if (this.sourceDbConnectionInfo.Server == this.targetDbConnectionInfo.Server
                && this.sourceDbConnectionInfo.Port == this.targetDbConnectionInfo.Port
                && this.sourceDbConnectionInfo.Database == this.targetDbConnectionInfo.Database)
            {
                MessageBox.Show("Source database cannot be equal to the target database.");
                return;
            }

            DatabaseType sourceDbType = this.useSourceConnector ? this.sourceDbProfile.DatabaseType : this.sourceDatabaseType;
            DatabaseType targetDbType = this.targetDbProfile.DatabaseType;

            DbInterpreterOption sourceScriptOption = new DbInterpreterOption()
            {
                ScriptOutputMode = GenerateScriptOutputMode.None,
                SortObjectsByReference = true,
                GetTableAllObjects = true,
                ThrowExceptionWhenErrorOccurs = true,
                ExcludeGeometryForData = this.chkExcludeGeometryForData.Checked
            };

            DbInterpreterOption targetScriptOption = new DbInterpreterOption()
            {
                ScriptOutputMode = GenerateScriptOutputMode.WriteToString,
                ThrowExceptionWhenErrorOccurs = true,
                ExcludeGeometryForData = this.chkExcludeGeometryForData.Checked
            };

            this.SetGenerateScriptOption(sourceScriptOption, targetScriptOption);            

            if (this.chkOutputScripts.Checked)
            {
                targetScriptOption.ScriptOutputMode |= GenerateScriptOutputMode.WriteToFile;
            }

            if (this.chkTreatBytesAsNull.Checked)
            {
                sourceScriptOption.TreatBytesAsNullForReading = true;
                targetScriptOption.TreatBytesAsNullForExecuting = true;
            }

            targetScriptOption.TableScriptsGenerateOption.GenerateIdentity = this.chkGenerateIdentity.Checked;
            targetScriptOption.TableScriptsGenerateOption.GenerateConstraint = this.chkGenerateCheckConstraint.Checked;
            targetScriptOption.TableScriptsGenerateOption.GenerateComment = this.chkGenerateComment.Checked;

            GenerateScriptMode scriptMode = this.GetGenerateScriptMode();

            if (scriptMode == GenerateScriptMode.None)
            {
                MessageBox.Show("Please specify the script mode.");
                return;
            }

            DbConveterInfo source = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(sourceDbType, this.sourceDbConnectionInfo, sourceScriptOption) };
            DbConveterInfo target = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(targetDbType, this.targetDbConnectionInfo, targetScriptOption) };

            bool iscancelled = false;

            try
            {
                using (this.dbConverter = new DbConverter(source, target))
                {
                    var option = this.dbConverter.Option;

                    option.GenerateScriptMode = scriptMode;
                    option.BulkCopy = this.chkBulkCopy.Checked;
                    option.ExecuteScriptOnTargetServer = this.chkExecuteOnTarget.Checked;
                    option.UseTransaction = this.chkUseTransaction.Checked;
                    option.ContinueWhenErrorOccurs = this.chkContinueWhenErrorOccurs.Checked;
                    option.ConvertComputeColumnExpression = this.chkComputeColumn.Checked;
                    option.OnlyCommentComputeColumnExpressionInScript = this.chkOnlyCommentComputeExpression.Checked;
                    option.SplitScriptsToExecute = true;
                    option.UseOriginalDataTypeIfUdtHasOnlyOneAttr = SettingManager.Setting.UseOriginalDataTypeIfUdtHasOnlyOneAttr;
                    option.CreateSchemaIfNotExists = this.chkCreateSchemaIfNotExists.Checked;
                    option.NcharToDoubleChar = this.chkNcharToDoubleChar.Checked;
                    option.ConvertConcatChar = TranslateHelper.NeedConvertConcatChar(SettingManager.Setting.ConvertConcatCharTargetDatabases, targetDbType);
                    option.CollectTranslateResultAfterTranslated = false;
                    option.SchemaMappings = this.schemaMappings;
                    option.NeedPreview = this.chkNeedPreview.Checked;

                    if (this.cboDataTypeMappingFile.SelectedIndex >= 0)
                    {
                        ConvertConfigFileInfo configFileInfo = this.cboDataTypeMappingFile.SelectedItem as ConvertConfigFileInfo;

                        option.DataTypeMappingFilePath = configFileInfo.FilePath;
                    }

                    if (sourceDbType == DatabaseType.MySql)
                    {
                        source.DbInterpreter.Option.InQueryItemLimitCount = 2000;
                    }

                    if (targetDbType == DatabaseType.MySql)
                    {
                        target.DbInterpreter.Option.RemoveEmoji = true;
                    }

                    this.dbConverter.Subscribe(this);

                    this.SetExecuteButtonEnabled(false);

                    DbConvertResult result = await this.dbConverter.Convert(schemaInfo);

                    if (option.NeedPreview)
                    {
                        var translatedSchemaInfo = result.TranslatedSchemaInfo;
                        IEnumerable<TableColumnContentMaxLength> tableColumnContentMaxLengths = null;

                        if (SettingManager.Setting.ShowCurrentColumnContentMaxLength)
                        {
                            DbStatistic statistic = new DbStatistic(sourceDbType, this.sourceDbConnectionInfo);

                            statistic.Subscribe(this);

                            tableColumnContentMaxLengths = await statistic.GetTableColumnContentLengths(new SchemaInfoFilter() { TableNames= schemaInfo.Tables.Select(item=>item.Name).ToArray() });
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

                            result = await this.dbConverter.Convert(schemaInfo, null, translatedSchemaInfo);
                        }
                    }

                    this.SetExecuteButtonEnabled(true);

                    if (result.InfoType == DbConvertResultInfoType.Information)
                    {
                        if (this.dbConverter != null)
                        {
                            if (!this.dbConverter.CancelRequested)
                            {
                                this.txtMessage.AppendText(Environment.NewLine + DONE);

                                MessageBox.Show(result.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Task has been canceled.");
                            }
                        }
                    }
                    else if (result.InfoType == DbConvertResultInfoType.Warnning)
                    {
                        MessageBox.Show(result.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (result.InfoType == DbConvertResultInfoType.Error)
                    {
                        if (result.ExceptionType != typeof(OperationCanceledException))
                        {
                            MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            iscancelled = true;
                        }
                    }
                }
            }
            catch (TaskCanceledException tce)
            {
                iscancelled = true;
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
            finally
            {
                this.dbConverter = null;

                GC.Collect();
            }

            if (iscancelled)
            {
                MessageBox.Show("Operation has been cancelled.");
            }
        }

        private void SetExecuteButtonEnabled(bool enable)
        {
            this.btnExecute.Enabled = enable;
            this.btnCancel.Enabled = !enable;
        }

        private void HandleException(Exception ex)
        {
            string errMsg = ExceptionHelper.GetExceptionDetails(ex);

            LogHelper.LogError(errMsg);

            this.AppendMessage(errMsg, true);

            this.txtMessage.SelectionStart = this.txtMessage.TextLength;
            this.txtMessage.ScrollToCaret();

            this.btnExecute.Enabled = true;
            this.btnCancel.Enabled = false;

            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Feedback(FeedbackInfo info)
        {
            Action action = () =>
            {
                if (info.InfoType == FeedbackInfoType.Error)
                {
                    if (!info.IgnoreError)
                    {
                        if (this.chkExecuteOnTarget.Checked && !this.chkContinueWhenErrorOccurs.Checked)
                        {
                            if (this.dbConverter != null && this.dbConverter.IsBusy)
                            {
                                this.dbConverter.Cancle();
                            }

                            this.SetExecuteButtonEnabled(true);
                        }
                    }

                    this.AppendMessage(info.Message, true);
                }
                else
                {
                    this.AppendMessage(info, false);
                }
            };

            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(action);
                }
                else
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ExceptionHelper.GetExceptionDetails(ex));
            }
        }

        private void AppendMessage(FeedbackInfo info, bool isError = false)
        {
            if (this.dbConverter != null && info.InfoType == FeedbackInfoType.Info && info.IsReportProgress)
            {
                string prefix = string.Format(this.dbConverter.TableDataSyncProgressMessagePrefixFormat, info.ObjectName);

                var lines = this.txtMessage.Lines;

                int lineIndex = -1;

                for (int i = lines.Length - 1; i >= 0; i--)
                {
                    string line = lines[i];

                    if (line.StartsWith(prefix) && line.Contains("%"))
                    {
                        lineIndex = i;
                        break;
                    }
                }

                if (lineIndex >= 0)
                {
                    int length = lines[lineIndex].Length;
                    int start = this.txtMessage.GetFirstCharIndexFromLine(lineIndex);

                    this.txtMessage.Select(start, length);
                    this.txtMessage.SelectedText = info.Message;
                }
                else
                {
                    this.AppendMessage(info.Message, isError);
                }
            }
            else
            {
                this.AppendMessage(info.Message, isError);
            }
        }

        private void AppendMessage(string message, bool isError = false)
        {
            RichTextBoxHelper.AppendMessage(this.txtMessage, message, isError);
        }

        private bool ConfirmCancel()
        {
            if (MessageBox.Show("Are you sure to abandon current task?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                return true;
            }
            return false;
        }

        private async void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.dbConverter != null && this.dbConverter.IsBusy)
            {
                if (this.ConfirmCancel())
                {
                    await this.cancellationTokenSource.CancelAsync();

                    if (this.dbConverter != null)
                    {
                        this.dbConverter.Cancle();
                    }

                    this.SetExecuteButtonEnabled(true);
                }
            }
        }

        #region IObserver<FeedbackInfo>
        void IObserver<FeedbackInfo>.OnCompleted()
        {
        }
        void IObserver<FeedbackInfo>.OnError(Exception error)
        {
        }
        void IObserver<FeedbackInfo>.OnNext(FeedbackInfo info)
        {
            this.Feedback(info);
        }
        #endregion    

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.dbConverter != null && this.dbConverter.IsBusy)
            {
                if (this.ConfirmCancel())
                {
                    this.dbConverter.Cancle();
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void btnCopyMessage_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtMessage.Text))
            {
                Clipboard.SetDataObject(this.txtMessage.Text);
                MessageBox.Show("The message has been copied to clipboard.");
            }
            else
            {
                MessageBox.Show("There's no message.");
            }
        }

        private void btnSaveMessage_Click(object sender, EventArgs e)
        {
            if (this.dlgSaveLog == null)
            {
                this.dlgSaveLog = new SaveFileDialog();
            }

            if (!string.IsNullOrEmpty(this.txtMessage.Text))
            {
                this.dlgSaveLog.Filter = "txt files|*.txt|all files|*.*";
                DialogResult dialogResult = this.dlgSaveLog.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    File.WriteAllLines(this.dlgSaveLog.FileName, this.txtMessage.Text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                    this.dlgSaveLog.Reset();
                }
            }
            else
            {
                MessageBox.Show("There's no message.");
            }
        }

        private void btnOutputFolder_Click(object sender, EventArgs e)
        {
            if (this.dlgOutputFolder == null)
            {
                this.dlgOutputFolder = new FolderBrowserDialog();
            }

            DialogResult result = this.dlgOutputFolder.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.txtOutputFolder.Text = this.dlgOutputFolder.SelectedPath;
            }
        }

        private void sourceDbProfile_ProfileSelectedChanged(object sender, ConnectionInfo connectionInfo)
        {
            this.sourceDbConnectionInfo = connectionInfo;

            if (useSourceConnector)
            {
                this.SetControlsStatus();
            }
        }

        private void targetDbProfile_ProfileSelectedChanged(object sender, ConnectionInfo connectionInfo)
        {
            this.targetDbConnectionInfo = connectionInfo;

            this.SetControlsStatus();
            this.SetControlStateByMode();
        }

        private void SetControlsStatus()
        {
            bool enable = false;

            DatabaseType targetDatabaseType = DatabaseType.Unknown;

            if (this.targetDbProfile.IsDbTypeSelected())
            {
                targetDatabaseType = this.targetDbProfile.DatabaseType;

                enable = !(targetDatabaseType == DatabaseType.Oracle || targetDatabaseType == DatabaseType.MySql
                     || this.sourceDatabaseType == DatabaseType.Oracle || this.sourceDatabaseType == DatabaseType.MySql);

                if (this.targetDbConnectionInfo != null)
                {
                    var targetDbInterpreter = DbInterpreterHelper.GetDbInterpreter(targetDatabaseType, this.targetDbConnectionInfo, new DbInterpreterOption());

                    this.chkNcharToDoubleChar.Enabled = !targetDbInterpreter.SupportNchar;
                }
            }
            else
            {
                this.chkNcharToDoubleChar.Enabled = false;
            }

            this.btnSetSchemaMappings.Enabled = enable;

            if (!enable)
            {
                this.chkCreateSchemaIfNotExists.Enabled = false;
                this.chkCreateSchemaIfNotExists.Checked = false;
            }
            else
            {
                this.chkCreateSchemaIfNotExists.Enabled = true;
            }

            if (!this.chkNcharToDoubleChar.Enabled)
            {
                this.chkNcharToDoubleChar.Checked = true;
            }

            this.schemaMappings = new List<SchemaMappingInfo>();
        }

        private void txtMessage_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (this.txtMessage.SelectionLength > 0)
                {
                    this.contextMenuStrip1.Show(this.txtMessage, e.Location);
                }
            }
        }

        private void tsmiCopySelection_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(this.txtMessage.SelectedText);
        }

        private void chkComputeColumn_CheckedChanged(object sender, EventArgs e)
        {
            this.chkOnlyCommentComputeExpression.Enabled = !this.chkComputeColumn.Checked;

            if (this.chkComputeColumn.Checked)
            {
                this.chkOnlyCommentComputeExpression.Checked = false;
            }
        }

        private async void btnSetSchemaMappings_Click(object sender, EventArgs e)
        {
            DatabaseType sourceDbType = this.useSourceConnector ? this.sourceDbProfile.DatabaseType : this.sourceDatabaseType;
            DatabaseType targetDbType = this.targetDbProfile.DatabaseType;

            if (sourceDbType == DatabaseType.Unknown || targetDbType == DatabaseType.Unknown)
            {
                return;
            }

            if (this.sourceDbConnectionInfo == null || this.targetDbConnectionInfo == null)
            {
                return;
            }

            DbInterpreterOption option = new DbInterpreterOption() { };

            DbInterpreter sourceInterpreter = DbInterpreterHelper.GetDbInterpreter(sourceDbType, this.sourceDbConnectionInfo, option);
            DbInterpreter targetInterpreter = DbInterpreterHelper.GetDbInterpreter(targetDbType, this.targetDbConnectionInfo, option);

            List<DatabaseSchema> sourceSchemas = new List<DatabaseSchema>();
            List<DatabaseSchema> targetSchemas = new List<DatabaseSchema>();

            try
            {
                sourceSchemas = await sourceInterpreter.GetDatabaseSchemasAsync();
                targetSchemas = await targetInterpreter.GetDatabaseSchemasAsync();
            }
            catch (Exception ex)
            {
            }

            frmSchemaMapping form = new frmSchemaMapping()
            {
                Mappings = this.schemaMappings,
                SourceSchemas = sourceSchemas.Select(item => item.Name).ToList(),
                TargetSchemas = targetSchemas.Select(item => item.Name).ToList()
            };

            if (form.ShowDialog() == DialogResult.OK)
            {
                this.schemaMappings = form.Mappings;
            }
        }

        private void chkOutputScripts_CheckedChanged(object sender, EventArgs e)
        {
            string defaultOutputFolder = SettingManager.Setting.ScriptsDefaultOutputFolder;

            if (this.chkOutputScripts.Checked && string.IsNullOrEmpty(this.txtOutputFolder.Text) && !string.IsNullOrEmpty(defaultOutputFolder))
            {
                this.txtOutputFolder.Text = defaultOutputFolder;
            }
        }

        private void cboMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SetControlStateByMode();

            this.SetNeedPreviewControlStatus();
        }

        private void SetControlStateByMode()
        {
            if (this.configCheckboxes == null)
            {
                return;
            }

            GenerateScriptMode mode = this.GetGenerateScriptMode();

            bool schemaOnly = mode == GenerateScriptMode.Schema;
            bool dataOnly = mode == GenerateScriptMode.Data;

            var schemaOnlyCheckboxes = this.configCheckboxes.Where(item => item.Tag?.ToString() == "Schema");
            var dataOnlyCheckboxes = this.configCheckboxes.Where(item => item.Tag?.ToString() == "Data");

            foreach (var checkbox in schemaOnlyCheckboxes)
            {
                checkbox.Enabled = !dataOnly;
            }

            foreach (var checkbox in dataOnlyCheckboxes)
            {
                checkbox.Enabled = !schemaOnly;
            }

            if (schemaOnly || dataOnly)
            {
                this.UncheckIfNotEnable();
            }
            else
            {
                this.chkCreateSchemaIfNotExists.Checked = true;
                this.chkGenerateCheckConstraint.Checked = true;
                this.chkGenerateIdentity.Checked = true;
                this.chkGenerateComment.Checked = true;
                this.chkComputeColumn.Checked = true;             
            }

            bool supportBulkCopy = true;

            if (this.targetDbProfile.DatabaseType != DatabaseType.Unknown)
            {
                DbInterpreter targetInterpreter = DbInterpreterHelper.GetDbInterpreter(this.targetDbProfile.DatabaseType, this.targetDbConnectionInfo??new ConnectionInfo(), new DbInterpreterOption());

                if (targetInterpreter != null)
                {
                    supportBulkCopy = targetInterpreter.SupportBulkCopy;
                }
            }

            this.chkBulkCopy.Enabled = !schemaOnly && supportBulkCopy;
            this.chkBulkCopy.Checked = !schemaOnly && supportBulkCopy;
            this.chkUseTransaction.Checked = true;            

            this.SetControlsStatus();
        }

        private void UncheckIfNotEnable()
        {
            foreach (var checkbox in this.configCheckboxes)
            {
                if (!checkbox.Enabled)
                {
                    checkbox.Checked = false;
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

        private void SetNeedPreviewControlStatus()
        {
            bool enabled = (this.targetDbProfile.DatabaseType != this.sourceDbProfile.DatabaseType) && (this.GetGenerateScriptMode() != GenerateScriptMode.Data);

            this.chkNeedPreview.Enabled = enabled;

            if(!enabled)
            {
                this.chkNeedPreview.Checked = false;
            }
        }

        private void sourceDbProfile_DatabaseTypeSelectedChanged(object sender, EventArgs e)
        {
            this.SetNeedPreviewControlStatus();

            this.ShowDataTypeMappingConfigFiles();            
        }

        private void targetDbProfile_DatabaseTypeSelectedChanged(object sender, EventArgs e)
        {
            this.SetNeedPreviewControlStatus();

            this.ShowDataTypeMappingConfigFiles();
        }

        private void ShowDataTypeMappingConfigFiles()
        {
            this.cboDataTypeMappingFile.Items.Clear();
            this.lblDataTypeMappingFileType.Text = "";

            if ((this.sourceDbProfile.Visible && this.sourceDbProfile.IsDbTypeSelected() || this.sourceDatabaseType != DatabaseType.Unknown) && this.targetDbProfile.IsDbTypeSelected())
            {
                DatabaseType sourceDatabaseType = this.sourceDatabaseType == DatabaseType.Unknown ? this.sourceDbProfile.DatabaseType : this.sourceDatabaseType;
                DatabaseType targetDatabaseType = this.targetDbProfile.DatabaseType;

                List<ConvertConfigFileInfo> files = new List<ConvertConfigFileInfo>();

                if (sourceDatabaseType != targetDatabaseType)
                {
                    string defaultDataTypeMappingFilePath = DataTypeMappingManager.GetDataTypeMappingFilePath(sourceDatabaseType, targetDatabaseType);

                    if (File.Exists(defaultDataTypeMappingFilePath))
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

        private void lblDataTypeMappingFileType_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var selectedItem = this.cboDataTypeMappingFile.SelectedItem;

            if (selectedItem != null)
            {
                ConvertConfigFileInfo configFileInfo = selectedItem as ConvertConfigFileInfo;

                DatabaseType sourceDatabaseType = this.sourceDatabaseType == DatabaseType.Unknown ? this.sourceDbProfile.DatabaseType : this.sourceDatabaseType;
                DatabaseType targetDatabaseType = this.targetDbProfile.DatabaseType;

                frmDataTypeMappingSetting form = new frmDataTypeMappingSetting(true, sourceDatabaseType, targetDatabaseType, configFileInfo.IsDefault, configFileInfo.Name);

                form.ShowDialog();
            }
        }
    }
}
