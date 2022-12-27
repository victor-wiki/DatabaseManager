using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using SqlAnalyser.Core.Model;
using SqlAnalyser.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class PlSqlScriptBuildFactory : ScriptBuildFactory
    {
        public override DatabaseType DatabaseType => DatabaseType.Oracle;

        protected override void PreHandleStatements(List<Statement> statements)
        {
            base.PreHandleStatements(statements);

            this.PreHandle(statements);
        }

        protected override void PostHandleStatements(StringBuilder sb)
        {
            base.PostHandleStatements(sb);

            this.PostHandle(sb);
        }

        private void PreHandle(List<Statement> statements)
        {
            this.StatementBuilder.Option.NotBuildDeclareStatement = true;
            this.StatementBuilder.Option.CollectDeclareStatement = true;
            this.StatementBuilder.Option.CollectSpecialStatementTypes.Add(typeof(PreparedStatement));
        }

        private void PostHandle(StringBuilder sb, RoutineType routineType = RoutineType.UNKNOWN, ScriptBuildResult result = null, int? declareStartIndex = default(int?))
        {
            var declareStatements = this.StatementBuilder.GetDeclareStatements();

            if (declareStatements.Count > 0)
            {
                this.StatementBuilder.Clear();

                StringBuilder sbDeclare = new StringBuilder();

                foreach (var declareStatement in declareStatements)
                {
                    this.StatementBuilder.Option.NotBuildDeclareStatement = false;
                    this.StatementBuilder.Option.CollectDeclareStatement = false;

                    string content = this.BuildStatement(declareStatement, routineType).Trim();

                    sbDeclare.AppendLine(content.Replace("DECLARE ", ""));
                }

                sb.Insert(declareStartIndex.HasValue ? declareStartIndex.Value : 0, sbDeclare.ToString());

                if (result != null)
                {
                    result.BodyStartIndex += sbDeclare.Length;
                    result.BodyStopIndex += sbDeclare.Length;
                }
            }
        }

        public override ScriptBuildResult GenerateRoutineScripts(RoutineScript script)
        {
            ScriptBuildResult result = new ScriptBuildResult();

            this.PreHandle(script.Statements);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CREATE OR REPLACE {script.Type.ToString()} {script.Name}");

            if (script.Parameters.Count > 0)
            {
                sb.AppendLine("(");

                int i = 0;

                foreach (Parameter parameter in script.Parameters)
                {
                    ParameterType parameterType = parameter.ParameterType;

                    string dataType = parameter.DataType.Symbol;
                    string defaultValue = parameter.DefaultValue == null ? "" : $" DEFAULT {parameter.DefaultValue}";
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

                    sb.AppendLine($"{parameter.Name} {strParameterType} {dataType}{defaultValue}{(i == script.Parameters.Count - 1 ? "" : ",")}");

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

            int declareStartIndex = sb.Length - 1;

            sb.AppendLine("BEGIN");

            if (script.ReturnTable != null)
            {

            }

            result.BodyStartIndex = sb.Length;

            foreach (Statement statement in script.Statements)
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

                sb.AppendLine(this.BuildStatement(statement, script.Type));
            }

            result.BodyStopIndex = sb.Length - 1;

            sb.AppendLine($"END {script.Name};");

            this.PostHandle(sb, script.Type, result, declareStartIndex);

            result.Script = sb.ToString();

            return result;
        }

        public override ScriptBuildResult GenearteViewScripts(ViewScript script)
        {
            ScriptBuildResult result = new ScriptBuildResult();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CREATE OR REPLACE VIEW {script.NameWithSchema} AS");

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

            string events = string.Join(" OR ", script.Events);

            sb.AppendLine($"CREATE OR REPLACE TRIGGER {script.NameWithSchema}");
            sb.AppendLine($"{script.Time} {events} ON {script.TableName}");
            sb.AppendLine($"FOR EACH ROW");

            if (script.Condition != null)
            {
                sb.AppendLine($"WHEN ({script.Condition})");
            }

            foreach (Statement statement in script.Statements.Where(item => item is DeclareVariableStatement))
            {
                sb.AppendLine(this.BuildStatement(statement));
            }

            sb.AppendLine("BEGIN");

            result.BodyStartIndex = sb.Length;

            foreach (Statement statement in script.Statements.Where(item => !(item is DeclareVariableStatement)))
            {
                sb.AppendLine(this.BuildStatement(statement));
            }

            result.BodyStopIndex = sb.Length - 1;

            sb.AppendLine("END;");

            result.Script = sb.ToString();

            return result;
        }
    }
}
