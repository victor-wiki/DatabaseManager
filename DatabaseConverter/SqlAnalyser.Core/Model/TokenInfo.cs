using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace SqlAnalyser.Model
{
    public class TokenInfo
    {
        public TokenType Type { get; set; }
        public string Symbol { get; set; }
        public int? StartIndex { get; set; }
        public int? StopIndex { get; set; }

        public object Tag { get; set; }

        public int Length => this.StartIndex.HasValue && this.StopIndex.HasValue ? (this.StopIndex - this.StartIndex + 1).Value : 0;

        public TokenInfo(string symbol)
        {
            this.Symbol = symbol;
        }

        public TokenInfo(ParserRuleContext context)
        {
            this.Symbol = context?.GetText();
            this.SetIndex(context);
        }

        public TokenInfo(string symbol, ParserRuleContext context)
        {
            this.Symbol = symbol;
            this.SetIndex(context);
        }

        public TokenInfo(ITerminalNode node)
        {
            this.Symbol = node?.GetText();
            this.SetIndex(node);
        }

        public TokenInfo(string symbol, ITerminalNode context)
        {
            this.Symbol = symbol;
            this.SetIndex(context);
        }

        public TokenInfo SetIndex(ParserRuleContext context)
        {
            this.StartIndex = context?.Start?.StartIndex;
            this.StopIndex = context?.Stop?.StopIndex;

            return this;
        }

        public TokenInfo SetIndex(ITerminalNode node)
        {
            this.StartIndex = node?.Symbol?.StartIndex;
            this.StopIndex = node?.Symbol?.StopIndex;

            return this;
        }

        public override string ToString()
        {
            return this.Symbol;
        }
    }

    public enum TokenType
    {
        General = 0,
        ParameterName = 1,
        DataType = 2,
        VariableName = 3,
        TableName = 4,
        ColumnName = 5,
        Condition = 6
    }
}
