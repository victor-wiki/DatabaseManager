using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseManager.Helper;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Core;
using DatabaseManager.Model;
using DatabaseInterpreter.Utility;

namespace DatabaseManager.Controls
{
    public partial class UC_TableColumns : UserControl
    {
        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown;
        private int rowIndexOfItemUnderMouseToDrop;
        private IEnumerable<DataTypeSpecification> dataTypeSpecifications;
        public DatabaseType DatabaseType { get; set; }

        public UC_TableColumns()
        {
            InitializeComponent();
        }

        private void UC_TableColumns_Load(object sender, EventArgs e)
        {
            this.InitColumnsGrid();
        }

        public void InitControls()
        {
            this.LoadDataTypes();
        }

        private void InitColumnsGrid()
        {
            foreach (DataGridViewRow row in this.dgvColumns.Rows)
            {
                if (row.Tag == null)
                {
                    row.Tag = new TableColumnDesingerInfo();
                }

                string columnName = row.Cells[nameof(colColumnName)].Value?.ToString();

                if (string.IsNullOrEmpty(columnName))
                {
                    DataGridViewHelper.SetRowColumnsReadOnly(this.dgvColumns, row, true, this.colColumnName);
                }
            }
        }

        public void LoadColumns(Table table, IEnumerable<TableColumnDesingerInfo> columns, IEnumerable<TablePrimaryKey> primaryKeys)
        {
            this.dgvColumns.Rows.Clear();

            foreach (TableColumnDesingerInfo column in columns)
            {
                int rowIndex = this.dgvColumns.Rows.Add();

                DataGridViewRow row = this.dgvColumns.Rows[rowIndex];

                bool isPrimary = primaryKeys.Any(item => item.ColumnName == column.Name);

                row.Cells[this.colColumnName.Name].Value = column.Name;
                row.Cells[this.colDataType.Name].Value = column.DataType;
                row.Cells[this.colLength.Name].Value = column.Length;
                row.Cells[this.colNullable.Name].Value = column.IsNullable;
                row.Cells[this.colIdentity.Name].Value = column.IsIdentity;
                row.Cells[this.colPrimary.Name].Value = column.IsPrimary;
                row.Cells[this.colDefaultValue.Name].Value = column.DefaultValue?.Trim('(', ')');
                row.Cells[this.colComment.Name].Value = column.Comment;               

                row.Tag = colLength;

                TableColumnExtraPropertyInfo extraPropertyInfo = new TableColumnExtraPropertyInfo();              

                if (column.IsComputed)
                {
                    extraPropertyInfo.Expression = column.ComputeExp;
                }

                if (column.IsIdentity && table.IdentitySeed.HasValue)
                {
                    extraPropertyInfo.Seed = table.IdentitySeed.Value;
                    extraPropertyInfo.Increment = table.IdentityIncrement.Value;
                }
            }
        }

        public void OnSaved()
        {
            for (int i = 0; i < this.dgvColumns.RowCount; i++)
            {
                DataGridViewRow row = this.dgvColumns.Rows[i];

                if(!row.IsNewRow)
                {
                    TableColumnDesingerInfo columnDesingerInfo = row.Tag as TableColumnDesingerInfo;

                    if(columnDesingerInfo!=null)
                    {
                        columnDesingerInfo.OldName = columnDesingerInfo.Name;
                    }
                }
            }
        }

        private void LoadDataTypes()
        {
            dataTypeSpecifications = DataTypeManager.GetDataTypeSpecifications(this.DatabaseType);

            this.colDataType.DataSource = dataTypeSpecifications;
            this.colDataType.DisplayMember = "Name";
            this.colDataType.ValueMember = "Name";
            this.colDataType.AutoComplete = true;
        }

        public void EndEdit()
        {
            this.dgvColumns.EndEdit();
        }

        public List<TableColumnDesingerInfo> GetColumns()
        {
            List<TableColumnDesingerInfo> columnDesingerInfos = new List<TableColumnDesingerInfo>();

            int order = 1;
            foreach (DataGridViewRow row in this.dgvColumns.Rows)
            {
                if (row.IsNewRow)
                {
                    break;
                }

                TableColumnDesingerInfo col = new TableColumnDesingerInfo() { Order = order };

                string colName = row.Cells["colColumnName"].Value?.ToString();

                col.Name = colName;
                col.DataType = this.GetCellStringValue(row, this.colDataType.Name);
                col.Length = this.GetCellStringValue(row, this.colLength.Name);
                col.IsNullable = this.GetCellBoolValue(row, this.colNullable.Name);
                col.IsPrimary = this.GetCellBoolValue(row, this.colPrimary.Name);
                col.IsIdentity = this.GetCellBoolValue(row, this.colIdentity.Name);
                col.DefaultValue = this.GetCellStringValue(row, this.colDefaultValue.Name);
                col.Comment = this.GetCellStringValue(row, this.colComment.Name);
                col.ExtraPropertyInfo = (row.Tag as TableColumnDesingerInfo)?.ExtraPropertyInfo;

                columnDesingerInfos.Add(col);

                order++;
            }

            return columnDesingerInfos;
        }

