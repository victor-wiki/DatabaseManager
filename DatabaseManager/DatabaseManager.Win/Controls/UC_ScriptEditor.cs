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

namespace DatabaseManager.Controls
{
    public partial class UC_ScriptEditor : UserControl, IDbObjContentDisplayer, IObserver<FeedbackInfo>
    {
        private ScriptRunner scriptRunner;     
        private RichTextBox resultTextbox;
        private UC_DataGridView resultGridView;

        public UC_ScriptEditor()
        {
            InitializeComponent();
        }

        public RichTextBox Editor => this.txtEditor;

        public void Show(DatabaseObjectDisplayInfo displayInfo)
        {
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

            this.SetControlVisible(result.ResultType);

            if (result.HasError)
            {
                this.CheckCreateResultTextBox();
                this.resultTextbox.Clear();

                this.AppendMessage(result.Result?.ToString(), true);
            }
            else
            {
                if (result.ResultType == QueryResultType.Grid)
                {
                    this.CheckCreateDataGridView();

                    this.resultGridView.LoadData(result.Result as DataTable);
                }
                else if (result.ResultType == QueryResultType.Text)
                {
                    this.CheckCreateResultTextBox();
                    this.resultTextbox.Clear();

                    this.AppendMessage("command executed.");
                }               
            }

            this.splitContainer1.Panel2Collapsed = false;
        }

        public async void RunScripts(DatabaseObjectDisplayInfo data)
        {
            string script = this.Editor.SelectionLength > 0 ? this.Editor.SelectedText : this.Editor.Text;

            if (script.Trim().Length == 0)
            {
                return;
            }

            if (this.resultTextbox != null)
            {
                this.resultTextbox.Clear();
            }

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

        private void CheckCreateResultTextBox()
        {
            if (this.resultTextbox == null)
            {
                this.resultTextbox = new RichTextBox();
                this.resultTextbox.Dock = DockStyle.Fill;

                this.splitContainer1.Panel2.Controls.Add(this.resultTextbox);
            }
        }

        private void CheckCreateDataGridView()
        {
            if (this.resultGridView == null)
            {
                this.resultGridView = new UC_DataGridView();
                this.resultGridView.Dock = DockStyle.Fill;

                this.splitContainer1.Panel2.Controls.Add(this.resultGridView);
            }
        }

        private void SetControlVisible(QueryResultType resultType)
        {
            if (this.resultTextbox != null)
            {
                this.resultTextbox.Visible = resultType == QueryResultType.Text;
            }

            if (this.resultGridView != null)
            {
                this.resultGridView.Visible = resultType == QueryResultType.Grid;
            }
        }

        private void AppendMessage(string message, bool isError = false)
        {
            this.CheckCreateResultTextBox();

            RichTextBoxHelper.AppendMessage(this.resultTextbox, message, isError, false);

            this.splitContainer1.Panel2Collapsed = false;
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
    }
}
