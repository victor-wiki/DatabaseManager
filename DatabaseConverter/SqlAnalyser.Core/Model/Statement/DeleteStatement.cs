namespace SqlAnalyser.Model
{
    public class DeleteStatement : Statement
    {
        public TableName TableName { get; set; }
        public TokenInfo Condition { get; set; }
    }
}