        private string GetCellStringValue(DataGridViewRow row, string columnName)
        {
            return row.Cells[columnName].Value?.ToString()?.Trim();
        }

        private bool GetCellBoolValue(DataGridViewRow row, string columnName)
        {
            return this.IsTrueValue(row.Cells[columnName].Value);
        }

        private bool IsTrueValue(object value)
        {
            return value?.ToString() == "True";
        }

        private void UC_TableColumns_SizeChanged(object sender, EventArgs e)
        {
            DataGridViewHelper.AutoSizeColumn(this.dgvColumns, colComment);
        }

        private void dgvColumns_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Tag = new TableColumnDesingerInfo();

            DataGridViewHelper.SetRowColumnsReadOnly(this.dgvColumns, e.Row, true, this.colColumnName);
        }

        private void dgvColumns_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dgvColumns.Rows[e.RowIndex];
                DataGridViewCell cell = row.Cells[e.ColumnIndex];

                if (e.ColumnIndex == this.colColumnName.Index)
                {
                    string columnName = cell.Value?.ToString();

                    DataGridViewHelper.SetRowColumnsReadOnly(this.dgvColumns, row, string.IsNullOrEmpty(columnName), this.colColumnName);
                    this.SetColumnCellsReadonly(row);
                }
                else if (e.ColumnIndex == this.colDataType.Index)
                {
                    this.SetColumnCellsReadonly(row);
                }
                else if (e.ColumnIndex == this.colPrimary.Index)
                {
                    DataGridViewCell primaryCell = row.Cells[this.colPrimary.Name];
                    DataGridViewCell nullableCell = row.Cells[this.colNullable.Name];

                    if (this.IsTrueValue(primaryCell.Value) && this.IsTrueValue(nullableCell.Value))
                    {
                        nullableCell.Value = false;
                    }
                }
                else if (e.ColumnIndex == this.colIdentity.Index)
                {
                    if (this.IsTrueValue(cell.Value))
                    {
                        foreach (DataGridViewRow r in this.dgvColumns.Rows)
                        {
                            if (r.Index >= 0 && r.Index != e.RowIndex)
                            {
                                r.Cells[this.colIdentity.Name].Value = false;
                            }
                        }
                    }

                    this.ShowColumnExtraPropertites();
                }
            }
        }

        private void SetColumnCellsReadonly(DataGridViewRow row)
        {
            DataGridViewCell lengthCell = row.Cells[this.colLength.Name];
            DataGridViewCell primaryCell = row.Cells[this.colPrimary.Name];
            DataGridViewCell identityCell = row.Cells[this.colIdentity.Name];

            string dataType = this.GetCellStringValue(row, this.colDataType.Name);

            if (!string.IsNullOrEmpty(dataType))
            {
                DataTypeSpecification dataTypeSpec = this.dataTypeSpecifications.FirstOrDefault(item => item.Name == dataType);

                if (dataTypeSpec != null)
                {
                    bool isLengthReadOnly = string.IsNullOrEmpty(dataTypeSpec.Args);
                    bool isPrimaryReadOnly = dataTypeSpec.IndexForbidden;
                    bool isIdentityReadOnly = !dataTypeSpec.AllowIdentity;

                    lengthCell.ReadOnly = isLengthReadOnly;
                    primaryCell.ReadOnly = isPrimaryReadOnly;
                    identityCell.ReadOnly = isIdentityReadOnly;

                    if (isLengthReadOnly)
                    {
                        lengthCell.Value = null;
                    }

                    if (isPrimaryReadOnly)
                    {
                        primaryCell.Value = false;
                    }

                    if (isIdentityReadOnly)
                    {
                        identityCell.Value = false;
                    }
                }
            }
            else
            {
                lengthCell.ReadOnly = true;
                primaryCell.ReadOnly = true;
                identityCell.ReadOnly = true;
            }
        }

        private void dgvColumns_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void dgvColumns_SelectionChanged(object sender, EventArgs e)
        {
            this.ShowColumnExtraPropertites();
        }

        private void ShowColumnExtraPropertites()
        {
            var selectedRows = this.dgvColumns.SelectedRows.OfType<DataGridViewRow>();

            if (selectedRows.Count() > 0)
            {
                DataGridViewRow row = selectedRows.FirstOrDefault();

                TableColumnDesingerInfo column = row.Tag as TableColumnDesingerInfo;

                if (column == null)
                {
                    column = new TableColumnDesingerInfo();
                    row.Tag = column;
                }

                TableColumnExtraPropertyInfo extralProperty = column?.ExtraPropertyInfo;

                if (extralProperty == null)
                {
                    extralProperty = new TableColumnExtraPropertyInfo();
                    column.ExtraPropertyInfo = extralProperty;
                }

                DataGridViewCell identityCell = row.Cells[this.colIdentity.Name];

                if (!this.IsTrueValue(identityCell.Value))
                {
                    this.columnPropertites.HiddenProperties = new string[] { nameof(extralProperty.Seed), nameof(extralProperty.Increment) };
                }
                else
                {
                    this.columnPropertites.HiddenProperties = null;
                }

                this.columnPropertites.SelectedObject = extralProperty;
                this.columnPropertites.Refresh();
            }
        }

        private void dgvColumns_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (e.ColumnIndex == this.colIdentity.Index)
            {
                this.dgvColumns.EndEdit();
            }
        }

        private void dgvColumns_DragDrop(object sender, DragEventArgs e)
        {
            Point clientPoint = this.dgvColumns.PointToClient(new Point(e.X, e.Y));

            rowIndexOfItemUnderMouseToDrop = this.dgvColumns.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            if (rowIndexOfItemUnderMouseToDrop == -1)
            {
                return;
            }

            if (rowIndexFromMouseDown >= 0 && rowIndexOfItemUnderMouseToDrop < this.dgvColumns.Rows.Count)
            {
                if (this.dgvColumns.Rows[rowIndexOfItemUnderMouseToDrop].IsNewRow)
                {
                    return;
                }
            }

            if (e.Effect == DragDropEffects.Move)
            {
                DataGridViewRow rowToMove = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;

                if (rowToMove.Index >= 0)
                {
                    this.dgvColumns.Rows.RemoveAt(rowIndexFromMouseDown);
                    this.dgvColumns.Rows.Insert(rowIndexOfItemUnderMouseToDrop, rowToMove);

                    string columnName = this.GetCellStringValue(rowToMove, this.colColumnName.Name);

                    DataGridViewHelper.SetRowColumnsReadOnly(this.dgvColumns, rowToMove, string.IsNullOrEmpty(columnName), this.colColumnName);
                    this.SetColumnCellsReadonly(rowToMove);
                }
            }
        }

        private void dgvColumns_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if (dragBoxFromMouseDown != Rectangle.Empty &&
                !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    DragDropEffects dropEffect = this.dgvColumns.DoDragDrop(
                          this.dgvColumns.Rows[rowIndexFromMouseDown],
                          DragDropEffects.Move);
                }
            }
        }

        private void dgvColumns_MouseDown(object sender, MouseEventArgs e)
        {
            var hit = this.dgvColumns.HitTest(e.X, e.Y);
            rowIndexFromMouseDown = hit.RowIndex;

            if (hit.Type == DataGridViewHitTestType.RowHeader && rowIndexFromMouseDown != -1)
            {
                Size dragSize = SystemInformation.DragSize;

                dragBoxFromMouseDown = new Rectangle(
                          new Point(
                            e.X - (dragSize.Width / 2),
                            e.Y - (dragSize.Height / 2)),
                      dragSize);
            }
            else
            {
                dragBoxFromMouseDown = Rectangle.Empty;
            }
        }

        private void dgvColumns_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void tsmiInsertColumn_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = DataGridViewHelper.GetSelectedRow(this.dgvColumns);

            if (row != null)
            {
                this.dgvColumns.Rows.Insert(row.Index - 1 < 0 ? 0 : row.Index - 1);
            }
        }

        private void tsmiDeleteColumn_Click(object sender, EventArgs e)
        {
            this.DeleteRow();
        }

        private void dgvColumns_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DataGridViewRow row = DataGridViewHelper.GetSelectedRow(this.dgvColumns);

                bool isNewRow = row != null && row.IsNewRow;

                this.tsmiDeleteColumn.Enabled = !isNewRow;

                this.contextMenuStrip1.Show(this.dgvColumns, e.Location);
            }
        }

        private void dgvColumns_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                this.DeleteRow();
            }
        }

        private void DeleteRow()
        {
            DataGridViewRow row = DataGridViewHelper.GetSelectedRow(this.dgvColumns);

            if (row != null && !row.IsNewRow)
            {
                this.dgvColumns.Rows.RemoveAt(row.Index);
            }
        }

        private void dgvColumns_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            this.dgvColumns.EndEdit();
            this.dgvColumns.CurrentCell = null;
            this.dgvColumns.Rows[e.RowIndex].Selected = true;
        }
    }
}
