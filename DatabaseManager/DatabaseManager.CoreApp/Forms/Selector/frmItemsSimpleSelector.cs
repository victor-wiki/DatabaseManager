using DatabaseInterpreter.Model;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmItemsSimpleSelector : Form
    {
        private IEnumerable<string> items;
        public bool IsSingleSelect { get; set; }
        public List<string> SelectedItems { get; set; } = new List<string>();

        public frmItemsSimpleSelector(IEnumerable<string> items)
        {
            InitializeComponent();

            this.items = items;
        }

        private void frmColumSelect_Load(object sender, EventArgs e)
        {
            this.InitGrid();
        }

        private void InitGrid()
        {
            if (this.IsSingleSelect)
            {
                this.dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                this.dgvItems.MultiSelect = false;
            }

            this.LoadItems();
        }

        private void LoadItems()
        {
            this.dgvItems.Rows.Clear();

            if (this.items != null)
            {
                foreach (var item in this.items)
                {
                    var rowIndex = this.dgvItems.Rows.Add();

                    this.dgvItems.Rows[rowIndex].Cells[0].Value = item;

                    if (this.SelectedItems != null && this.SelectedItems.Count > 0)
                    {
                        if (this.SelectedItems.Contains(item))
                        {
                            this.dgvItems.Rows[rowIndex].Selected = true;
                        }
                    }
                }
            }

            if (this.SelectedItems == null || this.SelectedItems.Count == 0)
            {
                this.dgvItems.ClearSelection();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.ConfirmSelect();
        }

        private void ConfirmSelect()
        {
            this.SelectedItems.Clear();

            var selectedRows = this.dgvItems.SelectedRows;

            foreach (DataGridViewRow row in selectedRows)
            {
                this.SelectedItems.Add(row.Cells[this.colItem.Name].Value.ToString());
            }

            this.DialogResult = DialogResult.OK;

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void dgvItems_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void dgvItems_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (this.SelectedItems == null || this.SelectedItems.Count == 0)
            {
                this.dgvItems.ClearSelection();
            }
        }

        private void dgvItems_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (this.IsSingleSelect)
            {
                this.ConfirmSelect();
            }
        }
    }
}
