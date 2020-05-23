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
        public bool ColumnIsReadOnly { get; set; }
        public List<IndexColumn> SelectedColumns { get; private set; }
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
            if(this.ColumnIsReadOnly)
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

        public void InitControls(IEnumerable<IndexColumn> columns, bool showSortColumn = true)
        {
            this.colSort.DataSource = Enum.GetValues(typeof(SortType));
            this.colColumName.DataSource = columns.ToList();
            this.colColumName.DisplayMember = nameof(IndexColumn.ColumnName);
            this.colColumName.ValueMember = nameof(IndexColumn.ColumnName);

            if (!showSortColumn)
            {
                this.colSort.Visible = false;
                this.colColumName.Width = this.dgvColumns.Width - this.dgvColumns.RowHeadersWidth;
            }
        }

        public void LoadColumns(IEnumerable<IndexColumn> columns)
        {
            this.dgvColumns.Rows.Clear();

            foreach (IndexColumn column in columns)
            {
                int rowIndex = this.dgvColumns.Rows.Add();

                DataGridViewRow row = this.dgvColumns.Rows[rowIndex];

                row.Cells[this.colColumName.Name].Value = column.ColumnName;
                row.Cells[this.colSort.Name].Value = column.IsDesc ? SortType.Descending : SortType.Ascending;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            List<IndexColumn> columns = new List<IndexColumn>();

            int order = 1;
            foreach (DataGridViewRow row in this.dgvColumns.Rows)
            {
                if (!row.IsNewRow)
                {
                    IndexColumn columnInfo = new IndexColumn() { Order = order };
                    columnInfo.ColumnName = row.Cells[this.colColumName.Name].Value?.ToString();
                    columnInfo.IsDesc = row.Cells[this.colSort.Name].Value?.ToString() == SortType.Descending.ToString();

                    columns.Add(columnInfo);

                    order++;
                }
            }

            if (columns.Count == 0)
            {
                MessageBox.Show("Please select column(s).");
                return;
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
