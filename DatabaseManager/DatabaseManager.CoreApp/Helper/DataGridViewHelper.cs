using DatabaseInterpreter.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Helper
{
    public class DataGridViewHelper
    {
        public static DataGridViewRow GetSelectedRow(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count > 0)
            {
                return dgv.SelectedRows.OfType<DataGridViewRow>().First();
            }

            return null;
        }

        public static DataGridViewRow GetCurrentRow(DataGridView dgv)
        {
            return dgv.CurrentRow;
        }
        public static DataTable ConvertDataTable(DataTable dataTable)
        {
            DataTable dt = dataTable.Clone();

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (dataTable.Columns[i].DataType == typeof(byte[]))
                {
                    dt.Columns[i].DataType = typeof(string);
                }
            }

            foreach (DataRow row in dataTable.Rows)
            {
                DataRow r = dt.NewRow();

                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    var value = row[i];

                    if (value != null)
                    {
                        if (value.GetType() == typeof(byte[]))
                        {
                            value = ValueHelper.BytesToHexString(value as byte[]);
                        }
                    }

                    r[i] = value;
                }

                dt.Rows.Add(r);
            }

            return dt;
        }

        public static void AutoSizeLastColumn(DataGridView dgv)
        {
            if (dgv.Columns.OfType<DataGridViewColumn>().Where(item => item.Visible).Count() <= 1)
            {
                return;
            }

            DataGridViewColumn column = dgv.Columns.OfType<DataGridViewColumn>().LastOrDefault(item => item.Visible);

            int gridWidth = dgv.Width;
            int totalWidth = 0;
            int rowHeadersWidth = (dgv.RowHeadersVisible ? dgv.RowHeadersWidth : 0);
            int width = 0;

            totalWidth += rowHeadersWidth;

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Visible)
                {
                    totalWidth += col.Width;

                    if (col.Name != column.Name)
                    {
                        width += col.Width;
                    }
                }
            }

            if (totalWidth < gridWidth)
            {
                column.Width = gridWidth - width - dgv.RowHeadersWidth;
            }

            var vScrollBar = dgv.Controls.OfType<VScrollBar>().FirstOrDefault();
            int scrollBarWidth = 0;

            if (vScrollBar != null && vScrollBar.Visible)
            {
                scrollBarWidth = vScrollBar.Width;
            }

            if (scrollBarWidth > 0)
            {
                column.Width -= scrollBarWidth;
            }
        }

        public static void SetRowColumnsReadOnly(DataGridView dgv, DataGridViewRow row, bool readony, params DataGridViewColumn[] excludeColumns)
        {
            foreach (DataGridViewColumn column in dgv.Columns)
            {
                if (excludeColumns != null && excludeColumns.Contains(column))
                {
                    continue;
                }

                row.Cells[column.Name].ReadOnly = readony;
            }
        }

        public static string GetCellStringValue(DataGridViewRow row, string columnName)
        {
            if (row == null)
            {
                return null;
            }

            return GetCellStringValue(row.Cells[columnName]);
        }

        public static string GetCellStringValue(DataGridViewCell cell)
        {
            return cell.Value?.ToString()?.Trim();
        }

        public static bool GetCellBoolValue(DataGridViewRow row, string columnName)
        {
            return GetCellBoolValue(row.Cells[columnName]);
        }

        public static bool GetCellBoolValue(DataGridViewCell cell)
        {
            return IsTrueValue(cell.Value);
        }

        public static bool IsTrueValue(object value)
        {
            return value?.ToString() == "True";
        }

        public static bool IsEmptyRow(DataGridViewRow row)
        {
            if(row == null)
            {
                return true;
            }

            int visibleCount = 0;
            int emptyCount = 0;

            foreach(DataGridViewCell cell in row.Cells)
            {
                if(cell.Visible)
                {
                    visibleCount++;

                    if(string.IsNullOrEmpty(cell.Value?.ToString()))
                    {
                        emptyCount++;
                    }
                }
            }

            return visibleCount == emptyCount;
        }

        public static void SetRowCellsReadOnly(DataGridViewRow row, bool readOnly)
        {
            if (row == null)
            {
                return;
            }

            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.ReadOnly = readOnly;
            }
        }
    }
}
