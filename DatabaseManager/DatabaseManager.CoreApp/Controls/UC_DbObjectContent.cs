using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_DbObjectContent : UserControl
    {
        private Dictionary<int, Rectangle> dictCloseButtonRectangle = new Dictionary<int, Rectangle>();

        public DataFilterHandler OnDataFilter;
        public FeedbackHandler OnFeedback;

        public UC_DbObjectContent()
        {
            InitializeComponent();

            TabControl.CheckForIllegalCrossThreadCalls = false;
            TabPage.CheckForIllegalCrossThreadCalls = false;

            FormEventCenter.OnSave += this.Save;
            FormEventCenter.OnRunScripts += this.RunScripts;
        }

        public void ShowContent(DatabaseObjectDisplayInfo info)
        {
            this.Visible = true;

            TabPage page = this.FindTabPage(info);

            string title = this.GetFormatTabHeaderText(this.GetInfoName(info));

            if (page == null)
            {
                page = new TabPage(title) { };

                this.tabControl1.TabPages.Insert(0, page);

                this.tabControl1.SelectedTab = page;
            }
            else
            {
                this.tabControl1.SelectedTab = page;
            }

            page.Tag = info;

            page.BackColor = Color.Transparent;

            this.SetTabPageContent(info, page);

            this.SetTabPageTooltip(page);
        }

        private string GetFormatTabHeaderText(string text)
        {
            return $" {text}  ";
        }

        private void SetTabPageTooltip(TabPage page)
        {
            DatabaseObjectDisplayInfo info = page.Tag as DatabaseObjectDisplayInfo;

            if (info != null)
            {
                string database = info.ConnectionInfo == null ? "" : $@": {info.ConnectionInfo?.Server}-{info.ConnectionInfo?.Database}";

                string filePath = File.Exists(info.FilePath) ? (info.FilePath + "  -  ") : "";

                string title = string.IsNullOrEmpty(filePath) ? $" - {page.Text}" : "";

                page.ToolTipText = $@"{filePath}{info.DatabaseType}{database}{title}";
            }
        }

        private void SetTabPageContent(DatabaseObjectDisplayInfo info, TabPage tabPage)
        {
            if (info.DisplayType == DatabaseObjectDisplayType.Script)
            {
                UC_SqlQuery sqlQuery = this.GetUcControl<UC_SqlQuery>(tabPage);

                if (sqlQuery == null)
                {
                    sqlQuery = this.AddControlToTabPage<UC_SqlQuery>(tabPage);
                }

                sqlQuery.Show(info);

                if (!string.IsNullOrEmpty(sqlQuery.Editor.Text))
                {
                    RichTextBoxHelper.Highlighting(sqlQuery.Editor, info.DatabaseType, false);

                    if (info.Error != null)
                    {
                        RichTextBoxHelper.HighlightingError(sqlQuery.Editor, info.Error);
                    }

                    if ((info.DatabaseObject is ScriptDbObject) && SettingManager.Setting.ValidateScriptsAfterTranslated && info.IsTranlatedScript)
                    {
                        sqlQuery.ValidateScripts();
                    }
                }
                else
                {
                    sqlQuery.Editor.Focus();
                }
            }
            else if (info.DisplayType == DatabaseObjectDisplayType.Data)
            {
                UC_DataViewer dataViewer = this.GetUcControl<UC_DataViewer>(tabPage);

                if (dataViewer == null)
                {
                    dataViewer = this.AddControlToTabPage<UC_DataViewer>(tabPage);
                    dataViewer.OnDataFilter += this.DataFilter;
                }

                dataViewer.Show(info);
            }
            else if (info.DisplayType == DatabaseObjectDisplayType.TableDesigner)
            {
                UC_TableDesigner tableDesigner = this.GetUcControl<UC_TableDesigner>(tabPage);

                if (tableDesigner == null)
                {
                    tableDesigner = this.AddControlToTabPage<UC_TableDesigner>(tabPage);
                    tableDesigner.OnFeedback += this.Feedback;
                }

                tableDesigner.Show(info);
            }
        }

        private void DataFilter(object sender)
        {
            if (this.OnDataFilter != null)
            {
                this.OnDataFilter(sender);
            }
        }

        private T AddControlToTabPage<T>(TabPage tabPage) where T : UserControl
        {
            T control = (T)Activator.CreateInstance(typeof(T));
            control.Dock = DockStyle.Fill;
            tabPage.Controls.Add(control);

            return control;
        }

        private T GetUcControl<T>(TabPage tabPage) where T : UserControl
        {
            foreach (Control control in tabPage.Controls)
            {
                if (control is T)
                {
                    return control as T;
                }
            }

            return null;
        }

        public TabPage FindTabPage(DatabaseObjectDisplayInfo displayInfo)
        {
            foreach (TabPage page in this.tabControl1.TabPages)
            {
                DatabaseObjectDisplayInfo data = page.Tag as DatabaseObjectDisplayInfo;

                if (data.Name == displayInfo.Name && displayInfo.DatabaseType == data.DatabaseType && displayInfo.DisplayType == data.DisplayType)
                {
                    return page;
                }
            }

            return null;
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= this.tabControl1.TabPages.Count)
            {
                return;
            }

            Bitmap imgClose = Resources.TabClose;

            int headerCloseButtonSize = imgClose.Size.Width;

            bool isSelected = e.Index == this.tabControl1.SelectedIndex;

            SolidBrush backBrush = new SolidBrush(isSelected ? Color.White : ColorTranslator.FromHtml("#DEE1E6"));

            Rectangle headerRect = tabControl1.GetTabRect(e.Index);

            e.Graphics.FillRectangle(backBrush, headerRect);

            Font font = new Font(this.Font, isSelected ? FontStyle.Bold : FontStyle.Regular);

            Brush fontBrush = new SolidBrush(Color.Black);

            e.Graphics.DrawString(this.tabControl1.TabPages[e.Index].Text.TrimEnd(), font, fontBrush, headerRect.X, headerRect.Y + 2);

            Rectangle closeButtonRect = new Rectangle(headerRect.X + headerRect.Width - headerCloseButtonSize, headerRect.Y + 2, headerCloseButtonSize, headerCloseButtonSize);

            e.Graphics.DrawImage(imgClose, closeButtonRect);

            if (!this.dictCloseButtonRectangle.ContainsKey(e.Index))
            {
                this.dictCloseButtonRectangle.Add(e.Index, closeButtonRect);
            }
            else
            {
                this.dictCloseButtonRectangle[e.Index] = closeButtonRect;
            }

            e.Graphics.Dispose();
        }

        private int FindTabPageIndex(int x, int y)
        {
            foreach (var kp in this.dictCloseButtonRectangle)
            {
                Rectangle rect = kp.Value;

                if (x >= rect.X && x <= rect.X + rect.Width
                 && y >= rect.Y && y <= rect.Y + rect.Height)
                {
                    return kp.Key;
                }
            }

            return -1;
        }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            int tabPageIndex = this.FindTabPageIndex(e.X, e.Y);

            if (tabPageIndex >= 0)
            {
                this.CloseTabPage(tabPageIndex);
            }
        }

        private void tabControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < this.tabControl1.TabPages.Count; i++)
                {
                    Rectangle r = this.tabControl1.GetTabRect(i);

                    if (r.Contains(e.Location))
                    {
                        this.tabControl1.SelectedIndex = i;

                        this.SetMenuItemsVisible();

                        this.scriptContentMenu.Show(this, e.Location);

                        break;
                    }
                }
            }
        }

        private void SetMenuItemsVisible()
        {
            this.tsmiCloseOthers.Visible = this.tabControl1.TabPages.Count > 1;
            this.tsmiCloseAll.Visible = this.tabControl1.TabPages.Count > 1;
        }

        private async void tsmiClose_Click(object sender, EventArgs e)
        {
            if (this.tabControl1.SelectedIndex >= 0)
            {
                await this.CloseTabPage(this.tabControl1.SelectedIndex);
            }

            this.SetControlVisible();
        }

        private async Task<bool> CloseTabPage(int tabPageIndex)
        {
            bool canClose = true;

            TabPage tabPage = this.tabControl1.TabPages[tabPageIndex];

            DatabaseObjectDisplayInfo info = tabPage.Tag as DatabaseObjectDisplayInfo;

            bool isNew = info.IsNew;

            if (info != null)
            {
                IDbObjContentDisplayer control = this.GetUcControlInterface(tabPage);

                bool saveRequired = false;

                if (control is UC_SqlQuery sqlQuery)
                {
                    if (isNew)
                    {
                        if (sqlQuery.Editor.Text.Trim().Length > 0)
                        {
                            saveRequired = true;
                        }
                    }
                    else
                    {
                        if (sqlQuery.IsTextChanged())
                        {
                            saveRequired = true;
                        }
                    }
                }
                else if (control is UC_TableDesigner tableDesigner)
                {
                    if (await tableDesigner.IsChanged())
                    {
                        saveRequired = true;
                    }
                }

                if (saveRequired)
                {
                    DialogResult result = MessageBox.Show($"Do you want to save {info.Name}?", "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        this.Save();
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        canClose = false;
                    }
                }
            }

            if (canClose)
            {
                if (info.DisplayType == DatabaseObjectDisplayType.Script)
                {
                    var sqlQueryControl = this.GetUcControlInterface(tabPage) as UC_SqlQuery;

                    sqlQueryControl.DisposeResources();
                }

                this.tabControl1.TabPages.RemoveAt(tabPageIndex);
                this.dictCloseButtonRectangle.Remove(tabPageIndex);
            }

            this.SetControlVisible();

            return canClose;
        }

        private async void tsmiCloseOthers_Click(object sender, EventArgs e)
        {
            this.dictCloseButtonRectangle.Clear();

            int index = this.tabControl1.SelectedIndex;

            for (int i = this.tabControl1.TabPages.Count - 1; i >= index + 1; i--)
            {
                await this.CloseTabPage(i);
            }

            while (this.tabControl1.TabPages.Count > 1)
            {
                await this.CloseTabPage(0);
            }

            this.SetControlVisible();
        }

        private void SetControlVisible()
        {
            this.Visible = this.tabControl1.TabPages.Count > 0;
        }

        private async void tsmiCloseAll_Click(object sender, EventArgs e)
        {
            for (int i = this.tabControl1.TabCount - 1; i >= 0; i--)
            {
                await this.CloseTabPage(i);
            }

            this.SetControlVisible();
        }

        private void tsmiSave_Click(object sender, EventArgs e)
        {
            this.Save();
        }

        private void Save()
        {
            if (this.tabControl1.TabCount < 1)
            {
                return;
            }

            TabPage tabPage = this.tabControl1.SelectedTab;

            if (tabPage == null)
            {
                return;
            }

            DatabaseObjectDisplayInfo displayInfo = tabPage.Tag as DatabaseObjectDisplayInfo;

            if (displayInfo == null)
            {
                return;
            }

            DatabaseObjectDisplayType displayType = displayInfo.DisplayType;

            if (displayType == DatabaseObjectDisplayType.Script || displayType == DatabaseObjectDisplayType.Data)
            {
                if (File.Exists(displayInfo.FilePath))
                {
                    this.SaveToFile(tabPage, displayInfo.FilePath);
                    return;
                }

                if (this.dlgSave == null)
                {
                    this.dlgSave = new SaveFileDialog();
                }

                this.dlgSave.FileName = tabPage.Text.Trim();

                if (displayType == DatabaseObjectDisplayType.Script)
                {
                    this.dlgSave.Filter = "sql file|*.sql|txt file|*.txt";
                }
                else if (displayType == DatabaseObjectDisplayType.Data)
                {
                    this.dlgSave.Filter = "csv file|*.csv|txt file|*.txt";
                }

                DialogResult result = this.dlgSave.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string filePath = this.dlgSave.FileName;

                    this.SaveToFile(tabPage, filePath);

                    displayInfo.FilePath = filePath;

                    string name = Path.GetFileNameWithoutExtension(filePath);

                    displayInfo.IsNew = false;
                    displayInfo.Name = name;

                    tabPage.Text = this.GetFormatTabHeaderText(name);

                    this.SetTabPageTooltip(tabPage);
                }
            }
            else if (displayType == DatabaseObjectDisplayType.TableDesigner)
            {
                UC_TableDesigner tableDesigner = this.GetUcControl<UC_TableDesigner>(tabPage);
                ContentSaveResult result = tableDesigner.Save(new ContentSaveInfo() { });

                if (result.IsOK)
                {
                    Table table = result.ResultData as Table;

                    displayInfo.IsNew = false;
                    displayInfo.Name = table.Name;

                    tabPage.Text = this.GetFormatTabHeaderText(this.GetInfoName(displayInfo));

                    this.SetTabPageTooltip(tabPage);
                }
            }
        }

        private void SaveToFile(TabPage tabPage, string filePath)
        {
            IDbObjContentDisplayer control = this.GetUcControlInterface(tabPage);

            if (control != null)
            {
                control.Save(new ContentSaveInfo() { FilePath = filePath });
            }
        }

        private IDbObjContentDisplayer GetUcControlInterface(TabPage tabPage)
        {
            foreach (Control control in tabPage.Controls)
            {
                if (control is IDbObjContentDisplayer)
                {
                    return control as IDbObjContentDisplayer;
                }
            }

            return null;
        }

        private string GetInfoName(DatabaseObjectDisplayInfo info)
        {
            string name = info.Name;
            bool isOpenFile = !string.IsNullOrEmpty(info.FilePath);

            if (isOpenFile)
            {
                return name;
            }

            if (string.IsNullOrEmpty(name))
            {
                if (info.IsNew)
                {
                    string prefix = "";

                    if (info.DisplayType == DatabaseObjectDisplayType.Script)
                    {
                        prefix = "SQLQuery";
                    }
                    else if (info.DisplayType == DatabaseObjectDisplayType.TableDesigner)
                    {
                        prefix = "Table";
                    }

                    int num = this.GetNewMaxNameNumber(prefix);

                    name = prefix + (num + 1);

                    info.Name = name;
                }
            }
            else
            {
                if (info.DatabaseType == DatabaseType.SqlServer || info.DatabaseType == DatabaseType.Postgres)
                {
                    var dbObject = info.DatabaseObject;

                    if (dbObject != null)
                    {
                        string schema = info.Schema ?? dbObject.Schema;

                        if(!string.IsNullOrEmpty(schema))
                        {
                            name = $"{schema}.{dbObject.Name}";
                        }
                        else
                        {
                            name = dbObject.Name;
                        }
                    }
                }
            }

            return name;
        }

        private int GetNewMaxNameNumber(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return 0;
            }

            List<string> names = new List<string>();
            foreach (TabPage page in this.tabControl1.TabPages)
            {
                DatabaseObjectDisplayInfo data = page.Tag as DatabaseObjectDisplayInfo;

                if (data.Name.Trim().StartsWith(prefix))
                {
                    names.Add(data.Name.Trim());
                }
            }

            string maxName = names.OrderByDescending(item => item.Length).ThenByDescending(item => item).FirstOrDefault();

            int num = 0;

            if (!string.IsNullOrEmpty(maxName))
            {
                string strNum = maxName.Replace(prefix, "");

                if (int.TryParse(strNum, out num))
                {
                }
            }

            return num;
        }

        public void RunScripts()
        {
            if (this.tabControl1.TabCount == 0)
            {
                return;
            }

            TabPage tabPage = this.tabControl1.SelectedTab;

            if (tabPage == null)
            {
                return;
            }

            DatabaseObjectDisplayInfo data = tabPage.Tag as DatabaseObjectDisplayInfo;

            if (data == null || data.DisplayType != DatabaseObjectDisplayType.Script)
            {
                return;
            }

            UC_SqlQuery sqlQuery = this.GetUcControl<UC_SqlQuery>(tabPage);

            sqlQuery.RunScripts(data);
        }

        private void tabControl1_MouseHover(object sender, EventArgs e)
        {
            TabPage tabPage = this.tabControl1.SelectedTab;

            if (tabPage != null)
            {
                this.SetTabPageTooltip(tabPage);
            }
        }

        private void Feedback(FeedbackInfo info)
        {
            if (this.OnFeedback != null)
            {
                this.OnFeedback(info);
            }
        }
    }
}
