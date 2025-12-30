using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Core.Model;
using DatabaseManager.Helper;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseManager.Controls
{
    public delegate void OnContentSavedHandler(string title, string tip);

    public partial class UC_DbObjectContent : UserControl
    {
        private DatabaseObjectDisplayInfo info;
        private IDbObjContentDisplayer control;
        private Dictionary<int, Rectangle> dictCloseButtonRectangle = new Dictionary<int, Rectangle>();

        public DataFilterHandler OnDataFilter;
        public FeedbackHandler OnFeedback;
        public OnContentSavedHandler OnContentSaved;

        internal DatabaseObjectDisplayInfo DisplayInfo => this.info;

        internal IEnumerable<IDockContent> ExistingContents { get; set; }

        public UC_DbObjectContent()
        {
            InitializeComponent();

            TabControl.CheckForIllegalCrossThreadCalls = false;
            TabPage.CheckForIllegalCrossThreadCalls = false;
        }

        public void ShowContent(DatabaseObjectDisplayInfo info)
        {
            this.info = info;

            if (info.DisplayType == DatabaseObjectDisplayType.Script)
            {
                UC_SqlQuery sqlQuery = new UC_SqlQuery();
                this.control = sqlQuery;

                this.AddControl(sqlQuery);

                sqlQuery.Show(info);

                if (!string.IsNullOrEmpty(sqlQuery.Editor.Text))
                {
                    if (info.Error != null)
                    {
                        TextEditorHelper.HighlightingError(sqlQuery.QueryEditor.Editor, info.Error);
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

                sqlQuery.QueryEditor.OnRunScripts += this.RunScripts;
            }
            else if (info.DisplayType == DatabaseObjectDisplayType.ViewData)
            {
                UC_DataViewer dataViewer = new UC_DataViewer();
                this.control = dataViewer;

                dataViewer.OnDataFilter += this.DataFilter;

                dataViewer.Show(info);

                this.AddControl(dataViewer);
            }
            else if (info.DisplayType == DatabaseObjectDisplayType.EditData)
            {
                UC_DataEditor dataEditor = new UC_DataEditor();
                this.control = dataEditor;

                dataEditor.OnDataFilter += this.DataFilter;

                dataEditor.Show(info);

                this.AddControl(dataEditor);
            }
            else if (info.DisplayType == DatabaseObjectDisplayType.TableDesigner)
            {
                UC_TableDesigner tableDesigner = new UC_TableDesigner();
                this.control = tableDesigner;

                tableDesigner.OnFeedback += this.Feedback;

                tableDesigner.Show(info);

                this.AddControl(tableDesigner);
            }
        }

        private void AddControl(Control control)
        {
            control.Dock = DockStyle.Fill;
            this.Controls.Add(control);
        }

        public string GetTooltip(DatabaseObjectDisplayInfo info)
        {
            string database = info.ConnectionInfo == null ? "" : $@": {info.ConnectionInfo?.Server}-{info.ConnectionInfo?.Database}";

            string filePath = File.Exists(info.FilePath) ? (info.FilePath + "  -  ") : "";

            string title = string.IsNullOrEmpty(filePath) ? $" - {this.GetInfoName(info)}" : "";

            string tooltip = $@"{filePath}{(info.DatabaseType == DatabaseType.Unknown ? "" : info.DatabaseType.ToString())}{database}{title}";

            return tooltip.TrimEnd('-',' ');
        }

        private void DataFilter(object sender)
        {
            if (this.OnDataFilter != null)
            {
                this.OnDataFilter(sender);
            }
        }

        internal ContentSaveResult Save()
        {
            ContentSaveResult saveResult = new ContentSaveResult() { IsOK = false };

            if (this.info == null)
            {
                return saveResult;
            }

            DatabaseObjectDisplayType displayType = this.info.DisplayType;

            if (displayType == DatabaseObjectDisplayType.Script || displayType == DatabaseObjectDisplayType.ViewData)
            {
                if (File.Exists(info.FilePath))
                {
                    this.SaveToFile(info.FilePath);

                    saveResult.IsOK = true;
                    saveResult.ResultData = info.FilePath;

                    return saveResult;
                }

                if (this.dlgSave == null)
                {
                    this.dlgSave = new SaveFileDialog();
                }

                this.dlgSave.FileName = this.GetInfoName(info).Trim();

                if (displayType == DatabaseObjectDisplayType.Script)
                {
                    this.dlgSave.Filter = "sql file|*.sql|txt file|*.txt";
                }
                else if (displayType == DatabaseObjectDisplayType.ViewData)
                {
                    this.dlgSave.Filter = "csv file|*.csv|txt file|*.txt";
                }

                DialogResult result = this.dlgSave.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string filePath = this.dlgSave.FileName;

                    this.SaveToFile(filePath);

                    this.info.FilePath = filePath;

                    string name = Path.GetFileNameWithoutExtension(filePath);

                    this.info.IsNew = false;
                    this.info.Name = name;

                    this.AfterSaved();

                    saveResult.IsOK = true;
                }
                else
                {
                    return saveResult;
                }
            }
            else if (displayType == DatabaseObjectDisplayType.EditData)
            {
                UC_DataEditor dataEditor = this.control as UC_DataEditor;

                ContentSaveResult result = dataEditor.Save(new ContentSaveInfo());

                if (!result.IsOK)
                {
                    this.Feedback(new FeedbackInfo() { InfoType = FeedbackInfoType.Error, Message = result.Message });
                }

                saveResult.IsOK = result.IsOK;
                saveResult.ResultData = result.ResultData;

                return saveResult;
            }
            else if (displayType == DatabaseObjectDisplayType.TableDesigner)
            {
                UC_TableDesigner tableDesigner = this.control as UC_TableDesigner;
                ContentSaveResult result = tableDesigner.Save(new ContentSaveInfo() { });

                if (result.IsOK)
                {
                    Table table = result.ResultData as Table;

                    this.info.IsNew = false;
                    this.info.Name = table.Name;

                    this.AfterSaved();

                    saveResult.IsOK = true;
                }
                else
                {
                    if (result.InfoType == ContentSaveResultInfoType.Error)
                    {
                        this.Feedback(new FeedbackInfo() { InfoType = FeedbackInfoType.Error, Message = result.Message });
                    }
                    else
                    {
                        this.Feedback(new FeedbackInfo() { InfoType = FeedbackInfoType.Info, Message = result.Message });
                    }

                    return saveResult;
                }
            }

            return saveResult;
        }

        private void AfterSaved()
        {
            if (this.info != null && this.OnContentSaved != null)
            {
                this.OnContentSaved(this.GetInfoName(this.info), this.GetTooltip(this.info));
            }
        }

        private void SaveToFile(string filePath)
        {
            if (this.control != null)
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

        public string GetInfoName(DatabaseObjectDisplayInfo info)
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

                        if (!string.IsNullOrEmpty(schema))
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

            if (info.DisplayType == DatabaseObjectDisplayType.Script && info.ScriptAction != ScriptAction.NONE)
            {
                return $"{name}({info.ScriptAction.ToString()})";
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

            if (this.ExistingContents != null)
            {
                foreach (IDockContent doc in this.ExistingContents)
                {
                    DatabaseObjectDisplayInfo data = (doc as Form).Tag as DatabaseObjectDisplayInfo;

                    if (data.Name.Trim().StartsWith(prefix))
                    {
                        names.Add(data.Name.Trim());
                    }
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

        internal void RunScripts()
        {
            DatabaseObjectDisplayInfo data = this.info;

            if (data == null || data.DisplayType != DatabaseObjectDisplayType.Script)
            {
                return;
            }

            UC_SqlQuery sqlQuery = this.control as UC_SqlQuery;

            sqlQuery.RunScripts(data);
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
