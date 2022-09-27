using Dapper;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using MySqlConnector;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class OracleInterpreter : DbInterpreter
    {
        #region Field & Property
        private string dbSchema;
        public const int DEFAULT_PORT = 1521;
        public const string DEFAULT_SERVICE_NAME = "ORCL";
        public const string SEMICOLON_FUNC = "CHR(59)";
        public const string CONNECT_CHAR = "||";
        public override string UnicodeInsertChar => "";
        public override string CommandParameterChar => ":";
        public const char QuotedLeftChar = '"';
        public const char QuotedRightChar = '"';
        public override char QuotationLeftChar { get { return QuotedLeftChar; } }
        public override char QuotationRightChar { get { return QuotedRightChar; } }
        public override string CommentString => "--";
        public override DatabaseType DatabaseType => DatabaseType.Oracle;
        public override string DefaultDataType => "varchar2";
        public override string DefaultSchema => this.ConnectionInfo.UserId;
        public override IndexType IndexType => IndexType.Normal | IndexType.Unique | IndexType.Bitmap | IndexType.Reverse;
        public override bool SupportBulkCopy { get { return true; } }
        public override List<string> BuiltinDatabases => new List<string> { "SYSTEM", "USERS", "TEMP", "SYSAUX" };
        #endregion

        #region Constructor
        public OracleInterpreter(ConnectionInfo connectionInfo, DbInterpreterOption option) : base(connectionInfo, option)
        {
            this.dbSchema = connectionInfo.UserId;
            this.dbConnector = this.GetDbConnector();
        }
        #endregion

        #region Common Method  
        public override DbConnector GetDbConnector()
        {
            return new DbConnector(new OracleProvider(), new OracleConnectionBuilder(), this.ConnectionInfo);
        }

        public override bool IsLowDbVersion(string version)
        {
            if (version != null)
            {
                if (string.Compare(version.Substring(0, 2), "12") < 0)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Schema Information
        #region Database

        public string GetCurrentUserName()
        {
            string sql = "SELECT sys_context('USERENV', 'CURRENT_USER') FROM dual";
            return (this.GetScalar(this.CreateConnection(), sql))?.ToString();
        }

        public string GetDbSchema()
        {
            if (this.ConnectionInfo != null && this.ConnectionInfo.IntegratedSecurity && (string.IsNullOrEmpty(this.dbSchema) || this.dbSchema == "/"))
            {
                try
                {
                    return this.dbSchema = this.GetCurrentUserName();
                }
                catch (Exception ex)
                {
                    this.Feedback(FeedbackInfoType.Error, ExceptionHelper.GetExceptionDetails(ex));
                }
            }

            return this.dbSchema;
        }

        public override Task<List<Database>> GetDatabasesAsync()
        {
            string notShowBuiltinDatabaseCondition = "";

            if (!this.ShowBuiltinDatabase)
            {
                string strBuiltinDatabase = this.BuiltinDatabases.Count > 0 ? string.Join(",", this.BuiltinDatabases.Select(item => $"'{item}'")) : "";
                notShowBuiltinDatabaseCondition = string.IsNullOrEmpty(strBuiltinDatabase) ? "" : $"AND TABLESPACE_NAME NOT IN({strBuiltinDatabase}) AND CONTENTS <>'UNDO'";
            }

            string sql = $@"SELECT TABLESPACE_NAME AS ""Name"" FROM USER_TABLESPACES WHERE TABLESPACE_NAME IN(SELECT DEFAULT_TABLESPACE FROM USER_USERS WHERE UPPER(USERNAME)=UPPER('{this.GetDbSchema()}')) {notShowBuiltinDatabaseCondition}";

            return base.GetDbObjectsAsync<Database>(sql);
        }
        #endregion

        #region Database Schema
        public override async Task<List<DatabaseSchema>> GetDatabaseSchemasAsync()
        {
            string database = this.ConnectionInfo.UserId;

            List<DatabaseSchema> databaseSchemas = new List<DatabaseSchema>() { new DatabaseSchema() { Schema = database, Name = database } };

            return await Task.Run(() => { return databaseSchemas; });
        }
        #endregion

        #region User Defined Type       

        public override Task<List<UserDefinedType>> GetUserDefinedTypesAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<UserDefinedType>(this.GetSqlForUserDefinedTypes(filter));
        }

        public override Task<List<UserDefinedType>> GetUserDefinedTypesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<UserDefinedType>(dbConnection, this.GetSqlForUserDefinedTypes(filter));
        }

        private string GetSqlForUserDefinedTypes(SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            string sql = "";

            if (isSimpleMode)
            {
                sql = $@"SELECT T.OWNER AS ""Schema"",T.TYPE_NAME AS ""Name""
                        FROM ALL_TYPES T";
            }
            else
            {
                sql = $@"SELECT T.OWNER AS ""Schema"",T.TYPE_NAME AS ""Name"",TA.ATTR_NAME AS ""AttrName"", TA.ATTR_TYPE_NAME AS ""Type"",TA.LENGTH AS ""MaxLength"",TA.PRECISION AS ""Precision"",TA.SCALE AS ""Scale""
                        FROM ALL_TYPES T
                        JOIN ALL_TYPE_ATTRS TA ON T.OWNER = TA.OWNER AND T.TYPE_NAME = TA.TYPE_NAME
                      ";
            }

            sql += $" WHERE UPPER(T.OWNER)=UPPER('{this.GetDbSchema()}')";

            if (filter != null && filter.UserDefinedTypeNames != null && filter.UserDefinedTypeNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.UserDefinedTypeNames);
                sql += $" AND T.TYPE_NAME IN ({strNames})";
            }

            sql += $" ORDER BY T.TYPE_NAME{(isSimpleMode ? "" : ",TA.ATTR_NAME")}";

            return sql;
        }
        #endregion

        #region Sequence
        public override Task<List<Sequence>> GetSequencesAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Sequence>(this.GetSqlForSequences(filter));
        }
        public override Task<List<Sequence>> GetSequencesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Sequence>(dbConnection, this.GetSqlForSequences(filter));
        }

        private string GetSqlForSequences(SchemaInfoFilter filter = null)
        {
            string sql = $@"SELECT SEQUENCE_OWNER AS ""Schema"",SEQUENCE_NAME AS ""Name"",'number' AS ""DataType"",
                            MIN_VALUE AS ""MinValue"",MAX_VALUE AS ""MaxValue"",INCREMENT_BY AS ""Increment"",
                            CASE CYCLE_FLAG WHEN 'Y' THEN 1 ELSE 0 END AS ""Cycled"",CASE order_flag WHEN 'Y' THEN 1 ELSE 0 END AS ""Ordered"",
                            1 AS ""UseCache"",CACHE_SIZE AS ""CacheSize""
                            FROM all_sequences s
                            WHERE sequence_name='MySeq'
                            AND UPPER(SEQUENCE_OWNER)=UPPER('{this.ConnectionInfo.Database}')";

            if (filter != null && filter.SequenceNames != null && filter.SequenceNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.SequenceNames);
                sql += $" AND s.SEQUENCE_NAME in ({strNames})";
            }

            sql += " ORDER BY s.SEQUENCE_NAME";

            return sql;
        }
        #endregion

        #region Function  
        public override Task<List<Function>> GetFunctionsAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Function>(this.GetSqlForRoutines("FUNCTION", filter));
        }

        public override Task<List<Function>> GetFunctionsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Function>(dbConnection, this.GetSqlForRoutines("FUNCTION", filter));
        }

        private string GetSqlForRoutines(string type, SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            bool isFunction = type.ToUpper() == "FUNCTION";
            string sql = "";

            string ownerCondition = $" AND UPPER(P.OWNER) = UPPER('{this.GetDbSchema()}')";
            string nameCondition = "";
            string[] objectNames = isFunction ? filter?.FunctionNames : filter?.ProcedureNames;

            if (objectNames != null && objectNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(objectNames);
                nameCondition = $" AND P.OBJECT_NAME IN ({strNames})";
            }

            if (isSimpleMode)
            {
                sql = $@"SELECT P.OBJECT_NAME AS ""Name"", P.OWNER AS ""Schema""
                         FROM ALL_PROCEDURES P 
                         WHERE P.OBJECT_TYPE='{type}' {ownerCondition}
                         {nameCondition}";
            }
            else
            {
                sql = $@"SELECT S.NAME AS ""Name"", P.OWNER AS ""Schema"", 'CREATE OR REPLACE ' || LISTAGG(TEXT,'') WITHIN GROUP(ORDER BY LINE) ""Definition""
                        FROM ALL_PROCEDURES P
                        JOIN ALL_SOURCE S ON P.OWNER = S.OWNER AND P.OBJECT_NAME = S.NAME
                        WHERE P.OBJECT_TYPE = '{type}' {ownerCondition}
                        {nameCondition}
                        GROUP BY P.OWNER, S.NAME
                       ";
            }

            if (isSimpleMode)
            {
                sql += " ORDER BY P.OBJECT_NAME";
            }
            else
            {
                sql += " ORDER BY S.NAME";
            }

            return sql;
        }

        #endregion

        #region Table
        public override Task<List<Table>> GetTablesAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Table>(this.GetSqlForTables(filter));
        }

        public override Task<List<Table>> GetTablesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Table>(dbConnection, this.GetSqlForTables(filter));
        }

        private string GetSqlForTables(SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = "";
            string tablespaceCondition = string.IsNullOrEmpty(this.ConnectionInfo.Database) ? "" : $" AND UPPER(T.TABLESPACE_NAME)=UPPER('{this.ConnectionInfo.Database}')";

            if (isSimpleMode)
            {
                sql = $@"SELECT T.OWNER AS ""Schema"", T.TABLE_NAME AS ""Name""
                         FROM ALL_TABLES T 
                        ";
            }
            else
            {
                sql = $@"SELECT T.OWNER AS ""Schema"", T.TABLE_NAME AS ""Name"", C.COMMENTS AS ""Comment"",
                          1 AS ""IdentitySeed"", 1 AS ""IdentityIncrement""
                          FROM ALL_TABLES T
                          LEFT JOIN USER_TAB_COMMENTS C ON T.TABLE_NAME= C.TABLE_NAME
                     ";
            }

            sql += $" WHERE UPPER(OWNER)=UPPER('{this.GetDbSchema()}')" + tablespaceCondition;

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND T.TABLE_NAME IN ({strTableNames})";
            }

            sql += " ORDER BY T.TABLE_NAME";

            return sql;
        }
        #endregion

        #region Table Column

        public override Task<List<TableColumn>> GetTableColumnsAsync(SchemaInfoFilter filter = null)
        {
            DbConnection connection = this.CreateConnection();
            bool isLowDbVersion = this.IsLowDbVersion(connection);

            return base.GetDbObjectsAsync<TableColumn>(connection, this.GetSqlForTableColumns(filter));
        }

        public override Task<List<TableColumn>> GetTableColumnsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            bool isLowDbVersion = this.IsLowDbVersion(dbConnection);

            return base.GetDbObjectsAsync<TableColumn>(dbConnection, this.GetSqlForTableColumns(filter, isLowDbVersion));
        }

        private string GetSqlForTableColumns(SchemaInfoFilter filter = null, bool isLowDbVersion = false)
        {
            string userGeneratedCondition = isLowDbVersion ? "" : " AND C.USER_GENERATED='YES'";

            string sql = $@"SELECT OWNER AS ""Schema"", C.TABLE_NAME AS ""TableName"",C.COLUMN_NAME AS ""Name"",DATA_TYPE AS ""DataType"",C.DATA_TYPE_OWNER AS ""DataTypeSchema"",
                 CASE NULLABLE WHEN 'Y' THEN 1 ELSE 0 END AS ""IsNullable"", DATA_LENGTH AS ""MaxLength"",
                 DATA_PRECISION AS ""Precision"",DATA_SCALE AS ""Scale"", COLUMN_ID AS ""Order"", 0 AS ""IsIdentity"", CC.COMMENTS AS ""Comment"" ,
                 CASE WHEN C.VIRTUAL_COLUMN='YES' THEN NULL ELSE DATA_DEFAULT END AS ""DefaultValue"",
                 CASE WHEN C.VIRTUAL_COLUMN='YES' THEN DATA_DEFAULT ELSE NULL END AS ""ComputeExp""
                 FROM ALL_TAB_COLS C
                 LEFT JOIN USER_COL_COMMENTS CC ON C.TABLE_NAME=CC.TABLE_NAME AND C.COLUMN_NAME=CC.COLUMN_NAME
                 WHERE UPPER(OWNER)=UPPER('{this.GetDbSchema()}'){userGeneratedCondition}";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND C.TABLE_NAME IN ({strTableNames})";
            }

            return sql;
        }
        #endregion

        #region Table Primary Key
        public override Task<List<TablePrimaryKeyItem>> GetTablePrimaryKeyItemsAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TablePrimaryKeyItem>(this.GetSqlForTablePrimaryKeyItems(filter));
        }

        public override Task<List<TablePrimaryKeyItem>> GetTablePrimaryKeyItemsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TablePrimaryKeyItem>(dbConnection, this.GetSqlForTablePrimaryKeyItems(filter));
        }

        private string GetSqlForTablePrimaryKeyItems(SchemaInfoFilter filter = null)
        {
            string sql = $@"SELECT UC.OWNER AS ""Schema"", UC.TABLE_NAME AS ""TableName"",UC.CONSTRAINT_NAME AS ""Name"",UCC.COLUMN_NAME AS ""ColumnName"", UCC.POSITION AS ""Order"", 0 AS ""IsDesc""
                        FROM USER_CONSTRAINTS UC
                        JOIN USER_CONS_COLUMNS UCC ON UC.OWNER=UCC.OWNER AND UC.TABLE_NAME=UCC.TABLE_NAME AND UC.CONSTRAINT_NAME=UCC.CONSTRAINT_NAME  
                        WHERE UC.CONSTRAINT_TYPE='P' AND UPPER(UC.OWNER)=UPPER('{this.GetDbSchema()}')";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND UC.TABLE_NAME IN ({strTableNames})";
            }

            return sql;
        }
        #endregion

        #region Table Foreign Key
        public override Task<List<TableForeignKeyItem>> GetTableForeignKeyItemsAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableForeignKeyItem>(this.GetSqlForTableForeignKeyItems(filter));
        }

        public override Task<List<TableForeignKeyItem>> GetTableForeignKeyItemsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableForeignKeyItem>(dbConnection, this.GetSqlForTableForeignKeyItems(filter));
        }

        private string GetSqlForTableForeignKeyItems(SchemaInfoFilter filter = null)
        {
            string sql = $@"SELECT UC.OWNER AS ""Schema"", UC.TABLE_NAME AS ""TableName"", UC.CONSTRAINT_NAME AS ""Name"", UCC.column_name AS ""ColumnName"",
                        RUCC.TABLE_NAME AS ""ReferencedTableName"",RUCC.COLUMN_NAME AS ""ReferencedColumnName"",
                        0 AS ""UpdateCascade"", CASE UC.DELETE_RULE WHEN 'CASCADE' THEN 1 ELSE 0 END AS ""DeleteCascade"" 
                        FROM USER_CONSTRAINTS UC                       
                        JOIN USER_CONS_COLUMNS UCC ON UC.OWNER=UCC.OWNER AND UC.TABLE_NAME=UCC.TABLE_NAME AND UC.CONSTRAINT_NAME=UCC.CONSTRAINT_NAME                       
                        JOIN USER_CONS_COLUMNS RUCC ON UC.OWNER=RUCC.OWNER AND UC.R_CONSTRAINT_NAME=RUCC.CONSTRAINT_NAME AND UCC.POSITION=RUCC.POSITION
                        WHERE UC.CONSTRAINT_TYPE='R' AND UPPER(UC.OWNER)=UPPER('{this.GetDbSchema()}')";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND UC.TABLE_NAME IN ({strTableNames})";
            }

            return sql;
        }
        #endregion

        #region Table Index
        public override Task<List<TableIndexItem>> GetTableIndexItemsAsync(SchemaInfoFilter filter = null, bool includePrimaryKey = false)
        {
            return base.GetDbObjectsAsync<TableIndexItem>(this.GetSqlForTableIndexItems(filter, includePrimaryKey));
        }

        public override Task<List<TableIndexItem>> GetTableIndexItemsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null, bool includePrimaryKey = false)
        {
            return base.GetDbObjectsAsync<TableIndexItem>(dbConnection, this.GetSqlForTableIndexItems(filter, includePrimaryKey));
        }

        private string GetSqlForTableIndexItems(SchemaInfoFilter filter = null, bool includePrimaryKey = false)
        {

            string sql = $@"SELECT UI.TABLE_OWNER AS ""Schema"", UI.TABLE_NAME AS ""TableName"", UI.INDEX_NAME AS ""Name"", UIC.COLUMN_NAME AS ""ColumnName"", UIC.COLUMN_POSITION AS ""Order"",
                CASE UIC.DESCEND WHEN 'ASC' THEN 0 ELSE 1 END AS ""IsDesc"", CASE UI.UNIQUENESS WHEN 'UNIQUE' THEN 1 ELSE 0 END AS ""IsUnique"",
                UI.INDEX_TYPE AS ""Type"", CASE WHEN UC.CONSTRAINT_NAME IS NOT NULL THEN 1 ELSE 0 END AS ""IsPrimary"", CASE WHEN UC.CONSTRAINT_NAME IS NOT NULL THEN 1 ELSE 0 END AS ""Clustered""
                FROM USER_INDEXES UI
                JOIN USER_IND_COLUMNS UIC ON UI.INDEX_NAME = UIC.INDEX_NAME AND UI.TABLE_NAME = UIC.TABLE_NAME
                LEFT JOIN USER_CONSTRAINTS UC ON UI.TABLE_NAME = UC.TABLE_NAME AND UI.TABLE_OWNER = UC.OWNER AND UI.INDEX_NAME = UC.CONSTRAINT_NAME AND UC.CONSTRAINT_TYPE = 'P'
                WHERE UPPER(UI.TABLE_OWNER)=UPPER('{this.GetDbSchema()}'){(includePrimaryKey ? "" : " AND UC.CONSTRAINT_NAME IS NULL")}";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND UI.TABLE_NAME IN ({strTableNames})";
            }

            return sql;
        }
        #endregion

        #region Table Trigger      
        public override Task<List<TableTrigger>> GetTableTriggersAsync(SchemaInfoFilter filter = null)
        {
            if (this.IsObjectFectchSimpleMode())
            {
                return base.GetDbObjectsAsync<TableTrigger>(this.GetSqlForTableTriggers(filter));
            }
            else
            {
                return this.GetTriggerDefinition(base.GetDbObjectsAsync<TableTrigger>(this.GetSqlForTableTriggers(filter)));
            }
        }

        public override Task<List<TableTrigger>> GetTableTriggersAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            if (this.IsObjectFectchSimpleMode())
            {
                return base.GetDbObjectsAsync<TableTrigger>(dbConnection, this.GetSqlForTableTriggers(filter));
            }
            else
            {
                return this.GetTriggerDefinition(base.GetDbObjectsAsync<TableTrigger>(dbConnection, this.GetSqlForTableTriggers(filter)));
            }
        }

        private Task<List<TableTrigger>> GetTriggerDefinition(Task<List<TableTrigger>> tableTriggers)
        {
            foreach (TableTrigger trigger in tableTriggers.Result)
            {
                trigger.Definition = trigger.CreateClause + trigger.Definition;
            }

            return tableTriggers;
        }

        private string GetSqlForTableTriggers(SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = $@"SELECT TRIGGER_NAME AS ""Name"",TABLE_OWNER AS ""Schema"", TABLE_NAME AS ""TableName"", 
                         {(isSimpleMode ? "''" : "('CREATE OR REPLACE TRIGGER ' || DESCRIPTION)")} AS ""CreateClause"",
                         {(isSimpleMode ? "''" : "TRIGGER_BODY")} AS ""Definition""
                        FROM USER_TRIGGERS
                        WHERE UPPER(TABLE_OWNER) = UPPER('{this.GetDbSchema()}')
                        ";

            if (filter != null)
            {
                if (filter.TableNames != null && filter.TableNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                    sql += $" AND TABLE_NAME IN ({strNames})";
                }

                if (filter.TableTriggerNames != null && filter.TableTriggerNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter.TableTriggerNames);
                    sql += $" AND TRIGGER_NAME IN ({strNames})";
                }
            }

            sql += " ORDER BY TRIGGER_NAME";

            return sql;
        }
        #endregion

        #region Table Constraint
        public override Task<List<TableConstraint>> GetTableConstraintsAsync(SchemaInfoFilter filter = null)
        {
            DbConnection connection = this.CreateConnection();
            bool isLowDbVersion = this.IsLowDbVersion(connection);

            return base.GetDbObjectsAsync<TableConstraint>(connection, this.GetSqlForTableConstraints(filter, isLowDbVersion));
        }

        public override Task<List<TableConstraint>> GetTableConstraintsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            bool isLowDbVersion = this.IsLowDbVersion(dbConnection);

            return base.GetDbObjectsAsync<TableConstraint>(dbConnection, this.GetSqlForTableConstraints(filter, isLowDbVersion));
        }

        private string GetSqlForTableConstraints(SchemaInfoFilter filter = null, bool isLowDbVersion = false)
        {
            string definitionField = isLowDbVersion ? "SEARCH_CONDITION" : "SEARCH_CONDITION_VC";

            string sql = $@"SELECT OWNER AS ""Schema"", CONSTRAINT_NAME AS ""Name"", TABLE_NAME AS ""TableName"", {definitionField} AS ""Definition""
                         FROM ALL_CONSTRAINTS C
                         WHERE UPPER(OWNER) = UPPER('{this.GetDbSchema()}') 
                         AND CONSTRAINT_TYPE = 'C' AND GENERATED = 'USER NAME'
                         ";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND TABLE_NAME IN ({strNames})";
            }

            sql += " ORDER BY CONSTRAINT_NAME";

            return sql;
        }
        #endregion

        #region View  
        public override Task<List<View>> GetViewsAsync(SchemaInfoFilter filter = null)
        {
            var dbConnection = this.CreateConnection();
            bool isLowDbVersion = this.IsLowDbVersion(dbConnection);

            return base.GetDbObjectsAsync<View>(dbConnection, this.GetSqlForViews(filter, isLowDbVersion));
        }

        public override Task<List<View>> GetViewsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            bool isLowDbVersion = this.IsLowDbVersion(dbConnection);

            return base.GetDbObjectsAsync<View>(dbConnection, this.GetSqlForViews(filter, isLowDbVersion));
        }

        private string GetSqlForViews(SchemaInfoFilter filter = null, bool isLowDbVersion = false)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            string definitionField = isLowDbVersion ? $"DBMS_METADATA.GET_DDL('VIEW',V.VIEW_NAME, '{this.ConnectionInfo.UserId}')" : "'CREATE OR REPLACE VIEW '||V.VIEW_NAME||' AS ' || CHR(13) || TEXT_VC";

            string sql = $@"SELECT V.OWNER AS ""Schema"", V.VIEW_NAME AS ""Name"", {(isSimpleMode ? "''" : definitionField)} AS ""Definition"" 
                        FROM ALL_VIEWS V
                        WHERE UPPER(OWNER) = UPPER('{this.GetDbSchema()}')";

            if (filter != null && filter.ViewNames != null && filter.ViewNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.ViewNames);
                sql += $" AND V.VIEW_NAME IN ({strNames})";
            }

            sql += " ORDER BY VIEW_NAME";

            return sql;
        }
        #endregion

        #region Procedure     

        public override Task<List<Procedure>> GetProceduresAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Procedure>(this.GetSqlForRoutines("PROCEDURE", filter));
        }

        public override Task<List<Procedure>> GetProceduresAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Procedure>(dbConnection, this.GetSqlForRoutines("PROCEDURE", filter));
        }
        #endregion
        #endregion       

        #region BulkCopy
        public override async Task BulkCopyAsync(DbConnection connection, DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            if (!(connection is OracleConnection conn))
            {
                return;
            }

            using (var bulkCopy = new OracleBulkCopy(conn, bulkCopyInfo.Transaction as OracleTransaction))
            {
                bulkCopy.BatchSize = dataTable.Rows.Count;
                bulkCopy.DestinationTableName = this.GetQuotedString(bulkCopyInfo.DestinationTableName);
                bulkCopy.BulkCopyTimeout = bulkCopyInfo.Timeout.HasValue ? bulkCopyInfo.Timeout.Value : SettingManager.Setting.CommandTimeout; ;
                bulkCopy.ColumnNameNeedQuoted = this.DbObjectNameMode == DbObjectNameMode.WithQuotation;
                bulkCopy.DetectDateTimeTypeByValues = bulkCopyInfo.DetectDateTimeTypeByValues;

                await bulkCopy.WriteToServerAsync(this.ConvertDataTable(dataTable, bulkCopyInfo));
            }
        }

        private DataTable ConvertDataTable(DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            var columns = dataTable.Columns.Cast<DataColumn>();

            if (!columns.Any(item => item.DataType == typeof(MySqlDateTime)
               || item.DataType.Name == "SqlGeography"
               || item.DataType.Name == nameof(System.Collections.BitArray)))
            {
                return dataTable;
            }

            Dictionary<int, DataTableColumnChangeInfo> changedColumns = new Dictionary<int, DataTableColumnChangeInfo>();
            Dictionary<(int RowIndex, int ColumnIndex), dynamic> changedValues = new Dictionary<(int RowIndex, int ColumnIndex), dynamic>();

            int rowIndex = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    object value = row[i];

                    if (value != null)
                    {
                        Type type = value.GetType();

                        if (type != typeof(DBNull))
                        {
                            Type newColumnType = null;
                            TableColumn tableColumn = bulkCopyInfo.Columns.FirstOrDefault(item => item.Name == dataTable.Columns[i].ColumnName);
                            string dataType = tableColumn.DataType.ToLower();

                            if (type == typeof(MySqlDateTime))
                            {
                                MySqlDateTime mySqlDateTime = (MySqlDateTime)value;

                                if (dataType.Contains("date") || dataType.Contains("timestamp"))
                                {
                                    DateTime dateTime = mySqlDateTime.GetDateTime();

                                    newColumnType = typeof(DateTime);

                                    changedValues.Add((rowIndex, i), dateTime);
                                }
                            }
                            else if (type == typeof(System.Collections.BitArray))
                            {
                                newColumnType = typeof(byte[]);

                                var bitArray = value as BitArray;
                                byte[] bytes = new byte[bitArray.Length];
                                bitArray.CopyTo(bytes, 0);

                                changedValues.Add((rowIndex, i), bytes);
                            }

                            if (newColumnType != null && !changedColumns.ContainsKey(i))
                            {
                                changedColumns.Add(i, new DataTableColumnChangeInfo() { Type = newColumnType });
                            }
                        }
                    }
                }

                rowIndex++;
            }

            if (changedColumns.Count == 0)
            {
                return dataTable;
            }

            DataTable dtChanged = DataTableHelper.GetChangedDataTable(dataTable, changedColumns, changedValues);

            return dtChanged;
        }
        #endregion

        #region Sql Query Clause
        public override Task<long> GetTableRecordCountAsync(DbConnection connection, Table table, string whereClause = "")
        {
            string sql = $@"SELECT COUNT(1) FROM {this.GetDbSchema()}.{this.GetQuotedString(table.Name)}";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += whereClause;
            }

            return base.GetTableRecordCountAsync(connection, sql);
        }
        protected override string GetSqlForPagination(string tableName, string columnNames, string orderColumns, string whereClause, long pageNumber, int pageSize)
        {
            var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

            string orderClause = string.IsNullOrEmpty(orderColumns) ? this.GetDefaultOrder() : orderColumns;

            string pagedSql = $@"with PagedRecords as
								(
									SELECT {columnNames}, ROW_NUMBER() OVER (ORDER BY {orderClause}) AS ""{RowNumberColumnName}""
									FROM {tableName}
                                    {whereClause}
								)
								SELECT *
								FROM PagedRecords
								WHERE ""{RowNumberColumnName}"" BETWEEN {startEndRowNumber.StartRowNumber} AND {startEndRowNumber.EndRowNumber}";

            return pagedSql;
        }

        public override string GetDefaultOrder()
        {
            return "(SELECT 0 FROM DUAL)";
        }

        public override string GetLimitStatement(int limitStart, int limitCount)
        {
            return $"OFFSET {limitStart} ROWS FETCH NEXT {limitCount} ROWS ONLY";
        }
        #endregion

        #region Parse Column & DataType
        public override string ParseColumn(Table table, TableColumn column)
        {
            if (column.IsComputed)
            {
                string computeExpression = this.GetColumnComputeExpression(column);

                return $"{this.GetQuotedString(column.Name)} AS ({computeExpression})";
            }
            else
            {
                string dataType = this.ParseDataType(column);
                string requiredClause = (column.IsRequired ? "NOT NULL" : "NULL");
                string defaultValueClause = "";

                if (column.DefaultValue != null && !ValueHelper.IsSequenceNextVal(column.DefaultValue))
                {
                    defaultValueClause = this.Option.TableScriptsGenerateOption.GenerateDefaultValue && !string.IsNullOrEmpty(column.DefaultValue) ? (" DEFAULT " + this.GetColumnDefaultValue(column)) : "";
                }

                string scriptComment = string.IsNullOrEmpty(column.ScriptComment) ? "" : $"/*{column.ScriptComment}*/";

                string content = $"{this.GetQuotedString(column.Name)} {dataType}{defaultValueClause} {requiredClause}{scriptComment}";

                return content;
            }
        }

        public override string ParseDataType(TableColumn column)
        {
            string dataType = column.DataType;

            if (dataType.IndexOf("(") < 0)
            {
                DataTypeSpecification dataTypeSpec = this.GetDataTypeSpecification(dataType.ToLower());

                if (dataTypeSpec != null && !string.IsNullOrEmpty(dataTypeSpec.Format))
                {
                    string format = dataTypeSpec.Format;
                    string args = dataTypeSpec.Args;

                    if (!string.IsNullOrEmpty(args))
                    {
                        string[] argItems = args.Split(',');

                        foreach (string argItem in argItems)
                        {
                            if (argItem == "dayScale")
                            {
                                format = format.Replace("$dayScale$", (column.Precision.HasValue ? column.Precision.Value : 0).ToString());
                            }
                            else if (argItem == "precision")
                            {
                                format = format.Replace("$precision$", (column.Precision.HasValue ? column.Precision.Value : 0).ToString());
                            }
                            else if (argItem == "scale")
                            {
                                format = format.Replace("$scale$", (column.Scale.HasValue ? column.Scale.Value : 0).ToString());
                            }
                        }

                        dataType = format;
                    }
                    else
                    {
                        if (dataType.ToLower() == "st_geometry")
                        {
                            string geometryMode = SettingManager.Setting.OracleGeometryMode;

                            if (string.IsNullOrEmpty(geometryMode) || geometryMode == "MDSYS")
                            {
                                dataType = $@"""PUBLIC"".{dataType}";
                            }
                            else
                            {
                                dataType = $"{geometryMode}.{dataType}";
                            }
                        }
                    }
                }
                else
                {
                    string dataLength = this.GetColumnDataLength(column);

                    if (!string.IsNullOrEmpty(dataLength))
                    {
                        dataType += $"({dataLength})";
                    }
                }
            }

            return dataType.Trim();
        }

        public override string GetColumnDataLength(TableColumn column)
        {
            string dataType = column.DataType;
            string dataLength = string.Empty;

            DataTypeInfo dataTypeInfo = DataTypeHelper.GetDataTypeInfo(this, dataType);
            bool isChar = DataTypeHelper.IsCharType(column.DataType.ToLower());

            DataTypeSpecification dataTypeSpec = this.GetDataTypeSpecification(dataTypeInfo.DataType);

            if (dataTypeSpec != null)
            {
                if (!string.IsNullOrEmpty(dataTypeSpec.Args))
                {
                    if (string.IsNullOrEmpty(dataTypeInfo.Args))
                    {
                        if (isChar)
                        {
                            long? length = column.MaxLength;

                            if (length > 0 && DataTypeHelper.StartWithN(dataType))
                            {
                                length = length / 2;
                            }

                            dataLength = length.ToString();
                        }
                        else if (!this.IsNoLengthDataType(dataType))
                        {
                            if (!((column.Precision == null || column.Precision == 0) && (column.Scale == null || column.Scale == 0)))
                            {
                                long? precision = (column.Precision != null && column.Precision.HasValue) ? column.Precision.Value : column.MaxLength;

                                if (dataType == "raw")
                                {
                                    dataLength = precision.ToString();
                                }
                                else if (string.IsNullOrEmpty(dataTypeSpec.Format))
                                {
                                    string precisionScale = this.GetDataTypePrecisionScale(column, dataTypeInfo.DataType);

                                    dataLength = precisionScale;
                                }
                            }
                            else if (column.MaxLength > 0)
                            {
                                dataLength = column.MaxLength.ToString();
                            }
                        }
                    }
                    else
                    {
                        dataLength = dataTypeInfo.Args;
                    }
                }
            }

            return dataLength;
        }
        #endregion      
    }
}
