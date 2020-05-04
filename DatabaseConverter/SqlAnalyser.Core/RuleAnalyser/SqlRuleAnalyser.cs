using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlAnalyser.Core
{
    public abstract class SqlRuleAnalyser
    {
        public abstract Lexer GetLexer(string content);

        public virtual ICharStream GetCharStreamFromString(string content)
        {
            return CharStreams.fromstring(content);
        }

        public abstract Parser GetParser(CommonTokenStream tokenStream);

        public virtual Parser GetParser(string content)
        {
            Lexer lexer = this.GetLexer(content);

            CommonTokenStream tokens = new CommonTokenStream(lexer);

            Parser parser = this.GetParser(tokens);

            return parser;
        }

        public SqlSyntaxErrorListener AddParserErrorListener(Parser parser)
        {
            SqlSyntaxErrorListener errorListener = new SqlSyntaxErrorListener();

            parser.AddErrorListener(errorListener);

            return errorListener;
        }

        public abstract TableName ParseTableName(ParserRuleContext node, bool strict = false);
        public abstract ColumnName ParseColumnName(ParserRuleContext node, bool strict = false);
        public abstract bool IsFunction(IParseTree node);
        public abstract List<TokenInfo> GetTableNameTokens(IParseTree node);
        public abstract List<TokenInfo> GetColumnNameTokens(IParseTree node);
        public abstract List<TokenInfo> GetRoutineNameTokens(IParseTree node);

        public virtual AnalyseResult Analyse<T>(string content)
            where T : DatabaseObject
        {
            AnalyseResult result = null;

            if (typeof(T) == typeof(Procedure))
            {
                result = this.AnalyseProcedure(content);
            }
            else if (typeof(T) == typeof(Function))
            {
                result = this.AnalyseFunction(content);
            }
            else if (typeof(T) == typeof(View))
            {
                result = this.AnalyseView(content);
            }
            else if (typeof(T) == typeof(TableTrigger))
            {
                result = this.AnalyseTrigger(content);
            }
            else
            {
                throw new NotSupportedException($"Not support analyse for type:{typeof(T).Name}");
            }

            return result;
        }

        public abstract AnalyseResult AnalyseProcedure(string content);

        public abstract AnalyseResult AnalyseFunction(string content);

        public abstract AnalyseResult AnalyseTrigger(string content);

        public abstract AnalyseResult AnalyseView(string content);

        public virtual void ExtractFunctions(CommonScript script, ParserRuleContext node)
        {
            if (node == null || node.children == null)
            {
                return;
            }

            foreach (var child in node.children)
            {
                if (this.IsFunction(child))
                {
                    script.Functions.Add(new TokenInfo(child as ParserRuleContext));
                }
                else if (child is ParserRuleContext)
                {
                    this.ExtractFunctions(script, child as ParserRuleContext);
                }
            }
        }

        protected TokenInfo ParseToken(ParserRuleContext node, TokenType tokenType = TokenType.General, bool strict = false)
        {
            TokenInfo tokenInfo = new TokenInfo(node) { Type = tokenType };

            List<TokenInfo> tokens = new List<TokenInfo>();

            this.AddTokens(tokenInfo.Tokens, node, tokenType, strict);

            return tokenInfo;
        }

        protected void AddTokens(List<TokenInfo> tokens, ParserRuleContext node, TokenType tokenType = TokenType.General, bool strict = false)
        {
            Func<TokenType, bool> isMatched = (type) =>
            {
                return !strict || (strict && tokenType == type);
            };

            foreach (var child in node.children)
            {
                List<TokenInfo> ts = new List<TokenInfo>();

                if (isMatched(TokenType.ColumnName))
                {
                    ts.AddRange(this.GetColumnNameTokens(child));
                }

                if (isMatched(TokenType.TableName))
                {
                    ts.AddRange(this.GetTableNameTokens(child));
                }

                if (isMatched(TokenType.RoutineName))
                {
                    ts.AddRange(this.GetRoutineNameTokens(child));
                }

                if (ts.Count > 0)
                {
                    ts.ForEach(item => this.AddToken(tokens, item));
                }
                else if (child is ParserRuleContext)
                {
                    this.AddTokens(tokens, child as ParserRuleContext, tokenType, strict);
                }
            }
        }


        protected void AddToken(List<TokenInfo> tokens, TokenInfo tokenInfo)
        {
            if (!tokens.Any(item => item != null && item.Symbol != null && item.StartIndex == tokenInfo.StartIndex && item.StopIndex == tokenInfo.StopIndex))
            {              
                tokens.Add(tokenInfo);
            }
        }
    }
}
