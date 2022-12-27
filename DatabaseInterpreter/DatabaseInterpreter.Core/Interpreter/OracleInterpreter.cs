using DatabaseInterpreter.Geometry;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Microsoft.SqlServer.Types;
using MySqlConnector;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using PgGeom = NetTopologySuite.Geometries;

namespace DatabaseInterpreter.Core
{
    public class OracleInterpreter : DbInterpreter
    {
        #region Field & Property
        private string dbSchema;
        public const int DEFAULT_PORT = 1521;
        public const string DEFAULT_SERVICE_NAME = "ORCL";
        public const string SEMICOLON_FUNC = "CHR(59)";
        public override string STR_CONCAT_CHARS => "||";
        public override string UnicodeLeadingFlag => "";
        public override string CommandParameterChar => ":";
        public const char QuotedLeftChar = '"';
        public const char QuotedRightChar = '"';
        public override bool SupportQuotationChar => true;
        public override char QuotationLeftChar { get { return QuotedLeftChar; } }
        public override char QuotationRightChar { get { return QuotedRightChar; } }
        public override string CommentString => "--";
        public override DatabaseType DatabaseType => DatabaseType.Oracle;
        public override string DefaultDataType => "varchar2";
        public override string DefaultSchema => this.ConnectionInfo.UserId?.ToUpper();
        public override IndexType IndexType => IndexType.Primary | IndexType.Normal | IndexType.Unique | IndexType.Bitmap | IndexType.Reverse;
        public override DatabaseObjectType SupportDbObjectType => DatabaseObjectType.Table | DatabaseObjectType.View | DatabaseObjectType.Function
                                                                | DatabaseObjectType.Procedure | DatabaseObjectType.Type | DatabaseObjectType.Sequence;
        public override bool SupportBulkCopy => true;
        public override bool SupportNchar => true;
        public override List<string> BuiltinDatabases => new List<string> { "SYSTEM", "USERS", "TEMP", "SYSAUX" };
        public static readonly string[] GeometryTypeSchemas = { "PUBLIC", "SDE" };
        #endregion

        #region Constructor
        public OracleInterpreter(ConnectionInfo connectionInfo, DbInterpreterOption option) : base(connectionInfo, option)
        {
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
            return this.IsLowDbVersion(version, "12");
        }

        private bool IsBuiltinDatabase()
        {
            return this.BuiltinDatabases.Any(item => item.ToUpper() == this.ConnectionInfo.Database?.ToUpper());
        }

        private string GetSchemaBySchemaFilter(SchemaInfoFilter filter)
        {
            if (filter != null && !string.IsNullOrEmpty(filter.Schema))
            {
                return filter.Schema;
            }

            return this.GetDbSchema();
        }
        #endregion

        #region Schema Information
        #region Database

        public string GetCurrentUserName()
        {
            string sql = "SELECT sys_context('USERENV', 'CURRENT_USER') FROM DUAL";
            return (this.GetScalar(this.CreateConnection(), sql))?.ToString();
        }

        public string GetDbSchema()
        {
            if (string.IsNullOrEmpty(this.dbSchema))
            {
                bool isValidConnection = this.ConnectionInfo != null
                    && (this.ConnectionInfo.IntegratedSecurity
                    || (!string.IsNullOrEmpty(this.ConnectionInfo.UserId) && !string.IsNullOrEmpty(this.ConnectionInfo.Password))
                   );

                if (isValidConnection)
                {
                    string sql = this.GetSqlForTablespaces();

                    this.dbSchema = (this.GetScalar(this.CreateConnection(), sql))?.ToString();
                }
            }

            return this.dbSchema;
        }

        public override Task<List<Database>> GetDatabasesAsync()
        {
            string sql = this.GetSqlForTablespaces();

            return base.GetDbObjectsAsync<Database>(sql);
        }

        private string GetSqlForTablespaces()
        {
            string notShowBuiltinDatabaseCondition = this.GetExcludeBuiltinDbNamesCondition("TABLESPACE_NAME", false);

            if (!string.IsNullOrEmpty(notShowBuiltinDatabaseCondition))
            {
                notShowBuiltinDatabaseCondition += " AND CONTENTS <>'UNDO'";
            }

            string sql = $@"SELECT TABLESPACE_NAME AS ""Name"" FROM USER_TABLESPACES WHERE TABLESPACE_NAME IN(SELECT DEFAULT_TABLESPACE FROM USER_USERS WHERE UPPER(USERNAME)=UPPER('{this.GetCurrentUserName()}')) {notShowBuiltinDatabaseCondition}";

            return sql;
        }
        #endregion

        #region Database Schema
        public override async Task<List<DatabaseSchema>> GetDatabaseSchemasAsync()
        {
            var tablespaces = await this.GetDatabasesAsync();

            string tablesapce = tablespaces.FirstOrDefault().Name;

            List<DatabaseSchema> databaseSchemas = new List<DatabaseSchema>() { new DatabaseSchema() { Schema = tablesapce, Name = tablesapce } };

            return await Task.Run(() => { return databaseSchemas; });
        }

