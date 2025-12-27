using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Core.Model;
using DatabaseManager.FileUtility;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmImportData : Form, IObserver<FeedbackInfo>
    {
        private DbInterpreter dbInterpreter;
        private Table table;
        private CancellationTokenSource cancellationTokenSource;
        private List<Table> tables;

        public DatabaseInterpreter.Utility.FeedbackHandler OnFeedback;

        public frmImportData(DbInterpreter dbInterpreter, Table table)
        {
            InitializeComponent();

            this.dbInterpreter = dbInterpreter;
            this.table = table;
        }

        private void SetControlStatus()
        {
            if (this.chkUseColumnMapping.Checked)
            {
                this.dgvColumns.Enabled = true;
            }
            else
            {
                this.dgvColumns.Enabled = false;
                this.dgvColumns.Rows.Clear();
            }
        }

        private async Task<List<TableColumn>> GetTableColumns(Table table)
        {
            SchemaInfoFilter filter = new SchemaInfoFilter() { TableNames = [table.Name], Schema = table.Schema };

            var columns = await this.dbInterpreter.GetTableColumnsAsync(filter);

            return columns;
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.FileName = "";

            DialogResult result = this.openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                string filePath = this.openFileDialog1.FileName;

                this.txtFilePath.Text = filePath;

                this.GetFileColumns();
            }
        }

        private string[] GetFileColumns()
        {
            string filePath = this.txtFilePath.Text.Trim();

            if (File.Exists(filePath))
            {
                string fileExtension = Path.GetExtension(filePath).ToLower();

                DataReadResult result = null;

                SourceFileInfo sourceFileInfo = new SourceFileInfo() { FilePath = filePath, FirstRowIsColumnName = true };

                if (fileExtension == ".csv")
                {
                    CsvReader reader = new CsvReader(sourceFileInfo);

                    result = reader.Read(true);
                }
                else if (fileExtension == ".xlsx" || fileExtension == ".xls")
                {
                    ExcelReader reader = new ExcelReader(sourceFileInfo);

                    result = reader.Read(true);
                }

                return result.HeaderColumns;
            }

            return Array.Empty<string>();
        }

        private bool ValidateMappings(out List<DataImportColumnMapping> mappings)
        {
            mappings = new List<DataImportColumnMapping>();

            List<string> fileColumnNames = new List<string>();

            foreach (DataGridViewRow row in this.dgvColumns.Rows)
            {
                string tableColumName = DataGridViewHelper.GetCellStringValue(row, this.colTableColumnName.Name);
                string fileColumnName = DataGridViewHelper.GetCellStringValue(row, this.colFileColumnName.Name);
                string referencedTableName = DataGridViewHelper.GetCellStringValue(row, this.colReferencedTable.Name);
                string referencedTableColumnName = DataGridViewHelper.GetCellStringValue(row, this.colReferencedTableColumn.Name);

                if (!string.IsNullOrEmpty(fileColumnName))
                {
                    if (fileColumnNames.Contains(fileColumnName))
                    {
                        MessageBox.Show("File Column Name must be unique!");
                        return false;
                    }

                    fileColumnNames.Add(fileColumnName);

                    if (referencedTableName != null && referencedTableColumnName != null)
                    {
                        DataImportColumnMapping mapping = new DataImportColumnMapping();

                        mapping.TableColumName = tableColumName;
                        mapping.FileColumnName = fileColumnName;
                        mapping.ReferencedTableName = referencedTableName;
                        mapping.ReferencedTableColumnName = referencedTableColumnName;

                        var referencedTableColumnNameCell = row.Cells[this.colReferencedTableColumn.Name];

                        if (referencedTableColumnNameCell != null)
                        {
                            mapping.ReferencedTableColumnDataType = (referencedTableColumnNameCell.Tag as TableColumn)?.DataType;
                        }

                        mappings.Add(mapping);
                    }
                }
            }

            return true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string filePath = this.txtFilePath.Text;

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("File Path can't be empty!");
                return;
            }
            else if(!File.Exists(filePath))
            {
                MessageBox.Show("File is not exist!");
                return;
            }

            Task.Run(() => { this.Import(); });
        }

        private async void Import()
        {
            string filePath = this.txtFilePath.Text;           

            List<DataImportColumnMapping> columnMappings = null;

            if (this.chkUseColumnMapping.Checked)
            {
                if (!this.ValidateMappings(out columnMappings))
                {
                    return;
                }
            }

            SourceFileInfo info = new SourceFileInfo();

            info.FilePath = filePath;
            info.FirstRowIsColumnName = this.chkFirstRowIsColumnName.Checked;

            DataImporter importer = new DataImporter();

            importer.Subscribe(this);

            this.cancellationTokenSource = new CancellationTokenSource();

            var token = this.cancellationTokenSource.Token;

            (bool Success, DataValidateResult ValidateResult) = await importer.Import(this.dbInterpreter, table, info, columnMappings, token);

            if (!token.IsCancellationRequested)
            {
                if (Success)
                {
                    MessageBox.Show("Import successfully.");
                }
                else
                {
                    MessageBox.Show("Import failed!");

                    if (ValidateResult != null && ValidateResult.IsValid == false)
                    {
                        string resultFilePath = importer.WriteValidateResultToExcel(ValidateResult, table, info.FirstRowIsColumnName, columnMappings);

                        if (File.Exists(resultFilePath))
                        {
                            var psi = new ProcessStartInfo(resultFilePath) { UseShellExecute = true };

                            Process.Start(psi);
                        }
                    }
                }
            }
            else
            {
                this.Feedback("Task has been canceled.");
            }
        }

        private async void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.cancellationTokenSource != null)
            {
                await this.cancellationTokenSource.CancelAsync();
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

        private void Feedback(FeedbackInfo info)
        {
            if (this.OnFeedback != null)
            {
                this.OnFeedback(info);
            }

            this.Invoke(() =>
            {
                this.txtMessage.Text = info.Message;

                this.txtMessage.ForeColor = (info.InfoType == FeedbackInfoType.Error) ? Color.Red : Color.Black;
            });
        }

        private void Feedback(string message)
        {
            this.Feedback(new FeedbackInfo() { Message = message });
        }

        private async void GetTableNames()
        {
            if (this.colReferencedTable.DataSource == null)
            {
                var tables = await this.dbInterpreter.GetTablesAsync();

                this.tables = tables;

                this.colReferencedTable.DataSource = tables;
                this.colReferencedTable.ValueMember = "Name";
                this.colReferencedTable.DisplayMember = "Name";
            }
        }

        private void chkFirstRowIsColumnName_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.chkFirstRowIsColumnName.Checked)
            {
                this.chkUseColumnMapping.Checked = false;
                this.chkUseColumnMapping.Enabled = false;
            }
            else
            {
                this.chkUseColumnMapping.Enabled = true;
            }
        }

        private async void LoadData()
        {
            try
            {
                this.dgvColumns.Rows.Clear();

                string[] headerColumns = this.GetFileColumns();

                this.colFileColumnName.DataSource = headerColumns;

                var tableColumns = await this.GetTableColumns(this.table);

                foreach (var column in tableColumns)
                {
                    var rowIndex = this.dgvColumns.Rows.Add();

                    this.dgvColumns.Rows[rowIndex].Cells[this.colTableColumnName.Name].Value = column.Name;

                    string mappedColumnName = null;

                    if (headerColumns != null && headerColumns.Length > 0)
                    {
                        string headerColumn = headerColumns.FirstOrDefault(item => item.Replace(" ", "").ToLower() == column.Name.ToLower());

                        if (headerColumn != null)
                        {
                            mappedColumnName = headerColumn;
                        }
                    }

                    this.dgvColumns.Rows[rowIndex].Cells[this.colFileColumnName.Name].Value = mappedColumnName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void dgvColumns_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == this.colReferencedTableColumn.Index)
            {
                int rowIndex = e.RowIndex;

                if (rowIndex < 0)
                {
                    return;
                }

                string referencedTableName = this.dgvColumns.Rows[rowIndex].Cells[this.colReferencedTable.Index].Value?.ToString();

                if (!string.IsNullOrEmpty(referencedTableName))
                {
                    string value = this.dgvColumns.CurrentCell.Value?.ToString();

                    SchemaInfoFilter filter = new SchemaInfoFilter() { TableNames = [referencedTableName] };

                    var columns = await this.dbInterpreter.GetTableColumnsAsync(filter);

                    frmItemsSimpleSelector frm = new frmItemsSimpleSelector(columns.Select(item => item.Name)) { IsSingleSelect = true };
                    frm.Text = "Select Column";

                    if (!string.IsNullOrEmpty(value))
                    {
                        frm.SelectedItems.Add(value);
                    }

                    var result = frm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        string columnName = frm.SelectedItems.FirstOrDefault();

                        this.dgvColumns.CurrentCell.Value = columnName;

                        this.dgvColumns.CurrentCell.Tag = columns.FirstOrDefault(item => item.Name == columnName);
                    }
                }
            }
        }

        private void dgvColumns_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (this.dgvColumns.CurrentCellAddress.X == this.colReferencedTable.DisplayIndex)
            {
                ComboBox combo = e.Control as ComboBox;

                if (combo != null)
                {
                    combo.DropDownStyle = ComboBoxStyle.DropDown;
                }
            }
        }

        private void dgvColumns_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (e.ColumnIndex == this.colReferencedTable.Index || e.ColumnIndex == this.colFileColumnName.Index)
            {
                var value = this.dgvColumns.CurrentCell.EditedFormattedValue?.ToString();

                if (string.IsNullOrEmpty(value))
                {
                    this.dgvColumns.CurrentCell.Value = null;
                }
                else if (e.ColumnIndex == this.colReferencedTable.Index)
                {
                    if (this.tables != null && !this.tables.Any(item => item.Name.ToLower() == value.ToLower()))
                    {
                        this.dgvColumns.CancelEdit();
                    }
                    else
                    {
                        this.dgvColumns.CurrentCell.Value = value;
                    }
                }
            }
        }

        private void chkUseColumMapping_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlStatus();

            if (this.chkUseColumnMapping.Checked)
            {
                if (!string.IsNullOrEmpty(this.txtFilePath.Text))
                {
                    this.GetTableNames();
                }

                this.LoadData();
            }
        }
    }
}
