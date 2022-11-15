using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class CallStatement : Statement
    {
        public bool IsExecuteSql { get; set; }
        public TokenInfo Sql { get; set; }
        public TokenInfo Name { get; set; }
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public List<Parameter> ParameterDetails{ get; set; } = new List<Parameter>();
    }
}
