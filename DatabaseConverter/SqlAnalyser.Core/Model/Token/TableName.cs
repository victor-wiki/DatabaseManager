using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace SqlAnalyser.Model
{
    public class TableName : NameToken
    {
        public override TokenType Type => TokenType.TableName;       

        public TableName(string symbol) : base(symbol)
        {
        }

        public TableName(ParserRuleContext context) : base(context)
        {
        }

        public TableName(string symbol, ParserRuleContext context) : base(symbol, context)
        {
        }

        public TableName(ITerminalNode node) : base(node)
        {
        }

        public TableName(string symbol, ITerminalNode node) : base(symbol, node)
        {
        }
    }
}
