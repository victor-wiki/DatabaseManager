using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace SqlAnalyser.Model
{
    public class NameToken : TokenInfo
    {
        public string Schema { get; set; }

        protected TokenInfo alias;

        public virtual string Name
        {
            get
            {
                return this.Symbol;
            }
            set
            {
                this.Symbol = value;
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

        public string NameWithSchema
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Schema))
                {
                    return $"{this.Schema}.{this.Name}";
                }

                return this.Name;
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
