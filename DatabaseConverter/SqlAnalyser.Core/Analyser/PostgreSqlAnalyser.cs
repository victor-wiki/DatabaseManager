using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
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
        public override SqlRuleAnalyser RuleAnalyser => this.ruleAnalyser;

        public PostgreSqlAnalyser()
        {
            this.ruleAnalyser = new PostgreSqlRuleAnalyser();
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

            this.StatementBuilder.Option.NotBuildDeclareStatement = true;
            this.StatementBuilder.Option.CollectDeclareStatement = true ;

            StringBuilder sb = new StringBuilder();

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
                else if (script.ReturnTable != null)
                {
                    if (script.ReturnTable.Columns.Count > 0)
                    {
                        sb.AppendLine($"RETURNS TABLE({string.Join(",", script.ReturnTable.Columns.Select(item => this.GetColumnInfo(item)))})");
                    }
                }
                else
                {
                    if (script.Statements.Count > 0 && script.Statements.First() is SelectStatement select)
                    {
                        sb.AppendLine($"RETURNS TABLE({string.Join(",", select.Columns.Select(item => $"{item.FieldName} character varying"))})");
                    }
                }
            }

            sb.AppendLine("LANGUAGE 'plpgsql'");
            sb.AppendLine("AS");
            sb.AppendLine("$$");

            int declareStartIndex = sb.Length;

            sb.AppendLine("BEGIN");

            result.BodyStartIndex = sb.Length;

            if (script.Type == RoutineType.FUNCTION)
            {
                if (script.ReturnDataType == null)
                {
                    sb.Append("RETURN QUERY ");
                }
            }

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
                        @while.Condition.Symbol = "1=1";

                        if (fs.Variables.Count == 0)
                        {
                            @while.Statements.Insert(0, new LoopExitStatement() { Condition = new TokenInfo("EXIT WHEN NOT FOUND;") });
                            @while.Statements.Insert(0, fetchCursorStatement);
                        }
                    }
                }

                sb.AppendLine(this.BuildStatement(statement, script.Type));
            }

            result.BodyStopIndex = sb.Length - 1;

            sb.AppendLine($"END");
            sb.AppendLine("$$;");

            if (this.StatementBuilder.DeclareStatements.Count > 0)
            {
                this.StatementBuilder.Clear();

                StringBuilder sbDeclare = new StringBuilder();

                foreach(var declareStatement in this.StatementBuilder.DeclareStatements)
                {
                    this.StatementBuilder.Option.NotBuildDeclareStatement = false;
                    this.StatementBuilder.Option.CollectDeclareStatement = false;

                    sbDeclare.AppendLine(this.BuildStatement(declareStatement, script.Type).Trim());
                }

                sb.Insert(declareStartIndex, sbDeclare.ToString());

                result.BodyStartIndex += sbDeclare.Length;
                result.BodyStopIndex += sbDeclare.Length;

                this.StatementBuilder.DeclareStatements.Clear();
            }

            result.Script = sb.ToString();

            return result;
        }

        private string GetColumnInfo(ColumnInfo columnInfo)
        {
            string name = columnInfo.Name.FieldName;
            string dataType = string.IsNullOrEmpty(columnInfo.DataType?.Symbol) ? "character varying" : columnInfo.DataType.Symbol;

            return $"{name} {dataType}";
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

            sb.AppendLine($"CREATE OR REPLACE TRIGGER {script.Name}");
            sb.AppendLine($"{script.Time} {events} ON {script.TableName.NameWithSchema}");
            sb.AppendLine("FOR EACH ROW");

            result.BodyStartIndex = sb.Length;

            sb.AppendLine($"EXECUTE PROCEDURE {script.FunctionName?.NameWithSchema}();");

            result.BodyStopIndex = sb.Length - 1;

            result.Script = sb.ToString();

            return result;
        }
    }
}
