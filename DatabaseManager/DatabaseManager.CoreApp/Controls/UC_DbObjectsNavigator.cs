using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseManager.Helper;
using DatabaseManager.Profile;
using DatabaseInterpreter.Utility;
using DatabaseManager.Model;
using DatabaseInterpreter.Core;
using DatabaseManager.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Data;
using DatabaseManager.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_DbObjectsNavigator : UserControl
    {
        public ShowDbObjectContentHandler OnShowContent;
        public FeedbackHandler OnFeedback;

        public DatabaseType DatabaseType
        {
            get
            {
                return ManagerUtil.GetDatabaseType(this.cboDbType.Text);
            }
        }

        public UC_DbObjectsNavigator()
        {
            InitializeComponent();
        }

        private void UC_DbObjectsNavigator_Load(object sender, EventArgs e)
        {
            this.InitControls();

            this.tvDbObjects.OnFeedback += this.Feedback;
        }

        private void InitControls()
        {
            this.tvDbObjects.OnShowContent += this.ShowContent;
            this.LoadDbTypes();
        }

        private void ShowContent(DatabaseObjectDisplayInfo content)
        {
            if (this.OnShowContent != null)
            {
                this.OnShowContent(content);
            }
        }

        private void Feedback(FeedbackInfo info)
        {
            if (this.OnFeedback != null)
            {
                this.OnFeedback(info);
            }
        }

        public void LoadDbTypes()
        {
            var databaseTypes = DbInterpreterHelper.GetDisplayDatabaseTypes();
            foreach (var value in databaseTypes)
            {
                this.cboDbType.Items.Add(value.ToString());
            }

            if (this.cboDbType.Items.Count > 0)
            {
                this.cboDbType.Text = SettingManager.Setting.PreferredDatabase.ToString();

                if (string.IsNullOrEmpty(this.cboDbType.Text))
                {
                    this.cboDbType.SelectedIndex = 0;
                }
            }

            this.btnConnect.Focus();
        }

        private void cboDbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasValue = this.cboDbType.SelectedIndex >= 0;
            this.btnAddAccount.Enabled = hasValue;

            if (hasValue)
            {
                DatabaseType databaseType = ManagerUtil.GetDatabaseType(this.cboDbType.Text);

                if(!ManagerUtil.IsFileConnection(databaseType))
                {
                    this.LoadAccounts();
                }
                else
                {
                    this.LoadFileConnections();
                }                
            }
        }

        private async void LoadAccounts(string defaultValue = null)
        {
            string type = this.cboDbType.Text;

            var profiles = (await AccountProfileManager.GetProfiles(type)).OrderBy(item => item.Description);

            this.cboAccount.DataSource = profiles.ToList();
            this.cboAccount.DisplayMember = nameof(AccountProfileInfo.Description);
            this.cboAccount.ValueMember = nameof(AccountProfileInfo.Id);

            List<string> ids = profiles.Select(item => item.Id).ToList();

            if (string.IsNullOrEmpty(defaultValue))
            {
                if (profiles.Count() > 0)
                {
                    this.cboAccount.SelectedIndex = 0;
                }
            }
            else
            {
                if (ids.Contains(defaultValue))
                {
                    this.cboAccount.Text = profiles.FirstOrDefault(item => item.Id == defaultValue)?.Description;
                }
            }

            btnConnect.Enabled = this.cboAccount.Items.Count > 0;
        }

        private async void LoadFileConnections(string defaultValue = null)
        {
            string type = this.cboDbType.Text;

            var profiles = (await FileConnectionProfileManager.GetProfiles(type)).OrderBy(item => item.Description);

            this.cboAccount.DataSource = profiles.ToList();
            this.cboAccount.DisplayMember = nameof(FileConnectionProfileInfo.Description);
            this.cboAccount.ValueMember = nameof(FileConnectionProfileInfo.Id);

            List<string> ids = profiles.Select(item => item.Id).ToList();

            if (string.IsNullOrEmpty(defaultValue))
            {
                if (profiles.Count() > 0)
                {
                    this.cboAccount.SelectedIndex = 0;
                }
            }
            else
            {
                if (ids.Contains(defaultValue))
                {
                    this.cboAccount.Text = profiles.FirstOrDefault(item => item.Id == defaultValue)?.Description;
                }
            }

            btnConnect.Enabled = this.cboAccount.Items.Count > 0;
        }

        private void btnAddAccount_Click(object sender, EventArgs e)
        {
            string databaseType = this.cboDbType.Text;

            if (string.IsNullOrEmpty(databaseType))
            {
                MessageBox.Show("Please select a database type first.");
            }
            else
            {
                DatabaseType dbType = ManagerUtil.GetDatabaseType(databaseType);

                if(!ManagerUtil.IsFileConnection(dbType))
                {
                    frmAccountInfo form = new frmAccountInfo(dbType);
                    DialogResult result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        this.LoadAccounts(form.AccountProfileId);

                        if (this.cboAccount.SelectedItem != null)
                        {
                            (this.cboAccount.SelectedItem as AccountProfileInfo).Password = form.AccountProfileInfo.Password;
                        }
                    }
                }
                else
                {
                    frmFileConnection form = new frmFileConnection(dbType);

                    DialogResult result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        this.LoadFileConnections(form.FileConnectionProfileId);

                        if (this.cboAccount.SelectedItem != null)
                        {
                            (this.cboAccount.SelectedItem as FileConnectionProfileInfo).Password = form.FileConnectionProfileInfo.Password;
                        }
                    }
                }
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.Invoke(new Action(this.Connect));
        }

        private async void Connect()
        {
            object selectedItem = this.cboAccount.SelectedItem;

            ConnectionInfo connectionInfo = new ConnectionInfo();

            AccountProfileInfo accountProfileInfo=null;
            FileConnectionProfileInfo fileConnectionProfileInfo = null;

            if (selectedItem is AccountProfileInfo)
            {
                accountProfileInfo= selectedItem as AccountProfileInfo;

                if (!accountProfileInfo.IntegratedSecurity && string.IsNullOrEmpty(accountProfileInfo.Password))
                {
                    var storedInfo = DataStore.GetAccountProfileInfo(accountProfileInfo.Id);

                    if (storedInfo != null && !string.IsNullOrEmpty(storedInfo.Password))
                    {
                        accountProfileInfo.Password = storedInfo.Password;
                    }
                    else
                    {
                        MessageBox.Show("Please specify password for the database.");

                        if (!this.SetConnectionInfo(accountProfileInfo))
                        {
                            return;
                        }
                    }
                }

                ObjectHelper.CopyProperties(accountProfileInfo, connectionInfo);
            }
            else if(selectedItem is FileConnectionProfileInfo)
            {
                fileConnectionProfileInfo= selectedItem as FileConnectionProfileInfo;

                if (fileConnectionProfileInfo.HasPassword && string.IsNullOrEmpty(fileConnectionProfileInfo.Password))
                {
                    var storedInfo = DataStore.GetFileConnectionProfileInfo(fileConnectionProfileInfo.Id);

                    if (storedInfo != null && !string.IsNullOrEmpty(storedInfo.Password))
                    {
                        fileConnectionProfileInfo.Password = storedInfo.Password;
                    }
                    else
                    {
                        MessageBox.Show("Please specify password for the database.");

                        if (!this.SetFileConnectionInfo(fileConnectionProfileInfo))
                        {
                            return;
                        }
                    }
                }

                ObjectHelper.CopyProperties(fileConnectionProfileInfo, connectionInfo);
            }            

            this.btnConnect.Enabled = false;

            try
            {
                await this.tvDbObjects.LoadTree(this.DatabaseType, connectionInfo);

                if (SettingManager.Setting.RememberPasswordDuringSession)
                {
                    if (accountProfileInfo != null)
                    {
                        DataStore.SetAccountProfileInfo(accountProfileInfo);
                    }
                    else if (fileConnectionProfileInfo != null)
                    {
                        DataStore.SetFileConnectionProfileInfo(fileConnectionProfileInfo);
                    }
                }               
            }
            catch (Exception ex)
            {
                this.tvDbObjects.ClearNodes();

                string message = ExceptionHelper.GetExceptionDetails(ex);

                LogHelper.LogError(message);

                MessageBox.Show("Error:" + message);

                if (accountProfileInfo!=null && !this.SetConnectionInfo(accountProfileInfo))
                {
                    return;
                }
                else if(fileConnectionProfileInfo!=null && !this.SetFileConnectionInfo(fileConnectionProfileInfo))
                {
                    return;
                }
                else
                {
                    this.Connect();
                }
            }

            this.btnConnect.Enabled = true;
        }

        private bool SetConnectionInfo(AccountProfileInfo accountProfileInfo)
        {
            DatabaseType dbType = ManagerUtil.GetDatabaseType(this.cboDbType.Text);

            frmAccountInfo frmAccountInfo = new frmAccountInfo(dbType, true) { AccountProfileInfo = accountProfileInfo };

            DialogResult dialogResult = frmAccountInfo.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                AccountProfileInfo profileInfo = frmAccountInfo.AccountProfileInfo;

                ObjectHelper.CopyProperties(profileInfo, (this.cboAccount.SelectedItem as AccountProfileInfo));
                this.cboAccount.Text = profileInfo.Description;                

                return true;
            }
            else
            {
                this.btnConnect.Enabled = true;
            }

            return false;
        }

        private bool SetFileConnectionInfo(FileConnectionProfileInfo fileConnectionProfileInfo)
        {
            DatabaseType dbType = ManagerUtil.GetDatabaseType(this.cboDbType.Text);

            frmFileConnection form = new frmFileConnection(dbType, true) { FileConnectionProfileInfo = fileConnectionProfileInfo };

            DialogResult dialogResult = form.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                FileConnectionProfileInfo profileInfo = form.FileConnectionProfileInfo;

                ObjectHelper.CopyProperties(profileInfo, (this.cboAccount.SelectedItem as FileConnectionProfileInfo));
                this.cboAccount.Text = profileInfo.Description;

                return true;
            }
            else
            {
                this.btnConnect.Enabled = true;
            }

            return false;
        }

        public ConnectionInfo GetCurrentConnectionInfo()
        {
            return this.tvDbObjects.GetCurrentConnectionInfo();
        }

        public DatabaseObjectDisplayInfo GetDisplayInfo()
        {
            DatabaseObjectDisplayInfo info = this.tvDbObjects.GetDisplayInfo();

            info.DatabaseType = this.DatabaseType;

            return info;
        }

        private void UC_DbObjectsNavigator_SizeChanged(object sender, EventArgs e)
        {
            this.cboDbType.Refresh();
            this.cboAccount.Refresh();
        }
    }
}
