using System.Collections.Generic;
using System.Data;

namespace DatabaseInterpreter.Model
{
    public class TableDataReadInfo
    {
        public Table Table { get; set; }
        public List<TableColumn> Columns { get; set; }
        public long TotalCount { get; set; }
        public List<Dictionary<string, object>> Data { get; set; }
        public DataTable DataTable { get; set; }
    }
}
