using DatabaseInterpreter.Utility;
using DatabaseManager.Core.Model;
using DatabaseManager.Export;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmTableRecordCount : Form
    {

        public frmTableRecordCount()
        {
            InitializeComponent();
        }

        private void frmTableRecordCount_Load(object sender, EventArgs e)
        {
            this.dgvResult.ClearSelection();
        }

        public void LoadData(IEnumerable<TableRecordCount> records)
        {
            int schemaCount = records.Select(item => item.Schema).Distinct().Count();

            foreach (TableRecordCount item in records.OrderByDescending(item=>item.RecordCount))
            {
                int rowIndex = this.dgvResult.Rows.Add();

                DataGridViewRow row = this.dgvResult.Rows[rowIndex];

                row.Cells[this.colTableName.Name].Value = schemaCount>1 ? $"{item.Schema}.{item.TableName}" : item.TableName;
                row.Cells[this.colRecordCount.Name].Value = item.RecordCount;               

                row.Tag = item;
            }
        }      

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
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
            this.dgvResult.ClipboardCopyMode = mode;

            Clipboard.SetDataObject(this.dgvResult.GetClipboardContent());
        }

        private void dgvResult_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                bool canCopy = this.dgvResult.GetCellCount(DataGridViewElementStates.Selected) > 0;

                this.tsmiCopy.Enabled = canCopy;
                this.tsmiCopyWithHeader.Enabled = canCopy;

                this.contextMenuStrip1.Show(this.dgvResult, e.Location);
            }
        }

        private void dgvResult_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvResult.ClearSelection();
        }

        private void dgvResult_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            this.dgvResult.ClearSelection();
        }      

        private void tsmiSave_Click(object sender, EventArgs e)
        {
            this.Save();
        }

        private void Save()
        {
            if (this.dgvResult == null)
            {
                this.dlgSave = new SaveFileDialog();
            }

            this.dlgSave.FileName = "";

            DialogResult result = this.dlgSave.ShowDialog();

            if (result == DialogResult.OK)
            {
                DataTable table = new DataTable();

                foreach (DataGridViewColumn column in this.dgvResult.Columns)
                {
                    table.Columns.Add(new DataColumn() { ColumnName = column.HeaderText });
                }

                foreach (DataGridViewRow row in this.dgvResult.Rows)
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

                    exporter.WriteToExcel(table, new FileUtility.ExportDataOption() { ShowColumnNames = true, FilePath = this.dlgSave.FileName });

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

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Save();
        }
    }
}
