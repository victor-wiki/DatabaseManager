using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Profile;
using DatabaseManager.Core;
using DatabaseManager.Model;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseManager.Data;

namespace DatabaseManager.Controls
{
    public delegate void TestDbConnectHandler();

    public partial class UC_DbAccountInfo : UserControl
    {
        private string serverVersion;

        public DatabaseType DatabaseType { get; set; }

        public bool RememberPassword => this.chkRememberPassword.Checked;

        public TestDbConnectHandler OnTestConnect;         

        public UC_DbAccountInfo()
        {
            InitializeComponent();
        }

        public async void InitControls()
        {
            if (this.DatabaseType == DatabaseType.MySql)
            {
                this.lblPort.Visible = this.txtPort.Visible = true;
                this.txtPort.Text = MySqlInterpreter.DEFAULT_PORT.ToString();
                this.chkUseSsl.Visible = true;
            }
            else if (this.DatabaseType == DatabaseType.Oracle)
            {
                this.lblPort.Visible = this.txtPort.Visible = true;
                this.txtPort.Text = OracleInterpreter.DEFAULT_PORT.ToString();
            }
            else if (this.DatabaseType == DatabaseType.Postgres)
            {
                this.lblPort.Visible = this.txtPort.Visible = true;
                this.txtPort.Text = PostgresInterpreter.DEFAULT_PORT.ToString();
            }

            var authTypes = Enum.GetNames(typeof(AuthenticationType));
            this.cboAuthentication.Items.AddRange(authTypes);

            if (this.DatabaseType != DatabaseType.SqlServer)
            {
                this.cboAuthentication.Text = AuthenticationType.Password.ToString();
                //this.cboAuthentication.Enabled = false;
            }
            else
            {
                this.cboAuthentication.Text = AuthenticationType.IntegratedSecurity.ToString();
            }

            this.chkAsDba.Visible = this.DatabaseType == DatabaseType.Oracle;

            var profiles = await AccountProfileManager.GetProfiles(this.DatabaseType.ToString());
            var serverNames = profiles.Select(item => item.Server).Distinct().OrderBy(item => item).ToArray();
            this.cboServer.Items.AddRange(serverNames);
        }

        public void LoadData(DatabaseAccountInfo info, string password = null)
        {
            this.cboServer.Text = info.Server;
            this.txtPort.Text = info.Port;
            this.cboAuthentication.Text = info.IntegratedSecurity ? AuthenticationType.IntegratedSecurity.ToString() : AuthenticationType.Password.ToString();
            this.txtUserId.Text = info.UserId;
            this.txtPassword.Text = info.Password;
            this.chkAsDba.Checked = info.IsDba;
            this.chkUseSsl.Checked = info.UseSsl;
            this.serverVersion = info.ServerVersion;

            if (info.IntegratedSecurity)
            {
                this.cboAuthentication.Text = AuthenticationType.IntegratedSecurity.ToString();
            }
            else
            {
                if(!string.IsNullOrEmpty(password))
                {
                    this.txtPassword.Text = password;
                }  
                
                if(!string.IsNullOrEmpty(info.Password))
                {
                    this.chkRememberPassword.Checked = true;
                }
            }
        }

        public bool ValidateInfo()
        {
            if (string.IsNullOrEmpty(this.cboServer.Text))
            {
                MessageBox.Show("Server name can't be empty.");
                return false;
            }

            if (string.IsNullOrEmpty(this.cboAuthentication.Text))
            {
                MessageBox.Show("Please select a authentication type.");
                return false;
            }
            else if (this.cboAuthentication.Text == AuthenticationType.Password.ToString())
            {
                if (string.IsNullOrEmpty(this.txtUserId.Text))
                {
                    MessageBox.Show("User name can't be empty.");
                    return false;
                }
                else if (string.IsNullOrEmpty(this.txtPassword.Text))
                {
                    MessageBox.Show("Password can't be empty.");
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> TestConnect()
        {
            if (!this.ValidateInfo())
            {
                return false;
            }

            ConnectionInfo connectionInfo = this.GetConnectionInfo();

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, connectionInfo, new DbInterpreterOption());

            try
            {
                using (DbConnection dbConnection = dbInterpreter.CreateConnection())
                {
                    await dbConnection.OpenAsync();

                    this.serverVersion = dbConnection.ServerVersion;

                    MessageBox.Show("Success.");

                    if (this.OnTestConnect != null)
                    {
                        this.OnTestConnect();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed:" + ex.Message);
                return false;
            }
        }

        public ConnectionInfo GetConnectionInfo()
        {
            ConnectionInfo connectionInfo = new ConnectionInfo()
            {
                Server = this.cboServer.Text.Trim(),
                Port = this.txtPort.Text.Trim(),
                IntegratedSecurity = this.cboAuthentication.Text != AuthenticationType.Password.ToString(),
                UserId = this.txtUserId.Text.Trim(),
                Password = this.txtPassword.Text.Trim(),
                IsDba = this.chkAsDba.Checked,
                UseSsl = this.chkUseSsl.Checked                
            };

            if(!string.IsNullOrEmpty(this.serverVersion))
            {
                connectionInfo.ServerVersion = this.serverVersion;
            }

            return connectionInfo;
        }

        private void cboAuthentication_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isWindowsAuth = this.cboAuthentication.Text == AuthenticationType.IntegratedSecurity.ToString();

            this.txtUserId.Enabled = !isWindowsAuth;
            this.txtPassword.Enabled = !isWindowsAuth;
            this.chkRememberPassword.Enabled = !isWindowsAuth;

            this.chkRememberPassword.Checked = false;
            this.txtUserId.Text = this.txtPassword.Text = "";
        }

        public void FocusPasswordTextbox()
        {
            this.txtPassword.Focus();
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            await this.TestConnect();
        }
    }
}
