using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Data;
using DatabaseManager.Forms;
using DatabaseManager.Helper;
using DatabaseManager.Profile.Manager;
using DatabaseManager.Profile.Model;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public delegate void ProfileSelectedChangeHandler(object sender, ConnectionInfo connectionInfo);
    public delegate void DatabaseTypeSelectedChangeHandler(object sender, EventArgs e);

    public partial class UC_DbConnectionProfile : UserControl
    {
        private ConnectionInfo connectionInfo;
        private bool isDataBinding = false;
        public ConnectionInfo ConnectionInfo => this.connectionInfo;

        public event ProfileSelectedChangeHandler ProfileSelectedChanged;
        public event DatabaseTypeSelectedChangeHandler DatabaseTypeSelectedChanged;

        [Category("Title")]
        public string Title
        {
            get
            {
                return this.lblTitle.Text;
            }
            set
            {
                this.lblTitle.Text = value;
            }
        }

        public int ClientHeight => this.btnAddDbProfile.Height;

        public DatabaseType DatabaseType
        {
            get { return ManagerUtil.GetDatabaseType(this.cboDbType.Text); }
            set { this.cboDbType.Text = value.ToString(); }
        }

        public bool EnableDatabaseType
        {
            get { return this.cboDbType.Enabled; }
            set { this.cboDbType.Enabled = value; }
        }

        public UC_DbConnectionProfile()
        {
            InitializeComponent();

            this.Init();
        }

        private void Init()
        {
            this.btnAddDbProfile.Image = IconImageHelper.GetImageByFontType(IconChar.Plus, IconFont.Solid);
        }

        private void UC_DbConnectionProfile_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            this.LoadDbTypes();
        }

        public void LoadDbTypes()
        {
            var databaseTypes = DbInterpreterHelper.GetDisplayDatabaseTypes();

            foreach (var value in databaseTypes)
            {
                this.cboDbType.Items.Add(value.ToString());
            }
        }

        public bool IsDbTypeSelected()
        {
            return !string.IsNullOrEmpty(this.cboDbType.Text);
        }

        public bool IsProfileSelected()
        {
            return !string.IsNullOrEmpty(this.cboDbProfile.Text);
        }

        private void cboDbProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.isDataBinding)
            {
                this.GetConnectionInfoByProfile();
            }
        }

        private async void GetConnectionInfoByProfile()
        {
            var selectedItem = this.cboDbProfile.SelectedItem;

            if (selectedItem == null)
            {
                return;
            }

            DatabaseType dbType = this.DatabaseType;

            if (!ManagerUtil.IsFileConnection(dbType))
            {
                string profileId = (selectedItem as ConnectionProfileInfo).Id;

                ConnectionInfo connectionInfo = await ConnectionProfileManager.GetProfileById(profileId);

                if (connectionInfo != null)
                {
                    this.SetConnectionPasswordFromDataStore(connectionInfo);

                    if (this.ProfileSelectedChanged != null)
                    {
                        this.ProfileSelectedChanged(this, connectionInfo);
                    }
                }
            }
            else
            {
                string id = (selectedItem as FileConnectionProfileInfo)?.Id;

                FileConnectionProfileInfo profile = await FileConnectionProfileManager.GetProfileById(id);

                if (profile != null)
                {
                    ConnectionInfo connectionInfo = new ConnectionInfo();

                    ObjectHelper.CopyProperties(profile, connectionInfo);

                    this.SetFileConnectionPasswordFromDataStore(connectionInfo);

                    if (this.ProfileSelectedChanged != null)
                    {
                        this.ProfileSelectedChanged(this, connectionInfo);
                    }
                }
            }
        }

        private async void LoadProfileNames(string defaultValue = null)
        {
            string dbType = this.cboDbType.Text;

            if (dbType != "")
            {
                IEnumerable<dynamic> profiles = null;
                List<string> names = null;

                string displayMember = null;
                string valueMember = null;

                if (!ManagerUtil.IsFileConnection(this.DatabaseType))
                {
                    profiles = await ConnectionProfileManager.GetProfiles(dbType);
                    names = profiles.Select(item => (item as ConnectionProfileInfo).Name).ToList();

                    displayMember = nameof(ConnectionProfileInfo.Description);
                    valueMember = nameof(ConnectionProfileInfo.Name);
                }
                else
                {
                    profiles = await FileConnectionProfileManager.GetProfiles(dbType);
                    names = profiles.Select(item => (item as FileConnectionProfileInfo).Name).ToList();

                    displayMember = nameof(FileConnectionProfileInfo.Description);
                    valueMember = nameof(FileConnectionProfileInfo.Name);
                }

                this.cboDbProfile.Items.Clear();

                this.isDataBinding = true;

                this.cboDbProfile.DisplayMember = displayMember;
                this.cboDbProfile.ValueMember = valueMember;

                foreach (dynamic profile in profiles)
                {
                    this.cboDbProfile.Items.Add(profile);
                }

                this.isDataBinding = false;

                if (string.IsNullOrEmpty(defaultValue))
                {
                    if (profiles.Count() > 0)
                    {
                        // this.cboDbProfile.SelectedIndex = 0;
                    }
                }
                else
                {
                    if (names.Contains(defaultValue))
                    {
                        this.cboDbProfile.Text = profiles.FirstOrDefault(item => item.Name == defaultValue)?.Description;
                    }
                }

                bool selected = this.cboDbProfile.Text.Length > 0;

                this.GetConnectionInfoByProfile();
            }
        }

        private void cboDbProfile_DrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox combobox = sender as ComboBox;
            if (combobox.DroppedDown)
            {
                e.DrawBackground();
            }

            e.DrawFocusRectangle();

            var items = combobox.Items;

            if (e.Index < 0)
            {
                e.Graphics.DrawString(combobox.Text, e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left, e.Bounds.Y);
            }
            else
            {
                if (items.Count > 0 && e.Index < items.Count)
                {
                    object obj = items[e.Index];

                    string descrition = null;

                    if (obj is ConnectionProfileInfo cpi)
                    {
                        descrition = cpi.Description;
                    }
                    else if (obj is FileConnectionProfileInfo fcpi)
                    {
                        descrition = fcpi.Description;
                    }

                    e.Graphics.DrawString(descrition, e.Font, new SolidBrush(combobox.DroppedDown ? e.ForeColor : Color.Black), e.Bounds.Left, e.Bounds.Y);
                }
            }
        }

        private void btnAddDbProfile_Click(object sender, EventArgs e)
        {
            this.AddConnection(true, this.cboDbType.Text);
        }

        private async void AddConnection(bool isSource, string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                MessageBox.Show("Please select database type.");
                return;
            }

            DatabaseType dbType = this.DatabaseType;

            if (!ManagerUtil.IsFileConnection(dbType))
            {
                frmDbConnect form = new frmDbConnect(dbType);

                if (this.SetConnectionInfo(form))
                {
                    string profileId = form.ProfileId;

                    ConnectionProfileInfo profile = await ConnectionProfileManager.GetProfileById(profileId);

                    if (profile != null)
                    {
                        this.LoadProfileNames(profile.Name);
                    }
                }
            }
            else
            {
                frmFileConnection form = new frmFileConnection(dbType) { ShowChooseControls = true };

                if (this.SetFileConnectionInfo(form))
                {
                    this.LoadProfileNames(form.FileConnectionProfileInfo.Name);
                }
            }
        }

        private bool SetConnectionInfo(frmDbConnect frmDbConnect)
        {
            DialogResult dialogResult = frmDbConnect.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                ConnectionInfo connectionInfo = frmDbConnect.ConnectionInfo;

                this.SetConnectionPasswordFromDataStore(connectionInfo);

                this.connectionInfo = connectionInfo;

                var profileInfo = this.cboDbProfile.SelectedItem as ConnectionProfileInfo;

                if (profileInfo != null)
                {
                    if (!profileInfo.IntegratedSecurity && string.IsNullOrEmpty(profileInfo.Password) && !string.IsNullOrEmpty(connectionInfo.Password))
                    {
                        profileInfo.Password = connectionInfo.Password;
                    }
                }

                if (this.ProfileSelectedChanged != null)
                {
                    this.ProfileSelectedChanged(this, connectionInfo);
                }

                return true;
            }

            return false;
        }

        private bool SetFileConnectionInfo(frmFileConnection frmFileConnect)
        {
            DialogResult dialogResult = frmFileConnect.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                ConnectionInfo connectionInfo = frmFileConnect.ConnectionInfo;

                this.SetFileConnectionPasswordFromDataStore(connectionInfo);

                this.connectionInfo = connectionInfo;

                var profileInfo = this.cboDbProfile.SelectedItem as FileConnectionProfileInfo;

                if (profileInfo != null)
                {
                    if (string.IsNullOrEmpty(profileInfo.Password) && !string.IsNullOrEmpty(connectionInfo.Password))
                    {
                        profileInfo.Password = connectionInfo.Password;
                    }
                }

                if (this.ProfileSelectedChanged != null)
                {
                    this.ProfileSelectedChanged(this, connectionInfo);
                }

                return true;
            }

            return false;
        }

        private void SetConnectionPasswordFromDataStore(ConnectionInfo connectionInfo)
        {
            if (!SettingManager.Setting.RememberPasswordDuringSession || connectionInfo.IntegratedSecurity)
            {
                return;
            }

            var profileInfo = this.cboDbProfile.SelectedItem as ConnectionProfileInfo;

            if (profileInfo != null)
            {
                if (string.IsNullOrEmpty(connectionInfo.Password))
                {
                    var accountProfile = DataStore.GetAccountProfileInfo(profileInfo.AccountId);

                    if (accountProfile != null && !accountProfile.IntegratedSecurity && !string.IsNullOrEmpty(accountProfile.Password))
                    {
                        connectionInfo.Password = accountProfile.Password;

                        if (string.IsNullOrEmpty(profileInfo.Password))
                        {
                            profileInfo.Password = accountProfile.Password;
                        }
                    }
                }
            }
        }

        private void SetFileConnectionPasswordFromDataStore(ConnectionInfo connectionInfo)
        {
            if (!SettingManager.Setting.RememberPasswordDuringSession)
            {
                return;
            }

            var profileInfo = this.cboDbProfile.SelectedItem as FileConnectionProfileInfo;

            if (profileInfo != null)
            {
                if (string.IsNullOrEmpty(connectionInfo.Password))
                {
                    var profile = DataStore.GetFileConnectionProfileInfo(profileInfo.Id);

                    if (profile != null && !string.IsNullOrEmpty(profile.Password))
                    {
                        connectionInfo.Password = profile.Password;

                        if (string.IsNullOrEmpty(profileInfo.Password))
                        {
                            profileInfo.Password = profile.Password;
                        }
                    }
                }
            }
        }

        public async void ConfigConnection(bool requriePassword = false)
        {
            string type = this.cboDbType.Text;
            object selectedItem = this.cboDbProfile.SelectedItem;

            if (string.IsNullOrEmpty(type))
            {
                MessageBox.Show("Please select database type.");
                return;
            }

            if (selectedItem == null || string.IsNullOrEmpty(this.cboDbProfile.Text))
            {
                MessageBox.Show("Please select a profile.");
                return;
            }

            DatabaseType dbType = this.DatabaseType;

            if (!ManagerUtil.IsFileConnection(dbType))
            {
                ConnectionProfileInfo profile = selectedItem as ConnectionProfileInfo;
                string profileName = profile.Name;

                frmDbConnect frm = new frmDbConnect(dbType, profile.Id, requriePassword);
                frm.ConnectionInfo = this.GetConnectionInfo(profile);

                var result = await ConnectionProfileManager.GetProfileById(profile.Id);

                this.SetConnectionInfo(frm);

                if (profileName != result.Name)
                {
                    this.LoadProfileNames(result.Name);
                }

                if (this.cboDbProfile.SelectedItem != null)
                {
                    var p = (this.cboDbProfile.SelectedItem as ConnectionProfileInfo);

                    this.SetProfileConnectionInfo(p, frm.ConnectionInfo);
                }
            }
            else
            {
                FileConnectionProfileInfo profile = selectedItem as FileConnectionProfileInfo;
                string profileName = profile.Name;

                frmFileConnection frm = new frmFileConnection(dbType, requriePassword) { ShowChooseControls = true };
                frm.FileConnectionProfileInfo = profile;

                this.SetFileConnectionInfo(frm);

                if (profileName != frm.FileConnectionProfileInfo?.Name)
                {
                    this.LoadProfileNames(frm.FileConnectionProfileInfo?.Name);
                }

                if (this.cboDbProfile.SelectedItem != null)
                {
                    var p = (this.cboDbProfile.SelectedItem as FileConnectionProfileInfo);

                    this.SetFileProfileConnectionInfo(p, frm.ConnectionInfo);
                }
            }
        }

        private ConnectionInfo GetConnectionInfo(ConnectionProfileInfo profile)
        {
            if (profile != null)
            {
                return new ConnectionInfo() { Server = profile.Server, Port = profile.Port, Database = profile.Database, IntegratedSecurity = profile.IntegratedSecurity, UserId = profile.UserId, Password = profile.Password, IsDba = profile.IsDba, UseSsl = profile.UseSsl };
            }

            return null;
        }

        private void SetProfileConnectionInfo(ConnectionProfileInfo profile, ConnectionInfo connectionInfo)
        {
            if (connectionInfo != null)
            {
                profile.Server = connectionInfo.Server;
                profile.Port = connectionInfo.Port;
                profile.Database = connectionInfo.Database;
                profile.IntegratedSecurity = connectionInfo.IntegratedSecurity;
                profile.UserId = connectionInfo.UserId;
                profile.Password = connectionInfo.Password;
                profile.IsDba = connectionInfo.IsDba;
                profile.UseSsl = connectionInfo.UseSsl;
            }
        }

        private void SetFileProfileConnectionInfo(FileConnectionProfileInfo profile, ConnectionInfo connectionInfo)
        {
            if (connectionInfo != null)
            {
                profile.Database = connectionInfo.Database;
                profile.Password = connectionInfo.Password;
            }
        }

        private void cboDbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadProfileNames();

            if (this.DatabaseTypeSelectedChanged != null)
            {
                this.DatabaseTypeSelectedChanged(sender, e);
            }
        }

        public bool ValidateProfile()
        {
            var profileInfo = this.cboDbProfile.SelectedItem as ConnectionProfileInfo;

            if (profileInfo != null)
            {
                if (!profileInfo.IntegratedSecurity && string.IsNullOrEmpty(profileInfo.Password))
                {
                    MessageBox.Show("Please specify the password.");

                    this.ConfigConnection(true);

                    return false;
                }
            }

            return true;
        }
    }
}
