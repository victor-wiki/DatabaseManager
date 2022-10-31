using DatabaseInterpreter.Utility;
using DatabaseManager.Helper;
using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_QueryResultGrid : UserControl
    {
        public UC_QueryResultGrid()
        {
            InitializeComponent();

            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, this.dgvData, new object[] { true });
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
                this.SetContextMenuItemVisible();

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

        private void dgvData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewHelper.FormatCell(this.dgvData, e);
        }

        private void SetContextMenuItemVisible()
        {
            int selectedCount = this.dgvData.GetCellCount(DataGridViewElementStates.Selected);
            this.tsmiCopy.Visible = selectedCount > 1;
            this.tsmiCopyWithHeader.Visible = selectedCount > 1;
            this.tsmiViewGeometry.Visible = selectedCount ==1 && DataGridViewHelper.IsGeometryValue(this.dgvData);
            this.tsmiCopyContent.Visible = selectedCount == 1;
            this.tsmiShowContent.Visible = selectedCount == 1;
        }

        private void tsmiViewGeometry_Click(object sender, EventArgs e)
        {
            DataGridViewHelper.ShowGeometryViewer(this.dgvData);
        }

        private void tsmiCopyContent_Click(object sender, EventArgs e)
        {
            var value = DataGridViewHelper.GetCurrentCellValue(this.dgvData);

            if (!string.IsNullOrEmpty(value))
            {
                Clipboard.SetDataObject(value);
            }
        }

        private void tsmiShowContent_Click(object sender, EventArgs e)
        {
            DataGridViewHelper.ShowCellContent(this.dgvData);
        }
    }
}
