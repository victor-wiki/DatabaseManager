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
        public abstract IEnumerable<Type> ParseTableAliasTypes { get; }
        public abstract IEnumerable<Type> ParseColumnAliasTypes { get; }

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

        public abstract SqlSyntaxError Validate(string content);
        public abstract TableName ParseTableName(ParserRuleContext node, bool strict = false);
        public abstract ColumnName ParseColumnName(ParserRuleContext node, bool strict = false);
        public abstract TokenInfo ParseTableAlias(ParserRuleContext node);
        public abstract TokenInfo ParseColumnAlias(ParserRuleContext node);
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

        public abstract AnalyseResult AnalyseCommon(string content);

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

                    if (!script.Functions.Any(item => item.StartIndex.Value == childNode.Start.StartIndex && item.StopIndex.Value == childNode.Stop.StopIndex))
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

        protected List<TokenInfo> ParseTableAndColumnNames(ParserRuleContext node, bool isOnlyForColumn = false)
        {
            List<TokenInfo> tokens = new List<TokenInfo>();
            List<TableName> tableNameTokens = new List<TableName>();
            List<ColumnName> columnNameTokens = new List<ColumnName>();
            List<TokenInfo> aliasTokens = new List<TokenInfo>();

            var types = isOnlyForColumn ? this.ParseColumnTypes.Union(this.ParseColumnAliasTypes)
                        : this.ParseTableTypes.Union(this.ParseColumnTypes).Union(this.ParseTableAliasTypes).Union(this.ParseColumnAliasTypes);

            var results = this.FindSpecificContexts(node, types);

            var tableNames = !isOnlyForColumn ? results.Where(item => this.ParseTableTypes.Any(t => item.GetType() == t)) : Enumerable.Empty<ParserRuleContext>();
            var columnNames = results.Where(item => this.ParseColumnTypes.Any(t => item.GetType() == t));
            var tableAliases = !isOnlyForColumn ? results.Where(item => this.ParseTableAliasTypes.Any(t => item.GetType() == t)) : Enumerable.Empty<ParserRuleContext>();
            var columnAliases = results.Where(item => this.ParseColumnAliasTypes.Any(t => item.GetType() == t));

            foreach (var columnName in columnNames)
            {
                columnNameTokens.Add(this.ParseColumnName(columnName));
            }

            if (!isOnlyForColumn)
            {
                foreach (var tableName in tableNames)
                {
                    tableNameTokens.Add(this.ParseTableName(tableName));
                }
            }

            foreach (var columnAlias in columnAliases)
            {
                TokenInfo alias = this.ParseColumnAlias(columnAlias);

                if (!this.IsAliasExisted(columnNameTokens, alias))
                {
                    aliasTokens.Add(alias);                    
                }
            }

            if (!isOnlyForColumn)
            {
                foreach (var tableAlias in tableAliases)
                {
                    TokenInfo alias = this.ParseTableAlias(tableAlias);

                    if (!this.IsAliasExisted(tableNameTokens, alias))
                    {
                        aliasTokens.Add(alias);
                    }
                }
            }

            tokens.AddRange(columnNameTokens);

            if (!isOnlyForColumn)
            {
                tokens.AddRange(tableNameTokens);
            }

            tokens.AddRange(aliasTokens);

            return tokens;
        }

        private bool IsAliasExisted(IEnumerable<NameToken> tokens, TokenInfo alias)
        {
            if (tokens.Any(item => item.Alias?.Symbol == alias?.Symbol && item.Alias?.StartIndex == alias?.StartIndex))
            {
                return true;
            }

            return false;
        }

        protected void AddChildTableAndColumnNameToken(ParserRuleContext node, TokenInfo token)
        {
            if (this.Option.ParseTokenChildren)
            {
                if (token != null)
                {
                    this.ParseTableAndColumnNames(node).ForEach(item => token.AddChild(item));
                }
            }
        }

        protected void AddChildColumnNameToken(ParserRuleContext node, TokenInfo token, IEnumerable<Type> searchTypes)
        {
            if (this.Option.ParseTokenChildren)
            {
                if (token != null)
                {
                    this.ParseTableAndColumnNames(node, true).ForEach(item => token.AddChild(item));
                }
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
