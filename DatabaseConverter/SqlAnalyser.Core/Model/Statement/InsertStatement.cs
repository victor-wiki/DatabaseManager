using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class InsertStatement : Statement
    {
        public TableName TableName { get; set; }
        public List<ColumnName> Columns { get; set; } = new List<ColumnName>();
        public List<TokenInfo> Values { get; set; } = new List<TokenInfo>();
        public List<SelectStatement> SelectStatements { get; set; }
    }
}
