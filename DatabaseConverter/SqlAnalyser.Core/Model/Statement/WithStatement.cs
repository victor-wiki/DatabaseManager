using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class WithStatement : Statement
    {
        public TableName Name { get; set; }
        public List<ColumnName> Columns { get; set; } = new List<ColumnName>();
        public List<SelectStatement> SelectStatements { get; set; }
    }
}
