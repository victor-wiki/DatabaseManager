using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlAnalyser.Core
{
    public class PlSqlStatementScriptBuilder : StatementScriptBuilder
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
                this.AppendLine($"UPDATE");

                List<TableName> tableNames = new List<TableName>();

                if (update.FromItems != null)
                {
                    int i = 0;

                    List<NameValueItem> nameValues = new List<NameValueItem>();

                    foreach (FromItem fromItem in update.FromItems)
                    {
                        if (i == 0 && fromItem.TableName != null)
                        {
                            tableNames.Add(fromItem.TableName);
                        }

                        NameValueItem nameValueItem = new NameValueItem();

                        nameValueItem.Name = new TokenInfo($"({string.Join(",", update.SetItems.Select(item => item.Name))})");

                        string colNames = string.Join(",", update.SetItems.Select(item => item.Value));

                        string join = "";

                        int j = 0;
                        foreach (var joinItem in fromItem.JoinItems)
                        {
                            if (j == 0)
                            {
                                join += "FROM";
                            }
                            else
                            {
                                join += joinItem.Type.ToString() + " JOIN";
                            }

                            join += $" {this.GetNameWithAlias(joinItem.TableName)} ON {joinItem.Condition}{Environment.NewLine}";
                            j++;
                        }

                        nameValueItem.Value = new TokenInfo($"(SELECT {colNames} {join})");

                        nameValues.Add(nameValueItem);

                        i++;
                    }

                    update.SetItems = nameValues;
                }

                if (tableNames.Count == 0 && update.TableNames.Count > 0)
                {
                    tableNames.AddRange(update.TableNames);
                }

                this.Append($" {string.Join(",", tableNames.Select(item => item.NameWithAlias))}", false);

                this.AppendLine("SET");

                this.AppendLine(string.Join("," + Environment.NewLine + indent, update.SetItems.Select(item => $"{item.Name}={item.Value}")));

                if (update.Condition != null && update.Condition.Symbol != null)
                {
                    this.AppendLine($"WHERE {update.Condition}");
                }

                this.AppendLine(";");
            }
            else if (statement is DeleteStatement delete)
            {
                this.AppendLine($"DELETE {delete.TableName}");

                if (delete.Condition != null)
                {
                    this.AppendLine($"WHERE {delete.Condition}");
                }

                this.AppendLine(";");
            }
            else if (statement is DeclareStatement declare)
            {
                if (!(this.Option != null && this.Option.NotBuildDeclareStatement))
                {
                    if (declare.Type == DeclareType.Variable)
                    {
                        string defaultValue = (declare.DefaultValue == null ? "" : $" :={declare.DefaultValue}");

                        this.AppendLine($"DECLARE {declare.Name} {declare.DataType}{defaultValue};");
                    }
                    else if (declare.Type == DeclareType.Table)
                    {

                    }
                }

                if (this.Option != null && this.Option.CollectDeclareStatement)
                {
                    this.DeclareStatements.Add(declare);
                }
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
                this.AppendLine($"EXIT WHEN {loopExit.Condition};");
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
                this.AppendLine($"RETURN {@return.Value};");
            }
            else if (statement is PrintStatement print)
            {
                this.AppendLine($"DBMS_OUTPUT.PUT_LINE({print.Content.Symbol?.ToString()?.Replace("+", "||")});");
            }
            else if (statement is CallStatement execute)
            {
                this.AppendLine($"{(execute.IsExecuteSql ? "EXECUTE IMMEDIATE" : "")} {execute.Name}({string.Join(",", execute.Arguments.Select(item => item.Symbol?.Split('=')?.LastOrDefault()))});");
            }
            else if (statement is TransactionStatement transaction)
            {
                TransactionCommandType commandType = transaction.CommandType;

                switch (commandType)
                {
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
            else if (statement is TryCatchStatement tryCatch)
            {
                this.AppendLine("EXCEPTION");
                this.AppendLine("BEGIN");

                this.AppendChildStatements(tryCatch.CatchStatements, true);

                this.AppendLine("END;");

                this.AppendChildStatements(tryCatch.TryStatements, true);
            }
            else if (statement is ExceptionStatement exception)
            {
                this.AppendLine("EXCEPTION");

                foreach (ExceptionItem exceptionItem in exception.Items)
                {
                    this.AppendLine($"WHEN {exceptionItem.Name} THEN");
                    this.AppendLine("BEGIN");

                    this.AppendChildStatements(exceptionItem.Statements, true);

                    this.AppendLine("END;");
                }
            }
            else if (statement is DeclareCursorStatement declareCursor)
            {
                if (!(this.Option != null && this.Option.NotBuildDeclareStatement))
                {
                    this.AppendLine($"DECLARE CURSOR {declareCursor.CursorName}{(declareCursor.SelectStatement != null ? " IS" : "")}");

                    if (declareCursor.SelectStatement != null)
                    {
                        this.Build(declareCursor.SelectStatement);
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

                this.AppendLine($"DROP {objectType} {drop.ObjectName.NameWithSchema};");
            }

            return this;
        }

        protected override void BuildSelectStatement(SelectStatement select, bool appendSeparator = true)
        {
            bool isWith = select.WithStatements != null && select.WithStatements.Count > 0;
            bool hasAssignVariableColumn = this.HasAssignVariableColumn(select);
            string selectColumns = $"SELECT {string.Join(",", select.Columns.Select(item => this.GetNameWithAlias(item)))}";

            bool handled = false;

            if (select.TableName == null && hasAssignVariableColumn)
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

                handled = true;
            }
            else if (select.TableName != null && hasAssignVariableColumn && (this.RoutineType == RoutineType.PROCEDURE || this.RoutineType == RoutineType.FUNCTION || this.RoutineType == RoutineType.TRIGGER))
            {
                //use "select column1, column2 into var1, var2" instead of "select var1=column1, var2=column2"

                List<string> variables = new List<string>();
                List<string> columnNames = new List<string>();

                foreach (var column in select.Columns)
                {
                    if (column.Symbol.Contains("="))
                    {
                        string[] items = column.Symbol.Split('=');
                        string variable = items[0].Trim();
                        string columName = items[1].Trim();

                        variables.Add(variable);
                        columnNames.Add(columName);
                    }
                }

                this.AppendLine($"SELECT {string.Join(",", columnNames)} INTO {string.Join(",", variables)}");
            }
            else if (!isWith)
            {
                this.AppendLine(selectColumns);
            }

            if (!handled)
            {
                if (select.TableName == null)
                {
                    select.TableName = new TableName("DUAL");
                }
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
                if (select.FromItems != null && select.FromItems.Count > 0)
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
                this.AppendLine($"FETCH NEXT {select.TopInfo.TopCount} ROWS ONLY");
            }

            if (select.LimitInfo != null)
            {
                this.AppendLine($"OFFSET {select.LimitInfo.StartRowIndex} ROWS FETCH NEXT {select.LimitInfo.RowCount} ROWS ONLY");
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
    }
}
