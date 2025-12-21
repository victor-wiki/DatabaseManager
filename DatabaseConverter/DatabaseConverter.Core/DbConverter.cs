using DatabaseConverter.Model;
using DatabaseConverter.Profile;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseConverter.Core
{
    public class DbConverter : IDisposable
    {
        private bool hasError = false;
        private bool isBusy = false;
        private IObserver<FeedbackInfo> observer;
        private DbTransaction transaction = null;
        private DatabaseObject translateDbObject = null;

        public bool HasError => this.hasError;
        public bool IsBusy => this.isBusy;
        public readonly string TableDataSyncProgressMessagePrefixFormat = "Table \"{0}\":";

        public DbConveterInfo Source { get; set; }
        public DbConveterInfo Target { get; set; }

        public DbConverterOption Option { get; set; } = new DbConverterOption();

        public event FeedbackHandler OnFeedback;

        public DbConverter(DbConveterInfo source, DbConveterInfo target)
        {
            this.Source = source;
            this.Target = target;
        }

        public DbConverter(DbConveterInfo source, DbConveterInfo target, DbConverterOption option)
        {
            this.Source = source;
            this.Target = target;

            if (option != null)
            {
                this.Option = option;
            }
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public Task<DbConvertResult> Translate(DatabaseObject dbObject, CancellationToken cancellationToken)
        {
            this.translateDbObject = dbObject;

            SchemaInfo schemaInfo = SchemaInfoHelper.GetSchemaInfoByDbObject(dbObject);

            return this.InternalConvert(cancellationToken, schemaInfo, dbObject.Schema);
        }

        public Task<DbConvertResult> Convert(CancellationToken cancellationToken, SchemaInfo schemaInfo = null, string schema = null, SchemaInfo targetSchemaInfo = null)
        {
            return this.InternalConvert(cancellationToken, schemaInfo, schema, targetSchemaInfo);
        }

        private async Task<DbConvertResult> InternalConvert(CancellationToken cancellationToken, SchemaInfo schemaInfo = null, string schema = null, SchemaInfo targetSchemaInfo = null)
        {
            this.isBusy = false;
            this.hasError = false;

            DbConvertResult result = new DbConvertResult();

            bool hasTargetSchemaInfo = targetSchemaInfo != null;

            bool continuedWhenErrorOccured = false;

            GenerateScriptMode mode = this.Option.GenerateScriptMode;

            bool schemaModeOnly = mode == GenerateScriptMode.Schema;
            bool dataModeOnly = mode == GenerateScriptMode.Data;

            bool onlyForTranslate = this.Option.OnlyForTranslate;
            bool onlyForTableCopy = this.Option.OnlyForTableCopy;
            bool executeScriptOnTargetServer = this.Option.ExecuteScriptOnTargetServer;

            DbInterpreter sourceDbInterpreter = this.Source.DbInterpreter;
            DbInterpreter targetDbInterpreter = this.Target.DbInterpreter;

            sourceDbInterpreter.Subscribe(this.observer);
            targetDbInterpreter.Subscribe(this.observer);

            var sourceDbType = sourceDbInterpreter.DatabaseType;
            var targetDbType = targetDbInterpreter.DatabaseType;

            var sourceInterpreterOption = sourceDbInterpreter.Option;
            var targetInterpreterOption = targetDbInterpreter.Option;

            sourceInterpreterOption.BulkCopy = this.Option.BulkCopy;
            sourceInterpreterOption.GetTableAllObjects = false;
            targetInterpreterOption.GetTableAllObjects = false;
            targetInterpreterOption.TableScriptsGenerateOption.ExecuteScriptOnServer = executeScriptOnTargetServer;

            targetInterpreterOption.ObjectFetchMode = DatabaseObjectFetchMode.Simple;

            this.isBusy = true;

            #region Schema filter

            DatabaseObjectType databaseObjectType = (DatabaseObjectType)Enum.GetValues(typeof(DatabaseObjectType)).Cast<int>().Sum();

            if (schemaInfo != null && !sourceInterpreterOption.GetTableAllObjects
                && (schemaInfo.TableTriggers == null || schemaInfo.TableTriggers.Count == 0))
            {
                databaseObjectType = databaseObjectType ^ DatabaseObjectType.Trigger;
            }

            if (this.Source.DatabaseObjectType != DatabaseObjectType.None)
            {
                databaseObjectType = databaseObjectType & this.Source.DatabaseObjectType;
            }

            if (this.Target.DbInterpreter.Option.TableScriptsGenerateOption.GenerateConstraint == false)
            {
                databaseObjectType = databaseObjectType ^ DatabaseObjectType.Constraint;
            }

            SchemaInfoFilter filter = new SchemaInfoFilter() { Strict = true, DatabaseObjectType = databaseObjectType };

            if (schema != null)
            {
                filter.Schema = schema;
            }

            SchemaInfoHelper.SetSchemaInfoFilterValues(filter, schemaInfo);

            #endregion

            SchemaInfo sourceSchemaInfo = await sourceDbInterpreter.GetSchemaInfoAsync(filter);

            if (this.hasError)
            {
                result.InfoType = DbConvertResultInfoType.Error;
                result.Message = "Source database interpreter has error occured.";
                return result;
            }

            sourceSchemaInfo.TableColumns = DbObjectHelper.ResortTableColumns(sourceSchemaInfo.Tables, sourceSchemaInfo.TableColumns);

            #region Check whether database objects already existed.

            List<TableColumn> existedTableColumns = null;

            if (DbInterpreter.Setting.NotCreateIfExists && !onlyForTranslate && !onlyForTableCopy)
            {
                if (!dataModeOnly)
                {
                    targetInterpreterOption.ObjectFetchMode = DatabaseObjectFetchMode.Simple;

                    SchemaInfo targetSchema = await targetDbInterpreter.GetSchemaInfoAsync(filter);

                    existedTableColumns = targetSchema.TableColumns.Where(item => sourceSchemaInfo.TableColumns.Any(t => SchemaInfoHelper.IsSameTableColumnIgnoreCase(t, item))).ToList();

                    SchemaInfoHelper.ExcludeExistingObjects(sourceSchemaInfo, targetSchema);
                }
            }

            if (!onlyForTranslate && existedTableColumns == null && !schemaModeOnly)
            {
                SchemaInfoFilter columnFilter = new SchemaInfoFilter() { TableNames = filter.TableNames };

                existedTableColumns = await targetDbInterpreter.GetTableColumnsAsync(columnFilter);
            }
            #endregion

            #region User defined type handle
            List<UserDefinedType> utypes = new List<UserDefinedType>();

            if (!dataModeOnly)
            {
                if (sourceDbType != targetDbType)
                {
                    utypes = await sourceDbInterpreter.GetUserDefinedTypesAsync();

                    if (this.Option.UseOriginalDataTypeIfUdtHasOnlyOneAttr)
                    {
                        if (utypes != null && utypes.Count > 0)
                        {
                            foreach (TableColumn column in sourceSchemaInfo.TableColumns)
                            {
                                UserDefinedType utype = utypes.FirstOrDefault(item => item.Name == column.DataType);

                                if (utype != null && utype.Attributes.Count == 1)
                                {
                                    var attr = utype.Attributes.First();

                                    column.DataType = attr.DataType;
                                    column.MaxLength = attr.MaxLength;
                                    column.Precision = attr.Precision;
                                    column.Scale = attr.Scale;
                                    column.IsUserDefined = false;
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            if (!hasTargetSchemaInfo)
            {
                targetSchemaInfo = SchemaInfoHelper.Clone(sourceSchemaInfo);

                #region Table copy handle

                if (onlyForTableCopy)
                {
                    if (this.Source.TableNameMappings != null && this.Source.TableNameMappings.Count > 0)
                    {
                        SchemaInfoHelper.MapTableNames(targetSchemaInfo, this.Source.TableNameMappings);
                    }

                    if (this.Option.RenameTableChildren)
                    {
                        SchemaInfoHelper.RenameTableChildren(targetSchemaInfo);
                    }

                    if (this.Option.IgnoreNotSelfForeignKey)
                    {
                        targetSchemaInfo.TableForeignKeys = targetSchemaInfo.TableForeignKeys.Where(item => item.TableName == item.ReferencedTableName).ToList();
                    }
                }

                #endregion
            }

            DbScriptGenerator targetDbScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(targetDbInterpreter);

            #region Create schema if not exists

            if (!dataModeOnly && this.Option.CreateSchemaIfNotExists && (targetDbType == DatabaseType.SqlServer || targetDbType == DatabaseType.Postgres))
            {
                using (DbConnection dbConnection = targetDbInterpreter.CreateConnection())
                {
                    if (dbConnection.State != ConnectionState.Open)
                    {
                        await dbConnection.OpenAsync();
                    }

                    #region Schema handle
                    if (sourceDbType != DatabaseType.Sqlite)
                    {
                        var sourceSchemas = (await sourceDbInterpreter.GetDatabaseSchemasAsync()).Select(item => item.Name);
                        var targetSchemas = (await targetDbInterpreter.GetDatabaseSchemasAsync(dbConnection)).Select(item => item.Name);

                        var notExistsSchemas = sourceSchemas.Where(item => item != sourceDbInterpreter.DefaultSchema).Select(item => item)
                             .Union(this.Option.SchemaMappings.Select(item => item.TargetSchema))
                             .Except(targetSchemas.Select(item => item)).Distinct();

                        foreach (var schemaName in notExistsSchemas)
                        {
                            string createSchemaScript = targetDbScriptGenerator.CreateSchema(new DatabaseSchema() { Name = schemaName }).Content;

                            await targetDbInterpreter.ExecuteNonQueryAsync(dbConnection, GetCommandInfo(createSchemaScript, cancellationToken));
                        }

                        if (this.Option.SchemaMappings.Count == 1 && this.Option.SchemaMappings.First().SourceSchema == "")
                        {
                            this.Option.SchemaMappings.Clear();
                        }

                        foreach (var ss in sourceSchemas)
                        {
                            string mappedSchema = SchemaInfoHelper.GetMappedSchema(ss, this.Option.SchemaMappings);

                            if (string.IsNullOrEmpty(mappedSchema))
                            {
                                string targetSchema = ss == sourceDbInterpreter.DefaultSchema ? targetDbInterpreter.DefaultSchema : ss;

                                this.Option.SchemaMappings.Add(new SchemaMappingInfo() { SourceSchema = ss, TargetSchema = targetSchema });
                            }
                        }
                    }
                    #endregion
                }
            }

            #endregion

            if (!hasTargetSchemaInfo)
            {
                this.ConvertSchema(sourceDbInterpreter, targetDbInterpreter, targetSchemaInfo);

                #region Translate

                if (!dataModeOnly)
                {
                    TranslateEngine translateEngine = new TranslateEngine(sourceSchemaInfo, targetSchemaInfo, sourceDbInterpreter, targetDbInterpreter, this.Option);

                    translateEngine.ContinueWhenErrorOccurs = this.Option.ContinueWhenErrorOccurs || (!executeScriptOnTargetServer && !onlyForTranslate);

                    DatabaseObjectType translateDbObjectType = TranslateEngine.SupportDatabaseObjectType;

                    translateEngine.UserDefinedTypes = utypes;
                    translateEngine.ExistedTableColumns = existedTableColumns;

                    translateEngine.Subscribe(this.observer);

                    await Task.Run(() => translateEngine.Translate(translateDbObjectType), cancellationToken);

                    result.TranslateResults = translateEngine.TranslateResults;
                    result.TranslatedSchemaInfo = targetSchemaInfo;

                    if (this.Option.NeedPreview)
                    {
                        return result;
                    }
                }

                #endregion
            }

            #region Handle names of primary key and index

            if (!dataModeOnly && targetSchemaInfo.Tables.Any())
            {
                if (this.Option.EnsurePrimaryKeyNameUnique)
                {
                    SchemaInfoHelper.EnsurePrimaryKeyNameUnique(targetSchemaInfo);

                    if (sourceDbType == DatabaseType.MySql)
                    {
                        SchemaInfoHelper.ForceRenameMySqlPrimaryKey(targetSchemaInfo);
                    }
                }

                if (this.Option.EnsureIndexNameUnique)
                {
                    SchemaInfoHelper.EnsureIndexNameUnique(targetSchemaInfo);
                }
            }

            #endregion

            bool generateIdentity = targetInterpreterOption.TableScriptsGenerateOption.GenerateIdentity;

            string script = "";

            DataTransferErrorProfile dataErrorProfile = null;

            Script currentScript = null;

            using (DbConnection dbConnection = executeScriptOnTargetServer ? targetDbInterpreter.CreateConnection() : null)
            {
                ScriptBuilder scriptBuilder = null;

                if (!dataModeOnly)
                {
                    #region Oracle name length of low database version handle
                    if (targetDbType == DatabaseType.Oracle)
                    {
                        if (!onlyForTranslate)
                        {
                            if (executeScriptOnTargetServer)
                            {
                                string serverVersion = targetDbInterpreter.ConnectionInfo?.ServerVersion;
                                bool isLowDbVersion = !string.IsNullOrEmpty(serverVersion) ? targetDbInterpreter.IsLowDbVersion() : targetDbInterpreter.IsLowDbVersion(dbConnection);

                                if (isLowDbVersion)
                                {
                                    SchemaInfoHelper.RistrictNameLength(targetSchemaInfo, 30);
                                }
                            }
                        }
                    }
                    #endregion

                    scriptBuilder = targetDbScriptGenerator.GenerateSchemaScripts(targetSchemaInfo);

                    if (onlyForTranslate)
                    {
                        if (!(this.translateDbObject is ScriptDbObject)) //script db object uses script translator which uses event to feed back to ui.
                        {
                            result.TranslateResults.Add(new TranslateResult() { DbObjectType = DbObjectHelper.GetDatabaseObjectType(this.translateDbObject), DbObjectName = this.translateDbObject.Name, Data = scriptBuilder.ToString() });
                        }
                    }
                }

                if (onlyForTranslate)
                {
                    result.InfoType = DbConvertResultInfoType.Information;
                    result.Message = "Translate has finished";

                    return result;
                }

                bool canCommit = false;

                if (executeScriptOnTargetServer)
                {
                    if (dbConnection.State != ConnectionState.Open)
                    {
                        await dbConnection.OpenAsync();
                    }

                    if (this.Option.UseTransaction)
                    {
                        this.transaction = await dbConnection.BeginTransactionAsync();
                        canCommit = true;
                    }
                }

                #region Schema sync             

                if (scriptBuilder != null && executeScriptOnTargetServer)
                {
                    List<Script> scripts = scriptBuilder.Scripts;

                    if (scripts.Count == 0)
                    {
                        this.Feedback(targetDbInterpreter, $"The script to create schema is empty.", FeedbackInfoType.Info);

                        this.isBusy = false;

                        result.InfoType = DbConvertResultInfoType.Information;
                        result.Message = "No any script to execute.";

                        return result;
                    }

                    targetDbInterpreter.Feedback(FeedbackInfoType.Info, "Begin to sync schema...");

                    try
                    {
                        if (!this.Option.SplitScriptsToExecute)
                        {
                            targetDbInterpreter.Feedback(FeedbackInfoType.Info, script);

                            await targetDbInterpreter.ExecuteNonQueryAsync(dbConnection, GetCommandInfo(script, cancellationToken, null, this.transaction));
                        }
                        else
                        {
                            Func<Script, bool> isValidScript = (s) =>
                            {
                                return !(s is NewLineScript || s is SpliterScript || string.IsNullOrEmpty(s.Content) || s.Content == targetDbInterpreter.ScriptsDelimiter);
                            };

                            int count = scripts.Where(item => isValidScript(item)).Count();
                            int i = 0;

                            foreach (Script s in scripts)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    this.Rollback();

                                    break;
                                }

                                currentScript = s;

                                bool isView = s.ObjectType == nameof(View);
                                bool isRoutineScript = this.IsRoutineScript(s);
                                bool isRoutineScriptOrView = isRoutineScript || isView;

                                if (!isValidScript(s))
                                {
                                    continue;
                                }

                                string sql = s.Content?.Trim();

                                if (!string.IsNullOrEmpty(sql) && sql != targetDbInterpreter.ScriptsDelimiter)
                                {
                                    i++;

                                    if (!isRoutineScript && targetDbInterpreter.ScriptsDelimiter.Length == 1 && sql.EndsWith(targetDbInterpreter.ScriptsDelimiter))
                                    {
                                        sql = sql.TrimEnd(targetDbInterpreter.ScriptsDelimiter.ToArray());
                                    }

                                    if (!this.hasError || (isRoutineScriptOrView && this.Option.ContinueWhenErrorOccurs))
                                    {
                                        targetDbInterpreter.Feedback(FeedbackInfoType.Info, $"({i}/{count}), executing:{Environment.NewLine} {sql}");

                                        CommandInfo commandInfo = GetCommandInfo(sql, cancellationToken, null, transaction);

                                        commandInfo.ContinueWhenErrorOccurs = isRoutineScriptOrView && this.Option.ContinueWhenErrorOccurs;

                                        ExecuteResult executeResult = await targetDbInterpreter.ExecuteNonQueryAsync(dbConnection, commandInfo);

                                        if (executeResult.HasError)
                                        {
                                            this.hasError = true;

                                            if (!this.Option.ContinueWhenErrorOccurs)
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                if (isRoutineScriptOrView)
                                                {
                                                    continuedWhenErrorOccured = true;
                                                }
                                            }
                                        }

                                        if (executeResult.TransactionRollbacked)
                                        {
                                            canCommit = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Rollback(ex);

                        ConnectionInfo sourceConnectionInfo = sourceDbInterpreter.ConnectionInfo;
                        ConnectionInfo targetConnectionInfo = targetDbInterpreter.ConnectionInfo;

                        SchemaTransferException schemaTransferException = new SchemaTransferException(ex)
                        {
                            SourceServer = sourceConnectionInfo.Server,
                            SourceDatabase = sourceConnectionInfo.Database,
                            TargetServer = targetConnectionInfo.Server,
                            TargetDatabase = targetConnectionInfo.Database
                        };

                        var res = this.HandleError(schemaTransferException);

                        result.InfoType = res.InfoType;
                        result.Message = res.Message;

                        if (ex is DbCommandException dce)
                        {
                            result.ExceptionType = dce.BaseException?.GetType();
                        }
                        else
                        {
                            result.ExceptionType = ex.GetType();
                        }
                    }

                    targetDbInterpreter.Feedback(FeedbackInfoType.Info, "End sync schema.");
                }

                #endregion

                #region Data sync

                if (!schemaModeOnly)
                {
                    await this.SyncData(sourceDbInterpreter, targetDbInterpreter, dbConnection, sourceSchemaInfo, targetSchemaInfo, targetDbScriptGenerator, result, cancellationToken);
                }

                #endregion

                if (!this.hasError && this.transaction != null && this.transaction.Connection != null && !cancellationToken.IsCancellationRequested && canCommit)
                {
                    await this.transaction.CommitAsync();
                }

                this.isBusy = false;
            }

            if (dataErrorProfile != null && !this.hasError && !cancellationToken.IsCancellationRequested)
            {
                DataTransferErrorProfileManager.Remove(dataErrorProfile);
            }

            if (this.hasError)
            {
                if (continuedWhenErrorOccured)
                {
                    result.InfoType = DbConvertResultInfoType.Warnning;
                    result.Message = $"Convert has finished,{Environment.NewLine}but some errors occured.";
                }
                else
                {
                    result.InfoType = DbConvertResultInfoType.Error;

                    if (string.IsNullOrEmpty(result.Message))
                    {
                        result.Message = "Convert failed.";
                    }
                }
            }
            else
            {
                result.InfoType = DbConvertResultInfoType.Information;

                if (this.Option.ExecuteScriptOnTargetServer
                    || this.Target.DbInterpreter.Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile)
                   )
                {
                    result.Message = "Convert has finished.";
                }
                else
                {
                    result.Message = "No action was taken.";
                }
            }

            return result;
        }

        private async Task SyncData(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, DbConnection dbConnection, SchemaInfo sourceSchemaInfo, SchemaInfo targetSchemaInfo, DbScriptGenerator targetDbScriptGenerator, DbConvertResult result, CancellationToken cancellationToken)
        {
            var sourceDbType = sourceDbInterpreter.DatabaseType;
            var targetDbType = targetDbInterpreter.DatabaseType;

            GenerateScriptMode mode = this.Option.GenerateScriptMode;

            bool schemaModeOnly = mode == GenerateScriptMode.Schema;
            bool dataModeOnly = mode == GenerateScriptMode.Data;

            var sourceInterpreterOption = sourceDbInterpreter.Option;
            var targetInterpreterOption = targetDbInterpreter.Option;

            bool executeScriptOnTargetServer = this.Option.ExecuteScriptOnTargetServer;
            bool generateIdentity = targetInterpreterOption.TableScriptsGenerateOption.GenerateIdentity;

            if (!this.hasError && !schemaModeOnly && sourceSchemaInfo.Tables.Count > 0)
            {
                if (targetDbType == DatabaseType.Oracle && generateIdentity)
                {
                    if (!targetDbInterpreter.IsLowDbVersion())
                    {
                        sourceDbInterpreter.Option.ExcludeIdentityForData = true;
                        targetDbInterpreter.Option.ExcludeIdentityForData = true;
                    }
                }

                List<TableColumn> identityTableColumns = new List<TableColumn>();

                if (generateIdentity)
                {
                    identityTableColumns = targetSchemaInfo.TableColumns.Where(item => item.IsIdentity).ToList();
                }

                //await this.SetIdentityEnabled(identityTableColumns, targetDbInterpreter, targetDbScriptGenerator, dbConnection, transaction, false);

                if (executeScriptOnTargetServer || targetDbInterpreter.Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
                {
                    Dictionary<Table, long> dictTableDataTransferredCount = new Dictionary<Table, long>();

                    sourceDbInterpreter.OnDataRead += async (TableDataReadInfo tableDataReadInfo) =>
                    {
                        if (!this.hasError)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                this.Rollback();
                                return;
                            }

                            Table table = tableDataReadInfo.Table;
                            List<TableColumn> columns = tableDataReadInfo.Columns;

                            try
                            {
                                string targetTableSchema = this.Option.SchemaMappings.Where(item => item.SourceSchema == table.Schema).FirstOrDefault()?.TargetSchema;

                                if (string.IsNullOrEmpty(targetTableSchema))
                                {
                                    targetTableSchema = targetDbInterpreter.DefaultSchema;
                                }

                                (Table Table, List<TableColumn> Columns) targetTableAndColumns = this.GetTargetTableColumns(targetSchemaInfo, targetTableSchema, table, columns);

                                if (targetTableAndColumns.Table == null || targetTableAndColumns.Columns == null)
                                {
                                    return;
                                }

                                if (executeScriptOnTargetServer)
                                {
                                    DataTable dataTable = tableDataReadInfo.DataTable;

                                    await CopyData(sourceDbInterpreter, targetDbInterpreter, dbConnection, targetDbScriptGenerator, table, columns, targetTableAndColumns.Table, targetTableAndColumns.Columns,
                                        dataTable, this.Option.BulkCopy, cancellationToken, this.Option.SchemaMappings, this.Source.TableNameMappings, this.transaction);

                                    if (!dictTableDataTransferredCount.ContainsKey(table))
                                    {
                                        dictTableDataTransferredCount.Add(table, dataTable.Rows.Count);
                                    }
                                    else
                                    {
                                        dictTableDataTransferredCount[table] += dataTable.Rows.Count;
                                    }

                                    long transferredCount = dictTableDataTransferredCount[table];

                                    double percent = (transferredCount * 1.0 / tableDataReadInfo.TotalCount) * 100;

                                    string strPercent = (percent == (int)percent) ? (percent + "%") : (percent / 100).ToString("P2");

                                    targetDbInterpreter.FeedbackProgress($"{(string.Format(this.TableDataSyncProgressMessagePrefixFormat, table.Name))}Transferred {transferredCount}/{tableDataReadInfo.TotalCount}, {strPercent}.", table.Name);
                                }
                                else
                                {
                                    List<Dictionary<string, object>> data = sourceDbInterpreter.ConvertDataTableToDictionaryList(tableDataReadInfo.DataTable, columns);

                                    GenerateScripts(targetDbScriptGenerator, targetTableAndColumns.Table, targetTableAndColumns.Columns, data);
                                }
                            }
                            catch (Exception ex)
                            {
                                this.Rollback(ex);

                                ConnectionInfo sourceConnectionInfo = sourceDbInterpreter.ConnectionInfo;
                                ConnectionInfo targetConnectionInfo = targetDbInterpreter.ConnectionInfo;

                                string mappedTableName = GetMappedTableName(table.Name, this.Source.TableNameMappings);

                                DataTransferException dataTransferException = new DataTransferException(ex)
                                {
                                    SourceServer = sourceConnectionInfo.Server,
                                    SourceDatabase = sourceConnectionInfo.Database,
                                    SourceObject = table.Name,
                                    TargetServer = targetConnectionInfo.Server,
                                    TargetDatabase = targetConnectionInfo.Database,
                                    TargetObject = mappedTableName
                                };

                                if (!this.Option.UseTransaction)
                                {
                                    DataTransferErrorProfileManager.Save(new DataTransferErrorProfile
                                    {
                                        SourceServer = sourceConnectionInfo.Server,
                                        SourceDatabase = sourceConnectionInfo.Database,
                                        SourceTableName = table.Name,
                                        TargetServer = targetConnectionInfo.Server,
                                        TargetDatabase = targetConnectionInfo.Database,
                                        TargetTableName = mappedTableName
                                    });
                                }

                                var res = this.HandleError(dataTransferException);

                                result.InfoType = DbConvertResultInfoType.Error;
                                result.Message = res.Message;
                            }
                        }
                    };
                }

                DbScriptGenerator sourceDbScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(sourceDbInterpreter);

                await sourceDbScriptGenerator.GenerateDataScriptsAsync(sourceSchemaInfo);

                //await this.SetIdentityEnabled(identityTableColumns, targetDbInterpreter, targetDbScriptGenerator, dbConnection, transaction, true);
            }
        }

        public static async Task CopyData(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, DbConnection dbConnection, DbScriptGenerator dbScriptGenerator, Table sourceTable, List<TableColumn> sourceTableColumns,
            Table targetTable, List<TableColumn> targetTableColumns, DataTable dataTable, bool useBulkCopy, CancellationToken cancellationToken,
            List<SchemaMappingInfo> schemaMappings = null, Dictionary<string, string> tableNameMappings = null, DbTransaction transaction = null)
        {
            DatabaseType targetDbType = targetDbInterpreter.DatabaseType;

            List<Dictionary<string, object>> data = sourceDbInterpreter.ConvertDataTableToDictionaryList(dataTable, sourceTableColumns);

            if (useBulkCopy && targetDbInterpreter.SupportBulkCopy)
            {
                BulkCopyInfo bulkCopyInfo = GetBulkCopyInfo(sourceDbInterpreter, targetDbInterpreter, sourceTable, targetTableColumns, cancellationToken, schemaMappings, tableNameMappings, transaction);

                if (targetDbType == DatabaseType.Oracle)
                {
                    if (sourceTableColumns.Any(item => item.DataType.ToLower().Contains("datetime2") || item.DataType.ToLower().Contains("timestamp")))
                    {
                        bulkCopyInfo.DetectDateTimeTypeByValues = true;
                    }
                }

                await targetDbInterpreter.BulkCopyAsync(dbConnection, dataTable, bulkCopyInfo);
            }
            else
            {
                (Dictionary<string, object> Parameters, string Script) scriptResult = GenerateScripts(dbScriptGenerator, targetTable, targetTableColumns, data);

                string script = scriptResult.Script;

                string delimiter = ");" + Environment.NewLine;

                if (!script.Contains(delimiter))
                {
                    await targetDbInterpreter.ExecuteNonQueryAsync(dbConnection, GetCommandInfo(script, cancellationToken, scriptResult.Parameters, transaction));
                }
                else
                {
                    var items = script.Split(delimiter);

                    List<string> insertItems = new List<string>();

                    foreach (var item in items)
                    {
                        if (item.Trim().ToUpper().StartsWith("INSERT INTO "))
                        {
                            insertItems.Add(item);
                        }
                        else
                        {
                            if (insertItems.Any())
                            {
                                insertItems[insertItems.Count - 1] += (delimiter + item);
                            }
                        }
                    }

                    int count = 0;

                    foreach (var item in insertItems)
                    {
                        count++;

                        var cmd = count < insertItems.Count ? (item + delimiter).Trim().Trim(';') : item;

                        await targetDbInterpreter.ExecuteNonQueryAsync(dbConnection, GetCommandInfo(cmd, cancellationToken, scriptResult.Parameters, transaction));
                    }
                }
            }
        }

        private bool IsRoutineScript(Script script)
        {
            return script.ObjectType == nameof(Function) || script.ObjectType == nameof(Procedure) || script.ObjectType == nameof(TableTrigger);
        }

        public static (Dictionary<string, object> Parameters, string Script) GenerateScripts(DbScriptGenerator targetDbScriptGenerator, Table table, List<TableColumn> columns, List<Dictionary<string, object>> data)
        {
            StringBuilder sb = new StringBuilder();

            Dictionary<string, object> parameters = targetDbScriptGenerator.AppendDataScripts(sb, table, columns, new Dictionary<long, List<Dictionary<string, object>>>() { { 1, data } });

            string script = sb.ToString().Trim().Trim(';');

            return (parameters, script);
        }

        #region Not use it currently, because bulkcopy doesn't care identity and insert script has excluded the identity columns.
        private async Task SetIdentityEnabled(IEnumerable<TableColumn> identityTableColumns, DbInterpreter dbInterpreter, DbScriptGenerator scriptGenerator,
                                             DbConnection dbConnection, DbTransaction transaction, bool enabled, CancellationToken cancellationToken)
        {
            foreach (var item in identityTableColumns)
            {
                string sql = scriptGenerator.SetIdentityEnabled(item, enabled).Content;

                if (!string.IsNullOrEmpty(sql))
                {
                    CommandInfo commandInfo = GetCommandInfo(sql, cancellationToken, null, transaction);
                    commandInfo.ContinueWhenErrorOccurs = true;

                    await dbInterpreter.ExecuteNonQueryAsync(dbConnection, commandInfo);
                }
            }
        }
        #endregion

        private void Rollback(Exception ex = null)
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
                        this.transaction.Rollback();
                    }
                }
                catch (Exception e)
                {
                    //throw;
                }
            }
        }

        private static CommandInfo GetCommandInfo(string commandText, CancellationToken cancellationToken, Dictionary<string, object> parameters = null, DbTransaction transaction = null)
        {
            CommandInfo commandInfo = new CommandInfo()
            {
                CommandType = CommandType.Text,
                CommandText = commandText,
                Parameters = parameters,
                Transaction = transaction,
                CancellationToken = cancellationToken
            };

            return commandInfo;
        }

        private static BulkCopyInfo GetBulkCopyInfo(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, Table table, List<TableColumn> tableColumns, CancellationToken cancellationToken, List<SchemaMappingInfo> schemaMappings = null, Dictionary<string, string> tableNameMappings = null, DbTransaction transaction = null)
        {
            string tableName = table.Name;

            if (tableNameMappings != null)
            {
                tableName = GetMappedTableName(table.Name, tableNameMappings);
            }

            BulkCopyInfo bulkCopyInfo = new BulkCopyInfo()
            {
                SourceDatabaseType = sourceDbInterpreter.DatabaseType,
                DestinationTableName = tableName,
                Transaction = transaction,
                CancellationToken = cancellationToken
            };

            string mappedSchema = null;

            if (schemaMappings != null)
            {
                mappedSchema = SchemaInfoHelper.GetMappedSchema(table.Schema, schemaMappings);
            }

            if (mappedSchema == null)
            {
                mappedSchema = targetDbInterpreter.DefaultSchema;
            }

            if (mappedSchema != null)
            {
                bulkCopyInfo.DestinationTableSchema = mappedSchema;

                bulkCopyInfo.Columns = tableColumns.Where(item => item.TableName == tableName && item.Schema == mappedSchema);
            }
            else
            {
                bulkCopyInfo.Columns = tableColumns.Where(item => item.TableName == tableName);
            }

            return bulkCopyInfo;
        }

        private static string GetMappedTableName(string tableName, Dictionary<string, string> tableNameMappings = null)
        {
            if (tableNameMappings != null)
            {
                return SchemaInfoHelper.GetMappedTableName(tableName, tableNameMappings);
            }

            return null;
        }

        public static async Task<(List<string> SourceSchemas, List<string> TargetSchemas)> GetSourceAndTargetSchemas(DatabaseType sourceDbType, DatabaseType targetDbType, ConnectionInfo sourceDbConnectionInfo, ConnectionInfo targetDbConnectionInfo)
        {
            DbInterpreterOption option = new DbInterpreterOption() { };

            DbInterpreter sourceInterpreter = DbInterpreterHelper.GetDbInterpreter(sourceDbType, sourceDbConnectionInfo, option);
            DbInterpreter targetInterpreter = DbInterpreterHelper.GetDbInterpreter(targetDbType, targetDbConnectionInfo, option);

            List<string> sourceSchemas = new List<string>();
            List<string> targetSchemas = new List<string>();

            try
            {
                sourceSchemas = (await sourceInterpreter.GetDatabaseSchemasAsync()).Select(item => item.Name).ToList();
                targetSchemas = (await targetInterpreter.GetDatabaseSchemasAsync()).Select(item => item.Name).ToList();
            }
            catch (Exception ex)
            {
            }

            return (sourceSchemas, targetSchemas);
        }

        public static List<SchemaMappingInfo> GetAutoMappedSchemas(List<string> sourceSchemas, List<string> targetSchemas)
        {
            List<SchemaMappingInfo> schemaMappingInfos = new List<SchemaMappingInfo>();

            if (sourceSchemas != null && targetSchemas != null)
            {
                foreach (string sourceSchema in sourceSchemas)
                {
                    string targetSchema = targetSchemas.FirstOrDefault(item => item.ToUpper() == sourceSchema.ToUpper());

                    if (targetSchema != null)
                    {
                        schemaMappingInfos.Add(new SchemaMappingInfo() { SourceSchema = sourceSchema, TargetSchema = targetSchema });
                    }
                }
            }

            return schemaMappingInfos;
        }

        private DbConvertResult HandleError(ConvertException ex)
        {
            this.hasError = true;
            this.isBusy = false;

            string errMsg = ExceptionHelper.GetExceptionDetails(ex);
            this.Feedback(this, errMsg, FeedbackInfoType.Error);

            DbConvertResult result = new DbConvertResult();
            result.InfoType = DbConvertResultInfoType.Error;
            result.Message = errMsg;

            return result;
        }

        private (Table Table, List<TableColumn> Columns) GetTargetTableColumns(SchemaInfo targetSchemaInfo, string targetSchema, Table sourceTable, List<TableColumn> sourceColumns)
        {
            string mappedTableName = GetMappedTableName(sourceTable.Name, this.Source.TableNameMappings);

            Table targetTable = targetSchemaInfo.Tables.FirstOrDefault(item => (item.Schema == targetSchema || string.IsNullOrEmpty(targetSchema)) && item.Name == mappedTableName);

            if (targetTable == null)
            {
                this.Feedback(this, $"Source table {sourceTable.Name} cannot get a target table.", FeedbackInfoType.Error);
                return (null, null);
            }

            List<TableColumn> targetTableColumns = new List<TableColumn>();

            foreach (TableColumn sourceColumn in sourceColumns)
            {
                TableColumn targetTableColumn = targetSchemaInfo.TableColumns.FirstOrDefault(item => (item.Schema == targetSchema || string.IsNullOrEmpty(targetSchema)) && item.TableName == mappedTableName && item.Name == sourceColumn.Name);

                if (targetTableColumn == null)
                {
                    this.Feedback(this, $"Source column {sourceColumn.TableName} of table {sourceColumn.TableName} cannot get a target column.", FeedbackInfoType.Error);
                    return (null, null);
                }

                targetTableColumns.Add(targetTableColumn);
            }

            return (targetTable, targetTableColumns);
        }

        private void ConvertSchema(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, SchemaInfo targetSchemaInfo)
        {
            List<SchemaMappingInfo> schemaMappings = this.Option.SchemaMappings;

            if (schemaMappings.Count == 0)
            {
                schemaMappings.Add(new SchemaMappingInfo() { SourceSchema = "", TargetSchema = targetDbInterpreter.DefaultSchema });
            }
            else
            {
                if (sourceDbInterpreter.DefaultSchema != null && targetDbInterpreter.DefaultSchema != null)
                {
                    if (!schemaMappings.Any(item => item.SourceSchema == sourceDbInterpreter.DefaultSchema))
                    {
                        schemaMappings.Add(new SchemaMappingInfo() { SourceSchema = sourceDbInterpreter.DefaultSchema, TargetSchema = targetDbInterpreter.DefaultSchema });
                    }
                }
            }

            SchemaInfoHelper.MapDatabaseObjectSchema(targetSchemaInfo, schemaMappings);
        }

        public void Feedback(object owner, string content, FeedbackInfoType infoType = FeedbackInfoType.Info, bool enableLog = true)
        {
            if (infoType == FeedbackInfoType.Error)
            {
                this.hasError = true;
            }

            FeedbackInfo info = new FeedbackInfo() { InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(content), Owner = owner };

            FeedbackHelper.Feedback(this.observer, info, enableLog);

            if (this.OnFeedback != null)
            {
                this.OnFeedback(info);
            }
        }

        public void Dispose()
        {
            this.Source = null;
            this.Target = null;
            this.transaction = null;
        }
    }
}
