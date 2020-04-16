using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using SqlAnalyser.Core;
using System.Linq;
using SqlAnalyser.Model;
using System.Text.RegularExpressions;
using DatabaseInterpreter.Utility;

namespace DatabaseConverter.Core
{
    public class ScriptTranslator<T> : DbObjectTokenTranslator
        where T : ScriptDbObject
    {
        public List<UserDefinedType> UserDefinedTypes { get; set; } = new List<UserDefinedType>();
        public string TargetDbOwner { get; set; }
        private List<T> scripts;

        public ScriptTranslator(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, List<T> scripts) : base(sourceDbInterpreter, targetDbInterpreter)
        {
            this.scripts = scripts;
        }

        public override void Translate()
        {
            if (this.sourceDbInterpreter.DatabaseType == this.targetDbInterpreter.DatabaseType)
            {
                return;
            }

            this.LoadMappings();

            SqlAnalyserBase sourceAnalyser = this.GetSqlAnalyser(this.sourceDbInterpreter.DatabaseType);
            SqlAnalyserBase targetAnalyser = this.GetSqlAnalyser(this.targetDbInterpreter.DatabaseType);

            foreach (T dbObj in this.scripts)
            {
                try
                {
                    CommonScript script = sourceAnalyser.Analyse<T>(dbObj.Definition.ToUpper());

                    if (script != null)
                    {
                        if (script.HasError)
                        {
                            if(typeof(T) == typeof(View))
                            {
                                //Currently, ANTLR can't parse some complex tsql accurately, so it uses general strategy.
                                if (this.sourceDbInterpreter.DatabaseType == DatabaseType.SqlServer)
                                {
                                    ViewTranslator viewTranslator = new ViewTranslator(this.sourceDbInterpreter, this.targetDbInterpreter, new List<View>() { dbObj as View }, this.TargetDbOwner) { SkipError = this.SkipError };
                                    viewTranslator.Translate();
                                }                               
                            }                            
                        }

                        if(!script.HasError)
                        {
                            if (typeof(T) == typeof(Function))
                            {
                                script = sourceAnalyser.AnalyseFunction(dbObj.Definition.ToUpper());

                                RoutineScript routine = script as RoutineScript;

                                if (this.targetDbInterpreter.DatabaseType == DatabaseType.MySql && routine.ReturnTable != null)
                                {
                                    routine.Type = RoutineType.PROCEDURE;
                                }
                            }

                            ScriptTokenProcessor tokenProcessor = new ScriptTokenProcessor(script, dbObj, this.sourceDbInterpreter, this.targetDbInterpreter);
                            tokenProcessor.UserDefinedTypes = this.UserDefinedTypes;
                            tokenProcessor.TargetDbOwner = this.TargetDbOwner;

                            tokenProcessor.Process();

                            dbObj.Definition = targetAnalyser.GenerateScripts(script);
                        }                       

                        bool formatHasError = false;

                        string definition = this.ReplaceVariables(dbObj.Definition);

                        dbObj.Definition = definition; // this.FormatSql(definition, out formatHasError);

                        if (formatHasError)
                        {
                            dbObj.Definition = definition;
                        }

                        if (this.OnTranslated != null)
                        {
                            this.OnTranslated(this.targetDbInterpreter.DatabaseType, dbObj, dbObj.Definition);
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

                    if (!this.SkipError)
                    {
                        throw sce;
                    }
                    else
                    {
                        FeedbackInfo info = new FeedbackInfo() { InfoType = FeedbackInfoType.Error, Message = ExceptionHelper.GetExceptionDetails(ex), Owner = this };
                        FeedbackHelper.Feedback(info);
                    }
                }
            }
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

            return sqlAnalyser;
        }
    }
}
