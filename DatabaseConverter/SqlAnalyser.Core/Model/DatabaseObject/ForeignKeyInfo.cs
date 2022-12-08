using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class ForeignKeyInfo
    {
        public TableName TableName { get; set; }
        public List<ColumnName> ColumnNames { get; set; } = new List<ColumnName>();
        public TableName RefTableName { get; set; }
        public List<ColumnName> RefColumNames { get; set; } = new List<ColumnName>();
        public bool UpdateCascade { get; set; }
        public bool DeleteCascade { get; set; }
    }
}
