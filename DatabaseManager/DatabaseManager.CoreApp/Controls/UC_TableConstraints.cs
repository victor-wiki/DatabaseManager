using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseInterpreter.Model;
using DatabaseManager.Model;
using DatabaseManager.Helper;

namespace DatabaseManager.Controls
{
    public partial class UC_TableConstraints : UserControl
    {
        private bool inited = false;
        private bool loadedData = false;

        public bool Inited => this.inited;
        public bool LoadedData => this.loadedData;
        public Table Table { get; set; }

        public DatabaseType DatabaseType { get; set; }

        public GeneateChangeScriptsHandler OnGenerateChangeScripts;
        public event ColumnSelectHandler OnColumnSelect;

        public UC_TableConstraints()
        {
            InitializeComponent();          
        }

        private void UC_TableConstraints_Load(object sender, EventArgs e)
        {
            if (this.DatabaseType == DatabaseType.MySql)
            {
                this.dgvConstraints.Columns["colComment"].Visible = false;
            }
        }

        public void InitControls()
        {
            if (this.DatabaseType == DatabaseType.Oracle || this.DatabaseType == DatabaseType.MySql || this.DatabaseType == DatabaseType.Sqlite)
            {
                this.colComment.Visible = false;
            }

            if(this.DatabaseType == DatabaseType.Sqlite)
            {
                this.colColumnName.Visible = true;
            }

            this.inited = true;
        }

        public void LoadConstraints(IEnumerable<TableConstraintDesignerInfo> constraintDesignerInfos)
        {
            this.dgvConstraints.Rows.Clear();

            foreach (TableConstraintDesignerInfo constriant in constraintDesignerInfos)
            {
                int rowIndex = this.dgvConstraints.Rows.Add();

                DataGridViewRow row = this.dgvConstraints.Rows[rowIndex];

                row.Cells[this.colColumnName.Name].Value = constriant.ColumnName;
                row.Cells[this.colName.Name].Value = constriant.Name;
                row.Cells[this.colDefinition.Name].Value = constriant.Definition;
                row.Cells[this.colComment.Name].Value = constriant.Comment;

                row.Tag = constriant;
            }

            this.loadedData = true;

            this.AutoSizeColumns();
            this.dgvConstraints.ClearSelection();
        }

        private void AutoSizeColumns()
        {
            DataGridViewHelper.AutoSizeLastColumn(this.dgvConstraints);
        }

        public List<TableConstraintDesignerInfo> GetConstraints()
        {
            List<TableConstraintDesignerInfo> constraintDesingerInfos = new List<TableConstraintDesignerInfo>();

            foreach (DataGridViewRow row in this.dgvConstraints.Rows)
            {
                TableConstraintDesignerInfo constraint = new TableConstraintDesignerInfo();

                string constraintName = row.Cells[this.colName.Name].Value?.ToString();
                string columnName = row.Cells[this.colColumnName.Name].Value?.ToString();

                if (!string.IsNullOrEmpty(constraintName) || !string.IsNullOrEmpty(columnName))
                {
                    TableConstraintDesignerInfo tag = row.Tag as TableConstraintDesignerInfo;

                    constraint.OldName = tag?.OldName;
                    constraint.Name = constraintName;
                    constraint.ColumnName = columnName;
                    constraint.Definition = DataGridViewHelper.GetCellStringValue(row, this.colDefinition.Name);
                    constraint.Comment = DataGridViewHelper.GetCellStringValue(row, this.colComment.Name);

                    row.Tag = constraint;

                    constraintDesingerInfos.Add(constraint);
                }
            }

            return constraintDesingerInfos;
        }

        private void dgvConstraints_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                this.DeleteRow();
            }
        }

        private void DeleteRow()
        {
            DataGridViewRow row = DataGridViewHelper.GetSelectedRow(this.dgvConstraints);

            if (row != null && !row.IsNewRow)
            {
                this.dgvConstraints.Rows.RemoveAt(row.Index);
            }
        }

        private void tsmiDeleteConstraint_Click(object sender, EventArgs e)
        {
            this.DeleteRow();
        }

        private void dgvConstraints_SizeChanged(object sender, EventArgs e)
        {
            this.AutoSizeColumns();
        }

        private void dgvConstraints_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        public void OnSaved()
        {
            for (int i = 0; i < this.dgvConstraints.RowCount; i++)
            {
                DataGridViewRow row = this.dgvConstraints.Rows[i];

                TableForeignKeyDesignerInfo keyDesingerInfo = row.Tag as TableForeignKeyDesignerInfo;

                if (keyDesingerInfo != null && !string.IsNullOrEmpty(keyDesingerInfo.Name))
                {
                    keyDesingerInfo.OldName = keyDesingerInfo.Name;
                }
            }
        }

        public void EndEdit()
        {
            this.dgvConstraints.EndEdit();
            this.dgvConstraints.CurrentCell = null;
        }

        private void dgvConstraints_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DataGridViewRow row = DataGridViewHelper.GetSelectedRow(this.dgvConstraints);

                if (row != null)
                {
                    bool isEmptyNewRow = row.IsNewRow && DataGridViewHelper.IsEmptyRow(row);

                    this.tsmiDeleteConstraint.Enabled = !isEmptyNewRow;
                }
                else
                {
                    this.tsmiDeleteConstraint.Enabled = false;
                }

                this.contextMenuStrip1.Show(this.dgvConstraints, e.Location);
            }
        }

        private void dgvConstraints_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            this.dgvConstraints.EndEdit();
            this.dgvConstraints.CurrentCell = null;
            this.dgvConstraints.Rows[e.RowIndex].Selected = true;
        }

        private void tsmiGenerateChangeScripts_Click(object sender, EventArgs e)
        {
            if (this.OnGenerateChangeScripts != null)
            {
                this.OnGenerateChangeScripts();
            }
        }

        private void dgvConstraints_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (e.ColumnIndex == this.colColumnName.Index)
            {
                DataGridViewRow row = this.dgvConstraints.Rows[e.RowIndex];
                DataGridViewCell cell = row.Cells[this.colColumnName.Name];           

                TableConstraintDesignerInfo designerInfo = row.Tag as TableConstraintDesignerInfo;

                if (this.OnColumnSelect != null)
                {
                    this.OnColumnSelect(DatabaseObjectType.Constraint,
                        designerInfo?.ColumnName == null ? Enumerable.Empty<SimpleColumn>() : new SimpleColumn[] { new SimpleColumn() { ColumnName = designerInfo.ColumnName } },
                        false, true
                    );
                }
            }
        }

        public void SetRowColumns(IEnumerable<SimpleColumn> columnInfos)
        {
            DataGridViewCell cell = this.dgvConstraints.CurrentCell;

            if (cell != null)
            {
                string columnName = columnInfos.FirstOrDefault()?.ColumnName;

                cell.Value = columnName;

                TableConstraintDesignerInfo designerInfo = cell.OwningRow.Tag as TableConstraintDesignerInfo;

                if (designerInfo == null)
                {
                    designerInfo = new TableConstraintDesignerInfo();
                }

                designerInfo.ColumnName = columnName;

                cell.OwningRow.Tag = designerInfo;
            }
        }
    }
}
