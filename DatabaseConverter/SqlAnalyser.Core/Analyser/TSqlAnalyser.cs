using DatabaseInterpreter.Model;
using SqlAnalyser.Core.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class TSqlAnalyser : SqlAnalyserBase
    {
        private TSqlRuleAnalyser ruleAnalyser = null;

        public override DatabaseType DatabaseType => DatabaseType.SqlServer;

        public TSqlAnalyser()
        {
            this.ruleAnalyser = new TSqlRuleAnalyser();
        }

        public override AnalyseResult AnalyseView(string content)
        {
            return this.ruleAnalyser.AnalyseView(content);

        }

        public override AnalyseResult AnalyseProcedure(string content)
        {
            return this.ruleAnalyser.AnalyseProcedure(content);
        }

        public override AnalyseResult AnalyseFunction(string content)
        {
            return this.ruleAnalyser.AnalyseFunction(content);
        }

        public override AnalyseResult AnalyseTrigger(string content)
        {
            return this.ruleAnalyser.AnalyseTrigger(content);
        }

        public override ScriptBuildResult GenerateScripts(CommonScript script)
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

        public ScriptBuildResult GenerateRoutineScripts(RoutineScript script)
        {
            ScriptBuildResult result = new ScriptBuildResult();

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
                    sb.AppendLine($"RETURNS {script.ReturnTable.Name}({string.Join(",", script.ReturnTable.Columns.Select(t => $"{t.Symbol} {t.DataType}")) })");
                }
            }

            sb.AppendLine("AS");

            sb.AppendLine("BEGIN");

            StringBuilder sbBody = new StringBuilder();

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

                            sbBody.AppendLine(this.BuildStatement(fetchCursorStatement));
                        }
                    }

                    sbBody.AppendLine(this.BuildStatement(statement));
                }
            };

            ExceptionStatement exceptionStatement = (ExceptionStatement)script.Statements.FirstOrDefault(item => item is ExceptionStatement);

            if (exceptionStatement != null)
            {
                sbBody.AppendLine("BEGIN TRY");
                appendStatements(script.Statements.Where(item => !(item is ExceptionStatement)));
                sbBody.AppendLine("END TRY");

                sbBody.AppendLine("BEGIN CATCH");

                foreach (ExceptionItem exceptionItem in exceptionStatement.Items)
                {
                    sbBody.AppendLine($"IF {exceptionItem.Name} = ERROR_PROCEDURE() OR {exceptionItem.Name} = ERROR_NUMBER()");
                    sbBody.AppendLine("BEGIN");

                    appendStatements(exceptionItem.Statements);

                    sbBody.AppendLine("END");
                }

                sbBody.AppendLine("END CATCH");
            }
            else
            {
                appendStatements(script.Statements);
            }

            result.Body = sbBody.ToString();

            sb.Append(sbBody);

            sb.AppendLine("END");

            result.Script = sb.ToString();

            return result;
        }

        public ScriptBuildResult GenearteViewScripts(ViewScript script)
        {
            ScriptBuildResult result = new ScriptBuildResult();

            StringBuilder sb = new StringBuilder();
            StringBuilder sbBody = new StringBuilder();

            sb.AppendLine($"CREATE VIEW {script.NameWithSchema} AS");

            foreach (Statement statement in script.Statements)
            {
                sbBody.AppendLine(this.BuildStatement(statement));
            }

            sb.Append(sbBody);

            result.Body = sbBody.ToString();
            result.Script = sb.ToString();

            return result;
        }

        public ScriptBuildResult GenearteTriggerScripts(TriggerScript script)
        {
            ScriptBuildResult result = new ScriptBuildResult();

            StringBuilder sb = new StringBuilder();
            StringBuilder sbBody = new StringBuilder();

            string time = (script.Time == TriggerTime.BEFORE || script.Time == TriggerTime.INSTEAD_OF) ? "INSTEAD OF" : script.Time.ToString();
            string events = string.Join(",", script.Events);

            sb.AppendLine($"CREATE TRIGGER {script.NameWithSchema} ON {script.TableName}");
            sb.AppendLine($"{time} {events} NOT FOR REPLICATION ");

            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");

            foreach (Statement statement in script.Statements)
            {
                sbBody.Append(this.BuildStatement(statement));
            }

            sb.Append(sbBody);

            sb.AppendLine("END");            

            result.Body = sbBody.ToString();
            result.Script = sb.ToString();

            return result;
        }
    }
}
