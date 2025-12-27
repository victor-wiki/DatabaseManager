using DatabaseInterpreter.Model;
using DatabaseManager.FileUtility.Model;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseManager.Model
{
    public class DataValidateResult
    {
        public List<TableColumn> Columns { get; set; }
        public List<DataValidateResultRow> Rows;

        public bool IsValid
        {
            get
            {
                if(this.Rows?.Any(item=> item.IsValid == false) == true)
                {
                    return false;
                }

                if(this.Rows?.Any(item=>item.Cells?.Any(t=>t.IsValid == false) == true) == true)
                {
                    return false;
                }

                return true;
            }
        }
    }

    public class DataValidateResultRow
    {
        public int RowIndex { get; set; }
        public List<DataValidateResultCell> Cells;
        public bool IsValid { get; set; } = true;     
        public List<DataValidateResultRowInvalidMessage> InvalidMessages;
    }

    public class DataValidateResultRowInvalidMessage
    {
        public string Message { get; set; }
        public List<int> ColumnIndexes { get; set; }
    }

    public class DataValidateResultCell
    {
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; }
        public string Content { get; set; }
        public bool IsValid { get; set; } = true;
        public string InvalidMessage { get; set; }
    }
}
