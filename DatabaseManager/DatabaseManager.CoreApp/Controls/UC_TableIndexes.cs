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
using DatabaseManager.Core;

namespace DatabaseManager.Controls
{
    public partial class UC_TableIndexes : UserControl
    {
        private bool inited = false;
        private bool loadedData = false;
        private DateTime dtTypeCellClick = DateTime.Now;
        public DatabaseType DatabaseType { get; set; }
        public Table Table { get; set; }

        public bool Inited => this.inited;
        public bool LoadedData => this.loadedData;

        public event ColumnSelectHandler OnColumnSelect;
        public GeneateChangeScriptsHandler OnGenerateChangeScripts;

        public UC_TableIndexes()
        {
            InitializeComponent();
        }

        public void InitControls(DbInterpreter dbInterpreter)
        {
            if (this.inited)
            {
                return;
            }

            if (!ManagerUtil.SupportComment(this.DatabaseType))
            {
                this.colComment.Visible = false;
            }

            var types = Enum.GetValues(typeof(IndexType));

            List<string> typeNames = new List<string>();

            foreach (var type in types)
            {
                if (dbInterpreter.IndexType.HasFlag((IndexType)type) && (IndexType)type != IndexType.None)
                {
                    typeNames.Add(type.ToString());
                }
            }

            this.colType.DataSource = typeNames;

            if (this.DatabaseType == DatabaseType.Oracle)
            {
                this.colComment.Visible = false;
            }

            this.inited = true;
        }

        public void LoadIndexes(IEnumerable<TableIndexDesignerInfo> indexDesignerInfos)
        {
            this.dgvIndexes.Rows.Clear();

            foreach (TableIndexDesignerInfo index in indexDesignerInfos)
            {
                int rowIndex = this.dgvIndexes.Rows.Add();

                DataGridViewRow row = this.dgvIndexes.Rows[rowIndex];

                row.Cells[this.colIndexName.Name].Value = index.Name;
                row.Cells[this.colType.Name].Value = this.GetIndexTypeEnumName(index.Type);
                row.Cells[this.colColumns.Name].Value = this.GetColumnsDisplayText(index.Columns);
                row.Cells[this.colComment.Name].Value = index.Comment;

                row.Tag = index;
            }

            this.loadedData = true;

            this.AutoSizeColumns();
            this.dgvIndexes.ClearSelection();
        }

        private string GetIndexTypeEnumName(string type)
        {
            var typeNames = Enum.GetNames(typeof(IndexType));

            foreach (var tn in typeNames)
            {
                if (tn.ToLower() == type.ToLower())
                {
                    return tn;
                }
            }

            return type;
        }

        public void LoadPrimaryKeys(IEnumerable<TableColumnDesingerInfo> columnDesingerInfos)
        {
            int primaryRowIndex = -1;

            foreach (DataGridViewRow row in this.dgvIndexes.Rows)
            {
                if (!row.IsNewRow)
                {
                    TableIndexDesignerInfo indexDesignerInfo = row.Tag as TableIndexDesignerInfo;

                    if (indexDesignerInfo != null && indexDesignerInfo.IsPrimary)
                    {
                        primaryRowIndex = row.Index;

                        if (columnDesingerInfos.Count() > 0)
                        {
                            indexDesignerInfo.Columns.Clear();

                            indexDesignerInfo.Columns.AddRange(columnDesingerInfos.Select(item => new IndexColumn() { ColumnName = item.Name }));

                            row.Cells[this.colColumns.Name].Value = this.GetColumnsDisplayText(indexDesignerInfo.Columns);
                        }

                        break;
                    }
                }
            }

            if (primaryRowIndex >= 0 && columnDesingerInfos.Count() == 0)
            {
                this.dgvIndexes.Rows.RemoveAt(primaryRowIndex);
            }
            else if (primaryRowIndex < 0 && columnDesingerInfos.Count() > 0)
            {
                int rowIndex = this.dgvIndexes.Rows.Add();

                TableIndexDesignerInfo tableIndexDesignerInfo = new TableIndexDesignerInfo()
                {
                    Type = this.DatabaseType == DatabaseType.Oracle ? IndexType.Unique.ToString() : IndexType.Primary.ToString(),
                    Name = IndexManager.GetPrimaryKeyDefaultName(this.Table),
                    IsPrimary = true
                };

                tableIndexDesignerInfo.Columns.AddRange(columnDesingerInfos.Select(item => new IndexColumn() { ColumnName = item.Name }));

                DataGridViewRow primaryRow = this.dgvIndexes.Rows[rowIndex];
                primaryRow.Cells[this.colType.Name].Value = tableIndexDesignerInfo.Type;
                primaryRow.Cells[this.colIndexName.Name].Value = tableIndexDesignerInfo.Name;
                primaryRow.Cells[this.colColumns.Name].Value = this.GetColumnsDisplayText(tableIndexDesignerInfo.Columns);

                tableIndexDesignerInfo.ExtraPropertyInfo = new TableIndexExtraPropertyInfo() { Clustered = true };

                primaryRow.Tag = tableIndexDesignerInfo;
            }

            this.ShowIndexExtraPropertites();
        }

