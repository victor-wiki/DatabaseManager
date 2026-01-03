using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Core.Model;
using DatabaseManager.FileUtility.Model;
using DatabaseManager.Helper;
using NPOI.Util.Collections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public delegate Task<DataTable> ExportDataRequiredHandler(ExportSpecificDataOption option, List<DataExportColumn> columns);

    public partial class frmExportData : Form, IObserver<FeedbackInfo>
    {
        private DbInterpreter dbInterpreter;
        private DatabaseObject tableOrView;
        private ExportSpecificDataOption option;
        private CancellationTokenSource cancellationTokenSource;
        private List<TableColumn> columns;
        private bool showScope = true;
        private DataTable dataTable;        

        public DatabaseInterpreter.Utility.FeedbackHandler OnFeedback;
        public ExportDataRequiredHandler ExportDataRequired;

        public frmExportData(DbInterpreter dbInterpreter, DatabaseObject tableOrView, ExportSpecificDataOption option = null, bool showScope = true)
        {
            InitializeComponent();

            this.dbInterpreter = dbInterpreter;
            this.tableOrView = tableOrView;

            this.option = option;
            this.showScope = showScope;

            this.Init();
        }

        public frmExportData(DataTable dataTable, ExportSpecificDataOption option = null, bool showScope = true)
        {
            InitializeComponent();

            this.dataTable = dataTable;
            this.option = option;

            this.showScope = showScope;

            this.Init();
        }

        private void Init()
        {
            if (option == null)
            {
                option = new ExportSpecificDataOption();
            }

            if (!this.showScope)
            {
                this.gbScope.Visible = false;
                this.rbAllPages.Checked = true;

                this.gbColumns.Height += this.gbScope.Height;
            }
        }

        private void frmExportData_Load(object sender, EventArgs e)
        {
            this.LoadData();
        }

        private async void LoadData()
        {
            var names = Enum.GetNames(typeof(ExportFileType)).Where(item => item != nameof(ExportFileType.None)).ToArray();

            this.cboFileType.Items.AddRange(names);

            this.cboFileType.SelectedIndex = 0;

            this.AddHeaderCheckBox();

            if (this.tableOrView != null)
            {
                this.columns = await this.dbInterpreter.GetTableColumnsAsync(new SchemaInfoFilter() { TableNames = [this.tableOrView.Name] });

                foreach (TableColumn column in this.columns)
                {
                    int rowIndex = this.dgvColumns.Rows.Add();

                    this.dgvColumns.Rows[rowIndex].Cells[this.colSelect.Name].Value = true;
                    this.dgvColumns.Rows[rowIndex].Cells[this.colColumnName.Name].Value = column.Name;
                    this.dgvColumns.Rows[rowIndex].Cells[this.colDisplayName.Name].Value = column.Name;
                }
            }
            else if (this.dataTable != null)
            {
                foreach (DataColumn column in this.dataTable.Columns)
                {
                    int rowIndex = this.dgvColumns.Rows.Add();

                    this.dgvColumns.Rows[rowIndex].Cells[this.colSelect.Name].Value = true;
                    this.dgvColumns.Rows[rowIndex].Cells[this.colColumnName.Name].Value = column.ColumnName;
                    this.dgvColumns.Rows[rowIndex].Cells[this.colDisplayName.Name].Value = column.ColumnName;
                }
            }

            this.dgvColumns.ClearSelection();
        }

        private void AddHeaderCheckBox()
        {
            CheckBox headerCheckBox = new CheckBox() { Checked = true };
            headerCheckBox.Size = new Size(15, 15);

            headerCheckBox.CheckedChanged += this.HeaderCheckBox_CheckedChanged;

            this.dgvColumns.Controls.Add(headerCheckBox);

            Rectangle headerCell = this.dgvColumns.GetCellDisplayRectangle(0, -1, true);
            headerCheckBox.Location = new Point(
            headerCell.Location.X + (headerCell.Width - headerCheckBox.Width) / 2 + 1,
            headerCell.Location.Y + (headerCell.Height - headerCheckBox.Height) / 2
           );
        }

        private void HeaderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = (sender as CheckBox).Checked;

            this.dgvColumns.CurrentCell = null;

            this.dgvColumns.EndEdit();

            foreach (DataGridViewRow row in this.dgvColumns.Rows)
            {
                row.Cells[this.colSelect.Name].Value = isChecked;
            }
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            string tableName = "";

            if (this.tableOrView != null)
            {
                tableName = this.tableOrView.Name;
            }
            else if (this.dataTable != null)
            {
                tableName = this.dataTable.TableName;
            }

            if (!string.IsNullOrEmpty(tableName))
            {
                this.saveFileDialog1.FileName = tableName;
            }

            if (this.cboFileType.Text == nameof(ExportFileType.CSV))
            {
                this.saveFileDialog1.Filter = "csv file|*.csv";
            }
            else
            {
                this.saveFileDialog1.Filter = "excel file|*.xlsx";
            }

            DialogResult result = this.saveFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.txtFilePath.Text = this.saveFileDialog1.FileName;
            }
        }

        private async void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.cancellationTokenSource != null)
            {
                await this.cancellationTokenSource.CancelAsync();
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

            if (this.rbPageNumberRange.Checked)
            {
                var pageNumbers = this.GetPageNumbers();

                if (pageNumbers.Count == 0)
                {
                    MessageBox.Show("Page number range is invalid.");
                    return;
                }
            }

            Task.Run(() =>
            {
                this.ExportData();
            });
        }

        private async void ExportData()
        {
            this.btnOK.Enabled = false;
            this.btnCancel.Enabled = true;

            this.option.FileType = (ExportFileType)Enum.Parse(typeof(ExportFileType), this.cboFileType.Text);
            this.option.ShowColumnNames = this.chkShowColumnName.Checked;
            this.option.FilePath = this.txtFilePath.Text;

            this.option.ExportAllThatMeetCondition = this.rbAllPages.Checked;

            if (this.rbPageNumberRange.Checked)
            {
                this.option.PageNumbers = this.GetPageNumbers();
            }

            List<DataExportColumn> columns = new List<DataExportColumn>();

            List<string> displayNames = new List<string>();

            foreach (DataGridViewRow row in this.dgvColumns.Rows)
            {
                bool isChecked = Convert.ToBoolean(row.Cells[this.colSelect.Name].Value);

                if (isChecked)
                {
                    DataExportColumn column = new DataExportColumn();

                    column.ColumnName = DataGridViewHelper.GetCellStringValue(row.Cells[this.colColumnName.Name]);
                    column.DisplayName = DataGridViewHelper.GetCellStringValue(row.Cells[this.colDisplayName.Name]);

                    if (this.columns != null)
                    {
                        column.DataType = this.columns.FirstOrDefault(item => item.Name == column.ColumnName)?.DataType;
                    }

                    columns.Add(column);

                    if (!string.IsNullOrEmpty(column.DisplayName))
                    {
                        if (displayNames.Contains(column.DisplayName))
                        {
                            MessageBox.Show($@"Display name ""{column.DisplayName}"" is duplicated.");
                            return;
                        }
                        else
                        {
                            displayNames.Add(column.DisplayName);
                        }
                    }
                }
            }

            if (columns.Count == 0)
            {
                MessageBox.Show("No column selected.");
                return;
            }

            DataExporter exporter = new DataExporter();

            exporter.Subscribe(this);

            this.cancellationTokenSource = new CancellationTokenSource();

            var token = this.cancellationTokenSource.Token;

            DataExportResult result = null;

            if (this.tableOrView != null)
            {
                result = await exporter.Export(this.dbInterpreter, this.tableOrView, columns, this.option, token);
            }
            else if (this.dataTable != null)
            {
                if (this.dataTable.Rows.Count == 0)
                {
                    if (this.ExportDataRequired != null)
                    {
                        DataTable table = await this.ExportDataRequired(option, columns);

                        this.Feedback("Start to export...");

                        result = exporter.Export(table, columns, option);

                        this.Feedback("End export.");
                    }
                }
            }

            if (!token.IsCancellationRequested)
            {
                if (result.IsOK)
                {
                    MessageBox.Show("Export successfully.");

                    this.Feedback("");
                }
                else
                {
                    MessageBox.Show("Export failed.");

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

        private List<long> GetPageNumbers()
        {
            List<long> pageNumbers = new List<long>();

            string content = this.txtPageNumberRange.Text.Trim();

            var items = content.Split(',');

            foreach (var item in items)
            {
                var subItems = item.Split("-");

                string strStartPageNumber = null;
                string strEndPageNumber = null;

                if (subItems.Length == 2)
                {
                    strStartPageNumber = subItems[0];
                    strEndPageNumber = subItems[1];
                }
                else
                {
                    strStartPageNumber = strEndPageNumber = item;
                }

                if (long.TryParse(strStartPageNumber.Trim(), out _) && long.TryParse(strEndPageNumber.Trim(), out _))
                {
                    long starPageNumber = long.Parse(strStartPageNumber.Trim());
                    long endPageNumber = long.Parse(strEndPageNumber.Trim());

                    for (long i = starPageNumber; i <= endPageNumber; i++)
                    {
                        if (i > 0 && i <= this.option.PageCount && !pageNumbers.Contains(i))
                        {
                            pageNumbers.Add(i);
                        }
                    }
                }
            }

            return pageNumbers;
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

        private void chkShowColumnName_CheckedChanged(object sender, EventArgs e)
        {
            this.colDisplayName.Visible = this.chkShowColumnName.Checked;
        }

        private void rbPageNumberRange_CheckedChanged(object sender, EventArgs e)
        {
            this.txtPageNumberRange.Enabled = this.rbPageNumberRange.Checked;
        }
    }
}
