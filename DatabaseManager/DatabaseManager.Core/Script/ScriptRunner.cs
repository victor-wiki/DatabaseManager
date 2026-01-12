using Dapper;
using DatabaseConverter.Core;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core.Model;
using Newtonsoft.Json;
using SqlAnalyser.Core;
using SqlAnalyser.Model;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseManager.Core
{
    public class ScriptRunner
    {
        private DbTransaction transaction = null;
        private IObserver<FeedbackInfo> observer;
        private bool isBusy = false;
        private bool hasError = false;
        private ScriptRunOption option;

        public bool IgnoreForeignKeyConstraint { get; set; }

        public int LimitCount { get; set; } = 1000;

        public event FeedbackHandler OnFeedback;

        public ScriptRunner(ScriptRunOption option = null)
        {
            if (option == null)
            {
                option = new ScriptRunOption();
            }

            this.option = option;
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public async Task<QueryResult> Run(DatabaseType dbType, ConnectionInfo connectionInfo, string script,
            CancellationToken cancellationToken, ScriptAction action = ScriptAction.NONE, List<RoutineParameter> parameters = null, PaginationInfo paginationInfo = null)
        {
            this.isBusy = false;

            QueryResult result = new QueryResult();

            DbInterpreterOption option = new DbInterpreterOption() { RequireInfoMessage = true };

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(dbType, connectionInfo, option);

            dbInterpreter.Subscribe(this.observer);

            try
            {
                ScriptParser scriptParser = new ScriptParser(dbInterpreter, script);

                bool isSelect = scriptParser.IsSelect();

                if (isSelect)
                {
                    connectionInfo.NeedCheckServerVersion = true;
                }

                bool useProfiler = this.option.UseProfiler;

                MiniProfiler mp = null;

                DbConnection dbConnection = dbInterpreter.CreateConnection();

                if (useProfiler)
                {
                    mp = MiniProfiler.StartNew(Guid.NewGuid().ToString());

                    dbConnection = new StackExchange.Profiling.Data.ProfiledDbConnection(dbConnection, MiniProfiler.Current);
                }

                using (dbConnection)
                {
                    if (isSelect)
                    {
                        this.isBusy = true;
                        result.ResultType = QueryResultType.Grid;

                        if(this.option.UseSqlParser)
                        {
                            SelectScriptAnalyseResult analyseResult = this.AnalyseSelect(dbInterpreter, script, paginationInfo);

                            result.SelectScriptAnalyseResult = analyseResult;

                            script = analyseResult.Script;

                            if ((analyseResult.HasTableName || analyseResult.HasAlias) && analyseResult.CountScript != null)
                            {
                                await dbConnection.OpenAsync();

                                string countScript = analyseResult.CountScript;

                                countScript = GetTrimedScript(dbInterpreter, countScript);

                                analyseResult.CountScript = countScript;

                                long? totalCount = (await dbConnection.QueryAsync<long?>(countScript)).Sum(item => item ?? 0);

                                result.TotalCount = totalCount;
                            }
                        }              

                        script = GetTrimedScript(dbInterpreter, script);

                        DataTable dataTable = await dbInterpreter.GetDataTableAsync(dbConnection, script, cancellationToken, true);

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

                        int commandCount = commands.Count();
                        int affectedRows = 0;

                        bool isProcedureCall = action == ScriptAction.EXECUTE;

                        var commandType = (isProcedureCall && dbInterpreter.DatabaseType == DatabaseType.Oracle) ? CommandType.StoredProcedure : CommandType.Text;


                        foreach (string command in commands)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                await transaction.RollbackAsync();
                                break;
                            }

                            if (string.IsNullOrEmpty(command.Trim()))
                            {
                                continue;
                            }

                            CommandInfo commandInfo = new CommandInfo()
                            {
                                CommandType = commandType,
                                CommandText = command,
                                Transaction = this.transaction,
                                CancellationToken = cancellationToken
                            };

                            if (commandType == CommandType.StoredProcedure)
                            {
                                if (action == ScriptAction.EXECUTE && dbInterpreter.DatabaseType == DatabaseType.Oracle)
                                {
                                    this.ParseOracleProcedureCall(commandInfo, parameters);
                                }
                            }

                            ExecuteResult res = await dbInterpreter.ExecuteNonQueryAsync(dbConnection, commandInfo);

                            if (commandCount > 1)
                            {
                                affectedRows += (res.NumberOfRowsAffected == -1 ? 0 : res.NumberOfRowsAffected);
                            }
                            else
                            {
                                affectedRows = res.NumberOfRowsAffected;
                            }

                        }

                        result.Result = affectedRows;

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            await this.transaction.CommitAsync();
                        }
                    }

                    if (mp != null)
                    {
                        try
                        {
                            await mp.StopAsync();

                            var profilingInfo = JsonConvert.DeserializeObject<ProfilingInfo>(mp.ToJson());

                            if (profilingInfo != null && profilingInfo.Root != null && profilingInfo.Root.CustomTimings != null && profilingInfo.Root.CustomTimings.sql != null)
                            {
                                result.ProfilingResult = new ProfilingResult()
                                {
                                    Id = profilingInfo.Id,
                                    Name = profilingInfo.Name,
                                    Started = profilingInfo.Started,
                                    Duration = profilingInfo.DurationMilliseconds,
                                    Details = new List<ProfilingResultDetail>()
                                };

                                result.ProfilingResult.Details.AddRange(profilingInfo.Root.CustomTimings.sql.Select(item => new ProfilingResultDetail()
                                {
                                    Id = item.Id,
                                    Sql = item.CommandString?.TrimEnd(' ', ';'),
                                    ExecuteType = item.ExecuteType,
                                    Duration = item.DurationMilliseconds,
                                    HasError = item.Errored
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
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

        private static string GetTrimedScript(DbInterpreter dbInterpreter, string script)
        {
            if (dbInterpreter.ScriptsDelimiter.Length == 1)
            {
                script = script.Trim().TrimEnd(dbInterpreter.ScriptsDelimiter[0]).Trim();
            }

            return script;
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

        public async Task Run(DbInterpreter dbInterpreter, IEnumerable<Script> scripts, CancellationToken cancellationToken)
        {
            using (DbConnection dbConnection = dbInterpreter.CreateConnection())
            {
                if (this.IgnoreForeignKeyConstraint)
                {
                    if (dbInterpreter.DatabaseType == DatabaseType.Sqlite)
                    {
                        dbConnection.ConnectionString = dbConnection.ConnectionString + ";Foreign Keys=False";
                    }
                }

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

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
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
                                CancellationToken = cancellationToken
                            };

                            await dbInterpreter.ExecuteNonQueryAsync(dbConnection, commandInfo);
                        }
                    }
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    await transaction.CommitAsync();
                }
            }
        }

        private void HandleError(Exception ex)
        {
            this.isBusy = false;

            string errMsg = ExceptionHelper.GetExceptionDetails(ex);
            this.Feedback(this, errMsg, FeedbackInfoType.Error, true, true);
        }

        private async void Rollback(Exception ex = null)
        {
            if (this.transaction != null && this.transaction.Connection != null && this.transaction.Connection.State == ConnectionState.Open)
            {
                try
                {
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

        public SelectScriptAnalyseResult AnalyseSelect(DbInterpreter dbInterpreter, string script, PaginationInfo paginationInfo)
        {
            SelectScriptAnalyseResult analyseResult = new SelectScriptAnalyseResult();

            DatabaseType databaseType = dbInterpreter.DatabaseType;

            try
            {
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
                        bool isFromDUAL = tableName?.Symbol?.ToUpper() == "DUAL";

                        analyseResult.HasTableName = hasTableName;
                        analyseResult.HasAlias = hasAlias;
                        analyseResult.HasLimitCount = selectStatement.TopInfo != null || selectStatement.LimitInfo != null;

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

                            int pageSize = paginationInfo == null ? this.LimitCount : paginationInfo.PageSize;
                            long pageNumber = paginationInfo == null ? 1 : paginationInfo.PageNumber;
                            long offset = (pageNumber - 1) * pageSize;
                            bool isUseSpecialPaination = false;
                            string rowNumberColumnName = DbInterpreter.RowNumberColumnName;
                            SelectStatement lastSelectStatement = selectStatement;

                            Action useNormalPagination = () =>
                            {
                                if(selectStatement.UnionStatements!=null && selectStatement.UnionStatements.Count>0)
                                {
                                    lastSelectStatement = selectStatement.UnionStatements.Last().SelectStatement;
                                }

                                lastSelectStatement.LimitInfo = new SelectLimitInfo() { StartRowIndex = new TokenInfo(offset.ToString()), RowCount = new TokenInfo(pageSize.ToString()) };
                            };

                            Action useSpecialPaination = () =>
                            {
                                isUseSpecialPaination = true;

                                string orderBy = selectStatement.OrderBy == null ? "": string.Join(",", selectStatement.OrderBy.Select(item => item.Symbol));

                                ColumnName columnName = new ColumnName($"ROW_NUMBER() OVER (ORDER BY {orderBy}) AS {rowNumberColumnName}");

                                selectStatement.Columns.Add(columnName);
                            };

                            if (databaseType == DatabaseType.MySql || databaseType == DatabaseType.Postgres || databaseType == DatabaseType.Sqlite)
                            {
                                useNormalPagination();
                            }
                            else if (databaseType == DatabaseType.SqlServer)
                            {
                                if (!dbInterpreter.IsLowDbVersion(dbInterpreter.ServerVersion, "11"))
                                {
                                    useNormalPagination();
                                }
                                else
                                {
                                    useSpecialPaination();
                                }
                            }
                            else if (databaseType == DatabaseType.Oracle)
                            {
                                if (!dbInterpreter.IsLowDbVersion(dbInterpreter.ServerVersion, "12.1"))
                                {
                                    useNormalPagination();
                                }
                                else
                                {
                                    useSpecialPaination();
                                }
                            }

                            ScriptBuildFactory scriptBuildFactory = TranslateHelper.GetScriptBuildFactory(databaseType);

                            script = scriptBuildFactory.GenerateScripts(cs).Script;

                            string specialPaginationScript = null;

                            if (isUseSpecialPaination)
                            {
                                specialPaginationScript = $"{script.Trim().TrimEnd(';')}";

                                var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

                                script = $@"SELECT * FROM
                               (
                                 {specialPaginationScript}
                               ) TEMP
                               WHERE {rowNumberColumnName} BETWEEN {startEndRowNumber.StartRowNumber} AND {startEndRowNumber.EndRowNumber}";
                            }

                            if (hasTableName || hasAlias)
                            {
                                List<List<ColumnName>> columnsList = new List<List<ColumnName>>();
                                List<List<TokenInfo>> orderByList = new List<List<TokenInfo>>();

                                SelectLimitInfo limitInfo = new SelectLimitInfo() { StartRowIndex = lastSelectStatement.LimitInfo?.StartRowIndex, RowCount = lastSelectStatement.LimitInfo?.RowCount };

                                List<SelectStatement> statements = new List<SelectStatement>();

                                statements.Add(selectStatement);

                                if (selectStatement.UnionStatements!=null && selectStatement.UnionStatements.Count>0)
                                {
                                    statements.AddRange(selectStatement.UnionStatements.Select(item => item.SelectStatement));
                                }

                                int i = 0;

                                foreach(var statement in statements)
                                {
                                    columnsList.Add(selectStatement.Columns.Select(item => item).ToList());
                                    orderByList.Add(selectStatement.OrderBy == null ? null : selectStatement.OrderBy.Select(item => item).ToList());

                                    statement.Columns.Clear();
                                    statement.Columns.Add(new ColumnName("COUNT(1)"));
                                    statement.OrderBy = null;
                                    statement.LimitInfo = null;

                                    i++;
                                }

                                analyseResult.CountScript = scriptBuildFactory.GenerateScripts(cs).Script;

                                i = 0;
                                foreach (var statement in statements)
                                {
                                    selectStatement.Columns.Clear();
                                    selectStatement.Columns = columnsList[i];
                                    selectStatement.OrderBy = orderByList[i];

                                    i++;
                                }                                   

                                lastSelectStatement.LimitInfo = limitInfo;
                            }

                            analyseResult.SelectStatement = selectStatement;
                            analyseResult.SpecialPaginationScript = specialPaginationScript;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            analyseResult.Script = script;

            return analyseResult;
        }

        public static async Task<DataTable> GetPagedDatatable(DbInterpreter dbInterpreter, SelectScriptAnalyseResult selectScriptAnalyseResult, PaginationInfo paginationInfo, CancellationToken cancellationToken)
        {
            SelectStatement selectStatement = selectScriptAnalyseResult.SelectStatement;

            int pageSize = paginationInfo.PageSize;
            long pageNumber = paginationInfo.PageNumber;
            long offset = (pageNumber - 1) * pageSize;

            string sql = null;

            if (selectScriptAnalyseResult.SpecialPaginationScript == null)
            {
                selectStatement.LimitInfo = new SelectLimitInfo() { StartRowIndex = new TokenInfo(offset.ToString()), RowCount = new TokenInfo(pageSize.ToString()) };

                CommonScript commonScript = new CommonScript();
                commonScript.Statements.Add(selectStatement);

                ScriptBuildFactory scriptBuildFactory = TranslateHelper.GetScriptBuildFactory(dbInterpreter.DatabaseType);

                sql = scriptBuildFactory.GenerateScripts(commonScript).Script;

                sql = GetTrimedScript(dbInterpreter, sql);
            }
            else
            {
                var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

                sql = $@"SELECT * FROM
                               (
                                 {selectScriptAnalyseResult.SpecialPaginationScript}
                               ) TEMP
                               WHERE {DbInterpreter.RowNumberColumnName} BETWEEN {startEndRowNumber.StartRowNumber} AND {startEndRowNumber.EndRowNumber}";
            }

            using (DbConnection dbConnection = dbInterpreter.CreateConnection())
            {
                DataTable dataTable = await dbInterpreter.GetDataTableAsync(dbConnection, sql, cancellationToken, true);

                return dataTable;
            }
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
