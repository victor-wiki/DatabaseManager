using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
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
            this.txtMySqlCharset.Text = setting.MySqlCharset;
            this.txtMySqlCharsetCollation.Text = setting.MySqlCharsetCollation;
            this.chkNotCreateIfExists.Checked = setting.NotCreateIfExists;
            this.chkEnableLog.Checked = setting.EnableLog;
            this.cboDbObjectNameMode.Text = setting.DbObjectNameMode.ToString();
            this.chkLogInfo.Checked = setting.LogType.HasFlag(LogType.Info);
            this.chkLogError.Checked = setting.LogType.HasFlag(LogType.Error);
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            Setting setting = SettingManager.Setting;
            setting.CommandTimeout = (int)this.numCommandTimeout.Value;
            setting.DataBatchSize = (int)this.numDataBatchSize.Value;
            setting.ShowBuiltinDatabase = this.chkShowBuiltinDatabase.Checked;
            setting.MySqlCharset = this.txtMySqlCharset.Text.Trim();
            setting.MySqlCharsetCollation = this.txtMySqlCharsetCollation.Text.Trim();
            setting.NotCreateIfExists = this.chkNotCreateIfExists.Checked;
            setting.EnableLog = this.chkEnableLog.Checked;
            setting.DbObjectNameMode = (DbObjectNameMode)Enum.Parse(typeof(DbObjectNameMode), this.cboDbObjectNameMode.Text);

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
        }
    }
}
