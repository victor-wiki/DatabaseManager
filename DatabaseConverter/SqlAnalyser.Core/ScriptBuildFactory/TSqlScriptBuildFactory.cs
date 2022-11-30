using DatabaseInterpreter.Model;
using SqlAnalyser.Core.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class TSqlScriptBuildFactory:ScriptBuildFactory
    {
        public override DatabaseType DatabaseType => DatabaseType.SqlServer;

        public override ScriptBuildResult GenerateRoutineScripts(RoutineScript script)
        {
            ScriptBuildResult result = new ScriptBuildResult();

            this.StatementBuilder.Option.CollectSpecialStatementTypes.Add(typeof(PreparedStatement));

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CREATE {script.Type.ToString()} {script.NameWithSchema}");

            if (script.Parameters.Count > 0)
            {
                sb.AppendLine("(");

                int i = 0;
                foreach (Parameter parameter in script.Parameters)
                {
                    ParameterType parameterType = parameter.ParameterType;

                    string strParameterType = "";

                    if (parameterType == ParameterType.IN)
                    {
                        strParameterType = "";
                    }
                    else if (parameterType.HasFlag(ParameterType.IN) && parameterType.HasFlag(ParameterType.OUT))
                    {
                        strParameterType = "OUT";
                    }
                    else if (parameterType != ParameterType.NONE)
                    {
                        strParameterType = parameterType.ToString();
                    }

                    string defaultValue = parameter.DefaultValue == null ? "" : "=" + parameter.DefaultValue;

                    sb.AppendLine($"{parameter.Name} {parameter.DataType} {defaultValue} {strParameterType}{(i == script.Parameters.Count - 1 ? "" : ",")}");

                    i++;
                }

                sb.AppendLine(")");
            }
            else if (script.Type == RoutineType.FUNCTION)
            {
                sb.AppendLine("(");
                sb.AppendLine(")");
            }

            if (script.Type == RoutineType.FUNCTION)
            {
                if (script.ReturnTable == null)
                {
                    sb.AppendLine($"RETURNS {script.ReturnDataType}");
                }
                else
                {
                    sb.AppendLine($"RETURNS {script.ReturnTable.Name}({string.Join(",", script.ReturnTable.Columns.Select(t => $"{t.Name.Symbol} {t.DataType}"))})");
                }
            }

            sb.AppendLine("AS");

            sb.AppendLine("BEGIN");

            result.BodyStartIndex = sb.Length;

            Action<IEnumerable<Statement>> appendStatements = (statements) =>
            {
                foreach (Statement statement in statements)
                {
                    if (statement is WhileStatement @while)
                    {
                        FetchCursorStatement fetchCursorStatement = @while.Statements.FirstOrDefault(item => item is FetchCursorStatement) as FetchCursorStatement;

                        if (fetchCursorStatement != null && !statements.Any(item => item is FetchCursorStatement))
                        {
                            string condition = @while.Condition?.Symbol;

                            if (condition == null)
                            {
                                @while.Condition = new TokenInfo("");
                            }

                            @while.Condition.Symbol = "@@FETCH_STATUS = 0";

                            if (condition != null)
                            {
                                @while.Condition.Symbol += " AND " + condition;
                            }

                            sb.AppendLine(this.BuildStatement(fetchCursorStatement));
                        }
                    }

                    sb.AppendLine(this.BuildStatement(statement));
                }
            };

            ExceptionStatement exceptionStatement = (ExceptionStatement)script.Statements.FirstOrDefault(item => item is ExceptionStatement);

            if (exceptionStatement != null)
            {
                sb.AppendLine("BEGIN TRY");
                appendStatements(script.Statements.Where(item => !(item is ExceptionStatement)));
                sb.AppendLine("END TRY");

                sb.AppendLine("BEGIN CATCH");

                foreach (ExceptionItem exceptionItem in exceptionStatement.Items)
                {
                    sb.AppendLine($"IF {exceptionItem.Name} = ERROR_PROCEDURE() OR {exceptionItem.Name} = ERROR_NUMBER()");
                    sb.AppendLine("BEGIN");

                    appendStatements(exceptionItem.Statements);

                    sb.AppendLine("END");
                }

                sb.AppendLine("END CATCH");
            }
            else
            {
                appendStatements(script.Statements);
            }

            result.BodyStopIndex = sb.Length - 1;

            sb.AppendLine("END");

            result.Script = sb.ToString();

            return result;
        }

        public override ScriptBuildResult GenearteViewScripts(ViewScript script)
        {
            ScriptBuildResult result = new ScriptBuildResult();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CREATE VIEW {script.NameWithSchema} AS");

            result.BodyStartIndex = sb.Length;

            foreach (Statement statement in script.Statements)
            {
                sb.AppendLine(this.BuildStatement(statement));
            }

            result.BodyStopIndex = sb.Length - 1;

            result.Script = sb.ToString();

            return result;
        }

        public override ScriptBuildResult GenearteTriggerScripts(TriggerScript script)
        {
            ScriptBuildResult result = new ScriptBuildResult();

            StringBuilder sb = new StringBuilder();

            string time = (script.Time == TriggerTime.BEFORE || script.Time == TriggerTime.INSTEAD_OF) ? "INSTEAD OF" : script.Time.ToString();
            string events = string.Join(",", script.Events);

            sb.AppendLine($"CREATE TRIGGER {script.NameWithSchema} ON {script.TableName}");
            sb.AppendLine($"{time} {events} NOT FOR REPLICATION ");

            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");

            result.BodyStartIndex = sb.Length;

            foreach (Statement statement in script.Statements)
            {
                sb.Append(this.BuildStatement(statement));
            }

            result.BodyStopIndex = sb.Length - 1;

            sb.AppendLine("END");

            result.Script = sb.ToString();

            return result;
        }
    }
}
