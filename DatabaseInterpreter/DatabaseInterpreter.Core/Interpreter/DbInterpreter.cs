using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;

namespace DatabaseInterpreter.Core
{
    public abstract class DbInterpreter
    {
        #region Field & Property       
        private IObserver<FeedbackInfo> m_Observer;
        protected DbConnector dbConnector;
        protected bool hasError = false;
        public const string RowNumberColumnName = "ROWNUMBER";
        public virtual string UnicodeInsertChar { get; } = "N";
        public virtual string ScriptsSplitString => ";";
        public bool ShowBuiltinDatabase => SettingManager.Setting.ShowBuiltinDatabase;
        public abstract string CommandParameterChar { get; }
        public abstract char QuotationLeftChar { get; }
        public abstract char QuotationRightChar { get; }
        public abstract DatabaseType DatabaseType { get; }
        public abstract bool SupportBulkCopy { get; }
        public virtual List<string> BuiltinDatabases { get; } = new List<string>();
        public bool CancelRequested { get; set; }
        public bool HasError => this.hasError;
        public DbInterpreterOption Option { get; set; } = new DbInterpreterOption();
        public ConnectionInfo ConnectionInfo { get; protected set; }

        public delegate Task DataReadHandler(Table table, List<TableColumn> columns, List<Dictionary<string, object>> data, DataTable dataTable);
        public event DataReadHandler OnDataRead;

        #endregion

        #region Constructor     

        public DbInterpreter(ConnectionInfo connectionInfo, DbInterpreterOption option)
        {
            this.ConnectionInfo = connectionInfo;
            this.Option = option;
        }
        #endregion

        #region Common Method    
        public string GetObjectDisplayName(DatabaseObject obj, bool useQuotedString = false)
        {
            if (this.GetType().Name == nameof(SqlServerInterpreter))
            {
                return $"{GetString(obj.Owner, useQuotedString)}.{GetString(obj.Name, useQuotedString)}";
            }
            return $"{GetString(obj.Name, useQuotedString)}";
        }

        private string GetString(string str, bool useQuotedString = false)
        {
            return useQuotedString ? GetQuotedString(str) : str;
        }

        public abstract DbConnector GetDbConnector();
        protected string GetQuotedObjectName(DatabaseObject obj)
        {
            return this.GetObjectDisplayName(obj, true);
        }
        protected string GetQuotedColumnNames(IEnumerable<TableColumn> columns)
        {
            return string.Join(",", columns.OrderBy(item => item.Order).Select(item => this.GetQuotedString(item.Name)));
        }

        protected string GetQuotedString(string str)
        {
            return $"{ this.QuotationLeftChar}{str}{this.QuotationRightChar}";
        }

        protected bool IsObjectFectchSimpleMode()
        {
            return this.Option.ObjectFetchMode == DatabaseObjectFetchMode.Simple;
        }

        protected async Task<List<T>> GetDbObjectsAsync<T>(string sql) where T : DatabaseObject
        {
            if (!string.IsNullOrEmpty(sql))
            {
                using (DbConnection dbConnection = this.dbConnector.CreateConnection())
                {
                    return await this.GetDbObjectsAsync<T>(dbConnection, sql);
                }
            }

            return new List<T>();
        }

        protected async Task<List<T>> GetDbObjectsAsync<T>(DbConnection dbConnection, string sql) where T : DatabaseObject
        {
            List<T> objects = new List<T>();

            if (!string.IsNullOrEmpty(sql))
            {
                objects = (await dbConnection.QueryAsync<T>(sql)).ToList();

                bool isAllOrdersIsZero = !objects.Any(item => item.Order != 0);

                if (isAllOrdersIsZero)
                {
                    int i = 1;
                    objects.ForEach(item =>
                    {
                        item.Order = i++;
                    });
                }
            }

            this.FeedbackInfo($"Get {objects.Count} {StringHelper.GetFriendlyTypeName(typeof(T).Name).ToLower()}{(objects.Count > 1 ? "s" : "")}.");

            return objects;
        }
        #endregion