        public override async Task<List<DatabaseSchema>> GetDatabaseSchemasAsync(DbConnection dbConnection)
        {
            return await this.GetDatabaseSchemasAsync();
        }
        #endregion

        #region User Defined Type       

        public override Task<List<UserDefinedTypeAttribute>> GetUserDefinedTypeAttributesAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<UserDefinedTypeAttribute>(this.GetSqlForUserDefinedTypes(filter));
        }

        public override Task<List<UserDefinedTypeAttribute>> GetUserDefinedTypeAttributesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<UserDefinedTypeAttribute>(dbConnection, this.GetSqlForUserDefinedTypes(filter));
        }

        private string GetSqlForUserDefinedTypes(SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            var sb = this.CreateSqlBuilder();

            if (isSimpleMode)
            {
                sb.Append($@"SELECT T.OWNER AS ""Schema"",T.TYPE_NAME AS ""TypeName""
                        FROM ALL_TYPES T");
            }
            else
            {
                sb.Append($@"SELECT T.OWNER AS ""Schema"",T.TYPE_NAME AS ""TypeName"",TA.ATTR_NAME AS ""Name"", TA.ATTR_TYPE_NAME AS ""DataType"",TA.LENGTH AS ""MaxLength"",TA.PRECISION AS ""Precision"",TA.SCALE AS ""Scale""
                        FROM ALL_TYPES T
                        JOIN ALL_TYPE_ATTRS TA ON T.OWNER = TA.OWNER AND T.TYPE_NAME = TA.TYPE_NAME");
            }

            sb.Append($"WHERE UPPER(T.OWNER)=UPPER('{this.GetSchemaBySchemaFilter(filter)}')");

            sb.Append(this.GetFilterNamesCondition(filter, filter?.UserDefinedTypeNames, "T.TYPE_NAME"));

            sb.Append($"ORDER BY T.TYPE_NAME{(isSimpleMode ? "" : ",TA.ATTR_NAME")}");

            return sb.Content;
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
            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT SEQUENCE_OWNER AS ""Schema"",SEQUENCE_NAME AS ""Name"",'number' AS ""DataType"",
                            MIN_VALUE AS ""MinValue"",MAX_VALUE AS ""MaxValue"",INCREMENT_BY AS ""Increment"",
                            CASE CYCLE_FLAG WHEN 'Y' THEN 1 ELSE 0 END AS ""Cycled"",CASE order_flag WHEN 'Y' THEN 1 ELSE 0 END AS ""Ordered"",
                            1 AS ""UseCache"",CACHE_SIZE AS ""CacheSize""
                            FROM all_sequences s
                            WHERE SEQUENCE_NAME NOT LIKE 'ISEQ$$_%' AND UPPER(SEQUENCE_OWNER)=UPPER('{this.ConnectionInfo.Database}')");

            sb.Append(this.GetFilterNamesCondition(filter, filter?.SequenceNames, "s.SEQUENCE_NAME"));

            sb.Append("ORDER BY s.SEQUENCE_NAME");

            return sb.Content;
        }
        #endregion

        #region Function  
        public override Task<List<Function>> GetFunctionsAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Function>(this.GetSqlForRoutines(DatabaseObjectType.Function, filter));
        }

        public override Task<List<Function>> GetFunctionsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Function>(dbConnection, this.GetSqlForRoutines(DatabaseObjectType.Function, filter));
        }

        private string GetSqlForRoutines(DatabaseObjectType databaseObjectType, SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            bool isFunction = databaseObjectType == DatabaseObjectType.Function;
            string type = isFunction ? "FUNCTION" : "PROCEDURE";

            string ownerCondition = $" AND UPPER(P.OWNER) = UPPER('{this.GetSchemaBySchemaFilter(filter)}')";
            string[] objectNames = isFunction ? filter?.FunctionNames : filter?.ProcedureNames;

            var sb = this.CreateSqlBuilder();

            Action appendNamesCondition = () =>
            {
                sb.Append(this.GetFilterNamesCondition(filter, objectNames, "P.OBJECT_NAME"));
            };

            if (isSimpleMode)
            {
                sb.Append($@"SELECT P.OBJECT_NAME AS ""Name"", P.OWNER AS ""Schema"", A.DATA_TYPE AS ""DataType""
                         FROM ALL_PROCEDURES P
                         LEFT JOIN SYS.ALL_ARGUMENTS A ON P.OBJECT_ID = A.OBJECT_ID AND A.ARGUMENT_NAME IS NULL
                         WHERE P.OBJECT_TYPE='{type}' {ownerCondition}");

                appendNamesCondition();
            }
            else
            {
                sb.Append($@"SELECT S.NAME AS ""Name"", P.OWNER AS ""Schema"",A.DATA_TYPE AS ""DataType"",
                        'CREATE OR REPLACE ' || XMLAGG(XMLPARSE(CONTENT TEXT WELLFORMED) ORDER BY LINE).GETCLOBVAL() ""Definition""
                        FROM ALL_PROCEDURES P
                        JOIN ALL_SOURCE S ON P.OWNER = S.OWNER AND P.OBJECT_NAME = S.NAME
                        LEFT JOIN SYS.ALL_ARGUMENTS A ON P.OBJECT_ID = A.OBJECT_ID AND A.ARGUMENT_NAME IS NULL
                        WHERE P.OBJECT_TYPE = '{type}' {ownerCondition}");

                appendNamesCondition();

                sb.Append("GROUP BY P.OWNER, S.NAME, A.DATA_TYPE");
            }

            if (isSimpleMode)
            {
                sb.Append("ORDER BY P.OBJECT_NAME");
            }
            else
            {
                sb.Append("ORDER BY S.NAME");
            }

            return sb.Content;
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

            string tablespaceCondition = string.IsNullOrEmpty(this.ConnectionInfo.Database) ? "" : $" AND UPPER(T.TABLESPACE_NAME)=UPPER('{this.ConnectionInfo.Database}')";

            var sb = this.CreateSqlBuilder();

            if (isSimpleMode)
            {
                sb.Append($@"SELECT T.OWNER AS ""Schema"", T.TABLE_NAME AS ""Name""
                         FROM ALL_TABLES T");
            }
            else
            {
                sb.Append($@"SELECT T.OWNER AS ""Schema"", T.TABLE_NAME AS ""Name"", C.COMMENTS AS ""Comment"",
                          1 AS ""IdentitySeed"", 1 AS ""IdentityIncrement""
                          FROM ALL_TABLES T
                          LEFT JOIN USER_TAB_COMMENTS C ON T.TABLE_NAME= C.TABLE_NAME");
            }

            sb.Append($" WHERE UPPER(OWNER)=UPPER('{this.GetSchemaBySchemaFilter(filter)}')" + tablespaceCondition);

            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "T.TABLE_NAME"));

            sb.Append("ORDER BY T.TABLE_NAME");

            return sb.Content;
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
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            bool isForView = this.IsForViewColumnFilter(filter);

            string userGeneratedCondition = isLowDbVersion ? "" : " AND C.USER_GENERATED='YES'";
            string identityColumn = isLowDbVersion ? "0" : "CASE C.IDENTITY_COLUMN  WHEN 'YES' THEN 1 ELSE 0 END";

            string commentColumn = isSimpleMode ? "" : @", CC.COMMENTS AS ""Comment""";
            string commentClause = isSimpleMode ? "" : "LEFT JOIN USER_COL_COMMENTS CC ON C.TABLE_NAME=CC.TABLE_NAME AND C.COLUMN_NAME=CC.COLUMN_NAME";

            string joinTable = !isForView ? "JOIN ALL_TABLES T ON C.OWNER=T.OWNER AND C.TABLE_NAME=T.TABLE_NAME" : " JOIN ALL_VIEWS V ON C.OWNER=V.OWNER AND C.TABLE_NAME=V.VIEW_NAME";

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT C.OWNER AS ""Schema"", C.TABLE_NAME AS ""TableName"",C.COLUMN_NAME AS ""Name"",DATA_TYPE AS ""DataType"",C.DATA_TYPE_OWNER AS ""DataTypeSchema"",
                 CASE NULLABLE WHEN 'Y' THEN 1 ELSE 0 END AS ""IsNullable"", DATA_LENGTH AS ""MaxLength"",
                 DATA_PRECISION AS ""Precision"",DATA_SCALE AS ""Scale"", COLUMN_ID AS ""Order""{commentColumn},
                 {identityColumn} AS ""IsIdentity"",
                 CASE WHEN C.VIRTUAL_COLUMN='YES' THEN NULL ELSE DATA_DEFAULT END AS ""DefaultValue"",
                 CASE WHEN C.VIRTUAL_COLUMN='YES' THEN DATA_DEFAULT ELSE NULL END AS ""ComputeExp"",
                 CASE WHEN TP.TYPE_NAME IS NULL THEN 0 ELSE 1 END AS ""IsUserDefined""
                 FROM ALL_TAB_COLS C
                 {joinTable}
                 LEFT JOIN ALL_TYPES TP ON C.OWNER=TP.OWNER AND C.DATA_TYPE=TP.TYPE_NAME
                 {commentClause}
                 WHERE UPPER(C.OWNER)=UPPER('{this.GetSchemaBySchemaFilter(filter)}') AND C.HIDDEN_COLUMN='NO'{userGeneratedCondition}");

            if (this.IsBuiltinDatabase())
            {
                if (isSimpleMode)
                {
                    sb.Append("AND C.TABLE_NAME NOT LIKE '%$%' AND C.COLUMN_NAME NOT LIKE '%#%'");
                }
            }

            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "C.TABLE_NAME"));

            sb.Append("ORDER BY C.TABLE_NAME,C.COLUMN_ID");

            return sb.Content;
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
            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT UC.OWNER AS ""Schema"", UC.TABLE_NAME AS ""TableName"",UC.CONSTRAINT_NAME AS ""Name"",UCC.COLUMN_NAME AS ""ColumnName"", UCC.POSITION AS ""Order"", 0 AS ""IsDesc""
                        FROM USER_CONSTRAINTS UC
                        JOIN USER_CONS_COLUMNS UCC ON UC.OWNER=UCC.OWNER AND UC.TABLE_NAME=UCC.TABLE_NAME AND UC.CONSTRAINT_NAME=UCC.CONSTRAINT_NAME  
                        WHERE UC.CONSTRAINT_TYPE='P' AND UPPER(UC.OWNER)=UPPER('{this.GetSchemaBySchemaFilter(filter)}')");

            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "UC.TABLE_NAME"));

            return sb.Content;
        }
        #endregion

        #region Table Foreign Key
        public override Task<List<TableForeignKeyItem>> GetTableForeignKeyItemsAsync(SchemaInfoFilter filter = null, bool isFilterForReferenced = false)
        {
            return base.GetDbObjectsAsync<TableForeignKeyItem>(this.GetSqlForTableForeignKeyItems(filter, isFilterForReferenced));
        }

        public override Task<List<TableForeignKeyItem>> GetTableForeignKeyItemsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null, bool isFilterForReferenced = false)
        {
            return base.GetDbObjectsAsync<TableForeignKeyItem>(dbConnection, this.GetSqlForTableForeignKeyItems(filter, isFilterForReferenced));
        }

        private string GetSqlForTableForeignKeyItems(SchemaInfoFilter filter = null, bool isFilterForReferenced = false)
        {
            string tableAlias = !isFilterForReferenced ? "UC" : "RUCC";

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT UC.OWNER AS ""Schema"", UC.TABLE_NAME AS ""TableName"", UC.CONSTRAINT_NAME AS ""Name"", UCC.column_name AS ""ColumnName"",
                        RUCC.OWNER AS ""ReferencedSchema"",RUCC.TABLE_NAME AS ""ReferencedTableName"",RUCC.COLUMN_NAME AS ""ReferencedColumnName"",
                        0 AS ""UpdateCascade"", CASE UC.DELETE_RULE WHEN 'CASCADE' THEN 1 ELSE 0 END AS ""DeleteCascade"" 
                        FROM USER_CONSTRAINTS UC                       
                        JOIN USER_CONS_COLUMNS UCC ON UC.OWNER=UCC.OWNER AND UC.TABLE_NAME=UCC.TABLE_NAME AND UC.CONSTRAINT_NAME=UCC.CONSTRAINT_NAME                       
                        JOIN USER_CONS_COLUMNS RUCC ON UC.OWNER=RUCC.OWNER AND UC.R_CONSTRAINT_NAME=RUCC.CONSTRAINT_NAME AND UCC.POSITION=RUCC.POSITION
                        WHERE UC.CONSTRAINT_TYPE='R' AND UPPER(UC.OWNER)=UPPER('{this.GetSchemaBySchemaFilter(filter)}')");

            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, $"{tableAlias}.TABLE_NAME"));

            return sb.Content;
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
            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT UI.TABLE_OWNER AS ""Schema"", UI.TABLE_NAME AS ""TableName"", UI.INDEX_NAME AS ""Name"", UIC.COLUMN_NAME AS ""ColumnName"", UIC.COLUMN_POSITION AS ""Order"",
                CASE UIC.DESCEND WHEN 'ASC' THEN 0 ELSE 1 END AS ""IsDesc"", CASE UI.UNIQUENESS WHEN 'UNIQUE' THEN 1 ELSE 0 END AS ""IsUnique"",
                UI.INDEX_TYPE AS ""Type"", CASE WHEN UC.CONSTRAINT_NAME IS NOT NULL THEN 1 ELSE 0 END AS ""IsPrimary"", CASE WHEN UC.CONSTRAINT_NAME IS NOT NULL THEN 1 ELSE 0 END AS ""Clustered""
                FROM USER_INDEXES UI
                JOIN USER_IND_COLUMNS UIC ON UI.INDEX_NAME = UIC.INDEX_NAME AND UI.TABLE_NAME = UIC.TABLE_NAME
                LEFT JOIN USER_CONSTRAINTS UC ON UI.TABLE_NAME = UC.TABLE_NAME AND UI.TABLE_OWNER = UC.OWNER AND UI.INDEX_NAME = UC.CONSTRAINT_NAME AND UC.CONSTRAINT_TYPE = 'P'
                WHERE UPPER(UI.TABLE_OWNER)=UPPER('{this.GetSchemaBySchemaFilter(filter)}'){(includePrimaryKey ? "" : " AND UC.CONSTRAINT_NAME IS NULL")}");

            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "UI.TABLE_NAME"));

            return sb.Content;
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

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT TRIGGER_NAME AS ""Name"",TABLE_OWNER AS ""Schema"", TABLE_NAME AS ""TableName"", 
                         {(isSimpleMode ? "''" : "('CREATE OR REPLACE TRIGGER ' || DESCRIPTION)")} AS ""CreateClause"",
                         {(isSimpleMode ? "''" : "TRIGGER_BODY")} AS ""Definition""
                        FROM USER_TRIGGERS
                        WHERE UPPER(TABLE_OWNER) = UPPER('{this.GetSchemaBySchemaFilter(filter)}')");

            if (filter != null)
            {
                sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "TABLE_NAME"));

                if (filter.TableTriggerNames != null && filter.TableTriggerNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter?.TableTriggerNames);
                    sb.Append($"AND TRIGGER_NAME IN ({strNames})");
                }
            }

            sb.Append("ORDER BY TRIGGER_NAME");

            return sb.Content;
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

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT OWNER AS ""Schema"", CONSTRAINT_NAME AS ""Name"", TABLE_NAME AS ""TableName"", {definitionField} AS ""Definition""
                         FROM ALL_CONSTRAINTS C
                         WHERE UPPER(OWNER) = UPPER('{this.GetSchemaBySchemaFilter(filter)}') 
                         AND CONSTRAINT_TYPE = 'C' AND GENERATED = 'USER NAME'");

            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "TABLE_NAME"));

            sb.Append("ORDER BY CONSTRAINT_NAME");

            return sb.Content;
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
            string definitionField = isLowDbVersion ? $"DBMS_METADATA.GET_DDL('VIEW',V.VIEW_NAME, '{this.ConnectionInfo.UserId}')" : @"'CREATE OR REPLACE VIEW ""'||V.VIEW_NAME||'"" AS ' || CHR(13) || TEXT_VC";

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT V.OWNER AS ""Schema"", V.VIEW_NAME AS ""Name"", {(isSimpleMode ? "''" : definitionField)} AS ""Definition"" 
                        FROM ALL_VIEWS V
                        WHERE UPPER(OWNER) = UPPER('{this.GetSchemaBySchemaFilter(filter)}')");

            sb.Append(this.GetFilterNamesCondition(filter, filter?.ViewNames, "V.VIEW_NAME"));

            sb.Append("ORDER BY VIEW_NAME");

            return sb.Content;
        }
        #endregion

        #region Procedure     

        public override Task<List<Procedure>> GetProceduresAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Procedure>(this.GetSqlForRoutines(DatabaseObjectType.Procedure, filter));
        }

        public override Task<List<Procedure>> GetProceduresAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Procedure>(dbConnection, this.GetSqlForRoutines(DatabaseObjectType.Procedure, filter));
        }
        #endregion

        #region Routine Parameter        
        public override Task<List<RoutineParameter>> GetFunctionParametersAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<RoutineParameter>(this.GetSqlForRoutineParameters(DatabaseObjectType.Function, filter));
        }
        public override Task<List<RoutineParameter>> GetFunctionParametersAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<RoutineParameter>(dbConnection, this.GetSqlForRoutineParameters(DatabaseObjectType.Function, filter));
        }

        public override Task<List<RoutineParameter>> GetProcedureParametersAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<RoutineParameter>(this.GetSqlForRoutineParameters(DatabaseObjectType.Procedure, filter));
        }
        public override Task<List<RoutineParameter>> GetProcedureParametersAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<RoutineParameter>(dbConnection, this.GetSqlForRoutineParameters(DatabaseObjectType.Procedure, filter));
        }

        private string GetSqlForRoutineParameters(DatabaseObjectType databaseObjectType, SchemaInfoFilter filter = null)
        {
            SqlBuilder sb = new SqlBuilder();

            bool isFunction = databaseObjectType == DatabaseObjectType.Function;

            string objectType = isFunction ? "FUNCTION" : "PROCEDURE";

            var routineNames = isFunction ? filter?.FunctionNames : filter?.ProcedureNames;

            sb.Append($@"SELECT a.OWNER AS ""Schema"",a.OBJECT_NAME AS ""RoutineName"",a.ARGUMENT_NAME AS ""Name"",a.POSITION AS ""Order"",
                         a.DATA_TYPE AS ""DataType"", a.DATA_LENGTH AS ""MaxLength"",a.DATA_PRECISION AS ""Precision"",a.DATA_SCALE AS ""Scale"",
                         CASE WHEN a.IN_OUT='IN' THEN 0 ELSE 1 END AS ""IsOutput""
                         FROM ALL_PROCEDURES p
                         JOIN SYS.ALL_ARGUMENTS a ON p.OBJECT_ID = a.OBJECT_ID AND p.OBJECT_TYPE='{objectType}'
                         WHERE a.ARGUMENT_NAME IS NOT NULL AND UPPER(a.OWNER) = UPPER('{this.GetSchemaBySchemaFilter(filter)}') ");

            sb.Append(this.GetFilterNamesCondition(filter, routineNames, "a.OBJECT_NAME"));

            sb.Append("ORDER BY a.OBJECT_NAME, a.POSITION");

            return sb.Content;
        }
        #endregion
        #endregion

        #region Dependency
        #region View->Table Usage
        public override Task<List<ViewTableUsage>> GetViewTableUsages(SchemaInfoFilter filter = null, bool isFilterForReferenced = false)
        {
            return base.GetDbObjectUsagesAsync<ViewTableUsage>(this.GetSqlForViewTableUsages(filter, isFilterForReferenced));
        }

        public override Task<List<ViewTableUsage>> GetViewTableUsages(DbConnection dbConnection, SchemaInfoFilter filter = null, bool isFilterForReferenced = false)
        {
            return base.GetDbObjectUsagesAsync<ViewTableUsage>(dbConnection, this.GetSqlForViewTableUsages(filter, isFilterForReferenced));
        }

        private string GetSqlForViewTableUsages(SchemaInfoFilter filter = null, bool isFilterForReferenced = false)
        {
            SqlBuilder sb = new SqlBuilder();

            string owner = !isFilterForReferenced ? "d.OWNER" : "d.REFERENCED_OWNER";

            sb.Append($@"SELECT d.OWNER AS ""ObjectSchema"",d.NAME AS ""ObjectName"",d.REFERENCED_OWNER AS ""RefObjectSchema"", d.REFERENCED_NAME AS ""RefObjectName""
                         FROM sys.all_dependencies d
                         WHERE d.TYPE = 'VIEW' and d.REFERENCED_TYPE='TABLE' AND d.REFERENCED_OWNER NOT IN('SYS','PUBLIC')
                         AND UPPER({owner})=UPPER('{this.GetSchemaBySchemaFilter(filter)}')");

            sb.Append(this.GetFilterNamesCondition(filter, !isFilterForReferenced ? filter?.ViewNames : filter?.TableNames, !isFilterForReferenced ? "d.NAME" : "d.REFERENCED_NAME"));

            return sb.Content;
        }
        #endregion

        #region View->Column Usage
        public override Task<List<ViewColumnUsage>> GetViewColumnUsages(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectUsagesAsync<ViewColumnUsage>("");
        }

        public override Task<List<ViewColumnUsage>> GetViewColumnUsages(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectUsagesAsync<ViewColumnUsage>(dbConnection, "");
        }
        #endregion

        #region Routine Script Usage
        public override Task<List<RoutineScriptUsage>> GetRoutineScriptUsages(SchemaInfoFilter filter = null, bool isFilterForReferenced = false, bool includeViewTableUsages = false)
        {
            return base.GetDbObjectUsagesAsync<RoutineScriptUsage>(this.GetSqlForRountineScriptUsages(filter, isFilterForReferenced,includeViewTableUsages));
        }

        public override Task<List<RoutineScriptUsage>> GetRoutineScriptUsages(DbConnection dbConnection, SchemaInfoFilter filter = null, bool isFilterForReferenced = false, bool includeViewTableUsages = false)
        {
            return base.GetDbObjectUsagesAsync<RoutineScriptUsage>(dbConnection, this.GetSqlForRountineScriptUsages(filter, isFilterForReferenced, includeViewTableUsages));
        }

        private string GetSqlForRountineScriptUsages(SchemaInfoFilter filter = null, bool isFilterForReferenced = false, bool includeViewTableUsages = false)
        {
            SqlBuilder sb = new SqlBuilder();

            string owner = !isFilterForReferenced ? "d.OWNER" : "d.REFERENCED_OWNER";

            sb.Append($@"SELECT SUBSTR(d.TYPE,1,1) || LOWER(SUBSTR(d.TYPE,2)) AS ""ObjectType"",SUBSTR(d.REFERENCED_TYPE,1,1) || LOWER(SUBSTR(d.REFERENCED_TYPE,2)) AS ""RefObjectType"",
                        d.OWNER AS ""ObjectSchema"",d.NAME AS ""ObjectName"",d.REFERENCED_OWNER AS ""RefObjectSchema"", d.REFERENCED_NAME AS ""RefObjectName""
                        FROM sys.all_dependencies d
                        WHERE d.REFERENCED_OWNER NOT IN('SYS','PUBLIC')
                        AND UPPER({owner})=UPPER('{this.GetSchemaBySchemaFilter(filter)}')");

            if(!includeViewTableUsages)
            {
                sb.Append("AND NOT (d.TYPE= 'VIEW' AND d.REFERENCED_TYPE='TABLE') AND NOT (d.TYPE= 'VIEW' AND d.REFERENCED_TYPE='VIEW')");
            }

            string typeColumn = !isFilterForReferenced ? "TYPE": "REFERENCED_TYPE";
            string nameColumn = !isFilterForReferenced ? "NAME" : "REFERENCED_NAME";
            string typeName = null;
            string[] filterNames = null;            

            if (filter?.DatabaseObjectType == DatabaseObjectType.Procedure)
            {
                typeName = "PROCEDURE";
                filterNames = filter?.ProcedureNames;
            }
            else if (filter?.DatabaseObjectType == DatabaseObjectType.Function)
            {
                typeName = "FUNCTION";
                filterNames = filter?.FunctionNames;               
            }
            else if (filter?.DatabaseObjectType == DatabaseObjectType.View)
            {
                typeName = "VIEW";
                filterNames = filter?.ViewNames;
            }
            else if (filter?.DatabaseObjectType == DatabaseObjectType.Table)
            {
                typeName = "TABLE";
                filterNames = filter?.TableNames;
            }

            if (typeName != null)
            {
                sb.Append($"AND d.{typeColumn} ='{typeName}'");               
            }

            sb.Append(this.GetFilterNamesCondition(filter, filterNames, $"d.{nameColumn}"));

            return sb.Content;
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
                bulkCopy.BulkCopyTimeout = bulkCopyInfo.Timeout.HasValue ? bulkCopyInfo.Timeout.Value : Setting.CommandTimeout; ;
                bulkCopy.ColumnNameNeedQuoted = this.DbObjectNameMode == DbObjectNameMode.WithQuotation;
                bulkCopy.DetectDateTimeTypeByValues = bulkCopyInfo.DetectDateTimeTypeByValues;

                await bulkCopy.WriteToServerAsync(this.ConvertDataTable(dataTable, bulkCopyInfo));
            }
        }

        private DataTable ConvertDataTable(DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            var columns = dataTable.Columns.Cast<DataColumn>();

            if (!columns.Any(item => item.DataType == typeof(MySqlDateTime)
               || item.DataType.Name == nameof(System.Collections.BitArray)
               || item.DataType.Name == nameof(SqlGeography)
               || item.DataType.Name == nameof(SqlGeometry)
               || item.DataType == typeof(byte[])
               || item.DataType == typeof(PgGeom.Geometry)
              ))
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

                        TableColumn tableColumn = bulkCopyInfo.Columns.FirstOrDefault(item => item.Name == dataTable.Columns[i].ColumnName);
                        string dataType = tableColumn.DataType.ToLower();

                        Type newColumnType = null;
                        dynamic newValue = null;

                        if (type != typeof(DBNull))
                        {
                            if (type == typeof(MySqlDateTime))
                            {
                                MySqlDateTime mySqlDateTime = (MySqlDateTime)value;

                                if (dataType.Contains("date") || dataType.Contains("timestamp"))
                                {
                                    DateTime dateTime = mySqlDateTime.GetDateTime();

                                    newColumnType = typeof(DateTime);

                                    newValue = dateTime;
                                }
                            }
                            else if (type == typeof(System.Collections.BitArray))
                            {
                                newColumnType = typeof(byte[]);

                                var bitArray = value as BitArray;
                                byte[] bytes = new byte[bitArray.Length];
                                bitArray.CopyTo(bytes, 0);

                                newValue = bytes;
                            }
                            else if (type == typeof(SqlGeography))
                            {
                                if (dataType == "sdo_geometry")
                                {
                                    newColumnType = typeof(StGeometry);

                                    newValue = SqlGeographyHelper.ToOracleSdoGeometry(value as SqlGeography);
                                }
                                else if (dataType == "st_geometry")
                                {
                                    newColumnType = typeof(StGeometry);

                                    newValue = SqlGeographyHelper.ToOracleStGeometry(value as SqlGeography);
                                }
                            }
                            else if (type == typeof(SqlGeometry))
                            {
                                if (dataType == "sdo_geometry")
                                {
                                    newColumnType = typeof(SdoGeometry);

                                    newValue = SqlGeometryHelper.ToOracleSdoGeometry(value as SqlGeometry);
                                }
                                else if (dataType == "st_geometry")
                                {
                                    newColumnType = typeof(StGeometry);

                                    newValue = SqlGeometryHelper.ToOracleStGeometry(value as SqlGeometry);
                                }
                            }
                            else if (type == typeof(byte[]))
                            {
                                DatabaseType sourcedDbType = bulkCopyInfo.SourceDatabaseType;

                                if (sourcedDbType == DatabaseType.MySql)
                                {
                                    if (dataType == "sdo_geometry")
                                    {
                                        newColumnType = typeof(SdoGeometry);

                                        newValue = MySqlGeometryHelper.ToOracleSdoGeometry(value as byte[]);
                                    }
                                    else if (dataType == "st_geometry")
                                    {
                                        newColumnType = typeof(StGeometry);

                                        newValue = MySqlGeometryHelper.ToOracleStGeometry(value as byte[]);
                                    }
                                }
                            }
                            else if (value is PgGeom.Geometry)
                            {
                                if (dataType == "sdo_geometry")
                                {
                                    newColumnType = typeof(SdoGeometry);

                                    newValue = PostgresGeometryHelper.ToOracleSdoGeometry(value as PgGeom.Geometry);
                                }
                                else if (dataType == "st_geometry")
                                {
                                    newColumnType = typeof(StGeometry);

                                    newValue = PostgresGeometryHelper.ToOracleStGeometry(value as PgGeom.Geometry);
                                }
                            }
                        }
                        else
                        {
                            if (dataType == "sdo_geometry")
                            {
                                newColumnType = typeof(SdoGeometry);
                            }
                            else if (dataType == "st_geometry")
                            {
                                newColumnType = typeof(StGeometry);
                            }
                        }

                        if (DataTypeHelper.IsGeometryType(dataType) && newColumnType != null && newValue == null)
                        {
                            newValue = DBNull.Value;
                        }

                        if (newColumnType != null && !changedColumns.ContainsKey(i))
                        {
                            changedColumns.Add(i, new DataTableColumnChangeInfo() { Type = newColumnType });
                        }

                        if (newValue != null)
                        {
                            changedValues.Add((rowIndex, i), newValue);
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
            return this.GetRecordCount(table, connection, whereClause);
        }       

        private Task<long> GetRecordCount(DatabaseObject dbObject, DbConnection connection, string whereClause = "")
        {
            string sql = $@"SELECT COUNT(1) FROM {this.GetDbSchema()}.{this.GetQuotedString(dbObject.Name)}";

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
            string requiredClause = (column.IsRequired ? "NOT NULL" : "NULL");

            if (column.IsComputed)
            {
                string computeExpression = this.GetColumnComputeExpression(column);

                return $"{this.GetQuotedString(column.Name)} AS ({computeExpression}) {requiredClause}";
            }
            else
            {
                bool isLowDbVersion = this.IsLowDbVersion();

                string dataType = this.ParseDataType(column);
                string identityClause = (this.Option.TableScriptsGenerateOption.GenerateIdentity && column.IsIdentity && !isLowDbVersion ? $"GENERATED ALWAYS AS IDENTITY" : "");
                string defaultValueClause = "";

                if (column.DefaultValue != null && !ValueHelper.IsSequenceNextVal(column.DefaultValue))
                {
                    defaultValueClause = this.Option.TableScriptsGenerateOption.GenerateDefaultValue && !string.IsNullOrEmpty(column.DefaultValue) ? (" DEFAULT " + StringHelper.GetParenthesisedString(this.GetColumnDefaultValue(column))) : "";
                }

                string scriptComment = string.IsNullOrEmpty(column.ScriptComment) ? "" : $"/*{column.ScriptComment}*/";

                string content = $"{this.GetQuotedString(column.Name)} {dataType}{defaultValueClause} {identityClause} {requiredClause} {scriptComment}";

                return content;
            }
        }

        public override string ParseDataType(TableColumn column)
        {
            if (DataTypeHelper.IsUserDefinedType(column))
            {
                return this.GetQuotedString(column.DataType);
            }

            string dataType = column.DataType;

            if (dataType.IndexOf("(") < 0)
            {
                DataTypeSpecification dataTypeSpec = this.GetDataTypeSpecification(dataType.ToLower());

                bool applied = false;
                if (dataTypeSpec != null)
                {
                    string format = dataTypeSpec.Format;
                    string args = dataTypeSpec.Args;

                    if (!string.IsNullOrEmpty(args))
                    {
                        if (!string.IsNullOrEmpty(format))
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
                            applied = true;
                        }
                    }
                    else
                    {
                        if (dataType.ToLower() == "st_geometry")
                        {
                            string dataTypeSchema = column.DataTypeSchema?.ToUpper();

                            if (!string.IsNullOrEmpty(dataTypeSchema) && GeometryTypeSchemas.Contains(dataTypeSchema))
                            {
                                dataType = $"{dataTypeSchema}.{dataType}";
                            }
                            else
                            {
                                dataType = $@"MDSYS.{dataType}";
                            }

                            applied = true;
                        }
                    }
                }

                if (!applied)
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

            DataTypeInfo dataTypeInfo = this.GetDataTypeInfo(dataType);
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

                            if (length > 0 && DataTypeHelper.StartsWithN(dataType))
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
                                if (dataTypeSpec.Args == "length")
                                {
                                    dataLength = column.MaxLength.ToString();
                                }
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
