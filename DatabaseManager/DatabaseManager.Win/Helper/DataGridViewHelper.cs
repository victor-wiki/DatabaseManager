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

        public static void AutoSizeColumn(DataGridView dgv, DataGridViewColumn column)
        {
            if (dgv.ColumnCount <= 1)
            {
                return;
            }

            int width = 0;

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Name != column.Name)
                {
                    width += col.Width;
                }
            }

            column.Width = dgv.Width - width - (dgv.Columns.Count - 1);
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
    }
}
