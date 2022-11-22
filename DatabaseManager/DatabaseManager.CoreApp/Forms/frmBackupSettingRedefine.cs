using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseInterpreter.Model;
using DatabaseManager.Core;
using DatabaseManager.Model;

namespace DatabaseManager
{
    public partial class frmBackupSettingRedefine : Form
    {
        public DatabaseType DatabaseType { get; set; }
        public BackupSetting Setting { get; private set; }

        public frmBackupSettingRedefine()
        {
            InitializeComponent();
        }

        private void frmBackupSettingRedefine_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            this.Setting = BackupSettingManager.GetSettings().FirstOrDefault(item => item.DatabaseType == this.DatabaseType.ToString());

            if (this.Setting != null)
            {
                this.txtSaveFolder.Text = this.Setting.SaveFolder;
                this.chkZipFile.Checked = this.Setting.ZipFile;
            }

            if (this.DatabaseType == DatabaseType.SqlServer || this.DatabaseType == DatabaseType.Postgres)
            {
                this.chkZipFile.Enabled = false;
            }
        }

        private void btnSaveFolder_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1 == null)
            {
                this.folderBrowserDialog1 = new FolderBrowserDialog();
            }

            if (!string.IsNullOrEmpty(this.txtSaveFolder.Text))
            {
                this.folderBrowserDialog1.SelectedPath = this.txtSaveFolder.Text;
            }

            DialogResult result = this.folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.txtSaveFolder.Text = this.folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            string saveFolder = this.txtSaveFolder.Text.Trim();
            bool zipFile = this.chkZipFile.Checked;

            if (this.Setting == null)
            {
                this.Setting = new BackupSetting() { DatabaseType = this.DatabaseType.ToString(), SaveFolder = saveFolder, ZipFile = zipFile };
            }

            if (this.chkSetAsDefault.Checked)
            {
                var settings = BackupSettingManager.GetSettings();

                var setting = settings.FirstOrDefault(item => item.DatabaseType == this.DatabaseType.ToString());

                if (setting == null)
                {
                    settings.Add(this.Setting);
                }
                else
                {
                    setting.SaveFolder = saveFolder;
                    setting.ZipFile = zipFile;
                }

                BackupSettingManager.SaveConfig(settings);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
