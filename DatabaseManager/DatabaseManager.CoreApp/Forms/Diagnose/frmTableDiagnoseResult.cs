using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core.Model;
using DatabaseManager.Export;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmTableDiagnoseResult : Form
    {
        public DatabaseType DatabaseType { get; set; }
        public ConnectionInfo ConnectionInfo { get; set; }

        public frmTableDiagnoseResult()
        {
            InitializeComponent();
        }

        private void frmTableDiagnoseResult_Load(object sender, EventArgs e)
        {
            this.dgvResult.ClearSelection();
        }

        public void LoadResult(TableDiagnoseResult result)
        {
            this.colInvalidRecordCount.Visible = result.ShowRecordCount;

            int schemaCount = result.Details.Select(item => item.DatabaseObject.Schema).Distinct().Count();

            foreach (TableDiagnoseResultDetail item in result.Details)
            {
                int rowIndex = this.dgvResult.Rows.Add();

                DataGridViewRow row = this.dgvResult.Rows[rowIndex];

                string schema = item.DatabaseObject.Schema;
                string tableName = this.GetTableName(item.DatabaseObject);

                row.Cells[this.colTableName.Name].Value = schemaCount > 1 ? $"{schema}.{tableName}" : tableName; ;
                row.Cells[this.colObjectType.Name].Value = item.DatabaseObject.GetType().Name;
                row.Cells[this.colObjectName.Name].Value = item.DatabaseObject.Name;
                row.Cells[this.colInvalidRecordCount.Name].Value = item.RecordCount;

                row.Tag = item;
            }
        }

        private string GetTableName(DatabaseObject dbObject)
        {
            if (dbObject is TableChild tableChild)
            {
                return tableChild.TableName;
            }
            else if (dbObject is Table table)
            {
                return table.Name;
            }

            return string.Empty;
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

        private void dgvResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (e.ColumnIndex == this.colInvalidRecordCount.Index)
            {
                TableDiagnoseResultDetail resultItem = this.dgvResult.Rows[e.RowIndex].Tag as TableDiagnoseResultDetail;

                string sql = resultItem.Sql;

                if(!string.IsNullOrEmpty(sql))
                {
                    frmSqlQuery form = new frmSqlQuery() { ReadOnly = true, ShowEditorMessage = false, SplitterDistance = 80 };

                    form.Init();

                    DatabaseObjectDisplayInfo displayInfo = new DatabaseObjectDisplayInfo()
                    {
                        DatabaseType = this.DatabaseType,
                        ConnectionInfo = this.ConnectionInfo,
                        Content = sql
                    };

                    form.Show();

                    form.Query(displayInfo);
                }               
            }
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
