using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class MySqlStatementScriptBuilder : StatementScriptBuilder
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
                this.Append($"INSERT INTO {insert.TableName}");

                if (insert.Columns.Count > 0)
                {
                    this.AppendLine($"({ string.Join(",", insert.Columns.Select(item => item))})");
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
                this.Append($"UPDATE");

                List<TableName> tableNames = new List<TableName>();

                if (update.FromItems != null)
                {
                    tableNames.Add(update.FromItems.First().TableName);
                }
                else if (update.TableNames.Count > 0)
                {
                    tableNames.AddRange(update.TableNames);
                }

                this.Append($" {string.Join(",", tableNames.Select(item=> item.NameWithAlias))}");

                if (update.FromItems != null && update.FromItems.Count > 0)
                {
                    int i = 0;

                    foreach (FromItem fromItem in update.FromItems)
                    {
                        if (fromItem.TableName != null && i > 0)
                        {
                            this.AppendLine($" {fromItem.TableName}");
                        }

                        foreach (JoinItem joinItem in fromItem.JoinItems)
                        {
                            string condition = joinItem.Condition == null ? "" : $" ON {joinItem.Condition}";

                            this.AppendLine($"{joinItem.Type} JOIN {this.GetNameWithAlias(joinItem.TableName)}{condition}");
                        }

                        i++;
                    }
                }

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
                this.AppendLine($"DELETE FROM {delete.TableName}");

                if (delete.Condition != null)
                {
                    this.AppendLine($"WHERE {delete.Condition}");
                }

                this.AppendLine(";");
            }
            else if (statement is DeclareStatement declare)
            {
                if (declare.Type == DeclareType.Variable)
                {
                    string defaultValue = (declare.DefaultValue == null ? "" : $" DEFAULT {declare.DefaultValue}");
                    this.AppendLine($"DECLARE {declare.Name} {declare.DataType}{defaultValue};");
                }
                else if (declare.Type == DeclareType.Table)
                {
                    this.AppendLine(BuildTemporaryTable(declare.Table));
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
                    this.AppendLine($"SET {set.Key } = {set.Value };");
                }
            }
            else if (statement is LoopStatement loop)
            {
                TokenInfo name = loop.Name;

                if (loop.Type != LoopType.LOOP)
                {
                    if (loop.Type == LoopType.LOOP)
                    {
                        this.AppendLine($"WHILE 1=1 DO");
                    }
                    else
                    {
                        this.AppendLine($"WHILE {loop.Condition} DO");
                    }
                }
                else
                {
                    this.AppendLine("LOOP");
                }

                this.AppendLine("BEGIN");

                this.AppendChildStatements(loop.Statements, true);

                this.AppendLine("END;");

                if (loop.Type != LoopType.LOOP)
                {
                    this.AppendLine($"END WHILE;");
                }
                else
                {
                    this.AppendLine($"END LOOP {(name == null ? "" : name + ":")};");
                }
            }
            else if (statement is WhileStatement @while)
            {
                this.AppendLine($"WHILE {@while.Condition} DO");

                this.AppendChildStatements(@while.Statements, true);

                this.AppendLine("END WHILE;");
            }
            else if (statement is LoopExitStatement whileExit)
            {
                if(!whileExit.IsCursorLoopExit)
                {
                    this.AppendLine($"IF {whileExit.Condition} THEN");
                    this.AppendLine("BEGIN");
                    this.AppendLine("BREAK;");
                    this.AppendLine("END;");
                    this.AppendLine("END IF;");
                }               
            }
            else if (statement is ReturnStatement @return)
            {
                this.AppendLine($"RETURN {@return.Value};");
            }
            else if (statement is PrintStatement print)
            {
                this.AppendLine($"SELECT {print.Content.Symbol?.Replace("||", "+")};");
            }
            else if (statement is CallStatement call)
            {
                this.AppendLine($"CALL {call.Name}({string.Join(",", call.Arguments.Select(item => item.Symbol?.Split('=')?.LastOrDefault()))});");
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
                this.AppendLine("LEAVE sp;");
            }
            else if (statement is TryCatchStatement tryCatch)
            {
                this.AppendLine("DECLARE EXIT HANDLER FOR 1 #[REPLACE ERROR CODE HERE]");
                this.AppendLine("BEGIN");

                this.AppendChildStatements(tryCatch.CatchStatements, true);

                this.AppendLine("END;");

                this.AppendChildStatements(tryCatch.TryStatements, true);
            }
            else if (statement is ExceptionStatement exception)
            {
                foreach (ExceptionItem exceptionItem in exception.Items)
                {
                    this.AppendLine($"DECLARE EXIT HANDLER FOR {exceptionItem.Name}");
                    this.AppendLine("BEGIN");

                    this.AppendChildStatements(exceptionItem.Statements, true);

                    this.AppendLine("END;");
                }
            }
            else if (statement is DeclareCursorStatement declareCursor)
            {
                this.AppendLine($"DECLARE {declareCursor.CursorName} CURSOR FOR");
                this.Build(declareCursor.SelectStatement);
            }
            else if (statement is DeclareCursorHandlerStatement declareCursorHandler)
            {
                this.AppendLine($"DECLARE CONTINUE HANDLER");
                this.AppendLine($"FOR NOT FOUND");
                this.AppendLine($"BEGIN");
                this.AppendChildStatements(declareCursorHandler.Statements, true);
                this.AppendLine($"END;");
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
                this.AppendLine($"TRUNCATE TABLE { truncate.TableName};");
            }

            return this;
        }

        protected override void BuildSelectStatement(SelectStatement select, bool appendSeparator = true)
        {
            bool isWith = select.WithStatements != null && select.WithStatements.Count > 0;

            string selectColumns = $"SELECT {string.Join(",", select.Columns.Select(item => this.GetNameWithAlias(item)))}";

            if (select.TableName == null && select.Columns.Count == 1 && select.Columns[0].Symbol.Contains("="))
            {
                ColumnName columnName = select.Columns.First();

                this.AppendLine($"SET {columnName}");
            }
            else if(!isWith)
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
                this.AppendLine($"GROUP BY {string.Join(",", select.GroupBy.Select(item=>item))}");
            }

            if (select.Having != null)
            {
                this.AppendLine($"HAVING {select.Having}");
            }

            if (select.OrderBy != null && select.OrderBy.Count > 0)
            {
                this.AppendLine($"ORDER BY {string.Join(",", select.OrderBy.Select(item=>item))}");
            }

            if (select.TopInfo != null)
            {
                this.AppendLine($"LIMIT 0,{select.TopInfo.TopCount}");
            }

            if (select.LimitInfo != null)
            {
                this.AppendLine($"LIMIT {select.LimitInfo.StartRowIndex},{select.LimitInfo.RowCount}");
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
                this.AppendLine(";", false);
            }
        }

        private string GetUnionTypeName(UnionType unionType)
        {
            switch (unionType)
            {
                case UnionType.UNION_ALL:
                    return "UNION ALL";
                //case UnionType.INTERSECT:
                //case UnionType.MINUS:
                //case UnionType.EXCEPT:
                //    return nameof(UnionType.UNION);
                default:
                    return unionType.ToString();
            }
        }

        public static string BuildTemporaryTable(TemporaryTable table)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CREATE TEMPORARY TABLE {table.Name}(");

            int i = 0;
            foreach (var column in table.Columns)
            {
                sb.AppendLine($"{column.Symbol} {column.DataType}{(i == table.Columns.Count - 1 ? "" : ",")}");
                i++;
            }

            sb.AppendLine(");");

            return sb.ToString();
        }
    }
}
