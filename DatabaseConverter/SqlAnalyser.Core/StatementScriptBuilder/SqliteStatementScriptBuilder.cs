using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class SqliteStatementScriptBuilder : StatementScriptBuilder
    {
        public override StatementScriptBuilder Build(Statement statement, bool appendSeparator = true)
        {
            base.Build(statement, appendSeparator);

            if (statement is SelectStatement select)
            {
                this.BuildSelectStatement(select, appendSeparator);
            }
            else if (statement is InsertStatement insert)
            {
                this.Append($"INSERT INTO {insert.TableName}", false);

                if (insert.Columns.Count > 0)
                {
                    this.AppendLine($"({string.Join(",", insert.Columns.Select(item => item))})");
                }

                if (insert.SelectStatements != null && insert.SelectStatements.Count > 0)
                {
                    this.AppendChildStatements(insert.SelectStatements, true);
                }
                else
                {
                    this.AppendLine($"VALUES({string.Join(",", insert.Values.Select(item => item))});");
                }
            }
            else if (statement is UnionStatement union)
            {
                this.AppendLine(this.GetUnionTypeName(union.Type));
                this.Build(union.SelectStatement);
            }
            else if (statement is UpdateStatement update)
            {
                int fromItemsCount = update.FromItems == null ? 0 : update.FromItems.Count;
                TableName tableName = StatementScriptBuilderHelper.GetUpdateSetTableName(update);

                if (tableName == null && update.TableNames != null)
                {
                    tableName = update.TableNames.FirstOrDefault();
                }

                Func<TokenInfo, string> getCleanColumnName = (token) =>
                {
                    return token.Symbol.Split('.').LastOrDefault();
                };

                bool useAlias = false;

                this.AppendLine($"UPDATE {(useAlias ? tableName.Alias.Symbol : tableName.Symbol)}");

                this.Append("SET ");

                if (!StatementScriptBuilderHelper.IsCompositeUpdateSetColumnName(update))
                {
                    int k = 0;

                    foreach (var item in update.SetItems)
                    {
                        string cleanColumnName = getCleanColumnName(item.Name);

                        this.Append($"{cleanColumnName}=");

                        this.BuildUpdateSetValue(item);

                        if (k < update.SetItems.Count - 1)
                        {
                            this.Append(",");
                        }

                        this.AppendLine(this.Indent);

                        k++;
                    }
                }
                else if (update.SetItems.Count > 0)
                {
                    this.Append(StatementScriptBuilderHelper.ParseCompositeUpdateSet(this, update));

                    return this;
                }

                StringBuilder sb = new StringBuilder();

                if (fromItemsCount > 0)
                {
                    int i = 0;

                    foreach (FromItem fromItem in update.FromItems)
                    {
                        bool hasJoin = fromItem.HasJoinItems;

                        if (hasJoin)
                        {
                            if (i == 0)
                            {
                                this.AppendLine($"FROM {fromItem.TableName.NameWithAlias}");
                            }

                            foreach (JoinItem joinItem in fromItem.JoinItems)
                            {
                                string condition = joinItem.Condition == null ? "" : $" ON {joinItem.Condition}";

                                this.AppendLine($"{joinItem.Type} JOIN {joinItem.TableName.NameWithAlias}{condition}");
                            }
                        }
                        else
                        {
                            if (i == 0)
                            {
                                this.Append($"FROM ");
                            }
                            else
                            {
                                this.Append(",");
                            }

                            if (fromItem.SubSelectStatement != null)
                            {
                                string alias = fromItem.Alias == null ? "" : fromItem.Alias.Symbol;

                                this.AppendLine("(");
                                this.BuildSelectStatement(fromItem.SubSelectStatement, false);
                                this.AppendLine($") {alias}");
                            }
                            else if (fromItem.TableName != null)
                            {
                                this.Append($"{fromItem.TableName.NameWithAlias}");
                            }
                        }

                        i++;
                    }
                }
                else
                {
                    if (useAlias)
                    {
                        this.AppendLine($"FROM {tableName.NameWithAlias}");
                    }
                }

                if (update.Condition != null && update.Condition.Symbol != null)
                {
                    this.AppendLine($"WHERE {update.Condition}");
                }

                if (update.Option != null)
                {
                    this.AppendLine(update.Option.ToString());
                }

                this.AppendLine(";");
            }
            else if (statement is DeleteStatement delete)
            {
                bool hasJoin = AnalyserHelper.IsFromItemsHaveJoin(delete.FromItems);

                if (!hasJoin)
                {
                    this.AppendLine($"DELETE FROM {this.GetNameWithAlias(delete.TableName)}");

                    if (delete.Condition != null)
                    {
                        this.AppendLine($"WHERE {delete.Condition}");
                    }
                }
                else
                {
                    string tableName = delete.TableName.Symbol;

                    this.AppendLine($"DELETE FROM {delete.TableName}");

                    string alias = null;

                    int i = 0;

                    foreach (FromItem fromItem in delete.FromItems)
                    {
                        if (i == 0)
                        {
                            if (fromItem.TableName != null && fromItem.TableName.Alias != null)
                            {
                                alias = fromItem.TableName.Alias.Symbol;
                            }
                            else if (fromItem.Alias != null)
                            {
                                alias = fromItem.Alias.Symbol;
                            }
                        }

                        i++;
                    }

                    //use placeholder, the actual column name should according to the business needs.
                    this.AppendLine($"WHERE $columnName$ IN (SELECT $columnName$");

                    this.BuildFromItems(delete.FromItems, null, true);

                    if (delete.Condition != null)
                    {
                        this.AppendLine($"WHERE {delete.Condition}");
                    }

                    this.AppendLine(")");
                }

                this.AppendLine(";");
            }
            else if (statement is CreateTableStatement createTable)
            {
                this.AppendLine(this.BuildTable(createTable.TableInfo));
            }
            else if (statement is CaseStatement @case)
            {
                string variableName = @case.VariableName.ToString();

                IfStatement ifStatement = new IfStatement();

                int i = 0;
                foreach (var item in @case.Items)
                {
                    IfStatementItem ifItem = new IfStatementItem();

                    ifItem.Type = i == 0 ? IfStatementType.IF : item.Type;

                    if (item.Type != IfStatementType.ELSE)
                    {
                        ifItem.Condition = new TokenInfo($"{variableName}={item.Condition}") { Type = TokenType.IfCondition };
                    }

                    i++;
                }

                this.Build(ifStatement);
            }
            else if (statement is PrintStatement print)
            {
                this.AppendLine($"PRINT {print.Content.Symbol?.Replace("||", "+")};");
            }
            else if (statement is TruncateStatement truncate)
            {
                this.AppendLine($"DELETE FROM {truncate.TableName};");
            }
            else if (statement is DropStatement drop)
            {
                string objectType = drop.ObjectType.ToString().ToUpper();

                this.AppendLine($"DROP {objectType} IF EXISTS {drop.ObjectName.NameWithSchema};");
            }

            return this;
        }

        protected override void BuildSelectStatement(SelectStatement select, bool appendSeparator = true)
        {
            bool isCreateTemporaryTable = false;

            TokenInfo intoTableName = AnalyserHelper.GetIntoTableName(select);

            if (intoTableName != null)
            {
                isCreateTemporaryTable = true;

                this.AppendLine($"CREATE TEMPORARY TABLE IF NOT EXISTS {intoTableName} AS (");
            }

            bool isWith = select.WithStatements != null && select.WithStatements.Count > 0;

            string selectColumns = $"SELECT {string.Join(",", select.Columns.Select(item => this.GetNameWithAlias(item)))}";

            if (!isWith)
            {
                this.Append(selectColumns);
            }

            if (intoTableName != null)
            {
                this.AppendLine($"INTO {intoTableName}");
            }

            Action appendWith = () =>
            {
                int i = 0;

                foreach (WithStatement withStatement in select.WithStatements)
                {
                    if (i == 0)
                    {
                        this.AppendLine($"WITH {withStatement.Name}");

                        if (withStatement.Columns != null && withStatement.Columns.Count > 0)
                        {
                            this.AppendLine($"({string.Join(",", withStatement.Columns.Select(item => item))})");
                        }
                    }
                    else
                    {
                        this.AppendLine($",{withStatement.Name}");
                    }

                    this.AppendLine("AS(");

                    this.AppendChildStatements(withStatement.SelectStatements, false);

                    this.AppendLine(")");

                    i++;
                }
            };

            Action appendFrom = () =>
            {
                if (select.HasFromItems)
                {
                    this.BuildSelectStatementFromItems(select);
                }
                else if (select.TableName != null)
                {
                    this.AppendLine($"FROM {this.GetNameWithAlias(select.TableName)}");
                }
            };

            if (isWith)
            {
                appendWith();

                this.AppendLine(selectColumns);
            }

            appendFrom();

            if (select.Where != null)
            {
                this.Append($"WHERE {select.Where}");
            }

            if (select.GroupBy != null && select.GroupBy.Count > 0)
            {
                this.AppendLine($"GROUP BY {string.Join(",", select.GroupBy.Select(item => item))}");
            }

            if (select.Having != null)
            {
                this.AppendLine($"HAVING {select.Having}");
            }

            if (select.OrderBy != null && select.OrderBy.Count > 0)
            {
                this.AppendLine($"ORDER BY {string.Join(",", select.OrderBy.Select(item => item))}");
            }

            if (select.TopInfo != null)
            {
                this.AppendLine($"LIMIT 0,{select.TopInfo.TopCount}");
            }

            if (select.LimitInfo != null)
            {
                this.AppendLine($"LIMIT {select.LimitInfo.StartRowIndex?.Symbol ?? "0"},{select.LimitInfo.RowCount}");
            }

            if (select.UnionStatements != null)
            {
                foreach (UnionStatement union in select.UnionStatements)
                {
                    this.Build(union, false).TrimSeparator();
                    this.AppendLine();
                }
            }

            if (isCreateTemporaryTable)
            {
                this.AppendLine(")");
            }

            if (appendSeparator)
            {
                this.AppendLine(";");
            }
        }

        private void MakeupRoutineName(TokenInfo token)
        {
            string symbol = token.Symbol;
            int index = symbol.IndexOf("(");

            string name = index == -1 ? symbol : symbol.Substring(0, index);

            if (!name.Contains("."))
            {
                token.Symbol = $"dbo." + symbol;
            }
        }

        private string GetUnionTypeName(UnionType unionType)
        {
            switch (unionType)
            {
                case UnionType.UNION_ALL:
                    return "UNION ALL";
                default:
                    return unionType.ToString();
            }
        }

        public string BuildTable(TableInfo table)
        {
            StringBuilder sb = new StringBuilder();

            var columns = table.Columns;
            var selectStatement = table.SelectStatement;

            sb.AppendLine($"CREATE {(table.IsTemporary ? "TEMPORARY" : "")} TABLE {table.Name}{(columns.Count > 0 ? "(" : "AS")}");

            if (columns.Count > 0)
            {
                bool hasTableConstraints = table.HasTableConstraints;

                int i = 0;

                foreach (var column in columns)
                {
                    string name = column.Name.Symbol;
                    string dataType = column.DataType?.Symbol ?? "";
                    string require = column.IsNullable ? " NULL" : " NOT NULL";
                    string seperator = (i == table.Columns.Count - 1 ? (hasTableConstraints ? "," : "") : ",");

                    bool isComputeExp = column.IsComputed;

                    if (isComputeExp)
                    {
                        sb.AppendLine($"{name} {dataType} GENERATED ALWAYS AS ({column.ComputeExp}) STORED{require}{seperator}");
                    }
                    else
                    {
                        string identity = column.IsIdentity ? " AUTO_INCREMENT" : "";
                        string defaultValue = string.IsNullOrEmpty(column.DefaultValue?.Symbol) ? "" : $" DEFAULT {StringHelper.GetParenthesisedString(column.DefaultValue.Symbol)}";
                        string constraint = this.GetConstriants(column.Constraints, true);
                        string strConstraint = string.IsNullOrEmpty(constraint) ? "" : $" {constraint}";

                        sb.AppendLine($"{name} {column.DataType}{identity}{require}{defaultValue}{strConstraint}{seperator}");
                    }

                    i++;
                }

                if (hasTableConstraints)
                {
                    sb.AppendLine(this.GetConstriants(table.Constraints));
                }

                sb.Append(")");
            }
            else
            {
                SqliteStatementScriptBuilder builder = new SqliteStatementScriptBuilder();

                builder.BuildSelectStatement(selectStatement, false);

                sb.AppendLine(builder.ToString());
            }

            sb.AppendLine(";");

            return sb.ToString();
        }
    }
}
