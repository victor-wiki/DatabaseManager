using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Core.Model;
using DatabaseManager.Data;
using DatabaseManager.Forms;
using DatabaseManager.Helper;
using FontAwesome.Sharp;
using SqlCodeEditor;
using System;
using System.Collections.Generic;
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
        private SelectScriptAnalyseResult selectScriptAnalyseResult;
        private frmExportData exportDataForm;

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
                this.lblMessage.Visible = value;
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

            this.tabProfiling.Parent = null;

            this.btnExport.Image = IconImageHelper.GetImage(IconChar.FileExport);

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
                this.splitContainer1.Height += this.lblMessage.Height;
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
            this.lblMessage.Text = message;
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

        public void RunScripts(DatabaseObjectDisplayInfo data, long pageNumber = 1)
        {
            Task.Run(() =>
            {
                this.Execute(data, pageNumber);
            });
        }

        private async void Execute(DatabaseObjectDisplayInfo data, long pageNumber = 1)
        {
            this.isResultReturned = false;
            this.displayInfo = data;

            string script = this.Editor.SelectedText.Length > 0 ? this.Editor.SelectedText : this.Editor.Text;

            if (script.Trim().Length == 0)
            {
                return;
            }

            this.ClearResults();

            this.scriptRunner = new ScriptRunner(new ScriptRunOption() { UseSqlParser = this.queryEditor.UseSqlParser, UseProfiler = this.queryEditor.UseProfiler });
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

                QueryResult result = await scriptRunner.Run(data.DatabaseType, data.ConnectionInfo, script, token, data.ScriptAction, data.ScriptParameters, new PaginationInfo() { PageNumber = this.pagination.PageNumber, PageSize = this.pagination.PageSize });

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

                this.pagination.Visible = false;

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

                        this.selectScriptAnalyseResult = result.SelectScriptAnalyseResult;

                        if (this.selectScriptAnalyseResult != null)
                        {
                            this.pagination.Visible = !this.selectScriptAnalyseResult.HasLimitCount
                                && (this.selectScriptAnalyseResult.HasTableName || this.selectScriptAnalyseResult.HasAlias);
                        }
                        else
                        {
                            this.pagination.Visible = false;
                        }

                        if (result.TotalCount > 0)
                        {
                            this.pagination.TotalCount = result.TotalCount.Value;
                        }

                        this.btnExport.Visible = dataTable.Rows.Count > 0;
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

                if (result.ProfilingResult != null && result.ProfilingResult.Details != null && result.ProfilingResult.Details.Count > 0)
                {
                    this.tabProfiling.Parent = this.tabResult;

                    this.ucProfilingResultGrid.LoadData(result.ProfilingResult);
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
                this.selectScriptAnalyseResult = null;
                this.exportDataForm = null;
            }
        }

        private void resultGridView_SizeChanged(object sender, EventArgs e)
        {
            this.loadingPanel.RefreshStatus();
        }

        private void pagination_OnPageNumberChanged(long pageNumber)
        {
            if (this.selectScriptAnalyseResult != null && this.selectScriptAnalyseResult.SelectStatement != null)
            {
                this.pagination.TotalCount = this.pagination.TotalCount;

                Task.Run(() => { this.LoadData(pageNumber); });
            }
            else
            {
                this.RunScripts(this.displayInfo, pageNumber);
            }
        }

        private async void LoadData(long pageNumber)
        {
            try
            {
                this.loadingPanel.ShowLoading(this.resultGridView);

                this.cancellationTokenSource = new CancellationTokenSource();

                var token = this.cancellationTokenSource.Token;

                DataTable dataTable = await ScriptRunner.GetPagedDatatable(this.GetDbInterpreter(), this.selectScriptAnalyseResult, new PaginationInfo() { PageNumber = pageNumber, PageSize = this.pagination.PageSize }, token);

                this.resultGridView.LoadData(dataTable);
            }
            catch (Exception ex)
            {
                this.tabResult.SelectedIndex = 1;

                this.Invoke(() =>
                {
                    this.AppendMessage(ex.Message, true);
                });                
            }
            finally
            {
                this.loadingPanel.HideLoading();
            }            
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            this.Export();
        }

        private void Export()
        {
            DataTable table = this.resultGridView.DataGrid.DataSource as DataTable;

            if (table == null)
            {
                MessageBox.Show("No data to export.");
                return;
            }

            long pageNumber = this.pagination.PageNumber;
            long pageCount = this.pagination.PageCount;

            ExportSpecificDataOption option = new ExportSpecificDataOption();

            option.PageNumbers.Add(pageNumber);
            option.PageCount = pageCount;

            DataTable dataTable = table.Clone();

            dataTable.TableName = "";

            this.exportDataForm = new frmExportData(dataTable, option);

            this.exportDataForm.ExportDataRequired += this.GetDataTableForExporting;

            this.exportDataForm.ShowDialog();
        }

        private async Task<DataTable> GetDataTableForExporting(ExportSpecificDataOption option, List<DataExportColumn> columns)
        {
            DataTable gridDataTable = (this.resultGridView.DataGrid.DataSource as DataTable);

            if (this.selectScriptAnalyseResult != null && this.selectScriptAnalyseResult.SelectStatement != null)
            {
                DataTable dataTable = gridDataTable.Clone();

                if (option.ExportAllThatMeetCondition)
                {
                    option.PageNumbers.Clear();

                    for (int i = 1; i <= option.PageCount; i++)
                    {
                        option.PageNumbers.Add(i);
                    }
                }

                List<long> pageNumbers = option.PageNumbers;

                long total = this.pagination.TotalCount;
                int pageSize = this.pagination.PageSize;

                this.cancellationTokenSource = new CancellationTokenSource();

                var token = this.cancellationTokenSource.Token;

                foreach (var pageNumber in pageNumbers)
                {
                    long count = pageNumber == option.PageCount ? total : pageNumber * pageSize;

                    if (this.exportDataForm != null)
                    {
                        this.exportDataForm.Feedback($"Reading data {count}/{total}...");
                    }

                    var table = await ScriptRunner.GetPagedDatatable(this.GetDbInterpreter(), this.selectScriptAnalyseResult, new PaginationInfo() { PageNumber = pageNumber, PageSize = pageSize }, token);

                    dataTable.Merge(table);
                }

                return dataTable;
            }
            else
            {
                return gridDataTable;
            }
        }
    }
}
