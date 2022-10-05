using Dapper;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public abstract class DbInterpreter
    {
        #region Field & Property       
        private IObserver<FeedbackInfo> observer;
        protected DbConnector dbConnector;
        protected bool hasError = false;

        public readonly DateTime MinDateTime = new DateTime(1970, 1, 1);
        public const string RowNumberColumnName = "_ROWNUMBER";
        public virtual string UnicodeInsertChar { get; } = "N";
        public virtual string ScriptsDelimiter => ";";
        public abstract string CommentString { get; }
        public bool ShowBuiltinDatabase => SettingManager.Setting.ShowBuiltinDatabase;
        public DbObjectNameMode DbObjectNameMode => SettingManager.Setting.DbObjectNameMode;
        public int DataBatchSize => SettingManager.Setting.DataBatchSize;
        public bool NotCreateIfExists => SettingManager.Setting.NotCreateIfExists;
        public abstract string CommandParameterChar { get; }
        public abstract char QuotationLeftChar { get; }
        public abstract char QuotationRightChar { get; }
        public virtual IndexType IndexType => IndexType.Normal;
        public abstract DatabaseType DatabaseType { get; }
        public abstract string DefaultDataType { get; }
        public abstract string DefaultSchema { get; }
        public abstract bool SupportBulkCopy { get; }
        public virtual List<string> BuiltinDatabases { get; } = new List<string>();
        public bool CancelRequested { get; set; }
        public bool HasError => this.hasError;
        public DbInterpreterOption Option { get; set; } = new DbInterpreterOption();
        public ConnectionInfo ConnectionInfo { get; protected set; }

        public delegate Task DataReadHandler(TableDataReadInfo tableDataReadInfo);
        public event DataReadHandler OnDataRead;

        #endregion

        #region Constructor     

        public DbInterpreter(ConnectionInfo connectionInfo, DbInterpreterOption option)
        {
            this.ConnectionInfo = connectionInfo;
            this.Option = option;
        }
        #endregion     

        #region Schema Informatioin
        #region Database
        public abstract Task<List<Database>> GetDatabasesAsync();
        #endregion

        #region Database Schema
        public abstract Task<List<DatabaseSchema>> GetDatabaseSchemasAsync();
        #endregion

        #region User Defined Type     
        public abstract Task<List<UserDefinedType>> GetUserDefinedTypesAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<UserDefinedType>> GetUserDefinedTypesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);

        #endregion

        #region Sequence     
        public abstract Task<List<Sequence>> GetSequencesAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<Sequence>> GetSequencesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);

        #endregion

        #region Function        
        public abstract Task<List<Function>> GetFunctionsAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<Function>> GetFunctionsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);
        #endregion

        #region Table      
        public abstract Task<List<Table>> GetTablesAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<Table>> GetTablesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);
        #endregion

        #region Table Column
        public abstract Task<List<TableColumn>> GetTableColumnsAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<TableColumn>> GetTableColumnsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);
        #endregion

        #region Table Primary Key
        public abstract Task<List<TablePrimaryKeyItem>> GetTablePrimaryKeyItemsAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<TablePrimaryKeyItem>> GetTablePrimaryKeyItemsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);

        public virtual async Task<List<TablePrimaryKey>> GetTablePrimaryKeysAsync(SchemaInfoFilter filter = null)
        {
            List<TablePrimaryKeyItem> primaryKeyItems = await this.GetTablePrimaryKeyItemsAsync(filter);
            return SchemaInfoHelper.GetTablePrimaryKeys(primaryKeyItems);
        }

        public virtual async Task<List<TablePrimaryKey>> GetTablePrimaryKeysAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            List<TablePrimaryKeyItem> primaryKeyItems = await this.GetTablePrimaryKeyItemsAsync(dbConnection, filter);
            return SchemaInfoHelper.GetTablePrimaryKeys(primaryKeyItems);
        }
        #endregion

        #region Table Foreign Key       
        public abstract Task<List<TableForeignKeyItem>> GetTableForeignKeyItemsAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<TableForeignKeyItem>> GetTableForeignKeyItemsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);

        public virtual async Task<List<TableForeignKey>> GetTableForeignKeysAsync(SchemaInfoFilter filter = null)
        {
            List<TableForeignKeyItem> foreignKeyItems = await this.GetTableForeignKeyItemsAsync(filter);
            return SchemaInfoHelper.GetTableForeignKeys(foreignKeyItems);
        }

        public virtual async Task<List<TableForeignKey>> GetTableForeignKeysAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            List<TableForeignKeyItem> foreignKeyItems = await this.GetTableForeignKeyItemsAsync(dbConnection, filter);
            return SchemaInfoHelper.GetTableForeignKeys(foreignKeyItems);
        }
        #endregion

        #region Table Index
        public abstract Task<List<TableIndexItem>> GetTableIndexItemsAsync(SchemaInfoFilter filter = null, bool includePrimaryKey = false);
        public abstract Task<List<TableIndexItem>> GetTableIndexItemsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null, bool includePrimaryKey = false);

        public virtual async Task<List<TableIndex>> GetTableIndexesAsync(SchemaInfoFilter filter = null, bool includePrimaryKey = false)
        {
            List<TableIndexItem> indexItems = await this.GetTableIndexItemsAsync(filter, includePrimaryKey);
            return SchemaInfoHelper.GetTableIndexes(indexItems);
        }

        public virtual async Task<List<TableIndex>> GetTableIndexesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null, bool includePrimaryKey = false)
        {
            List<TableIndexItem> indexItems = await this.GetTableIndexItemsAsync(dbConnection, filter, includePrimaryKey);
            return SchemaInfoHelper.GetTableIndexes(indexItems);
        }
        #endregion

        #region Table Trigger        
        public abstract Task<List<TableTrigger>> GetTableTriggersAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<TableTrigger>> GetTableTriggersAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);
        #endregion

        #region Table Constraint
        public abstract Task<List<TableConstraint>> GetTableConstraintsAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<TableConstraint>> GetTableConstraintsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);

        #endregion

        #region View        
        public abstract Task<List<View>> GetViewsAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<View>> GetViewsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);
        #endregion     

        #region Procedure        
        public abstract Task<List<Procedure>> GetProceduresAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<Procedure>> GetProceduresAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);
        #endregion

        #region SchemaInfo
        protected async Task<List<T>> GetDbObjectsAsync<T>(string sql) where T : DatabaseObject
        {
            if (!string.IsNullOrEmpty(sql))
            {
                using (DbConnection dbConnection = this.CreateConnection())
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
                try
                {
                    await this.OpenConnectionAsync(dbConnection);

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
                catch (Exception ex)
                {
                    if (this.Option.ThrowExceptionWhenErrorOccurs)
                    {
                        throw ex;
                    }

                    this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex));
                }
            }

            this.FeedbackInfo($"Got {objects.Count} {StringHelper.GetFriendlyTypeName(typeof(T).Name).ToLower()}(s).");

            return objects;
        }

        public virtual async Task<SchemaInfo> GetSchemaInfoAsync(SchemaInfoFilter filter = null)
        {
            if (filter == null)
            {
                filter = new SchemaInfoFilter();
            }

            this.FeedbackInfo("Getting schema info...");

            DatabaseObjectType dbObjectType = filter.DatabaseObjectType;

            SchemaInfo schemaInfo = new SchemaInfo();

            using (DbConnection connection = this.CreateConnection())
            {
                if (this.NeedFetchObjects(DatabaseObjectType.UserDefinedType, filter.UserDefinedTypeNames, filter))
                {
                    schemaInfo.UserDefinedTypes = await this.GetUserDefinedTypesAsync(connection, filter);
                }

                if (this.NeedFetchObjects(DatabaseObjectType.Sequence, filter.SequenceNames, filter))
                {
                    schemaInfo.Sequences = await this.GetSequencesAsync(connection, filter);
                }

                if (this.NeedFetchObjects(DatabaseObjectType.Function, filter.FunctionNames, filter))
                {
                    schemaInfo.Functions = await this.GetFunctionsAsync(connection, filter);
                }

                if (this.NeedFetchObjects(DatabaseObjectType.Table, filter.TableNames, filter))
                {
                    schemaInfo.Tables = await this.GetTablesAsync(connection, filter);
                }

                if (this.NeedFetchObjects(DatabaseObjectType.View, filter.ViewNames, filter))
                {
                    schemaInfo.Views = await this.GetViewsAsync(connection, filter);
                }

                if (this.NeedFetchObjects(DatabaseObjectType.Procedure, filter.ProcedureNames, filter))
                {
                    schemaInfo.Procedures = await this.GetProceduresAsync(connection, filter);
                }

                if (this.NeedFetchTableObjects(DatabaseObjectType.TableColumn, filter, null))
                {
                    schemaInfo.TableColumns = await this.GetTableColumnsAsync(connection, filter);
                }

                if (this.NeedFetchTableObjects(DatabaseObjectType.TablePrimaryKey, filter, null))
                {
                    schemaInfo.TablePrimaryKeys = await this.GetTablePrimaryKeysAsync(connection, filter);
                }

                if ((this.Option.SortObjectsByReference && schemaInfo.Tables.Count > 1) || this.NeedFetchTableObjects(DatabaseObjectType.TableForeignKey, filter, null))
                {
                    schemaInfo.TableForeignKeys = await this.GetTableForeignKeysAsync(connection, filter);
                }

                if (this.NeedFetchTableObjects(DatabaseObjectType.TableIndex, filter, null))
                {
                    schemaInfo.TableIndexes = await this.GetTableIndexesAsync(connection, filter, this.Option.IncludePrimaryKeyWhenGetTableIndex);
                }

                if (this.NeedFetchTableObjects(DatabaseObjectType.TableConstraint, filter, null))
                {
                    schemaInfo.TableConstraints = await this.GetTableConstraintsAsync(connection, filter);
                }

                if (this.NeedFetchTableObjects(DatabaseObjectType.TableTrigger, filter, filter.TableTriggerNames))
                {
                    schemaInfo.TableTriggers = await this.GetTableTriggersAsync(connection, filter);
                }
            }

            if (this.Option.SortObjectsByReference)
            {
                if (schemaInfo.Tables.Count > 1)
                {
                    schemaInfo.Tables = TableReferenceHelper.ResortTables(schemaInfo.Tables, schemaInfo.TableForeignKeys);
                }

                DbObjectHelper.Resort(schemaInfo.Views);
                DbObjectHelper.Resort(schemaInfo.Functions);
                DbObjectHelper.Resort(schemaInfo.Procedures);
            }

            this.FeedbackInfo("End get schema info.");

            return schemaInfo;
        }

        private bool NeedFetchTableObjects(DatabaseObjectType currentObjectType, SchemaInfoFilter filter, string[] childrenNames)
        {
            var filterNames = (filter.TableNames ?? Enumerable.Empty<string>()).Union(childrenNames ?? Enumerable.Empty<string>());

            return this.Option.GetTableAllObjects || this.NeedFetchObjects(currentObjectType, filterNames, filter);
        }

        private bool NeedFetchObjects(DatabaseObjectType currentObjectType, IEnumerable<string> names, SchemaInfoFilter filter)
        {
            bool hasName = names != null && names.Any();

            if (filter.Strict)
            {
                return hasName && filter.DatabaseObjectType.HasFlag(currentObjectType);
            }
            else
            {
                return hasName || filter.DatabaseObjectType.HasFlag(currentObjectType);
            }
        }
        #endregion
        #endregion

        #region Database Operation

        public abstract DbConnector GetDbConnector();

        public DbConnection CreateConnection()
        {
            DbConnection dbConnection = this.dbConnector.CreateConnection();

            if (this.Option.RequireInfoMessage)
            {
                this.SubscribeInfoMessage(dbConnection);
            }

            return dbConnection;
        }

        public async Task OpenConnectionAsync(DbConnection connection)
        {
            if (connection.State == ConnectionState.Closed)
            {
                await connection.OpenAsync();
            }
        }

        public Task<int> ExecuteNonQueryAsync(string sql)
        {
            return this.InternalExecuteNonQuery(this.CreateConnection(), new CommandInfo() { CommandText = sql });
        }

        public Task<int> ExecuteNonQueryAsync(CommandInfo commandInfo)
        {
            return this.InternalExecuteNonQuery(this.CreateConnection(), commandInfo, true);
        }

        public Task<int> ExecuteNonQueryAsync(DbConnection dbConnection, string sql, bool disposeConnection = false)
        {
            return this.InternalExecuteNonQuery(dbConnection, new CommandInfo() { CommandText = sql }, disposeConnection);
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

            DbCommand command = dbConnection.CreateCommand();
            command.CommandType = commandInfo.CommandType;
            command.CommandText = commandInfo.CommandText;
            command.CommandTimeout = SettingManager.Setting.CommandTimeout;

            if (this.Option.RequireInfoMessage)
            {
                this.SubscribeInfoMessage(command);
            }

            if (commandInfo.Transaction != null)
            {
                command.Transaction = commandInfo.Transaction;
            }

            if (commandInfo.Parameters != null)
            {
                foreach (var kp in commandInfo.Parameters)
                {
                    DbParameter dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = kp.Key;
                    dbParameter.Value = kp.Value;

                    command.Parameters.Add(dbParameter);
                }
            }

            Func<Task<int>> exec = async () =>
            {
                bool wasClosed = dbConnection.State == ConnectionState.Closed;

                try
                {
                    if (wasClosed)
                    {
                        await dbConnection.OpenAsync(commandInfo.CancellationToken).ConfigureAwait(false);
                    }

                    int result = await command.ExecuteNonQueryAsync(commandInfo.CancellationToken).ConfigureAwait(false);

                    return result;
                }
                catch (Exception ex)
                {
                    commandInfo.HasError = true;

                    if (!commandInfo.ContinueWhenErrorOccurs)
                    {
                        if (dbConnection.State == ConnectionState.Open && command.Transaction != null)
                        {
                            command.Transaction.Rollback();
                            commandInfo.TransactionRollbacked = true;
                        }
                    }

                    if (this.Option.ThrowExceptionWhenErrorOccurs)
                    {
                        throw ex;
                    }

                    this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex), commandInfo.ContinueWhenErrorOccurs);

                    return 0;
                }
                finally
                {
                    if (disposeConnection && wasClosed && dbConnection != null && dbConnection.State != ConnectionState.Closed)
                    {
                        dbConnection.Close();
                    }
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

        public abstract Task BulkCopyAsync(DbConnection connection, DataTable dataTable, BulkCopyInfo bulkCopyInfo);

        public abstract Task<long> GetTableRecordCountAsync(DbConnection connection, Table table, string whereClause = "");

        protected async Task<long> GetTableRecordCountAsync(DbConnection connection, string sql)
        {
            return await connection.ExecuteScalarAsync<long>(sql);
        }

        public object GetScalar(string sql)
        {
            return this.GetScalar(this.CreateConnection(), sql);
        }

        public object GetScalar(DbConnection dbConnection, string sql)
        {
            return dbConnection.ExecuteScalar(sql);
        }

        public async Task<object> GetScalarAsync(string sql)
        {
            return await this.GetScalarAsync(this.CreateConnection(), sql);
        }

        public async Task<object> GetScalarAsync(DbConnection dbConnection, string sql)
        {
            return await dbConnection.ExecuteScalarAsync(sql);
        }

        public async Task<DbDataReader> GetDataReaderAsync(string sql)
        {
            return await this.GetDataReaderAsync(this.CreateConnection(), sql);
        }

        public async Task<DbDataReader> GetDataReaderAsync(DbConnection dbConnection, string sql)
        {
            return await dbConnection.ExecuteReaderAsync(sql);
        }

        public IDataReader GetDataReader(DbConnection dbConnection, string sql)
        {
            return dbConnection.ExecuteReader(sql);
        }

        public async Task<DataTable> GetDataTableAsync(DbConnection dbConnection, string sql, int? limitCount = null)
        {
            if (limitCount > 0)
            {
                sql = this.AppendLimitClause(sql, limitCount.Value);
            }

            if (this.DatabaseType == DatabaseType.Postgres)
            {
                NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite(geographyAsDefault: false);
            }

            DbDataReader reader = await dbConnection.ExecuteReaderAsync(sql);

            DataTable table = new DataTable();
            table.Load(reader);

            return table;
        }

        protected string AppendLimitClause(string sql, int limitCount)
        {
            sql = sql.TrimEnd(';', '\r', '\n');

            int index = sql.LastIndexOf(')');

            string select = index > 0 ? sql.Substring(index + 1) : sql;

            if (!Regex.IsMatch(select, @"(LIMIT|OFFSET)(.[\n]?)+", RegexOptions.IgnoreCase))
            {
                if (!Regex.IsMatch(select, @"ORDER[\s]+BY", RegexOptions.IgnoreCase))
                {
                    string orderBy = this.GetDefaultOrder();

                    if (!string.IsNullOrEmpty(orderBy))
                    {
                        sql += Environment.NewLine + "ORDER BY " + orderBy;
                    }
                }

                if (this.DatabaseType == DatabaseType.SqlServer)
                {
                    if (!Regex.IsMatch(select, @"SELECT[\s]+TOP[\s]+", RegexOptions.IgnoreCase))
                    {
                        sql = sql.Substring(0, index + 1) + Regex.Replace(select, "SELECT", $"SELECT TOP {limitCount} ", RegexOptions.IgnoreCase);
                    }
                }
                else if (this.DatabaseType == DatabaseType.MySql || this.DatabaseType == DatabaseType.Postgres)
                {
                    sql = $@"SELECT * FROM
                       (
                         {sql}
                       ) TEMP"
                      + Environment.NewLine + this.GetLimitStatement(0, limitCount);
                }
                else if (this.DatabaseType == DatabaseType.Oracle)
                {
                    sql = $@"SELECT * FROM
                       (
                         {sql}
                       ) TEMP
                       WHERE ROWNUM BETWEEN 1 AND {limitCount}";
                }
            }

            return sql;
        }

        public async Task<DataTable> GetPagedDataTableAsync(DbConnection connection, Table table, List<TableColumn> columns, string orderColumns, long total, int pageSize, long pageNumber, string whereClause = "")
        {
            string quotedTableName = this.GetQuotedDbObjectNameWithSchema(table);

            List<string> columnNames = new List<string>();

            foreach (TableColumn column in columns)
            {
                string columnName = this.GetQuotedString(column.Name);
                string dataType = column.DataType.ToLower();
                #region MySql
                if (this.DatabaseType == DatabaseType.MySql)
                {
                    // Convert float to decimal, avoid scientific notation
                    if (dataType.Contains("float"))
                    {
                        DataTypeInfo dataTypeInfo = DataTypeHelper.GetDataTypeInfo(column.DataType);

                        if (!string.IsNullOrEmpty(dataTypeInfo.Args))
                        {
                            string strPrecision = dataTypeInfo.Args.Split(',')[0].Trim();
                            int precision;

                            DataTypeSpecification dataTypeSpec = this.GetDataTypeSpecification("decimal");

                            if (dataTypeSpec != null)
                            {
                                ArgumentRange? precisionRange = DataTypeManager.GetArgumentRange(dataTypeSpec, "precision");

                                if (precisionRange.HasValue)
                                {
                                    if (int.TryParse(strPrecision, out precision) && precision > 0 && precision <= precisionRange.Value.Max)
                                    {
                                        columnName = $"CONVERT({columnName},DECIMAL({dataTypeInfo.Args})) AS {columnName}";
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Oralce
                else if (this.DatabaseType == DatabaseType.Oracle)
                {
                    if (dataType == "st_geometry")
                    {
                        string geometryMode = SettingManager.Setting.OracleGeometryMode;

                        if (string.IsNullOrEmpty(geometryMode) || geometryMode == "MDSYS")
                        {
                            quotedTableName += " t"; //must use alias
                            columnName = $"t.{columnName}.GET_WKT() AS {columnName}";
                        }
                        else if (geometryMode == "SDE")
                        {
                            columnName = $"SDE.ST_ASTEXT({columnName}) AS {columnName}";
                        }
                    }
                }
                #endregion

                columnNames.Add(columnName);
            }

            string strColumnNames = string.Join(",", columnNames);

            string pagedSql = this.GetSqlForPagination(quotedTableName, strColumnNames, orderColumns, whereClause, pageNumber, pageSize);

            DataTable dt = await this.GetDataTableAsync(connection, pagedSql);

            if (dt.Columns.OfType<DataColumn>().Any(item => item.ColumnName == RowNumberColumnName))
            {
                dt.Columns.Remove(RowNumberColumnName);
            }

            return dt;
        }

        public async Task<(long, DataTable)> GetPagedDataTableAsync(Table table, string orderColumns, int pageSize, long pageNumber, string whereClause = "")
        {
            using (DbConnection connection = this.CreateConnection())
            {
                long total = await this.GetTableRecordCountAsync(connection, table, whereClause);

                List<TableColumn> columns = await this.GetTableColumnsAsync(connection, new SchemaInfoFilter() { TableNames = new string[] { table.Name } });

                DataTable dt = await this.GetPagedDataTableAsync(this.CreateConnection(), table, columns, orderColumns, total, pageSize, pageNumber, whereClause);

                return (total, dt);
            }
        }

        public async Task<Dictionary<long, List<Dictionary<string, object>>>> GetPagedDataListAsync(DbConnection connection, Table table, List<TableColumn> columns, string primaryKeyColumns, long total, int pageSize, string whereClause = "")
        {
            var dictPagedData = new Dictionary<long, List<Dictionary<string, object>>>();

            long pageCount = PaginationHelper.GetPageCount(total, pageSize);

            for (long pageNumber = 1; pageNumber <= pageCount; pageNumber++)
            {
                if (this.CancelRequested)
                {
                    break;
                }

                DataTable dataTable = await this.GetPagedDataTableAsync(connection, table, columns, primaryKeyColumns, total, pageSize, pageNumber, whereClause);

                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var dicField = new Dictionary<string, object>();

                    for (var i = 0; i < dataTable.Columns.Count; i++)
                    {
                        DataColumn column = dataTable.Columns[i];
                        string columnName = column.ColumnName;

                        if (columnName == DbInterpreter.RowNumberColumnName)
                        {
                            continue;
                        }

                        TableColumn tableColumn = columns.FirstOrDefault(item => item.Name == columnName);

                        object value = row[i];

                        if (ValueHelper.IsBytes(value))
                        {
                            if (this.Option.TreatBytesAsNullForReading)
                            {
                                if (!(((Byte[])value).Length == 16) && this.DatabaseType == DatabaseType.Oracle)
                                {
                                    value = null;

                                    if (dataTable.Columns[i].ReadOnly)
                                    {
                                        dataTable.Columns[i].ReadOnly = false;
                                    }

                                    row[i] = null;
                                }
                            }
                        }

                        dicField.Add(columnName, value);
                    }

                    rows.Add(dicField);
                }

                dictPagedData.Add(pageNumber, rows);

                if (this.OnDataRead != null && !this.CancelRequested && !this.HasError)
                {
                    await this.OnDataRead(new TableDataReadInfo()
                    {
                        Table = table,
                        Columns = columns,
                        TotalCount = total,
                        Data = rows,
                        DataTable = dataTable
                    });
                }
            }

            return dictPagedData;
        }

        #endregion

        #region Sql Query Clause
        public virtual string GetDefaultOrder() { return string.Empty; }
        public virtual string GetLimitStatement(int limitStart, int limitCount) { return string.Empty; }

        protected abstract string GetSqlForPagination(string tableName, string columnNames, string orderColumns, string whereClause, long pageNumber, int pageSize);
        #endregion

        #region Common Method 
        public string GetQuotedDbObjectNameWithSchema(DatabaseObject obj)
        {
            if (this.DatabaseType == DatabaseType.SqlServer || this.DatabaseType == DatabaseType.Postgres)
            {
                if (!string.IsNullOrEmpty(obj.Schema))
                {
                    return $"{this.GetString(obj.Schema, true)}.{this.GetString(obj.Name, true)}";
                }
                else
                {
                    return obj.Name;
                }
            }

            return $"{this.GetString(obj.Name, true)}";
        }

        public string GetQuotedDbObjectNameWithSchema(string schema, string dbObjName)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return this.GetQuotedString(dbObjName);
            }
            else
            {
                return $"{this.GetQuotedString(schema)}.{this.GetQuotedString(dbObjName)}";
            }
        }

        public string GetString(string str, bool useQuotedString = false)
        {
            return useQuotedString ? this.GetQuotedString(str) : str;
        }

        public string GetQuotedString(string str)
        {
            if (str != null && (this.DbObjectNameMode == DbObjectNameMode.WithQuotation || str.Contains(" ")))
            {
                return $"{this.QuotationLeftChar}{str}{this.QuotationRightChar}";
            }
            else
            {
                return str;
            }
        }

        public string GetQuotedColumnNames(IEnumerable<TableColumn> columns)
        {
            return string.Join(",", columns.OrderBy(item => item.Order).Select(item => this.GetQuotedString(item.Name)));
        }

        protected bool IsObjectFectchSimpleMode()
        {
            return this.Option.ObjectFetchMode == DatabaseObjectFetchMode.Simple;
        }

        public string ReplaceSplitChar(string value)
        {
            return value?.Replace(this.ScriptsDelimiter, ",");
        }

        public bool IsLowDbVersion(DbConnection connection)
        {
            string serverVersion = this.ConnectionInfo.ServerVersion;

            if (string.IsNullOrEmpty(serverVersion))
            {
                try
                {
                    bool needClose = false;

                    if (connection == null)
                    {
                        connection = this.CreateConnection();
                        needClose = true;
                    }

                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    serverVersion = connection.ServerVersion;

                    if (needClose)
                    {
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            return this.IsLowDbVersion(serverVersion);
        }

        public abstract bool IsLowDbVersion(string serverVersion);
        protected virtual void SubscribeInfoMessage(DbConnection dbConnection) { }
        protected virtual void SubscribeInfoMessage(DbCommand dbCommand) { }


        #endregion

        #region Parse Column & DataType 
        public abstract string ParseColumn(Table table, TableColumn column);
        public abstract string ParseDataType(TableColumn column);
        public virtual string GetColumnDataLength(TableColumn column) { return string.Empty; }

        public virtual string GetColumnDefaultValue(TableColumn column)
        {
            bool isChar = DataTypeHelper.IsCharType(column.DataType);

            if (isChar && !column.DefaultValue.Trim('(', ')').StartsWith("'"))
            {
                return $"'{column.DefaultValue}'";
            }

            return column.DefaultValue?.Trim();
        }

        protected virtual string GetColumnComputeExpression(TableColumn column)
        {
            string computeExpression = column.ComputeExp.Trim();

            if (computeExpression.StartsWith("(") && computeExpression.EndsWith(")"))
            {
                return computeExpression;
            }
            else
            {
                return $"({computeExpression})";
            }
        }

        public DataTypeSpecification GetDataTypeSpecification(string dataType)
        {
            return DataTypeManager.GetDataTypeSpecification(this.DatabaseType, dataType);
        }

        public virtual bool IsNoLengthDataType(string dataType)
        {
            IEnumerable<DataTypeSpecification> dataTypeSpecs = DataTypeManager.GetDataTypeSpecifications(this.DatabaseType);

            return dataTypeSpecs.Any(item => item.Name.ToUpper() == dataType.ToUpper() && string.IsNullOrEmpty(item.Args));
        }

        public string GetDataTypePrecisionScale(TableColumn column, string dataType)
        {
            DataTypeSpecification dataTypeSpecification = this.GetDataTypeSpecification(dataType);

            if (dataTypeSpecification != null)
            {
                long precision = column.Precision.HasValue ? column.Precision.Value : 0;
                int scale = column.Scale.HasValue ? column.Scale.Value : 0;

                if (dataTypeSpecification.Args.Contains(","))
                {
                    if (precision > 0)
                    {
                        return $"{precision},{scale}";
                    }
                }
                else if (dataTypeSpecification.Args == "scale")
                {
                    ArgumentRange? range = DataTypeManager.GetArgumentRange(dataTypeSpecification, "scale");

                    if (range.HasValue)
                    {
                        if (scale > range.Value.Max)
                        {
                            scale = range.Value.Max;
                        }
                    }

                    return $"{scale}";
                }
            }

            return string.Empty;
        }
        #endregion

        #region Feedback
        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public void Feedback(FeedbackInfoType infoType, string message)
        {
            FeedbackInfo info = new FeedbackInfo() { Owner = this, InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(message) };

            if (this.observer != null)
            {
                FeedbackHelper.Feedback(this.observer, info);
            }
        }

        public void FeedbackInfo(string message)
        {
            this.Feedback(FeedbackInfoType.Info, message);
        }

        public void FeedbackError(string message, bool skipError = false)
        {
            if (!skipError)
            {
                this.hasError = true;
            }

            this.Feedback(FeedbackInfoType.Error, message);
        }

        public void FeedbackInfo(OperationState state, DatabaseObject dbObject)
        {
            string message = $"{state.ToString()}{(state == OperationState.Begin ? " to" : "")} generate script for {StringHelper.GetFriendlyTypeName(dbObject.GetType().Name).ToLower()} \"{dbObject.Name}\".";
            this.Feedback(FeedbackInfoType.Info, message);
        }
        #endregion
    }
}
