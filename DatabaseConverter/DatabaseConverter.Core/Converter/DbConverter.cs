using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseConverter.Profile;
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

            sourceInterpreter.Option.TreatBytesAsNullForScript = true;

            SelectionInfo selectionInfo = new SelectionInfo();

            if (schemaInfo != null)
            {
                selectionInfo.UserDefinedTypeNames = schemaInfo.UserDefinedTypes.Select(item => item.Name).ToArray();
                selectionInfo.TableNames = schemaInfo.Tables.Select(t => t.Name).ToArray();
                selectionInfo.ViewNames = schemaInfo.Views.Select(item => item.Name).ToArray();
            }

            SchemaInfo sourceSchemaInfo = await sourceInterpreter.GetSchemaInfoAsync(selectionInfo);

            #region Set data type by user define type          

            List<UserDefinedType> utypes = await sourceInterpreter.GetUserDefinedTypesAsync();

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

            #endregion

            SchemaInfo targetSchemaInfo = SchemaInfoHelper.Clone(sourceSchemaInfo);

            if (!string.IsNullOrEmpty(this.Target.DbOwner))
            {
                SchemaInfoHelper.TransformOwner(targetSchemaInfo, this.Target.DbOwner);
            }

            ColumnTranslator columnTranslator = new ColumnTranslator(targetSchemaInfo.TableColumns, this.Source.DbInterpreter.DatabaseType, this.Target.DbInterpreter.DatabaseType);
            targetSchemaInfo.TableColumns = columnTranslator.Translate();

            ViewTranslator viewTranslator = new ViewTranslator(targetSchemaInfo.Views, sourceInterpreter, this.Target.DbInterpreter, this.Target.DbOwner);
            targetSchemaInfo.Views = viewTranslator.Translate();

            if (this.Option.EnsurePrimaryKeyNameUnique)
            {
                SchemaInfoHelper.EnsurePrimaryKeyNameUnique(targetSchemaInfo);
            }

            if (this.Option.EnsureIndexNameUnique)
            {
                SchemaInfoHelper.EnsureIndexNameUnique(targetSchemaInfo);
            }

            DbInterpreter targetInterpreter = this.Target.DbInterpreter;
            bool generateIdentity = targetInterpreter.Option.GenerateIdentity;

            if (generateIdentity)
            {
                targetInterpreter.Option.InsertIdentityValue = true;
            }

            string script = "";

            sourceInterpreter.Subscribe(this.observer);
            targetInterpreter.Subscribe(this.observer);

            DataTransferErrorProfile dataErrorProfile = null;

            using (DbConnection dbConnection = targetInterpreter.GetDbConnector().CreateConnection())
            {
                this.isBusy = true;

                if (this.Option.UseTransaction)
                {
                    dbConnection.Open();
                    this.transaction = dbConnection.BeginTransaction();
                }

                #region Schema sync
                if (this.Option.GenerateScriptMode.HasFlag(GenerateScriptMode.Schema))
                {
                    script = targetInterpreter.GenerateSchemaScripts(targetSchemaInfo);

                    if (this.Option.ExecuteScriptOnTargetServer)
                    {
                        if (string.IsNullOrEmpty(script))
                        {
                            this.Feedback(targetInterpreter, $"The script to create schema is empty.", FeedbackInfoType.Error);
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
                                string[] sqls = script.Split(new string[] { targetInterpreter.ScriptsSplitString }, StringSplitOptions.RemoveEmptyEntries);
                                int count = sqls.Count();

                                int i = 0;
                                foreach (string sql in sqls)
                                {
                                    if (!string.IsNullOrEmpty(sql.Trim()))
                                    {
                                        i++;

                                        if (!targetInterpreter.HasError)
                                        {
                                            targetInterpreter.Feedback(FeedbackInfoType.Info, $"({i}/{count}), executing:{Environment.NewLine} {sql}");
                                            await targetInterpreter.ExecuteNonQueryAsync(dbConnection, this.GetCommandInfo(sql.Trim(), null, transaction));
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
                            sourceSchemaInfo.PickupTable  = new Table() { Owner = schemaInfo.Tables.FirstOrDefault()?.Owner, Name = dataErrorProfile.SourceTableName };
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
                                            await targetInterpreter.BulkCopyAsync(dbConnection, dataTable, table.Name);
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
            if (this.transaction != null)
            {
                this.transaction.Rollback();
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
            this.Source.DbInterpreter.CancelRequested = true;
            this.Target.DbInterpreter.CancelRequested = true;

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

            if (this.transaction != null)
            {
                this.transaction = null;
            }
        }
    }
}
