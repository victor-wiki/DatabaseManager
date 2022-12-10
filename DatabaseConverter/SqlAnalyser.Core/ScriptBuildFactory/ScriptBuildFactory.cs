using DatabaseInterpreter.Model;
using SqlAnalyser.Core.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlAnalyser.Core
{
    public abstract class ScriptBuildFactory
    {
        private StatementScriptBuilder statementBuilder;
        public abstract DatabaseType DatabaseType { get; }
        public StatementScriptBuilderOption ScriptBuilderOption { get; set; } = new StatementScriptBuilderOption();

        public abstract ScriptBuildResult GenerateRoutineScripts(RoutineScript script);
        public abstract ScriptBuildResult GenearteViewScripts(ViewScript script);
        public abstract ScriptBuildResult GenearteTriggerScripts(TriggerScript script);

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
            else if (this.DatabaseType == DatabaseType.Sqlite)
            {
                builder = new SqliteStatementScriptBuilder();
            }
            else
            {
                throw new NotSupportedException($"Not support build statement for: {this.DatabaseType}");
            }

            builder.Option = this.ScriptBuilderOption;

            return builder;
        }

        public string BuildStatement(Statement statement, RoutineType routineType = RoutineType.UNKNOWN)
        {
            this.StatementBuilder.RoutineType = routineType;

            this.StatementBuilder.Clear();

            this.StatementBuilder.Build(statement);

            return this.StatementBuilder.ToString();
        }

        public virtual ScriptBuildResult GenerateScripts(CommonScript script)
        {
            ScriptBuildResult result;

            if (script is RoutineScript routineScript)
            {
                result = this.GenerateRoutineScripts(routineScript);
            }
            else if (script is ViewScript viewScript)
            {
                result = this.GenearteViewScripts(viewScript);
            }
            else if (script is TriggerScript triggerScript)
            {
                result = this.GenearteTriggerScripts(triggerScript);
            }
            else if (script is CommonScript commonScript)
            {
                result = this.GenerateCommonScripts(commonScript);
            }
            else
            {
                throw new NotSupportedException($"Not support generate scripts for type: {script.GetType()}.");
            }

            if (this.statementBuilder != null && this.statementBuilder.Replacements.Count > 0)
            {
                foreach (var kp in this.statementBuilder.Replacements)
                {
                    result.Script = AnalyserHelper.ReplaceSymbol(result.Script, kp.Key, kp.Value);
                }
            }

            if (this.statementBuilder != null)
            {
                this.statementBuilder.Dispose();
            }

            return result;
        }

        protected virtual void PreHandleStatements(List<Statement> statements) { }
        protected virtual void PostHandleStatements(StringBuilder sb) { }

        protected virtual ScriptBuildResult GenerateCommonScripts(CommonScript script)
        {
            this.PreHandleStatements(script.Statements);

            ScriptBuildResult result = new ScriptBuildResult();

            StringBuilder sb = new StringBuilder();

            foreach (Statement statement in script.Statements)
            {
                sb.AppendLine(this.BuildStatement(statement));
            }

            this.PostHandleStatements(sb);

            result.Script = sb.ToString().Trim();

            if (this.statementBuilder != null)
            {
                this.statementBuilder.Dispose();
            }

            return result;
        }
    }
}
