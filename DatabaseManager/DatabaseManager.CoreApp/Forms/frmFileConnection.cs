using DatabaseInterpreter.Model;
using DatabaseManager.Core;
using DatabaseManager.Data;
using DatabaseManager.Profile;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmFileConnection : Form
    {
        private bool requriePassword = false;
        public DatabaseType DatabaseType { get; set; }
        public ConnectionInfo ConnectionInfo { get; set; }
        public bool ShowChooseControls { get; set; }

        public string FileConnectionProfileId { get; set; }

        public FileConnectionProfileInfo FileConnectionProfileInfo { get; set; }

        public frmFileConnection()
        {
            InitializeComponent();
        }

        public frmFileConnection(DatabaseType dbType)
        {
            InitializeComponent();

            this.DatabaseType = dbType;
        }

        public frmFileConnection(DatabaseType dbType, bool requriePassword)
        {
            InitializeComponent();

            this.DatabaseType = dbType;
            this.requriePassword = requriePassword;
        }

        private void frmFileConnection_Load(object sender, EventArgs e)
        {
            this.Init();
        }

        private void Init()
        {
            if (!this.ShowChooseControls)
            {
                this.panelChoose.Visible = false;

                int height = this.panelChoose.Height + 10;

                this.panelContent.Top -= height;
                this.Height -= height;
            }

            this.ucFileConnection.DatabaseType = this.DatabaseType;

            if (this.FileConnectionProfileInfo != null)
            {
                this.ucFileConnection.LoadData(this.FileConnectionProfileInfo);

                this.txtDisplayName.Text = this.FileConnectionProfileInfo.Name;
            }

            this.ucFileConnection.OnFileSelect += this.OnFileSelected;
        }

        private void OnFileSelected(object? sender, EventArgs e)
        {
            ConnectionInfo connectionInfo = this.ucFileConnection.GetConnectionInfo();

            if (string.IsNullOrEmpty(this.txtDisplayName.Text) && !string.IsNullOrEmpty(connectionInfo.Database))
            {
                this.txtDisplayName.Text = Path.GetFileNameWithoutExtension(connectionInfo.Database);
            }
        }

        private async void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!this.ucFileConnection.ValidateInfo())
            {
                return;
            }

            if (string.IsNullOrEmpty(this.txtDisplayName.Text.Trim()))
            {
                MessageBox.Show("Display name can't be empty.");
                return;
            }

            FileConnectionProfileInfo profileInfo = this.GetFileConnectionProfileInfo();

            var profiles = await FileConnectionProfileManager.GetProfiles(this.DatabaseType.ToString());

            bool isAdd = this.FileConnectionProfileInfo == null;

            if (isAdd)
            {
                if (profiles.Any(item => item.Database == profileInfo.Database))
                {
                    MessageBox.Show($"The record has already existed:{profileInfo.Description}");
                    return;
                }
            }
            else
            {
                if (profiles.Where(item => item.Id != this.FileConnectionProfileInfo.Id).Any(item => item.Database == profileInfo.Database))
                {
                    MessageBox.Show($"The record has already existed:{profileInfo.Description}");
                    return;
                }
            }

            this.FileConnectionProfileId = await FileConnectionProfileManager.Save(profileInfo, this.ucFileConnection.RememberPassword);

            this.FileConnectionProfileInfo = profileInfo;

            this.DialogResult = DialogResult.OK;

            if (SettingManager.Setting.RememberPasswordDuringSession)
            {
                DataStore.SetFileConnectionProfileInfo(profileInfo);
            }

            this.Close();
        }

        private FileConnectionProfileInfo GetFileConnectionProfileInfo()
        {
            ConnectionInfo connectionInfo = this.ucFileConnection.GetConnectionInfo();

            this.ConnectionInfo = connectionInfo;

            FileConnectionProfileInfo profileInfo = new FileConnectionProfileInfo()
            {
                DatabaseType = this.DatabaseType.ToString(),
                Database = connectionInfo.Database,
                HasPassword = this.ucFileConnection.HasPassword,
                Password = connectionInfo.Password,
                Name = this.txtDisplayName.Text.Trim()
            };

            if (this.FileConnectionProfileInfo != null)
            {
                profileInfo.Id = this.FileConnectionProfileInfo.Id;
            }

            return profileInfo;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
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
                        var storeInfo = DataStore.GetFileConnectionProfileInfo(frm.SelectedFileConnectionProfileInfo.Id);

                        if (storeInfo != null && !string.IsNullOrEmpty(storeInfo.Password))
                        {
                            password = storeInfo.Password;
                        }
                    }

                    this.ucFileConnection.LoadData(frm.SelectedFileConnectionProfileInfo, password);

                    this.txtDisplayName.Text = frm.SelectedFileConnectionProfileInfo.Name;
                }
            }
        }
    }
}
