using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class TokenInfo
    {
        public TokenType Type { get; set; }
        public string Symbol { get; set; }
        public int? StartIndex { get; set; }
        public int? StopIndex { get; set; }       

        public int Length => this.StartIndex.HasValue && this.StopIndex.HasValue ? (this.StopIndex - this.StartIndex + 1).Value : 0;

        public List<TokenInfo> Tokens { get; set; } = new List<TokenInfo>();

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

        public TokenInfo(string symbol, ITerminalNode node)
        {
            this.Symbol = symbol;
            this.SetIndex(node);
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
        RoutineName = 5,
        ColumnName = 6,
        Condition = 7,
        OrderBy = 8,
        GroupBy =9,
        Option = 10,       
        JoinOn = 11,
        CursorName = 12
    }
}
