using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace SqlAnalyser.Model
{
    public class NameToken : TokenInfo
    {
        private TokenInfo _name;       

        public TokenInfo Name
        {
            get
            {
                if (this._name == null && this != null)
                {
                    return this;
                }

                return this._name;
            }
            set
            {
                this._name = value;
            }
        }
        public TokenInfo Alias { get; set; }

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
