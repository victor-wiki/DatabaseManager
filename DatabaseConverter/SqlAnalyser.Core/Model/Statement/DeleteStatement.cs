using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class DeleteStatement : Statement
    {
        public TableName TableName { get; set; }
        public List<FromItem> FromItems { get; set; }
        public TokenInfo Condition { get; set; }
    }
}
