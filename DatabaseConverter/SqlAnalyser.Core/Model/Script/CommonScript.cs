using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class CommonScript : DbScript
    {
        public bool HasError { get; internal set; }
        public List<TokenInfo> Functions { get; set; } = new List<TokenInfo>();
        public List<Statement> Statements { get; set; } = new List<Statement>();
    }
}
