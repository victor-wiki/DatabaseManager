using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Controls;
//using DatabaseManager.Controls.Model;
using DatabaseManager.Core;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using DatabaseManager.Profile;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmSetting : Form
    {
        private List<string> convertConcatCharTargetDatabases;

        public frmSetting()
        {
            InitializeComponent();
        }

        private void frmSetting_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private async void InitControls()
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
            this.chkValidateScriptsAfterTranslated.Checked = setting.ValidateScriptsAfterTranslated;

            var dbTypes = Enum.GetNames(typeof(DatabaseType));
            this.cboPreferredDatabase.Items.AddRange(dbTypes);
            this.chkRememberPasswordDuringSession.Checked = setting.RememberPasswordDuringSession;
            this.cboPreferredDatabase.Text = setting.PreferredDatabase.ToString();
            this.txtOutputFolder.Text = setting.ScriptsDefaultOutputFolder;

            PersonalSetting ps = await PersonalSettingManager.GetPersonalSetting();

            if (ps != null && !string.IsNullOrEmpty(ps.LockPassword))
            {
                this.txtLockPassword.Text = ps.LockPassword;
            }

            this.convertConcatCharTargetDatabases = setting.ConvertConcatCharTargetDatabases;
        }

        private async void btnConfirm_Click(object sender, EventArgs e)
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
            setting.ValidateScriptsAfterTranslated = this.chkValidateScriptsAfterTranslated.Checked;

            string password = this.txtLockPassword.Text.Trim();

            PersonalSetting ps = new PersonalSetting() { LockPassword = password };

            await PersonalSettingManager.Save(ps);

            if (this.cboPreferredDatabase.SelectedIndex >= 0)
            {
                setting.PreferredDatabase = (DatabaseType)Enum.Parse(typeof(DatabaseType), this.cboPreferredDatabase.Text);
            }

            LogType logType = LogType.None;

            if (this.chkLogInfo.Checked)
            {
                logType |= LogType.Info;
            }

            if (this.chkLogError.Checked)
            {
                logType |= LogType.Error;
            }

            setting.LogType = logType;

            setting.ConvertConcatCharTargetDatabases = this.convertConcatCharTargetDatabases;

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

        private void btnSelectTargetDatabaseTypesForConcatChar_Click(object sender, EventArgs e)
        {
            frmItemsSelector selector = new frmItemsSelector("Select Database Types", ItemsSelectorHelper.GetDatabaseTypeItems(this.convertConcatCharTargetDatabases));

            if (selector.ShowDialog() == DialogResult.OK)
            {
                this.convertConcatCharTargetDatabases = selector.CheckedItem.Select(item => item.Name).ToList();
            }
        }
    }
}
