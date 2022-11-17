using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class CallStatement : Statement
    {
        public bool IsExecuteSql { get; set; }
        public TokenInfo Name { get; set; }
        public List<CallParameter> Parameters { get; set; } = new List<CallParameter>();       
    }
}
