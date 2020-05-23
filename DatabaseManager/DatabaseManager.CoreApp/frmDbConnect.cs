using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Profile;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Windows.Forms;

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

        private void LoadProfile()
        {
            ConnectionInfo connectionInfo = ConnectionProfileManager.GetConnectionInfo(this.DatabaseType.ToString(), this.ProflieName);          

            this.ucDbAccountInfo.LoadData(connectionInfo, this.ConnectionInfo?.Password);

            this.cboDatabase.Text = connectionInfo.Database;
        }

        private async void TestConnect()
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

        private void btnConfirm_Click(object sender, EventArgs e)
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
                IEnumerable<ConnectionProfileInfo> profiles = ConnectionProfileManager.GetProfiles(this.DatabaseType.ToString());

                Guid? oldAccountProfileId = null;

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
                        oldAccountProfileId = profiles.FirstOrDefault(item => item.Name == profileName).AccountProfileId;
                    }
                }

                ConnectionProfileInfo profile = new ConnectionProfileInfo() { ConnectionInfo = this.ConnectionInfo };

                if (oldAccountProfileId.HasValue)
                {
                    profile.AccountProfileId = oldAccountProfileId.Value;
                }

                profile.Name = profileName;
                profile.DatabaseType = this.DatabaseType.ToString();

                this.ProflieName = ConnectionProfileManager.Save(profile, this.ucDbAccountInfo.RememberPassword);
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
                    this.ucDbAccountInfo.LoadData(frm.SelectedAccountProfileInfo);
                }
            }
        }
    }
}
