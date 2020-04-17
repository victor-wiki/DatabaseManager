using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class TryCatchStatement : Statement
    {
        public List<Statement> TryStatements { get; set; } = new List<Statement>();
        public List<Statement> CatchStatements { get; set; } = new List<Statement>();
    }
}
