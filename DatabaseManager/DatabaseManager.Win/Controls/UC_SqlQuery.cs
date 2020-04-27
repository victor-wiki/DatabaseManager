using DatabaseInterpreter.Core;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using DatabaseInterpreter.Model;
using DatabaseManager.Data;

namespace DatabaseManager.Controls
{
    public partial class UC_SqlQuery : UserControl, IDbObjContentDisplayer, IObserver<FeedbackInfo>
    {
        private DatabaseObjectDisplayInfo displayInfo;
        private ScriptRunner scriptRunner;

        public UC_SqlQuery()
        {
            InitializeComponent();

            this.SetResultPanelVisible(false);
            this.queryEditor.OnQueryEditorInfoMessage += this.ShowEditorInfoMessage;
            this.queryEditor.SetupIntellisenseRequired += QueryEditor_SetupIntellisenseRequired;
        }

        private void QueryEditor_SetupIntellisenseRequired(object sender, EventArgs e)
        {
            this.SetupIntellisence();
        }

        public RichTextBox Editor => this.queryEditor.Editor;

        private void ShowEditorInfoMessage(string message)
        {
            this.tsslMessage.Text = message;
        }

        public void Show(DatabaseObjectDisplayInfo displayInfo)
        {
            this.displayInfo = displayInfo;

            this.Editor.Clear();

            if (!string.IsNullOrEmpty(displayInfo.Content))
            {
                this.Editor.AppendText(displayInfo.Content);
            }
            else if(File.Exists(displayInfo.FilePath))
            {
                this.Editor.AppendText(File.ReadAllText(displayInfo.FilePath));
            }

            if (displayInfo.IsNew)
            {
                this.SetupIntellisence();
            }
        }

        private async void SetupIntellisence()
        {
            this.queryEditor.DatabaseType = this.displayInfo.DatabaseType;

            if(this.CheckConnection())
            {
                DbInterpreterOption option = new DbInterpreterOption();

                DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.displayInfo.DatabaseType, this.displayInfo.ConnectionInfo, option);

                this.queryEditor.DbInterpreter = dbInterpreter;              

                SchemaInfoFilter filter = new SchemaInfoFilter()
                {
                    DatabaseObjectType = DatabaseObjectType.Table | DatabaseObjectType.View | DatabaseObjectType.TableColumn
                };

                SchemaInfo schemaInfo = await dbInterpreter.GetSchemaInfoAsync(filter);

                DataStore.SetSchemaInfo(this.displayInfo.DatabaseType, schemaInfo);

                this.queryEditor.SetupIntellisence();
            }           
        }

        public void Save(string filePath)
        {
            File.WriteAllText(filePath, this.Editor.Text);
        }

        public void ShowResult(QueryResult result)
        {
            if (result == null)
            {
                return;
            }

            if (result.DoNothing)
            {
                this.tabResult.SelectedIndex = 1;

                this.AppendMessage("Nothing can be done.");
            }
            else if (result.HasError)
            {
                this.tabResult.SelectedIndex = 1;

                this.AppendMessage(result.Result?.ToString(), true);
            }
            else
            {
                int selectedTabIndex = -1;

                if (result.ResultType == QueryResultType.Grid)
                {
                    DataTable dataTable = result.Result as DataTable;

                    if (dataTable != null)
                    {
                        selectedTabIndex = 0;

                        this.resultGridView.LoadData(dataTable);
                    }
                }
                else if (result.ResultType == QueryResultType.Text)
                {
                    selectedTabIndex = 1;

                    if (this.resultTextBox.Text.Length == 0)
                    {  
                        if(!(this.displayInfo.ScriptAction== ScriptAction.CREATE || this.displayInfo.ScriptAction == ScriptAction.ALTER))
                        {
                            if (result.Result is int affectedRows)
                            {
                                if (affectedRows >= 0)
                                {
                                    this.AppendMessage($"{affectedRows} row(s) affected.");
                                }
                            }
                        }                                           
                    }

                    this.AppendMessage("command executed.");
                }

                if (selectedTabIndex >= 0)
                {
                    this.tabResult.SelectedIndex = selectedTabIndex;
                }
            }

            this.SetResultPanelVisible(true);
        }

        public async void RunScripts(DatabaseObjectDisplayInfo data)
        {
            this.displayInfo = data;

            string script = this.Editor.SelectionLength > 0 ? this.Editor.SelectedText : this.Editor.Text;

            if (script.Trim().Length == 0)
            {
                return;
            }

            this.ClearResults();

            this.scriptRunner = new ScriptRunner() { DelimiterRelaceChars = "\r" };
            this.scriptRunner.Subscribe(this);

            if (this.CheckConnection())
            {
                QueryResult result = await scriptRunner.Run(data.DatabaseType, data.ConnectionInfo, script);

                this.ShowResult(result);
            }
        }

        private bool CheckConnection()
        {
            if (this.displayInfo.ConnectionInfo == null)
            {
                frmDbConnect dbConnect = new frmDbConnect(this.displayInfo.DatabaseType) { NotUseProfile = true };
                if (dbConnect.ShowDialog() == DialogResult.OK)
                {
                    this.displayInfo.ConnectionInfo = dbConnect.ConnectionInfo;
                }
            }

            if (this.displayInfo.ConnectionInfo != null && !string.IsNullOrEmpty(this.displayInfo.ConnectionInfo.Database))
            {
                return true;
            }
            else
            {                
                return false;
            }
        }

        #region IObserver<FeedbackInfo>
        public void OnNext(FeedbackInfo value)
        {
            this.Feedback(value);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
        #endregion

        private void Feedback(FeedbackInfo info)
        {
            this.Invoke(new Action(() =>
            {
                if (info.InfoType == FeedbackInfoType.Error)
                {
                    if (!info.IgnoreError)
                    {
                        if (this.scriptRunner != null && this.scriptRunner.IsBusy)
                        {
                            this.scriptRunner.Cancle();
                        }
                    }

                    this.AppendMessage(info.Message, true);
                }
                else
                {
                    this.AppendMessage(info.Message, false);
                }
            }));
        }

        private void ClearResults()
        {
            this.resultTextBox.Clear();
            this.resultGridView.ClearData();
        }

        private void AppendMessage(string message, bool isError = false)
        {
            RichTextBoxHelper.AppendMessage(this.resultTextBox, message, isError, false);

            this.SetResultPanelVisible(true);
        }

        private void SetResultPanelVisible(bool visible)
        {
            this.splitContainer1.Panel2Collapsed = !visible;
            this.splitContainer1.SplitterWidth = visible ? 3 : 1;
        }
    }
}
