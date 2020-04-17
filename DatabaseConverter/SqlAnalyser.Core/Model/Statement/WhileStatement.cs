using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class WhileStatement : Statement
    {
        public TokenInfo Condition { get; set; }

        public List<Statement> Statements { get; set; } = new List<Statement>();
    }
}