        #region Observer
        public IDisposable Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.m_Observer = observer;
            return null;
        }

        public void Feedback(FeedbackInfoType infoType, string message)
        {
            if (this.m_Observer != null)
            {
                FeedbackHelper.Feedback(this.m_Observer, new FeedbackInfo() { Owner = this, InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(message) });
            }
        }

        public void FeedbackInfo(string message)
        {
            this.Feedback(FeedbackInfoType.Info, message);
        }
        public void FeedbackError(string message)
        {
            this.hasError = true;
            this.Feedback(FeedbackInfoType.Error, message);
        }

        public void FeedbackInfo(OperationState state, DatabaseObject dbObject)
        {
            string message = $"{state.ToString()} to generate script for { StringHelper.GetFriendlyTypeName(dbObject.GetType().Name).ToLower() } \"{dbObject.Name}\".";
            this.Feedback(FeedbackInfoType.Info, message);
        }
        #endregion

        #region Schema Informatioin
        #region Database
        public abstract Task<List<Database>> GetDatabasesAsync();
        #endregion

        #region User Defined Type     
        public abstract Task<List<UserDefinedType>> GetUserDefinedTypesAsync(params string[] userDefinedTypeNames);
        public abstract Task<List<UserDefinedType>> GetUserDefinedTypesAsync(DbConnection dbConnection, params string[] userDefinedTypeNames);

        #endregion

        #region Function        
        public abstract Task<List<Function>> GetFunctionsAsync(params string[] viewNames);
        public abstract Task<List<Function>> GetFunctionsAsync(DbConnection dbConnection, params string[] viewNames);
        #endregion

        #region Table      
        public abstract Task<List<Table>> GetTablesAsync(params string[] tableNames);
        public abstract Task<List<Table>> GetTablesAsync(DbConnection dbConnection, params string[] tableNames);
        #endregion

        #region Table Column
        public abstract Task<List<TableColumn>> GetTableColumnsAsync(params string[] tableNames);
        public abstract Task<List<TableColumn>> GetTableColumnsAsync(DbConnection dbConnection, params string[] tableNames);
        #endregion

        #region Table Primary Key
        public abstract Task<List<TablePrimaryKey>> GetTablePrimaryKeysAsync(params string[] tableNames);
        public abstract Task<List<TablePrimaryKey>> GetTablePrimaryKeysAsync(DbConnection dbConnection, params string[] tableNames);
        #endregion

        #region Table Foreign Key
        public abstract Task<List<TableForeignKey>> GetTableForeignKeysAsync(params string[] tableNames);
        public abstract Task<List<TableForeignKey>> GetTableForeignKeysAsync(DbConnection dbConnection, params string[] tableNames);
        #endregion

        #region Table Index
        public abstract Task<List<TableIndex>> GetTableIndexesAsync(params string[] tableNames);
        public abstract Task<List<TableIndex>> GetTableIndexesAsync(DbConnection dbConnection, params string[] tableNames);

        #endregion

        #region View        
        public abstract Task<List<View>> GetViewsAsync(params string[] viewNames);
        public abstract Task<List<View>> GetViewsAsync(DbConnection dbConnection, params string[] viewNames);
        #endregion

        #region Trigger        
        public abstract Task<List<Trigger>> GetTriggersAsync(params string[] triggerNames);
        public abstract Task<List<Trigger>> GetTriggersAsync(DbConnection dbConnection, params string[] triggerNames);
        #endregion

        #region Procedure        
        public abstract Task<List<Procedure>> GetProceduresAsync(params string[] procedureNames);
        public abstract Task<List<Procedure>> GetProceduresAsync(DbConnection dbConnection, params string[] procedureNames);
        #endregion

