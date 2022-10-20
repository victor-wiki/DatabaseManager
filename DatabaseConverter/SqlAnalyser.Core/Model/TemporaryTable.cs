using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class TemporaryTable
    {
        public TokenInfo Name { get; set; }
        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();
    }

    public class ColumnInfo
    {
        public ColumnName Name { get; set; }
        public string DataType { get; set; }
    }
}