        private string GetColumnsDisplayText(IEnumerable<IndexColumn> columns)
        {
            return string.Join(",", columns.Select(item => item.ColumnName));
        }

        public List<TableIndexDesignerInfo> GetIndexes()
        {
            List<TableIndexDesignerInfo> indexDesingerInfos = new List<TableIndexDesignerInfo>();

            foreach (DataGridViewRow row in this.dgvIndexes.Rows)
            {
                TableIndexDesignerInfo index = new TableIndexDesignerInfo();

                string indexName = row.Cells[this.colIndexName.Name].Value?.ToString();

                if (!string.IsNullOrEmpty(indexName))
                {
                    TableIndexDesignerInfo tag = row.Tag as TableIndexDesignerInfo;

                    index.OldName = tag?.OldName;
                    index.OldType = tag?.OldType;
                    index.Name = indexName;
                    index.Type = DataGridViewHelper.GetCellStringValue(row, this.colType.Name);
                    index.Columns = tag?.Columns;
                    index.Comment = DataGridViewHelper.GetCellStringValue(row, this.colComment.Name);
                    index.ExtraPropertyInfo = tag?.ExtraPropertyInfo;
                    index.IsPrimary = tag?.IsPrimary == true;
                    row.Tag = index;

                    indexDesingerInfos.Add(index);
                }
            }

            return indexDesingerInfos;
        }

