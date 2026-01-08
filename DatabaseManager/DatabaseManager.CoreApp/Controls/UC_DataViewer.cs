using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Core.Model;
using DatabaseManager.Forms;
using DatabaseManager.Helper;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using View = DatabaseInterpreter.Model.View;

namespace DatabaseManager.Controls
{
    public delegate void DataFilterHandler(object sender);

    public partial class UC_DataViewer : UserControl, IDbObjContentDisplayer
    {
        private DatabaseObjectDisplayInfo displayInfo;
        private int sortedColumnIndex = -1;
        private SortOrder sortOrder = SortOrder.None;
        private bool isSorting = false;
        private DbInterpreter dbInterpreter;
        private QueryConditionBuilder conditionBuilder;
        private QuickQueryConditionBuilder quickQueryConditionBuilder;
        private CancellationTokenSource cancellationTokenSource;

        public IEnumerable<DataGridViewColumn> Columns => this.dgvData.Columns.Cast<DataGridViewColumn>();
        public QueryConditionBuilder ConditionBuilder => this.conditionBuilder;
        public DataFilterHandler OnDataFilter;

        public UC_DataViewer()
        {
            InitializeComponent();

            this.btnFilter.Image = IconImageHelper.GetImage(IconChar.Filter);
            this.btnExport.Image = IconImageHelper.GetImage(IconChar.FileExport);

            this.pagination.PageSize = 50;

            this.dgvData.AutoGenerateColumns = true;

            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, this.dgvData, new object[] { true });
        }

        public void Show(DatabaseObjectDisplayInfo displayInfo)
        {
            this.displayInfo = displayInfo;

            this.ShowData();

            this.uc_QuickFilter.Query += UC_QuickFilter_Query;        
        }

        private void ShowData(long pageNumber = 1, bool isSort = false)
        {
            Task.Run(() =>
            {
                this.LoadData(pageNumber, isSort);
            });
        }

