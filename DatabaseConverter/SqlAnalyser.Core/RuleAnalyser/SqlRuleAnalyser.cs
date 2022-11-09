using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using static PlSqlParser;
using static TSqlParser;

namespace SqlAnalyser.Core
{
    public abstract class SqlRuleAnalyser
    {
        public SqlRuleAnalyserOption Option { get; set; } = new SqlRuleAnalyserOption();
        public abstract IEnumerable<Type> ParseTableTypes { get; }
        public abstract IEnumerable<Type> ParseColumnTypes { get; }

        public abstract Lexer GetLexer(string content);

        public virtual ICharStream GetCharStreamFromString(string content)
        {
            return CharStreams.fromString(content);
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
        public virtual TokenInfo ParseFunction(ParserRuleContext node)
        {
            TokenInfo token = new TokenInfo(node as ParserRuleContext);
            return token;
        }

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
            if (!this.Option.ExtractFunctions)
            {
                return;
            }

            if (node == null || node.children == null)
            {
                return;
            }

            foreach (var child in node.children)
            {
                bool isFunction = false;

                if (this.IsFunction(child))
                {
                    isFunction = true;

                    var childNode = child as ParserRuleContext;

                    if (!script.Functions.Any(item=>item.StartIndex.Value == childNode.Start.StartIndex && item.StopIndex.Value == childNode.Stop.StopIndex))
                    {
                        script.Functions.Add(this.ParseFunction(childNode));
                    }                    
                }

                if (isFunction && !this.Option.ExtractFunctionChildren)
                {
                    continue;
                }
                else
                {
                    if (child is ParserRuleContext)
                    {
                        this.ExtractFunctions(script, child as ParserRuleContext);
                    }
                }
            }
        }        

        protected TokenInfo CreateToken(ParserRuleContext node, TokenType tokenType = TokenType.General)
        {
            TokenInfo tokenInfo = new TokenInfo(node) { Type = tokenType };

            return tokenInfo;
        }

        protected bool HasAsFlag(ParserRuleContext node)
        {
            return node.children.Count > 0 && node.GetChild(0).GetText() == "AS";
        }

        protected List<ParserRuleContext> FindSpecificContexts(ParserRuleContext node, IEnumerable<Type> searchTypes)
        {
            List<ParserRuleContext> fullNames = new List<ParserRuleContext>();

            if (node != null && node.children != null)
            {
                foreach (var child in node.children)
                {
                    if (searchTypes.Any(t => t == child.GetType()))
                    {
                        var c = child as ParserRuleContext;

                        fullNames.Add(c);
                    }
                    else if (!(child is TerminalNodeImpl))
                    {
                        fullNames.AddRange(this.FindSpecificContexts(child as ParserRuleContext, searchTypes));
                    }
                }
            }

            return fullNames;
        }

        protected List<TokenInfo> ParseTableAndColumnNames(ParserRuleContext node, IEnumerable<Type> tableTypes, IEnumerable<Type> columnTypes)
        {
            List<TokenInfo> tokens = new List<TokenInfo>();

            var fullTableNames = this.FindSpecificContexts(node, tableTypes);
            var fullColumnNames = this.FindSpecificContexts(node, columnTypes);

            foreach (var columnName in fullColumnNames)
            {
                tokens.Add(this.ParseColumnName(columnName));
            }

            foreach (var tableName in fullTableNames)
            {
                tokens.Add(this.ParseTableName(tableName));
            }

            return tokens;
        }

        protected void AddChildTableAndColumnNameToken(ParserRuleContext node, TokenInfo token, IEnumerable<Type> tableTypes, IEnumerable<Type> columnTypes)
        {
            if (this.Option.ParseTokenChildren)
            {
                this.ParseTableAndColumnNames(node, tableTypes, columnTypes).ForEach(item => token.AddChild(item));
            }
        }

        protected void AddChildColumnNameToken(ParserRuleContext node, TokenInfo token, IEnumerable<Type> searchTypes)
        {
            if (this.Option.ParseTokenChildren)
            {
                this.FindSpecificContexts(node, searchTypes).ForEach(item => token.AddChild(this.ParseColumnName(item)));
            }
        }

        protected void AddChildTableAndColumnNameToken(ParserRuleContext node, TokenInfo token)
        {
            if (this.Option.ParseTokenChildren)
            {
                this.AddChildTableAndColumnNameToken(node, token, this.ParseTableTypes, this.ParseColumnTypes);
            }
        }

        protected void AddChildColumnNameToken(ParserRuleContext node, TokenInfo token)
        {
            if (this.Option.ParseTokenChildren)
            {
                this.AddChildColumnNameToken(node, token, this.ParseColumnTypes);
            }
        }
    }
}
