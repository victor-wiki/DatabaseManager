using Dapper;
using DatabaseInterpreter.Geometry;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgGeom = NetTopologySuite.Geometries;

namespace DatabaseInterpreter.Core
{
    public abstract class DbInterpreter
    {
        #region Field & Property       
        private IObserver<FeedbackInfo> observer;
        protected DbConnector dbConnector;
        protected bool hasError = false;

        public string ServerVersion => this.ConnectionInfo?.ServerVersion;
        public readonly DateTime MinDateTime = new DateTime(1970, 1, 1);
        public const string RowNumberColumnName = "_ROWNUMBER";
        public virtual string UnicodeLeadingFlag { get; } = "N";
        public virtual string STR_CONCAT_CHARS { get; }
        public virtual string ScriptsDelimiter => ";";
        public abstract string CommentString { get; }
        public bool ShowBuiltinDatabase => Setting.ShowBuiltinDatabase;
        public DbObjectNameMode DbObjectNameMode => Setting.DbObjectNameMode;
        public int DataBatchSize => Setting.DataBatchSize;
        public bool NotCreateIfExists => Setting.NotCreateIfExists;
        public abstract string CommandParameterChar { get; }
        public abstract bool SupportQuotationChar { get; }
        public virtual char QuotationLeftChar { get; }
        public virtual char QuotationRightChar { get; }
        public virtual IndexType IndexType => IndexType.Normal;
        public abstract DatabaseObjectType SupportDbObjectType { get; }
        public abstract DatabaseType DatabaseType { get; }
        public abstract string DefaultDataType { get; }
        public abstract string DefaultSchema { get; }
        public abstract bool SupportBulkCopy { get; }
        public abstract bool SupportNchar { get; }
        public virtual List<string> BuiltinDatabases { get; } = new List<string>();
        public bool CancelRequested { get; set; }
        public bool HasError => this.hasError;
        public static DbInterpreterSetting Setting = new DbInterpreterSetting();
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
        public abstract Task<List<DatabaseSchema>> GetDatabaseSchemasAsync(DbConnection dbConnection);
        #endregion

        #region User Defined Type     
        public abstract Task<List<UserDefinedTypeAttribute>> GetUserDefinedTypeAttributesAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<UserDefinedTypeAttribute>> GetUserDefinedTypeAttributesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);

