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

                    sb.AppendLine($"{strParameterType} {parameter.Name} {parameter.DataType}{(i == script.Parameters.Count - 1 ? "" : ",")}");

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
                sb.AppendLine(MySqlStatementScriptBuilder.BuildTemporaryTable(script.ReturnTable));
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

            string time = script.Time == TriggerTime.INSTEAD_OF ? "AFTER" : script.Time.ToString();

            sb.AppendLine($"CREATE TRIGGER {script.FullName} {time} {events} ON {script.TableName}");
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
    }
}
