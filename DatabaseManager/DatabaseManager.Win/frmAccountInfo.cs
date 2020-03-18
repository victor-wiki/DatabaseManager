using DatabaseInterpreter.Model;
using DatabaseInterpreter.Profile;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmAccountInfo : Form
    {        
        private bool requriePassword = false;
        public DatabaseType DatabaseType { get; set; }
        public Guid AccountProfileId { get; set; }
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

            if(this.AccountProfileInfo!=null)
            {
                this.ucAccountInfo.LoadData(this.AccountProfileInfo);
            }
        }

        private void frmAccountInfo_Activated(object sender, EventArgs e)
        {
            if(this.requriePassword)
            {
                this.ucAccountInfo.FocusPasswordTextbox();
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if(!this.ucAccountInfo.ValidateInfo())
            {
                return;
            }

            AccountProfileInfo accountProfileInfo = this.GetAccountProfileInfo();

            this.AccountProfileId = AccountProfileManager.Save(accountProfileInfo, this.ucAccountInfo.RememberPassword);

            this.AccountProfileInfo = accountProfileInfo;           

            this.DialogResult = DialogResult.OK;

            this.Close();
        }

        private AccountProfileInfo GetAccountProfileInfo()
        {
            ConnectionInfo connectionInfo = this.ucAccountInfo.GetConnectionInfo();
            AccountProfileInfo accountProfileInfo = new AccountProfileInfo() {  DatabaseType = this.DatabaseType.ToString() };

            ObjectHelper.CopyProperties(connectionInfo, accountProfileInfo);

            return accountProfileInfo;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            this.ucAccountInfo.TestConnect();
        }
    }
}
