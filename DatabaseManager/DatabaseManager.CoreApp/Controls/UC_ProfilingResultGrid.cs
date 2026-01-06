using DatabaseManager.Core.Model;
using DatabaseManager.Helper;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UserControl = System.Windows.Forms.UserControl;

namespace DatabaseManager.Controls
{
    public partial class UC_ProfilingResultGrid : UserControl
    {
        public UC_ProfilingResultGrid()
        {
            InitializeComponent();

            this.dgvData.AutoGenerateColumns = false;
        }

        public void LoadData(ProfilingResult result)
        {
            this.dgvData.DataSource = result.Details;
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

        private void dgvData_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvData.ClearSelection();
        }

        private void dgvData_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Action action = () =>
                {
                    this.SetContextMenuItemVisible();

                    this.contextMenuStrip1.Show(this.dgvData, e.Location);
                };

                if (this.InvokeRequired)
                {
                    this.Invoke(action);
                }
                else
                {
                    action();
                }

                this.Invalidate();
            }
        }

        private void SetContextMenuItemVisible()
        {
            int selectedCount = this.dgvData.GetCellCount(DataGridViewElementStates.Selected);

            this.tsmiCopyContent.Visible = selectedCount == 1;
            this.tsmiShowContent.Visible = selectedCount == 1;
        }

        private void dgvData_Resize(object sender, EventArgs e)
        {
            this.colSQL.Width = this.dgvData.Width - this.colExecuteType.Width - this.colDuration.Width - 3;
        }

        private void dgvData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewCell cell = this.dgvData.Rows[e.RowIndex].Cells[e.ColumnIndex];

                cell.ToolTipText = (this.dgvData.DataSource as List<ProfilingResultDetail>)[e.RowIndex].Sql;
            }
        }
    }
}
