using DatabaseConverter.Core.Model;
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
    public delegate void TranslateHandler(DatabaseType dbType, DatabaseObject dbObject, TranslateResult result);

    public class DbConverter : IDisposable
    {
        private bool hasError = false;
        private bool isBusy = false;
        private bool cancelRequested = false;
        private IObserver<FeedbackInfo> observer;
        private DbTransaction transaction = null;
        private DatabaseObject translateDbObject = null;

        public bool HasError => this.hasError;
        public bool IsBusy => this.isBusy;
        public bool CancelRequested => this.cancelRequested;

        public DbConveterInfo Source { get; set; }
        public DbConveterInfo Target { get; set; }

        public DbConverterOption Option { get; set; } = new DbConverterOption();

        public event FeedbackHandler OnFeedback;
        public event TranslateHandler OnTranslated;

        public CancellationTokenSource CancellationTokenSource { get; private set; }

        public DbConverter(DbConveterInfo source, DbConveterInfo target)
        {
            this.Source = source;
            this.Target = target;

            this.Init();
        }

        public DbConverter(DbConveterInfo source, DbConveterInfo target, DbConverterOption option)
        {
            this.Source = source;
            this.Target = target;

            if (option != null)
            {
                this.Option = option;
            }

            this.Init();
        }

        private void Init()
        {
            this.CancellationTokenSource = new CancellationTokenSource();
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public Task<DbConverterResult> Translate(DatabaseObject dbObject)
        {
            this.translateDbObject = dbObject;

            SchemaInfo schemaInfo = SchemaInfoHelper.GetSchemaInfoByDbObject(dbObject);

            return this.InternalConvert(schemaInfo, dbObject.Schema);
        }

        public Task<DbConverterResult> Convert(SchemaInfo schemaInfo = null, string schema = null)
        {
            return this.InternalConvert(schemaInfo, schema);
        }

        private async Task<DbConverterResult> InternalConvert(SchemaInfo schemaInfo = null, string schema = null)
        {
            DbConverterResult result = new DbConverterResult();
            bool continuedWhenErrorOccured = false;

            DbInterpreter sourceInterpreter = this.Source.DbInterpreter;
            sourceInterpreter.Option.BulkCopy = this.Option.BulkCopy;
            sourceInterpreter.Subscribe(this.observer);

            sourceInterpreter.Option.GetTableAllObjects = false;

            DatabaseObjectType databaseObjectType = (DatabaseObjectType)Enum.GetValues(typeof(DatabaseObjectType)).Cast<int>().Sum();

            if (schemaInfo != null && !this.Source.DbInterpreter.Option.GetTableAllObjects
                && (schemaInfo.TableTriggers == null || schemaInfo.TableTriggers.Count == 0))
            {
                databaseObjectType = databaseObjectType ^ DatabaseObjectType.TableTrigger;
            }

            if (this.Source.DatabaseObjectType != DatabaseObjectType.None)
            {
                databaseObjectType = databaseObjectType & this.Source.DatabaseObjectType;
            }

            SchemaInfoFilter filter = new SchemaInfoFilter() { Strict = true, DatabaseObjectType = databaseObjectType };

            if (schema != null)
            {
                filter.Schema = schema;
            }

            SchemaInfoHelper.SetSchemaInfoFilterValues(filter, schemaInfo);

            SchemaInfo sourceSchemaInfo = await sourceInterpreter.GetSchemaInfoAsync(filter);

            if (sourceInterpreter.HasError)
            {
                result.InfoType = DbConverterResultInfoType.Error;
                result.Message = "Source database interpreter has error occured.";
                return result;
            }

            sourceSchemaInfo.TableColumns = DbObjectHelper.ResortTableColumns(sourceSchemaInfo.Tables, sourceSchemaInfo.TableColumns);

            if (SettingManager.Setting.NotCreateIfExists)
            {
                this.Target.DbInterpreter.Option.GetTableAllObjects = false;

                SchemaInfo targetSchema = await this.Target.DbInterpreter.GetSchemaInfoAsync(filter);

                SchemaInfoHelper.ExcludeExistingObjects(sourceSchemaInfo, targetSchema);
            }

            List<UserDefinedType> utypes = new List<UserDefinedType>();

            if (sourceInterpreter.DatabaseType != this.Target.DbInterpreter.DatabaseType)
            {
                utypes = await sourceInterpreter.GetUserDefinedTypesAsync();

                if(SettingManager.Setting.UseOriginalDataTypeIfUdtHasOnlyOneAttr)
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

            SchemaInfo targetSchemaInfo = SchemaInfoHelper.Clone(sourceSchemaInfo);

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

            #region Translate
            TranslateEngine translateEngine = new TranslateEngine(sourceSchemaInfo, targetSchemaInfo, sourceInterpreter, this.Target.DbInterpreter, this.Option);

            translateEngine.ContinueWhenErrorOccurs = this.Option.ContinueWhenErrorOccurs || this.Option.OnlyForTranslate || !this.Option.ExecuteScriptOnTargetServer;

            DatabaseObjectType translateDbObjectType = TranslateEngine.SupportDatabaseObjectType;

            if (!this.Option.GenerateScriptMode.HasFlag(GenerateScriptMode.Schema) && this.Option.BulkCopy && this.Target.DbInterpreter.SupportBulkCopy)
            {
                translateDbObjectType = DatabaseObjectType.TableColumn;
            }

            translateEngine.UserDefinedTypes = utypes;
            translateEngine.OnTranslated += this.Translated;
            translateEngine.Subscribe(this.observer);
            translateEngine.Translate(translateDbObjectType);

            #endregion

            if (targetSchemaInfo.Tables.Any())
            {
                if (this.Option.EnsurePrimaryKeyNameUnique)
                {
                    SchemaInfoHelper.EnsurePrimaryKeyNameUnique(targetSchemaInfo);

                    if (sourceInterpreter.DatabaseType == DatabaseType.MySql)
                    {
                        SchemaInfoHelper.ForceRenameMySqlPrimaryKey(targetSchemaInfo);
                    }
                }

                if (this.Option.EnsureIndexNameUnique)
                {
                    SchemaInfoHelper.EnsureIndexNameUnique(targetSchemaInfo);
                }
            }

            DbInterpreter targetInterpreter = this.Target.DbInterpreter;

            bool generateIdentity = targetInterpreter.Option.TableScriptsGenerateOption.GenerateIdentity;

            string script = "";

            targetInterpreter.Subscribe(this.observer);

            DataTransferErrorProfile dataErrorProfile = null;

            using (DbConnection dbConnection = this.Option.ExecuteScriptOnTargetServer ? targetInterpreter.CreateConnection() : null)
            {
                ScriptBuilder scriptBuilder = null;

                DbScriptGenerator targetDbScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(targetInterpreter);

                if (this.Option.GenerateScriptMode.HasFlag(GenerateScriptMode.Schema))
                {
                    if (this.Target.DbInterpreter.DatabaseType == DatabaseType.Oracle)
                    {
                        if (!this.Option.OnlyForTranslate)
                        {
                            if (this.Target.DbInterpreter.IsLowDbVersion(dbConnection))
                            {
                                SchemaInfoHelper.RistrictNameLength(targetSchemaInfo, 30);
                            }
                        }
                    }

                    scriptBuilder = targetDbScriptGenerator.GenerateSchemaScripts(targetSchemaInfo);

                    if (this.Option.OnlyForTranslate)
                    {
                        this.Translated(targetInterpreter.DatabaseType, this.translateDbObject, new TranslateResult() { Data = scriptBuilder.ToString() });
                    }
                }

                if (this.Option.OnlyForTranslate)
                {
                    result.InfoType = DbConverterResultInfoType.Information;
                    result.Message = "Translate has finished";
                    return result;
                }

                this.isBusy = true;
                bool canCommit = false;

                if (this.Option.ExecuteScriptOnTargetServer)
                {
                    if (dbConnection.State != ConnectionState.Open)
                    {
                        dbConnection.Open();
                    }

                    if (this.Option.UseTransaction)
                    {
                        this.transaction = dbConnection.BeginTransaction();
                        canCommit = true;
                    }
                }

                #region Schema sync        

                if (scriptBuilder != null && this.Option.ExecuteScriptOnTargetServer)
                {
                    List<Script> scripts = scriptBuilder.Scripts;

                    if (scripts.Count == 0)
                    {
                        this.Feedback(targetInterpreter, $"The script to create schema is empty.", FeedbackInfoType.Info);
                        this.isBusy = false;
                        result.InfoType = DbConverterResultInfoType.Information;
                        result.Message = "No any script to execute.";

                        return result;
                    }

                    targetInterpreter.Feedback(FeedbackInfoType.Info, "Begin to sync schema...");

                    try
                    {
                        if (!this.Option.SplitScriptsToExecute)
                        {
                            targetInterpreter.Feedback(FeedbackInfoType.Info, script);

                            await targetInterpreter.ExecuteNonQueryAsync(dbConnection, this.GetCommandInfo(script, null, this.transaction));
                        }
                        else
                        {
                            Func<Script, bool> isValidScript = (s) =>
                            {
                                return !(s is NewLineSript || s is SpliterScript || string.IsNullOrEmpty(s.Content) || s.Content == targetInterpreter.ScriptsDelimiter);
                            };

                            int count = scripts.Where(item => isValidScript(item)).Count();
                            int i = 0;

                            foreach (Script s in scripts)
                            {
                                bool isCreateScript = s.ObjectType == nameof(Function) || s.ObjectType == nameof(Procedure) || s.ObjectType == nameof(TableTrigger);

                                bool continueWhenErrorOccurs = false;

                                if (targetInterpreter.HasError)
                                {
                                    if (!this.Option.ContinueWhenErrorOccurs)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        if (isCreateScript)
                                        {
                                            continueWhenErrorOccurs = true;
                                            continuedWhenErrorOccured = true;
                                        }
                                    }
                                }

                                if (!isValidScript(s))
                                {
                                    continue;
                                }

                                string sql = s.Content?.Trim();

                                if (!string.IsNullOrEmpty(sql) && sql != targetInterpreter.ScriptsDelimiter)
                                {
                                    i++;

                                    if (!isCreateScript && targetInterpreter.ScriptsDelimiter.Length == 1 && sql.EndsWith(targetInterpreter.ScriptsDelimiter))
                                    {
                                        sql = sql.TrimEnd(targetInterpreter.ScriptsDelimiter.ToArray());
                                    }

                                    if (!targetInterpreter.HasError || continueWhenErrorOccurs)
                                    {
                                        targetInterpreter.Feedback(FeedbackInfoType.Info, $"({i}/{count}), executing:{Environment.NewLine} {sql}");

                                        CommandInfo commandInfo = this.GetCommandInfo(sql, null, transaction);
                                        commandInfo.ContinueWhenErrorOccurs = continueWhenErrorOccurs;

                                        await targetInterpreter.ExecuteNonQueryAsync(dbConnection, commandInfo);

                                        if (commandInfo.HasError)
                                        {
                                            this.hasError = true;
                                        }

                                        if (commandInfo.TransactionRollbacked)
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
                        targetInterpreter.CancelRequested = true;

                        this.Rollback();

                        ConnectionInfo sourceConnectionInfo = sourceInterpreter.ConnectionInfo;
                        ConnectionInfo targetConnectionInfo = targetInterpreter.ConnectionInfo;

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
                    }

                    targetInterpreter.Feedback(FeedbackInfoType.Info, "End sync schema.");
                }
                #endregion

                #region Data sync
                if (!targetInterpreter.HasError && this.Option.GenerateScriptMode.HasFlag(GenerateScriptMode.Data) && sourceSchemaInfo.Tables.Count > 0)
                {
                    List<TableColumn> identityTableColumns = new List<TableColumn>();

                    if (generateIdentity)
                    {
                        identityTableColumns = targetSchemaInfo.TableColumns.Where(item => item.IsIdentity).ToList();
                    }

                    //await this.SetIdentityEnabled(identityTableColumns, targetInterpreter, targetDbScriptGenerator, dbConnection, transaction, false);

                    if (this.Option.ExecuteScriptOnTargetServer || targetInterpreter.Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
                    {
                        Dictionary<Table, long> dictTableDataTransferredCount = new Dictionary<Table, long>();

                        sourceInterpreter.OnDataRead += async (TableDataReadInfo tableDataReadInfo) =>
                        {
                            if (!this.hasError)
                            {
                                Table table = tableDataReadInfo.Table;
                                List<TableColumn> columns = tableDataReadInfo.Columns;

                                try
                                {
                                    string targetTableSchema = this.Option.SchemaMappings.Where(item => item.SourceSchema == table.Schema).FirstOrDefault()?.TargetSchema;

                                    if (string.IsNullOrEmpty(targetTableSchema))
                                    {
                                        targetTableSchema = targetInterpreter.DefaultSchema;
                                    }

                                    (Table Table, List<TableColumn> Columns) targetTableAndColumns = this.GetTargetTableColumns(targetSchemaInfo, targetTableSchema, table, columns);

                                    if (targetTableAndColumns.Table == null || targetTableAndColumns.Columns == null)
                                    {
                                        return;
                                    }

                                    List<Dictionary<string, object>> data = tableDataReadInfo.Data;

                                    if (this.Option.ExecuteScriptOnTargetServer)
                                    {
                                        DataTable dataTable = tableDataReadInfo.DataTable;

                                        if (this.Option.BulkCopy && targetInterpreter.SupportBulkCopy)
                                        {
                                            BulkCopyInfo bulkCopyInfo = this.GetBulkCopyInfo(table, targetSchemaInfo, this.transaction);

                                            if (targetInterpreter.DatabaseType == DatabaseType.Oracle)
                                            {
                                                if (columns.Any(item => item.DataType.ToLower().Contains("datetime2") || item.DataType.ToLower().Contains("timestamp")))
                                                {
                                                    bulkCopyInfo.DetectDateTimeTypeByValues = true;
                                                }
                                            }

                                            if (this.Option.ConvertComputeColumnExpression)
                                            {
                                                IEnumerable<DataColumn> dataColumns = dataTable.Columns.OfType<DataColumn>();

                                                foreach (TableColumn column in bulkCopyInfo.Columns)
                                                {
                                                    if (column.IsComputed && dataColumns.Any(item => item.ColumnName == column.Name))
                                                    {
                                                        dataTable.Columns.Remove(column.Name);
                                                    }
                                                }
                                            }

                                            await targetInterpreter.BulkCopyAsync(dbConnection, dataTable, bulkCopyInfo);
                                        }
                                        else
                                        {
                                            (Dictionary<string, object> Paramters, string Script) scriptResult = this.GenerateScripts(targetDbScriptGenerator, targetTableAndColumns, data);

                                            await targetInterpreter.ExecuteNonQueryAsync(dbConnection, this.GetCommandInfo(scriptResult.Script, scriptResult.Paramters, this.transaction));
                                        }

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

                                        targetInterpreter.FeedbackInfo($"Table \"{table.Name}\":{dataTable.Rows.Count} records transferred.({transferredCount}/{tableDataReadInfo.TotalCount},{strPercent})");
                                    }
                                    else
                                    {
                                        this.GenerateScripts(targetDbScriptGenerator, targetTableAndColumns, data);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    sourceInterpreter.CancelRequested = true;

                                    this.Rollback();

                                    ConnectionInfo sourceConnectionInfo = sourceInterpreter.ConnectionInfo;
                                    ConnectionInfo targetConnectionInfo = targetInterpreter.ConnectionInfo;

                                    string mappedTableName = this.GetMappedTableName(table.Name);

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

                                    result.InfoType = DbConverterResultInfoType.Error;
                                    result.Message = res.Message;
                                }
                            }
                        };
                    }

                    DbScriptGenerator sourceDbScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(sourceInterpreter);

                    await sourceDbScriptGenerator.GenerateDataScriptsAsync(sourceSchemaInfo);

                    //await this.SetIdentityEnabled(identityTableColumns, targetInterpreter, targetDbScriptGenerator, dbConnection, transaction, true);
                }
                #endregion

                if (this.transaction != null && this.transaction.Connection != null && !this.cancelRequested && canCommit)
                {
                    this.transaction.Commit();
                }

                this.isBusy = false;
            }

            if (dataErrorProfile != null && !this.hasError && !this.cancelRequested)
            {
                DataTransferErrorProfileManager.Remove(dataErrorProfile);
            }

            if (this.hasError)
            {
                if (continuedWhenErrorOccured)
                {
                    result.InfoType = DbConverterResultInfoType.Warnning;
                    result.Message = $"Convert has finished,{Environment.NewLine}but some errors occured.";
                }
                else
                {
                    result.InfoType = DbConverterResultInfoType.Error;

                    if (string.IsNullOrEmpty(result.Message))
                    {
                        result.Message = "Convert failed.";
                    }
                }
            }
            else
            {
                result.InfoType = DbConverterResultInfoType.Information;
                result.Message = "Convert has finished.";
            }

            return result;
        }

        private (Dictionary<string, object> Paramters, string Script) GenerateScripts(DbScriptGenerator targetDbScriptGenerator, (Table Table, List<TableColumn> Columns) targetTableAndColumns, List<Dictionary<string, object>> data)
        {
            StringBuilder sb = new StringBuilder();

            Dictionary<string, object> paramters = targetDbScriptGenerator.AppendDataScripts(sb, targetTableAndColumns.Table, targetTableAndColumns.Columns, new Dictionary<long, List<Dictionary<string, object>>>() { { 1, data } });

            string script = sb.ToString().Trim().Trim(';');

            return (paramters, script);
        }

        #region Not use it currently, because bulkcopy doesn't care identity and insert script has excluded the identity columns.
        private async Task SetIdentityEnabled(IEnumerable<TableColumn> identityTableColumns, DbInterpreter dbInterpreter, DbScriptGenerator scriptGenerator,
                                             DbConnection dbConnection, DbTransaction transaction, bool enabled)
        {
            foreach (var item in identityTableColumns)
            {
                string sql = scriptGenerator.SetIdentityEnabled(item, enabled).Content;

                if (!string.IsNullOrEmpty(sql))
                {
                    CommandInfo commandInfo = this.GetCommandInfo(sql, null, transaction);
                    commandInfo.ContinueWhenErrorOccurs = true;

                    await dbInterpreter.ExecuteNonQueryAsync(dbConnection, commandInfo);
                }
            }
        }
        #endregion

        private void Rollback()
        {
            if (this.transaction != null && this.transaction.Connection != null && this.transaction.Connection.State == ConnectionState.Open)
            {
                try
                {
                    this.cancelRequested = true;

                    this.transaction.Rollback();
                }
                catch (Exception ex)
                {
                    //throw;
                }
            }
        }

        private CommandInfo GetCommandInfo(string commandText, Dictionary<string, object> parameters = null, DbTransaction transaction = null)
        {
            CommandInfo commandInfo = new CommandInfo()
            {
                CommandType = CommandType.Text,
                CommandText = commandText,
                Parameters = parameters,
                Transaction = transaction,
                CancellationToken = this.CancellationTokenSource.Token
            };

            return commandInfo;
        }

        private BulkCopyInfo GetBulkCopyInfo(Table table, SchemaInfo schemaInfo, DbTransaction transaction = null)
        {
            string tableName = this.GetMappedTableName(table.Name);

            BulkCopyInfo bulkCopyInfo = new BulkCopyInfo()
            {
                KeepIdentity = this.Target.DbInterpreter.Option.TableScriptsGenerateOption.GenerateIdentity,
                DestinationTableName = tableName,
                Transaction = transaction,
                CancellationToken = this.CancellationTokenSource.Token
            };

            string mappedSchema = SchemaInfoHelper.GetMappedSchema(table.Schema, this.Option.SchemaMappings);

            if (mappedSchema == null)
            {
                mappedSchema = this.Target.DbInterpreter.DefaultSchema;
            }

            bulkCopyInfo.DestinationTableSchema = mappedSchema;
            bulkCopyInfo.Columns = schemaInfo.TableColumns.Where(item => item.TableName == tableName && item.Schema == mappedSchema);

            return bulkCopyInfo;
        }

        private string GetMappedTableName(string tableName)
        {
            return SchemaInfoHelper.GetMappedTableName(tableName, this.Source.TableNameMappings);
        }

        private DbConverterResult HandleError(ConvertException ex)
        {
            this.hasError = true;
            this.isBusy = false;

            string errMsg = ExceptionHelper.GetExceptionDetails(ex);
            this.Feedback(this, errMsg, FeedbackInfoType.Error);

            DbConverterResult result = new DbConverterResult();
            result.InfoType = DbConverterResultInfoType.Error;
            result.Message = errMsg;

            return result;
        }

        public void Cancle()
        {
            this.cancelRequested = true;

            if (this.Source != null)
            {
                this.Source.DbInterpreter.CancelRequested = true;
            }

            if (this.Target != null)
            {
                this.Target.DbInterpreter.CancelRequested = true;
            }

            this.Rollback();

            if (this.CancellationTokenSource != null)
            {
                this.CancellationTokenSource.Cancel();
            }
        }

        private (Table Table, List<TableColumn> Columns) GetTargetTableColumns(SchemaInfo targetSchemaInfo, string targetSchema, Table sourceTable, List<TableColumn> sourceColumns)
        {
            string mappedTableName = this.GetMappedTableName(sourceTable.Name);

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

        private void Translated(DatabaseType dbType, DatabaseObject dbObject, TranslateResult result)
        {
            if (this.OnTranslated != null)
            {
                this.OnTranslated(dbType, dbObject, result);
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
