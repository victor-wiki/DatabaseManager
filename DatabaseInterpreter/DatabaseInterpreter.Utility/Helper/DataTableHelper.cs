using CsvHelper;
using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;

namespace DatabaseInterpreter.Utility
{
    public class DataTableHelper
    {       
        public static DataTable GetChangedDataTable(DataTable dataTable, Dictionary<int, DataTableColumnChangeInfo> changedColumns, Dictionary<(int RowIndex, int ColumnIndex), dynamic> changedValues)
        {
            DataTable dtChanged = dataTable.Clone();

            for (int i = 0; i < dtChanged.Columns.Count; i++)
            {
                if (changedColumns.ContainsKey(i))
                {
                    if (changedColumns[i].MaxLength.HasValue)
                    {
                        dtChanged.Columns[i].MaxLength = changedColumns[i].MaxLength.Value;
                    }

                    dtChanged.Columns[i].DataType = changedColumns[i].Type;
                }
            }

            int rowIndex = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                DataRow r = dtChanged.NewRow();

                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    var value = row[i];

                    if (changedValues.ContainsKey((rowIndex, i)))
                    {
                        r[i] = changedValues[(rowIndex, i)];
                    }
                    else
                    {
                        r[i] = value;
                    }
                }

                dtChanged.Rows.Add(r);

                rowIndex++;
            }

            return dtChanged;
        }

        public static void WriteToFile(DataTable dataTable, string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                CsvWriter writer = new CsvWriter(sw, CultureInfo.CurrentCulture);

                foreach (DataColumn column in dataTable.Columns)
                {
                    writer.WriteField(column.ColumnName);
                }

                writer.NextRecord();

                foreach (DataRow row in dataTable.Rows)
                {
                    for (var i = 0; i < dataTable.Columns.Count; i++)
                    {
                        writer.WriteField(row[i]);
                    }

                    writer.NextRecord();
                }
            }
        }
    }
}
