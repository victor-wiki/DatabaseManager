using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlAnalyser.Core
{
    public class PlSqlAnalyser : SqlAnalyserBase
    {
        private PlSqlRuleAnalyser ruleAnalyser = null;

        public override DatabaseType DatabaseType => DatabaseType.Oracle;

        public PlSqlAnalyser()
        {
            this.ruleAnalyser = new PlSqlRuleAnalyser();
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

            sb.AppendLine($"CREATE OR REPLACE {script.Type.ToString()} {script.FullName}");

            if (script.Parameters.Count > 0)
            {
                sb.AppendLine("(");
                int i = 0;
                foreach (Parameter parameter in script.Parameters)
                {
                    ParameterType parameterType = parameter.ParameterType;

                    string dataType = parameter.DataType.Symbol;
                    string strParameterType = "";

                    int parenthesesIndex = dataType.IndexOf("(");

                    if (parenthesesIndex > 0)
                    {
                        dataType = dataType.Substring(0, parenthesesIndex);
                    }

                    if (parameterType.HasFlag(ParameterType.IN) && parameterType.HasFlag(ParameterType.OUT))
                    {
                        strParameterType = "IN OUT";
                    }
                    else if (parameterType != ParameterType.NONE)
                    {
                        strParameterType = parameterType.ToString();
                    }

                    sb.AppendLine($"{parameter.Name} {strParameterType} {dataType} {(i == script.Parameters.Count - 1 ? "" : ",")}");

                    i++;
                }

                sb.AppendLine(")");
            }

            if (script.Type == RoutineType.FUNCTION)
            {
                sb.AppendLine($"RETURN {script.ReturnDataType}");
            }

            sb.AppendLine("AS");

            foreach (Statement statement in script.Statements.Where(item => item is DeclareStatement || item is DeclareCursorStatement))
            {
                sb.AppendLine(this.BuildStatement(statement).Replace("DECLARE ", ""));
            }

            sb.AppendLine("BEGIN");

            if (script.ReturnTable != null)
            {

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
                        @while.Condition.Symbol = $"{fs.CursorName}%NOTFOUND";

                        if (fs.Variables.Count == 0)
                        {
                            @while.Statements.Insert(0, fetchCursorStatement);
                        }
                    }
                }

                sb.AppendLine(this.BuildStatement(statement));
            }

            sb.AppendLine($"END {script.FullName};");

            return this.FormatScripts(sb.ToString());
        }

        public string GenearteViewScripts(ViewScript script)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CREATE VIEW {script.FullName} AS");

            foreach (SelectStatement statement in script.Statements.Where(item => item is SelectStatement))
            {
                sb.AppendLine($"SELECT {string.Join("," + Environment.NewLine, statement.Columns.Select(item => item.ToString()))}");

                sb.AppendLine($"FROM {statement.TableName}");

                if (statement.Condition != null)
                {
                    sb.AppendLine($"WHERE {statement.Condition}");
                }

                sb.Append(";");
            }

            return this.FormatScripts(sb.ToString());
        }

        public string GenearteTriggerScripts(TriggerScript script)
        {
            StringBuilder sb = new StringBuilder();

            string events = string.Join(" OR ", script.Events);

            sb.AppendLine($"CREATE OR TRIGGER {script.FullName}");
            sb.AppendLine($"{script.Time} {events} ON {script.TableName}");
            sb.AppendLine($"FOR EACH ROW");

            if (script.Condition != null)
            {
                sb.AppendLine($"WHEN ({script.Condition})");
            }

            foreach (Statement statement in script.Statements.Where(item => item is DeclareStatement))
            {
                sb.AppendLine(this.BuildStatement(statement));
            }

            sb.AppendLine("BEGIN");

            foreach (Statement statement in script.Statements.Where(item => !(item is DeclareStatement)))
            {
                sb.AppendLine(this.BuildStatement(statement));
            }

            sb.AppendLine("END;");

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
                if (select.TableName == null)
                {
                    select.TableName = new TokenInfo("DUAL");
                }

                if (select.TableName == null && select.Columns.Count == 1 && select.Columns[0].Symbol.Contains("="))
                {
                    appendLine($"SET {select.Columns.First()}");
                }
                else
                {
                    appendLine($"SELECT {string.Join("," + Environment.NewLine + indent, select.Columns.Select(item => item.ToString()))}");
                }                

                if (select.IntoTableName != null)
                {
                    appendLine($"INTO {select.IntoTableName.ToString()}");
                }

                if (select.TableName != null)
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

                if (select.Condition != null)
                {
                    appendLine($"WHERE {select.Condition}");
                }

                if (select.OrderBy != null)
                {
                    appendLine(select.OrderBy.ToString());
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
                    int i = 0;

                    foreach (UpdateFromItem fromItem in update.FromItems)
                    {
                        if (i == 0 && fromItem.TableName != null)
                        {
                            tableNames.Add(fromItem.TableName);
                        }
                        StringBuilder from = new StringBuilder();

                        foreach (TokenInfo join in fromItem.Joins)
                        {
                            from.AppendLine(join.ToString());
                        }

                        string strFrom = from.ToString();

                        foreach (NameValueItem nameValue in update.SetItems)
                        {
                            string[] ids = nameValue.Value.Symbol.Split('.').Select(item => item.Trim()).ToArray();

                            string tableName = ids[0];
                            string columnName = ids[1];

                            if (strFrom.Split(' ', '.').Any(item => item == tableName))
                            {
                                int joinIndex = strFrom.IndexOf("JOIN");

                                nameValue.Value.Symbol = $"(SELECT {nameValue.Value} FROM {strFrom.Substring(joinIndex + 5).Trim()})";

                                nameValue.Value.Symbol = Regex.Replace(nameValue.Value.Symbol, " ON ", " WHERE ", RegexOptions.IgnoreCase);
                            }
                        }

                        i++;
                    }
                }


                if (tableNames.Count == 0 && update.TableNames.Count > 0)
                {
                    tableNames.AddRange(update.TableNames);
                }

                append($" {string.Join(",", tableNames)}");

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
                appendLine($"DELETE {delete.TableName}");

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
                    string defaultValue = (declare.DefaultValue == null ? "" : $":={declare.DefaultValue}");
                    appendLine($"DECLARE {declare.Name} {declare.DataType} {defaultValue};");
                }
                else if (declare.Type == DeclareType.Table)
                {

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
                    appendLine($"{set.Key } := {set.Value };");
                }
            }
            else if (statement is LoopStatement loop)
            {
                if (loop.Type == LoopType.LOOP)
                {
                    appendLine("LOOP");
                }
                else
                {
                    appendLine($"{loop.Type.ToString()} {loop.Condition} LOOP");
                }

                appendStatements(loop.Statements, true);
                appendLine("END LOOP;");
            }
            else if (statement is WhileStatement @while)
            {
                LoopStatement loopStatement = @while as LoopStatement;

                appendLine($"WHILE {@while.Condition} LOOP");
                appendStatements(@while.Statements, true);
                appendLine("END LOOP;");
            }
            else if (statement is ReturnStatement @return)
            {
                appendLine($"RETURN {@return.Value};");
            }
            else if (statement is PrintStatement print)
            {
                appendLine($"DBMS_OUTPUT.PUT_LINE({print.Content.Symbol?.ToString()?.Replace("+", "||")});");
            }
            else if (statement is FunctionCallStatement funcCall)
            {
                appendLine($"{funcCall.Name}({string.Join(",", funcCall.Arguments)});");
            }
            else if (statement is ProcedureCallStatement execute)
            {
                appendLine($"EXECUTE {execute.Content};");
            }
            else if (statement is TransactionStatement transaction)
            {
                TransactionCommandType commandType = transaction.CommandType;

                switch (commandType)
                {
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
                appendLine("RETURN;");
            }
            else if (statement is TryCatchStatement tryCatch)
            {
                appendLine("EXCEPTION");
                appendLine("BEGIN");

                appendStatements(tryCatch.CatchStatements, true);

                appendLine("END;");

                appendStatements(tryCatch.TryStatements, true);
            }
            else if (statement is ExceptionStatement exception)
            {
                appendLine("EXCEPTION");

                foreach (ExceptionItem exceptionItem in exception.Items)
                {
                    appendLine($"WHEN {exceptionItem.Name} THEN");
                    appendLine("BEGIN");

                    appendStatements(exceptionItem.Statements, true);

                    appendLine("END;");
                }
            }
            else if (statement is DeclareCursorStatement declareCursor)
            {
                appendLine($"DECLARE CURSOR {declareCursor.CursorName} IS");
                append(this.BuildStatement(declareCursor.SelectStatement));
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

            return sb.ToString();
        }
    }
}
