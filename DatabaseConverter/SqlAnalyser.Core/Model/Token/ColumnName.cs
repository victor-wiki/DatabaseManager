using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlAnalyser.Model
{
    public class ColumnName: NameToken
    {
        public new TokenType Type => TokenType.ColumnName;       

        public TokenInfo DataType { get; set; }

        public ColumnName(string symbol):base(symbol)
        {            
        }

        public ColumnName(ParserRuleContext context):base(context)
        {            
        }

        public ColumnName(string symbol, ParserRuleContext context):base(symbol, context)
        {           
        }

        public ColumnName(ITerminalNode node):base(node)
        {            
        }

        public ColumnName(string symbol, ITerminalNode node):base(symbol, node)
        {
        }
    }
}
