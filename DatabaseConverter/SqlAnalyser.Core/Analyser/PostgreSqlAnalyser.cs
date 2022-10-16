using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using SqlAnalyser.Core.Model;
using SqlAnalyser.Model;
using System;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class PostgreSqlAnalyser : SqlAnalyserBase
    {
        private PostgreSqlRuleAnalyser ruleAnalyser = null;

        public override DatabaseType DatabaseType => DatabaseType.Postgres;

        public PostgreSqlAnalyser()
        {
            this.ruleAnalyser = new PostgreSqlRuleAnalyser();
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
            StringBuilder sbBody = new StringBuilder();

            sb.Append($"CREATE OR REPLACE {script.Type.ToString()} {script.NameWithSchema}");

            if (script.Parameters.Count > 0)
            {
                sb.AppendLine();
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

                    sb.AppendLine($"{parameter.Name} {strParameterType} {dataType}{(i == script.Parameters.Count - 1 ? "" : ",")}");

                    i++;
                }

                sb.AppendLine(")");
            }
            else
            {
                sb.Append("()" + Environment.NewLine);
            }

            if (script.Type == RoutineType.FUNCTION)
            {
                if (script.ReturnDataType != null)
                {
                    string dataType = script.ReturnDataType.Symbol;

                    if (DataTypeHelper.IsCharType(dataType))
                    {
                        DataTypeInfo dataTypeInfo = DataTypeHelper.GetDataTypeInfo(dataType);
                        dataType = dataTypeInfo.DataType;
                    }

                    sb.AppendLine($"RETURNS {dataType}");
                }                
            }

            sb.AppendLine("LANGUAGE 'plpgsql'");
            sb.AppendLine("AS");
            sb.AppendLine("$$");

            foreach (Statement statement in script.Statements.Where(item => item is DeclareStatement || item is DeclareCursorStatement))
            {
                sb.Append(this.BuildStatement(statement));
            }

            sb.AppendLine("BEGIN");

            if (script.ReturnTable != null)
            {
                //
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
                        @while.Condition.Symbol = "1=1";

                        if (fs.Variables.Count == 0)
                        {
                            @while.Statements.Insert(0, new LoopExitStatement() { Condition = new TokenInfo("EXIT WHEN NOT FOUND;") });
                            @while.Statements.Insert(0, fetchCursorStatement);
                        }
                    }
                }

                sbBody.AppendLine(this.BuildStatement(statement));
            }

            sb.Append(sbBody);

            sb.AppendLine($"END");
            sb.AppendLine("$$;");

            result.Script = sb.ToString();
            result.Body = sbBody.ToString();

            return result;
        }

        public ScriptBuildResult GenearteViewScripts(ViewScript script)
        {
            ScriptBuildResult result = new ScriptBuildResult();

            StringBuilder sb = new StringBuilder();
            StringBuilder sbBody = new StringBuilder();

            sb.AppendLine($"CREATE OR REPLACE VIEW {script.NameWithSchema} AS");

            foreach (Statement statement in script.Statements)
            {
                sbBody.AppendLine(this.BuildStatement(statement));
            }

            sb.Append(sbBody);

            result.Script = sb.ToString();
            result.Body = sbBody.ToString();

            return result;
        }

        public ScriptBuildResult GenearteTriggerScripts(TriggerScript script)
        {
            ScriptBuildResult result = new ScriptBuildResult();

            StringBuilder sb = new StringBuilder();
            StringBuilder sbBody = new StringBuilder();

            string events = string.Join(" OR ", script.Events);

            sb.AppendLine($"CREATE OR REPLACE TRIGGER {script.Name}");
            sb.AppendLine($"{script.Time} {events} ON {script.TableName.NameWithSchema}");
            sb.AppendLine("FOR EACH ROW");

            sbBody.AppendLine($"EXECUTE PROCEDURE {script.FunctionName?.NameWithSchema}();");

            sb.Append(sbBody);

            result.Script = sb.ToString();
            result.Body = sbBody.ToString();

            return result;
        }
    }
}
