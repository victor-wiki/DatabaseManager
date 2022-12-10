using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using SqlAnalyser.Core;
using SqlAnalyser.Core.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class ScriptTranslator<T> : DbObjectTokenTranslator
        where T : ScriptDbObject
    {
        private IEnumerable<T> scripts;
        private ScriptBuildFactory scriptBuildFactory;
        public bool AutoMakeupSchemaName { get; set; } = true;
        public bool IsCommonScript { get; set; }


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

            this.scriptBuildFactory = this.GetScriptBuildFactory();

            int total = this.scripts.Count();
            int count = 0;
            int successCount = 0;

            foreach (T dbObj in this.scripts)
            {
                if (this.hasError)
                {
                    break;
                }

                if(string.IsNullOrEmpty(dbObj.Definition))
                {
                    continue;
                }

                count++;

                Type type = typeof(T);

                string percent = total == 1 ? "" : $"({count}/{total})";

                this.FeedbackInfo($"Begin to translate {type.Name}: \"{dbObj.Name}\"{percent}.");

                TranslateResult result = this.TranslateScript(dbObj);

                if (this.Option.CollectTranslateResultAfterTranslated)
                {
                    this.TranslateResults.Add(result);
                }

                if (!result.HasError)
                {
                    successCount++;
                }
            }

            if (total > 1)
            {
                this.FeedbackInfo($"Translated {total} script(s): {successCount} succeeded, {(total - successCount)} failed.");
            }            
        }

        private TranslateResult TranslateScript(ScriptDbObject dbObj,  bool isPartial = false)
        {
            TranslateResult translateResult = new TranslateResult() { DbObjectType = DbObjectHelper.GetDatabaseObjectType(dbObj), DbObjectSchema = dbObj.Schema, DbObjectName = dbObj.Name };

            try
            {
                Type type = typeof(T);

                bool tokenProcessed = false;

                dbObj.Definition = dbObj.Definition.Trim();

                if (this.Option.RemoveCarriagRreturnChar)
                {
                    dbObj.Definition = dbObj.Definition.Replace("\r\n", "\n");
                }

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
                AnalyseResult anlyseResult = null;

                SqlAnalyserBase sqlAnalyser = this.GetSqlAnalyser(this.sourceDbInterpreter.DatabaseType, originalDefinition);               

                if (!isPartial)
                {
                    anlyseResult = sqlAnalyser.Analyse<T>();
                }
                else
                {
                    anlyseResult = sqlAnalyser.AnalyseCommon();
                }

                CommonScript script = anlyseResult.Script;

                if (script == null)
                {
                    translateResult.Error = anlyseResult.Error;
                    translateResult.Data = dbObj.Definition;

                    return translateResult;
                }

                bool replaced = false;

                if (anlyseResult.HasError)
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

                            sqlAnalyser = this.GetSqlAnalyser(this.sourceDbType, dbObj.Definition);

                            AnalyseResult procResult = sqlAnalyser.Analyse<Procedure>();

                            if (!procResult.HasError)
                            {
                                this.ProcessTokens(dbObj, procResult.Script);

                                tokenProcessed = true;

                                dbObj.Definition = Regex.Replace(dbObj.Definition, " PROCEDURE ", " VIEW ", RegexOptions.IgnoreCase);
                                dbObj.Definition = Regex.Replace(dbObj.Definition, @"(BEGIN[\r][\n])|(END[\r][\n])", "", RegexOptions.IgnoreCase);

                                replaced = true;
                            }
                        }
                    }
                    #endregion
                }

                if (!anlyseResult.HasError && !tokenProcessed)
                {
                    if (string.IsNullOrEmpty(dbObj.Name) && !string.IsNullOrEmpty(anlyseResult.Script?.Name?.Symbol))
                    {
                        if (this.AutoMakeupSchemaName)
                        {
                            dbObj.Schema = anlyseResult.Script.Schema;
                        }

                        TranslateHelper.RestoreTokenValue(originalDefinition, anlyseResult.Script.Name);

                        dbObj.Name = anlyseResult.Script.Name.Symbol;
                    }

                    this.ProcessTokens(dbObj, script);
                }

                dbObj.Definition = this.ReplaceVariables(dbObj.Definition, this.variableMappings);

                if (script is TriggerScript)
                {
                    var triggerVariableMappings = TriggerVariableMappingManager.GetVariableMappings();

                    dbObj.Definition = this.ReplaceVariables(dbObj.Definition, triggerVariableMappings);
                }

                if (script is RoutineScript)
                {
                    if (this.sourceDbType == DatabaseType.Postgres)
                    {
                        if (!isPartial)
                        {
                            string declaresAndBody = PostgresTranslateHelper.ExtractRountineScriptDeclaresAndBody(originalDefinition);

                            ScriptDbObject scriptDbObject = new ScriptDbObject() { Definition = declaresAndBody };

                            TranslateResult res = this.TranslateScript(scriptDbObject, true);

                            if (!res.HasError)
                            {
                                dbObj.Definition = PostgresTranslateHelper.MergeDefinition(dbObj.Definition, scriptDbObject.Definition);
                            }
                            else
                            {
                                anlyseResult = new AnalyseResult() { Error = res.Error as SqlSyntaxError };
                            }
                        }
                    }
                }

                dbObj.Definition = TranslateHelper.TranslateComments(this.sourceDbInterpreter, this.targetDbInterpreter, dbObj.Definition);

                if (isPartial)
                {
                    return translateResult;
                }

                translateResult.Error = replaced ? null : anlyseResult.Error;
                translateResult.Data = dbObj.Definition;

                this.FeedbackInfo($"End translate {type.Name}: \"{dbObj.Name}\", translate result: {(anlyseResult.HasError ? "Error" : "OK")}.");

                if (!replaced && anlyseResult.HasError)
                {
                    string errMsg = this.ParseSqlSyntaxError(anlyseResult.Error, originalDefinition).ToString();

                    this.FeedbackError(errMsg, this.ContinueWhenErrorOccurs);

                    if (!this.ContinueWhenErrorOccurs)
                    {
                        this.hasError = true;
                    }
                }

                return translateResult;
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

                return translateResult;
            }
        }

        private void ProcessTokens(ScriptDbObject dbObj, CommonScript script)
        {
            if (typeof(T) == typeof(Function))
            {
                SqlAnalyserBase sqlAnalyser = this.GetSqlAnalyser(this.sourceDbType, dbObj.Definition);

                AnalyseResult result = sqlAnalyser.AnalyseFunction();

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

                        anotherDefinition = StringHelper.FormatScript(this.scriptBuildFactory.GenerateScripts(rs).Script);
                    }
                }

                ScriptBuildResult scriptBuildResult = this.scriptBuildFactory.GenerateScripts(script);

                dbObj.Definition = StringHelper.FormatScript(scriptBuildResult.Script);

                if (anotherDefinition != null)
                {
                    dbObj.Definition = anotherDefinition + Environment.NewLine + dbObj.Definition;
                }
            };
        }

        private SqlSyntaxError ParseSqlSyntaxError(SqlSyntaxError error, string definition)
        {
            foreach (SqlSyntaxErrorItem item in error.Items)
            {
                item.Text = definition.Substring(item.StartIndex, item.StopIndex - item.StartIndex + 1);
            }

            return error;
        }

        public SqlAnalyserBase GetSqlAnalyser(DatabaseType dbType, string content)
        {
            SqlAnalyserBase sqlAnalyser = TranslateHelper.GetSqlAnalyser(dbType, content);

            sqlAnalyser.RuleAnalyser.Option.ParseTokenChildren = true;
            sqlAnalyser.RuleAnalyser.Option.ExtractFunctions = true;
            sqlAnalyser.RuleAnalyser.Option.ExtractFunctionChildren = false;
            sqlAnalyser.RuleAnalyser.Option.IsCommonScript = this.IsCommonScript;            

            return sqlAnalyser;
        }

        public ScriptBuildFactory GetScriptBuildFactory()
        {
            ScriptBuildFactory factory = TranslateHelper.GetScriptBuildFactory(this.targetDbType);

            factory.ScriptBuilderOption.OutputRemindInformation = this.Option.OutputRemindInformation;

            return factory;
        }
    }
}
