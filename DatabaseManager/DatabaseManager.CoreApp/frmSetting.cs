using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Model;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmSetting : Form
    {
        public frmSetting()
        {
            InitializeComponent();
        }

        private void frmSetting_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            this.tabControl1.SelectedIndex = 0;

            var dbObjectNameModes = Enum.GetNames(typeof(DbObjectNameMode));
            this.cboDbObjectNameMode.Items.AddRange(dbObjectNameModes);           

            Setting setting = SettingManager.Setting;

            this.numCommandTimeout.Value = setting.CommandTimeout;
            this.numDataBatchSize.Value = setting.DataBatchSize;
            this.chkShowBuiltinDatabase.Checked = setting.ShowBuiltinDatabase;
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Checked = setting.UseOriginalDataTypeIfUdtHasOnlyOneAttr;
            this.txtMySqlCharset.Text = setting.MySqlCharset;
            this.txtMySqlCharsetCollation.Text = setting.MySqlCharsetCollation;
            this.chkNotCreateIfExists.Checked = setting.NotCreateIfExists;
            this.chkEnableLog.Checked = setting.EnableLog;
            this.cboDbObjectNameMode.Text = setting.DbObjectNameMode.ToString();
            this.chkLogInfo.Checked = setting.LogType.HasFlag(LogType.Info);
            this.chkLogError.Checked = setting.LogType.HasFlag(LogType.Error);
            this.chkEnableEditorHighlighting.Checked = setting.EnableEditorHighlighting;
            this.chkEditorEnableIntellisence.Checked = setting.EnableEditorIntellisence;
            this.chkExcludePostgresExtensionObjects.Checked = setting.ExcludePostgresExtensionObjects;           

            var dbTypes = Enum.GetNames(typeof(DatabaseType));
            this.cboPreferredDatabase.Items.AddRange(dbTypes);
            this.chkRememberPasswordDuringSession.Checked = setting.RememberPasswordDuringSession;
            this.cboPreferredDatabase.Text = setting.PreferredDatabase.ToString();
            this.txtOutputFolder.Text = setting.ScriptsDefaultOutputFolder;

            if(!string.IsNullOrEmpty(setting.LockPassword))
            {
                this.txtLockPassword.Text = AesHelper.Decrypt(setting.LockPassword);
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            Setting setting = SettingManager.Setting;
            setting.CommandTimeout = (int)this.numCommandTimeout.Value;
            setting.DataBatchSize = (int)this.numDataBatchSize.Value;
            setting.ShowBuiltinDatabase = this.chkShowBuiltinDatabase.Checked;
            setting.UseOriginalDataTypeIfUdtHasOnlyOneAttr = this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Checked;
            setting.MySqlCharset = this.txtMySqlCharset.Text.Trim();
            setting.MySqlCharsetCollation = this.txtMySqlCharsetCollation.Text.Trim();
            setting.NotCreateIfExists = this.chkNotCreateIfExists.Checked;
            setting.EnableLog = this.chkEnableLog.Checked;
            setting.DbObjectNameMode = (DbObjectNameMode)Enum.Parse(typeof(DbObjectNameMode), this.cboDbObjectNameMode.Text);
            setting.RememberPasswordDuringSession = this.chkRememberPasswordDuringSession.Checked;
            setting.EnableEditorHighlighting = this.chkEnableEditorHighlighting.Checked;
            setting.EnableEditorIntellisence = this.chkEditorEnableIntellisence.Checked;
            setting.ExcludePostgresExtensionObjects = this.chkExcludePostgresExtensionObjects.Checked;           
            setting.ScriptsDefaultOutputFolder = this.txtOutputFolder.Text;

            string password = this.txtLockPassword.Text.Trim();

            if(!string.IsNullOrEmpty(password))
            {
                setting.LockPassword = AesHelper.Encrypt(password);
            }
            else
            {
                setting.LockPassword = "";
            }

            if(this.cboPreferredDatabase.SelectedIndex>=0)
            {
                setting.PreferredDatabase =(DatabaseType) Enum.Parse(typeof(DatabaseType), this.cboPreferredDatabase.Text);
            }

            LogType logType = LogType.None;

            if(this.chkLogInfo.Checked)
            {
                logType |= LogType.Info;
            }

            if (this.chkLogError.Checked)
            {
                logType |= LogType.Error;
            }

            setting.LogType = logType;

            SettingManager.SaveConfig(setting);

            DbInterpreter.Setting = SettingManager.GetInterpreterSetting();
        }

        private void btnOutputFolder_Click(object sender, EventArgs e)
        {
            if (this.dlgOutputFolder == null)
            {
                this.dlgOutputFolder = new FolderBrowserDialog();
            }

            DialogResult result = this.dlgOutputFolder.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.txtOutputFolder.Text = this.dlgOutputFolder.SelectedPath;
            }
        }
    }
}
