using DatabaseInterpreter.Model;
using SqlAnalyser.Core.Model;
using SqlAnalyser.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class MySqlScriptBuildFactory:ScriptBuildFactory
    {
        public override DatabaseType DatabaseType => DatabaseType.MySql;

        protected override void PreHandleStatements(List<Statement> statements)
        {
            this.PreHandle(statements);
        }

        protected override void PostHandleStatements(StringBuilder sb)
        {
            base.PostHandleStatements(sb);

            this.PostHandle(sb);
        }

        private void PreHandle(List<Statement> statements)
        {
            this.StatementBuilder.Option.CollectDeclareStatement = true;
            this.StatementBuilder.Option.NotBuildDeclareStatement = true;
            this.StatementBuilder.Option.CollectDeclareStatement = true;

            base.PreHandleStatements(statements);

            MySqlAnalyserHelper.RearrangeStatements(statements);
        }

        private void PostHandle(StringBuilder sb, RoutineType routineType = RoutineType.UNKNOWN, ScriptBuildResult result = null, int? beginIndex = default(int?))
        {
            var declareStatements = this.StatementBuilder.GetDeclareStatements();

            if (declareStatements.Count > 0)
            {
                this.StatementBuilder.Clear();

                StringBuilder sbDeclare = new StringBuilder();

                bool hasDeclareCursor = false;
                bool hasDeclareCursorHanlder = false;

                foreach (var declareStatement in declareStatements)
                {
                    if (declareStatement is DeclareCursorStatement)
                    {
                        hasDeclareCursor = true;
                    }

                    if (declareStatement is DeclareCursorHandlerStatement)
                    {
                        hasDeclareCursorHanlder = true;
                    }
                }

                if (hasDeclareCursor && !hasDeclareCursorHanlder)
                {
                    DeclareVariableStatement declareVaribleStatement = new DeclareVariableStatement()
                    {
                        Name = new TokenInfo("FINISHED") { Type = TokenType.VariableName },
                        DataType = new TokenInfo("INT"),
                        DefaultValue = new TokenInfo("0")
                    };

                    this.StatementBuilder.DeclareVariableStatements.Insert(0, declareVaribleStatement);

                    DeclareCursorHandlerStatement declareHandlerStatement = new DeclareCursorHandlerStatement();
                    declareHandlerStatement.Statements.Add(new SetStatement() { Key = new TokenInfo("FINISHED") { Type = TokenType.VariableName }, Value = new TokenInfo("1") });

                    this.StatementBuilder.OtherDeclareStatements.Add(declareHandlerStatement);
                }

                foreach (var declareStatement in declareStatements)
                {
                    this.StatementBuilder.Option.NotBuildDeclareStatement = false;
                    this.StatementBuilder.Option.CollectDeclareStatement = false;

                    string content = this.BuildStatement(declareStatement, routineType).TrimEnd();

                    sbDeclare.AppendLine(content);
                }

                sb.Insert(result == null ? 0 : result.BodyStartIndex, sbDeclare.ToString());

                if (result != null)
                {
                    result.BodyStartIndex += sbDeclare.Length;
                    result.BodyStopIndex += sbDeclare.Length;
                }                
            }

            if (this.StatementBuilder.SpecialStatements.Any(item => item.GetType() == typeof(LeaveStatement)))
            {
                this.StatementBuilder.Option.CollectSpecialStatementTypes.Clear();
                this.StatementBuilder.SpecialStatements.Clear();

                if (beginIndex.HasValue)
                {
                    sb.Insert(beginIndex.Value, "sp:");
                }

                if (result != null)
                {
                    result.BodyStartIndex += 3;
                    result.BodyStopIndex += 3;
                }
            }
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
                string returnType = script.ReturnDataType != null ? script.ReturnDataType.Symbol
                                   : (script.ReturnTable != null ? $"#Table#{script.ReturnTable.Name}" : "");

                sb.AppendLine($"RETURNS {returnType}");
            }

            int beginIndex = sb.Length - 1;

            sb.AppendLine("BEGIN");

            result.BodyStartIndex = sb.Length;

            if (script.ReturnTable != null)
            {
                sb.AppendLine((this.StatementBuilder as MySqlStatementScriptBuilder).BuildTable(script.ReturnTable));
            }

            this.StatementBuilder.Option.CollectSpecialStatementTypes.Add(typeof(LeaveStatement));

            this.PreHandle(script.Statements);

            foreach (Statement statement in script.Statements)
            {
                sb.AppendLine(this.BuildStatement(statement, script.Type));
            }

            result.BodyStopIndex = sb.Length - 1;

            sb.AppendLine("END");

            this.PostHandle(sb, script.Type, result, beginIndex);

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
