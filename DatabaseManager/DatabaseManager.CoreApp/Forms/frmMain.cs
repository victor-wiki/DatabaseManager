using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Controls;
using DatabaseManager.Core;
using DatabaseManager.Core.Model;
using DatabaseManager.Helper;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseManager.Forms
{
    public partial class frmMain : Form, IObserver<FeedbackInfo>
    {
        private Setting setting = SettingManager.Setting;
        private frmObjectsExplorer explorerForm = new frmObjectsExplorer();
        private frmMessage messageForm = new frmMessage();
        internal static bool IsClosing { get; private set; } = false;

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
            this.tsmiCodeGenerator.Image = IconImageHelper.GetImage(IconChar.Code);

            FeedbackHelper.EnableLog = SettingManager.Setting.EnableLog;
            LogHelper.LogType = SettingManager.Setting.LogType;
            FeedbackHelper.EnableDebug = true;

            this.explorerForm.HideOnClose = true;
            this.messageForm.HideOnClose = true;

            this.explorerForm.Explorer.OnShowContent += this.ShowDbObjectContent;
            this.explorerForm.Explorer.OnFeedback += this.Feedback;

            this.ApplyTheme();

            this.LoadLayoutFromProfile();
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(frmMessage).ToString())
            {
                return messageForm;
            }
            else if (persistString == typeof(frmObjectsExplorer).ToString())
            {
                return explorerForm;
            }
            else
            {
                return null;
            }
        }

        private void LoadLayoutFromProfile()
        {
            bool useProfileLayout = false;

            if (setting.RememberApplicationLayoutInformation)
            {
                string profileFilePath = ProfileFileHelper.LayoutFilePath;

                if (File.Exists(profileFilePath))
                {
                    try
                    {
                        DeserializeDockContent deserializeDockContent = new DeserializeDockContent(this.GetContentFromPersistString);

                        this.dockPanelMain.LoadFromXml(profileFilePath, deserializeDockContent);

                        useProfileLayout = true;
                    }
                    catch (Exception ex)
                    {

                    }
                }

                if (File.Exists(ProfileFileHelper.RecentFilePath))
                {
                    var filePaths = ProfileFileHelper.GetRecentFiles().Distinct();

                    foreach (var filePath in filePaths)
                    {
                        if (File.Exists(filePath))
                        {
                            this.LoadFile(filePath);
                        }
                    }
                }
            }

            if (!useProfileLayout)
            {
                this.explorerForm.Show(this.dockPanelMain, DockState.DockLeft);
                this.messageForm.Show(this.dockPanelMain, DockState.DockBottomAutoHide);
            }
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

        private void UpdateContentFormTooltip(DatabaseObjectDisplayInfo info)
        {
            IDockContent content = this.FindContent(info);

            frmContent contentForm = null;

            if (content != null)
            {
                contentForm = content as frmContent;

                contentForm.ToolTipText = contentForm.ContentControl.GetTooltip(info);
            }
        }

        private IDockContent FindContent(DatabaseObjectDisplayInfo info)
        {
            foreach (IDockContent content in this.dockPanelMain.Documents)
            {
                frmContent form = content as frmContent;

                DatabaseObjectDisplayInfo data = form.Tag as DatabaseObjectDisplayInfo;

                if (data.Name == info.Name && data.Schema == info.Schema && info.DatabaseType == data.DatabaseType && info.DisplayType == data.DisplayType)
                {
                    if (ObjectHelper.AreObjectsEqual(data.ConnectionInfo, info.ConnectionInfo))
                    {
                        if (info.DisplayType == DatabaseObjectDisplayType.Script)
                        {
                            if (info.ScriptAction != data.ScriptAction)
                            {
                                return null;
                            }
                        }

                        return content;
                    }
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
                DatabaseObjectDisplayInfo info = new DatabaseObjectDisplayInfo() { IsNew = true, DisplayType = DatabaseObjectDisplayType.Script, DatabaseType = this.GetCurrentDatabaseType() };

                info.ConnectionInfo = connectionInfo;

                this.ShowDbObjectContent(info);
            }
            else
            {
                MessageBox.Show("Please select a database from the left navigator first.");

                this.explorerForm.Explorer.SelectNone();
            }
        }

        private void tsBtnRun_Click(object sender, EventArgs e)
        {
            this.RunScripts();
        }

        private void RunScripts()
        {
            var control = this.GetCurrentContentForm()?.ContentControl;

            if (control != null)
            {
                var selectedDatabaseType = this.GetCurrentDatabaseType();
                var oldDatabaseType = control.DisplayInfo.DatabaseType;
                var oldConnectionInfo = control.DisplayInfo.ConnectionInfo;

                if (selectedDatabaseType != oldDatabaseType && oldConnectionInfo != null)
                {
                    control.RunScripts();
                }
                else
                {
                    ConnectionInfo connectionInfo = this.explorerForm.Explorer.GetCurrentConnectionInfo();

                    if (this.CheckConnectionInfo(connectionInfo))
                    {
                        control.DisplayInfo.DatabaseType = selectedDatabaseType;
                        control.DisplayInfo.ConnectionInfo = connectionInfo;

                        this.UpdateContentFormTooltip(control.DisplayInfo);

                        control.RunScripts();
                    }
                }
            }
        }

        private bool CheckConnectionInfo(ConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
            {
                MessageBox.Show("Please select a database from the left navigator first.");
                return false;
            }

            return true;
        }

        private DatabaseType GetCurrentDatabaseType()
        {
            return this.explorerForm.Explorer.DatabaseType;
        }

        private void tsBtnSave_Click(object sender, EventArgs e)
        {
            this.SaveContent();
        }

        private void tsBtnOpenFile_Click(object sender, EventArgs e)
        {
            bool hasSelectedNode = this.explorerForm.Explorer.HasSelectedNode();

            if (this.dlgOpenFile == null)
            {
                this.dlgOpenFile = new OpenFileDialog();
            }

            if (this.dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                if (!hasSelectedNode)
                {
                    this.explorerForm.Explorer.SelectNone();
                }

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

            this.ShowDbObjectContent(info);
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
            frmSchemaCompare form = new frmSchemaCompare();
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
            IsClosing = true;

            bool rememberLayout = this.setting.RememberApplicationLayoutInformation;

            if (rememberLayout)
            {
                ProfileFileHelper.ResetRecentFile();
            }

            this.CloseContentForms();

            e.Cancel = this.dockPanelMain.DocumentsCount > 0;

            if (!e.Cancel)
            {
                if (rememberLayout)
                {
                    this.SaveLayoutInformation();
                }
            }

            IsClosing = false;
        }

        private void SaveLayoutInformation()
        {
            try
            {
                this.dockPanelMain.SaveAsXml(ProfileFileHelper.LayoutFilePath);
            }
            catch (Exception ex)
            {
            }
        }

        private void CloseContentForms()
        {
            var documents = this.dockPanelMain.Documents;

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

                    if (this.dockPanelMain.InvokeRequired)
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

        private void tsBtnDataCompare_Click(object sender, EventArgs e)
        {
            frmDataCompare frm = new frmDataCompare();
            frm.ShowDialog();
        }

        private void tsmiJsonViwer_Click(object sender, EventArgs e)
        {
            frmJsonViewer frm = new frmJsonViewer();
            frm.Show();
        }

        private void tsmiCodeGenerator_Click(object sender, EventArgs e)
        {
            frmCodeGenerator frm = new frmCodeGenerator();
            frm.Show();
        }
    }
}
