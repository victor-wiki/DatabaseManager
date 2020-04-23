using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DatabaseManager.Core;
using DatabaseManager.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Helper;
using DatabaseInterpreter.Core;

namespace DatabaseManager.Controls
{
    public partial class UC_ScriptEditor : UserControl, IDbObjContentDisplayer, IObserver<FeedbackInfo>
    {
        private DatabaseObjectDisplayInfo displayInfo;
        private ScriptRunner scriptRunner;

        public UC_ScriptEditor()
        {
            InitializeComponent();

            this.SetResultPanelVisible(false);             
        }   

        public RichTextBox Editor => this.txtEditor;

        public void Show(DatabaseObjectDisplayInfo displayInfo)
        {
            this.displayInfo = displayInfo;

            this.txtEditor.Clear();

            if (!string.IsNullOrEmpty(displayInfo.Content))
            {
                this.txtEditor.AppendText(displayInfo.Content);
            }
        }

        public void Save(string filePath)
        {
            File.WriteAllText(filePath, this.txtEditor.Text);
        }

        private void tsmiCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(this.txtEditor.SelectedText);
        }

        private void txtEditor_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.tsmiCopy.Enabled = this.txtEditor.SelectionLength > 0;

                this.editorContexMenu.Show(this.txtEditor, e.Location);
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

                    if (dataTable != null && dataTable.Rows.Count > 0)
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
                        bool appended = false;

                        if (result.Result is int affectedRows)
                        {
                            if (affectedRows >= 0)
                            {
                                appended = true;

                                this.AppendMessage($"{affectedRows} row(s) affected.");
                            }
                        }

                        if(!appended)
                        {
                            this.AppendMessage("command executed.");
                        }                       
                    }
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
            string script = this.Editor.SelectionLength > 0 ? this.Editor.SelectedText : this.Editor.Text;

            if (script.Trim().Length == 0)
            {
                return;
            }         

            this.ClearResults();           

            this.scriptRunner = new ScriptRunner() { DelimiterRelaceChars = "\r" };
            this.scriptRunner.Subscribe(this);

            QueryResult result = await scriptRunner.Run(data.DatabaseType, data.ConnectionInfo, script);

            this.ShowResult(result);
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

        private void txtEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                if (FormEventCenter.OnRunScripts != null)
                {
                    FormEventCenter.OnRunScripts();
                }
            }
        }

        private void tsmiPaste_Click(object sender, EventArgs e)
        {
            this.txtEditor.Paste();
        }

        private void txtEditor_MouseClick(object sender, MouseEventArgs e)
        {
            this.ShowCurrentPosition();
        }

        private void SetResultPanelVisible(bool visible)
        {
            this.splitContainer1.Panel2Collapsed = !visible;
            this.splitContainer1.SplitterWidth = visible ? 3 : 1;
        }

        private void ShowCurrentPosition()
        {
            if (this.txtEditor.SelectionStart >= 0)
            {
                int lineIndex = this.txtEditor.GetLineFromCharIndex(this.txtEditor.SelectionStart);
                int column = this.txtEditor.SelectionStart - this.txtEditor.GetFirstCharIndexOfCurrentLine() + 1;

                this.tsslMessage.Text = $"Line:{lineIndex + 1}  Column:{column}";
            }
            else
            {
                this.tsslMessage.Text = "";
            }
        }

        private void txtEditor_KeyUp(object sender, KeyEventArgs e)
        {
            this.ShowCurrentPosition();
        }
    }
}
