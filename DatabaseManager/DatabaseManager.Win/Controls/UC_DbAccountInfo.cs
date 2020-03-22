using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Profile;
using DatabaseManager.Core;
using DatabaseManager.Model;
using System;
using System.Data.Common;
using System.Linq;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_DbAccountInfo : UserControl
    {
        public DatabaseType DatabaseType { get; set; }

        public bool RememberPassword => this.chkRememberPassword.Checked;

        public UC_DbAccountInfo()
        {
            InitializeComponent();
        }       

        public void InitControls()
        {
            if (this.DatabaseType == DatabaseType.MySql)
            {
                this.lblPort.Visible = this.txtPort.Visible = true;
                this.txtPort.Text = "3306";
            }

            var authTypes = Enum.GetNames(typeof(AuthenticationType));
            this.cboAuthentication.Items.AddRange(authTypes);

            if (this.DatabaseType != DatabaseType.SqlServer)
            {
                this.cboAuthentication.Text = AuthenticationType.Password.ToString();
                //this.cboAuthentication.Enabled = false;
            }

            this.chkAsDba.Visible = this.DatabaseType == DatabaseType.Oracle;

            var profiles = AccountProfileManager.GetProfiles(this.DatabaseType.ToString());
            var serverNames = profiles.Select(item => item.Server).Distinct().OrderBy(item=>item).ToArray();
            this.cboServer.Items.AddRange(serverNames);
        }

        public void LoadData(DatabaseAccountInfo info)
        {
            this.cboServer.Text = info.Server;
            this.txtPort.Text = info.Port;
            this.cboAuthentication.Text = info.IntegratedSecurity ? AuthenticationType.IntegratedSecurity.ToString() : AuthenticationType.Password.ToString();
            this.txtUserId.Text = info.UserId;
            this.txtPassword.Text = info.Password;
            this.chkAsDba.Checked = info.IsDba;

            if (info.IntegratedSecurity)
            {
                this.cboAuthentication.Text = AuthenticationType.IntegratedSecurity.ToString();
            }
            else
            {
                if (!string.IsNullOrEmpty(info.Password))
                {
                    this.chkRememberPassword.Checked = true;
                }
            }
        }

        public bool ValidateInfo()
        {
            if(string.IsNullOrEmpty(this.cboServer.Text))
            {
                MessageBox.Show("Server name can't be empty.");
                return false;
            }

            if(string.IsNullOrEmpty(this.cboAuthentication.Text))
            {
                MessageBox.Show("Please select a authentication type.");
                return false;
            }
            else if(this.cboAuthentication.Text == AuthenticationType.Password.ToString())
            {
                if(string.IsNullOrEmpty(this.txtUserId.Text))
                {
                    MessageBox.Show("User name can't be empty.");
                    return false;
                }
                else if(string.IsNullOrEmpty(this.txtPassword.Text))
                {
                    MessageBox.Show("Password can't be empty.");
                    return false;
                }               
            }

            return true;
        }

        public bool TestConnect()
        {
            if(!this.ValidateInfo())
            {
                return false;
            }

            ConnectionInfo connectionInfo = this.GetConnectionInfo();

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, connectionInfo, new DbInterpreterOption());           

            try
            {
                using (DbConnection dbConnection = dbInterpreter.CreateConnection())
                {
                    dbConnection.Open();

                    MessageBox.Show("Success.");

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
            return new ConnectionInfo()
            {
                Server = this.cboServer.Text.Trim(),
                Port = this.txtPort.Text.Trim(),
                IntegratedSecurity = this.cboAuthentication.Text != AuthenticationType.Password.ToString(),
                UserId = this.txtUserId.Text.Trim(),
                Password = this.txtPassword.Text.Trim(),
                IsDba = this.chkAsDba.Checked
            };
        }

        private void cboAuthentication_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isWindowsAuth = this.cboAuthentication.Text == AuthenticationType.IntegratedSecurity.ToString();

            this.txtUserId.Enabled = !isWindowsAuth;
            this.txtPassword.Enabled = !isWindowsAuth;
            this.chkRememberPassword.Enabled = !isWindowsAuth;

            if (!isWindowsAuth)
            {
                this.txtUserId.Text = this.txtPassword.Text = "";
                this.chkRememberPassword.Checked = false;
            }
        }

        public void FocusPasswordTextbox()
        {
            this.txtPassword.Focus();
        }
    }
}
