using DatabaseInterpreter.Model;
using SqlAnalyser.Core.Model;
using SqlAnalyser.Model;
using System;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class MySqlAnalyser : SqlAnalyserBase
    {
        private MySqlRuleAnalyser ruleAnalyser = null;

        public override DatabaseType DatabaseType => DatabaseType.MySql;
        public override SqlRuleAnalyser RuleAnalyser => this.ruleAnalyser;

        public MySqlAnalyser()
        {
            this.ruleAnalyser = new MySqlRuleAnalyser();
        }

        public override AnalyseResult AnalyseCommon(string content)
        {
            return this.ruleAnalyser.AnalyseCommon(content);
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

        public override ScriptBuildResult GenerateRoutineScripts(RoutineScript script)
        {
            ScriptBuildResult result = new ScriptBuildResult();          

            StringBuilder sb = new StringBuilder();          

            sb.AppendLine($"CREATE {script.Type.ToString()} {script.NameWithSchema}");

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

            sb.AppendLine("BEGIN");

            result.BodyStartIndex = sb.Length;          

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
                if (!script.Statements.Any(item => item is DeclareVariableStatement && (item as DeclareVariableStatement).Name.Symbol == "FINISHED"))
                {
                    DeclareVariableStatement declareStatement = new DeclareVariableStatement()
                    {
                        Name = new TokenInfo("FINISHED") { Type = TokenType.VariableName },
                        DataType = new TokenInfo("INT"),
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
                sb.AppendLine((this.StatementBuilder as MySqlStatementScriptBuilder).BuildTable(script.ReturnTable));
            }

            this.StatementBuilder.Option.NotBuildDeclareStatement = true;
            this.StatementBuilder.Option.CollectDeclareStatement = true;
            this.StatementBuilder.Option.CollectSpecialStatementTypes.Add(typeof(LeaveStatement));

            FetchCursorStatement fetchCursorStatement = null;

            foreach (Statement statement in script.Statements)
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
          
                sb.AppendLine(this.BuildStatement(statement, script.Type));
            }

            result.BodyStopIndex = sb.Length - 1;

            sb.AppendLine("END");            

            if (this.StatementBuilder.DeclareStatements.Count > 0)
            {
                this.StatementBuilder.Clear();

                StringBuilder sbDeclare = new StringBuilder();

                foreach (var declareStatement in this.StatementBuilder.DeclareStatements)
                {
                    this.StatementBuilder.Option.NotBuildDeclareStatement = false;
                    this.StatementBuilder.Option.CollectDeclareStatement = false;

                    string content = this.BuildStatement(declareStatement, script.Type).Trim();

                    sbDeclare.AppendLine(content);
                }

                sb.Insert(result.BodyStartIndex, sbDeclare.ToString());

                result.BodyStartIndex += sbDeclare.Length;
                result.BodyStopIndex += sbDeclare.Length;

                this.StatementBuilder.DeclareStatements.Clear();
            }

            if (this.StatementBuilder.SpecialStatements.Any(item=>item.GetType() == typeof(LeaveStatement)))
            {
                this.StatementBuilder.Option.CollectSpecialStatementTypes.Clear();
                this.StatementBuilder.SpecialStatements.Clear();

                sb.Insert(beginIndex, "sp:");

                result.BodyStartIndex += 3;
                result.BodyStopIndex += 3;
            }           

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

            //only allow one event type: INSERT, UPDATE OR DELETE
            var events = script.Events.FirstOrDefault(); // string.Join(",", script.Events);

            string time = script.Time == TriggerTime.INSTEAD_OF ? "AFTER" : script.Time.ToString();

            sb.AppendLine($"CREATE TRIGGER {script.NameWithSchema} {time} {events} ON {script.TableName}");
            sb.AppendLine($"FOR EACH ROW {script.Behavior} {script.OtherTriggerName}");

            int beginIndex = sb.Length - 1;
            bool hasLeaveStatement = false;

            sb.AppendLine("BEGIN");

            result.BodyStartIndex = sb.Length;

            foreach (Statement statement in script.Statements.Where(item => item is DeclareVariableStatement))
            {
                sb.AppendLine(this.BuildStatement(statement));
            }

            foreach (Statement statement in script.Statements.Where(item => !(item is DeclareVariableStatement)))
            {
                if (statement is LeaveStatement)
                {
                    hasLeaveStatement = true;
                }

                sb.AppendLine(this.BuildStatement(statement, RoutineType.TRIGGER));
            }

            result.BodyStopIndex = sb.Length - 1;

            sb.AppendLine("END");

            if (hasLeaveStatement)
            {
                sb.Insert(beginIndex, "sp:");
            }

            result.Script = sb.ToString();            

            return result;
        }
    }
}
