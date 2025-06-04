using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Controls;
using DatabaseManager.Core;
using DatabaseManager.Forms;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using static System.Net.Mime.MediaTypeNames;

namespace DatabaseManager
{
    public partial class frmMain : Form, IObserver<FeedbackInfo>
    {
        frmObjectsExplorer explorerForm = new frmObjectsExplorer();
        frmMessage messageForm = new frmMessage();

        public frmMain()
        {
            InitializeComponent();

            AutoScaleMode = AutoScaleMode.Dpi;

            this.InitControls();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
        }

        private void InitControls()
        {
            TreeView.CheckForIllegalCrossThreadCalls = false;
            TextBox.CheckForIllegalCrossThreadCalls = false;

            this.tsmiSetting.Image = IconImageHelper.GetImage(IconChar.Gear);         
            this.tsmiLock.Image = IconImageHelper.GetImage(IconChar.Lock, Color.Orange);
            this.tsmiWktView.Image = IconImageHelper.GetImage(IconChar.DrawPolygon);
            this.tsmiImageViewer.Image = IconImageHelper.GetImageByFontType(IconChar.Image, IconFont.Solid);

            FeedbackHelper.EnableLog = SettingManager.Setting.EnableLog;
            LogHelper.LogType = SettingManager.Setting.LogType;
            FeedbackHelper.EnableDebug = true;

            this.explorerForm.HideOnClose = true;
            this.messageForm.HideOnClose = true;

            this.explorerForm.Explorer.OnShowContent += this.ShowDbObjectContent;
            this.explorerForm.Explorer.OnFeedback += this.Feedback;

            this.ApplyTheme();


            this.explorerForm.Show(this.dockPanelMain, DockState.DockLeft);
            this.messageForm.Show(this.dockPanelMain, DockState.DockBottomAutoHide);
        }

        private void ApplyTheme()
        {
            ThemeOption themeOption = SettingManager.Setting.ThemeOption;

            ThemeBase theme = null;

            switch (themeOption.ThemeType)
            {
                case ThemeType.Light:
                    theme = new WeifenLuo.WinFormsUI.Docking.VS2015LightTheme();
                    break;
                case ThemeType.Blue:
                    theme = new WeifenLuo.WinFormsUI.Docking.VS2015BlueTheme();
                    break;
                case ThemeType.Dark:
                    theme = new WeifenLuo.WinFormsUI.Docking.VS2015DarkTheme();
                    break;
            }

            this.dockPanelMain.Theme = theme;
        }

        private void ShowDbObjectContent(DatabaseObjectDisplayInfo info)
        {
            IDockContent content = this.FindContent(info);

            frmContent contentForm = null;

            if (content != null)
            {
                contentForm = content as frmContent;

                contentForm.Show(this.dockPanelMain);
            }
            else
            {
                contentForm = new frmContent();

                var documents = this.dockPanelMain.Documents;

                contentForm.Tag = info;
                contentForm.ContentControl.ExistingContents = documents;
                contentForm.Text = contentForm.ContentControl.GetInfoName(info);
                contentForm.ToolTipText = contentForm.ContentControl.GetTooltip(info);

                contentForm.ContentControl.OnDataFilter += this.DataFilter;
                contentForm.ContentControl.OnFeedback += this.Feedback;
                contentForm.ContentControl.OnContentSaved += (title, tip) =>
                {
                    contentForm.Text = title;
                    contentForm.ToolTipText = tip;
                };

                contentForm.Show(this.dockPanelMain, DockState.Document);

                if (documents != null && documents.Count() > 1)
                {
                    contentForm.DockTo(this.dockPanelMain.ActiveDocumentPane, DockStyle.Fill, 0);
                }

                contentForm.ContentControl.ShowContent(info);
            }
        }

        private IDockContent FindContent(DatabaseObjectDisplayInfo info)
        {
            foreach (IDockContent content in this.dockPanelMain.Documents)
            {
                frmContent form = content as frmContent;

                DatabaseObjectDisplayInfo data = form.Tag as DatabaseObjectDisplayInfo;

                if (data.Name == info.Name && info.DatabaseType == data.DatabaseType && info.DisplayType == data.DisplayType)
                {
                    if(info.DisplayType == DatabaseObjectDisplayType.Script)
                    {
                        if(info.ScriptAction != data.ScriptAction)
                        {
                            return null;
                        }
                    }

                    return content;
                }
            }

            return null;
        }


