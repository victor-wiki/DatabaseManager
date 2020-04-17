using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class TemporaryTable
    {
        public TokenInfo Name { get; set; }
        public List<ColumnName> Columns { get; set; } = new List<ColumnName>();
    }
}
