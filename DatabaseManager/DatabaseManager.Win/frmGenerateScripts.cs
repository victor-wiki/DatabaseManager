using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmGenerateScripts : Form, IObserver<FeedbackInfo>
    {
        private const string DONE = "Done";
        private DatabaseType databaseType;
        private ConnectionInfo connectionInfo;
        private bool isBusy = false;
        private DbInterpreter dbInterpreter;
        private bool useConnector = true;

        public frmGenerateScripts()
        {
            InitializeComponent();
        }

        public frmGenerateScripts(DatabaseType databaseType, ConnectionInfo connectionInfo)
        {
            InitializeComponent();
            this.databaseType = databaseType;
            this.connectionInfo = connectionInfo;

            this.useConnector = false;
        }

        private void frmGenerateScripts_Load(object sender, EventArgs e)
        {
            this.Init();
        }

        private void Init()
        {
            if (!this.useConnector && this.connectionInfo != null)
            {
                this.panelConnector.Visible = false;
                this.tvDbObjects.Top -= this.panelConnector.Height;
                this.tvDbObjects.Height += this.panelConnector.Height;

                this.Connect();
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

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.Invoke(new Action(this.Connect));
        }

        private async void Connect()
        {
            DatabaseType dbType;

            if (this.useConnector)
            {
                if (!this.dbConnectionProfile.IsDbTypeSelected())
                {
                    MessageBox.Show("Please select a database type.");
                    return;
                }

                if (!this.dbConnectionProfile.IsProfileSelected())
                {
                    MessageBox.Show("Please select a database profile.");
                    return;
                }

                if (!this.connectionInfo.IntegratedSecurity && string.IsNullOrEmpty(this.connectionInfo.Password))
                {
                    MessageBox.Show("Please specify password for the database.");
                    this.dbConnectionProfile.ConfigConnection(true);
                    return;
                }

                dbType = this.dbConnectionProfile.DatabaseType;
            }
            else
            {
                dbType = this.databaseType;
            }

            this.btnConnect.Text = "...";

            try
            {
                await this.tvDbObjects.LoadTree(dbType, this.connectionInfo);
            }
            catch (Exception ex)
            {
                this.tvDbObjects.ClearNodes();

                string message = ExceptionHelper.GetExceptionDetails(ex);

                LogHelper.LogInfo(message);

                MessageBox.Show("Error:" + message);
            }

            this.btnConnect.Text = "Connect";
        }

        private void dbConnectionProfile_OnSelectedChanged(object sender, ConnectionInfo connectionInfo)
        {
            this.connectionInfo = connectionInfo;
        }

        private async void btnGenerate_Click(object sender, EventArgs e)
        {
            await Task.Run(() => this.GenerateScripts());
        }

        private async void GenerateScripts()
        {
            SchemaInfo schemaInfo = this.tvDbObjects.GetSchemaInfo();

            if (!this.Validate(schemaInfo))
            {
                return;
            }

            this.isBusy = true;
            this.btnGenerate.Enabled = false;

            DatabaseType sourceDbType = this.useConnector ? this.dbConnectionProfile.DatabaseType : this.databaseType;

            DbInterpreterOption option = new DbInterpreterOption()
            {
                ScriptOutputMode = GenerateScriptOutputMode.WriteToFile,
                SortTablesByKeyReference = true,
                GetTableAllObjects = true
            };

            this.SetGenerateScriptOption(option);

            GenerateScriptMode scriptMode = this.GetGenerateScriptMode();

            if (scriptMode == GenerateScriptMode.None)
            {
                MessageBox.Show("Please specify the script mode.");
                return;
            }

            this.dbInterpreter = DbInterpreterHelper.GetDbInterpreter(sourceDbType, this.connectionInfo, option);

            SelectionInfo selectionInfo = new SelectionInfo()
            {
                UserDefinedTypeNames = schemaInfo.UserDefinedTypes.Select(item => item.Name).ToArray(),
                TableNames = schemaInfo.Tables.Select(item => item.Name).ToArray(),
                ViewNames = schemaInfo.Views.Select(item => item.Name).ToArray()
            };

            try
            {
                schemaInfo = await this.dbInterpreter.GetSchemaInfoAsync(selectionInfo);

                this.dbInterpreter.Subscribe(this);

                GenerateScriptMode mode = GenerateScriptMode.None;

                if (scriptMode.HasFlag(GenerateScriptMode.Schema))
                {
                    mode = GenerateScriptMode.Schema;
                    this.dbInterpreter.GenerateSchemaScripts(schemaInfo);
                }

                if (scriptMode.HasFlag(GenerateScriptMode.Data))
                {
                    mode = GenerateScriptMode.Data;
                    await dbInterpreter.GenerateDataScriptsAsync(schemaInfo);
                }

                this.isBusy = false;
                ManagerUtil.OpenInExplorer(dbInterpreter.GetScriptOutputFilePath(mode));

                MessageBox.Show(DONE);
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }

            this.btnGenerate.Enabled = true;
        }

        private void HandleException(Exception ex)
        {
            this.isBusy = false;

            string errMsg = ExceptionHelper.GetExceptionDetails(ex);

            LogHelper.LogInfo(errMsg);

            this.AppendMessage(errMsg, true);

            this.txtMessage.SelectionStart = this.txtMessage.TextLength;
            this.txtMessage.ScrollToCaret();

            this.btnGenerate.Enabled = true;

            MessageBox.Show(ex.Message);
        }

        private bool Validate(SchemaInfo schemaInfo)
        {
            if (schemaInfo.UserDefinedTypes.Count == 0 && schemaInfo.Tables.Count == 0 && schemaInfo.Views.Count == 0)
            {
                MessageBox.Show("Please select objects from tree.");
                return false;
            }

            if (this.connectionInfo == null)
            {
                MessageBox.Show("Connection is null.");
                return false;
            }

            return true;
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

        private void Feedback(FeedbackInfo info)
        {
            this.Invoke(new Action(() =>
            {
                if (info.InfoType == FeedbackInfoType.Error)
                {
                    this.AppendMessage(info.Message, true);
                }
                else
                {
                    this.AppendMessage(info.Message, false);
                }
            }));
        }

        private void AppendMessage(string message, bool isError = false)
        {
            UiHelper.AppendMessage(this.txtMessage, message, isError);
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

        private void frmGenerateScripts_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.isBusy)
            {
                if (this.ConfirmCancel())
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private bool ConfirmCancel()
        {
            if (MessageBox.Show("Are you sure to abandon current task?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (this.dbInterpreter != null)
                {
                    this.dbInterpreter.CancelRequested = true;
                }

                return true;
            }
            return false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (this.isBusy)
            {
                if (!this.ConfirmCancel())
                {
                    return;
                }
            }

            this.Close();
        }
    }
}
