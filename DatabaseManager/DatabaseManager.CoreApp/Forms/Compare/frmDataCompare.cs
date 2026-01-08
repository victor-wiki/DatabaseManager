using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Core.Model;
using DatabaseManager.Forms.Compare;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmDataCompare : Form, IObserver<FeedbackInfo>
    {
        private DatabaseType sourceDatabaseType;
        private ConnectionInfo sourceDbConnectionInfo;
        private ConnectionInfo targetDbConnectionInfo;
        private bool useSourceConnector = true;
        private DbInterpreter sourceInterpreter;
        private DbInterpreter targetInterpreter;
        private SchemaInfo sourceSchemaInfo;
        private SchemaInfo targetSchemaInfo;
        private DataCompare dataCompare;
        private CancellationTokenSource cancellationTokenSource;

        public frmDataCompare()
        {
            InitializeComponent();

            this.InitControls();
        }

        public frmDataCompare(DatabaseType sourceDatabaseType, ConnectionInfo sourceConnectionInfo)
        {
            InitializeComponent();

            this.sourceDatabaseType = sourceDatabaseType;
            this.sourceDbConnectionInfo = sourceConnectionInfo;
            this.useSourceConnector = false;

            this.InitControls();
        }

        private void frmDataCompare_Load(object sender, EventArgs e)
        {
            if (!this.useSourceConnector)
            {
                this.targetDbProfile.DatabaseType = sourceDatabaseType;
                this.targetDbProfile.EnableDatabaseType = false;
            }
        }

        private void InitControls()
        {
            if (!this.useSourceConnector)
            {
                int increaseHeight = this.sourceDbProfile.Height;

                this.btnFetch.Height = this.targetDbProfile.ClientHeight;            
                this.sourceDbProfile.Visible = false;
                this.btnCompare.Height = this.targetDbProfile.ClientHeight;
                this.targetDbProfile.Top -= increaseHeight;
                this.btnFetch.Top = this.targetDbProfile.Top + 1 ;
            }
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            if (this.useSourceConnector)
            {
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

            SchemaInfo schemaInfo = this.tvDbObjects.GetSchemaInfo();

            if (!this.ValidateSource(schemaInfo))
            {
                return;
            }

            if (this.GetDisplayMode() == DataCompareDisplayMode.None)
            {
                MessageBox.Show("Please choose at least one item to display.");
                return;
            }            

            Task.Run(() => { this.Compare(); });
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

        private async void Compare()
        {
            DatabaseType dbType;

            if (this.useSourceConnector)
            {
                dbType = this.sourceDbProfile.DatabaseType;
            }
            else
            {
                dbType = this.sourceDatabaseType;
            }

            if (this.targetDbConnectionInfo == null)
            {
                MessageBox.Show("Target connection info is null.");
                return;
            }

            if (dbType != this.targetDbProfile.DatabaseType)
            {
                MessageBox.Show("Target database type must be same as source database type.");
                return;
            }

            if (this.sourceDbConnectionInfo.Server == this.targetDbConnectionInfo.Server
                && this.sourceDbConnectionInfo.Port == this.targetDbConnectionInfo.Port
                && this.sourceDbConnectionInfo.Database == this.targetDbConnectionInfo.Database)
            {
                MessageBox.Show("Source database cannot be equal to the target database.");
                return;
            }

            if (!this.targetDbProfile.ValidateProfile())
            {
                return;
            }

            this.btnCompare.Text = "...";
            this.btnCompare.Enabled = false;
            this.btnCancel.Enabled = true;

            try
            {
                SchemaInfo schemaInfoFromTree = this.tvDbObjects.GetSchemaInfo();

                this.sourceInterpreter = DbInterpreterHelper.GetDbInterpreter(dbType, this.sourceDbConnectionInfo);
                this.targetInterpreter = DbInterpreterHelper.GetDbInterpreter(this.targetDbProfile.DatabaseType, this.targetDbConnectionInfo, new DbInterpreterOption());

                this.sourceInterpreter.Subscribe(this);
                this.targetInterpreter.Subscribe(this);

                DataCompareOption option = new DataCompareOption();

                option.DisplayMode = this.GetDisplayMode();

                this.dataCompare = new DataCompare(this.sourceInterpreter, this.targetInterpreter, schemaInfoFromTree, option);

                dataCompare.Subscribe(this);

                this.Feedback("Begin to compare...");

                this.cancellationTokenSource = new CancellationTokenSource();

                var token = this.cancellationTokenSource.Token;              

                DataCompareResult result = await dataCompare.Compare(token);        
                
                if(!token.IsCancellationRequested)
                {
                    var details = result.Details;

                    if (details.Count > 0)
                    {
                        this.Invoke(() =>
                        {
                            frmDataCompareResult frm = new frmDataCompareResult(this.sourceInterpreter, this.targetInterpreter, result, option);
                            frm.ShowDialog();
                        });
                    }
                    else
                    {
                        MessageBox.Show("No records to diaplay.");
                    }

                    this.Feedback("End compare.");
                }
                else
                {
                    this.Feedback("Task has been canceled.");
                }
            }
            catch (Exception ex)
            {
                string message = ExceptionHelper.GetExceptionDetails(ex);

                LogHelper.LogError(message);

                MessageBox.Show("Error:" + message);
            }
            finally
            {
                this.btnCompare.Text = "Compare";
                this.btnCompare.Enabled = true;
                this.btnCancel.Enabled = false;
            }
        }

        private DataCompareDisplayMode GetDisplayMode()
        {
            DataCompareDisplayMode displayMode = DataCompareDisplayMode.None;

            if (this.chkDifferentRecords.Checked)
            {
                displayMode |= DataCompareDisplayMode.Different;
            }

            if (this.chkOnlyInSourceRecords.Checked)
            {
                displayMode |= DataCompareDisplayMode.OnlyInSource;
            }

            if (this.chkOnlyInSourceRecords.Checked)
            {
                displayMode |= DataCompareDisplayMode.OnlyInTarget;
            }

            if (this.chkIdenticalRecords.Checked)
            {
                displayMode |= DataCompareDisplayMode.Indentical;
            }

            return displayMode;
        }

        private async void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.cancellationTokenSource != null)
            {
                await this.cancellationTokenSource.CancelAsync();               
            }

            this.btnCancel.Enabled = false;
            this.btnCompare.Enabled = true;
            this.btnCompare.Text = "Compare";
        }

        private void sourceDbProfile_DatabaseTypeSelectedChanged(object sender, EventArgs e)
        {
            DatabaseType sourceDbType = this.sourceDbProfile.DatabaseType;

            if (sourceDbType != DatabaseType.Unknown)
            {
                DatabaseType targetDbType = this.targetDbProfile.DatabaseType;

                if (targetDbType != sourceDbType)
                {
                    this.targetDbProfile.DatabaseType = sourceDbType;
                }
            }
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
                           this.toolTip1.SetToolTip(this.txtMessage, info.Message);
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
                await this.tvDbObjects.LoadTree(dbType, this.sourceDbConnectionInfo, DatabaseObjectType.Table);

                this.tvDbObjects.CheckRootNodes();

                this.btnCompare.Enabled = true;
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

        private void sourceDbProfile_ProfileSelectedChanged(object sender, ConnectionInfo connectionInfo)
        {
            this.sourceDbConnectionInfo = connectionInfo;
        }

        private void targetDbProfile_ProfileSelectedChanged(object sender, ConnectionInfo connectionInfo)
        {
            this.targetDbConnectionInfo = connectionInfo;
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
