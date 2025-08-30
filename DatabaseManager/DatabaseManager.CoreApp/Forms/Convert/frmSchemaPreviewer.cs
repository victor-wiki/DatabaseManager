using DatabaseInterpreter.Model;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmSchemaPreviewer : Form
    {
        private SchemaInfo schemaInfo;
        private Table currentTable;
        List<SchemaMappingInfo> schemaMappings = null;
        Dictionary<string, string> tableNameMappings = null;
        private IEnumerable<TableColumnContentMaxLength> tableColumnContentMaxLengths = null;
        private bool needCompareLength = false;

        public SchemaInfo SchemaInfo => this.schemaInfo;

        public frmSchemaPreviewer(SchemaInfo schemaInfo, List<SchemaMappingInfo> schemaMappings, Dictionary<string, string> tableNameMappings, IEnumerable<TableColumnContentMaxLength> tableColumnContentMaxLengths = null)
        {
            InitializeComponent();

            DataGridView.CheckForIllegalCrossThreadCalls = false;

            this.schemaInfo = schemaInfo;
            this.schemaMappings = schemaMappings;
            this.tableNameMappings = tableNameMappings;
            this.tableColumnContentMaxLengths = tableColumnContentMaxLengths;
        }

        private void frmSchemaPreviewer_Load(object sender, EventArgs e)
        {
            this.Init();
        }

        private void Init()
        {
            if (this.tableColumnContentMaxLengths == null)
            {
                this.colCurrentColumnContentMaxLength.Visible = false;
                this.Width -= this.colCurrentColumnContentMaxLength.Width;
            }
            else
            {
                this.needCompareLength = true;
            }

            this.tvDbObjects.ShowCheckBox = false;
            this.tvDbObjects.TreeNodeSelected += this.tvDbObjects_TreeNodeSelected;

            this.tvDbObjects.LoadTree(this.schemaInfo, true);
        }

        private void tvDbObjects_TreeNodeSelected(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;

            this.dgvColumn.Rows.Clear();

            if (node != null && node.Tag is Table table)
            {
                this.currentTable = table;

                string schema = table.Schema;
                string tableName = table.Name;

                var columns = this.schemaInfo.TableColumns.Where(item => item.Schema == schema && item.TableName == tableName);

                foreach (TableColumn column in columns)
                {
                    DataGridViewTextBoxCell[] cells = new DataGridViewTextBoxCell[7];

                    TableColumnContentMaxLength tableColumnContentMaxLength = null;

                    if (this.tableColumnContentMaxLengths != null)
                    {
                        string sourceSchema = this.schemaMappings != null ? (this.schemaMappings.FirstOrDefault(item => item.TargetSchema == schema)?.SourceSchema) : schema;
                        string sourceTableName = null;                   

                        if (this.tableNameMappings != null)
                        {
                            foreach (var kp in this.tableNameMappings)
                            {
                                if (kp.Value == tableName)
                                {
                                    sourceTableName = kp.Key;
                                }
                            }
                        }

                        if (sourceTableName == null)
                        {
                            sourceTableName = tableName;
                        }

                        tableColumnContentMaxLength = this.tableColumnContentMaxLengths.FirstOrDefault(item => 
                        (item.Schema == sourceSchema || string.IsNullOrEmpty(item.Schema) && string.IsNullOrEmpty(sourceSchema))
                        && item.TableName == sourceTableName 
                        && item.ColumnName == column.Name);
                    }

                    int? currentMaxLength = tableColumnContentMaxLength?.ContentMaxLength;

                    cells[0] = new DataGridViewTextBoxCell() { Value = column.Name };
                    cells[1] = new DataGridViewTextBoxCell() { Value = column.DataType, Tag = column.DataType };
                    cells[2] = new DataGridViewTextBoxCell() { Value = column.MaxLength, Tag = column.MaxLength };
                    cells[3] = new DataGridViewTextBoxCell() { Value = column.Precision, Tag = column.Precision };
                    cells[4] = new DataGridViewTextBoxCell() { Value = column.Scale, Tag = column.Scale };
                    cells[5] = new DataGridViewTextBoxCell() { Value = column.DefaultValue, Tag = column.DefaultValue };
                    cells[6] = new DataGridViewTextBoxCell() { Value = currentMaxLength, Tag = currentMaxLength };

                    if (this.needCompareLength)
                    {
                        if (column.MaxLength != -1 && column.MaxLength < currentMaxLength)
                        {
                            cells[2].Style.BackColor = Color.Pink;
                        }
                    }

                    DataGridViewRow row = new DataGridViewRow();

                    row.Cells.AddRange(cells);

                    this.dgvColumn.Rows.Add(row);
                }
            }
            else
            {
                this.currentTable = null;
            }

            this.dgvColumn.ClearSelection();
        }

        private void dgvColumn_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            int columnIndex = e.ColumnIndex;
            int rowIndex = e.RowIndex;

            if (columnIndex >= 0 && rowIndex >= 0)
            {
                if (this.dgvColumn.Columns[columnIndex].ReadOnly)
                {
                    return;
                }

                var cell = this.dgvColumn.Rows[rowIndex].Cells[columnIndex];

                string value = cell.Value?.ToString();
                string editValue = cell.EditedFormattedValue?.ToString();

                if (value != editValue)
                {
                    cell.Value = cell.EditedFormattedValue;
                }

                if (!this.IsCellValid(cell))
                {
                    return;
                }
                else
                {
                    this.CheckCellChange(cell);                    

                    cell.ErrorText = null;
                }
            }

            e.Cancel = false;
        }

        private void CheckCellChange(DataGridViewCell cell)
        {
            object oldValue = cell.Tag;

            if (cell.Value?.ToString() != oldValue?.ToString())
            {
                this.UpdateColumnProperty(cell);
            }

            this.CheckDataLengthCell(cell);
        }

        private void UpdateColumnProperty(DataGridViewCell cell)
        {
            try
            {
                int columnIndex = cell.ColumnIndex;
                int rowIndex = cell.RowIndex;

                if (rowIndex >= 0 && columnIndex >= 0)
                {
                    var row = this.dgvColumn.Rows[rowIndex];

                    string columnName = row.Cells[this.colColumnName.Index].Value.ToString();

                    var column = this.schemaInfo.TableColumns.FirstOrDefault(item => item.TableName == this.currentTable.Name && item.Schema == this.currentTable.Schema && item.Name == columnName);

                    if (column != null)
                    {
                        object oldValue = cell.Tag;
                        object currentValue = cell.Value;

                        if (oldValue?.ToString() != currentValue?.ToString())
                        {
                            if (columnIndex == this.colTargetDataType.Index)
                            {
                                column.DataType = currentValue.ToString();
                            }
                            else if (columnIndex == this.colLength.Index)
                            {
                                if (long.TryParse(currentValue?.ToString(), out _))
                                {
                                    column.MaxLength = Convert.ToInt64(currentValue);
                                }
                            }
                            else if (columnIndex == this.colPrecision.Index)
                            {
                                if (long.TryParse(currentValue?.ToString(), out _))
                                {
                                    column.Precision = Convert.ToInt64(currentValue);
                                }
                            }
                            else if (columnIndex == this.colScale.Index)
                            {
                                if (long.TryParse(currentValue?.ToString(), out _))
                                {
                                    column.Scale = Convert.ToInt64(currentValue);
                                }
                            }
                            else if (columnIndex == this.colDefaultValue.Index)
                            {
                                column.DefaultValue = currentValue?.ToString();
                            }

                            cell.Tag = currentValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckDataLengthCell(DataGridViewCell cell)
        {
            int columnIndex = cell.ColumnIndex;
            int rowIndex = cell.RowIndex;

            if (this.needCompareLength)
            {
                bool isValid = true;

                if (columnIndex == this.colLength.Index)
                {
                    if (!string.IsNullOrEmpty(cell.Value?.ToString()))
                    {
                        int length = Convert.ToInt32(cell.Value);

                        object maxLengthValue = this.dgvColumn.Rows[rowIndex].Cells[this.colCurrentColumnContentMaxLength.Index].Value;

                        int maxLength = 0;

                        if (int.TryParse(maxLengthValue?.ToString(), out maxLength))
                        {
                            if (length != -1 && length < maxLength)
                            {
                                isValid = false;
                            }
                        }
                    }
                }

                cell.Style.BackColor = !isValid ? Color.Pink : Color.White;
            }
        }

        private bool IsCellValid(DataGridViewCell cell)
        {
            int columnIndex = cell.ColumnIndex;

            if (columnIndex == this.colTargetDataType.Index)
            {
                if (this.IsNullValue(cell.Value))
                {
                    cell.ErrorText = "value is required!";
                    return false;
                }
            }
            else if (columnIndex == this.colLength.Index || columnIndex == this.colPrecision.Index || columnIndex == this.colScale.Index)
            {
                if (!this.IsNullValue(cell.Value))
                {
                    if (!this.IsIntegerValue(cell.Value))
                    {
                        cell.ErrorText = "value must be integer!";
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsNullValue(object value)
        {
            return value == null || value == DBNull.Value || value.ToString() == string.Empty;
        }

        private bool IsIntegerValue(object value)
        {
            return !this.IsNullValue(value) && int.TryParse(value.ToString(), out _);
        }

        private void frmSchemaPreviewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.dgvColumn.EndEdit();

            if (this.dgvColumn.CurrentCell != null)
            {
                this.CheckCellChange(this.dgvColumn.CurrentCell);
            }
        }

        private void dgvColumn_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void dgvColumn_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                this.PasteClipboardValue();
            }
        }

        private void PasteClipboardValue()
        {
            var selectedCells = this.dgvColumn.SelectedCells;

            if (selectedCells.Count == 0)
            {
                return;
            }

            string content = Clipboard.GetText().Replace(Environment.NewLine, "\n");

            if (!content.Contains("\n") && !content.Contains("\t"))
            {
                foreach (DataGridViewCell cell in selectedCells)
                {
                    cell.Value = content;

                    this.CheckCellChange(cell);
                }
            }
            else
            {
                DataGridViewCell startCell = selectedCells.OfType<DataGridViewCell>().OrderBy(item => item.RowIndex).ThenBy(item => item.ColumnIndex).FirstOrDefault();

                Dictionary<int, Dictionary<int, string>> values = this.GetClipBoardValues(content);

                int rowIndex = startCell.RowIndex;

                foreach (int rowKey in values.Keys)
                {
                    int columnIndex = startCell.ColumnIndex;

                    foreach (int cellKey in values[rowKey].Keys)
                    {
                        if (columnIndex <= this.dgvColumn.Columns.Count - 1 && rowIndex <= this.dgvColumn.Rows.Count - 1)
                        {
                            DataGridViewCell cell = this.dgvColumn[columnIndex, rowIndex];

                            cell.Value = values[rowKey][cellKey];

                            this.CheckCellChange(cell);
                        }

                        columnIndex++;
                    }

                    rowIndex++;
                }
            }
        }

        private Dictionary<int, Dictionary<int, string>> GetClipBoardValues(string clipboardValue)
        {
            Dictionary<int, Dictionary<int, string>> copyValues = new Dictionary<int, Dictionary<int, string>>();

            string[] lines = clipboardValue.Split('\n');

            for (int i = 0; i <= lines.Length - 1; i++)
            {
                copyValues[i] = new Dictionary<int, string>();
                string[] lineContent = lines[i].Split('\t');

                if (lineContent.Length == 0)
                {
                    copyValues[i][0] = string.Empty;
                }
                else
                {
                    for (int j = 0; j <= lineContent.Length - 1; j++)
                    {
                        copyValues[i][j] = lineContent[j];
                    }
                }
            }

            return copyValues;
        }

        private void dgvColumn_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.RowIndex == -1)
            {
                var selectedCells = this.dgvColumn.SelectedCells;

                if (selectedCells != null)
                {
                    foreach (DataGridViewCell cell in selectedCells)
                    {
                        cell.Selected = false;
                    }
                }

                int columnIndex = e.ColumnIndex;

                foreach (DataGridViewRow row in this.dgvColumn.Rows)
                {
                    var cell = row.Cells[columnIndex];

                    cell.Selected = true;
                }
            }
        }

        private void tsmiSetCellValueToNull_Click(object sender, EventArgs e)
        {
            var cells = this.dgvColumn.SelectedCells;

            if (cells != null)
            {
                foreach (DataGridViewCell cell in cells)
                {
                    cell.Value = DBNull.Value;
                }
            }
        }

        private void tsmiCopy_Click(object sender, EventArgs e)
        {
            var selectedCells = this.dgvColumn.SelectedCells;

            if (selectedCells != null && selectedCells.Count > 0)
            {
                this.dgvColumn.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

                DataObject content = this.dgvColumn.GetClipboardContent();

                Clipboard.SetDataObject(content);
            }
        }

        private void dgvColumn_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            var value = this.dgvColumn.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            bool hasValue = value != null && value.GetType() != typeof(DBNull) && !string.IsNullOrEmpty(value.ToString());

            if (e.Button == MouseButtons.Right)
            {
                DataGridViewCell cell = this.dgvColumn.Rows[e.RowIndex].Cells[e.ColumnIndex];

                var selectedCells = this.dgvColumn.SelectedCells;

                if (selectedCells == null || selectedCells.Count == 0 || !selectedCells.Contains(cell))
                {
                    this.dgvColumn.CurrentCell = cell;
                }

                this.SetContextMenuItemVisible();

                this.cellContextMenu.Show(Cursor.Position);
            }
        }

        private void SetContextMenuItemVisible()
        {
            var selectedCells = this.dgvColumn.SelectedCells;
            int selectedCellCount = selectedCells == null ? 0 : selectedCells.Count;
            this.tsmiCopy.Visible = selectedCellCount > 0;
        }
    }
}
