using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Linq;

namespace SqlAnalyser.Model
{
    public class NameToken : TokenInfo
    {
        protected TokenInfo name;
        protected TokenInfo alias;

        public virtual TokenInfo Name
        {
            get
            {
                return this.name;
            }
            set
            {
                value.Type = this.Type;
                this.name = value;
            }
        }
        public TokenInfo Alias
        {
            get
            {
                return this.alias;
            }
            set
            {
                if (value != null)
                {
                    value.Type = TokenType.Alias;
                }

                this.alias = value;
            }
        }

        public NameToken(string symbol) : base(symbol)
        {
        }

        public NameToken(ParserRuleContext context) : base(context)
        {
        }

        public NameToken(string symbol, ParserRuleContext context) : base(symbol, context)
        {
        }

        public NameToken(ITerminalNode node) : base(node)
        {
        }

        public NameToken(string symbol, ITerminalNode node) : base(symbol, node)
        {
        }
    }
}
