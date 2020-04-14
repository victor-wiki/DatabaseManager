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

        protected abstract bool IsFunction(IParseTree node);
        protected abstract bool IsTableName(IParseTree node, out ParserRuleContext parsedNode);
        protected abstract bool IsColumnName(IParseTree node, out ParserRuleContext parsedNode);


        public virtual CommonScript Analyse<T>(string content)
            where T : DatabaseObject
        {
            CommonScript script = null;

            if (typeof(T) == typeof(Procedure))
            {
                script = this.AnalyseProcedure(content);
            }
            else if (typeof(T) == typeof(Function))
            {
                script = this.AnalyseFunction(content);
            }
            else if (typeof(T) == typeof(View))
            {
                script = this.AnalyseView(content);
            }
            else if (typeof(T) == typeof(TableTrigger))
            {
                script = this.AnalyseTrigger(content);
            }
            else
            {
                throw new NotSupportedException($"Not support analyse for type:{typeof(T).Name}");
            }            

            return script;
        }

        public abstract RoutineScript AnalyseProcedure(string content);

        public abstract RoutineScript AnalyseFunction(string content);

        public abstract TriggerScript AnalyseTrigger(string content);

        public abstract ViewScript AnalyseView(string content);      

        public virtual void ExtractFunctions(CommonScript script, ParserRuleContext node)
        {
            if (node == null)
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

        protected TokenInfo ParseToken(ParserRuleContext node, TokenType tokenType = TokenType.General)
        {
            TokenInfo tokenInfo = new TokenInfo(node) { Type = tokenType };

            this.AddTokens(tokenInfo.Tokens, node, tokenType);

            return tokenInfo;
        }

        protected void AddTokens(List<TokenInfo> tokens, ParserRuleContext node, TokenType tokenType = TokenType.General)
        {
            foreach (var child in node.children)
            {
                ParserRuleContext parsedNode = null;

                if (this.IsTableName(child, out parsedNode))
                {
                    this.AddToken(tokens, new TokenInfo(parsedNode) { Type = TokenType.TableName });
                }
                else if (this.IsColumnName(child, out parsedNode))
                {
                    this.AddToken(tokens, new TokenInfo(parsedNode) { Type = TokenType.ColumnName });
                }
                else if (child is ParserRuleContext)
                {
                    this.AddTokens(tokens, child as ParserRuleContext, tokenType);
                }
            }
        }

        protected void AddToken(List<TokenInfo> tokens, TokenInfo tokenInfo)
        {
            if (!tokens.Any(item => item != null && item.StartIndex == tokenInfo.StartIndex && item.StopIndex == tokenInfo.StopIndex))
            {
                tokens.Add(tokenInfo);
            }
        }
    }
}
