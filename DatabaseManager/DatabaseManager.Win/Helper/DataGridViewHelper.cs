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

                    if(value!=null)
                    {
                        if(value.GetType()== typeof(byte[]))
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
    }
}
