using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class MySqlAnalyser : SqlAnalyserBase
    {
        private MySqlRuleAnalyser ruleAnalyser = null;

        public override DatabaseType DatabaseType => DatabaseType.MySql;

        public MySqlAnalyser()
        {
            this.ruleAnalyser = new MySqlRuleAnalyser();
        }

        public override ViewScript AnalyseView(string content)
        {
            ViewScript view = this.ruleAnalyser.AnalyseView(content);

            return view;
        }

        public override RoutineScript AnalyseProcedure(string content)
        {
            RoutineScript procedure = this.ruleAnalyser.AnalyseProcedure(content);

            procedure.Type = RoutineType.PROCEDURE;

            return procedure;
        }

        public override RoutineScript AnalyseFunction(string content)
        {
            RoutineScript function = this.ruleAnalyser.AnalyseFunction(content);
            function.Type = RoutineType.FUNCTION;

            return function;
        }

        public override TriggerScript AnalyseTrigger(string content)
        {
            TriggerScript trigger = this.ruleAnalyser.AnalyseTrigger(content);

            return trigger;
        }

        public override string GenerateScripts(CommonScript script)
        {
            if (script is RoutineScript routineScript)
            {
                return this.GenerateRoutineScripts(routineScript);
            }
            else if (script is ViewScript viewScript)
            {
                return this.GenearteViewScripts(viewScript);
            }
            else if (script is TriggerScript triggerScript)
            {
                return this.GenearteTriggerScripts(triggerScript);
            }
            else
            {
                throw new NotSupportedException($"Not support generate scripts for type: {script.GetType()}.");
            }
        }

        public string GenerateRoutineScripts(RoutineScript script)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CREATE {script.Type.ToString()} {script.FullName}");

            sb.AppendLine("(");

            if (script.Parameters.Count > 0)
            {
                int i = 0;
                foreach (Parameter parameter in script.Parameters)
                {
                    ParameterType parameterType = parameter.ParameterType;

                    string strParameterType = "";

                    if (parameterType.HasFlag(ParameterType.IN) && parameterType.HasFlag(ParameterType.OUT))
                    {
                        strParameterType = "INOUT";
                    }
                    else if (parameterType != ParameterType.NONE)
                    {
                        strParameterType = parameterType.ToString();
                    }

                    sb.AppendLine($"{strParameterType} {parameter.Name} {parameter.DataType} {(i == script.Parameters.Count - 1 ? "" : ",")}");

                    i++;
                }
            }

            sb.AppendLine(")");

            if (script.Type == RoutineType.FUNCTION)
            {
                sb.AppendLine($"RETURNS {script.ReturnDataType}");
            }

            int beginIndex = sb.Length - 1;
            bool hasLeaveStatement = false;

            sb.AppendLine("BEGIN");

            foreach (Statement statement in script.Statements.Where(item => item is DeclareStatement))
            {
                sb.AppendLine(this.BuildStatement(statement));
            }

            #region Cursor

            Action appendDeclareCursor = () =>
            {
                foreach (Statement statement in script.Statements.Where(item => item is DeclareCursorStatement))
                {
                    sb.AppendLine(this.BuildStatement(statement));
                }
            };

            if (script.Statements.Any(item => item is OpenCursorStatement) && !script.Statements.Any(item => item is DeclareCursorHandlerStatement))
            {
                if (!script.Statements.Any(item => item is DeclareStatement && (item as DeclareStatement).Name.Symbol == "FINISHED"))
                {
                    DeclareStatement declareStatement = new DeclareStatement()
                    {
                        Name = new TokenInfo("FINISHED") { Type = TokenType.VariableName },
                        DataType = new TokenInfo("INT") { Type = TokenType.DataType },
                        DefaultValue = new TokenInfo("0")
                    };

                    sb.AppendLine(this.BuildStatement(declareStatement));
                }

                appendDeclareCursor();

                DeclareCursorHandlerStatement handler = new DeclareCursorHandlerStatement();
                handler.Statements.Add(new SetStatement() { Key = new TokenInfo("FINISHED") { Type = TokenType.VariableName }, Value = new TokenInfo("1") });

                sb.AppendLine(this.BuildStatement(handler));
            }
            else
            {
                appendDeclareCursor();
            }

            #endregion

            if (script.ReturnTable != null)
            {
                sb.AppendLine(this.BuildTemporaryTable(script.ReturnTable));
            }

            FetchCursorStatement fetchCursorStatement = null;

            foreach (Statement statement in script.Statements.Where(item => !(item is DeclareStatement || item is DeclareCursorStatement)))
            {
                if (statement is FetchCursorStatement fetch)
                {
                    fetchCursorStatement = fetch;
                    continue;
                }
                else if (statement is WhileStatement @while)
                {
                    FetchCursorStatement fs = @while.Statements.FirstOrDefault(item => item is FetchCursorStatement) as FetchCursorStatement;

                    if (fetchCursorStatement != null && fs != null)
                    {
                        @while.Condition.Symbol = "FINISHED = 0";

                        if (fs.Variables.Count == 0)
                        {
                            @while.Statements.Insert(0, fetchCursorStatement);
                        }
                    }
                }

                if (statement is LeaveStatement)
                {
                    hasLeaveStatement = true;
                }

                sb.AppendLine(this.BuildStatement(statement));
            }

            sb.AppendLine("END");

            if (hasLeaveStatement)
            {
                sb.Insert(beginIndex, "sp:");
            }

            return this.FormatScripts(sb.ToString());
        }

        public string GenearteViewScripts(ViewScript script)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CREATE VIEW {script.FullName} AS");

            foreach (Statement statement in script.Statements)
            {
                sb.AppendLine(this.BuildStatement(statement));
            }

            return this.FormatScripts(sb.ToString());
        }

        public string GenearteTriggerScripts(TriggerScript script)
        {
            StringBuilder sb = new StringBuilder();

            string events = string.Join(",", script.Events);

            sb.AppendLine($"CREATE TRIGGER {script.FullName} {script.Time} {events} ON {script.TableName}");
            sb.AppendLine($"FOR EACH ROW {script.Behavior} {script.OtherTriggerName}");

            int beginIndex = sb.Length - 1;
            bool hasLeaveStatement = false;

            sb.AppendLine("BEGIN");

            foreach (Statement statement in script.Statements.Where(item => item is DeclareStatement))
            {
                sb.AppendLine(this.BuildStatement(statement));
            }

            foreach (Statement statement in script.Statements.Where(item => !(item is DeclareStatement)))
            {
                if (statement is LeaveStatement)
                {
                    hasLeaveStatement = true;
                }

                sb.AppendLine(this.BuildStatement(statement));
            }

            sb.AppendLine("END");

            if (hasLeaveStatement)
            {
                sb.Insert(beginIndex, "sp:");
            }

            return this.FormatScripts(sb.ToString());
        }

        private string BuildStatement(Statement statement, int level = 0, bool appendSeparator = true)
        {
            StringBuilder sb = new StringBuilder();

            string indent = " ".PadLeft((level + 1) * 2);

            Action<string> append = (value) => { sb.Append($"{indent}{value}"); };

            Action<string> appendLine = (value) => { append(value + Environment.NewLine); };

            Action<IEnumerable<Statement>, bool> appendStatements = (statements, needSeparator) =>
            {
                foreach (Statement st in statements)
                {
                    append(this.BuildStatement(st, level + 1, needSeparator));
                }
            };

            if (statement is SelectStatement select)
            {
                if (select.TableName == null && select.Columns.Count == 1 && select.Columns[0].Symbol.Contains("="))
                {
                    ColumnName columnName = select.Columns.First();

                    appendLine($"SET {columnName}");
                }
                else
                {
                    appendLine($"SELECT {string.Join("," + Environment.NewLine + indent, select.Columns.Select(item => item.ToString()))}");
                }

                if (select.IntoTableName != null)
                {
                    appendLine($"INTO {select.IntoTableName.ToString()}");
                }

                if (select.FromItems != null && select.FromItems.Count > 0)
                {
                    int i = 0;
                    foreach (FromItem fromItem in select.FromItems)
                    {
                        if (i == 0)
                        {
                            appendLine($"FROM {fromItem.TableName}");
                        }

                        foreach (JoinItem joinItem in fromItem.JoinItems)
                        {
                            string condition = joinItem.Condition == null ? "" : $" ON {joinItem.Condition}";

                            appendLine($"{joinItem.Type} JOIN {joinItem.TableName}{condition}");
                        }

                        i++;
                    }
                }
                else if (select.TableName != null)
                {
                    if (select.WithStatements == null || select.WithStatements.Count == 0)
                    {
                        appendLine($"FROM {select.TableName}");
                    }
                    else
                    {
                        string tableName = select.TableName.ToString();

                        appendLine("FROM");

                        int i = 0;

                        foreach (WithStatement withStatement in select.WithStatements)
                        {
                            appendLine("(");

                            appendStatements(withStatement.SelectStatements, false);

                            appendLine($") AS {(tableName.StartsWith(withStatement.Name.ToString()) ? "" : withStatement.Name.ToString())}{(i < select.WithStatements.Count - 1 ? "," : "")}");

                            i++;
                        }

                        appendLine(select.TableName.ToString());
                    }
                }

                if (select.Where != null)
                {
                    appendLine($"WHERE {select.Where}");
                }

                if (select.GroupBy != null && select.GroupBy.Count > 0)
                {
                    appendLine($"GROUP BY {string.Join(",", select.GroupBy)}");
                }

                if (select.Having != null)
                {
                    appendLine($"HAVING {select.Having}");
                }

                if (select.OrderBy != null && select.OrderBy.Count > 0)
                {
                    appendLine($"ORDER BY {string.Join(",", select.OrderBy)}");
                }

                if (select.TopInfo != null)
                {
                    appendLine($"LIMIT 0,{select.TopInfo.TopCount}");
                }

                if (select.LimitInfo != null)
                {
                    appendLine($"LIMIT {select.LimitInfo.StartRowIndex},{select.LimitInfo.RowCount}");
                }

                if (select.UnionStatements != null)
                {
                    foreach (var union in select.UnionStatements)
                    {
                        appendLine("UNION");
                        append(this.BuildStatement(union, level, false).TrimEnd(';'));
                    }
                }

                if (appendSeparator)
                {
                    appendLine(";");
                }
            }
            else if (statement is InsertStatement insert)
            {
                append($"INSERT INTO {insert.TableName}");

                if (insert.Columns.Count > 0)
                {
                    appendLine($"({ string.Join(",", insert.Columns.Select(item => item.ToString()))})");
                }

                if (insert.SelectStatements != null && insert.SelectStatements.Count > 0)
                {
                    appendStatements(insert.SelectStatements, true);
                }
                else
                {
                    appendLine($"VALUES({string.Join(",", insert.Values.Select(item => item))});");
                }
            }
            else if (statement is UpdateStatement update)
            {
                append($"UPDATE");

                List<TokenInfo> tableNames = new List<TokenInfo>();

                if (update.FromItems != null)
                {
                    tableNames.Add(update.FromItems.First().TableName);
                }
                else if (update.TableNames.Count > 0)
                {
                    tableNames.AddRange(update.TableNames);
                }

                append($" {string.Join(",", tableNames)}");

                if (update.FromItems != null && update.FromItems.Count > 0)
                {
                    int i = 0;

                    foreach (FromItem fromItem in update.FromItems)
                    {
                        if (fromItem.TableName != null && i > 0)
                        {
                            appendLine($" {fromItem.TableName}");
                        }

                        foreach (JoinItem joinItem in fromItem.JoinItems)
                        {
                            string condition = joinItem.Condition == null ? "" : $" ON {joinItem.Condition}";

                            appendLine($"{joinItem.Type} JOIN {joinItem.TableName}{condition}");
                        }

                        i++;
                    }
                }

                appendLine("SET");

                appendLine(string.Join("," + Environment.NewLine + indent, update.SetItems.Select(item => $"{item.Name}={item.Value}")));

                if (update.Condition != null && update.Condition.Symbol != null)
                {
                    appendLine($"WHERE {update.Condition}");
                }

                appendLine(";");
            }
            else if (statement is DeleteStatement delete)
            {
                appendLine($"DELETE FROM {delete.TableName}");

                if (delete.Condition != null)
                {
                    appendLine($"WHERE {delete.Condition}");
                }

                appendLine(";");
            }
            else if (statement is DeclareStatement declare)
            {
                if (declare.Type == DeclareType.Variable)
                {
                    string defaultValue = (declare.DefaultValue == null ? "" : $"DEFAULT {declare.DefaultValue}");
                    appendLine($"DECLARE {declare.Name} {declare.DataType} {defaultValue};");
                }
                else if (declare.Type == DeclareType.Table)
                {
                    appendLine(this.BuildTemporaryTable(declare.Table));
                }
            }
            else if (statement is IfStatement @if)
            {
                foreach (IfStatementItem item in @if.Items)
                {
                    if (item.Type == IfStatementType.IF || item.Type == IfStatementType.ELSEIF)
                    {
                        appendLine($"{item.Type} {item.Condition} THEN");
                    }
                    else
                    {
                        appendLine($"{item.Type}");
                    }

                    appendLine("BEGIN");

                    appendStatements(item.Statements, true);

                    appendLine("END;");
                }
                appendLine("END IF;");
            }
            else if (statement is CaseStatement @case)
            {
                appendLine($"CASE {@case.VariableName}");

                foreach (IfStatementItem item in @case.Items)
                {
                    if (item.Type != IfStatementType.ELSE)
                    {
                        appendLine($"WHEN {item.Condition} THEN");
                    }
                    else
                    {
                        appendLine("ELSE");
                    }

                    appendLine("BEGIN");
                    appendStatements(item.Statements, true);
                    appendLine("END;");
                }

                appendLine("END CASE;");
            }
            else if (statement is SetStatement set)
            {
                if (set.Key != null && set.Value != null)
                {
                    appendLine($"SET {set.Key } = {set.Value };");
                }
            }
            else if (statement is LoopStatement loop)
            {
                TokenInfo name = loop.Name;

                if (loop.Type != LoopType.LOOP)
                {
                    if (loop.Type == LoopType.LOOP)
                    {
                        appendLine($"WHILE 1=1 DO");
                    }
                    else
                    {
                        appendLine($"WHILE {loop.Condition} DO");
                    }
                }
                else
                {
                    appendLine("LOOP");
                }

                appendLine("BEGIN");

                appendStatements(loop.Statements, true);

                appendLine("END;");

                if (loop.Type != LoopType.LOOP)
                {
                    appendLine($"END WHILE;");
                }
                else
                {
                    appendLine($"END LOOP {(name == null ? "" : name + ":")};");
                }
            }
            else if (statement is WhileStatement @while)
            {
                appendLine($"WHILE {@while.Condition} DO");

                appendStatements(@while.Statements, true);

                appendLine("END WHILE;");
            }
            else if (statement is LoopExitStatement whileExit)
            {
                appendLine($"IF {whileExit.Condition} THEN");
                appendLine("BEGIN");
                appendLine("BREAK;");
                appendLine("END;");
                appendLine("END IF;");
            }
            else if (statement is ReturnStatement @return)
            {
                appendLine($"RETURN {@return.Value};");
            }
            else if (statement is PrintStatement print)
            {
                appendLine($"SELECT {print.Content.Symbol?.Replace("||", "+")};");
            }
            else if (statement is CallStatement call)
            {
                appendLine($"CALL {call.Name}({string.Join(",", call.Arguments.Select(item => item.Symbol?.Split('=')?.LastOrDefault()))});");
            }
            else if (statement is TransactionStatement transaction)
            {
                TransactionCommandType commandType = transaction.CommandType;

                switch (commandType)
                {
                    case TransactionCommandType.BEGIN:
                        appendLine("START TRANSACTION;");
                        break;
                    case TransactionCommandType.COMMIT:
                        appendLine("COMMIT;");
                        break;
                    case TransactionCommandType.ROLLBACK:
                        appendLine("ROLLBACK;");
                        break;
                }
            }
            else if (statement is LeaveStatement leave)
            {
                appendLine("LEAVE sp;");
            }
            else if (statement is TryCatchStatement tryCatch)
            {
                appendLine("DECLARE EXIT HANDLER FOR 1 #[REPLACE ERROR CODE HERE]");
                appendLine("BEGIN");

                appendStatements(tryCatch.CatchStatements, true);

                appendLine("END;");

                appendStatements(tryCatch.TryStatements, true);
            }
            else if (statement is ExceptionStatement exception)
            {
                foreach (ExceptionItem exceptionItem in exception.Items)
                {
                    appendLine($"DECLARE EXIT HANDLER FOR {exceptionItem.Name}");
                    appendLine("BEGIN");

                    appendStatements(exceptionItem.Statements, true);

                    appendLine("END;");
                }
            }
            else if (statement is DeclareCursorStatement declareCursor)
            {
                appendLine($"DECLARE {declareCursor.CursorName} CURSOR FOR");
                append(this.BuildStatement(declareCursor.SelectStatement));
            }
            else if (statement is DeclareCursorHandlerStatement declareCursorHandler)
            {
                appendLine($"DECLARE CONTINUE HANDLER");
                appendLine($"FOR NOT FOUND");
                appendLine($"BEGIN");
                appendStatements(declareCursorHandler.Statements, true);
                appendLine($"END;");
            }
            else if (statement is OpenCursorStatement openCursor)
            {
                appendLine($"OPEN {openCursor.CursorName};");
            }
            else if (statement is FetchCursorStatement fetchCursor)
            {
                if (fetchCursor.Variables.Count > 0)
                {
                    appendLine($"FETCH {fetchCursor.CursorName} INTO {string.Join(",", fetchCursor.Variables)};");
                }
            }
            else if (statement is CloseCursorStatement closeCursor)
            {
                appendLine($"CLOSE {closeCursor.CursorName};");
            }
            else if (statement is TruncateStatement truncate)
            {
                appendLine($"TRUNCATE TABLE {truncate.TableName};");
            }

            return sb.ToString();
        }

        private string BuildTemporaryTable(TemporaryTable table)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CREATE TEMPORARY TABLE {table.Name}(");

            int i = 0;
            foreach (var column in table.Columns)
            {
                sb.AppendLine($"{column.Name} {column.DataType}{(i == table.Columns.Count - 1 ? "" : ",")}");
                i++;
            }

            sb.AppendLine(");");

            return sb.ToString();
        }
    }
}
