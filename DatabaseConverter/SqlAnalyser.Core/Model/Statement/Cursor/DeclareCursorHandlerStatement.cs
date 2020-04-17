using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class DeclareCursorHandlerStatement : Statement
    {
        public List<TokenInfo> Conditions { get; set; } = new List<TokenInfo>();
        public List<Statement> Statements = new List<Statement>();
    }
}
