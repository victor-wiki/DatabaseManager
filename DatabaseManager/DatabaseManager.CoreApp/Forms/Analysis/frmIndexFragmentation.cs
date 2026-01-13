using DatabaseInterpreter.Core;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Core.Model;
using DatabaseManager.FileUtility.Model;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmIndexFragmentation : Form
    {
        private DbInterpreter dbInterpreter;

        public frmIndexFragmentation(DbInterpreter dbInterpreter)
        {
            InitializeComponent();

            this.dgvData.AutoGenerateColumns = false;

            this.dbInterpreter = dbInterpreter;
        }

        private void frmIndexFragmentation_Load(object sender, EventArgs e)
        {
            this.LoadData();
        }

        private async void LoadData()
        {
            try
            {
                Analysiser analysiser = new Analysiser(this.dbInterpreter);

                this.loadingPanel.ShowLoading(this.dgvData);

                var results = (await analysiser.GetIndexFragmentations()).OrderByDescending(item => item.FragmentationPercent);

                this.dgvData.DataSource = results.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.loadingPanel.HideLoading();
            }
        }

        private void dgvData_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvData.ClearSelection();
        }

        private void tsmiRefresh_Click(object sender, EventArgs e)
        {
            this.LoadData();
        }

        private void tsmiSave_Click(object sender, EventArgs e)
        {
            if (this.dgvData == null)
            {
                this.dlgSave = new SaveFileDialog();
            }

            this.dlgSave.FileName = "";

            DialogResult result = this.dlgSave.ShowDialog();

            if (result == DialogResult.OK)
            {
                DataTable table = new DataTable();

                foreach (DataGridViewColumn column in this.dgvData.Columns)
                {
                    table.Columns.Add(new DataColumn() { ColumnName = column.HeaderText });
                }

                foreach (DataGridViewRow row in this.dgvData.Rows)
                {
                    var r = table.Rows.Add();

                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        r[cell.ColumnIndex] = cell.Value;
                    }
                }

                try
                {
                    DataExporter exporter = new DataExporter();

                    exporter.WriteToExcel(table, new ExportDataOption() { ShowColumnNames = true, FilePath = this.dlgSave.FileName });

                    MessageBox.Show("Saved.");
                }
                catch (Exception ex)
                {
                    string errMsg = ExceptionHelper.GetExceptionDetails(ex);

                    LogHelper.LogError(errMsg);

                    MessageBox.Show(errMsg);
                }
            }
        }

        private void dgvData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

        }

        private async void tsmiRebuildIndex_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure to rebuild the index?", "Confirm", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                Analysiser analysiser = new Analysiser(this.dbInterpreter);

                var selectedRow = this.dgvData.SelectedRows[0];

                var indexFragmentation = selectedRow.DataBoundItem as IndexFragmentation;

                var res = await analysiser.RebuildIndex(indexFragmentation);

                if (res.IsOK)
                {
                    MessageBox.Show("Rebuild scucceed.");

                    this.LoadData();
                }
                else
                {
                    MessageBox.Show(res.Message, "Rebuild failed");
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            var selectedRows = this.dgvData.SelectedRows;

            this.tsmiRebuildIndex.Visible = selectedRows.Count > 0;
        }
    }
}
