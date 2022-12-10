using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Model;
using DatabaseManager.Profile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_FileConnectionInfo : UserControl
    {
        private string databaseVersion;

        public DatabaseType DatabaseType { get; set; }

        public bool HasPassword => this.chkHasPassword.Checked;
        public bool RememberPassword => this.chkRememberPassword.Checked;

        public TestDbConnectHandler OnTestConnect;
        public EventHandler OnFileSelect;

        public UC_FileConnectionInfo()
        {
            InitializeComponent();
        }

        public void LoadData(FileConnectionProfileInfo info, string password = null)
        {
            this.txtFilePath.Text = info.Database;           
            this.databaseVersion = info.DatabaseVersion;

            if (!string.IsNullOrEmpty(password))
            {
                this.chkHasPassword.Checked = true;
                this.chkRememberPassword.Checked = true;

                this.txtPassword.Text = password;
            }            
        }

        private void btnBrowserFile_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.FileName = "";

            DialogResult result = this.openFileDialog1.ShowDialog();

            if(result == DialogResult.OK)
            {
                this.txtFilePath.Text = this.openFileDialog1.FileName;

                if(this.OnFileSelect!=null)
                {
                    this.OnFileSelect(sender, e);
                }                
            }
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            await this.TestConnect();
        }

        public bool ValidateInfo()
        {
            if (string.IsNullOrEmpty(this.txtFilePath.Text))
            {
                MessageBox.Show("File path can't be empty.");
                return false;
            }
         
            if (this.chkHasPassword.Checked && string.IsNullOrEmpty(this.txtPassword.Text.Trim()))
            {
                MessageBox.Show("Password can't be empty.");
                return false;
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

                    this.databaseVersion = dbConnection.ServerVersion;

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
            ConnectionInfo connectionInfo = new ConnectionInfo();

            connectionInfo.Database = this.txtFilePath.Text.Trim();

            string password = this.txtPassword.Text.Trim();

            if(this.chkHasPassword.Checked && !string.IsNullOrEmpty(password))
            {
                connectionInfo.Password = password;
            }           

            if (!string.IsNullOrEmpty(this.databaseVersion))
            {
                connectionInfo.ServerVersion = this.databaseVersion;
            }

            return connectionInfo;
        }

        private void chkHasPassword_CheckedChanged(object sender, EventArgs e)
        {
            bool @checked = this.chkHasPassword.Checked;

            this.txtPassword.Enabled = @checked;
            this.chkRememberPassword.Enabled = @checked;

            if(!@checked)
            {
                this.txtPassword.Text = "";
                this.chkRememberPassword.Checked = false;
            }            
        }
    }
}
