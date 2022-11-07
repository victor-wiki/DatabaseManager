using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using PoorMansTSqlFormatterRedux;
using SqlAnalyser.Core;
using SqlAnalyser.Core.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace DatabaseConverter.Core
{
    public class ScriptTranslator<T> : DbObjectTokenTranslator
        where T : ScriptDbObject
    {
        private IEnumerable<T> scripts;

      
        public ScriptTranslator(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, IEnumerable<T> scripts) : base(sourceDbInterpreter, targetDbInterpreter)
        {
            this.scripts = scripts;
        }

        public override void Translate()
        {
            if (this.sourceDbInterpreter.DatabaseType == this.targetDbInterpreter.DatabaseType)
            {
                return;
            }

            if (this.hasError)
            {
                return;
            }

            this.LoadMappings();

            SqlAnalyserBase sourceAnalyser = this.GetSqlAnalyser(this.sourceDbInterpreter.DatabaseType);
            SqlAnalyserBase targetAnalyser = this.GetSqlAnalyser(this.targetDbInterpreter.DatabaseType);

            Action<T, CommonScript> processTokens = (dbObj, script) =>
            {
                if (typeof(T) == typeof(Function))
                {
                    AnalyseResult result = sourceAnalyser.AnalyseFunction(dbObj.Definition.ToUpper());

                    if (!result.HasError)
                    {
                        RoutineScript routine = result.Script as RoutineScript;

                        if (this.targetDbInterpreter.DatabaseType == DatabaseType.MySql && routine.ReturnTable != null)
                        {
                            routine.Type = RoutineType.PROCEDURE;
                        }
                    }
                }

                using (ScriptTokenProcessor tokenProcessor = new ScriptTokenProcessor(script, dbObj, this.sourceDbInterpreter, this.targetDbInterpreter))
                {
                    tokenProcessor.UserDefinedTypes = this.UserDefinedTypes;
                    tokenProcessor.Option = this.Option;

                    tokenProcessor.Process();

                    string anotherDefinition = null;

                    if (typeof(T) == typeof(TableTrigger))
                    {
                        //make up a trigger function
                        if (this.targetDbType == DatabaseType.Postgres)
                        {
                            string name = this.targetDbInterpreter.GetQuotedString($"func_{dbObj.Name}");
                            string nameWithSchema = string.IsNullOrEmpty(script.Schema) ? this.targetDbInterpreter.GetQuotedString(name) : $"{script.Schema}.{name}";

                            NameToken triggerFunctionName = new NameToken(nameWithSchema);

                            RoutineScript rs = new RoutineScript() { Name = triggerFunctionName, Type = RoutineType.FUNCTION, ReturnDataType = new TokenInfo("trigger") };
                            rs.Statements.AddRange(script.Statements);

                            (script as TriggerScript).FunctionName = triggerFunctionName;

                            anotherDefinition = StringHelper.FormatScript(targetAnalyser.GenerateScripts(rs).Script);
                        }
                    }

                    ScriptBuildResult scriptBuildResult = targetAnalyser.GenerateScripts(script);

                    dbObj.Definition = StringHelper.FormatScript(scriptBuildResult.Script);

                    if (anotherDefinition != null)
                    {
                        dbObj.Definition = anotherDefinition + Environment.NewLine + dbObj.Definition;
                    }
                };
            };

            int total = this.scripts.Count();
            int count = 0;

            foreach (T dbObj in this.scripts)
            {
                if (this.hasError)
                {
                    break;
                }

                try
                {
                    Type type = typeof(T);

                    count++;

                    this.FeedbackInfo($"Begin to translate {type.Name} \"{dbObj.Name}\"({count}/{total}).");

                    bool tokenProcessed = false;
                    
                    dbObj.Definition = dbObj.Definition.Trim();

                    if(this.Option.RemoveCarriagRreturnChar)
                    {
                        dbObj.Definition = dbObj.Definition.Replace("\r\n", "\n");
                    }

                    this.Validate(dbObj);

                    if (sourceDbType == DatabaseType.MySql)
                    {
                        string dbName = this.sourceDbInterpreter.ConnectionInfo.Database;

                        if (dbName != null)
                        {
                            dbObj.Definition = dbObj.Definition.Replace($"`{dbName}`.", "").Replace($"{dbName}.", "");
                        }
                    }
                    else if (sourceDbType == DatabaseType.Postgres)
                    {
                        if (dbObj.Definition.Contains("::"))
                        {
                            var dataTypeSpecs = DataTypeManager.GetDataTypeSpecifications(this.sourceDbType);
                            dbObj.Definition = TranslateHelper.RemovePostgresDataTypeConvertExpression(dbObj.Definition, dataTypeSpecs, this.targetDbInterpreter.QuotationLeftChar, this.targetDbInterpreter.QuotationRightChar);
                        }
                    }

                    string originalDefinition = dbObj.Definition;

                    AnalyseResult result = sourceAnalyser.Analyse<T>(originalDefinition.ToUpper());

                    CommonScript script = result.Script;

                    bool replaced = false;

                    if (result.HasError)
                    {
                        #region Special handle for view
                        if (typeof(T) == typeof(View))
                        {
                            //Currently, ANTLR can't parse some complex tsql accurately, so it uses general strategy.
                            if (this.sourceDbInterpreter.DatabaseType == DatabaseType.SqlServer)
                            {
                                ViewTranslator viewTranslator = new ViewTranslator(this.sourceDbInterpreter, this.targetDbInterpreter, new List<View>() { dbObj as View }) { ContinueWhenErrorOccurs = this.ContinueWhenErrorOccurs };
                                viewTranslator.Translate();

                                replaced = true;
                            }

                            //Currently, ANTLR can't parse some view correctly, use procedure to parse it temporarily.
                            if (this.sourceDbInterpreter.DatabaseType == DatabaseType.Oracle)
                            {
                                string oldDefinition = dbObj.Definition;

                                int asIndex = oldDefinition.IndexOf(" AS ", StringComparison.OrdinalIgnoreCase);

                                StringBuilder sbNewDefinition = new StringBuilder();

                                sbNewDefinition.AppendLine($"CREATE OR REPLACE PROCEDURE {dbObj.Name} AS");
                                sbNewDefinition.AppendLine("BEGIN");
                                sbNewDefinition.AppendLine($"{oldDefinition.Substring(asIndex + 5).TrimEnd(';') + ";"}");
                                sbNewDefinition.AppendLine($"END {dbObj.Name};");

                                dbObj.Definition = sbNewDefinition.ToString();

                                AnalyseResult procResult = sourceAnalyser.Analyse<Procedure>(dbObj.Definition.ToUpper());

                                if (!procResult.HasError)
                                {
                                    processTokens(dbObj, procResult.Script);

                                    tokenProcessed = true;

                                    dbObj.Definition = Regex.Replace(dbObj.Definition, " PROCEDURE ", " VIEW ", RegexOptions.IgnoreCase);
                                    dbObj.Definition = Regex.Replace(dbObj.Definition, @"(BEGIN[\r][\n])|(END[\r][\n])", "", RegexOptions.IgnoreCase);

                                    replaced = true;
                                }
                            }
                        }
                        #endregion
                    }

                    if (!result.HasError && !tokenProcessed)
                    {
                        if (string.IsNullOrEmpty(dbObj.Name) && !string.IsNullOrEmpty(result.Script?.Name?.Symbol))
                        {
                            dbObj.Name = result.Script.Name.Symbol;
                            dbObj.Schema = result.Script.Schema;
                        }

                        processTokens(dbObj, script);
                    }

                    dbObj.Definition = this.ReplaceVariables(dbObj.Definition, this.variableMappings);

                    if (script is TriggerScript)
                    {
                        var triggerVariableMappings = TriggerVariableMappingManager.GetVariableMappings();

                        dbObj.Definition = this.ReplaceVariables(dbObj.Definition, triggerVariableMappings);
                    }

                    if (this.OnTranslated != null)
                    {
                        this.OnTranslated(this.targetDbInterpreter.DatabaseType, dbObj, new TranslateResult() { Error = replaced ? null : result.Error, Data = dbObj.Definition });
                    }

                    this.FeedbackInfo($"End translate {type.Name} \"{dbObj.Name}\", translate result: {(result.HasError ? "Error" : "OK")}.");

                    if (!replaced && result.HasError)
                    {
                        this.FeedbackError(this.ParseSqlSyntaxError(result.Error, originalDefinition).ToString(), this.ContinueWhenErrorOccurs);

                        if (!this.ContinueWhenErrorOccurs)
                        {
                            this.hasError = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var sce = new ScriptConvertException<T>(ex)
                    {
                        SourceServer = this.sourceDbInterpreter.ConnectionInfo.Server,
                        SourceDatabase = this.sourceDbInterpreter.ConnectionInfo.Database,
                        SourceObject = dbObj.Name,
                        TargetServer = this.targetDbInterpreter.ConnectionInfo.Server,
                        TargetDatabase = this.targetDbInterpreter.ConnectionInfo.Database,
                        TargetObject = dbObj.Name
                    };

                    if (!this.ContinueWhenErrorOccurs)
                    {
                        this.hasError = true;
                        throw sce;
                    }
                    else
                    {
                        this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex), this.ContinueWhenErrorOccurs);
                    }
                }
            }
        }

        public void Validate(ScriptDbObject script)
        {
            if (sourceDbType == DatabaseType.SqlServer && targetDbType != DatabaseType.SqlServer)
            {
                //ANTRL can't handle "top 100 percent" correctly.
                Regex regex = new Regex(@"(TOP[\s]+100[\s]+PERCENT)", RegexOptions.IgnoreCase);

                if (regex.IsMatch(script.Definition))
                {
                    script.Definition = regex.Replace(script.Definition, "");
                }
            }
        }

        private SqlSyntaxError ParseSqlSyntaxError(SqlSyntaxError error, string definition)
        {
            foreach (SqlSyntaxErrorItem item in error.Items)
            {
                item.Text = definition.Substring(item.StartIndex, item.StopIndex - item.StartIndex + 1);
            }

            return error;
        }

        public SqlAnalyserBase GetSqlAnalyser(DatabaseType dbType)
        {
            SqlAnalyserBase sqlAnalyser = null;

            if (dbType == DatabaseType.SqlServer)
            {
                sqlAnalyser = new TSqlAnalyser();
            }
            else if (dbType == DatabaseType.MySql)
            {
                sqlAnalyser = new MySqlAnalyser();
            }
            else if (dbType == DatabaseType.Oracle)
            {
                sqlAnalyser = new PlSqlAnalyser();
            }
            else if (dbType == DatabaseType.Postgres)
            {
                sqlAnalyser = new PostgreSqlAnalyser();
            }

            sqlAnalyser.RuleAnalyser.Option = new SqlRuleAnalyserOption()
            {
                ParseTokenChildren = true,
                ExtractFunctions = true,
                ExtractFunctionChildren = false
            };

            return sqlAnalyser;
        }
    }
}
