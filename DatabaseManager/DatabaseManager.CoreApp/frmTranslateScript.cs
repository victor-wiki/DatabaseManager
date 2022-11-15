using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Helper;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmTranslateScript : Form
    {
        private bool isPasting = false;
        private bool isHighlightingRequesting = false;

        public frmTranslateScript()
        {
            InitializeComponent();
        }

        private void frmTranslateScript_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            this.splitContainer1.SplitterDistance = (int)((this.splitContainer1.Width - this.splitContainer1.SplitterWidth) / 2);

            this.LoadDbTypes();
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
            Task.Run(() => this.Translate());
        }

        private void Translate()
        {
            string sourceDbTypeName = this.cboSourceDbType.Text;
            string targetDbTypeName = this.cboTargetDbType.Text;

            if (string.IsNullOrEmpty(sourceDbTypeName) || string.IsNullOrEmpty(targetDbTypeName))
            {
                MessageBox.Show("Please specify the source and target database type.");
                return;
            }

            if (sourceDbTypeName == targetDbTypeName)
            {
                MessageBox.Show("The source database type can't be same as target.");
                return;
            }

            string sourceScript = this.txtSource.Text.Trim();

            if (string.IsNullOrEmpty(sourceScript))
            {
                MessageBox.Show("The source script can't be empty.");
                return;
            }

            this.btnTranlate.Enabled = false;
            this.txtTarget.Clear();

            var sourceDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), sourceDbTypeName);
            var targetDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), targetDbTypeName);

            try
            {
                TranslateManager translateManager = new TranslateManager();

                TranslateResult result = translateManager.Translate(sourceDbType, targetDbType, sourceScript);

                string resultData = result.Data?.ToString();

                this.txtTarget.Text = resultData;

                if(result.HasError)
                {
                    MessageBox.Show((result.Error as SqlSyntaxError).ToString());
                    return;
                }
                else if (string.IsNullOrEmpty(resultData) && sourceScript.Length > 0)
                {
                    MessageBox.Show("The tanslate result is empty, please check whether the source database type is right.");
                    return;
                }                

                this.HighlightingRichTextBox(this.txtTarget, this.cboTargetDbType);
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

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void HighlightingRichTextBox(RichTextBox richTextBox, ComboBox comboBox)
        {
            if (!string.IsNullOrEmpty(comboBox.Text))
            {
                var dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), comboBox.Text);

                if (this.chkHighlighting.Checked)
                {
                    RichTextBoxHelper.Highlighting(richTextBox, dbType, false, null, null, true);
                }
            }
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
            if(this.isPasting)
            {
                this.HandlePaste();
            }
        }

        private void HandlePaste()
        {
            this.txtSource.SelectAll();
            this.txtSource.SelectionColor = Color.Black;
        }

        private void txtSource_SelectionChanged(object sender, EventArgs e)
        {
            if (this.isPasting || this.isHighlightingRequesting)
            {
                this.isPasting = false;
                this.isHighlightingRequesting = false;

                this.HighlightingRichTextBox(this.txtSource, this.cboSourceDbType);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.txtSource.Text = "";
            this.txtTarget.Text = "";
        }

        private void txtSource_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.tsmiPaste.Visible = this.txtSource.Text.Trim().Length == 0 || this.txtSource.SelectionLength == this.txtSource.Text.Length;

                this.sourceContextMenuStrip.Show(this.txtSource, e.Location);
            }
        }

        private void tsmiPaste_Click(object sender, EventArgs e)
        {
            var data = Clipboard.GetDataObject();

            if (data != null)
            {
                this.HandlePaste();

                this.txtSource.Text = data.GetData(DataFormats.UnicodeText)?.ToString();

                this.isPasting = true;
                this.txtSource.SelectAll();
                this.txtSource.Select(0, 0);
            }
        }

        private void tsmiCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtTarget.Text))
            {
                Clipboard.SetDataObject(this.txtTarget.Text);
            }
        }

        private void txtTarget_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.tsmiCopy.Visible = this.txtTarget.Text.Length > 0;

                this.targetContextMenuStrip.Show(this.txtTarget, e.Location);
            }
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
            if (this.chkHighlighting.Checked)
            {
                this.HighlightingRichTextBox(this.txtSource, this.cboSourceDbType);
                this.HighlightingRichTextBox(this.txtTarget, this.cboTargetDbType);
            }
        }        
    }
}
