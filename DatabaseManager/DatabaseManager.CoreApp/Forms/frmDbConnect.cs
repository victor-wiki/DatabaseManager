using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Profile;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Windows.Forms;
using DatabaseManager.Data;
using System.Threading.Tasks;

namespace DatabaseManager
{
    public partial class frmDbConnect : Form
    {
        private bool requriePassword = false;
        private bool isAdd = true;

        public DatabaseType DatabaseType { get; set; }
        public string ProflieName { get; set; }
        public bool NotUseProfile { get; set; }

        public ConnectionInfo ConnectionInfo { get; set; }

        public frmDbConnect(DatabaseType dbType)
        {
            InitializeComponent();
            this.isAdd = true;
            this.DatabaseType = dbType;
        }

        public frmDbConnect(DatabaseType dbType, string profileName, bool requriePassword = false)
        {
            InitializeComponent();

            this.isAdd = false;
            this.requriePassword = requriePassword;
            this.DatabaseType = dbType;
            this.ProflieName = profileName;
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

            if (!this.NotUseProfile)
            {
                if (string.IsNullOrEmpty(this.ProflieName))
                {
                    this.txtProfileName.Text = "";
                }
                else
                {
                    this.txtProfileName.Text = this.ProflieName.ToString();
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
            ConnectionInfo connectionInfo = await ConnectionProfileManager.GetConnectionInfo(this.DatabaseType.ToString(), this.ProflieName);

            this.ucDbAccountInfo.LoadData(connectionInfo, this.ConnectionInfo?.Password);

            this.cboDatabase.Text = connectionInfo.Database;
        }

        private void TestConnect()
        {
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

            string profileName = this.txtProfileName.Text.Trim();
            string database = this.cboDatabase.Text;

            this.ConnectionInfo = this.GetConnectionInfo();

            if (!this.NotUseProfile)
            {
                IEnumerable<ConnectionProfileInfo> profiles = await ConnectionProfileManager.GetProfiles(this.DatabaseType.ToString());

                string oldAccountProfileId = null;

                if (!string.IsNullOrEmpty(profileName) && profiles.Any(item => item.Name == profileName))
                {
                    string msg = $"The profile name \"{profileName}\" has been existed";

                    if (this.isAdd)
                    {
                        DialogResult dialogResult = MessageBox.Show(msg + ", are you sure to override it.", "Confirm", MessageBoxButtons.YesNo);

                        if (dialogResult != DialogResult.Yes)
                        {
                            this.DialogResult = DialogResult.None;
                            return;
                        }
                    }
                    else if (!this.isAdd && this.ProflieName != profileName)
                    {
                        MessageBox.Show(msg + ", please edit that.");
                        return;
                    }
                    else //edit
                    {
                        oldAccountProfileId = profiles.FirstOrDefault(item => item.Name == profileName).AccountId;
                    }
                }
                else
                {
                    AccountProfileInfo accountProfile = await AccountProfileManager.GetProfile(this.DatabaseType.ToString(), this.ConnectionInfo.Server, this.ConnectionInfo.Port, this.ConnectionInfo.IntegratedSecurity, this.ConnectionInfo.UserId);

                    if (accountProfile != null)
                    {
                        oldAccountProfileId = accountProfile.Id;
                    }
                }

                ConnectionProfileInfo profile = new ConnectionProfileInfo()
                {
                    AccountId = oldAccountProfileId,
                    DatabaseType = this.DatabaseType.ToString(),
                    Server = this.ConnectionInfo.Server,
                    Port = this.ConnectionInfo.Port,
                    Database = this.ConnectionInfo.Database,
                    IntegratedSecurity = this.InvokeRequired,
                    UserId = this.ConnectionInfo.UserId,
                    Password =this.ConnectionInfo.Password,
                    IsDba = this.ConnectionInfo.IsDba,
                    UseSsl = this.ConnectionInfo.UseSsl
                };

                if (!string.IsNullOrEmpty(oldAccountProfileId))
                {
                    profile.AccountId = oldAccountProfileId;
                }

                profile.Name = profileName;
                profile.DatabaseType = this.DatabaseType.ToString();

                this.ProflieName = await ConnectionProfileManager.Save(profile, this.ucDbAccountInfo.RememberPassword);

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
            if (this.cboDatabase.Items.Count == 0)
            {
                this.PopulateDatabases();
            }
        }
    }
}