        #region SchemaInfo
        public virtual async Task<SchemaInfo> GetSchemaInfoAsync(SelectionInfo selectionInfo, DatabaseObjectType dbObjectType = DatabaseObjectType.None)
        {
            SchemaInfo schemaInfo = new SchemaInfo();

            using (DbConnection connection = this.dbConnector.CreateConnection())
            {
                if (this.NeedFetchObjects(DatabaseObjectType.UserDefinedTypes, selectionInfo.UserDefinedTypeNames, dbObjectType))
                {
                    schemaInfo.UserDefinedTypes = await this.GetUserDefinedTypesAsync(connection, selectionInfo.UserDefinedTypeNames);
                }

                if (this.NeedFetchObjects(DatabaseObjectType.Function, selectionInfo.FunctionNames, dbObjectType))
                {
                    schemaInfo.Functions = await this.GetFunctionsAsync(connection, selectionInfo.FunctionNames);
                }

                if (this.NeedFetchObjects(DatabaseObjectType.Table, selectionInfo.TableNames, dbObjectType))
                {
                    schemaInfo.Tables = await this.GetTablesAsync(connection, selectionInfo.TableNames);

                    if (!this.IsObjectFectchSimpleMode())
                    {
                        schemaInfo.TableColumns = await this.GetTableColumnsAsync(connection, selectionInfo.TableNames);

                        if (this.Option.GenerateKey)
                        {
                            schemaInfo.TablePrimaryKeys = await this.GetTablePrimaryKeysAsync(connection, selectionInfo.TableNames);
                            schemaInfo.TableForeignKeys = await this.GetTableForeignKeysAsync(connection, selectionInfo.TableNames);
                        }

                        if (this.Option.GenerateIndex)
                        {
                            schemaInfo.TableIndexes = await this.GetTableIndexesAsync(connection, selectionInfo.TableNames);
                        }

                        if (this.Option.SortTablesByKeyReference && this.Option.GenerateKey && schemaInfo.Tables.Count > 1)
                        {
                            string[] tableNames = schemaInfo.Tables.Select(item => item.Name).ToArray();

                            List<string> sortedTableNames = TableReferenceHelper.ResortTableNames(tableNames, schemaInfo.TableForeignKeys);

                            int i = 1;
                            foreach (string tableName in sortedTableNames)
                            {
                                Table table = schemaInfo.Tables.FirstOrDefault(item => item.Name == tableName);
                                if (table != null)
                                {
                                    table.Order = i++;
                                }
                            }

                            schemaInfo.Tables = schemaInfo.Tables.OrderBy(item => item.Order).ToList();
                        }
                    }
                }

                if (this.NeedFetchObjects(DatabaseObjectType.View, selectionInfo.ViewNames, dbObjectType))
                {
                    schemaInfo.Views = ViewHelper.ResortViews(await this.GetViewsAsync(connection, selectionInfo.ViewNames));
                }

                if (this.NeedFetchObjects(DatabaseObjectType.Trigger, selectionInfo.TriggerNames, dbObjectType))
                {
                    schemaInfo.Triggers = await this.GetTriggersAsync(connection, selectionInfo.TriggerNames);
                }

                if (this.NeedFetchObjects(DatabaseObjectType.Procedure, selectionInfo.ProcedureNames, dbObjectType))
                {
                    schemaInfo.Procedures = await this.GetProceduresAsync(connection, selectionInfo.ProcedureNames);
                }
            }

            return schemaInfo;
        }

        private bool NeedFetchObjects(DatabaseObjectType currentObjectType, string[] names, DatabaseObjectType fetchDbObjectType)
        {
            return (names != null && names.Any()) || this.Option.GetAllObjectsIfNotSpecified || fetchDbObjectType.HasFlag(currentObjectType);
        }
        #endregion
        #endregion

        #region Database Operation

        public virtual async Task SetIdentityEnabled(DbConnection dbConnection, TableColumn column, bool enabled) { await Task.Run(() => { }); }

        public Task<int> ExecuteNonQueryAsync(string sql)
        {
            return this.InternalExecuteNonQuery(this.dbConnector.CreateConnection(), new CommandInfo() { CommandText = sql });
        }

        public Task<int> ExecuteNonQueryAsync(CommandInfo commandInfo)
        {
            return this.InternalExecuteNonQuery(this.dbConnector.CreateConnection(), commandInfo, true);
        }

