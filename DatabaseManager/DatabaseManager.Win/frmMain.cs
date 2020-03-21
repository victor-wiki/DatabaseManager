using DatabaseInterpreter.Core;
using DatabaseInterpreter.Utility;
using DatabaseManager.Model;
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
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();            
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.InitControls();

            FeedbackHelper.EnableLog = SettingManager.Setting.EnableLog;
            LogHelper.EnableDebug = true;
        }

        private void InitControls()
        {
            this.navigator.OnShowContent += this.ShowDbObjectContent;
        }

        private void ShowDbObjectContent(DatabaseObjectDisplayInfo content)
        {
            this.ucContent.Visible = true;
            this.ucContent.ShowContent(content);
        }

        private void tsmiSetting_Click(object sender, EventArgs e)
        {
            frmSetting frmSetting = new frmSetting();
            frmSetting.ShowDialog();
        }

        private void btnGenerateScripts_Click(object sender, EventArgs e)
        {
            frmGenerateScripts frmGenerateScripts = new frmGenerateScripts();
            frmGenerateScripts.ShowDialog();
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            frmConvert frmConvert = new frmConvert();
            frmConvert.ShowDialog();
        }
    }
}
