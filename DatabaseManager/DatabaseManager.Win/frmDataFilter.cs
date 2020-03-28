using DatabaseManager.Core;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmDataFilter : Form
    {
        public List<DataGridViewColumn> Columns = new List<DataGridViewColumn>();

        public QueryConditionBuilder ConditionBuilder { get; set; }

        public frmDataFilter()
        {
            InitializeComponent();
        }

        private void frmDataFilter_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            this.LoadColumnsTree();

            if (this.ConditionBuilder != null)
            {
                foreach (QueryConditionItem condition in this.ConditionBuilder.Conditions)
                {
                    DataGridViewColumn column = this.Columns.FirstOrDefault(item => item.Name == condition.ColumnName);
                    if (column != null)
                    {
                        this.AddField(column, condition);
                    }
                }
            }
        }

        private void LoadColumnsTree()
        {
            foreach (DataGridViewColumn column in this.Columns)
            {
                TreeNode node = new TreeNode(column.Name);
                node.ImageKey = "Column.png";
                node.SelectedImageKey = node.ImageKey;
                node.Tag = column;

                this.tvColumns.Nodes.Add(node);
            }
        }

        private void tvColumns_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var node = e.Item as TreeNode;
            if (node != null)
            {
                if (node.Tag is DataGridViewColumn)
                {
                    this.tvColumns.SelectedNode = node;
                    DoDragDrop(e.Item, DragDropEffects.Copy);
                }
            }
        }

        private void tvColumns_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == this.tvColumns.SelectedNode && e.Node.Tag is DataGridViewColumn)
            {
                this.AddField(e.Node.Tag as DataGridViewColumn);
            }
        }

        private void AddField(DataGridViewColumn column, QueryConditionItem condition = null)
        {
            foreach (DataGridViewRow row in this.dgvFilter.Rows)
            {
                if (row.Cells["ColumnName"].Value.ToString() == column.Name)
                {
                    return;
                }
            }

            int rowIndex = this.dgvFilter.Rows.Add(column.Name);

            this.dgvFilter.Rows[rowIndex].Tag = condition;

            if (condition != null)
            {
                this.dgvFilter.Rows[rowIndex].Cells["Filter"].Value = condition.ToString();
            }
        }

        private void dgvFilter_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewColumn column = this.dgvFilter.Columns[e.ColumnIndex];

            if (column.Name == "Filter" && e.RowIndex > -1 && e.ColumnIndex > -1)
            {
                string columnName = this.dgvFilter.Rows[e.RowIndex].Cells["ColumnName"].Value.ToString();

                frmDataFilterCondition filterCondition = new frmDataFilterCondition() { Column = this.Columns.FirstOrDefault(item => item.Name == columnName) };
                filterCondition.Condition = this.dgvFilter.Rows[e.RowIndex].Tag as QueryConditionItem;

                if (filterCondition.ShowDialog() == DialogResult.OK)
                {
                    QueryConditionItem condition = filterCondition.Condition;

                    this.dgvFilter.Rows[e.RowIndex].Tag = condition;

                    this.dgvFilter.Rows[e.RowIndex].Cells["Filter"].Value = condition.ToString();
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.GetQueryConditionBuilder();

            if (this.ConditionBuilder.Conditions.Count == 0)
            {
                if (MessageBox.Show("Has no any condition, are you sure to query without any condition?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;

            this.Close();
        }

        private QueryConditionBuilder GetQueryConditionBuilder()
        {
            this.ConditionBuilder = new QueryConditionBuilder();

            foreach (DataGridViewRow row in this.dgvFilter.Rows)
            {
                QueryConditionItem condition = row.Tag as QueryConditionItem;

                if (condition != null)
                {
                    this.ConditionBuilder.Add(condition);
                }
            }

            return this.ConditionBuilder;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgvFilter_DragEnter(object sender, DragEventArgs e)
        {
            var node = e.Data.GetData(typeof(TreeNode)) as TreeNode;
            if (node != null)
            {
                if (node.Tag is DataGridViewColumn)
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }

            var row = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
            if (row != null)
            {
                if (this.dgvFilter.Rows.Contains(row))
                {
                    e.Effect = DragDropEffects.Move;
                    return;
                }
            }
        }

        private void dgvFilter_DragOver(object sender, DragEventArgs e)
        {
            var row = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;

            if (row != null)
            {
                var pt = this.dgvFilter.PointToClient(new Point(e.X, e.Y));
                var ht = this.dgvFilter.HitTest(pt.X, pt.Y);

                switch (ht.Type)
                {
                    case DataGridViewHitTestType.Cell:
                    case DataGridViewHitTestType.RowHeader:
                    case DataGridViewHitTestType.None:
                        e.Effect = DragDropEffects.Move;
                        break;
                    default:
                        e.Effect = DragDropEffects.None;
                        break;
                }
            }
        }

        private void dgvFilter_DragDrop(object sender, DragEventArgs e)
        {
            var node = e.Data.GetData(typeof(TreeNode)) as TreeNode;

            if (node != null)
            {
                this.AddField(node.Tag as DataGridViewColumn);
                return;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.dgvFilter.Rows.Clear();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            int count = this.dgvFilter.SelectedRows.Count;
            if (count == 0)
            {
                MessageBox.Show("Please select a row first.");
                return;
            }

            this.dgvFilter.Rows.RemoveAt(this.dgvFilter.SelectedRows[0].Index);
        }
    }
}
