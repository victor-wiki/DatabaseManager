using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class SelectStatement : Statement
    {
        public List<ColumnName> Columns { get; set; } = new List<ColumnName>();
        public TokenInfo IntoTableName { get; set; }
        public TableName TableName { get; set; }
        public TokenInfo Where { get; set; }
        public List<TokenInfo> GroupBy { get; set; }
        public TokenInfo Having { get; set; }
        public List<UnionStatement> UnionStatements { get; set; }
        public List<WithStatement> WithStatements { get; set; }
        public List<FromItem> FromItems { get; set; }
        public List<TokenInfo> OrderBy { get; set; }
        public TokenInfo Option { get; set; }
        public SelectTopInfo TopInfo { get; set; }
        public SelectLimitInfo LimitInfo { get; set; }
    }

    public class SelectTopInfo
    {
        public int TopCount { get; set; }
        public bool IsPercent { get; set; }
    }

    public class SelectLimitInfo
    {
        public long StartRowIndex { get; set; }
        public long RowCount { get; set; }
    }

    public class FromItem
    {
        public TableName TableName { get; set; }
        public List<JoinItem> JoinItems { get; set; } = new List<JoinItem>();
    }

    public class JoinItem
    {
        public JoinType Type { get; set; }
        public TableName TableName { get; set; }
        public TokenInfo Condition { get; set; }
    }

    public enum JoinType
    {
        INNER = 0,
        LEFT = 1,
        RIGHT = 2,
        FULL = 3,
        CROSS = 4
    }
}
