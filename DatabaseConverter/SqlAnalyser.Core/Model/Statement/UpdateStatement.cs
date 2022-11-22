using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class UpdateStatement : Statement
    {
        public List<TableName> TableNames { get; set; } = new List<TableName>();
        public List<NameValueItem> SetItems { get; set; } = new List<NameValueItem>();
        public List<FromItem> FromItems { get; set; }
        public TokenInfo Condition { get; set; }
        public TokenInfo Option { get; set; }
        public bool HasFromItems => this.FromItems != null && this.FromItems.Count > 0;
    }

    public class NameValueItem
    {
        public TokenInfo Name { get; set; }
        public TokenInfo Value { get; set; }
        public SelectStatement ValueStatement { get; set; }
    }
}