        public Task<int> ExecuteNonQueryAsync(DbConnection dbConnection, string sql)
        {
            return this.InternalExecuteNonQuery(dbConnection, new CommandInfo() { CommandText = sql });
        }

        public Task<int> ExecuteNonQueryAsync(DbConnection dbConnection, CommandInfo commandInfo)
        {
            return this.InternalExecuteNonQuery(dbConnection, commandInfo, false);
        }

        protected async Task<int> InternalExecuteNonQuery(DbConnection dbConnection, CommandInfo commandInfo, bool disposeConnection = true)
        {
            if (this.CancelRequested || this.hasError)
            {
                return 0;
            }

            CommandDefinition command = new CommandDefinition
            (
                commandInfo.CommandText,
                commandInfo.Parameters,
                commandInfo.Transaction,
                SettingManager.Setting.CommandTimeout,
                commandInfo.CommandType,
                CommandFlags.Buffered,
                commandInfo.CancellationToken
             );

            Func<Task<int>> exec = async () =>
            {
                try
                {
                    int result = await dbConnection.ExecuteAsync(command);

                    return result;
                }
                catch (Exception ex)
                {
                    if (command.Transaction != null)
                    {
                        command.Transaction.Rollback();
                    }

                    this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex));

                    return 0;
                }
            };

            if (disposeConnection)
            {
                using (dbConnection)
                {
                    return await exec();
                }
            }
            else
            {
                return await exec();
            }
        }

        public abstract Task<int> BulkCopyAsync(DbConnection connection, DataTable dataTable, string destinationTableName = null, int? bulkCopyTimeout = null, int? batchSize = null);

        protected async Task<object> GetScalarAsync(DbConnection dbConnection, string sql)
        {
            return await dbConnection.ExecuteScalarAsync(sql);
        }

        protected async Task<DataTable> GetDataTableAsync(DbConnection dbConnection, string sql)
        {
            var reader = await dbConnection.ExecuteReaderAsync(sql);

            DataTable table = new DataTable();
            table.Load(reader);

            return table;
        }

        #endregion

        #region Generate Scripts     

        public abstract string GenerateSchemaScripts(SchemaInfo schemaInfo);
        public abstract string ParseColumn(Table table, TableColumn column);      

        protected virtual string GenerateScriptDbObjectScripts<T>(List<T> dbObjects)
            where T : ScriptDbObject
        {
            StringBuilder sb = new StringBuilder();

            foreach (T dbObject in dbObjects)
            {
                this.FeedbackInfo(OperationState.Begin, dbObject);

                bool hasNewLine = this.ScriptsSplitString.Contains(Environment.NewLine);

                sb.Append(dbObject.Definition.Trim());

                if(!hasNewLine)
                {
                    sb.AppendLine(this.ScriptsSplitString);
                }
                else
                {
                    sb.AppendLine();
                    sb.Append(this.ScriptsSplitString);
                }
               
                sb.AppendLine();

                this.FeedbackInfo(OperationState.End, dbObject);
            }

            return sb.ToString();
        }

        public abstract long GetTableRecordCount(DbConnection connection, Table table);
        public abstract Task<long> GetTableRecordCountAsync(DbConnection connection, Table table);
        protected long GetTableRecordCount(DbConnection connection, string sql)
        {
            return connection.ExecuteScalar<long>(sql);
        }

        protected async Task<long> GetTableRecordCountAsync(DbConnection connection, string sql)
        {
            return await connection.ExecuteScalarAsync<long>(sql);
        }

        public virtual async Task<string> GenerateDataScriptsAsync(SchemaInfo schemaInfo)
        {
            StringBuilder sb = new StringBuilder();

            if (this.Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
            {
                this.AppendScriptsToFile("", GenerateScriptMode.Data, true);
            }

            int i = 0;
            int pickupIndex = -1;
            if (schemaInfo.PickupTable != null)
            {
                foreach (Table table in schemaInfo.Tables)
                {
                    if (table.Owner == schemaInfo.PickupTable.Owner && table.Name == schemaInfo.PickupTable.Name)
                    {
                        pickupIndex = i;
                        break;
                    }
                    i++;
                }
            }

            i = 0;
            using (DbConnection connection = this.dbConnector.CreateConnection())
            {
                int tableCount = schemaInfo.Tables.Count - (pickupIndex == -1 ? 0 : pickupIndex + 1);
                int count = 0;
                foreach (Table table in schemaInfo.Tables)
                {
                    if (this.CancelRequested)
                    {
                        break;
                    }

                    if (i < pickupIndex)
                    {
                        i++;
                        continue;
                    }

                    count++;

                    string strTableCount = $"({count}/{tableCount})";
                    string tableName = table.Name;
                    List<TableColumn> columns = schemaInfo.TableColumns.Where(item => item.Owner == table.Owner && item.TableName == tableName).OrderBy(item => item.Order).ToList();

                    bool isSelfReference = TableReferenceHelper.IsSelfReference(tableName, schemaInfo.TableForeignKeys);

                    List<TablePrimaryKey> primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.Owner == table.Owner && item.TableName == tableName).ToList();
                    string primaryKeyColumns = string.Join(",", primaryKeys.OrderBy(item => item.Order).Select(item => GetQuotedString(item.ColumnName)));

                    long total = await this.GetTableRecordCountAsync(connection, table);

                    if (this.Option.DataGenerateThreshold.HasValue && total > this.Option.DataGenerateThreshold.Value)
                    {
                        continue;
                    }

                    int pageSize = this.Option.DataBatchSize;

                    this.FeedbackInfo($"{strTableCount}Table \"{table.Name}\":record count is {total}.");

                    Dictionary<long, List<Dictionary<string, object>>> dictPagedData;

                    if (isSelfReference)
                    {
                        string parentColumnName = schemaInfo.TableForeignKeys.FirstOrDefault(item =>
                            item.Owner == table.Owner
                            && item.TableName == tableName
                            && item.ReferencedTableName == tableName)?.ColumnName;

                        string strWhere = $" WHERE {GetQuotedString(parentColumnName)} IS NULL";
                        dictPagedData = await this.GetSortedPageData(connection, table, primaryKeyColumns, parentColumnName, columns, strWhere);
                    }
                    else
                    {
                        dictPagedData = await this.GetPagedDataListAsync(connection, table, columns, primaryKeyColumns, total, pageSize);
                    }

                    this.FeedbackInfo($"{strTableCount}Table \"{table.Name}\":data read finished.");

                    this.AppendDataScripts(sb, table, columns, dictPagedData);

                    i++;
                }
            }

            var dataScripts = string.Empty;
            try
            {
                dataScripts = sb.ToString();
            }
            catch (OutOfMemoryException ex)
            {
                this.FeedbackError("Exception occurs:" + ex.Message);
            }
            finally
            {
                sb.Clear();
            }
            return dataScripts;
        }

        private async Task<Dictionary<long, List<Dictionary<string, object>>>> GetSortedPageData(DbConnection connection, Table table, string primaryKeyColumns, string parentColumnName, List<TableColumn> columns, string whereClause = "")
        {
            string quotedTableName = this.GetQuotedObjectName(table);

            int pageSize = this.Option.DataBatchSize;

            long total = Convert.ToInt64(await this.GetScalarAsync(connection, $"SELECT COUNT(1) FROM {quotedTableName} {whereClause}"));

            var dictPagedData = await this.GetPagedDataListAsync(connection, table, columns, primaryKeyColumns, total, pageSize, whereClause);

            List<object> parentValues = dictPagedData.Values.SelectMany(item => item.Select(t => t[primaryKeyColumns.Trim(QuotationLeftChar, QuotationRightChar)])).ToList();

            if (parentValues.Count > 0)
            {
                TableColumn parentColumn = columns.FirstOrDefault(item => item.Owner == table.Owner && item.Name == parentColumnName);

                long parentValuesPageCount = PaginationHelper.GetPageCount(parentValues.Count, this.Option.InQueryItemLimitCount);

                for (long parentValuePageNumber = 1; parentValuePageNumber <= parentValuesPageCount; parentValuePageNumber++)
                {
                    IEnumerable<object> pagedParentValues = parentValues.Skip((int)(parentValuePageNumber - 1) * pageSize).Take(this.Option.InQueryItemLimitCount);
                    whereClause = $" WHERE {GetQuotedString(parentColumnName)} IN ({string.Join(",", pagedParentValues.Select(item => ParseValue(parentColumn, item, true)))})";
                    total = Convert.ToInt64(await this.GetScalarAsync(connection, $"SELECT COUNT(1) FROM {quotedTableName} {whereClause}"));

                    if (total > 0)
                    {
                        Dictionary<long, List<Dictionary<string, object>>> dictChildPagedData = await this.GetSortedPageData(connection, table, primaryKeyColumns, parentColumnName, columns, whereClause);

                        foreach (var kp in dictChildPagedData)
                        {
                            long pageNumber = dictPagedData.Keys.Max(item => item);
                            dictPagedData.Add(pageNumber + 1, kp.Value);
                        }
                    }
                }
            }

            return dictPagedData;
        }

        private async Task<Dictionary<long, List<Dictionary<string, object>>>> GetPagedDataListAsync(DbConnection connection, Table table, List<TableColumn> columns, string primaryKeyColumns, long total, int pageSize, string whereClause = "")
        {
            string quotedTableName = this.GetQuotedObjectName(table);
            string columnNames = this.GetQuotedColumnNames(columns);

            var dictPagedData = new Dictionary<long, List<Dictionary<string, object>>>();

            long pageCount = PaginationHelper.GetPageCount(total, pageSize);

            for (long pageNumber = 1; pageNumber <= pageCount; pageNumber++)
            {
                if (this.CancelRequested)
                {
                    break;
                }

                string pagedSql = this.GetPagedSql(quotedTableName, columnNames, primaryKeyColumns, whereClause, pageNumber, pageSize);

                var dataTable = await this.GetDataTableAsync(connection, pagedSql);

                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                foreach (DataRow row in dataTable.Rows)
                {
                    var dicField = new Dictionary<string, object>();

                    for (var i = 0; i < dataTable.Columns.Count; i++)
                    {
                        DataColumn column = dataTable.Columns[i];
                        string columnName = column.ColumnName;

                        if (columnName == RowNumberColumnName)
                        {
                            continue;
                        }

                        TableColumn tableColumn = columns.FirstOrDefault(item => item.Name == columnName);

                        object value = row[i];

                        if (this.IsBytes(value) && this.Option.TreatBytesAsNullForData)
                        {
                            value = null;
                        }

                        object newValue = this.GetInsertValue(tableColumn, value);

                        dicField.Add(columnName, newValue);
                    }

                    rows.Add(dicField);
                }

                dictPagedData.Add(pageNumber, rows);

                if (this.OnDataRead != null && !this.CancelRequested && !this.hasError)
                {
                    await this.OnDataRead(table, columns, rows, dataTable);
                }
            }

            return dictPagedData;
        }

        protected abstract string GetPagedSql(string tableName, string columnNames, string primaryKeyColumns, string whereClause, long pageNumber, int pageSize);

        protected virtual object GetInsertValue(TableColumn column, object value)
        {
            return value;
        }

        public virtual Dictionary<string, object> AppendDataScripts(StringBuilder sb, Table table, List<TableColumn> columns, Dictionary<long, List<Dictionary<string, object>>> dictPagedData)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            bool appendString = this.Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToString);
            bool appendFile = this.Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile);

            List<string> excludeColumnNames = new List<string>();
            if (this.Option.GenerateIdentity && !this.Option.InsertIdentityValue)
            {
                excludeColumnNames = columns.Where(item => item.IsIdentity).Select(item => item.Name).ToList();
            }

            foreach (var kp in dictPagedData)
            {
                StringBuilder sbFilePage = new StringBuilder(Environment.NewLine);

                string tableName = this.GetQuotedObjectName(table);
                string insert = $"{this.GetBatchInsertPrefix()} {tableName}({this.GetQuotedColumnNames(columns.Where(item => !excludeColumnNames.Contains(item.Name)))})VALUES";

                if (appendString)
                {
                    sb.AppendLine(insert);
                }

                if (appendFile)
                {
                    sbFilePage.AppendLine(insert);
                }

                int rowCount = 0;
                foreach (var row in kp.Value)
                {
                    rowCount++;

                    string values = this.GetInsertValues(columns, excludeColumnNames, kp.Key, rowCount - 1, row, out var insertParameters, out var valuesWithoutParameter);

                    if (insertParameters != null)
                    {
                        foreach (var para in insertParameters)
                        {
                            parameters.Add(para.Key, para.Value);
                        }
                    }

                    string beginChar = this.GetBatchInsertItemBefore(tableName, rowCount == 1);
                    string endChar = this.GetBatchInsertItemEnd(rowCount == kp.Value.Count);

                    values = $"{beginChar}{values}{endChar}";

                    if (this.Option.RemoveEmoji)
                    {
                        values = StringHelper.RemoveEmoji(values);
                    }

                    if (appendString)
                    {
                        sb.AppendLine(values);
                    }

                    if (appendFile)
                    {
                        sbFilePage.AppendLine($"{beginChar}{valuesWithoutParameter}{endChar}");
                    }
                }

                if (appendString)
                {
                    sb.AppendLine();
                }

                if (appendFile)
                {
                    sbFilePage.AppendLine();

                    this.AppendScriptsToFile(sbFilePage.ToString(), GenerateScriptMode.Data);
                }
            }

            return parameters;
        }

        protected virtual string GetBatchInsertPrefix()
        {
            return "INSERT INTO";
        }

        protected virtual string GetBatchInsertItemBefore(string tableName, bool isFirstRow)
        {
            return "";
        }

        protected virtual string GetBatchInsertItemEnd(bool isAllEnd)
        {
            return (isAllEnd ? ";" : ",");
        }

        public string GetScriptOutputFilePath(GenerateScriptMode generateScriptMode)
        {
            string fileName = $"{this.ConnectionInfo.Database}_{this.GetType().Name.Replace("Interpreter", "")}_{DateTime.Today.ToString("yyyyMMdd")}_{generateScriptMode.ToString()}.sql";
            string filePath = Path.Combine(this.Option.ScriptOutputFolder, fileName);
            return filePath;
        }

        public virtual void AppendScriptsToFile(string content, GenerateScriptMode generateScriptMode, bool clearAll = false)
        {
            if (generateScriptMode == GenerateScriptMode.Schema)
            {
                content = StringHelper.ToSingleEmptyLine(content);
            }

            string filePath = this.GetScriptOutputFilePath(generateScriptMode);

            string directoryName = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (!clearAll)
            {
                File.AppendAllText(filePath, content, Encoding.UTF8);
            }
            else
            {
                File.WriteAllText(filePath, content, Encoding.UTF8);
            }
        }

        private string GetInsertValues(List<TableColumn> columns, List<string> excludeColumnNames, long pageNumber, int rowIndex, Dictionary<string, object> row, out Dictionary<string, object> parameters, out string valuesWithoutParameter)
        {
            valuesWithoutParameter = "";
            parameters = new Dictionary<string, object>();

            List<object> values = new List<object>();
            List<string> parameterPlaceholders = new List<string>();

            foreach (TableColumn column in columns)
            {
                if (!excludeColumnNames.Contains(column.Name))
                {
                    object value = this.ParseValue(column, row[column.Name]);

                    if (!this.Option.TreatBytesAsNullForScript && (this.BytesAsParameter(value) || this.NeedInsertParameter(value)))
                    {
                        string parameterName = $"P{pageNumber}_{rowIndex}_{column.Name}";
                        parameters.Add(this.CommandParameterChar + parameterName, value);

                        string parameterPlaceholder = this.CommandParameterChar + parameterName;
                        values.Add(parameterPlaceholder);
                        parameterPlaceholders.Add(parameterPlaceholder);
                    }
                    else
                    {
                        values.Add(value);
                    }
                }
            }

            valuesWithoutParameter = $"({string.Join(",", values.Select(item => parameterPlaceholders.Contains(item) ? "NULL" : item))})";

            return $"({string.Join(",", values.Select(item => item))})";
        }

        private bool BytesAsParameter(object value)
        {
            return this.IsBytes(value);
        }

        public bool IsBytes(object value)
        {
            return value != null && value.GetType() == typeof(byte[]);
        }

        protected virtual bool NeedInsertParameter(object value)
        {
            return false;
        }

        private object ParseValue(TableColumn column, object value, bool byteAsString = false)
        {
            if (value != null)
            {
                Type type = value.GetType();
                bool needQuotated = false;
                string strValue = "";

                if (type == typeof(DBNull))
                {
                    return "NULL";
                }
                else if (type == typeof(Byte[]))
                {
                    if (((Byte[])value).Length == 16) //GUID
                    {
                        if (this.GetType() == typeof(SqlServerInterpreter) && column.DataType.ToLower() == "uniqueidentifier")
                        {
                            needQuotated = true;
                            strValue = new Guid((byte[])value).ToString();
                        }
                        else if (this.GetType() == typeof(MySqlInterpreter) && column.DataType == "char" && column.MaxLength == 36)
                        {
                            needQuotated = true;
                            strValue = new Guid((byte[])value).ToString();
                        }
                        else if (byteAsString && this.GetType() == typeof(OracleInterpreter) && column.DataType.ToLower() == "raw" && column.MaxLength == 16)
                        {
                            needQuotated = true;
                            strValue = StringHelper.GuidToRaw(new Guid((byte[])value).ToString());
                        }
                        else
                        {
                            return value;
                        }
                    }
                    else
                    {
                        return value;
                    }
                }

                bool oracleSemicolon = false;

                switch (type.Name)
                {
                    case nameof(Guid):
                        needQuotated = true;
                        if (this.GetType() == typeof(OracleInterpreter) && column.DataType.ToLower() == "raw" && column.MaxLength == 16)
                        {
                            strValue = StringHelper.GuidToRaw(value.ToString());
                        }
                        else
                        {
                            strValue = value.ToString();
                        }
                        break;
                    case nameof(String):
                        needQuotated = true;
                        strValue = value.ToString();
                        if (this.GetType() == typeof(OracleInterpreter))
                        {
                            if (strValue.Contains(";"))
                            {
                                oracleSemicolon = true;
                            }
                        }
                        break;
                    case nameof(DateTime):
                        if (this.GetType() == typeof(OracleInterpreter))
                        {
                            strValue = $"TO_TIMESTAMP('{Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss.fff")}','yyyy-MM-dd hh24:mi:ss.ff')";
                        }
                        else
                        {
                            needQuotated = true;
                            strValue = value.ToString();
                        }
                        break;
                    case nameof(Boolean):
                        strValue = value.ToString() == "True" ? "1" : "0";
                        break;
                    default:
                        if (string.IsNullOrEmpty(strValue))
                        {
                            strValue = value.ToString();
                        }
                        break;
                }

                if (needQuotated)
                {
                    strValue = $"{this.UnicodeInsertChar}'{ValueHelper.TransferSingleQuotation(strValue)}'";

                    if (oracleSemicolon)
                    {
                        strValue = strValue.Replace(";", $"'{OracleInterpreter.CONNECT_CHAR}{OracleInterpreter.SEMICOLON_FUNC}{OracleInterpreter.CONNECT_CHAR}'");
                    }

                    return strValue;
                }
                else
                {
                    return strValue;
                }
            }
            else
            {
                return null;
            }
        }

        protected virtual string GetUnicodeInsertChar()
        {
            return this.UnicodeInsertChar;
        }

        protected virtual string GetColumnDefaultValue(TableColumn column)
        {
            bool isChar = DataTypeHelper.IsCharType(column.DataType);
            if (isChar && !column.DefaultValue.StartsWith("'"))
            {
                return $"'{column.DefaultValue}'";
            }
            return column.DefaultValue;
        }
        #endregion
    }
}
