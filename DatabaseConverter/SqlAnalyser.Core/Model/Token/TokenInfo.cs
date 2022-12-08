using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class TokenInfo
    {
        public virtual TokenType Type { get; set; }
        public string Symbol { get; set; }
        public int? StartIndex { get; set; }
        public int? StopIndex { get; set; }
        public bool IsConst { get; set; }

        public int Length => this.StartIndex.HasValue && this.StopIndex.HasValue ? (this.StopIndex - this.StartIndex + 1).Value : 0;

        public TokenInfo Parent { get; private set; }
        public List<TokenInfo> Children { get; } = new List<TokenInfo>();

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

        public void AddChild(TokenInfo child)
        {
            if (child == null)
            {
                return;
            }

            if (!(child.StartIndex == this.StartIndex && child.StopIndex == this.StopIndex))
            {
                child.Parent = this;
                this.Children.Add(child);
            }
        }
    }

    public enum TokenType
    {
        General = 0,
        TableName,
        ViewName,
        TypeName,
        SequenceName,
        TriggerName,
        FunctionName,
        ProcedureName,
        RoutineName,
        ColumnName,
        ParameterName,
        VariableName,
        UserVariableName, //for mysql
        CursorName,
        ConstraintName,
        DataType,   
        TableAlias,
        ColumnAlias,
        IfCondition, //not include query
        SearchCondition,
        TriggerCondition,
        ExitCondition,
        OrderBy,
        GroupBy,
        Option,
        JoinOn,
        Pivot,
        UnPivot,
        InsertValue,
        UpdateSetValue,
        Subquery,
        FunctionCall,
        StringLiteral
    }   
}
