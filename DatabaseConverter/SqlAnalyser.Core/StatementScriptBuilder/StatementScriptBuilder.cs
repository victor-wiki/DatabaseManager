using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class StatementScriptBuilder
    {
        private StringBuilder sb = new StringBuilder();
        protected int level = 0;
        protected string indent => " ".PadLeft((this.level + 1) * 2);

        public StringBuilder Script => this.sb;

        protected void Append(string value, bool appendIndent = true)
        {
            sb.Append($"{(appendIndent ? this.indent : "")}{value}");
        }

        protected void AppendLine(string value, bool appendIndent = true)
        {
            this.Append(value, appendIndent);
            this.sb.Append(Environment.NewLine);
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
            this.level++;
        }

        private void DecreaseLevel()
        {
            this.level--;
        }

        public void TrimEnd(params char[] characters)
        {
            if (characters != null && characters.Length > 0)
            {
                while (this.sb.Length > 0 && characters.Contains(this.sb[this.sb.Length - 1]))
                {
                    this.sb.Remove(this.sb.Length - 1, 1);
                }
            }
        }

        public void TrimSeparator()
        {
            this.TrimEnd(';', '\r', '\n', ' ');
        }

        public override string ToString()
        {
            return this.sb.ToString();
        }

        protected virtual void BuildSelectStatement(SelectStatement select, bool appendSeparator = true) { }

        protected void BuildSelectStatementFromItems(SelectStatement selectStatement)
        {
            int count = selectStatement.FromItems.Count;
            int i = 0;

            bool hasJoins = false;

            foreach (FromItem fromItem in selectStatement.FromItems)
            {
                if (i == 0)
                {
                    this.Append("FROM ");
                }

                hasJoins = fromItem.JoinItems.Count > 0;

                if (i > 0 && !hasJoins)
                {
                    this.Append(",", false);
                }

                NameToken fromTableName = fromItem.TableName;

                if (fromTableName == null)
                {
                    fromTableName = selectStatement.TableName;
                }

                string fromTableAlias = fromTableName.Alias == null ? "" : " " + fromTableName.Alias.ToString();

                this.Append($"{fromTableName}{fromTableAlias}{(hasJoins ? Environment.NewLine : "")}", false);

                bool hasSubSelect = false;

                if (fromItem.SubSelectStatement != null)
                {
                    hasSubSelect = true;

                    this.AppendLine("");
                    this.AppendLine("(");
                    this.BuildSelectStatement(fromItem.SubSelectStatement, false);
                    this.Append(")");

                    if (fromItem.Alias != null)
                    {
                        this.Append($"{fromItem.Alias}", false);
                    }
                }

                if (fromItem.JoinItems.Count > 0)
                {
                    if (hasSubSelect)
                    {
                        this.AppendLine("");
                    }

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
                            string condition = joinItem.Condition == null ? "" : $" ON {joinItem.Condition}";
                            string alias = joinItem.TableName.Alias == null ? "" : $" {joinItem.TableName.Alias}";
                            this.AppendLine($"{joinItem.Type} JOIN {joinItem.TableName}{alias}{condition}");
                        }
                    }
                }

                i++;
            }

            if (!hasJoins)
            {
                this.AppendLine("", false);
            }
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
    }
}
