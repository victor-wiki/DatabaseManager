using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Controls;
using DatabaseManager.Core;
using DatabaseManager.Forms;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmMain : Form, IObserver<FeedbackInfo>
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.InitControls();

            TreeView.CheckForIllegalCrossThreadCalls = false;
            TextBox.CheckForIllegalCrossThreadCalls = false;            

            FeedbackHelper.EnableLog = SettingManager.Setting.EnableLog;
            LogHelper.LogType = SettingManager.Setting.LogType;
            FeedbackHelper.EnableDebug = true;
        }

        private void InitControls()
        {
            this.navigator.OnShowContent += this.ShowDbObjectContent;
            this.navigator.OnFeedback += this.Feedback;
            this.ucContent.OnDataFilter += this.DataFilter;
            this.ucContent.OnFeedback += this.Feedback;
        }

        private void ShowDbObjectContent(DatabaseObjectDisplayInfo content)
        {
            this.ucContent.ShowContent(content);
        }

        private void DataFilter(object sender)
        {
            UC_DataViewer dataViewer = sender as UC_DataViewer;

            frmDataFilter filter = new frmDataFilter() { Columns = dataViewer.Columns.ToList(), ConditionBuilder = dataViewer.ConditionBuilder };
            if (filter.ShowDialog() == DialogResult.OK)
            {
                dataViewer.FilterData(filter.ConditionBuilder);
            }
        }

        private void Feedback(FeedbackInfo info)
        {
            this.txtMessage.ForeColor = Color.Black;

            if (info.InfoType == FeedbackInfoType.Error)
            {
                if (!info.IgnoreError)
                {
                    MessageBox.Show(info.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.txtMessage.Text = info.Message;
                this.txtMessage.BackColor = this.BackColor;
                this.txtMessage.ForeColor = Color.Red;       
            }
            else
            {
                this.txtMessage.Text = info.Message;
            }
            
        }

        private void tsmiSetting_Click(object sender, EventArgs e)
        {
            frmSetting frmSetting = new frmSetting();
            frmSetting.ShowDialog();
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            frmConvert frmConvert = new frmConvert();
            frmConvert.ShowDialog();
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                if (FormEventCenter.OnSave != null)
                {
                    FormEventCenter.OnSave();
                }
            }
        }

        public void OnNext(FeedbackInfo value)
        {
            this.Feedback(value);
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        private void tsmiDbConnection_Click(object sender, EventArgs e)
        {
            frmDbConnectionManage frmDbConnectionManage = new frmDbConnectionManage();
            frmDbConnectionManage.ShowDialog();
        }

        private void tsBtnGenerateScripts_Click(object sender, EventArgs e)
        {
            frmGenerateScripts frmGenerateScripts = new frmGenerateScripts();
            frmGenerateScripts.ShowDialog();
        }

        private void tsBtnConvert_Click(object sender, EventArgs e)
        {
            frmConvert frmConvert = new frmConvert();
            frmConvert.ShowDialog();
        }

        private void tsBtnAddQuery_Click(object sender, EventArgs e)
        {
            ConnectionInfo connectionInfo = this.navigator.GetCurrentConnectionInfo();

            if (connectionInfo != null)
            {
                DatabaseObjectDisplayInfo info = new DatabaseObjectDisplayInfo() { IsNew = true, DisplayType = DatabaseObjectDisplayType.Script, DatabaseType = this.navigator.DatabaseType };

                info.ConnectionInfo = connectionInfo;

                this.ShowDbObjectContent(info);
            }
            else
            {
                MessageBox.Show("Please select a database from left navigator first.");
            }
        }

        private void tsBtnRun_Click(object sender, EventArgs e)
        {
            this.RunScripts();
        }

        private void RunScripts()
        {
            this.ucContent.RunScripts();
        }

        private void tsBtnSave_Click(object sender, EventArgs e)
        {
            if (FormEventCenter.OnSave != null)
            {
                FormEventCenter.OnSave();
            }
        }

        private void tsBtnOpenFile_Click(object sender, EventArgs e)
        {
            if (this.dlgOpenFile == null)
            {
                this.dlgOpenFile = new OpenFileDialog();
            }

            if (this.dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                this.LoadFile(this.dlgOpenFile.FileName);
            }
        }

        private void LoadFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            DatabaseObjectDisplayInfo info = this.navigator.GetDisplayInfo();

            info.DisplayType = DatabaseObjectDisplayType.Script;
            info.FilePath = filePath;
            info.Name = Path.GetFileName(info.FilePath);

            this.ucContent.ShowContent(info);
        }

        private void frmMain_DragOver(object sender, DragEventArgs e)
        {
            this.SetDragEffect(e);
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            this.SetDropFiles(e);
        }

        private void SetDragEffect(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void SetDropFiles(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop.ToString());

                foreach (string filePath in filePaths)
                {
                    this.LoadFile(filePath);
                }
            }
        }

        private void tsmiBackupSetting_Click(object sender, EventArgs e)
        {
            frmBackupSetting frm = new frmBackupSetting();
            frm.ShowDialog();
        }

        private void tsBtnCompare_Click(object sender, EventArgs e)
        {
            frmCompare form = new frmCompare();
            form.ShowDialog();
        }

        private void tsmiLock_Click(object sender, EventArgs e)
        {
            frmLockApp lockApp = new frmLockApp();
            lockApp.ShowDialog();
        }

        private void txtMessage_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.SetToolTip(this.txtMessage, this.txtMessage.Text);
        }       

        private void tsBtnTranslateScript_Click(object sender, EventArgs e)
        {
            frmTranslateScript translateScript = new frmTranslateScript();
            translateScript.Show();
        }

        private void tsmiWktView_Click(object sender, EventArgs e)
        {
            frmWktViewer geomViewer = new frmWktViewer();
            geomViewer.Show();
        }

        private void tsmiImageViewer_Click(object sender, EventArgs e)
        {
            frmImageViewer imgViewer = new frmImageViewer();
            imgViewer.Show();
        }
    }
}
