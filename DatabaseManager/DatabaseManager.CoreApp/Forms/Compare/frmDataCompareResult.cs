using DatabaseConverter.Core;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Core.Model;
using DatabaseManager.Helper;
using FontAwesome.Sharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Forms.Compare
{
    public partial class frmDataCompareResult : Form, IObserver<FeedbackInfo>
    {
        private DbInterpreter sourceDbInterpreter;
        private DbInterpreter targetDbInterpreter;
        private DataCompareResult result;
        private DataCompareOption option;
        private CancellationTokenSource cancellationTokenSource;
        private List<SchemaMappingInfo> schemaMappings = new List<SchemaMappingInfo>();
        private frmExportData exportDataForm;

        public frmDataCompareResult(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, DataCompareResult result, DataCompareOption option)
        {
            InitializeComponent();

            DataGridView.CheckForIllegalCrossThreadCalls = false;

            this.tsbCancel.Image = IconImageHelper.GetImageByFontType(IconChar.Cancel, IconFont.Solid, Color.Red);

            var exportImage = IconImageHelper.GetImage(IconChar.FileExport);

            this.btnExportDifferent.Image = exportImage;
            this.btnExportOnlyInSource.Image = exportImage;
            this.btnExportOnlyInTarget.Image = exportImage;
            this.btnExportIdentical.Image = exportImage;

            this.sourceDbInterpreter = sourceDbInterpreter;
            this.targetDbInterpreter = targetDbInterpreter;
            this.result = result;
            this.option = option;

            this.LoadData();
        }

        private void LoadData()
        {
            DataCompareDisplayMode displayMode = this.option.DisplayMode;

            bool showDifferent = displayMode.HasFlag(DataCompareDisplayMode.Different);
            bool showIdentical = displayMode.HasFlag(DataCompareDisplayMode.Indentical);
            bool showOnlyInSource = displayMode.HasFlag(DataCompareDisplayMode.OnlyInSource);
            bool showOnlyInTarget = displayMode.HasFlag(DataCompareDisplayMode.OnlyInTarget);

            if (!showDifferent)
            {
                this.tlvDifferences.Columns.Remove(this.colDifferentCount);
                this.tabControl1.TabPages.Remove(this.tabPageDifferent);
            }

            if (!showOnlyInSource)
            {
                this.tlvDifferences.Columns.Remove(this.colOnlyInSourceCount);
                this.tabControl1.TabPages.Remove(this.tabPageOnlyInsource);
            }

            if (!showOnlyInTarget)
            {
                this.tlvDifferences.Columns.Remove(this.colOnlyInTargetCount);
                this.tabControl1.TabPages.Remove(this.tabPageOnlyInTarget);
            }

            if (!showIdentical)
            {
                this.tabControl1.TabPages.Remove(this.tabPageIdentical);
            }

            var details = this.result.Details;

            List<DataCompareDifference> differences = new List<DataCompareDifference>();

            foreach (var detail in details)
            {
                DataCompareDifference difference = new DataCompareDifference();

                difference.Detail = detail;
                difference.Type = detail.SourceTable.Name;
                difference.TableName = detail.SourceTable.Name;
                difference.SourceRecordCount = detail.SourceTableRecordCount;
                difference.TargetRecordCount = detail.TargetTableRecordCount;
                difference.OnlyInSourceCount = detail.OnlyInSourceCount;
                difference.OnlyInTargetCount = detail.OnlyInTargetCount;
                difference.DifferentCount = detail.DifferentCount;
                difference.IdenticalCount = detail.IdenticalCount;
                difference.IsIdentical = detail.IsIndentical;

                differences.Add(difference);
            }

            var differentDetails = differences.Where(item => item.DifferentCount > 0 || item.OnlyInSourceCount > 0 || item.OnlyInTargetCount > 0);

            if (differentDetails.Any())
            {
                var difference = this.AddRootDifference("Different", differentDetails);

                differences.Add(difference);
            }

            if (details.Any(item => item.IsIndentical))
            {
                var difference = this.AddRootDifference("Equal", differences.Where(item => item.IsIdentical));

                differences.Add(difference);
            }

            var roots = differences.Where(item => item.IsRoot);

            this.tlvDifferences.Roots = roots;

            if (roots.Count() == 1)
            {
                this.tlvDifferences.ExpandAll();

                this.tlvDifferences.Refresh();
            }

            this.tlvDifferences.CanExpandGetter = delegate (object obj)
            {
                DataCompareDifference difference = obj as DataCompareDifference;
                return difference.IsRoot;
            };

            this.tlvDifferences.ChildrenGetter = delegate (object obj)
            {
                DataCompareDifference difference = obj as DataCompareDifference;
                return difference.SubDifferences;
            };
        }

        private void frmDataCompareResult_Load(object sender, EventArgs e)
        {
            var roots = this.tlvDifferences.Roots as ArrayList;

            if (roots != null && roots.Count > 0)
            {
                this.tlvDifferences.Expand(roots[0]);
            }
        }

        private DataCompareDifference AddRootDifference(string type, IEnumerable<DataCompareDifference> children)
        {
            DataCompareDifference difference = new DataCompareDifference() { Type = type };

            difference.SubDifferences.AddRange(children);

            foreach (var chilid in children)
            {
                chilid.Parent = difference;
            }

            return difference;
        }

        private void tlvDifferences_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.dgvDifferent.Columns.Clear();
            this.dgvDifferent.Rows.Clear();

            this.ShowDetail(1);
        }

        private void ShowDetail(long pageNumber)
        {
            DataCompareDifference difference = this.tlvDifferences.SelectedObject as DataCompareDifference;

            if (difference != null)
            {
                var detail = difference.Detail;

                if (detail == null)
                {
                    return;
                }

                DataCompareDisplayMode displayMode = this.option.DisplayMode;

                bool showDifferent = displayMode.HasFlag(DataCompareDisplayMode.Different);
                bool showIdentical = displayMode.HasFlag(DataCompareDisplayMode.Indentical);
                bool showOnlyInSource = displayMode.HasFlag(DataCompareDisplayMode.OnlyInSource);
                bool showOnlyInTarget = displayMode.HasFlag(DataCompareDisplayMode.OnlyInTarget);

                if (showDifferent && detail.HasDifferent)
                {
                    this.ShowDifferentData(detail, pageNumber);
                }

                if (showOnlyInSource && detail.OnlyInSourceCount > 0)
                {
                    this.ShowOnlyInSourceData(detail, pageNumber);
                }
                else
                {
                    this.dgvOnlyInSource.DataSource = null;
                }

                if (showOnlyInTarget && detail.OnlyInTargetCount > 0)
                {
                    this.ShowOnlyInTargetData(detail, pageNumber);
                }
                else
                {
                    this.dgvOnlyInTarget.DataSource = null;
                }

                if (showIdentical && detail.IdenticalCount > 0)
                {
                    this.ShowIdenticalData(detail, pageNumber);
                }
                else
                {
                    this.dgvIdentical.DataSource = null;
                }
            }
        }

        private async void ShowDifferentData(DataCompareResultDetail detail, long pageNumber)
        {
            this.dgvDifferent.Columns.Clear();
            this.dgvDifferent.Rows.Clear();

            var rows = detail.DifferentKeyRows;

            int pageSize = this.paginationDifferent.PageSize;

            this.paginationDifferent.TotalCount = rows.Count;
            this.paginationDifferent.PageCount = PaginationHelper.GetPageCount(rows.Count, pageSize);

            (DataTable Data, Dictionary<int, List<DataCompareValueInfo>> ValueInfos) result = await DataCompare.GetDifferentData(this.sourceDbInterpreter, this.targetDbInterpreter, detail, pageSize, pageNumber);
           
            DataTable dataTable = result.Data;           

            foreach (DataColumn column in dataTable.Columns)
            {
                var dataGridViewColumn = new DataGridViewTextBoxColumn() { Name = column.ColumnName, HeaderText = DataCompare.GetDifferentDataColumnDisplayText(column.ColumnName) };
                
                dataGridViewColumn.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
                dataGridViewColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                this.dgvDifferent.Columns.Add(dataGridViewColumn);
            }

            int rowIndex = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                DataGridViewRow gridRow = new DataGridViewRow();

                DataGridViewCell[] cells = new DataGridViewCell[this.dgvDifferent.Columns.Count];

                for (int i = 0; i < cells.Length; i++)
                {
                    cells[i] = new DataGridViewTextBoxCell();

                    DataCompareValueInfo valueInfo = result.ValueInfos[rowIndex][i];

                    cells[i].Value = valueInfo.Value;

                    if (valueInfo.IsDifferent)
                    {
                        cells[i].Style.Font = new Font(this.dgvDifferent.Font, FontStyle.Bold);
                    }
                }

                gridRow.Cells.AddRange(cells);

                this.dgvDifferent.Rows.Add(gridRow);

                rowIndex++;
            }

            this.dgvDifferent.ClearSelection();
        }

        private DataCompareResultDetail GetSelectedDataCompareResultDetail()
        {
            if (this.tlvDifferences.SelectedObject != null)
            {
                return (this.tlvDifferences.SelectedObject as DataCompareDifference)?.Detail;
            }

            return null;
        }

        private List<DataRow> GetPagedKeyRows(List<DataRow> rows, int pageSize, long pageNumber)
        {
            return DataCompare.GetPagedKeyRows(rows, pageSize, pageNumber);
        }

        private async void ShowOnlyInSourceData(DataCompareResultDetail detail, long pageNumber)
        {
            var rows = detail.OnlyInSourceKeyRows;

            int pageSize = this.paginationOnlyInSource.PageSize;

            this.paginationOnlyInSource.TotalCount = rows.Count;
            this.paginationOnlyInSource.PageCount = PaginationHelper.GetPageCount(rows.Count, pageSize);

            DataTable sourceDataTable = await this.GetOnlyInSourceDataTable(detail, pageNumber);

            this.dgvOnlyInSource.DataSource = DataGridViewHelper.ConvertDataTable(sourceDataTable);
        }

        private async Task<DataTable> GetOnlyInSourceDataTable(DataCompareResultDetail detail, long pageNumber, List<DataExportColumn> exportColumns = null)
        {
            return await this.GetPagedDataTable(detail, detail.OnlyInSourceKeyRows, this.paginationOnlyInSource.PageSize, this.paginationOnlyInSource.PageCount, pageNumber, this.sourceDbInterpreter, detail.SourceTable, detail.SourceTableColumns, exportColumns);
        }

        private async void ShowOnlyInTargetData(DataCompareResultDetail detail, long pageNumber)
        {
            var rows = detail.OnlyInTargetKeyRows;

            int pageSize = this.paginationOnlyInTarget.PageSize;

            this.paginationOnlyInTarget.TotalCount = rows.Count;
            this.paginationOnlyInTarget.PageCount = PaginationHelper.GetPageCount(rows.Count, pageSize);

            DataTable targetDataTable = await this.GetOnlyInTargetDataTable(detail, pageNumber);

            this.dgvOnlyInTarget.DataSource = DataGridViewHelper.ConvertDataTable(targetDataTable);
        }

        private async Task<DataTable> GetOnlyInTargetDataTable(DataCompareResultDetail detail, long pageNumber, List<DataExportColumn> exportColumns = null)
        {
            return await this.GetPagedDataTable(detail, detail.OnlyInTargetKeyRows, this.paginationOnlyInTarget.PageSize, this.paginationOnlyInTarget.PageCount, pageNumber, this.targetDbInterpreter, detail.TargetTable, detail.TargetTableColumns, exportColumns);
        }

        private async void ShowIdenticalData(DataCompareResultDetail detail, long pageNumber)
        {
            var rows = detail.IdenticalKeyRows;

            int pageSize = this.paginationIdentical.PageSize;

            this.paginationIdentical.TotalCount = rows.Count;
            this.paginationIdentical.PageCount = PaginationHelper.GetPageCount(rows.Count, pageSize);

            DataTable sourceDataTable = await this.GetIdenticalDataTable(detail, pageNumber);

            this.dgvIdentical.DataSource = DataGridViewHelper.ConvertDataTable(sourceDataTable);
        }

        private async Task<DataTable> GetIdenticalDataTable(DataCompareResultDetail detail, long pageNumber, List<DataExportColumn> exportColumns = null)
        {
            return await this.GetPagedDataTable(detail, detail.IdenticalKeyRows, this.paginationIdentical.PageSize, this.paginationIdentical.PageCount, pageNumber, this.sourceDbInterpreter, detail.SourceTable, detail.SourceTableColumns, exportColumns);
        }

        private async Task<DataTable> GetPagedDataTable(DataCompareResultDetail detail, List<DataRow> keyRows, int pageSize, long pageCount, long pageNumber, DbInterpreter dbInterpreter,
            Table table, List<TableColumn> tableColumns, List<DataExportColumn> exportColumns = null)
        {
            var pagedKeyRows = this.GetPagedKeyRows(keyRows, pageSize, pageNumber);

            string whereCondition = DataCompare.GetKeyColumnWhereCondition(dbInterpreter, pagedKeyRows, detail.KeyColumns);

            string orderColumns = dbInterpreter.GetQuotedColumnNames(detail.KeyColumns);

            List<TableColumn> columns = null;

            bool isForExporting = false;

            if (exportColumns != null)
            {
                isForExporting = true;

                columns = tableColumns.Where(item => exportColumns.Any(t => t.ColumnName == item.Name)).ToList();
            }
            else
            {
                columns = tableColumns;
            }

            long count = pageNumber == pageCount ? keyRows.Count : pageNumber * pageSize;

            if (isForExporting)
            {
                this.Feedback($@"Reading table ""{table.Name}"" {count}/{keyRows.Count}...", true);
            }

            DataTable dataTable = await dbInterpreter.GetPagedDataTableAsync(dbInterpreter.CreateConnection(), table, columns, orderColumns, pageSize, 1, whereCondition);

            if (isForExporting && pageNumber == pageCount)
            {
                this.Feedback($@"End read table ""{table.Name}"".", true);
            }

            return dataTable;
        }

        private void paginationDifferent_OnPageNumberChanged(long pageNumber)
        {
            var detail = this.GetSelectedDataCompareResultDetail();

            if (detail != null)
            {
                this.ShowDifferentData(detail, pageNumber);
            }
        }

        private void dgvDifferent_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void dgvOnlyInSource_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvOnlyInSource.ClearSelection();
        }

        private void dgvOnlyInSource_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void paginationOnlyInSource_OnPageNumberChanged(long pageNumber)
        {
            var detail = this.GetSelectedDataCompareResultDetail();

            if (detail != null)
            {
                this.ShowOnlyInSourceData(detail, pageNumber);
            }
        }

        private void dgvDifferent_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewHelper.FormatCell(this.dgvDifferent, e, true, false);
        }

        private void dgvOnlyInSource_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewHelper.FormatCell(this.dgvOnlyInSource, e, true, true);
        }

        private void dgvOnlyInTarget_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewHelper.FormatCell(this.dgvOnlyInTarget, e, true, true);
        }

        private void dgvOnlyInTarget_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvOnlyInTarget.ClearSelection();
        }

        private void dgvOnlyInTarget_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void dgvIdentical_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewHelper.FormatCell(this.dgvIdentical, e, true, true);
        }

        private void dgvIdentical_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvIdentical.ClearSelection();
        }

        private void dgvIdentical_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void paginationOnlyInTarget_OnPageNumberChanged(long pageNumber)
        {
            var detail = this.GetSelectedDataCompareResultDetail();

            if (detail != null)
            {
                this.ShowOnlyInTargetData(detail, pageNumber);
            }
        }

        private void paginationIdentical_OnPageNumberChanged(long pageNumber)
        {
            var detail = this.GetSelectedDataCompareResultDetail();

            if (detail != null)
            {
                this.ShowIdenticalData(detail, pageNumber);
            }
        }

        private void tbsiSelectAll_Click(object sender, EventArgs e)
        {
            this.CheckAllListViewItem(true);
        }

        private void tsmiUnselectAll_Click(object sender, EventArgs e)
        {
            this.CheckAllListViewItem(false);
        }

        private void CheckAllListViewItem(bool @checked)
        {
            if (@checked)
            {
                this.tlvDifferences.CheckObjects(this.tlvDifferences.Roots as ArrayList);
            }
            else
            {
                this.tlvDifferences.UncheckObjects(this.tlvDifferences.Roots as ArrayList);
            }
        }

        private void tlvDifferences_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int itemCount = this.tlvDifferences.Items.Count;

            this.tsmiUnselectAll.Visible = itemCount > 0;
            this.tsmiUnselectAll.Visible = itemCount > 0;
        }

        private List<DataCompareResultDetail> GetSelectedDataCompareResultDetails()
        {
            List<DataCompareResultDetail> details = new List<DataCompareResultDetail>();

            var array = this.tlvDifferences.CheckedObjects as ArrayList;

            foreach (var item in array)
            {
                var difference = item as DataCompareDifference;

                if (!difference.IsRoot)
                {
                    details.Add(difference.Detail);
                }
            }

            return details.OrderBy(item => item.Order).ToList();
        }

        private bool ValidateSelectedItems()
        {
            var details = this.GetSelectedDataCompareResultDetails();

            if (details.Count == 0)
            {
                MessageBox.Show("Please check the items from the below list view.");
                return false;
            }
            else if (details.All(item => item.IsIndentical))
            {
                MessageBox.Show("All items are identical, it's no need to do this.");
                return false;
            }

            return true;
        }

        private void tsbGenerateScript_Click(object sender, EventArgs e)
        {
            if (!this.ValidateSelectedItems())
            {
                return;
            }

            DialogResult result = this.saveFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                Task.Run(() => { this.GenerateScripts(this.saveFileDialog1.FileName); });
            }
        }

        private async void GenerateScripts(string filePath)
        {
            this.Feedback("");

            var details = this.GetSelectedDataCompareResultDetails().Where(item => !item.IsIndentical).ToList();

            DataCompare dataCompare = new DataCompare(this.sourceDbInterpreter, this.targetDbInterpreter, null, this.option);

            dataCompare.Subscribe(this);

            this.cancellationTokenSource = new CancellationTokenSource();

            var token = this.cancellationTokenSource.Token;

            this.tsbCancel.Enabled = true;

            string script = await dataCompare.GenerateScripts(details, token);

            if (!token.IsCancellationRequested)
            {
                File.WriteAllText(filePath, script);

                MessageBox.Show("Generation is finished.");
            }
            else
            {
                this.Feedback("Task has been canceled.");
            }

            this.tsbCancel.Enabled = false;
        }

        private void tsbSynchronize_Click(object sender, EventArgs e)
        {
            if (!this.ValidateSelectedItems())
            {
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure to synchronize the source data to target?", "Confirm", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }

            Task.Run(() => { this.Synchronize(); });
        }

        private async void Synchronize()
        {
            this.Feedback("");

            DbInterpreter sourceInterpreter = DbInterpreterHelper.GetDbInterpreter(this.sourceDbInterpreter.DatabaseType, this.sourceDbInterpreter.ConnectionInfo);
            DbInterpreter targetInterpreter = DbInterpreterHelper.GetDbInterpreter(this.targetDbInterpreter.DatabaseType, this.targetDbInterpreter.ConnectionInfo);

            DatabaseType sourceDatabaseType = this.sourceDbInterpreter.DatabaseType;
            DatabaseType targetDatabaseType = this.targetDbInterpreter.DatabaseType;

            (List<string> SourceSchemas, List<string> TargetSchemas) schemasResult = await DbConverter.GetSourceAndTargetSchemas(sourceDatabaseType, targetDatabaseType, sourceDbInterpreter.ConnectionInfo, targetDbInterpreter.ConnectionInfo); ;

            List<string> sourceSchemas = schemasResult.SourceSchemas;
            List<string> targetSchemas = schemasResult.TargetSchemas;

            if (this.schemaMappings.Count == 0 && sourceDatabaseType == targetDatabaseType)
            {
                if (SettingManager.Setting.AutoMapSchemaIfSourceAndTargetIsSameDatabaseType)
                {
                    this.schemaMappings = DbConverter.GetAutoMappedSchemas(sourceSchemas, targetSchemas);
                }
            }

            bool isTargetHasOnlyOneSchema = targetSchemas.Count == 1;

            if (!isTargetHasOnlyOneSchema)
            {
                frmSchemaMapping form = new frmSchemaMapping()
                {
                    Mappings = this.schemaMappings,
                    SourceSchemas = sourceSchemas,
                    TargetSchemas = targetSchemas
                };

                if (form.ShowDialog() == DialogResult.OK)
                {
                    this.schemaMappings = form.Mappings;
                }
            }

            var details = this.GetSelectedDataCompareResultDetails().Where(item => !item.IsIndentical).ToList();

            DataCompare dataCompare = new DataCompare(this.sourceDbInterpreter, this.targetDbInterpreter, null, this.option);

            dataCompare.Subscribe(this);

            this.cancellationTokenSource = new CancellationTokenSource();

            var token = this.cancellationTokenSource.Token;

            this.tsbCancel.Enabled = true;

            DataSynchronizeResult result = await dataCompare.Synchronize(details, token, schemaMappings);

            if (!token.IsCancellationRequested)
            {
                if (result.IsOK)
                {
                    MessageBox.Show("Synchronization is finished.");
                }
                else
                {
                    MessageBox.Show(result.Message, "Synchronization is failed!");
                }
            }
            else
            {
                this.Feedback("Task has been canceled.");
            }

            this.tsbCancel.Enabled = false;
        }

        private void Feedback(string message, bool forExporting = false)
        {
            this.Feedback(new FeedbackInfo() { Message = message });
        }

        private void Feedback(FeedbackInfo info, bool forExporting = false)
        {
            try
            {
                if(!forExporting)
                {
                    Action show = () =>
                    {
                        this.txtMessage.Text = info.Message;

                        if (info.InfoType == FeedbackInfoType.Error)
                        {
                            this.txtMessage.ForeColor = Color.Red;
                            this.toolTip1.SetToolTip(this.txtMessage, info.Message);
                        }
                        else
                        {
                            this.txtMessage.ForeColor = Color.Black;
                        }
                    };

                    if (this.InvokeRequired)
                    {
                        this.Invoke(show);
                    }
                    else
                    {
                        show();
                    }
                }
                else
                {
                    if (this.exportDataForm != null)
                    {
                        this.exportDataForm.Feedback(info);
                    }
                }               
            }
            catch (Exception ex)
            {
            }
        }

        private void tlvDifferences_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            var difference = this.tlvDifferences.GetModelObject(e.Index) as DataCompareDifference;

            if (difference != null && difference.IsRoot)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    this.tlvDifferences.CheckObjects(difference.SubDifferences);
                }
                else
                {
                    this.tlvDifferences.UncheckObjects(difference.SubDifferences);
                }
            }
        }

        private void tlvDifferences_FormatRow(object sender, BrightIdeasSoftware.FormatRowEventArgs e)
        {
            DataCompareDifference difference = (DataCompareDifference)e.Model;

            if (difference != null && difference.IsRoot)
            {
                e.Item.ForeColor = Color.Blue;
                e.Item.Font = new Font(this.tlvDifferences.Font, FontStyle.Bold);
            }
        }

        private async void tsbCancel_Click(object sender, EventArgs e)
        {
            if (this.cancellationTokenSource != null)
            {
                await this.cancellationTokenSource.CancelAsync();
            }
        }

        private void btnExportDifferent_Click(object sender, EventArgs e)
        {
            var detail = this.GetSelectedDataCompareResultDetail();

            if(detail == null)
            {
                return;
            }

            if(this.paginationDifferent.TotalCount == 0)
            {
                MessageBox.Show("No record to export.");
                return;
            }

            this.saveFileDialog1.Filter = "excel file|*.xlsx";

            this.saveFileDialog1.FileName = detail.SourceTable.Name;

            DialogResult result = this.saveFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                Task.Run(() => { this.ExportDifferentData(detail); });
            }
        }

        private async void ExportDifferentData(DataCompareResultDetail detail)
        {
            string filePath = this.saveFileDialog1.FileName;

            DataExportResult exportResult = await DataCompare.ExportDifferentData(this.sourceDbInterpreter, this.targetDbInterpreter, detail, (int)this.paginationDifferent.TotalCount, 1, filePath);

            if (exportResult.IsOK)
            {
                FileHelper.SelectFileInExplorer(exportResult.FilePath);
            }
            else
            {
                MessageBox.Show(exportResult.Message);
            }
        }

        private void btnExportOnlyInSource_Click(object sender, EventArgs e)
        {
            this.ExportData(this.dgvOnlyInSource, this.paginationOnlyInSource.PageCount, this.paginationOnlyInSource.PageNumber, this.GetDataTableForExportingOnlyInSource);
        }

        private async Task<DataTable> GetDataTableForExportingOnlyInSource(ExportSpecificDataOption option, List<DataExportColumn> columns)
        {
            return await this.GetDataTableForExporting(this.dgvOnlyInSource, this.paginationOnlyInSource.PageCount, option, columns, this.GetOnlyInSourceDataTable);
        }

        private void btnExportOnlyInTarget_Click(object sender, EventArgs e)
        {
            this.ExportData(this.dgvOnlyInTarget, this.paginationOnlyInTarget.PageCount, this.paginationOnlyInTarget.PageNumber, this.GetDataTableForExportingOnlyInTarget);
        }

        private async Task<DataTable> GetDataTableForExportingOnlyInTarget(ExportSpecificDataOption option, List<DataExportColumn> columns)
        {
            return await this.GetDataTableForExporting(this.dgvOnlyInTarget, this.paginationOnlyInTarget.PageCount, option, columns, this.GetOnlyInTargetDataTable);
        }

        private void btnExportIdentical_Click(object sender, EventArgs e)
        {
            this.ExportData(this.dgvIdentical, this.paginationIdentical.PageCount, this.paginationIdentical.PageNumber, this.GetDataTableForExportingIdentical);
        }

        private void ExportData(DataGridView dataGridView, long pageCount, long pageNumber, ExportDataRequiredHandler handler)
        {
            var detail = this.GetSelectedDataCompareResultDetail();

            if (detail == null)
            {
                return;
            }

            DataTable table = dataGridView.DataSource as DataTable;

            if (table == null)
            {
                MessageBox.Show("No data to export.");
                return;
            }

            ExportSpecificDataOption option = new ExportSpecificDataOption();

            option.PageNumbers.Add(pageNumber);
            option.PageCount = pageCount;

            DataTable dataTable = table.Clone();

            string tableName = null;

            if (dataGridView.Name == this.dgvOnlyInSource.Name || dataGridView.Name == this.dgvIdentical.Name)
            {
                tableName = detail.SourceTable.Name;
            }
            else if (dataGridView.Name == this.dgvOnlyInTarget.Name)
            {
                tableName = detail.TargetTable.Name;
            }

            dataTable.TableName = tableName;

            this.exportDataForm = new frmExportData(dataTable, option);

            this.exportDataForm.ExportDataRequired += handler;

            this.exportDataForm.ShowDialog();
        }

        private async Task<DataTable> GetDataTableForExportingIdentical(ExportSpecificDataOption option, List<DataExportColumn> columns)
        {
            return await this.GetDataTableForExporting(this.dgvIdentical, this.paginationIdentical.PageCount, option, columns, this.GetIdenticalDataTable);
        }

        private async Task<DataTable> GetDataTableForExporting(DataGridView dataGridView, long pageCount, ExportSpecificDataOption option, List<DataExportColumn> columns,
            Func<DataCompareResultDetail, long, List<DataExportColumn>, Task<DataTable>> funcGetTable)
        {
            DataTable dataTable = (dataGridView.DataSource as DataTable).Clone();

            var detail = this.GetSelectedDataCompareResultDetail();

            if (detail != null && option != null)
            {
                if (option.ExportAllThatMeetCondition)
                {
                    option.PageNumbers.Clear();

                    for (int i = 1; i <= pageCount; i++)
                    {
                        option.PageNumbers.Add(i);
                    }
                }

                List<long> pageNumbers = option.PageNumbers;

                foreach (var pageNumber in pageNumbers)
                {
                    var table = await funcGetTable(detail, pageNumber, columns);

                    dataTable.Merge(table);
                }
            }

            return dataTable;
        }

        #region IObserver<FeedbackInfo>
        void IObserver<FeedbackInfo>.OnCompleted()
        {
        }
        void IObserver<FeedbackInfo>.OnError(Exception error)
        {
        }
        void IObserver<FeedbackInfo>.OnNext(FeedbackInfo info)
        {
            this.Feedback(info);
        }
        #endregion
    }   
}
