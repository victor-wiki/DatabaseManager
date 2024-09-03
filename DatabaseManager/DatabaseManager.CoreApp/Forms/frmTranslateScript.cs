using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Helper;
using SqlAnalyser.Model;
using SqlCodeEditor;
using SqlCodeEditor.Document;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmTranslateScript : Form
    {
        private bool isPasting = false;
        private delegate void AddNodeDelegate();

        public frmTranslateScript()
        {
            InitializeComponent();

            this.InitControls();
        }

        private void frmTranslateScript_Load(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmiValidateScripts = new ToolStripMenuItem("Validate Scripts") { Name = "tsmiValidateScripts" };
            tsmiValidateScripts.Click += this.tsmiValidateScripts_Click;
            this.txtTarget.ActiveTextAreaControl.ContextMenuStrip.Items.Add(tsmiValidateScripts);
            this.txtTarget.ActiveTextAreaControl.ContextMenuStrip.Opening += this.targetContextMenuStrip_Opening;
        }

        private void InitControls()
        {
            (this.txtSource.Document.TextBufferStrategy as GapTextBufferStrategy).CheckTread = false;
            (this.txtTarget.Document.TextBufferStrategy as GapTextBufferStrategy).CheckTread = false;           

            var option = SettingManager.Setting.TextEditorOption;

            TextEditorHelper.ApplySetting(this.txtSource, option);
            TextEditorHelper.ApplySetting(this.txtTarget, option);

            this.splitContainer1.SplitterDistance = (int)((this.splitContainer1.Width - this.splitContainer1.SplitterWidth) / 2);

            this.LoadDbTypes();

            if (SettingManager.Setting.ValidateScriptsAfterTranslated)
            {
                this.chkValidateScriptsAfterTranslated.Checked = true;
            }

            this.txtSource.SyntaxHighlighting = string.Empty;
            this.txtTarget.SyntaxHighlighting = string.Empty;           
        }

        public void LoadDbTypes()
        {
            var databaseTypes = DbInterpreterHelper.GetDisplayDatabaseTypes();

            foreach (var value in databaseTypes)
            {
                this.cboSourceDbType.Items.Add(value.ToString());
                this.cboTargetDbType.Items.Add(value.ToString());
            }
        }

        private void btnTranlate_Click(object sender, EventArgs e)
        {
            this.StartTranslate();
        }

        private void StartTranslate()
        {
            if (this.ValidateInputs())
            {
                Task.Run(() => this.Translate());
            }
        }

        private bool ValidateInputs()
        {
            string sourceDbTypeName = this.cboSourceDbType.Text;
            string targetDbTypeName = this.cboTargetDbType.Text;

            if (string.IsNullOrEmpty(sourceDbTypeName) || string.IsNullOrEmpty(targetDbTypeName))
            {
                MessageBox.Show("Please specify the source and target database type.");
                return false;
            }

            if (sourceDbTypeName == targetDbTypeName)
            {
                MessageBox.Show("The source database type can't be same as target.");
                return false;
            }

            string sourceScript = this.txtSource.Text.Trim();

            if (string.IsNullOrEmpty(sourceScript))
            {
                MessageBox.Show("The source script can't be empty.");
                return false;
            }

            return true;
        }

        private void Translate()
        {
            string sourceDbTypeName = this.cboSourceDbType.Text;
            string targetDbTypeName = this.cboTargetDbType.Text;
            string sourceScript = this.txtSource.Text.Trim();

            this.btnTranlate.Enabled = false;
            this.ClearSelection(this.txtSource);
            this.txtTarget.Text = "";

            var sourceDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), sourceDbTypeName);
            var targetDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), targetDbTypeName);

            try
            {
                TranslateManager translateManager = new TranslateManager();

                TranslateResult result = translateManager.Translate(sourceDbType, targetDbType, sourceScript);

                string resultData = result.Data?.ToString();

                this.txtTarget.Text = resultData;

                if (result.HasError)
                {
                    frmTextContent msgBox = new frmTextContent("Error Message", result.Error.ToString(), true);
                    msgBox.ShowDialog();

                    TextEditorHelper.HighlightingError(this.txtSource, result.Error);

                    return;
                }
                else if (string.IsNullOrEmpty(resultData) && sourceScript.Length > 0)
                {
                    MessageBox.Show("The tanslate result is empty, please check whether the source database type is right.");
                    return;
                }

                if (this.chkValidateScriptsAfterTranslated.Checked)
                {
                    this.ValidateScripts(targetDbType);
                }

                this.SetHighlighting(this.txtTarget, this.cboTargetDbType.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHelper.GetExceptionDetails(ex));
            }
            finally
            {
                this.btnTranlate.Enabled = true;
            }
        }

        private async void ValidateScripts(DatabaseType databaseType, bool showMessageBox = false)
        {
            string script = this.txtTarget.Text.Trim();

            if (string.IsNullOrEmpty(script))
            {
                return;
            }

            SqlSyntaxError error = await Task.Run(() => ScriptValidator.ValidateSyntax(databaseType, script));

            if (error != null && error.HasError)
            {
                if (showMessageBox)
                {
                    frmTextContent msgBox = new frmTextContent("Error Message", error.ToString(), true);
                    msgBox.ShowDialog();
                }

                TextEditorHelper.ClearMarkers(this.txtTarget);

                TextEditorHelper.HighlightingError(this.txtTarget, error);
            }
            else
            {
                if (showMessageBox)
                {
                    TextEditorHelper.ClearMarkers(this.txtTarget);

                    MessageBox.Show("The scripts is valid.");
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private DatabaseType GetDatabaseType(string type)
        {
            if (!string.IsNullOrEmpty(type))
            {
                return (DatabaseType)Enum.Parse(typeof(DatabaseType), type);
            }

            return DatabaseType.Unknown;
        }

        private void txtSource_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                this.isPasting = true;
            }
        }

        private void txtSource_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.isPasting)
            {
                this.HandlePaste();
                this.isPasting = false;
            }
        }

        private void HandlePaste()
        {
            this.txtSource.ActiveTextAreaControl.TextArea.Focus();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.txtSource.Text = "";
            this.txtTarget.Text = "";

            this.txtSource.Refresh();
            this.txtTarget.Refresh();

            this.txtSource.Invalidate();
            this.txtTarget.Invalidate();
        }            

        private void btnExchange_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.cboSourceDbType.Text) && !string.IsNullOrEmpty(this.cboTargetDbType.Text))
            {
                string temp = this.cboSourceDbType.Text;
                this.cboSourceDbType.Text = this.cboTargetDbType.Text;
                this.cboTargetDbType.Text = temp;
            }
        }

        private void chkHighlighting_CheckedChanged(object sender, EventArgs e)
        {
            this.SetHighlighting(this.txtSource, this.cboSourceDbType.Text);
            this.SetHighlighting(this.txtTarget, this.cboTargetDbType.Text);            
        }

        private void frmTranslateScript_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                this.StartTranslate();
            }
        }

        private void ClearSelection(TextEditorControlEx txtEditor)
        {
            txtEditor.ActiveTextAreaControl.SelectionManager.ClearSelection();
        }

        private void tsmiValidateScripts_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.cboTargetDbType.Text))
            {
                return;
            }

            this.ClearSelection(this.txtTarget);

            var targetDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), this.cboTargetDbType.Text);

            this.ValidateScripts(targetDbType, true);
        }

        private void targetContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            foreach (ToolStripItem item in this.txtTarget.ActiveTextAreaControl.ContextMenuStrip.Items)
            {
                if (item.Name == "tsmiValidateScripts")
                {
                    bool hasText = !string.IsNullOrEmpty(this.cboTargetDbType.Text) && this.txtTarget.Text.Trim().Length > 0;
                    item.Visible = hasText;
                    break;
                }                
            }            
        }

        private void cboSourceDbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SetHighlighting(this.txtSource, this.cboSourceDbType.Text);
        }

        private void cboTargetDbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SetHighlighting(this.txtTarget, this.cboTargetDbType.Text);
        }

        private void SetHighlighting(TextEditorControlEx editor, string databaseType)
        {
            if (this.chkHighlighting.Checked)
            {
                TextEditorHelper.Highlighting(editor, this.GetDatabaseType(databaseType));
            }
            else
            {
                TextEditorHelper.ClearHighlighting(editor);
            }
        }
    }
}
