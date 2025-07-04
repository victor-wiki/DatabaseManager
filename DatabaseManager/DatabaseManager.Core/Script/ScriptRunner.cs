﻿using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SqlAnalyser.Core;
using DatabaseConverter.Core;
using SqlAnalyser.Model;
using System.Runtime.InteropServices;
using Oracle.ManagedDataAccess.Client;

namespace DatabaseManager.Core
{
    public class ScriptRunner
    {
        private DbTransaction transaction = null;
        private bool cancelRequested = false;
        private IObserver<FeedbackInfo> observer;
        private bool isBusy = false;
        private bool hasError = false;

        public CancellationTokenSource CancellationTokenSource { get; private set; }
        public bool CancelRequested => this.cancelRequested;
        public bool IsBusy => this.isBusy;
        public int LimitCount { get; set; } = 1000;

        public event FeedbackHandler OnFeedback;

        public ScriptRunner()
        {
            this.CancellationTokenSource = new CancellationTokenSource();
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public async Task<QueryResult> Run(DatabaseType dbType, ConnectionInfo connectionInfo, string script, ScriptAction action = ScriptAction.NONE, List<RoutineParameter> parameters = null)
        {
            this.cancelRequested = false;
            this.isBusy = false;

            QueryResult result = new QueryResult();

            DbInterpreterOption option = new DbInterpreterOption() { RequireInfoMessage = true };

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(dbType, connectionInfo, option);

            dbInterpreter.Subscribe(this.observer);

            try
            {
                ScriptParser scriptParser = new ScriptParser(dbInterpreter, script);               

                using (DbConnection dbConnection = dbInterpreter.CreateConnection())
                {
                    if (scriptParser.IsSelect())
                    {
                        this.isBusy = true;
                        result.ResultType = QueryResultType.Grid;

                        script = this.DecorateSelectWithLimit(dbInterpreter, script);

                        if (!scriptParser.IsCreateOrAlterScript() && dbInterpreter.ScriptsDelimiter.Length == 1)
                        {
                            script = script.Trim().TrimEnd(dbInterpreter.ScriptsDelimiter[0]);
                        }

                        DataTable dataTable = await dbInterpreter.GetDataTableAsync(dbConnection, script);

                        result.Result = dataTable;
                    }
                    else
                    {
                        this.isBusy = true;
                        result.ResultType = QueryResultType.Text;

                        await dbConnection.OpenAsync();

                        this.transaction = await dbConnection.BeginTransactionAsync();

                        IEnumerable<string> commands = Enumerable.Empty<string>();

                        if (scriptParser.IsCreateOrAlterScript())
                        {
                            if (dbInterpreter.DatabaseType == DatabaseType.Oracle)
                            {
                                ScriptType scriptType = ScriptParser.DetectScriptType(script, dbInterpreter);

                                if (scriptType != ScriptType.Procedure && scriptType != ScriptType.Function && scriptType != ScriptType.Trigger)
                                {
                                    script = script.Trim().TrimEnd(dbInterpreter.ScriptsDelimiter[0]);
                                }
                            }

                            commands = new string[] { script };
                        }
                        else
                        {
                            string delimiter = dbInterpreter.ScriptsDelimiter;

                            StringBuilder sb = new StringBuilder();

                            var lines = script.Split(Environment.NewLine);

                            foreach (var line in lines)
                            {
                                if (line.Trim() == delimiter.Trim())
                                {
                                    continue;
                                }
                                else
                                {
                                    sb.AppendLine(line);
                                }
                            }

                            string str = sb.ToString();

                            string defaultDelimiter = ";";

                            var items = str.Split(new string[] { defaultDelimiter }, StringSplitOptions.RemoveEmptyEntries);

                            List<string> cmds = new List<string>();

                            int totalSingleQuotationCount = 0;

                            Action<string> appendToLastItem = (item) =>
                            {
                                if (cmds.Any())
                                {
                                    cmds[cmds.Count - 1] += defaultDelimiter + item;
                                }
                                else
                                {
                                    cmds.Add(item);
                                }
                            };

                            foreach (var item in items)
                            {
                                if (!item.Contains("'"))
                                {
                                    if (totalSingleQuotationCount % 2 == 0)
                                    {
                                        cmds.Add(item);
                                    }
                                    else
                                    {
                                        appendToLastItem(item);
                                    }
                                }
                                else
                                {
                                    if (item.Count(t => t == '\'') % 2 != 0)
                                    {
                                        appendToLastItem(item);
                                    }
                                    else
                                    {
                                        cmds.Add(item);
                                    }
                                }

                                totalSingleQuotationCount += item.Count(t => t == '\'');
                            }

                            commands = cmds.ToArray();
                        }

                        int affectedRows = 0;

                        bool isProcedureCall = action == ScriptAction.EXECUTE;

                        var commandType = (isProcedureCall && dbInterpreter.DatabaseType == DatabaseType.Oracle) ? CommandType.StoredProcedure : CommandType.Text;

                        foreach (string command in commands)
                        {
                            if (string.IsNullOrEmpty(command.Trim()))
                            {
                                continue;
                            }

                            CommandInfo commandInfo = new CommandInfo()
                            {
                                CommandType = commandType,
                                CommandText = command,
                                Transaction = this.transaction,
                                CancellationToken = this.CancellationTokenSource.Token
                            };

                            if (commandType == CommandType.StoredProcedure)
                            {
                                if (action == ScriptAction.EXECUTE && dbInterpreter.DatabaseType == DatabaseType.Oracle)
                                {
                                    this.ParseOracleProcedureCall(commandInfo, parameters);
                                }
                            }                           

                            ExecuteResult res = await dbInterpreter.ExecuteNonQueryAsync(dbConnection, commandInfo);

                            affectedRows += (res.NumberOfRowsAffected == -1 ? 0 : res.NumberOfRowsAffected);
                        }

                        result.Result = affectedRows;

                        if (!this.cancelRequested)
                        {
                            this.transaction.Commit();
                        }
                    }

                    this.isBusy = false;
                }
            }
            catch (Exception ex)
            {
                this.Rollback(ex);

                result.ResultType = QueryResultType.Text;
                result.HasError = true;
                result.Result = ex.Message;

                this.HandleError(ex);
            }

            return result;
        }

        private void ParseOracleProcedureCall(CommandInfo cmd, List<RoutineParameter> parameters)
        {
            SqlAnalyserBase sqlAnalyser = TranslateHelper.GetSqlAnalyser(DatabaseType.Oracle, cmd.CommandText);

            sqlAnalyser.RuleAnalyser.Option.ParseTokenChildren = false;
            sqlAnalyser.RuleAnalyser.Option.ExtractFunctions = false;
            sqlAnalyser.RuleAnalyser.Option.ExtractFunctionChildren = false;
            sqlAnalyser.RuleAnalyser.Option.IsCommonScript = true;

            var result = sqlAnalyser.AnalyseCommon();

            if (result != null && !result.HasError)
            {
                CommonScript cs = result.Script;

                CallStatement callStatement = cs.Statements.FirstOrDefault(item => item is CallStatement) as CallStatement;

                if (callStatement != null)
                {
                    cmd.CommandText = callStatement.Name.Symbol;

                    if (parameters != null && callStatement.Parameters != null && parameters.Count == callStatement.Parameters.Count)
                    {
                        cmd.Parameters = new Dictionary<string, object>();

                        int i = 0;

                        foreach (var para in parameters.OrderBy(item => item.Order))
                        {
                            cmd.Parameters.Add(para.Name, callStatement.Parameters[i]?.Value?.Symbol);

                            i++;
                        }
                    }
                }
            }
        }

        public async Task Run(DbInterpreter dbInterpreter, IEnumerable<Script> scripts)
        {
            using (DbConnection dbConnection = dbInterpreter.CreateConnection())
            {
                await dbConnection.OpenAsync();

                DbTransaction transaction = await dbConnection.BeginTransactionAsync();

                Func<Script, bool> isValidScript = (s) =>
                {
                    return !(s is NewLineScript || s is SpliterScript || string.IsNullOrEmpty(s.Content) || s.Content == dbInterpreter.ScriptsDelimiter);
                };

                int count = scripts.Where(item => isValidScript(item)).Count();
                int i = 0;

                foreach (Script s in scripts)
                {
                    if (!isValidScript(s))
                    {
                        continue;
                    }

                    string sql = s.Content?.Trim();

                    if (!string.IsNullOrEmpty(sql) && sql != dbInterpreter.ScriptsDelimiter)
                    {
                        i++;

                        if (dbInterpreter.ScriptsDelimiter.Length == 1 && sql.EndsWith(dbInterpreter.ScriptsDelimiter))
                        {
                            sql = sql.TrimEnd(dbInterpreter.ScriptsDelimiter.ToArray());
                        }

                        if (!this.hasError)
                        {
                            CommandInfo commandInfo = new CommandInfo()
                            {
                                CommandType = CommandType.Text,
                                CommandText = sql,
                                Transaction = transaction,
                                CancellationToken = this.CancellationTokenSource.Token
                            };

                            await dbInterpreter.ExecuteNonQueryAsync(dbConnection, commandInfo);
                        }
                    }
                }

                transaction.Commit();
            }
        }

        private void HandleError(Exception ex)
        {
            this.isBusy = false;

            string errMsg = ExceptionHelper.GetExceptionDetails(ex);
            this.Feedback(this, errMsg, FeedbackInfoType.Error, true, true);
        }

        public async void Cancle()
        {
            this.cancelRequested = true;

            this.Rollback();

            if (this.CancellationTokenSource != null)
            {
                await this.CancellationTokenSource.CancelAsync();
            }
        }

        private async void Rollback(Exception ex = null)
        {
            if (this.transaction != null && this.transaction.Connection != null && this.transaction.Connection.State == ConnectionState.Open)
            {
                try
                {
                    this.cancelRequested = true;

                    bool hasRollbacked = false;

                    if (ex != null && ex is DbCommandException dbe)
                    {
                        hasRollbacked = dbe.HasRollbackedTransaction;
                    }

                    if (!hasRollbacked)
                    {
                        await this.transaction.RollbackAsync();
                    }
                }
                catch (Exception e)
                {
                    //throw;
                }
            }
        }

        public string DecorateSelectWithLimit(DbInterpreter dbInterpreter, string script)
        {
            DatabaseType databaseType = dbInterpreter.DatabaseType;

            SqlAnalyserBase sqlAnalyser = TranslateHelper.GetSqlAnalyser(databaseType, script);

            sqlAnalyser.RuleAnalyser.Option.ParseTokenChildren = false;
            sqlAnalyser.RuleAnalyser.Option.ExtractFunctions = false;
            sqlAnalyser.RuleAnalyser.Option.ExtractFunctionChildren = false;
            sqlAnalyser.RuleAnalyser.Option.IsCommonScript = true;

            var result = sqlAnalyser.AnalyseCommon();

            if (result != null && !result.HasError)
            {
                CommonScript cs = result.Script;

                SelectStatement selectStatement = cs.Statements.FirstOrDefault(item => item is SelectStatement) as SelectStatement;

                if (selectStatement != null)
                {
                    ScriptTokenExtracter tokenExtracter = new ScriptTokenExtracter(selectStatement);
                    List<TokenInfo> tokens = tokenExtracter.Extract().ToList();

                    foreach (var token in tokens)
                    {
                        TranslateHelper.RestoreTokenValue(script, token);
                    }

                    TableName tableName = selectStatement.TableName;
                    TokenInfo alias = null;

                    if (tableName == null)
                    {
                        if (selectStatement.HasFromItems)
                        {
                            tableName = selectStatement.FromItems[0].TableName;
                            alias = selectStatement.FromItems[0].Alias;
                        }
                    }

                    bool hasTableName = tableName != null;
                    bool hasAlias = alias != null;
                    bool isFromDUAL =  tableName?.Symbol?.ToUpper() == "DUAL";

                    if (!isFromDUAL && (hasTableName || hasAlias) && (selectStatement.TopInfo == null && selectStatement.LimitInfo == null))
                    {
                        string defaultOrder = dbInterpreter.GetDefaultOrder();

                        if (selectStatement.OrderBy == null && !string.IsNullOrEmpty(defaultOrder))
                        {
                            bool canUseOrderby = true;

                            if (dbInterpreter.DatabaseType == DatabaseType.SqlServer && selectStatement.IsDistinct)
                            {
                                canUseOrderby = false;
                            }

                            if (canUseOrderby)
                            {
                                selectStatement.OrderBy = new List<TokenInfo>() { new TokenInfo(defaultOrder) };
                            }                            
                        }

                        if (databaseType == DatabaseType.SqlServer)
                        {
                            selectStatement.TopInfo = new SelectTopInfo() { TopCount = new TokenInfo(this.LimitCount.ToString()) };
                        }
                        else if (databaseType == DatabaseType.MySql || databaseType == DatabaseType.Postgres || databaseType == DatabaseType.Sqlite)
                        {
                            selectStatement.LimitInfo = new SelectLimitInfo() { StartRowIndex = new TokenInfo("0"), RowCount = new TokenInfo(this.LimitCount.ToString()) };
                        }

                        ScriptBuildFactory scriptBuildFactory = TranslateHelper.GetScriptBuildFactory(databaseType);

                        script = scriptBuildFactory.GenerateScripts(cs).Script;

                        if (databaseType == DatabaseType.Oracle) //oracle low version doesn't support limit clause.
                        {
                            script = $@"SELECT * FROM
                               (
                                 {script.Trim().TrimEnd(';')}
                               ) TEMP
                               WHERE ROWNUM BETWEEN 1 AND {this.LimitCount}";
                        }
                    }
                }
            }

            return script;
        }

        public void Feedback(object owner, string content, FeedbackInfoType infoType = FeedbackInfoType.Info, bool enableLog = true, bool suppressError = false)
        {
            FeedbackInfo info = new FeedbackInfo() { InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(content), Owner = owner };

            FeedbackHelper.Feedback(suppressError ? null : this.observer, info, enableLog);

            if (infoType == FeedbackInfoType.Error && !suppressError)
            {
                this.hasError = true;
            }

            if (this.OnFeedback != null)
            {
                this.OnFeedback(info);
            }
        }
    }
}
