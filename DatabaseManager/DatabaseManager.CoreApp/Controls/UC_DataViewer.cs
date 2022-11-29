using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
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
        private QueryConditionBuilder conditionBuilder;

        public IEnumerable<DataGridViewColumn> Columns => this.dgvData.Columns.Cast<DataGridViewColumn>();
        public QueryConditionBuilder ConditionBuilder => this.conditionBuilder;
        public DataFilterHandler OnDataFilter;

        public UC_DataViewer()
        {
            InitializeComponent();

            this.pagination.PageSize = 50;

            this.dgvData.AutoGenerateColumns = true;

            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, this.dgvData, new object[] { true });
        }

        public void Show(DatabaseObjectDisplayInfo displayInfo)
        {
            this.LoadData(displayInfo);
        }

        private async void LoadData(DatabaseObjectDisplayInfo displayInfo, long pageNum = 1, bool isSort = false)
        {
            this.displayInfo = displayInfo;

            this.pagination.PageNum = pageNum;

            DatabaseObject dbObject = displayInfo.DatabaseObject;

            int pageSize = this.pagination.PageSize;

            var option = new DbInterpreterOption() { ShowTextForGeometry = true };

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(displayInfo.DatabaseType, displayInfo.ConnectionInfo, option);

            string orderColumns = "";

            if (this.dgvData.SortedColumn != null)
            {
                string sortOrder = (this.sortOrder == SortOrder.Descending ? "DESC" : "ASC");
                orderColumns = $"{dbInterpreter.GetQuotedString(this.dgvData.SortedColumn.Name)} {sortOrder}";
            }

            string conditionClause = "";

            if (this.conditionBuilder != null && this.conditionBuilder.Conditions.Count > 0)
            {
                this.conditionBuilder.DatabaseType = dbInterpreter.DatabaseType;
                this.conditionBuilder.QuotationLeftChar = dbInterpreter.QuotationLeftChar;
                this.conditionBuilder.QuotationRightChar = dbInterpreter.QuotationRightChar;

                conditionClause = "WHERE " + this.conditionBuilder.ToString();
            }

            try
            {
                bool isForView = false;

                if(dbObject is View)
                {
                    dbObject = ObjectHelper.CloneObject<Table>(dbObject);

                    isForView = true;
                }

                (long Total, DataTable Data) result = await dbInterpreter.GetPagedDataTableAsync(dbObject as Table, orderColumns, pageSize, pageNum, conditionClause, isForView);

                this.pagination.TotalCount = result.Total;

                this.dgvData.DataSource = DataGridViewHelper.ConvertDataTable(result.Data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHelper.GetExceptionDetails(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            foreach (DataGridViewColumn column in this.dgvData.Columns)
            {
                Type valueType = column.ValueType;

                if (valueType == typeof(byte[]) || DataTypeHelper.IsGeometryType(valueType.Name))
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
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

        private ListSortDirection GetSortDirection(SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Descending ? ListSortDirection.Descending : ListSortDirection.Ascending;
        }

        private void pagination_OnPageNumberChanged(long pageNum)
        {
            this.LoadData(this.displayInfo, pageNum);
        }

        public ContentSaveResult Save(ContentSaveInfo info)
        {
            DataTableHelper.WriteToFile(this.dgvData.DataSource as DataTable, info.FilePath);

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

            this.LoadData(this.displayInfo, 1, true);
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

            this.LoadData(this.displayInfo, 1, false);
        }

        private void dgvData_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

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
                        this.dgvData.CurrentCell = this.dgvData.Rows[e.RowIndex].Cells[e.ColumnIndex];

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
            var value = DataGridViewHelper.GetCurrentCellValue(this.dgvData);

            if (!string.IsNullOrEmpty(value))
            {
                Clipboard.SetDataObject(value);
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
    }
}
