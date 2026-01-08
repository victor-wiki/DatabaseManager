using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core.Model;
using System;
using System.CodeDom.Compiler;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeGenerator = DatabaseManager.Core.CodeGenerator;

namespace DatabaseManager
{
    public partial class frmCodeGenerator : Form, IObserver<FeedbackInfo>
    {
        private ConnectionInfo connectionInfo;
        private CancellationTokenSource cancellationTokenSource;

        public frmCodeGenerator()
        {
            InitializeComponent();

            this.Init();
        }

        private void Init()
        {
            var languages = Enum.GetNames(typeof(ProgrammingLanguage)).Where(item => item != nameof(ProgrammingLanguage.None)).ToArray();

            this.cboLanguage.Items.AddRange(languages);

            this.cboLanguage.SelectedIndex = 0;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.Invoke(new Action(this.Connect));
        }

        private async void Connect()
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

            if (!this.dbConnectionProfile.ValidateProfile())
            {
                return;
            }

            var dbType = this.dbConnectionProfile.DatabaseType;

            this.btnConnect.Text = "...";

            try
            {
                await this.tvDbObjects.LoadTree(dbType, this.connectionInfo, DatabaseObjectType.Table | DatabaseObjectType.View);
            }
            catch (Exception ex)
            {
                this.tvDbObjects.ClearNodes();

                string message = ExceptionHelper.GetExceptionDetails(ex);

                LogHelper.LogError(message);

                MessageBox.Show("Error:" + message);
            }

            this.btnConnect.Text = "Connect";
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

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            string outputFolder = this.txtOutputFolder.Text;

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                MessageBox.Show("Output folder is required.");

                return;
            }

            SchemaInfo schemaInfo = this.tvDbObjects.GetSchemaInfo();

            if (!this.Validate(schemaInfo))
            {
                return;
            }

            Task.Run(async () => { this.Generate(schemaInfo); });
        }

        private async void Generate(SchemaInfo schemaInfo)
        {
            string outputFolder = this.txtOutputFolder.Text;

            CodeGenerateOption option = new CodeGenerateOption();
            option.OutputFolder = this.txtOutputFolder.Text.Trim();
            option.Language = (ProgrammingLanguage)Enum.Parse(typeof(ProgrammingLanguage), this.cboLanguage.Text);
            option.Namespace = this.txtNamespance.Text.Trim();
            option.Tables = schemaInfo.Tables;
            option.Views = schemaInfo.Views;

            this.cancellationTokenSource = new CancellationTokenSource();

            var token = this.cancellationTokenSource.Token;

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.dbConnectionProfile.DatabaseType, this.connectionInfo);

            CodeGenerator codeGenerator = new CodeGenerator(dbInterpreter, option);

            codeGenerator.Subscribe(this);

            CodeGenerateResult result = await codeGenerator.Generate(token);

            if (!token.IsCancellationRequested)
            {
                if (result.IsOK)
                {
                    MessageBox.Show("Generated successfully.");
                }
                else
                {
                    MessageBox.Show("Generated failed.");
                }
            }
            else
            {
                MessageBox.Show("Task has been canceled.");
            }
        }

        private bool Validate(SchemaInfo schemaInfo)
        {
            if (!this.tvDbObjects.HasDbObjectNodeSelected())
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

        private async void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.cancellationTokenSource != null)
            {
                await this.cancellationTokenSource.CancelAsync();
            }
        }

        private void dbConnectionProfile_ProfileSelectedChanged(object sender, DatabaseInterpreter.Model.ConnectionInfo connectionInfo)
        {
            this.connectionInfo = connectionInfo;
        }

        private void Feedback(string message)
        {
            this.Feedback(new FeedbackInfo() { Message = message });
        }

        private void Feedback(FeedbackInfo info)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    this.txtMessage.Text = info.Message;

                    if (info.InfoType == FeedbackInfoType.Error)
                    {
                        this.txtMessage.ForeColor = Color.Red;
                    }
                    else
                    {
                        this.txtMessage.ForeColor = Color.Black;
                    }
                }));
            }
            catch (Exception ex)
            {
            }
        }

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            string languate = this.cboLanguage.Text;

            switch(languate)
            {
                case nameof(ProgrammingLanguage.CSharp):
                    this.lblNamespace.Text = "Namespace:";
                    break;
                case nameof(ProgrammingLanguage.Java):
                    this.lblNamespace.Text = "Package:";
                    break;
                default:
                    this.lblNamespace.Text = "Namespace:";
                    break;
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
    }
}
