using DatabaseInterpreter.Utility;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlAnalyser.Core
{
    public class StatementScriptBuilder
    {
        private StringBuilder builder = new StringBuilder();
        internal int Level = 0;
        internal string Indent => " ".PadLeft((this.Level + 1) * 2);       

        public StringBuilder Script => this.builder;

        public RoutineType RoutineType { get; set; }

        public List<Statement> DeclareStatements { get; internal set; } = new List<Statement>();
        public List<Statement> SpecialStatements { get; internal set; } = new List<Statement>();
        public Dictionary<string, string> Replacements { get; internal set; } = new Dictionary<string, string>();
        public List<string> TemporaryTableNames { get; set; } = new List<string>();

        public StatementScriptBuilderOption Option { get; set; } = new StatementScriptBuilderOption();

        internal int Length
        {
            get
            {
                return this.builder.Length;
            }
        }        

        protected void Append(string value, bool appendIndent = true)
        {
            builder.Append($"{(appendIndent ? this.Indent : "")}{value}");
        }

        protected void AppendLine(string value = "", bool appendIndent = true)
        {
            this.Append(value, appendIndent);
            this.builder.Append(Environment.NewLine);
        }

        protected void AppendChildStatements(IEnumerable<Statement> statements, bool needSeparator = true)
        {
            int childCount = statements.Count();

            if (childCount > 0)
            {
                this.IncreaseLevel();
            }

            foreach (Statement statement in statements)
            {
                this.Build(statement, needSeparator);
            }

            if (childCount > 0)
            {
                this.DecreaseLevel();
            }
        }

        public virtual StatementScriptBuilder Build(Statement statement, bool appendSeparator = true)
        {
            return this;
        }

        private void IncreaseLevel()
        {
            this.Level++;
        }

        private void DecreaseLevel()
        {
            this.Level--;
        }

        public void TrimEnd(params char[] characters)
        {
            if (characters != null && characters.Length > 0)
            {
                while (this.builder.Length > 0 && characters.Contains(this.builder[this.builder.Length - 1]))
                {
                    this.builder.Remove(this.builder.Length - 1, 1);
                }
            }
        }

        public void TrimSeparator()
        {
            this.TrimEnd(';', '\r', '\n', ' ');
        }

        public override string ToString()
        {
            return this.builder.ToString();
        }

        public void Clear()
        {
            this.builder.Clear();
        }

        protected virtual void BuildSelectStatement(SelectStatement select, bool appendSeparator = true) { }

        protected void BuildSelectStatementFromItems(SelectStatement selectStatement)
        {
            this.BuildFromItems(selectStatement.FromItems, selectStatement);
        }

        protected void BuildFromItems(List<FromItem> fromItems, SelectStatement selectStatement = null, bool isDeleteFromItem = false)
        {
            int count = fromItems.Count;
            int i = 0;

            bool hasJoins = false;

            foreach (FromItem fromItem in fromItems)
            {
                if (i == 0)
                {
                    this.Append("FROM ");
                }

                hasJoins = fromItem.HasJoinItems;

                if (i > 0 && !hasJoins)
                {
                    this.Append(",", false);
                }

                NameToken fromTableName = fromItem.TableName;
                TokenInfo alias = fromItem.Alias;

                if (fromTableName == null && selectStatement?.TableName != null)
                {
                    fromTableName = selectStatement.TableName;
                }

                string nameWithAlias = this.GetNameWithAlias(fromTableName);

                if (nameWithAlias?.Trim() != alias?.Symbol?.Trim())
                {
                    this.Append($"{nameWithAlias}{(hasJoins ? Environment.NewLine : "")}", false);
                }

                bool hasSubSelect = false;

                if (fromItem.SubSelectStatement != null)
                {
                    hasSubSelect = true;
                    
                    this.AppendLine("(");
                    this.BuildSelectStatement(fromItem.SubSelectStatement, false);
                    this.Append(")");

                    if (alias != null)
                    {
                        this.Append($"{alias}", false);
                    }
                }

                if (fromItem.JoinItems.Count > 0)
                {
                    if (hasSubSelect)
                    {
                        this.AppendLine("");
                    }

                    int j = 0;

                    foreach (JoinItem joinItem in fromItem.JoinItems)
                    {
                        if (joinItem.Type == JoinType.PIVOT || joinItem.Type == JoinType.UNPIVOT)
                        {
                            if (joinItem.PivotItem != null)
                            {
                                this.BuildPivotItem(joinItem.PivotItem);
                            }
                            else if (joinItem.UnPivotItem != null)
                            {
                                this.BuildUnPivotItem(joinItem.UnPivotItem);
                            }

                            if (joinItem.Alias != null)
                            {
                                this.AppendLine(joinItem.Alias.Symbol);
                            }
                            else
                            {
                                this.AppendLine(joinItem.Type.ToString() + "_");
                            }
                        }
                        else
                        {
                            bool isPostgresDeleteFromJoin = isDeleteFromItem && this is PostgreSqlStatementScriptBuilder;

                            string condition = joinItem.Condition == null ? "" : $" {(isPostgresDeleteFromJoin && j == 0 ? "WHERE" : "ON")} {joinItem.Condition}";

                            string joinKeyword = (isPostgresDeleteFromJoin && j == 0) ? "USING " : $"{joinItem.Type} JOIN ";

                            TableName tableName = joinItem.TableName;

                            string joinTableName = null;

                            bool isSubquery = AnalyserHelper.IsSubquery(tableName.Symbol);

                            if (!isSubquery)
                            {
                                joinTableName = this.GetNameWithAlias(tableName);
                            }
                            else
                            {
                                joinTableName = $"({tableName}){(tableName.Alias == null ? "" : $" {tableName.Alias}")}";
                            }

                            this.AppendLine($"{joinKeyword}{joinTableName}{condition}");
                        }

                        j++;
                    }
                }

                i++;
            }

            if (!hasJoins)
            {
                this.AppendLine("", false);
            }
        }

        protected virtual string GetNameWithAlias(NameToken name)
        {
            return name?.NameWithAlias;
        }

        public string GetTrimedQuotationValue(string value)
        {
            return value?.Trim('[', ']', '"', '`');
        }

        protected virtual string GetPivotInItem(TokenInfo token)
        {
            return this.GetTrimedQuotationValue(token.Symbol);
        }

        public void BuildPivotItem(PivotItem pivotItem)
        {
            this.AppendLine("PIVOT");
            this.AppendLine("(");
            this.AppendLine($"{pivotItem.AggregationFunctionName}({pivotItem.AggregatedColumnName})");
            this.AppendLine($"FOR {pivotItem.ColumnName} IN ({(string.Join(",", pivotItem.Values.Select(item => this.GetPivotInItem(item))))})");
            this.AppendLine(")");
        }

        public void BuildUnPivotItem(UnPivotItem unpivotItem)
        {
            this.AppendLine("UNPIVOT");
            this.AppendLine("(");
            this.AppendLine($"{unpivotItem.ValueColumnName}");
            this.AppendLine($"FOR {unpivotItem.ForColumnName} IN ({(string.Join(",", unpivotItem.InColumnNames.Select(item => $"{item}")))})");
            this.AppendLine(")");
        }

        protected bool IsIdentifierNameBeforeEqualMark(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                string name = value.Split('=')[0].Trim();

                string pattern = RegexHelper.NameRegexPattern;

                return Regex.IsMatch(name, pattern);
            }

            return false;
        }

        protected bool HasAssignVariableColumn(SelectStatement statement)
        {
            var columns = statement.Columns;

            if (columns.Any(item => item.Symbol.Contains("=") && this.IsIdentifierNameBeforeEqualMark(item.Symbol)))
            {
                return true;
            }

            return false;
        }
    }
}
