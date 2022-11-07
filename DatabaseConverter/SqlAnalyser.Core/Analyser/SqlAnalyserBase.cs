using DatabaseInterpreter.Model;
using SqlAnalyser.Core.Model;
using SqlAnalyser.Model;
using System;
using System.Runtime.CompilerServices;

namespace SqlAnalyser.Core
{
    public abstract class SqlAnalyserBase
    {
        private StatementScriptBuilder statementBuilder;

        public abstract DatabaseType DatabaseType { get; }
        public abstract ScriptBuildResult GenerateScripts(CommonScript script);
        public abstract AnalyseResult AnalyseView(string content);
        public abstract AnalyseResult AnalyseProcedure(string content);
        public abstract AnalyseResult AnalyseFunction(string content);
        public abstract AnalyseResult AnalyseTrigger(string content);
        public abstract SqlRuleAnalyser RuleAnalyser { get; }

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

        public StatementScriptBuilder StatementBuilder
        {
            get
            {
                if (this.statementBuilder == null)
                {
                    this.statementBuilder = this.GetStatementBuilder();
                }

                return this.statementBuilder;
            }
        }

        private StatementScriptBuilder GetStatementBuilder()
        {
            StatementScriptBuilder builder = null;

            if (this.DatabaseType == DatabaseType.SqlServer)
            {
                builder = new TSqlStatementScriptBuilder();
            }
            else if (this.DatabaseType == DatabaseType.MySql)
            {
                builder = new MySqlStatementScriptBuilder();
            }
            else if (this.DatabaseType == DatabaseType.Oracle)
            {
                builder = new PlSqlStatementScriptBuilder();
            }
            else if (this.DatabaseType == DatabaseType.Postgres)
            {
                builder = new PostgreSqlStatementScriptBuilder();
            }
            else
            {
                throw new NotSupportedException($"Not support buid statement for: {this.DatabaseType}");
            }

            return builder;
        }

        public string BuildStatement(Statement statement,RoutineType routineType = RoutineType.UNKNOWN, StatementScriptBuilderOption option = null)
        {
            this.StatementBuilder.RoutineType = routineType;
            this.statementBuilder.Option = option;
            this.StatementBuilder.Clear();

            this.StatementBuilder.Build(statement);

            return this.StatementBuilder.ToString();
        }
    }
}
