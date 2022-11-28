using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.IO;

namespace SqlAnalyser.Model
{
    public class NameToken : TokenInfo
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string Schema { get; set; }

        public bool HasAs { get; set; }

        protected TokenInfo alias;      

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
                    Type type = this.GetType();
                    
                    if(type == typeof(TableName))
                    {
                        value.Type = TokenType.TableAlias;
                    }
                    else if(type == typeof(ColumnName))
                    {
                        value.Type = TokenType.ColumnAlias;
                    }                   
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
                    return $"{this.Schema}.{this.Symbol}";
                }

                return this.Symbol;
            }
        }

        public string NameWithAlias
        {
            get
            {
                if(this.alias == null)
                {
                    return this.Symbol;
                }

                string strAs = this.HasAs ? " AS " : " ";

                return $"{this.Symbol}{strAs}{this.alias}";
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
