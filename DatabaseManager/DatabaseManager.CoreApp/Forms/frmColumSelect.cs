using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseInterpreter.Model;
using DatabaseManager.Helper;
using DatabaseManager.Model;

namespace DatabaseManager
{
    public partial class frmColumSelect : Form
    {
        private bool isIndexColumn;
        public bool ColumnIsReadOnly { get; set; }
        public bool IsSingleSelect { get; set; }
        public List<SimpleColumn> SelectedColumns { get; private set; }
        public frmColumSelect()
        {
            InitializeComponent();
        }

        private void frmColumSelect_Load(object sender, EventArgs e)
        {
            this.InitGrid();
        }

        private void InitGrid()
        {
            if (this.ColumnIsReadOnly)
            {
                this.colColumName.ReadOnly = true;
                this.dgvColumns.AllowUserToAddRows = false;
            }

            foreach (DataGridViewRow row in this.dgvColumns.Rows)
            {
                if (row.Tag == null)
                {
                    row.Tag = new TableColumnDesingerInfo();
                }
            }
        }

        public void InitControls(IEnumerable<SimpleColumn> columns, bool showSortColumn = true)
        {
            this.isIndexColumn = columns?.FirstOrDefault()?.GetType() == typeof(IndexColumn);

            this.colSort.DataSource = Enum.GetValues(typeof(SortType));
            this.colColumName.DataSource = columns.ToList();
            this.colColumName.DisplayMember = nameof(SimpleColumn.ColumnName);
            this.colColumName.ValueMember = nameof(SimpleColumn.ColumnName);

            if (!showSortColumn)
            {
                this.colSort.Visible = false;
                this.colColumName.Width = this.dgvColumns.Width - this.dgvColumns.RowHeadersWidth;
            }
        }

        public void LoadColumns(IEnumerable<SimpleColumn> columns)
        {
            this.dgvColumns.Rows.Clear();

            foreach (var column in columns)
            {
                int rowIndex = this.dgvColumns.Rows.Add();

                DataGridViewRow row = this.dgvColumns.Rows[rowIndex];

                row.Cells[this.colColumName.Name].Value = column.ColumnName;

                if(this.isIndexColumn)
                {
                    row.Cells[this.colSort.Name].Value = (column as IndexColumn).IsDesc ? SortType.Descending : SortType.Ascending;
                }                
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            List<SimpleColumn> columns = new List<SimpleColumn>();

            int order = 1;
            foreach (DataGridViewRow row in this.dgvColumns.Rows)
            {
                if (!row.IsNewRow)
                {
                    SimpleColumn columnInfo = null;

                    if (this.isIndexColumn)
                    {
                        columnInfo = new IndexColumn();
                        (columnInfo as IndexColumn).IsDesc = row.Cells[this.colSort.Name].Value?.ToString() == SortType.Descending.ToString();
                    }
                    else
                    {
                        columnInfo = new SimpleColumn();
                    }

                    columnInfo.Order = order;
                    columnInfo.ColumnName = row.Cells[this.colColumName.Name].Value?.ToString();

                    columns.Add(columnInfo);

                    order++;
                }
            }

            if (columns.Count == 0)
            {
                MessageBox.Show("Please select column(s).");
                return;
            }
            else if (columns.Count > 1)
            {
                if (this.IsSingleSelect)
                {
                    MessageBox.Show("Only allow select one column.");
                    return;
                }
            }

            this.SelectedColumns = columns;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
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

        private void DeleteRow()
        {
            DataGridViewRow row = DataGridViewHelper.GetSelectedRow(this.dgvColumns);

            if (row != null && !row.IsNewRow)
            {
                this.dgvColumns.Rows.RemoveAt(row.Index);
            }
        }

        private void dgvColumns_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                this.DeleteRow();
            }
        }

        private void dgvColumns_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            this.dgvColumns.EndEdit();
            this.dgvColumns.CurrentCell = null;
            this.dgvColumns.Rows[e.RowIndex].Selected = true;
        }

        private void dgvColumns_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void dgvColumns_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvColumns.ClearSelection();
        }
    }
}
