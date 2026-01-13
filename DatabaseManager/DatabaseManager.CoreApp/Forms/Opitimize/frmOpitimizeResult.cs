using DatabaseManager.Core.Model;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmOpitimizeResult : Form
    {

        private OptimizeResult result;

        public frmOpitimizeResult(OptimizeResult result)
        {
            InitializeComponent();

            this.dgvResult.AutoGenerateColumns = false;

            this.result = result;
        }

        private void frmOpitimizeResult_Load(object sender, EventArgs e)
        {
            this.LoadData();
        }

        private void LoadData()
        {
            this.dgvResult.DataSource = this.result.Details;
        }

        private void dgvResult_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvResult.ClearSelection();
        }

        private void dgvResult_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            this.dgvResult.ClearSelection();
        }

        private void dgvResult_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == this.colResult.Index && e.RowIndex >= 0)
            {
                var row = this.dgvResult.Rows[e.RowIndex];
                var cell = row.Cells[e.ColumnIndex];

                var detail = row.DataBoundItem as OptimizeResultDetail;

                if(detail.IsOK)
                {
                    cell.Value = "OK";
                    cell.Style.BackColor = Color.LightGreen;
                }
                else
                {
                    cell.Value = "Failed";
                    cell.Style.BackColor = Color.Pink;
                    cell.ToolTipText = detail.Message;
                }
            }
        }
    }
}
