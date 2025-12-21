using DatabaseConverter.Core;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Core.Model;
using DatabaseManager.Core.Model.Compare;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using KellermanSoftware.CompareNetObjects;
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
        private List<SchemaMappingInfo> schemaMappings = new List<SchemaMappingInfo>();

        public frmDataCompareResult(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, DataCompareResult result, DataCompareOption option)
        {
            InitializeComponent();

            DataGridView.CheckForIllegalCrossThreadCalls = false;

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

            if (!showIdentical)
            {
                this.tlvDifferences.Columns.Remove(this.colIdenticalCount);
                this.tabControl1.TabPages.Remove(this.tabPageIdentical);
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

            var pagedKeyRows = this.GetPagedKeyRows(rows, pageSize, pageNumber);

            string whereCondition = DataCompare.GetKeyColumnWhereCondition(this.sourceDbInterpreter, pagedKeyRows, detail.KeyColumns);

            string orderColumns = this.sourceDbInterpreter.GetQuotedColumnNames(detail.KeyColumns);

            DataTable sourceDataTable = await this.sourceDbInterpreter.GetPagedDataTableAsync(this.sourceDbInterpreter.CreateConnection(), detail.SourceTable, detail.SameTableColumns, orderColumns, pageSize, 1, whereCondition);
            DataTable targetDataTable = await this.sourceDbInterpreter.GetPagedDataTableAsync(this.targetDbInterpreter.CreateConnection(), detail.TargetTable, detail.SameTableColumns, orderColumns, pageSize, 1, whereCondition);

            List<DataCompareDifferentRow> differentRows = new List<DataCompareDifferentRow>();

            for (int i = 0; i < sourceDataTable.Rows.Count; i++)
            {
                DataRow sourceRow = sourceDataTable.Rows[i];
                DataRow targetRow = targetDataTable.Rows[i];

                Dictionary<string, (object, object)> dict = new Dictionary<string, (object, object)>();

                foreach (var column in detail.SameTableColumns.Where(item => !detail.KeyColumns.Any(t => t.Name == item.Name)))
                {
                    string columnName = column.Name;

                    object sourceValue = sourceRow[columnName];
                    object targetValue = targetRow[columnName];

                    if (!Equals(sourceValue, targetValue))
                    {
                        dict.Add(columnName, (sourceValue, targetValue));
                    }
                }

                if (dict.Any())
                {
                    differentRows.Add(new DataCompareDifferentRow() { RowIndex = i, KeyRow = pagedKeyRows[i], Details = dict });
                }
            }

            foreach (var column in detail.KeyColumns)
            {
                this.dgvDifferent.Columns.Add(new DataGridViewTextBoxColumn() { Name = column.Name, HeaderText = column.Name });
            }

            var diffColumnNames = differentRows.SelectMany(item => item.Details.Select(t => t.Key)).Distinct();

            foreach (var name in diffColumnNames)
            {
                this.dgvDifferent.Columns.Add(new DataGridViewTextBoxColumn() { Name = name + "_source", HeaderText = name + "→", Tag = name });
                this.dgvDifferent.Columns.Add(new DataGridViewTextBoxColumn() { Name = name + "_target", HeaderText = "→" + name, Tag = name });
            }

            foreach (DataGridViewColumn column in this.dgvDifferent.Columns)
            {
                column.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
            }

            foreach (var row in differentRows)
            {
                int rowIndex = row.RowIndex;

                List<ValueInfo> valueInfos = new List<ValueInfo>();

                DataRow keyRow = row.KeyRow;

                DataGridViewRow gridRow = new DataGridViewRow();

                foreach (var keyColumn in detail.KeyColumns)
                {
                    valueInfos.Add(new ValueInfo() { Value = keyRow[keyColumn.Name] });
                }

                for (int i = detail.KeyColumns.Count; i < this.dgvDifferent.ColumnCount; i++)
                {
                    string columnName = this.dgvDifferent.Columns[i].Tag.ToString();

                    if (row.Details.ContainsKey(columnName))
                    {
                        var v = row.Details[columnName];

                        valueInfos.Add(new ValueInfo() { Value = v.Item1, IsDifferent = true });
                        valueInfos.Add(new ValueInfo() { Value = v.Item2, IsDifferent = true });
                    }
                    else
                    {
                        valueInfos.Add(new ValueInfo() { Value = sourceDataTable.Rows[rowIndex][columnName] });
                    }
                }

                DataGridViewCell[] cells = new DataGridViewCell[this.dgvDifferent.Columns.Count];

                for (int i = 0; i < cells.Length; i++)
                {
                    cells[i] = new DataGridViewTextBoxCell();

                    ValueInfo valueInfo = valueInfos[i];

                    cells[i].Value = valueInfo.Value;

                    if (valueInfo.IsDifferent)
                    {
                        cells[i].Style.Font = new Font(this.dgvDifferent.Font, FontStyle.Bold);
                    }
                }

                gridRow.Cells.AddRange(cells);

                this.dgvDifferent.Rows.Add(gridRow);

                this.dgvDifferent.ClearSelection();
            }
        }

        private DataCompareResultDetail GetSelectedDataCompareResultDetail()
        {
            if (this.tlvDifferences.SelectedObject != null)
            {
                return (this.tlvDifferences.SelectedObject as DataCompareDifference)?.Detail;
            }

            return null;
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

            var pagedKeyRows = this.GetPagedKeyRows(rows, pageSize, pageNumber);

            string whereCondition = DataCompare.GetKeyColumnWhereCondition(this.sourceDbInterpreter, pagedKeyRows, detail.KeyColumns);

            string orderColumns = this.sourceDbInterpreter.GetQuotedColumnNames(detail.KeyColumns);

            DataTable sourceDataTable = await this.sourceDbInterpreter.GetPagedDataTableAsync(this.sourceDbInterpreter.CreateConnection(), detail.SourceTable, detail.SourceTableColumns, orderColumns, pageSize, 1, whereCondition);

            this.dgvOnlyInSource.DataSource = DataGridViewHelper.ConvertDataTable(sourceDataTable);
        }

        private async void ShowOnlyInTargetData(DataCompareResultDetail detail, long pageNumber)
        {
            var rows = detail.OnlyInTargetKeyRows;

            int pageSize = this.paginationOnlyInTarget.PageSize;

            this.paginationOnlyInTarget.TotalCount = rows.Count;
            this.paginationOnlyInTarget.PageCount = PaginationHelper.GetPageCount(rows.Count, pageSize);

            var pagedKeyRows = this.GetPagedKeyRows(rows, pageSize, pageNumber);

            string whereCondition = DataCompare.GetKeyColumnWhereCondition(this.targetDbInterpreter, pagedKeyRows, detail.KeyColumns);

            string orderColumns = this.sourceDbInterpreter.GetQuotedColumnNames(detail.KeyColumns);

            DataTable targetDataTable = await this.sourceDbInterpreter.GetPagedDataTableAsync(this.targetDbInterpreter.CreateConnection(), detail.TargetTable, detail.TargetTableColumns, orderColumns, pageSize, 1, whereCondition);

            this.dgvOnlyInTarget.DataSource = DataGridViewHelper.ConvertDataTable(targetDataTable);
        }

        private async void ShowIdenticalData(DataCompareResultDetail detail, long pageNumber)
        {
            var rows = detail.IdenticalKeyRows;

            int pageSize = this.paginationIdentical.PageSize;

            this.paginationIdentical.TotalCount = rows.Count;
            this.paginationIdentical.PageCount = PaginationHelper.GetPageCount(rows.Count, pageSize);

            var pagedKeyRows = this.GetPagedKeyRows(rows, pageSize, pageNumber);

            string whereCondition = DataCompare.GetKeyColumnWhereCondition(this.sourceDbInterpreter, pagedKeyRows, detail.KeyColumns);

            string orderColumns = this.sourceDbInterpreter.GetQuotedColumnNames(detail.KeyColumns);

            DataTable sourceDataTable = await this.sourceDbInterpreter.GetPagedDataTableAsync(this.sourceDbInterpreter.CreateConnection(), detail.SourceTable, detail.SameTableColumns, orderColumns, pageSize, 1, whereCondition);

            this.dgvIdentical.DataSource = DataGridViewHelper.ConvertDataTable(sourceDataTable);
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
                this.GenerateScripts(this.saveFileDialog1.FileName);
            }
        }

        private async void GenerateScripts(string filePath)
        {
            this.Feedback("");

            var details = this.GetSelectedDataCompareResultDetails().Where(item => !item.IsIndentical).ToList();

            DataCompare dataCompare = new DataCompare(this.sourceDbInterpreter, this.targetDbInterpreter, null, this.option);

            dataCompare.Subscribe(this);

            string script = await dataCompare.GenerateScripts(details);

            File.WriteAllText(filePath, script);

            MessageBox.Show("Generation is finished.");
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

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var token = cancellationTokenSource.Token;

            DataSynchronizeResult result = await dataCompare.Synchronize(details, token, schemaMappings);

            if (result.IsOK)
            {
                MessageBox.Show("Synchronization is finished.");
            }
            else
            {
                MessageBox.Show(result.Message, "Synchronization is failed!");
            }
        }

        private void Feedback(string message)
        {
            this.Feedback(new FeedbackInfo() { Message = message });
        }

        private void Feedback(FeedbackInfo info)
        {
            try
            {
                this.Invoke(new Action(() =>
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
                }));
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

    public class DataCompareDifferentRow
    {
        public int RowIndex { get; set; }
        public DataRow KeyRow { get; set; }
        public Dictionary<string, (object, object)> Details = new Dictionary<string, (object, object)>();
    }

    public class ValueInfo()
    {
        public object Value { get; set; }
        public bool IsDifferent { get; set; }
    }
}
