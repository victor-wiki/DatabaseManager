using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

            if (script.ReturnTable != null)
            {
                sb.AppendLine(this.BuildTemporaryTable(script.ReturnTable));
            }

            foreach (Statement statement in script.Statements.Where(item => !(item is DeclareStatement)))
            {
                string result = this.BuildStatement(statement);

                if (script.Type == RoutineType.PROCEDURE)
                {
                    if (result.Split('\r', '\n').Any(item => item.Trim(' ', ';').ToUpper() == "RETURN"))
                    {
                        Regex regex = new Regex(@"(RETURN[\s]+.*[\r\n;])", RegexOptions.IgnoreCase);

                        result = regex.Replace(result, "LEAVE sp;" + Environment.NewLine);

                        hasLeaveStatement = true;
                    }
                }

                sb.AppendLine(result);
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

            sb.AppendLine($"CREATE TRIGGER {script.FullName} {script.Time} {script.Event} ON {script.TableName}");
            sb.AppendLine($"FOR EACH ROW {script.Behavior} {script.OtherTriggerName}");

            foreach (Statement statement in script.Statements)
            {
                sb.AppendLine(this.BuildStatement(statement));
            }

            return sb.ToString();
        }

        private string BuildStatement(Statement statement, int level = 0, bool appendSeparator = true)
        {
            StringBuilder sb = new StringBuilder();

            string indent = " ".PadLeft((level + 1) * 2);

            Action<string> append = (value) => { sb.Append($"{indent}{value}"); };

            Action<string> appendLine = (value) => { append(value + Environment.NewLine); };

            if (statement is SelectStatement select)
            {
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
                    appendLine($"FROM {select.TableName}");
                }

                if (select.Condition != null)
                {
                    appendLine($"WHERE {select.Condition}");
                }

                if (select.UnionStatements != null)
                {
                    foreach (var union in select.UnionStatements)
                    {
                        appendLine("UNION");
                        appendLine(this.BuildStatement(union, level, false).TrimEnd(';'));
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
                    foreach (SelectStatement st in insert.SelectStatements)
                    {
                        appendLine(this.BuildStatement(st, level));
                    }
                }
                else
                {
                    appendLine($"VALUES({string.Join(",", insert.Values.Select(item => item))});");
                }
            }
            else if (statement is UpdateStatement update)
            {
                appendLine($"UPDATE {update.TableName} SET");

                appendLine(string.Join("," + Environment.NewLine + indent, update.Items.Select(item => $"{item.Name}={item.Value}")));

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
                    appendLine($"DECLARE {declare.Name} {declare.DataType};");
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
                    foreach (Statement st in item.Statements)
                    {
                        append(this.BuildStatement(st, level + 1));
                    }
                    appendLine("END;");
                }
                appendLine("END IF;");
            }
            else if (statement is SetStatement set)
            {
                if (set.Key != null && set.Value != null)
                {
                    appendLine($"SET {set.Key } = {set.Value };");
                }
            }
            else if (statement is WhileStatement @while)
            {
                appendLine($"WHILE {@while.Condition} DO");

                foreach (Statement st in @while.Statements)
                {
                    append(this.BuildStatement(st, level + 1));
                }

                appendLine("END WHILE;");
            }
            else if (statement is ReturnStatement @return)
            {
                appendLine($"RETURN {@return.Value};");
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
