using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class CaseStatement : Statement
    {
        public TokenInfo VariableName { get; set; }
        public List<IfStatementItem> Items { get; set; } = new List<IfStatementItem>();
    }
}
