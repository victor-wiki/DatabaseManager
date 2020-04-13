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
    public delegate void FeedbackHandle(FeedbackInfo info);

    public class DbConverter : IDisposable
    {
        private bool hasError = false;
        private bool isBusy = false;
        private bool cancelRequested = false;
        private IObserver<FeedbackInfo> observer;
        private DbTransaction transaction = null;

        public bool HasError => this.hasError;
        public bool IsBusy => this.isBusy;
        public bool CancelRequested => this.cancelRequested;

        public DbConveterInfo Source { get; set; }
        public DbConveterInfo Target { get; set; }

        public DbConverterOption Option { get; set; } = new DbConverterOption();

        public event FeedbackHandle OnFeedback;

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

        public Task Convert(SchemaInfo schemaInfo = null)
        {
            return this.InternalConvert(schemaInfo);
        }

        private async Task InternalConvert(SchemaInfo schemaInfo = null)
        {
            DbInterpreter sourceInterpreter = this.Source.DbInterpreter;

            if (sourceInterpreter.DatabaseType == DatabaseType.Oracle)
            {
                sourceInterpreter.Option.TreatBytesAsNullForExecuting = true;
            }

            sourceInterpreter.Option.GetTableAllObjects = false;

            DatabaseObjectType databaseObjectType = (DatabaseObjectType)Enum.GetValues(typeof(DatabaseObjectType)).Cast<int>().Sum();

            SchemaInfoFilter filter = new SchemaInfoFilter() { Strict = true, DatabaseObjectType = databaseObjectType };

            SchemaInfoHelper.SetSchemaInfoFilterValues(filter, schemaInfo);

            SchemaInfo sourceSchemaInfo = await sourceInterpreter.GetSchemaInfoAsync(filter);

            if (SettingManager.Setting.NotCreateIfExists)
            {
                this.Target.DbInterpreter.Option.GetTableAllObjects = false;

                SchemaInfo targetSchema = await this.Target.DbInterpreter.GetSchemaInfoAsync(filter);

                SchemaInfoHelper.ExcludeExistingObjects(sourceSchemaInfo, targetSchema);
            }

            #region Set data type by user define type          

            List<UserDefinedType> utypes = new List<UserDefinedType>();

            if (sourceInterpreter.DatabaseType != this.Target.DbInterpreter.DatabaseType)
            {
                utypes = await sourceInterpreter.GetUserDefinedTypesAsync();

                if (utypes != null && utypes.Count > 0)
                {
                    foreach (TableColumn column in sourceSchemaInfo.TableColumns)
                    {
                        UserDefinedType utype = utypes.FirstOrDefault(item => item.Name == column.DataType);
                        if (utype != null)
                        {
                            column.DataType = utype.Type;
                            column.MaxLength = utype.MaxLength;
                        }
                    }
                }
            }

            #endregion

            SchemaInfo targetSchemaInfo = SchemaInfoHelper.Clone(sourceSchemaInfo);

            TranslateEngine translateEngine = new TranslateEngine(targetSchemaInfo, sourceInterpreter, this.Target.DbInterpreter, this.Target.DbOwner) { SkipError = this.Option.SkipScriptError };
            translateEngine.UserDefinedTypes = utypes;

            translateEngine.Translate();

            if (this.Option.EnsurePrimaryKeyNameUnique)
            {
                SchemaInfoHelper.EnsurePrimaryKeyNameUnique(targetSchemaInfo);
            }

            if (this.Option.EnsureIndexNameUnique)
            {
                SchemaInfoHelper.EnsureIndexNameUnique(targetSchemaInfo);
            }

            DbInterpreter targetInterpreter = this.Target.DbInterpreter;
            bool generateIdentity = targetInterpreter.Option.TableScriptsGenerateOption.GenerateIdentity;

            if (generateIdentity)
            {
                targetInterpreter.Option.InsertIdentityValue = true;
            }

            string script = "";

            sourceInterpreter.Subscribe(this.observer);
            targetInterpreter.Subscribe(this.observer);

            DataTransferErrorProfile dataErrorProfile = null;

            using (DbConnection dbConnection = targetInterpreter.CreateConnection())
            {
                this.isBusy = true;

                if (this.Option.ExecuteScriptOnTargetServer && this.Option.UseTransaction)
                {
                    dbConnection.Open();
                    this.transaction = dbConnection.BeginTransaction();
                }

                #region Schema sync
                if (this.Option.GenerateScriptMode.HasFlag(GenerateScriptMode.Schema))
                {
                    List<Script> scripts = targetInterpreter.GenerateSchemaScripts(targetSchemaInfo).Scripts;

                    if (this.Option.ExecuteScriptOnTargetServer)
                    {
                        if (scripts.Count == 0)
                        {
                            this.Feedback(targetInterpreter, $"The script to create schema is empty.", FeedbackInfoType.Info);
                            this.isBusy = false;
                            return;
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
                                    return !(s is NewLineSript || s is SpliterScript || string.IsNullOrEmpty(s.Content) || s.Content == targetInterpreter.ScriptsSplitString);
                                };

                                int count = scripts.Where(item=> isValidScript(item)).Count();
                                int i = 0;

                                foreach (Script s in scripts)
                                {
                                    if(!isValidScript(s))
                                    {
                                        continue;
                                    }

                                    bool isCreateScript = s.ObjectType == nameof(Function) || s.ObjectType == nameof(Procedure) || s.ObjectType == nameof(TableTrigger) || s.ObjectType == nameof(View);

                                    bool skipError = this.Option.SkipScriptError && isCreateScript;

                                    string sql = s.Content?.Trim();

                                    if (!string.IsNullOrEmpty(sql) && sql != targetInterpreter.ScriptsSplitString)
                                    {
                                        i++;

                                        if(!isCreateScript && targetInterpreter.ScriptsSplitString.Length == 1 && sql.EndsWith(targetInterpreter.ScriptsSplitString))
                                        {
                                            sql = sql.TrimEnd(targetInterpreter.ScriptsSplitString.ToArray());
                                        }

                                        if (!targetInterpreter.HasError)
                                        {
                                            targetInterpreter.Feedback(FeedbackInfoType.Info, $"({i}/{count}), executing:{Environment.NewLine} {sql}");

                                            CommandInfo commandInfo = this.GetCommandInfo(sql, null, transaction);
                                            commandInfo.SkipError = skipError;

                                            await targetInterpreter.ExecuteNonQueryAsync(dbConnection, commandInfo);
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

                            this.HandleError(schemaTransferException);
                        }

                        targetInterpreter.Feedback(FeedbackInfoType.Info, "End sync schema.");
                    }
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

                    if (this.Option.PickupTable)
                    {
                        dataErrorProfile = DataTransferErrorProfileManager.GetProfile(sourceInterpreter.ConnectionInfo, targetInterpreter.ConnectionInfo);

                        if (dataErrorProfile != null)
                        {
                            sourceSchemaInfo.PickupTable = new Table() { Owner = schemaInfo.Tables.FirstOrDefault()?.Owner, Name = dataErrorProfile.SourceTableName };
                        }
                    }

                    if (sourceInterpreter.Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
                    {
                        sourceInterpreter.AppendScriptsToFile("", GenerateScriptMode.Data, true);
                    }

                    if (targetInterpreter.Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
                    {
                        targetInterpreter.AppendScriptsToFile("", GenerateScriptMode.Data, true);
                    }

                    foreach (var item in identityTableColumns)
                    {
                        if (targetInterpreter.DatabaseType == DatabaseType.SqlServer)
                        {
                            await targetInterpreter.SetIdentityEnabled(dbConnection, item, false);
                        }
                    }

                    if (this.Option.ExecuteScriptOnTargetServer || targetInterpreter.Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
                    {
                        sourceInterpreter.OnDataRead += async (table, columns, data, dataTable) =>
                        {
                            if (!this.hasError)
                            {
                                try
                                {
                                    StringBuilder sb = new StringBuilder();

                                    (Table Table, List<TableColumn> Columns) targetTableAndColumns = this.GetTargetTableColumns(targetSchemaInfo, this.Target.DbOwner, table, columns);

                                    if (targetTableAndColumns.Table == null || targetTableAndColumns.Columns == null)
                                    {
                                        return;
                                    }

                                    Dictionary<string, object> paramters = targetInterpreter.AppendDataScripts(sb, targetTableAndColumns.Table, targetTableAndColumns.Columns, new Dictionary<long, List<Dictionary<string, object>>>() { { 1, data } });

                                    script = sb.ToString().Trim().Trim(';');

                                    if (this.Option.ExecuteScriptOnTargetServer)
                                    {
                                        if (this.Option.BulkCopy && targetInterpreter.SupportBulkCopy)
                                        {
                                            await targetInterpreter.BulkCopyAsync(dbConnection, dataTable, this.GetBulkCopyInfo(table.Name, this.transaction));
                                        }
                                        else
                                        {
                                            await targetInterpreter.ExecuteNonQueryAsync(dbConnection, this.GetCommandInfo(script, paramters, this.transaction));
                                        }

                                        targetInterpreter.FeedbackInfo($"Table \"{table.Name}\":{data.Count} records transferred.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    sourceInterpreter.CancelRequested = true;

                                    this.Rollback();

                                    ConnectionInfo sourceConnectionInfo = sourceInterpreter.ConnectionInfo;
                                    ConnectionInfo targetConnectionInfo = targetInterpreter.ConnectionInfo;

                                    DataTransferException dataTransferException = new DataTransferException(ex)
                                    {
                                        SourceServer = sourceConnectionInfo.Server,
                                        SourceDatabase = sourceConnectionInfo.Database,
                                        SourceObject = table.Name,
                                        TargetServer = targetConnectionInfo.Server,
                                        TargetDatabase = targetConnectionInfo.Database,
                                        TargetObject = table.Name
                                    };

                                    this.HandleError(dataTransferException);

                                    if (!this.Option.UseTransaction)
                                    {
                                        DataTransferErrorProfileManager.Save(new DataTransferErrorProfile
                                        {
                                            SourceServer = sourceConnectionInfo.Server,
                                            SourceDatabase = sourceConnectionInfo.Database,
                                            SourceTableName = table.Name,
                                            TargetServer = targetConnectionInfo.Server,
                                            TargetDatabase = targetConnectionInfo.Database,
                                            TargetTableName = table.Name
                                        });
                                    }
                                }
                            }
                        };
                    }

                    await sourceInterpreter.GenerateDataScriptsAsync(sourceSchemaInfo);

                    foreach (var item in identityTableColumns)
                    {
                        if (targetInterpreter.DatabaseType == DatabaseType.SqlServer)
                        {
                            await targetInterpreter.SetIdentityEnabled(dbConnection, item, true);
                        }
                    }
                }
                #endregion

                if (this.transaction != null && !this.cancelRequested)
                {
                    this.transaction.Commit();
                }

                this.isBusy = false;
            }

            if (dataErrorProfile != null && !this.hasError && !this.cancelRequested)
            {
                DataTransferErrorProfileManager.Remove(dataErrorProfile);
            }
        }

        private void Rollback()
        {
            if (this.transaction != null && this.transaction.Connection != null)
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

        private BulkCopyInfo GetBulkCopyInfo(string tableName, DbTransaction transaction = null)
        {
            BulkCopyInfo bulkCopyInfo = new BulkCopyInfo()
            {
                DestinationTableName = tableName,
                Transaction = transaction,
                CancellationToken = this.CancellationTokenSource.Token
            };

            return bulkCopyInfo;
        }

        private void HandleError(ConvertException ex)
        {
            this.hasError = true;
            this.isBusy = false;

            string errMsg = ExceptionHelper.GetExceptionDetails(ex);
            this.Feedback(this, errMsg, FeedbackInfoType.Error);
        }

        public void Cancle()
        {
            this.cancelRequested = true;

            if(this.Source!=null)
            {
                this.Source.DbInterpreter.CancelRequested = true;
            }
            
            if(this.Target!=null)
            {
                this.Target.DbInterpreter.CancelRequested = true;
            }          

            this.Rollback();

            if (this.CancellationTokenSource != null)
            {
                this.CancellationTokenSource.Cancel();
            }
        }

        private (Table Table, List<TableColumn> Columns) GetTargetTableColumns(SchemaInfo targetSchemaInfo, string targetOwner, Table sourceTable, List<TableColumn> sourceColumns)
        {
            Table targetTable = targetSchemaInfo.Tables.FirstOrDefault(item => (item.Owner == targetOwner || string.IsNullOrEmpty(targetOwner)) && item.Name == sourceTable.Name);

            if (targetTable == null)
            {
                this.Feedback(this, $"Source table {sourceTable.Name} cannot get a target table.", FeedbackInfoType.Error);
                return (null, null);
            }

            List<TableColumn> targetTableColumns = new List<TableColumn>();

            foreach (TableColumn sourceColumn in sourceColumns)
            {
                TableColumn targetTableColumn = targetSchemaInfo.TableColumns.FirstOrDefault(item => (item.Owner == targetOwner || string.IsNullOrEmpty(targetOwner)) && item.TableName == sourceColumn.TableName && item.Name == sourceColumn.Name);
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

        public void Dispose()
        {
            this.Source = null;
            this.Target = null;
            this.transaction = null;
        }
    }
}
