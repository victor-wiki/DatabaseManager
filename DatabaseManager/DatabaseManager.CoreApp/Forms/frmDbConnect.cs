using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Data;
using DatabaseManager.Profile.Manager;
using DatabaseManager.Profile.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmDbConnect : Form
    {
        private bool requriePassword = false;
        private bool isAdd = true;
        private bool isPopulating = false;

        public DatabaseType DatabaseType { get; set; }
        public string ProfileId { get; set; }     
        public bool IsOnlyForSelectDatabase { get; set; }

        public ConnectionInfo ConnectionInfo { get; set; }

        public frmDbConnect(DatabaseType dbType)
        {
            InitializeComponent();

            this.isAdd = true;
            this.DatabaseType = dbType;
        }

        public frmDbConnect(DatabaseType dbType, string profileId, bool requriePassword = false, bool cannotEditAccountInfo = false)
        {
            InitializeComponent();

            this.isAdd = false;
            this.requriePassword = requriePassword;
            this.DatabaseType = dbType;
            this.ProfileId = profileId;

            if(cannotEditAccountInfo)
            {
                this.panelMode.Visible = false;
                this.ucDbAccountInfo.Enabled = false;
                this.panelContent.Top = this.panelMode.Top;
                this.panelButton.Top -= this.panelMode.Height;

                this.Height -= this.panelMode.Height;
            }
        }

        private void frmDbConnect_Load(object sender, EventArgs e)
        {
            this.Init();
        }

        private void Init()
        {
            this.ucDbAccountInfo.OnTestConnect += this.TestConnect;
            this.ucDbAccountInfo.DatabaseType = this.DatabaseType;
            this.ucDbAccountInfo.InitControls();

            if (!this.IsOnlyForSelectDatabase)
            {
                if (!string.IsNullOrEmpty(this.ProfileId))
                {
                    this.LoadProfile();
                }                
            }
            else
            {
                this.lblProfileName.Visible = false;
                this.txtProfileName.Visible = false;
            }
        }

        private async void LoadProfile()
        {
            var profile = await ConnectionProfileManager.GetProfileById(this.ProfileId);

            ConnectionInfo connectionInfo = ConnectionProfileManager.GetConnectionInfoByProfileInfo(profile);

            this.ucDbAccountInfo.LoadData(connectionInfo, this.ConnectionInfo?.Password);

            this.cboDatabase.Text = connectionInfo.Database;
            this.txtProfileName.Text = profile.Name;
        }

        private void TestConnect()
        {
            this.isPopulating = true;
            this.PopulateDatabases();
        }

        private async void PopulateDatabases()
        {
            ConnectionInfo connectionInfo = this.GetConnectionInfo();
            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, connectionInfo, new DbInterpreterOption());

            string oldDatabase = this.cboDatabase.Text;

            try
            {
                this.cboDatabase.Items.Clear();

                List<Database> databaseses = await dbInterpreter.GetDatabasesAsync();

                databaseses.ForEach(item =>
                {
                    this.cboDatabase.Items.Add(item.Name);
                });

                this.cboDatabase.Text = oldDatabase;

                this.cboDatabase.DroppedDown = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed:" + ex.Message);
            }

            this.isPopulating = false;
        }

        public ConnectionInfo GetConnectionInfo()
        {
            ConnectionInfo connectionInfo = this.ucDbAccountInfo.GetConnectionInfo();
            connectionInfo.Database = this.cboDatabase.Text;

            return connectionInfo;
        }

        private async void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!this.ucDbAccountInfo.ValidateInfo())
            {
                return;
            }

            if (string.IsNullOrEmpty(this.cboDatabase.Text))
            {
                MessageBox.Show("Please select a database.");
                return;
            }

            string database = this.cboDatabase.Text;

            this.ConnectionInfo = this.GetConnectionInfo();

            if (!this.IsOnlyForSelectDatabase)
            {
                string profileName = this.txtProfileName.Text.Trim();

                if (string.IsNullOrEmpty(database))
                {
                    MessageBox.Show("Database can not be empty.");
                    return;
                }

                if (string.IsNullOrEmpty(profileName))
                {
                    MessageBox.Show("Profile name can not be empty.");
                    return;
                }

                string accountProfileId = null;

                if (this.isAdd)
                {
                    AccountProfileInfo accountProfile = await AccountProfileManager.GetProfile(this.DatabaseType.ToString(), this.ConnectionInfo.Server, this.ConnectionInfo.Port, this.ConnectionInfo.IntegratedSecurity, this.ConnectionInfo.UserId);

                    if (accountProfile != null)
                    {
                        accountProfileId = accountProfile.Id;
                    }
                }
                else
                {
                    ConnectionProfileInfo connectionProfileInfo = await ConnectionProfileManager.GetProfileById(this.ProfileId);

                    if (connectionProfileInfo != null)
                    {
                        accountProfileId = connectionProfileInfo.AccountId;
                    }
                }

                bool isNameExisted = await ConnectionProfileManager.IsNameExisted(this.isAdd, accountProfileId, profileName, this.ProfileId);

                if (isNameExisted)
                {
                    string msg = $"The profile name \"{profileName}\" has been existed";

                    MessageBox.Show(msg);

                    return;
                }

                ConnectionProfileInfo profile = new ConnectionProfileInfo()
                {
                    Id = this.ProfileId,
                    AccountId = accountProfileId,
                    DatabaseType = this.DatabaseType.ToString(),
                    Server = this.ConnectionInfo.Server,
                    Port = this.ConnectionInfo.Port,
                    ServerVersion = this.ConnectionInfo.ServerVersion,
                    Database = this.ConnectionInfo.Database,
                    IntegratedSecurity = this.ConnectionInfo.IntegratedSecurity,
                    UserId = this.ConnectionInfo.UserId,
                    Password = this.ConnectionInfo.Password,
                    IsDba = this.ConnectionInfo.IsDba,
                    UseSsl = this.ConnectionInfo.UseSsl
                };

                if (!string.IsNullOrEmpty(accountProfileId))
                {
                    profile.AccountId = accountProfileId;
                }

                profile.Name = profileName;
                profile.DatabaseType = this.DatabaseType.ToString();

                string profileId = await ConnectionProfileManager.Save(profile, this.ucDbAccountInfo.RememberPassword);

                if(!string.IsNullOrEmpty(profileId))
                {
                    if (SettingManager.Setting.RememberPasswordDuringSession)
                    {
                        if (!this.ConnectionInfo.IntegratedSecurity && !this.ucDbAccountInfo.RememberPassword && !string.IsNullOrEmpty(this.ConnectionInfo.Password))
                        {
                            AccountProfileInfo accountProfileInfo = new AccountProfileInfo() { Id = profile.AccountId };

                            ObjectHelper.CopyProperties(this.ConnectionInfo, accountProfileInfo);
                            accountProfileInfo.Password = this.ConnectionInfo.Password;

                            DataStore.SetAccountProfileInfo(accountProfileInfo);
                        }
                    }

                    this.ProfileId = profileId;                  
                }                
            }

            this.DialogResult = DialogResult.OK;
        }

        private void frmDbConnect_Activated(object sender, EventArgs e)
        {
            if (this.requriePassword)
            {
                this.ucDbAccountInfo.FocusPasswordTextbox();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cboDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtProfileName.Text) && !string.IsNullOrEmpty(this.cboDatabase.Text))
            {
                this.txtProfileName.Text = this.cboDatabase.Text;
            }
        }

        private void rbChoose_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rbChoose.Checked)
            {
                frmDbConnectionManage frm = new frmDbConnectionManage(this.DatabaseType) { IsForSelecting = true };

                if (frm.ShowDialog() == DialogResult.OK)
                {
                    string password = null;

                    if (SettingManager.Setting.RememberPasswordDuringSession)
                    {
                        var storeInfo = DataStore.GetAccountProfileInfo(frm.SelectedAccountProfileInfo.Id);

                        if (storeInfo != null && !frm.SelectedAccountProfileInfo.IntegratedSecurity && !string.IsNullOrEmpty(storeInfo.Password))
                        {
                            password = storeInfo.Password;
                        }
                    }

                    this.ucDbAccountInfo.LoadData(frm.SelectedAccountProfileInfo, password);
                }
            }
        }


        private void cboDatabase_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.cboDatabase.Items.Count == 0 && !this.isPopulating)
            {
                this.PopulateDatabases();
            }
        }
    }
}