        public virtual async Task<List<UserDefinedType>> GetUserDefinedTypesAsync(SchemaInfoFilter filter = null)
        {
            List<UserDefinedTypeAttribute> attributes = await this.GetUserDefinedTypeAttributesAsync(filter);
            return SchemaInfoHelper.GetUserDefinedTypes(attributes);
        }
        public virtual async Task<List<UserDefinedType>> GetUserDefinedTypesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            List<UserDefinedTypeAttribute> attributes = await this.GetUserDefinedTypeAttributesAsync(dbConnection, filter);
            return SchemaInfoHelper.GetUserDefinedTypes(attributes);
        }
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
        public abstract Task<List<TableForeignKeyItem>> GetTableForeignKeyItemsAsync(SchemaInfoFilter filter = null, bool isFilterForReferenced = false);
        public abstract Task<List<TableForeignKeyItem>> GetTableForeignKeyItemsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null, bool isFilterForReferenced = false);

        public virtual async Task<List<TableForeignKey>> GetTableForeignKeysAsync(SchemaInfoFilter filter = null, bool isFilterForReferenced = false)
        {
            List<TableForeignKeyItem> foreignKeyItems = await this.GetTableForeignKeyItemsAsync(filter, isFilterForReferenced);
            return SchemaInfoHelper.GetTableForeignKeys(foreignKeyItems);
        }

        public virtual async Task<List<TableForeignKey>> GetTableForeignKeysAsync(DbConnection dbConnection, SchemaInfoFilter filter = null, bool isFilterForReferenced = false)
        {
            List<TableForeignKeyItem> foreignKeyItems = await this.GetTableForeignKeyItemsAsync(dbConnection, filter, isFilterForReferenced);
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

        #region Routine Parameter        
        public abstract Task<List<RoutineParameter>> GetFunctionParametersAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<RoutineParameter>> GetFunctionParametersAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);

        public abstract Task<List<RoutineParameter>> GetProcedureParametersAsync(SchemaInfoFilter filter = null);
        public abstract Task<List<RoutineParameter>> GetProcedureParametersAsync(DbConnection dbConnection, SchemaInfoFilter filter = null);
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
                    this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex));

                    if (this.Option.ThrowExceptionWhenErrorOccurs)
                    {
                        throw ex;
                    }
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
                if (this.NeedFetchObjects(DatabaseObjectType.Type, filter.UserDefinedTypeNames, filter))
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

                if (this.NeedFetchTableObjects(DatabaseObjectType.Column, filter, null))
                {
                    schemaInfo.TableColumns = await this.GetTableColumnsAsync(connection, filter);
                }

                if (this.NeedFetchTableObjects(DatabaseObjectType.PrimaryKey, filter, null))
                {
                    schemaInfo.TablePrimaryKeys = await this.GetTablePrimaryKeysAsync(connection, filter);
                }

                if ((this.Option.SortObjectsByReference && schemaInfo.Tables.Count > 1) || this.NeedFetchTableObjects(DatabaseObjectType.ForeignKey, filter, null))
                {
                    schemaInfo.TableForeignKeys = await this.GetTableForeignKeysAsync(connection, filter);
                }

                if (this.NeedFetchTableObjects(DatabaseObjectType.Index, filter, null))
                {
                    schemaInfo.TableIndexes = await this.GetTableIndexesAsync(connection, filter, this.Option.IncludePrimaryKeyWhenGetTableIndex);
                }

                if (this.NeedFetchTableObjects(DatabaseObjectType.Constraint, filter, null))
                {
                    schemaInfo.TableConstraints = await this.GetTableConstraintsAsync(connection, filter);
                }

                if (this.NeedFetchTableObjects(DatabaseObjectType.Trigger, filter, filter.TableTriggerNames))
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

        #region Dependency
        public abstract Task<List<ViewTableUsage>> GetViewTableUsages(SchemaInfoFilter filter, bool isFilterForReferenced = false);
        public abstract Task<List<ViewTableUsage>> GetViewTableUsages(DbConnection dbConnection, SchemaInfoFilter filter, bool isFilterForReferenced = false);
        public abstract Task<List<ViewColumnUsage>> GetViewColumnUsages(SchemaInfoFilter filter);
        public abstract Task<List<ViewColumnUsage>> GetViewColumnUsages(DbConnection dbConnection, SchemaInfoFilter filter);
        public abstract Task<List<RoutineScriptUsage>> GetRoutineScriptUsages(SchemaInfoFilter filter, bool isFilterForReferenced = false, bool includeViewTableUsages = false);
        public abstract Task<List<RoutineScriptUsage>> GetRoutineScriptUsages(DbConnection dbConnection, SchemaInfoFilter filter, bool isFilterForReferenced = false, bool includeViewTableUsages = false);

        protected async Task<List<T>> GetDbObjectUsagesAsync<T>(string sql) where T : DbObjectUsage
        {
            if (!string.IsNullOrEmpty(sql))
            {
                using (DbConnection dbConnection = this.CreateConnection())
                {
                    return await this.GetDbObjectUsagesAsync<T>(dbConnection, sql);
                }
            }

            return new List<T>();
        }

        protected async Task<List<T>> GetDbObjectUsagesAsync<T>(DbConnection dbConnection, string sql) where T : DbObjectUsage
        {
            List<T> objects = new List<T>();

            if (!string.IsNullOrEmpty(sql))
            {
                try
                {
                    await this.OpenConnectionAsync(dbConnection);

                    objects = (await dbConnection.QueryAsync<T>(sql)).ToList();
                }
                catch (Exception ex)
                {
                    this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex));

                    if (this.Option.ThrowExceptionWhenErrorOccurs)
                    {
                        throw ex;
                    }
                }
            }

            this.FeedbackInfo($"Got {objects.Count} {StringHelper.GetFriendlyTypeName(typeof(T).Name).ToLower()}(s).");

            return objects;
        }
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
            command.CommandTimeout = Setting.CommandTimeout;

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
                bool isClosed = dbConnection.State == ConnectionState.Closed;

                try
                {
                    if (isClosed)
                    {
                        await dbConnection.OpenAsync(commandInfo.CancellationToken).ConfigureAwait(false);
                    }

                    int result = await command.ExecuteNonQueryAsync(commandInfo.CancellationToken).ConfigureAwait(false);

                    return result;
                }
                catch (Exception ex)
                {
                    commandInfo.HasError = true;

                    bool hasRollbackedTransaction = false;

                    if (!commandInfo.ContinueWhenErrorOccurs)
                    {
                        if (dbConnection.State == ConnectionState.Open && command.Transaction != null)
                        {
                            command.Transaction.Rollback();
                            commandInfo.TransactionRollbacked = true;

                            hasRollbackedTransaction = true;
                        }
                    }

                    this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex), commandInfo.ContinueWhenErrorOccurs);

                    if (this.Option.ThrowExceptionWhenErrorOccurs && !commandInfo.ContinueWhenErrorOccurs)
                    {
                        throw new DbCommandException(ex) { HasRollbackedTransaction = hasRollbackedTransaction };
                    }

                    return 0;
                }
                finally
                {
                    if (disposeConnection && isClosed && dbConnection != null && dbConnection.State != ConnectionState.Closed)
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

        public virtual Task<long> GetTableRecordCountAsync(DbConnection connection, Table table, string whereClause = "")
        {
            return this.GetRecordCount(table as DatabaseObject, connection, whereClause);
        }

        private Task<long> GetRecordCount(DatabaseObject dbObject, DbConnection connection, string whereClause = "")
        {
            string sql = $"SELECT COUNT(1) FROM {this.GetQuotedDbObjectNameWithSchema(dbObject)}";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += whereClause;
            }

            return this.GetTableRecordCountAsync(connection, sql);
        }

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

        public async Task<DataTable> GetDataTableAsync(DbConnection dbConnection, string sql)
        {
            if (this.DatabaseType == DatabaseType.Postgres)
            {
                NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite(geographyAsDefault: false);
            }

            if (this.DatabaseType == DatabaseType.Oracle)
            {
                GeometryUtility.Hook();
            }

            DbDataReader reader = await dbConnection.ExecuteReaderAsync(sql);

            DataTable table = new DataTable();
            table.Load(reader);

            return table;
        }

        public async Task<DataTable> GetPagedDataTableAsync(DbConnection connection, Table table, List<TableColumn> columns, string orderColumns, int pageSize, long pageNumber, string whereClause = "")
        {
            string quotedTableName = this.GetQuotedDbObjectNameWithSchema(table as DatabaseObject);

            List<string> columnNames = new List<string>();

            foreach (var column in columns)
            {
                if (this.Option.ExcludeGeometryForData && DataTypeHelper.IsGeometryType(column.DataType))
                {
                    continue;
                }

                if (this.Option.ExcludeIdentityForData && column.IsIdentity)
                {
                    continue;
                }

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
                    else if (dataType == "geometry")
                    {
                        if (this.Option.ShowTextForGeometry)
                        {
                            columnName = $"ST_ASTEXT({columnName}) AS {columnName}";
                        }
                    }
                }
                #endregion

                #region Oralce
                else if (this.DatabaseType == DatabaseType.Oracle)
                {
                    if (dataType == "st_geometry" && column.DataTypeSchema == "SDE")
                    {
                        columnName = $"SDE.ST_ASTEXT({columnName}) AS {columnName}";
                    }
                }
                #endregion

                #region User defined type
                if (column.IsUserDefined)
                {
                    if (this.DatabaseType == DatabaseType.Postgres)
                    {
                        if (!DataTypeHelper.IsGeometryType(column.DataType))
                        {
                            columnName = $@"{columnName}::CHARACTER VARYING AS {columnName}";
                        }
                    }
                    else if (this.DatabaseType == DatabaseType.Oracle)
                    {
                        if (!this.IsLowDbVersion())
                        {
                            columnName = $@"JSON_OBJECT({columnName}) AS {columnName}"; //JSON_OBJECT -> v12.2
                        }
                        else
                        {
                            quotedTableName += " t";

                            OracleInterpreter oracleInterpreter = this as OracleInterpreter;

                            var attributes = await oracleInterpreter.GetUserDefinedTypeAttributesAsync(connection, new SchemaInfoFilter() { UserDefinedTypeNames = new string[] { column.DataType } });

                            StringBuilder sb = new StringBuilder();
                            sb.Append("('('||");

                            int count = 0;

                            foreach (var atrribute in attributes)
                            {
                                if (count > 0)
                                {
                                    sb.Append("||','||");
                                }

                                string attrName = this.GetQuotedString(atrribute.Name);

                                if (!DataTypeHelper.IsCharType(atrribute.DataType))
                                {
                                    sb.Append($"TO_CHAR(t.{columnName}.{attrName})");
                                }
                                else
                                {
                                    sb.Append($"t.{columnName}.{attrName}");
                                }

                                count++;
                            }

                            sb.Append($"||')') AS {columnName}");

                            columnName = sb.ToString();
                        }
                    }
                }
                #endregion

                columnNames.Add(columnName);
            }

            if (columnNames.Count == 0)
            {
                return new DataTable();
            }

            string strColumnNames = string.Join(",", columnNames);

            string pagedSql = this.GetSqlForPagination(quotedTableName, strColumnNames, orderColumns, whereClause, pageNumber, pageSize);

            DataTable dt = await this.GetDataTableAsync(connection, pagedSql);

            var dtColumns = dt.Columns.OfType<DataColumn>();

            if (dtColumns.Any(item => item.ColumnName == RowNumberColumnName))
            {
                dt.Columns.Remove(RowNumberColumnName);
            }

            foreach (var col in dtColumns)
            {
                TableColumn tc = columns.FirstOrDefault(item => item.Name == col.ColumnName);

                if (tc != null)
                {
                    col.ExtendedProperties.Add(nameof(DataTypeInfo), new DataTypeInfo() { DataType = tc.DataType });
                }
            }

            return dt;
        }

        public async Task<(long, DataTable)> GetPagedDataTableAsync(Table table, string orderColumns, int pageSize, long pageNumber, string whereClause = "", bool isForView = false)
        {
            using (DbConnection connection = this.CreateConnection())
            {
                long total = await this.GetTableRecordCountAsync(connection, table, whereClause);

                SchemaInfoFilter filter = new SchemaInfoFilter() { Schema = table.Schema };

                if (isForView)
                {
                    filter.ColumnType = ColumnType.ViewColumn;
                }

                filter.TableNames = new string[] { table.Name };

                var columns = await this.GetTableColumnsAsync(connection, filter);

                DataTable dt = await this.GetPagedDataTableAsync(this.CreateConnection(), table, columns, orderColumns, pageSize, pageNumber, whereClause);

                return (total, dt);
            }
        }

        public async Task<Dictionary<long, List<Dictionary<string, object>>>> GetPagedDataListAsync(DbConnection connection, Table table, List<TableColumn> columns, string primaryKeyColumns, long total, long batchCount, int pageSize, string whereClause = "")
        {
            var dictPagedData = new Dictionary<long, List<Dictionary<string, object>>>();

            long pageCount = PaginationHelper.GetPageCount(batchCount, pageSize);

            for (long pageNumber = 1; pageNumber <= pageCount; pageNumber++)
            {
                if (this.CancelRequested)
                {
                    break;
                }

                DataTable dataTable = await this.GetPagedDataTableAsync(connection, table, columns, primaryKeyColumns, pageSize, pageNumber, whereClause);

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
                        else if (value is PgGeom.Geometry pg && tableColumn.DataType == "geography")
                        {
                            pg.UserData = new PostgresGeometryCustomInfo() { IsGeography = true };
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
        public abstract bool IsLowDbVersion(string serverVersion);
        protected virtual void SubscribeInfoMessage(DbConnection dbConnection) { }
        protected virtual void SubscribeInfoMessage(DbCommand dbCommand) { }
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
            if (str != null && this.SupportQuotationChar && (this.DbObjectNameMode == DbObjectNameMode.WithQuotation || str.Contains(" ")))
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

        public bool IsLowDbVersion()
        {
            string serverVersion = this.ServerVersion;

            if (!string.IsNullOrEmpty(serverVersion))
            {
                return this.IsLowDbVersion(serverVersion);
            }

            return false;
        }

        protected string GetDbVersion()
        {
            if (!string.IsNullOrEmpty(this.ServerVersion))
            {
                return this.ServerVersion;
            }

            return this.GetDbVersion(this.CreateConnection());
        }

        protected string GetDbVersion(DbConnection connection)
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

            string serverVersion = connection.ServerVersion;

            if (needClose)
            {
                connection.Close();
            }

            return serverVersion;
        }

        public bool IsLowDbVersion(DbConnection connection)
        {
            string serverVersion = this.ServerVersion;

            if (string.IsNullOrEmpty(serverVersion))
            {
                try
                {
                    serverVersion = this.GetDbVersion(connection);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            return this.IsLowDbVersion(serverVersion);
        }

        public bool IsLowDbVersion(string version, string versionToCompare)
        {
            if (version != null && versionToCompare != null)
            {
                string[] versionItems = version.Split('.');
                string[] versionItemsToCompare = versionToCompare.Split('.');

                int length = Math.Max(versionItems.Length, versionItemsToCompare.Length);

                for (int i = 0; i < length; i++)
                {
                    string item = i < versionItems.Length ? versionItems[i] : "0";
                    string itemToCompare = i < versionItemsToCompare.Length ? versionItemsToCompare[i] : "0";

                    if (!string.IsNullOrEmpty(item) && !string.IsNullOrEmpty(itemToCompare)
                        && int.TryParse(item, out _) && int.TryParse(itemToCompare, out _))
                    {
                        int vItem = int.Parse(item);
                        int vItemToCompare = int.Parse(itemToCompare);

                        if (vItem > vItemToCompare)
                        {
                            return false;
                        }
                        else if (vItem < vItemToCompare)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected SqlBuilder CreateSqlBuilder()
        {
            return new SqlBuilder();
        }

        protected string GetFilterSchemaCondition(SchemaInfoFilter filter, string columnName)
        {
            if (filter != null && !string.IsNullOrEmpty(filter.Schema))
            {
                return $"AND {columnName}='{filter.Schema}'";
            }

            return string.Empty;
        }

        protected string GetFilterNamesCondition(SchemaInfoFilter filter, string[] names, string columnName)
        {
            if (filter != null && names != null && names.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(names);

                return $"AND {columnName} IN ({strNames})";
            }

            return string.Empty;
        }

        protected string GetExcludeBuiltinDbNamesCondition(string columnName, bool isFirstCondition = true)
        {
            if (!this.ShowBuiltinDatabase)
            {
                string strBuiltinDatabase = this.BuiltinDatabases.Count > 0 ? string.Join(",", this.BuiltinDatabases.Select(item => $"'{item}'")) : "";
                return string.IsNullOrEmpty(strBuiltinDatabase) ? "" : $"{(isFirstCondition ? "WHERE" : "AND")} {columnName} NOT IN({strBuiltinDatabase})";
            }

            return string.Empty;
        }

        public DataTypeInfo GetDataTypeInfo(string dataType)
        {
            if (!(this.DatabaseType == DatabaseType.Postgres && dataType == "\"char\""))
            {
                dataType = dataType.Trim(this.QuotationLeftChar, this.QuotationRightChar).Trim();
            }

            return DataTypeHelper.GetDataTypeInfo(dataType);
        }

        protected bool IsForViewColumnFilter(SchemaInfoFilter filter)
        {
            return filter != null && filter.ColumnType == ColumnType.ViewColumn;
        }
        #endregion

        #region Parse Column & DataType 
        public abstract string ParseColumn(Table table, TableColumn column);
        public abstract string ParseDataType(TableColumn column);
        public virtual string GetColumnDataLength(TableColumn column) { return string.Empty; }

        public virtual string GetColumnDefaultValue(TableColumn column)
        {
            bool isChar = DataTypeHelper.IsCharType(column.DataType);

            if (isChar && column.DefaultValue != null)
            {
                string trimedValue = column.DefaultValue.Trim('(', ')').Trim().ToUpper();

                if (!trimedValue.StartsWith('\'') && !trimedValue.StartsWith("N'"))
                {
                    return $"'{column.DefaultValue}'";
                }
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
                long scale = column.Scale.HasValue ? column.Scale.Value : 0;

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

        public void Feedback(FeedbackInfoType infoType, string message, bool skipError = false)
        {
            FeedbackInfo info = new FeedbackInfo() { Owner = this, InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(message), IgnoreError = skipError };

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

            this.Feedback(FeedbackInfoType.Error, message, skipError);
        }

        public void FeedbackInfo(OperationState state, DatabaseObject dbObject)
        {
            string message = $"{state.ToString()}{(state == OperationState.Begin ? " to" : "")} generate script for {StringHelper.GetFriendlyTypeName(dbObject.GetType().Name).ToLower()} \"{dbObject.Name}\".";
            this.Feedback(FeedbackInfoType.Info, message);
        }
        #endregion
    }
}
