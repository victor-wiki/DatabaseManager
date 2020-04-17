using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class FetchCursorStatement : Statement
    {
        public TokenInfo CursorName { get; set; }
        public List<TokenInfo> Variables { get; set; } = new List<TokenInfo>();
    }
}
