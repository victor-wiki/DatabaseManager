using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class CallStatement : Statement
    {
        public TokenInfo Name { get; set; }
        public List<TokenInfo> Arguments { get; set; } = new List<TokenInfo>();
    }
}