        public async void LoadData(long pageNumber = 1, bool isSort = false)
        {
            this.pagination.PageNumber = pageNumber;          

            int pageSize = this.pagination.PageSize;

            var option = new DbInterpreterOption() { ShowTextForGeometry = true };

            var connectionInfo = displayInfo.ConnectionInfo;

            connectionInfo.NeedCheckServerVersion = displayInfo.DatabaseType == DatabaseType.SqlServer || displayInfo.DatabaseType == DatabaseType.Oracle;

            this.dbInterpreter = DbInterpreterHelper.GetDbInterpreter(displayInfo.DatabaseType, connectionInfo, option);

            try
            {
                this.loadingPanel.ShowLoading(this.dgvData);

                string orderColumns = this.GetOrderColumns();
                string conditionClause = await this.GetConditionClause();

                DatabaseObject dbObject = this.displayInfo.DatabaseObject;
                bool isForView = false;

                if (dbObject is View)
                {
                    dbObject = ObjectHelper.CloneObject<Table>(dbObject);

                    isForView = true;
                }

                this.cancellationTokenSource = new CancellationTokenSource();

                this.loadingPanel.CancellationTokenSource = this.cancellationTokenSource;

                var token = this.cancellationTokenSource.Token;

                (long Total, DataTable Data) result = await dbInterpreter.GetPagedDataTableAsync(dbObject as Table, orderColumns, pageSize, pageNumber, token, conditionClause, isForView);

                this.Invoke(() =>
                {
                    this.pagination.TotalCount = result.Total;

                    this.dgvData.DataSource = DataGridViewHelper.ConvertDataTable(result.Data);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHelper.GetExceptionDetails(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.loadingPanel.HideLoading();
            }

            foreach (DataGridViewColumn column in this.dgvData.Columns)
            {
                if (!this.CanSort(column))
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                else
                {
                    column.SortMode = DataGridViewColumnSortMode.Programmatic;
                }
            }

            if (this.sortedColumnIndex != -1)
            {
                DataGridViewColumn column = this.dgvData.Columns[this.sortedColumnIndex];

                this.isSorting = true;

                ListSortDirection sortDirection = this.GetSortDirection(this.sortOrder);

                this.dgvData.Sort(column, sortDirection);

                this.isSorting = false;
            }
        }

        private string GetOrderColumns()
        {
            string orderColumns = "";

            if (this.dgvData.SortedColumn != null)
            {
                string sortOrder = (this.sortOrder == SortOrder.Descending ? "DESC" : "ASC");
                orderColumns = $"{this.dbInterpreter.GetQuotedString(this.dgvData.SortedColumn.Name)} {sortOrder}";
            }

            return orderColumns;
        }

        private async Task<string> GetConditionClause()
        {
            string conditionClause = "";

            if (this.conditionBuilder != null && this.conditionBuilder.Conditions.Count > 0)
            {
                this.conditionBuilder.DatabaseType = this.dbInterpreter.DatabaseType;
                this.conditionBuilder.QuotationLeftChar = this.dbInterpreter.QuotationLeftChar;
                this.conditionBuilder.QuotationRightChar = this.dbInterpreter.QuotationRightChar;

                conditionClause = "WHERE " + this.conditionBuilder.ToString();
            }
            else if (this.quickQueryConditionBuilder != null)
            {
                string content = this.uc_QuickFilter.FilterContent;

                if (!string.IsNullOrEmpty(content))
                {
                    var quickFilterInfo = new QuickFilterInfo() { Content = content, FilterMode = this.uc_QuickFilter.FilterMode };

                    string condition = await this.quickQueryConditionBuilder.Build(quickFilterInfo);

                    if (!string.IsNullOrEmpty(condition))
                    {
                        conditionClause = "WHERE " + condition;
                    }
                }
            }

            return conditionClause;
        }   

        private bool CanSort(DataGridViewColumn column)
        {
            Type valueType = column.ValueType;

            if (valueType == typeof(byte[]) || DataTypeHelper.IsGeometryType(valueType.Name))
            {
                return false;
            }

            return true;
        }

        private ListSortDirection GetSortDirection(SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Descending ? ListSortDirection.Descending : ListSortDirection.Ascending;
        }

        private void pagination_OnPageNumberChanged(long pageNumber)
        {
            this.ShowData(pageNumber);
        }

        public ContentSaveResult Save(ContentSaveInfo info)
        {
            DataExporter.WriteToCsv(this.dgvData.DataSource as DataTable, info.FilePath);

            return new ContentSaveResult() { IsOK = true };
        }

        private void dgvData_Sorted(object sender, EventArgs e)
        {
            if (this.isSorting)
            {
                return;
            }

            this.sortedColumnIndex = this.dgvData.SortedColumn.DisplayIndex;
            this.sortOrder = this.dgvData.SortOrder;

            this.ShowData(1, true);
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            if (this.OnDataFilter != null)
            {
                this.OnDataFilter(this);
            }
        }

        public void FilterData(QueryConditionBuilder conditionBuilder)
        {
            this.conditionBuilder = conditionBuilder;

            this.quickQueryConditionBuilder = null;

            this.uc_QuickFilter.ClearContent();

            this.ShowData(1, false);
        }

        private void dgvData_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            this.loadingPanel.HideLoading();
        }
        private void dgvData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewHelper.FormatCell(this.dgvData, e);
        }

        private void dgvData_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            var value = this.dgvData.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            if (value != null)
            {
                if (value.GetType() != typeof(DBNull))
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        DataGridViewCell cell = this.dgvData.Rows[e.RowIndex].Cells[e.ColumnIndex];

                        var selectedCells = this.dgvData.SelectedCells;

                        if (selectedCells == null || selectedCells.Count == 0 || !selectedCells.Contains(cell))
                        {
                            this.dgvData.CurrentCell = cell;
                        }

                        this.SetContextMenuItemVisible();

                        this.cellContextMenu.Show(Cursor.Position);
                    }
                }
            }
        }

        private void SetContextMenuItemVisible()
        {
            this.tsmiViewGeometry.Visible = DataGridViewHelper.IsGeometryValue(this.dgvData);
        }

        private void tsmiCopy_Click(object sender, EventArgs e)
        {
            var selectedCells = this.dgvData.SelectedCells;

            if (selectedCells != null && selectedCells.Count > 0)
            {
                this.dgvData.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

                DataObject content = this.dgvData.GetClipboardContent();

                Clipboard.SetDataObject(content);
            }
        }

        private void tsmiViewGeometry_Click(object sender, EventArgs e)
        {
            DataGridViewHelper.ShowGeometryViewer(this.dgvData);
        }

        private void tsmiShowContent_Click(object sender, EventArgs e)
        {
            DataGridViewHelper.ShowCellContent(this.dgvData);
        }

        private void dgvData_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvData.ClearSelection();
        }

        private void UC_QuickFilter_Query(string content, FilterMode mode)
        {
            if (this.conditionBuilder != null)
            {
                this.conditionBuilder.ClearConditions();
            }

            this.quickQueryConditionBuilder = new QuickQueryConditionBuilder(this.dbInterpreter, this.displayInfo.DatabaseObject);

            this.ShowData(1, false);
        }

        private void dgvData_SizeChanged(object sender, EventArgs e)
        {
            this.loadingPanel.RefreshStatus();
        }

        private void dgvData_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int columnIndex = e.ColumnIndex;

            DataGridViewColumn column = this.dgvData.Columns[columnIndex];

            bool canSort = this.CanSort(column);

            if (canSort)
            {
                if (this.sortedColumnIndex != columnIndex)
                {
                    this.sortedColumnIndex = columnIndex;
                    this.sortOrder = SortOrder.Ascending;
                }
                else if (this.sortedColumnIndex == columnIndex)
                {
                    if (this.sortOrder == SortOrder.Ascending)
                    {
                        this.sortOrder = SortOrder.Descending;
                    }
                    else if (this.sortOrder == SortOrder.Descending)
                    {
                        this.sortOrder = SortOrder.None;
                        this.sortedColumnIndex = -1;
                    }
                }

                foreach (DataGridViewColumn col in this.dgvData.Columns)
                {
                    if (col.Index != columnIndex)
                    {
                        col.HeaderCell.SortGlyphDirection = SortOrder.None;
                    }
                }

                column.HeaderCell.SortGlyphDirection = this.sortOrder;

                this.ShowData(1, true);
            }
        }

        private async void btnExport_Click(object sender, EventArgs e)
        {
            ExportSpecificDataOption option = new ExportSpecificDataOption();

            option.OrderColumns = this.GetOrderColumns();
            option.ConditionClause = await this.GetConditionClause();
            option.PageNumbers.Add(this.pagination.PageNumber);
            option.PageSize = this.pagination.PageSize;
            option.PageCount = this.pagination.PageCount;

            frmExportData frm = new frmExportData(this.dbInterpreter, this.displayInfo.DatabaseObject, option);

            frm.ShowDialog();           
        }       
    }
}
