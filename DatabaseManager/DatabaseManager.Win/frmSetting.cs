using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
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
            this.tabControl1.SelectedIndex = 0;

            Setting setting = SettingManager.Setting;

            this.numCommandTimeout.Value = setting.CommandTimeout;
            this.numDataBatchSize.Value = setting.DataBatchSize;
            this.chkShowBuiltinDatabase.Checked = setting.ShowBuiltinDatabase;
            this.txtMySqlCharset.Text = setting.MySqlCharset;
            this.txtMySqlCharsetCollation.Text = setting.MySqlCharsetCollation;
            this.chkEnableLog.Checked = setting.EnableLog;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            Setting setting = SettingManager.Setting;
            setting.CommandTimeout = (int)this.numCommandTimeout.Value;
            setting.DataBatchSize = (int)this.numDataBatchSize.Value;
            setting.ShowBuiltinDatabase = this.chkShowBuiltinDatabase.Checked;
            setting.MySqlCharset = this.txtMySqlCharset.Text.Trim();
            setting.MySqlCharsetCollation = this.txtMySqlCharsetCollation.Text.Trim();
            setting.EnableLog = this.chkEnableLog.Checked;

            SettingManager.SaveConfig(setting);
        }
    }
}
