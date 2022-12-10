using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Data;
using DatabaseManager.Profile;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmAccountInfo : Form
    {
        private bool requriePassword = false;
        public DatabaseType DatabaseType { get; set; }
        public string AccountProfileId { get; set; }
        public AccountProfileInfo AccountProfileInfo { get; set; }

        public frmAccountInfo(DatabaseType dbType)
        {
            InitializeComponent();

            this.DatabaseType = dbType;
        }

        public frmAccountInfo(DatabaseType dbType, bool requriePassword)
        {
            InitializeComponent();

            this.DatabaseType = dbType;
            this.requriePassword = requriePassword;
        }

        private void frmAccountInfo_Load(object sender, EventArgs e)
        {
            this.Init();
        }

        private void Init()
        {
            this.ucAccountInfo.DatabaseType = this.DatabaseType;
            this.ucAccountInfo.InitControls();

            if (this.AccountProfileInfo != null)
            {
                this.ucAccountInfo.LoadData(this.AccountProfileInfo);
            }
        }

        private void frmAccountInfo_Activated(object sender, EventArgs e)
        {
            if (this.requriePassword)
            {
                this.ucAccountInfo.FocusPasswordTextbox();
            }
        }

        private async void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!this.ucAccountInfo.ValidateInfo())
            {
                return;
            }

            AccountProfileInfo accountProfileInfo = this.GetAccountProfileInfo();

            var profiles = await AccountProfileManager.GetProfiles(this.DatabaseType.ToString());

            bool isAdd = this.AccountProfileInfo == null;

            if (isAdd)
            {
                if (profiles.Any(item => item.Server == accountProfileInfo.Server
                                                     && item.IntegratedSecurity == accountProfileInfo.IntegratedSecurity
                                                     && item.UserId == accountProfileInfo.UserId
                                                     && item.Port == accountProfileInfo.Port))
                {
                    MessageBox.Show($"The record has already existed:{accountProfileInfo.Description}");
                    return;
                }
            }
            else
            {
                if (profiles.Where(item => item.Id != this.AccountProfileInfo.Id).Any(item => item.Server == accountProfileInfo.Server
                                                       && item.IntegratedSecurity == accountProfileInfo.IntegratedSecurity
                                                       && item.UserId == accountProfileInfo.UserId
                                                       && item.Port == accountProfileInfo.Port))
                {
                    MessageBox.Show($"The record has already existed:{accountProfileInfo.Description}");
                    return;
                }
            }

            this.AccountProfileId = await AccountProfileManager.Save(accountProfileInfo, this.ucAccountInfo.RememberPassword);

            this.AccountProfileInfo = accountProfileInfo;

            this.DialogResult = DialogResult.OK;

            if(SettingManager.Setting.RememberPasswordDuringSession)
            {
                DataStore.SetAccountProfileInfo(accountProfileInfo);
            }           

            this.Close();
        }

        private AccountProfileInfo GetAccountProfileInfo()
        {
            ConnectionInfo connectionInfo = this.ucAccountInfo.GetConnectionInfo();           

            AccountProfileInfo accountProfileInfo = new AccountProfileInfo() { DatabaseType = this.DatabaseType.ToString() };

            ObjectHelper.CopyProperties(connectionInfo, accountProfileInfo);

            if (this.AccountProfileInfo != null)
            {
                accountProfileInfo.Id = this.AccountProfileInfo.Id;
            }

            return accountProfileInfo;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
