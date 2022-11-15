using DatabaseInterpreter.Utility;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class PostgreSqlStatementScriptBuilder : StatementScriptBuilder
    {
        public override StatementScriptBuilder Build(Statement statement, bool appendSeparator = true)
        {
            base.Build(statement, appendSeparator);

            if (statement is SelectStatement select)
            {
                this.BuildSelectStatement(select, appendSeparator);
            }
            else if (statement is UnionStatement union)
            {
                this.AppendLine(this.GetUnionTypeName(union.Type));
                this.Build(union.SelectStatement);
            }
            else if (statement is InsertStatement insert)
            {
                this.AppendLine($"INSERT INTO {insert.TableName}");

                if (insert.Columns.Count > 0)
                {
                    this.AppendLine($"({string.Join(",", insert.Columns.Select(item => item.ToString()))})");
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
            else if (statement is UpdateStatement update)
            {
                int fromItemsCount = update.FromItems == null ? 0 : update.FromItems.Count;
                TableName tableName = StatementScriptBuilderHelper.GetUpdateSetTableName(update);
                string tableNameOrAlias = tableName?.Symbol;
                bool isCompositeColumnName = StatementScriptBuilderHelper.IsCompositeUpdateSetColumnName(update);

                this.AppendLine($"UPDATE");

                List<TableName> tableNames = new List<TableName>();

                List<string> joins = new List<string>();
                string otherCondition = null;
                string strTableName = null;
                string alias = null;

                if (tableName?.Alias != null)
                {
                    alias = tableName.Alias.Symbol;
                }

                Func<string, bool, string> getNoAliasString = (str, useOldName) =>
                {
                    if (str != null)
                    {
                        return alias == null ? str : str.Replace($"{alias}.", (useOldName ? $"{strTableName}." : ""));
                    }

                    return str;
                };

                if (update.FromItems != null)
                {
                    int i = 0;

                    foreach (FromItem fromItem in update.FromItems)
                    {
                        bool hasJoin = fromItem.HasJoinItems;

                        string tn = fromItem.TableName.Symbol;
                        string talias = fromItem.TableName.Alias?.ToString();

                        if (fromItem.TableName != null)
                        {
                            if (tn == tableNameOrAlias || talias == tableNameOrAlias)
                            {
                                tableNames.Add(fromItem.TableName);
                                strTableName = tn;
                                alias = talias;
                            }
                        }

                        if (hasJoin)
                        {
                            int j = 0;

                            foreach (var joinItem in fromItem.JoinItems)
                            {
                                if (j == 0)
                                {
                                    joins.Add($"FROM {joinItem.TableName.NameWithAlias}");
                                    otherCondition = getNoAliasString(joinItem.Condition.ToString(), true);
                                }
                                else
                                {
                                    joins.Add($"{joinItem.Type} JOIN {joinItem.TableName.NameWithAlias} ON {getNoAliasString(joinItem.Condition.ToString(), false)}");
                                }

                                j++;
                            }
                        }
                        else if (fromItemsCount > 1)
                        {
                            if (tn != strTableName)
                            {
                                joins.Add($"{(i == 0 ? "FROM" : "")} {fromItem.TableName.NameWithAlias}{((i < fromItemsCount - 2) ? "," : "")}");
                            }
                        }

                        i++;
                    }
                }

                if (tableNames.Count == 0 && update.TableNames.Count > 0)
                {
                    tableNames.AddRange(update.TableNames);

                    if (strTableName == null)
                    {
                        strTableName = update.TableNames.FirstOrDefault()?.Symbol;
                    }
                }

                this.Append($" {string.Join(",", tableNames.Select(item => item.NameWithAlias))}", false);

                this.AppendLine("SET");

                if (!isCompositeColumnName)
                {
                    this.AppendLine(string.Join("," + Environment.NewLine + Indent, update.SetItems.Select(item => $"{getNoAliasString(item.Name.ToString(), false)}={item.Value}")));

                    joins.ForEach(item => this.AppendLine(item));
                }
                else
                {
                    this.AppendLine(StatementScriptBuilderHelper.ParseCompositeUpdateSet(this, update));

                    return this;
                }

                bool hasCondition = false;

                if (update.Condition != null && update.Condition.Symbol != null)
                {
                    this.AppendLine($"WHERE {getNoAliasString(update.Condition.ToString(), true)}");
                    hasCondition = true;
                }

                if (otherCondition != null)
                {
                    this.AppendLine(hasCondition ? $"AND {otherCondition}" : $"WHERE {otherCondition}");
                }

                this.AppendLine(";");
            }
            else if (statement is DeleteStatement delete)
            {
                bool hasJoin = AnalyserHelper.IsFromItemsHaveJoin(delete.FromItems);

                if (!hasJoin)
                {
                    this.AppendLine($"DELETE FROM {this.GetNameWithAlias(delete.TableName)}");
                }
                else
                {
                    this.Append($"DELETE");

                    this.BuildFromItems(delete.FromItems, null, true);
                }

                if (delete.Condition != null)
                {
                    this.AppendLine($"{(hasJoin ? "AND" : "WHERE")} {delete.Condition}");
                }

                this.AppendLine(";");
            }
            else if (statement is DeclareVariableStatement declareVar)
            {
                StringBuilder sb = new StringBuilder();

                string defaultValue = (declareVar.DefaultValue == null ? "" : $" DEFAULT {declareVar.DefaultValue}");

                sb.Append($"DECLARE {declareVar.Name} {declareVar.DataType}{defaultValue};");

                if (!(this.Option != null && this.Option.NotBuildDeclareStatement))
                {
                    this.AppendLine(sb.ToString());
                }
                else
                {
                    if (this.Option.OutputRemindInformation)
                    {
                        this.PrintMessage($"'{StringHelper.HandleSingleQuotationChar(sb.ToString())}'");
                    }
                }

                if (this.Option != null && this.Option.CollectDeclareStatement)
                {
                    this.DeclareStatements.Add(declareVar);
                }
            }
            else if (statement is DeclareTableStatement declareTable)
            {
                this.AppendLine(this.BuildTable(declareTable.TableInfo));
            }
            else if (statement is CreateTableStatement createTable)
            {
                this.AppendLine(this.BuildTable(createTable.TableInfo));
            }
            else if (statement is IfStatement @if)
            {
                foreach (IfStatementItem item in @if.Items)
                {
                    if (item.Type == IfStatementType.IF || item.Type == IfStatementType.ELSEIF)
                    {
                        this.AppendLine($"{item.Type} {item.Condition} THEN");
                    }
                    else
                    {
                        this.AppendLine($"{item.Type}");
                    }

                    this.AppendLine("BEGIN");

                    this.AppendChildStatements(item.Statements, true);

                    this.AppendLine("END;");
                }

                this.AppendLine("END IF;");
            }
            else if (statement is CaseStatement @case)
            {
                this.AppendLine($"CASE {@case.VariableName}");

                foreach (IfStatementItem item in @case.Items)
                {
                    if (item.Type != IfStatementType.ELSE)
                    {
                        this.AppendLine($"WHEN {item.Condition} THEN");
                    }
                    else
                    {
                        this.AppendLine("ELSE");
                    }

                    this.AppendLine("BEGIN");
                    this.AppendChildStatements(item.Statements, true);
                    this.AppendLine("END;");
                }

                this.AppendLine("END CASE;");
            }
            else if (statement is SetStatement set)
            {
                if (set.Key != null && set.Value != null)
                {
                    this.AppendLine($"{set.Key} := {set.Value};");
                }
            }
            else if (statement is LoopStatement loop)
            {
                if (loop.Type == LoopType.LOOP)
                {
                    this.AppendLine("LOOP");
                }
                else
                {
                    this.AppendLine($"{loop.Type.ToString()} {loop.Condition} LOOP");
                }

                this.AppendChildStatements(loop.Statements, true);
                this.AppendLine("END LOOP;");
            }
            else if (statement is LoopExitStatement loopExit)
            {
                if (!loopExit.IsCursorLoopExit)
                {
                    this.AppendLine($"EXIT WHEN {loopExit.Condition};");
                }
                else
                {
                    this.AppendLine($"EXIT WHEN NOT FOUND;");
                }
            }
            else if (statement is WhileStatement @while)
            {
                LoopStatement loopStatement = @while as LoopStatement;

                this.AppendLine($"WHILE {@while.Condition} LOOP");
                this.AppendChildStatements(@while.Statements, true);
                this.AppendLine("END LOOP;");
            }
            else if (statement is ReturnStatement @return)
            {
                string value = @return.Value?.Symbol;

                if (this.RoutineType != RoutineType.PROCEDURE)
                {
                    this.AppendLine($"RETURN {value};");
                }
                else
                {
                    bool isStringValue = ValueHelper.IsStringValue(value);

                    this.PrintMessage(isStringValue ? StringHelper.HandleSingleQuotationChar(value) : value);

                    this.AppendLine("RETURN;");
                }
            }
            else if (statement is PrintStatement print)
            {
                this.PrintMessage(print.Content.Symbol);
            }
            else if (statement is CallStatement execute)
            {
                this.AppendLine($"{(execute.IsExecuteSql ? "EXECUTE" : "CALL")} {execute.Name}({string.Join(",", execute.Parameters.Select(item => item.Name?.Symbol?.Split('=')?.LastOrDefault()))});");
            }
            else if (statement is TransactionStatement transaction)
            {
                TransactionCommandType commandType = transaction.CommandType;

                switch (commandType)
                {
                    case TransactionCommandType.BEGIN:
                        this.AppendLine("START TRANSACTION;");
                        break;
                    case TransactionCommandType.COMMIT:
                        this.AppendLine("COMMIT;");
                        break;
                    case TransactionCommandType.ROLLBACK:
                        this.AppendLine("ROLLBACK;");
                        break;
                }
            }
            else if (statement is LeaveStatement leave)
            {
                this.AppendLine("RETURN;");
            }
            else if (statement is ExceptionStatement exception)
            {
                this.AppendLine("EXCEPTION");

                foreach (ExceptionItem exceptionItem in exception.Items)
                {
                    string name = exceptionItem.Name?.Symbol;

                    this.AppendLine($"WHEN {name} THEN");

                    this.AppendChildStatements(exceptionItem.Statements, true);
                }
            }
            else if (statement is TryCatchStatement tryCatch)
            {
                this.AppendChildStatements(tryCatch.TryStatements, true);

                this.AppendLine("EXCEPTION WHEN OTHERS THEN");

                this.AppendChildStatements(tryCatch.CatchStatements, true);
            }
            else if (statement is DeclareCursorStatement declareCursor)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append($"DECLARE {declareCursor.CursorName} CURSOR{(declareCursor.SelectStatement != null ? " FOR" : "")}");

                if (!(this.Option != null && this.Option.NotBuildDeclareStatement))
                {
                    this.AppendLine(sb.ToString());

                    if (declareCursor.SelectStatement != null)
                    {
                        this.Build(declareCursor.SelectStatement);
                    }
                }
                else
                {
                    if (this.Option.OutputRemindInformation)
                    {
                        this.PrintMessage($"'{StringHelper.HandleSingleQuotationChar(sb.ToString())}'");
                    }
                }

                if (this.Option != null && this.Option.CollectDeclareStatement)
                {
                    this.DeclareStatements.Add(declareCursor);
                }
            }
            else if (statement is OpenCursorStatement openCursor)
            {
                this.AppendLine($"OPEN {openCursor.CursorName};");
            }
            else if (statement is FetchCursorStatement fetchCursor)
            {
                if (fetchCursor.Variables.Count > 0)
                {
                    this.AppendLine($"FETCH {fetchCursor.CursorName} INTO {string.Join(",", fetchCursor.Variables)};");
                }
            }
            else if (statement is CloseCursorStatement closeCursor)
            {
                this.AppendLine($"CLOSE {closeCursor.CursorName};");
            }
            else if (statement is TruncateStatement truncate)
            {
                this.AppendLine($"TRUNCATE TABLE {truncate.TableName};");
            }
            else if (statement is DropStatement drop)
            {
                string objectType = drop.ObjectType.ToString().ToUpper();

                this.AppendLine($"DROP {objectType} IF EXISTS {drop.ObjectName.NameWithSchema};");
            }
            else if (statement is RaiseErrorStatement error)
            {
                if (error.Content != null)
                {
                    this.AppendLine($"RAISE EXCEPTION '%',{error.Content};");
                }
            }

            return this;
        }

        protected override void BuildSelectStatement(SelectStatement select, bool appendSeparator = true)
        {
            bool isWith = select.WithStatements != null && select.WithStatements.Count > 0;

            string selectColumns = $"SELECT {string.Join(",", select.Columns.Select(item => this.GetNameWithAlias(item)))}";

            if (select.NoTableName && select.Columns.Any(item => item.Symbol.Contains("=")))
            {
                foreach (var column in select.Columns)
                {
                    string symbol = column.Symbol;

                    if (this.IsIdentifierNameBeforeEqualMark(symbol))
                    {
                        string[] items = symbol.Split('=');

                        symbol = $"{items[0]}:={string.Join("=", items.Skip(1))}";
                    }

                    this.AppendLine($"{symbol};");
                }
            }
            else if (!isWith)
            {
                this.AppendLine(selectColumns);
            }

            if (select.IntoTableName != null)
            {
                this.AppendLine($"INTO {select.IntoTableName}");
            }

            Action appendWith = () =>
            {
                int i = 0;

                foreach (WithStatement withStatement in select.WithStatements)
                {
                    if (i == 0)
                    {
                        this.AppendLine($"WITH {withStatement.Name}");
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
                this.AppendLine($"WHERE {select.Where}");
            }

            if (select.GroupBy != null && select.GroupBy.Count > 0)
            {
                this.AppendLine($"GROUP BY {string.Join(",", select.GroupBy)}");
            }

            if (select.Having != null)
            {
                this.AppendLine($"HAVING {select.Having}");
            }

            if (select.OrderBy != null && select.OrderBy.Count > 0)
            {
                this.AppendLine($"ORDER BY {string.Join(",", select.OrderBy)}");
            }

            if (select.TopInfo != null)
            {
                this.AppendLine($"LIMIT {select.TopInfo.TopCount}");
            }

            if (select.LimitInfo != null)
            {
                this.AppendLine($"LIMIT {select.LimitInfo.RowCount} OFFSET {select.LimitInfo.StartRowIndex}");
            }

            if (select.UnionStatements != null)
            {
                foreach (var union in select.UnionStatements)
                {
                    this.Build(union, false).TrimSeparator();
                }
            }

            if (appendSeparator)
            {
                this.AppendLine(";");
            }
        }

        private string GetUnionTypeName(UnionType unionType)
        {
            switch (unionType)
            {
                case UnionType.UNION_ALL:
                    return "UNION ALL";
                case UnionType.EXCEPT:
                    return nameof(UnionType.MINUS);
                default:
                    return unionType.ToString();
            }
        }

        private void PrintMessage(string content)
        {
            this.AppendLine($"RAISE INFO '%',{content};");
        }

        public string BuildTable(TableInfo table)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CREATE {(table.IsTemporary ? "TEMPORARY" : "")} TABLE {table.Name}(");

            int i = 0;

            foreach (var column in table.Columns)
            {
                sb.AppendLine($"{column.Name.FieldName} {column.DataType}{(i == table.Columns.Count - 1 ? "" : ",")}");
                i++;
            }

            sb.AppendLine(");");

            return sb.ToString();
        }
    }
}
