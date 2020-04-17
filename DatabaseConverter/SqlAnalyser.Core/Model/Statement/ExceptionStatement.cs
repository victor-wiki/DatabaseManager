using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class ExceptionStatement : Statement
    {
        public List<ExceptionItem> Items { get; set; } = new List<ExceptionItem>();
    }

    public class ExceptionItem
    {
        public TokenInfo Name { get; set; }
        public List<Statement> Statements { get; set; } = new List<Statement>();
    }
}