        private void DataFilter(object sender)
        {
            IEnumerable<DataGridViewColumn> columns = null;
            QueryConditionBuilder queryConditionBuilder = null;

            if (sender is UC_DataViewer dataViewer)
            {
                columns = dataViewer.Columns;
                queryConditionBuilder = dataViewer.ConditionBuilder;
            }
            else if (sender is UC_DataEditor dataEditor)
            {
                columns = dataEditor.Columns;
                queryConditionBuilder = dataEditor.ConditionBuilder;
            }

            frmDataFilter filter = new frmDataFilter() { Columns = columns.ToList(), ConditionBuilder = queryConditionBuilder };

            if (filter.ShowDialog() == DialogResult.OK)
            {
                if (sender is UC_DataViewer dv)
                {
                    dv.FilterData(filter.ConditionBuilder);
                }
                else if (sender is UC_DataEditor de)
                {
                    de.FilterData(filter.ConditionBuilder);
                }
            }
        }

        private void Feedback(FeedbackInfo info)
        {
            this.messageForm.ShowMessage(info);

            if (info.InfoType == FeedbackInfoType.Error)
            {
                this.ShowMessage();
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
                this.SaveContent();
            }
        }

        private void SaveContent()
        {
            var contentControl = this.GetCurrentContentForm()?.ContentControl;

            if (contentControl != null)
            {
                contentControl.Save();
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
            ConnectionInfo connectionInfo = this.explorerForm.Explorer.GetCurrentConnectionInfo();

            if (connectionInfo != null)
            {
                DatabaseObjectDisplayInfo info = new DatabaseObjectDisplayInfo() { IsNew = true, DisplayType = DatabaseObjectDisplayType.Script, DatabaseType = this.explorerForm.Explorer.DatabaseType };

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
            this.GetCurrentContentForm()?.ContentControl?.RunScripts();
        }

        private void tsBtnSave_Click(object sender, EventArgs e)
        {
            this.SaveContent();
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

            DatabaseObjectDisplayInfo info = this.explorerForm.Explorer.GetDisplayInfo();

            info.DisplayType = DatabaseObjectDisplayType.Script;
            info.FilePath = filePath;
            info.Name = Path.GetFileName(info.FilePath);

            this.GetCurrentContentForm()?.ContentControl?.ShowContent(info);
        }

        private frmContent GetCurrentContentForm()
        {
            var doc = this.dockPanelMain.ActiveDocument;

            return (doc as frmContent);
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

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.CloseContentForms();

            e.Cancel = this.dockPanelMain.DocumentsCount > 0;
        }

        private void CloseContentForms()
        {
            var documents = this.dockPanelMain.Documents;
            var currentDocument = this.dockPanelMain.ActiveDocument;

            List<Form> forms = new List<Form>();

            foreach (IDockContent document in documents)
            {
                frmContent conentForm = document as frmContent;

                forms.Add(conentForm);
            }

            foreach (var item in forms)
            {
                item.Close();
            }
        }

        private void tsmiObjectsExplorer_Click(object sender, EventArgs e)
        {
            this.explorerForm.Show(this.dockPanelMain);
        }


        private void tsmiMessage_Click(object sender, EventArgs e)
        {
            this.ShowMessage();
        }

        private void ShowMessage()
        {
            this.messageForm.Show(this.dockPanelMain);

            try
            {
                if (this.messageForm.DockHandler.DockState.ToString().Contains("AutoHide"))
                {
                    Action action = () =>
                    {
                        this.dockPanelMain.ActiveAutoHideContent = this.messageForm;
                    };

                    if(this.dockPanelMain.InvokeRequired)
                    {
                        this.dockPanelMain.Invoke(action);
                    }
                    else
                    {
                        action();
                    }
                  
                    this.messageForm.DockHandler.Activate();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
