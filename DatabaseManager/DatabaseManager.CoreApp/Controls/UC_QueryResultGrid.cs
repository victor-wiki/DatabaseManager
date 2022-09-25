using DatabaseInterpreter.Utility;
using DatabaseManager.Helper;
using System;
using System.Data;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_QueryResultGrid : UserControl
    {
        public UC_QueryResultGrid()
        {
            InitializeComponent();
        }

        public void LoadData(DataTable dataTable)
        {
            this.dgvData.DataSource = DataGridViewHelper.ConvertDataTable(dataTable);
        }

        public void ClearData()
        {
            this.dgvData.DataSource = null;
        }

        private void tsmiSave_Click(object sender, EventArgs e)
        {
            this.Save();
        }

        private void Save()
        {
            if (this.dlgSave == null)
            {
                this.dlgSave = new SaveFileDialog();
            }

            this.dlgSave.FileName = "";

            DialogResult result = this.dlgSave.ShowDialog();
            if (result == DialogResult.OK)
            {
                DataTableHelper.WriteToFile(this.dgvData.DataSource as DataTable, this.dlgSave.FileName);
            }
        }

        private void dgvData_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                bool canCopy = this.dgvData.GetCellCount(DataGridViewElementStates.Selected) > 0;

                this.tsmiCopy.Enabled = canCopy;
                this.tsmiCopyWithHeader.Enabled = canCopy;

                this.contextMenuStrip1.Show(this.dgvData, e.Location);
            }
        }

        private void tsmiCopy_Click(object sender, EventArgs e)
        {
            this.Copy(DataGridViewClipboardCopyMode.EnableWithoutHeaderText);
        }

        private void tsmiCopyWithHeader_Click(object sender, EventArgs e)
        {
            this.Copy(DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText);
        }

        private void Copy(DataGridViewClipboardCopyMode mode)
        {
            this.dgvData.ClipboardCopyMode = mode;

            Clipboard.SetDataObject(this.dgvData.GetClipboardContent());
        }

        private void dgvData_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvData.ClearSelection();
        }

        private void dgvData_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }
    }
}
