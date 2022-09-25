using DatabaseInterpreter.Model;
using SqlAnalyser.Core.Model;
using SqlAnalyser.Model;
using System;

namespace SqlAnalyser.Core
{
    public abstract class SqlAnalyserBase
    {
        public abstract DatabaseType DatabaseType { get; }
        public abstract ScriptBuildResult GenerateScripts(CommonScript script);
        public abstract AnalyseResult AnalyseView(string content);
        public abstract AnalyseResult AnalyseProcedure(string content);
        public abstract AnalyseResult AnalyseFunction(string content);
        public abstract AnalyseResult AnalyseTrigger(string content);

        public AnalyseResult Analyse<T>(string content) where T : DatabaseObject
        {
            AnalyseResult result = null;

            if (typeof(T) == typeof(Procedure))
            {
                result = this.AnalyseProcedure(content);
            }
            else if (typeof(T) == typeof(Function))
            {
                result = this.AnalyseFunction(content);
            }
            else if (typeof(T) == typeof(View))
            {
                result = this.AnalyseView(content);
            }
            else if (typeof(T) == typeof(TableTrigger))
            {
                result = this.AnalyseTrigger(content);
            }
            else
            {
                throw new NotSupportedException($"Not support analyse for type:{typeof(T).Name}");
            }            

            return result;
        }     

        public string BuildStatement(Statement statement)
        {
            StatementScriptBuilder sb = null;

            if (this.DatabaseType == DatabaseType.SqlServer)
            {
                sb = new TSqlStatementScriptBuilder();               
            }
            else if (this.DatabaseType == DatabaseType.MySql)
            {
                sb = new MySqlStatementScriptBuilder();               
            }
            else if (this.DatabaseType == DatabaseType.Oracle)
            {
                sb = new PlSqlStatementScriptBuilder();               
            }
            else if(this.DatabaseType == DatabaseType.Postgres)
            {
                sb = new PostgreSqlStatementScriptBuilder();
            }
            else
            {
                throw new NotSupportedException($"Not support buid statement for: {this.DatabaseType}");
            }          

            sb.Build(statement);

            return sb.ToString();
        }       
    }
}
