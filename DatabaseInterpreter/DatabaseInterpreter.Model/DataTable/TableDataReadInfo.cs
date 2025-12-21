using System.Collections.Generic;
using System.Data;

namespace DatabaseInterpreter.Model
{
    public class TableDataReadInfo
    {
        public Table Table { get; set; }
        public DataTable DataTable { get; set; }
        public List<TableColumn> Columns { get; set; }
        public long TotalCount { get; set; }     
    }
}
