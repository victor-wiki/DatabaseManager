using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using SqlAnalyser.Core.Model;
using SqlAnalyser.Model;
using System;
using System.Linq;
using System.Text;

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

            sb.AppendLine($"CREATE OR REPLACE {script.Type.ToString()} {script.NameWithSchema}");

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

                    sb.AppendLine($"{parameter.Name} {strParameterType} {dataType}{(i == script.Parameters.Count - 1 ? "" : ",")}");

                    i++;
                }

                sb.AppendLine(")");
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

                    sb.AppendLine($"RETURN {dataType}");
                }
                else if (script.ReturnTable != null)
                {
                    //sb.AppendLine($"RETURN {script.ReturnTable}");
                }
            }

            sb.AppendLine("AS");

            foreach (Statement statement in script.Statements.Where(item => item is DeclareStatement || item is DeclareCursorStatement))
            {
                sb.Append(this.BuildStatement(statement).Replace("DECLARE ", ""));
            }

            sb.AppendLine("BEGIN");

            if (script.ReturnTable != null)
            {

            }

            foreach (Statement statement in script.Statements.Where(item => !(item is DeclareStatement || item is DeclareCursorStatement)))
            {
                if (statement is WhileStatement @while)
                {
                    FetchCursorStatement fs = @while.Statements.FirstOrDefault(item => item is FetchCursorStatement) as FetchCursorStatement;

                    if (fs != null)
                    {
                        @while.Condition.Symbol = "1=1";

                        @while.Statements.Insert(0, new LoopExitStatement() { Condition = new TokenInfo($"{fs.CursorName}%NOTFOUND") });
                    }
                }

                sbBody.AppendLine(this.BuildStatement(statement));
            }

            sb.Append(sbBody);

            sb.AppendLine($"END {script.NameWithSchema};");

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

            sb.AppendLine($"CREATE OR REPLACE TRIGGER {script.NameWithSchema}");
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
                sbBody.AppendLine(this.BuildStatement(statement));
            }

            sb.Append(sbBody);

            sb.AppendLine("END;");

            result.Script = sb.ToString();
            result.Body = sb.ToString();

            return result;
        }
    }
}
