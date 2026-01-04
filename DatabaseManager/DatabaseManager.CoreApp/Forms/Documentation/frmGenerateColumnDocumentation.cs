using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Core.Model;
using DatabaseManager.FileUtility.Model;
using DatabaseManager.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmGenerateColumnDocumentation : Form, IObserver<FeedbackInfo>
    {
        private DatabaseType databaseType;
        private ConnectionInfo connectionInfo;
        private CancellationTokenSource cancellationTokenSource;
        private GenerateColumnDocumentationOption option = new GenerateColumnDocumentationOption();

        public DatabaseInterpreter.Utility.FeedbackHandler OnFeedback;

        public frmGenerateColumnDocumentation(DatabaseType databaseType, ConnectionInfo connectionInfo)
        {
            InitializeComponent();

            this.databaseType = databaseType;
            this.connectionInfo = connectionInfo;
        }

        private void Init()
        {
            var properties = Enum.GetNames(typeof(TableColumnProperty)).Where(item => item != nameof(TableColumnProperty.None));

            foreach (var property in properties)
            {
                int rowIndex = this.dgvProperties.Rows.Add();

                string title = ManagerUtil.GetDisplayTitle(property);

                this.dgvProperties.Rows[rowIndex].Cells[this.colSelect.Name].Value = true;
                this.dgvProperties.Rows[rowIndex].Cells[this.colPropertyName.Name].Value = title;
                this.dgvProperties.Rows[rowIndex].Cells[this.colDisplayName.Name].Value = title;

                this.dgvProperties.Rows[rowIndex].Cells[this.colSelect.Name].Tag = property;
            }

            this.dgvProperties.ClearSelection();

            this.dgvProperties.CurrentCell = null;

            if (!string.IsNullOrEmpty(this.option.GridColumnHeaderBackgroundColor))
            {
                string backgroundColor = this.option.GridColumnHeaderBackgroundColor;

                this.txtBackColor.Text = backgroundColor;
                this.txtBackColor.BackColor = ColorTranslator.FromHtml(backgroundColor);
            }

            if (!string.IsNullOrEmpty(this.option.GridColumnHeaderForegroundColor))
            {
                string foregroundColor = this.option.GridColumnHeaderForegroundColor;

                this.txtForeColor.Text = foregroundColor;
                this.txtForeColor.BackColor = ColorTranslator.FromHtml(foregroundColor);
            }
        }

        private void frmGenerateColumnDocumentation_Load(object sender, EventArgs e)
        {
            this.Init();

            this.AddHeaderCheckBox();
        }

        private void AddHeaderCheckBox()
        {
            CheckBox headerCheckBox = new CheckBox() { Checked = true };
            headerCheckBox.Size = new Size(15, 15);

            headerCheckBox.CheckedChanged += this.HeaderCheckBox_CheckedChanged;

            this.dgvProperties.Controls.Add(headerCheckBox);

            Rectangle headerCell = this.dgvProperties.GetCellDisplayRectangle(0, -1, true);
            headerCheckBox.Location = new Point(
            headerCell.Location.X + (headerCell.Width - headerCheckBox.Width) / 2 + 1,
            headerCell.Location.Y + (headerCell.Height - headerCheckBox.Height) / 2
           );
        }

        private void HeaderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = (sender as CheckBox).Checked;

            this.dgvProperties.CurrentCell = null;

            this.dgvProperties.EndEdit();

            foreach (DataGridViewRow row in this.dgvProperties.Rows)
            {
                row.Cells[this.colSelect.Name].Value = isChecked;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string filePath = this.txtFilePath.Text;

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("File path is required.");
                return;
            }

            string backColor = this.txtBackColor.Text.Trim();
            string foreColor = this.txtForeColor.Text.Trim();

            if (!string.IsNullOrEmpty(backColor) && !this.IsValidColor(backColor))
            {
                MessageBox.Show("Background color value is invalid.");
                return;
            }

            if (!string.IsNullOrEmpty(foreColor) && !this.IsValidColor(foreColor))
            {
                MessageBox.Show("Text color value is invalid.");
                return;
            }

            Task.Run(() =>
            {
                this.Generate();
            });
        }

        private async void Generate()
        {
            this.btnOK.Enabled = false;
            this.btnCancel.Enabled = true;

            option.ShowTableComment = this.chkShowTableComment.Checked;
            option.FilePath = this.txtFilePath.Text;

            List<CustomProperty> properties = new List<CustomProperty>();

            List<string> displayNames = new List<string>();

            foreach (DataGridViewRow row in this.dgvProperties.Rows)
            {
                bool isChecked = Convert.ToBoolean(row.Cells[this.colSelect.Name].Value);

                if (isChecked)
                {
                    CustomProperty property = new CustomProperty();

                    property.PropertyName = row.Cells[this.colSelect.Name].Tag.ToString();
                    property.DisplayName = DataGridViewHelper.GetCellStringValue(row.Cells[this.colDisplayName.Name]);

                    properties.Add(property);

                    if (!string.IsNullOrEmpty(property.DisplayName))
                    {
                        if (displayNames.Contains(property.DisplayName))
                        {
                            MessageBox.Show($@"Display name ""{property.DisplayName}"" is duplicated.");
                            return;
                        }
                        else
                        {
                            displayNames.Add(property.DisplayName);
                        }
                    }
                }
            }

            if (properties.Count == 0)
            {
                MessageBox.Show("No property selected.");
                return;
            }

            option.Properties = properties;
            option.GridColumnHeaderBackgroundColor = this.GetColor(this.txtBackColor.Text);
            option.GridColumnHeaderForegroundColor = this.GetColor(this.txtForeColor.Text);
            option.GridColumnHeaderFontIsBold = this.chkColumnHeaderIsBold.Checked;

            DocumentationGenerator generator = new DocumentationGenerator();

            generator.Subscribe(this);

            this.cancellationTokenSource = new CancellationTokenSource();

            var token = this.cancellationTokenSource.Token;

            DocumentationGenerateResult result = null;

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.databaseType, this.connectionInfo);

            result = await generator.Generate(dbInterpreter, option, token);

            if (!token.IsCancellationRequested)
            {
                if (result.IsOK)
                {
                    MessageBox.Show("Generate successfully.");

                    this.Feedback("");
                }
                else
                {
                    MessageBox.Show("Generate failed.");

                    this.Feedback(new FeedbackInfo() { Message = result.Message, InfoType = FeedbackInfoType.Error });
                }
            }
            else
            {
                this.Feedback("Task has been canceled.");
            }

            this.btnOK.Enabled = true;
            this.btnCancel.Enabled = false;
        }

        private bool IsValidColor(string color)
        {
            try
            {
                if (color.StartsWith("#"))
                {
                    ColorTranslator.FromHtml(color);
                }
                else
                {
                    Color.FromName(color);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string GetColor(string color)
        {
            if (color != null && color.StartsWith("#"))
            {
                return color;
            }
            else if (!string.IsNullOrEmpty(color))
            {
                return ColorTranslator.ToHtml(Color.FromName(color));
            }

            return null;
        }

        private async void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.cancellationTokenSource != null)
            {
                await this.cancellationTokenSource.CancelAsync();
            }
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            this.saveFileDialog1.Filter = "Word file|*.docx";

            DialogResult result = this.saveFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.txtFilePath.Text = this.saveFileDialog1.FileName;
            }
        }

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

        public void Feedback(FeedbackInfo info)
        {
            try
            {
                if (this.OnFeedback != null)
                {
                    this.OnFeedback(info);
                }

                this.Invoke(() =>
                {
                    this.txtMessage.Text = info.Message;
                    this.txtMessage.ForeColor = info.InfoType == FeedbackInfoType.Error ? Color.Red : Color.Black;
                });
            }
            catch (Exception ex)
            {

            }
        }

        public void Feedback(string message)
        {
            this.Feedback(new FeedbackInfo() { Message = message });
        }

        private void btnSelectBackColor_Click(object sender, EventArgs e)
        {
            var result = this.colorDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.txtBackColor.Text = ColorTranslator.ToHtml(this.colorDialog1.Color);

                this.txtBackColor.BackColor = this.colorDialog1.Color;
            }
        }

        private void btnSelectForeColor_Click(object sender, EventArgs e)
        {
            var result = this.colorDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.txtForeColor.Text = ColorTranslator.ToHtml(this.colorDialog1.Color);

                this.txtForeColor.BackColor = this.colorDialog1.Color;
            }
        }
    }
}
