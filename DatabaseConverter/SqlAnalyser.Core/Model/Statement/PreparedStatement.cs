using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class PreparedStatement : Statement
    {
        public TokenInfo Id { get;set; }
        public PreparedStatementType Type { get; set; }
        public TokenInfo FromSqlOrVariable { get; set; }     
        public List<TokenInfo> ExecuteVariables { get; set; } = new List<TokenInfo>();
    }

    public enum PreparedStatementType
    {
        Prepare,
        Execute,
        Deallocate
    }
}
