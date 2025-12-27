using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Data;
using DatabaseManager.Forms;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using SqlCodeEditor;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_SqlQuery : UserControl, IDbObjContentDisplayer, IObserver<FeedbackInfo>
    {
        private DatabaseObjectDisplayInfo displayInfo;
        private ScriptRunner scriptRunner;
        private bool readOnly;
        private bool showEditorMessage = true;
        private string originalText = string.Empty;
        private bool isResultReturned = false;
        private CancellationTokenSource cancellationTokenSource;

        internal DatabaseObjectDisplayInfo DisplayInfo => this.displayInfo;

        internal UC_QueryEditor QueryEditor => this.queryEditor;


        public bool ReadOnly
        {
            get { return this.readOnly; }
            set
            {
                this.readOnly = value;
                this.Editor.IsReadOnly = value;
                this.Editor.BackColor = Color.White;
            }
        }

        public int SplitterDistance
        {
            get { return this.splitContainer1.SplitterDistance; }
            set { this.splitContainer1.SplitterDistance = value; }
        }

        public bool ShowEditorMessage
        {
            get { return this.showEditorMessage; }
            set
            {
                this.showEditorMessage = value;
                this.statusStrip1.Visible = value;
            }
        }

        public UC_SqlQuery()
        {
            InitializeComponent();

            UC_QueryEditor.CheckForIllegalCrossThreadCalls = false;
            TextEditorControlEx.CheckForIllegalCrossThreadCalls = false;
            TabControl.CheckForIllegalCrossThreadCalls = false;
            TabPage.CheckForIllegalCrossThreadCalls = false;
            StatusStrip.CheckForIllegalCrossThreadCalls = false;

            this.SetResultPanelVisible(false);
        }

        private void queryEditor_Load(object sender, EventArgs e)
        {
            if (this.showEditorMessage)
            {
                this.queryEditor.OnQueryEditorInfoMessage += this.ShowEditorInfoMessage;
            }
            else
            {
                this.splitContainer1.Height += this.statusStrip1.Height;
            }

            if (!this.readOnly)
            {
                this.queryEditor.SetupIntellisenseRequired += QueryEditor_SetupIntellisenseRequired;
            }
        }

        private void QueryEditor_SetupIntellisenseRequired(object sender, EventArgs e)
        {
            this.SetupIntellisence();
        }

        public TextEditorControlEx Editor => this.queryEditor.Editor;

        private void ShowEditorInfoMessage(string message)
        {
            this.tsslMessage.Text = message;
        }

        public void Show(DatabaseObjectDisplayInfo displayInfo)
        {
            this.displayInfo = displayInfo;

            if (this.Editor.Text.Length > 0)
            {
                this.Editor.ResetText();
            }

            if (!string.IsNullOrEmpty(displayInfo.Content))
            {
                this.Editor.Text = displayInfo.Content;
            }
            else if (File.Exists(displayInfo.FilePath))
            {
                this.Editor.Text = File.ReadAllText(displayInfo.FilePath);
            }

            this.queryEditor.DatabaseType = this.displayInfo.DatabaseType;

            this.queryEditor.Init();

            if (displayInfo.ConnectionInfo != null && !string.IsNullOrEmpty(displayInfo.ConnectionInfo.Database))
            {
                DbInterpreter dbInterpreter = this.GetDbInterpreter();

                if (dbInterpreter.BuiltinDatabases.Any(item => item.ToUpper() == this.displayInfo.ConnectionInfo.Database.ToUpper()))
                {
                    return;
                }

                if (SettingManager.Setting.EnableEditorIntellisence)
                {
                    this.SetupIntellisence();
                }
            }

            this.originalText = this.Editor.Text;
        }

        private DbInterpreter GetDbInterpreter()
        {
            DbInterpreterOption option = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple };

            return DbInterpreterHelper.GetDbInterpreter(this.displayInfo.DatabaseType, this.displayInfo.ConnectionInfo, option);
        }

        private async void SetupIntellisence()
        {
            if (this.CheckConnection())
            {
                try
                {
                    DbInterpreter dbInterpreter = this.GetDbInterpreter();

                    this.queryEditor.DbInterpreter = dbInterpreter;

                    SchemaInfoFilter filter = new SchemaInfoFilter()
                    {
                        DatabaseObjectType = DatabaseObjectType.Table
                        | DatabaseObjectType.Function
                        | DatabaseObjectType.View
                        | DatabaseObjectType.Column
                    };

                    SchemaInfo schemaInfo = await dbInterpreter.GetSchemaInfoAsync(filter);

                    var viewColumns = await dbInterpreter.GetTableColumnsAsync(new SchemaInfoFilter() { ColumnType = ColumnType.ViewColumn });

                    schemaInfo.TableColumns.AddRange(viewColumns);

                    DataStore.SetSchemaInfo(this.displayInfo.DatabaseType, schemaInfo);

                    this.queryEditor.SetupIntellisence();
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ExceptionHelper.GetExceptionDetails(ex));
                }
            }
        }

        public void RunScripts(DatabaseObjectDisplayInfo data)
        {
            Task.Run(() =>
            {
                this.Execute(data);
            });
        }

        private async void Execute(DatabaseObjectDisplayInfo data)
        {
            this.isResultReturned = false;
            this.displayInfo = data;

            string script = this.Editor.SelectedText.Length > 0 ? this.Editor.SelectedText : this.Editor.Text;

            if (script.Trim().Length == 0)
            {
                return;
            }

            this.ClearResults();

            this.scriptRunner = new ScriptRunner();
            this.scriptRunner.Subscribe(this);

            if (this.CheckConnection())
            {
                this.cancellationTokenSource = new CancellationTokenSource();

                var token = this.cancellationTokenSource.Token;

                token.Register(async () => { await this.Feedback(new FeedbackInfo() { Message = "Task has been canceled." }); });

                this.Invoke(() =>
                {
                    this.SetResultPanelVisible(true);
                    this.tabResult.SelectedIndex = 0;
                });

                this.loadingPanel.CancellationTokenSource = this.cancellationTokenSource;

                this.loadingPanel.ShowLoading(this.resultGridView);

                QueryResult result = await scriptRunner.Run(data.DatabaseType, data.ConnectionInfo, script, token, data.ScriptAction, data.ScriptParameters);

                this.isResultReturned = true;

                this.Invoke(() =>
                {
                    this.ShowResult(result);
                });

                this.loadingPanel.HideLoading();
            }
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
                        if (!(this.displayInfo.ScriptAction == ScriptAction.CREATE || this.displayInfo.ScriptAction == ScriptAction.ALTER))
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

            this.Invalidate();
        }

        private bool CheckConnection()
        {
            if (this.displayInfo.ConnectionInfo == null)
            {
                frmDbConnect dbConnect = new frmDbConnect(this.displayInfo.DatabaseType) { IsOnlyForSelectDatabase = true };

                if (dbConnect.ShowDialog() == DialogResult.OK)
                {
                    this.displayInfo.ConnectionInfo = dbConnect.ConnectionInfo;

                    if (SettingManager.Setting.EnableEditorIntellisence)
                    {
                        this.SetupIntellisence();
                    }
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

        public ContentSaveResult Save(ContentSaveInfo info)
        {
            string text = this.Editor.Text;

            File.WriteAllText(info.FilePath, text);

            this.originalText = text;

            return new ContentSaveResult() { IsOK = true };
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

        private async Task Feedback(FeedbackInfo info)
        {
            if (info.InfoType == FeedbackInfoType.Error)
            {
                if (!info.IgnoreError)
                {
                    if (this.scriptRunner != null)
                    {
                        if (this.cancellationTokenSource != null)
                        {
                            await this.cancellationTokenSource.CancelAsync();
                        }
                    }
                }

                this.AppendMessage(info.Message, true);

                this.tabResult.SelectedIndex = 1;
            }
            else
            {
                if (this.isResultReturned == false)
                {
                    this.tabResult.SelectedIndex = 1;
                }

                this.AppendMessage(info.Message, false);
            }
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

        internal bool IsTextChanged()
        {
            return this.originalText != this.Editor.Text;
        }

        internal void ValidateScripts()
        {
            this.queryEditor.ValidateScripts();
        }

        internal void DisposeResources()
        {
            if (this.queryEditor != null)
            {
                this.queryEditor.DisposeResources();
            }
        }   

        private void resultGridView_SizeChanged(object sender, EventArgs e)
        {
            this.loadingPanel.RefreshStatus();
        }
    }
}