        private void dgvIndexes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                this.DeleteRow();
            }
        }

        private void DeleteRow()
        {
            DataGridViewRow row = DataGridViewHelper.GetSelectedRow(this.dgvIndexes);

            if (row != null && !row.IsNewRow)
            {
                this.dgvIndexes.Rows.RemoveAt(row.Index);
            }
        }

        private void tsmiDeleteIndex_Click(object sender, EventArgs e)
        {
            this.DeleteRow();
        }

        private void dgvIndexes_SizeChanged(object sender, EventArgs e)
        {
            this.AutoSizeColumns();
        }

        private void AutoSizeColumns()
        {
            DataGridViewHelper.AutoSizeLastColumn(this.dgvIndexes);
        }

        private void dgvIndexes_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void dgvIndexes_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DataGridViewRow row = DataGridViewHelper.GetSelectedRow(this.dgvIndexes);

                if (row != null)
                {
                    bool isEmptyNewRow = row.IsNewRow && DataGridViewHelper.IsEmptyRow(row);

                    this.tsmiDeleteIndex.Enabled = !isEmptyNewRow;
                }
                else
                {
                    this.tsmiDeleteIndex.Enabled = false;
                }

                this.contextMenuStrip1.Show(this.dgvIndexes, e.Location);
            }
        }

        private void dgvIndexes_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            this.dgvIndexes.EndEdit();
            this.dgvIndexes.CurrentCell = null;
            this.dgvIndexes.Rows[e.RowIndex].Selected = true;
        }

        private void dgvIndexes_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (e.ColumnIndex == this.colType.Index)
            {
                this.ShowIndexExtraPropertites();

                DataGridViewRow row = this.dgvIndexes.Rows[e.RowIndex];

                DataGridViewCell nameCell = row.Cells[this.colIndexName.Name];

                string type = DataGridViewHelper.GetCellStringValue(row, this.colType.Name);

                string indexName = DataGridViewHelper.GetCellStringValue(nameCell);

                if (string.IsNullOrEmpty(indexName))
                {
                    nameCell.Value = type == IndexType.Primary.ToString() ? IndexManager.GetPrimaryKeyDefaultName(this.Table) : IndexManager.GetIndexDefaultName(type, this.Table);
                }

                if (row.Tag != null)
                {
                    (row.Tag as TableIndexDesignerInfo).IsPrimary = type == IndexType.Primary.ToString();
                }
            }
        }

        private void dgvIndexes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (e.ColumnIndex == this.colColumns.Index)
            {
                DataGridViewRow row = this.dgvIndexes.Rows[e.RowIndex];
                DataGridViewCell cell = row.Cells[this.colColumns.Name];

                string indexName = DataGridViewHelper.GetCellStringValue(row, this.colIndexName.Name);
                string type = DataGridViewHelper.GetCellStringValue(row, this.colType.Name);

                if (!string.IsNullOrEmpty(indexName))
                {
                    TableIndexDesignerInfo indexDesignerInfo = row.Tag as TableIndexDesignerInfo;

                    if (this.OnColumnSelect != null)
                    {
                        this.OnColumnSelect(DatabaseObjectType.Index,
                            indexDesignerInfo?.Columns == null ? Enumerable.Empty<IndexColumn>() : indexDesignerInfo?.Columns,
                            type == IndexType.Primary.ToString()
                        );
                    }
                }
            }
        }

        public void SetRowColumns(IEnumerable<IndexColumn> columnInfos)
        {
            DataGridViewCell cell = this.dgvIndexes.CurrentCell;

            if (cell != null)
            {
                cell.Value = string.Join(",", columnInfos.Select(item => item.ColumnName));

                TableIndexDesignerInfo tableIndexDesignerInfo = cell.OwningRow.Tag as TableIndexDesignerInfo;

                if (tableIndexDesignerInfo == null)
                {
                    tableIndexDesignerInfo = new TableIndexDesignerInfo();
                }

                tableIndexDesignerInfo.Columns = columnInfos.ToList();

                cell.OwningRow.Tag = tableIndexDesignerInfo;
            }
        }

        private void dgvIndexes_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            if (e.Row != null)
            {
                if (e.Row.Tag == null)
                {
                    e.Row.Tag = new TableIndexDesignerInfo();
                }
            }
        }

        private void ShowIndexExtraPropertites()
        {
            var row = DataGridViewHelper.GetSelectedRow(this.dgvIndexes);

            if (row != null)
            {
                string indexName = DataGridViewHelper.GetCellStringValue(row, this.colIndexName.Name);

                if (string.IsNullOrEmpty(indexName))
                {
                    return;
                }

                TableIndexDesignerInfo index = row.Tag as TableIndexDesignerInfo;

                if (index == null)
                {
                    index = new TableIndexDesignerInfo();
                    row.Tag = index;
                }

                TableIndexExtraPropertyInfo extralProperty = index?.ExtraPropertyInfo;

                DataGridViewCell typeCell = row.Cells[this.colType.Name];

                if (extralProperty == null)
                {
                    extralProperty = new TableIndexExtraPropertyInfo();
                    index.ExtraPropertyInfo = extralProperty;

                    if (DataGridViewHelper.GetCellStringValue(typeCell) != IndexType.Primary.ToString())
                    {
                        index.ExtraPropertyInfo.Clustered = false;
                    }
                }

                if (this.DatabaseType == DatabaseType.Oracle)
                {
                    this.indexPropertites.HiddenProperties = new string[] { nameof(extralProperty.Clustered) };
                }
                else
                {
                    this.indexPropertites.HiddenProperties = null;
                }

                this.indexPropertites.SelectedObject = extralProperty;
                this.indexPropertites.Refresh();
            }
        }

        private void dgvIndexes_SelectionChanged(object sender, EventArgs e)
        {
            this.ShowIndexExtraPropertites();
        }

        public void EndEdit()
        {
            this.dgvIndexes.EndEdit();
            this.dgvIndexes.CurrentCell = null;
        }

        public void OnSaved()
        {
            for (int i = 0; i < this.dgvIndexes.RowCount; i++)
            {
                DataGridViewRow row = this.dgvIndexes.Rows[i];

                TableIndexDesignerInfo indexDesingerInfo = row.Tag as TableIndexDesignerInfo;

                if (indexDesingerInfo != null && !string.IsNullOrEmpty(indexDesingerInfo.Name))
                {
                    indexDesingerInfo.OldName = indexDesingerInfo.Name;
                    indexDesingerInfo.OldType = indexDesingerInfo.Type;
                }
            }
        }

        private void tsmiGenerateChangeScripts_Click(object sender, EventArgs e)
        {
            if (this.OnGenerateChangeScripts != null)
            {
                this.OnGenerateChangeScripts();
            }
        }

        private void dgvIndexes_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == this.colType.Index)
            {
                var cell = this.dgvIndexes.CurrentCell;

                if (cell != null && cell.IsInEditMode && cell.EditedFormattedValue != cell.Value)
                {
                    cell.Value = cell.EditedFormattedValue;
                }
            }
        }
    }
}
