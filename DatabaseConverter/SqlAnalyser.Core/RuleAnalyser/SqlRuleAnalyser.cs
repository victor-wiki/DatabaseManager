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

        public abstract TableName ParseTableName(ParserRuleContext node);
        public abstract ColumnName ParseColumnName(ParserRuleContext node);
        public abstract bool IsFunction(IParseTree node);
        public abstract bool IsTableName(IParseTree node, out ParserRuleContext parsedNode);
        public abstract bool IsColumnName(IParseTree node, out ParserRuleContext parsedNode);
        public abstract bool IsRoutineName(IParseTree node, out ParserRuleContext parsedNode);


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
                ParserRuleContext parsedNode = null;

                if (isMatched(TokenType.TableName) && this.IsTableName(child, out parsedNode))
                {
                    this.AddToken(tokens, new TokenInfo(parsedNode) { Type = TokenType.TableName });
                }
                else if (isMatched(TokenType.ColumnName) && this.IsColumnName(child, out parsedNode))
                {
                    this.AddToken(tokens, new TokenInfo(parsedNode) { Type = TokenType.ColumnName });
                }
                else if (isMatched(TokenType.RoutineName) && this.IsRoutineName(child, out parsedNode))
                {
                    this.AddToken(tokens, new TokenInfo(parsedNode) { Type = TokenType.RoutineName });
                }
                else if (child is ParserRuleContext)
                {
                    this.AddTokens(tokens, child as ParserRuleContext, tokenType);
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
