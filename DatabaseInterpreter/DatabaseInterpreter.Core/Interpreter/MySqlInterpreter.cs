using Dapper;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Microsoft.SqlServer.Types;
using MySqlConnector;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace DatabaseInterpreter.Core
{
    public class MySqlInterpreter : DbInterpreter
    {
        #region Field & Property           
        public const int DEFAULT_PORT = 3306;
        public override string UnicodeInsertChar => "";
        public override string CommandParameterChar => "@";
        public const char QuotedLeftChar = '`';
        public const char QuotedRightChar = '`';
        public override char QuotationLeftChar { get { return QuotedLeftChar; } }
        public override char QuotationRightChar { get { return QuotedRightChar; } }
        public override string CommentString => "#";
        public override DatabaseType DatabaseType => DatabaseType.MySql;
        public override string DefaultDataType => "varchar";
        public override string DefaultSchema => this.ConnectionInfo.Database;
        public override IndexType IndexType => IndexType.Normal | IndexType.FullText | IndexType.Primary;
        public override bool SupportBulkCopy { get { return true; } }
        public override List<string> BuiltinDatabases => new List<string> { "sys", "mysql", "information_schema", "performance_schema" };

        public const int NameMaxLength = 64;
        public const int KeyIndexColumnMaxLength = 500;
        public readonly string DbCharset = SettingManager.Setting.MySqlCharset;
        public readonly string DbCharsetCollation = SettingManager.Setting.MySqlCharsetCollation;
        public string NotCreateIfExistsClause { get { return this.NotCreateIfExists ? "IF NOT EXISTS" : ""; } }
        #endregion

        #region Constructor
        public MySqlInterpreter(ConnectionInfo connectionInfo, DbInterpreterOption option) : base(connectionInfo, option)
        {
            this.dbConnector = this.GetDbConnector();
        }
        #endregion

        #region Common Method
        public override DbConnector GetDbConnector()
        {
            return new DbConnector(new MySqlProvider(), new MySqlConnectionBuilder(), this.ConnectionInfo);
        }

        public override bool IsLowDbVersion(string version)
        {
            if (version != null)
            {
                if (string.Compare(version.Substring(0, 1), "8") < 0)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Schema Information
        #region Database
        public override Task<List<Database>> GetDatabasesAsync()
        {
            string notShowBuiltinDatabaseCondition = "";

            if (!this.ShowBuiltinDatabase)
            {
                string strBuiltinDatabase = this.BuiltinDatabases.Count > 0 ? string.Join(",", this.BuiltinDatabases.Select(item => $"'{item}'")) : "";
                notShowBuiltinDatabaseCondition = string.IsNullOrEmpty(strBuiltinDatabase) ? "" : $"WHERE SCHEMA_NAME NOT IN({strBuiltinDatabase})";
            }

            string sql = $"SELECT SCHEMA_NAME AS `Name` FROM INFORMATION_SCHEMA.`SCHEMATA` {notShowBuiltinDatabaseCondition}";

            return base.GetDbObjectsAsync<Database>(sql);
        }

        public string GetDatabaseVersion()
        {
            return this.GetDatabaseVersion(this.dbConnector.CreateConnection());
        }

        public string GetDatabaseVersion(DbConnection dbConnection)
        {
            string sql = "select version() as version";
            return dbConnection.QuerySingleOrDefault(sql).version;
        }
        #endregion

        #region Database Schema
        public override async Task<List<DatabaseSchema>> GetDatabaseSchemasAsync()
        {
            string database = this.ConnectionInfo.Database;

            List<DatabaseSchema> databaseSchemas = new List<DatabaseSchema>() { new DatabaseSchema() { Schema = database, Name = database } };

            return await Task.Run(()=> { return  databaseSchemas; });           
        }
        #endregion

        #region User Defined Type       

        public override Task<List<UserDefinedType>> GetUserDefinedTypesAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<UserDefinedType>("");
        }

        public override Task<List<UserDefinedType>> GetUserDefinedTypesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<UserDefinedType>(dbConnection, "");
        }
        #endregion

        #region Sequence      

        public override Task<List<Sequence>> GetSequencesAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Sequence>("");
        }

        public override Task<List<Sequence>> GetSequencesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Sequence>(dbConnection, "");
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

            string nameColumn = isSimpleMode ? "ROUTINE_NAME" : "name";

            string sql = "";
            bool isFunction = type.ToUpper() == "FUNCTION";
            string[] objectNames = type == "FUNCTION" ? filter?.FunctionNames : filter?.ProcedureNames;
            string strNames = "";

            if (objectNames != null && objectNames.Any())
            {
                strNames = StringHelper.GetSingleQuotedString(objectNames);
            }

            if (isSimpleMode)
            {
                sql = $@"SELECT ROUTINE_NAME AS `Name`, ROUTINE_SCHEMA AS `Schema`                        
                        FROM INFORMATION_SCHEMA.`ROUTINES`
                        WHERE ROUTINE_TYPE = '{type}' AND ROUTINE_SCHEMA = '{this.ConnectionInfo.Database}'
                       ";

                if (strNames != "")
                {
                    sql += $" AND {nameColumn} IN ({strNames})";
                }

                sql += $" ORDER BY {nameColumn}";
            }
            else
            {
                string functionReturns = isFunction ? ", 'RETURNS ',IFNULL(r.DATA_TYPE,''), ' '" : "";
                string procParameterMode = isFunction ? "" : "IFNULL(p.PARAMETER_MODE,''),' ',";

                string condition = strNames == "" ? "" : $" AND r.ROUTINE_NAME IN({strNames})";

                sql = $@"SELECT ROUTINE_SCHEMA AS `Schema`, ROUTINE_NAME AS `Name`,
                        CONVERT(CONCAT('CREATE {type}  `', ROUTINE_SCHEMA, '`.`', ROUTINE_NAME, '`(', 
                        IFNULL(GROUP_CONCAT(CONCAT(IFNULL(CASE p.PARAMETER_MODE WHEN 'IN' THEN '' ELSE p.PARAMETER_MODE END,''),' ',p.PARAMETER_NAME, ' ', p.`DTD_IDENTIFIER`)),''), 
                        ') '{functionReturns}, '{Environment.NewLine}', ROUTINE_DEFINITION) USING utf8)  AS `Definition` 
                        FROM information_schema.Routines r
                        LEFT JOIN information_schema.`PARAMETERS` p ON r.`ROUTINE_SCHEMA`= p.`SPECIFIC_SCHEMA` AND r.`ROUTINE_NAME`= p.`SPECIFIC_NAME`
                        WHERE r.ROUTINE_TYPE = '{type}' AND ROUTINE_SCHEMA = '{this.ConnectionInfo.Database}'{condition}
                        GROUP BY ROUTINE_SCHEMA,ROUTINE_NAME
                        ORDER BY r.ROUTINE_NAME";
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

            string sql = $@"SELECT TABLE_SCHEMA AS `Schema`, TABLE_NAME AS `Name` {(isSimpleMode ? "" : ", TABLE_COMMENT AS `Comment`, 1 AS `IdentitySeed`, 1 AS `IdentityIncrement`")}
                        FROM INFORMATION_SCHEMA.`TABLES`
                        WHERE TABLE_TYPE ='BASE TABLE' AND TABLE_SCHEMA ='{this.ConnectionInfo.Database}' 
                        ";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND TABLE_NAME IN ({strTableNames})";
            }

            sql += " ORDER BY TABLE_NAME";

            return sql;
        }
        #endregion

        #region Table Column
        public override Task<List<TableColumn>> GetTableColumnsAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableColumn>(this.GetSqlForTableColumns(filter));
        }

        public override Task<List<TableColumn>> GetTableColumnsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableColumn>(dbConnection, this.GetSqlForTableColumns(filter));
        }

        private string GetSqlForTableColumns(SchemaInfoFilter filter = null)
        {
            string sql = $@"SELECT C.TABLE_SCHEMA AS `Schema`, C.TABLE_NAME AS `TableName`, COLUMN_NAME AS `Name`, COLUMN_TYPE AS `DataType`, 
                        CHARACTER_MAXIMUM_LENGTH AS `MaxLength`, CASE IS_NULLABLE WHEN 'YES' THEN 1 ELSE 0 END AS `IsNullable`,ORDINAL_POSITION AS `Order`,
                        NUMERIC_PRECISION AS `Precision`,NUMERIC_SCALE AS `Scale`, COLUMN_DEFAULT AS `DefaultValue`,COLUMN_COMMENT AS `Comment`,
                        CASE EXTRA WHEN 'auto_increment' THEN 1 ELSE 0 END AS `IsIdentity`,'' AS `DataTypeSchema`,GENERATION_EXPRESSION AS `ComputeExp`
                        FROM INFORMATION_SCHEMA.`COLUMNS` AS C
                        JOIN INFORMATION_SCHEMA.`TABLES` AS T ON T.`TABLE_NAME`= C.`TABLE_NAME` AND T.TABLE_TYPE='BASE TABLE' AND T.TABLE_SCHEMA=C.TABLE_SCHEMA
                        WHERE C.TABLE_SCHEMA ='{this.ConnectionInfo.Database}'";

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
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            string commentColumn = isSimpleMode ? "" : ",S.INDEX_COMMENT AS `Comment`";
            string commentJoin = isSimpleMode ? "" : "LEFT JOIN INFORMATION_SCHEMA.STATISTICS AS S ON K.TABLE_SCHEMA=S.TABLE_SCHEMA AND K.TABLE_NAME=S.TABLE_NAME AND K.CONSTRAINT_NAME=S.INDEX_NAME AND K.ORDINAL_POSITION=S.SEQ_IN_INDEX";

            //Note:TABLE_SCHEMA of INFORMATION_SCHEMA.KEY_COLUMN_USAGE will improve performance when it's used in where clause, just use CONSTRAINT_SCHEMA in join on clause because it equals to TABLE_SCHEMA.

            string sql = $@"SELECT C.`CONSTRAINT_SCHEMA` AS `Schema`, K.TABLE_NAME AS `TableName`, K.CONSTRAINT_NAME AS `Name`, 
                        K.COLUMN_NAME AS `ColumnName`, K.`ORDINAL_POSITION` AS `Order`, 0 AS `IsDesc`{commentColumn}
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K ON C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA AND C.TABLE_NAME = K.TABLE_NAME AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME
                        {commentJoin}
                        WHERE C.CONSTRAINT_TYPE = 'PRIMARY KEY'
                        AND K.TABLE_SCHEMA ='{this.ConnectionInfo.Database}'";

            if (filter.TableNames != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND C.TABLE_NAME IN ({strTableNames})";
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
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            string commentColumn = isSimpleMode ? "" : ",S.`INDEX_COMMENT` AS `Comment`";
            string commentJoin = isSimpleMode ? "" : "LEFT JOIN INFORMATION_SCHEMA.STATISTICS AS S ON K.TABLE_SCHEMA=S.TABLE_SCHEMA AND K.TABLE_NAME=S.TABLE_NAME AND K.CONSTRAINT_NAME=S.INDEX_NAME AND K.ORDINAL_POSITION=S.SEQ_IN_INDEX";

            string sql = $@"SELECT C.`CONSTRAINT_SCHEMA` AS `Schema`, K.TABLE_NAME AS `TableName`, K.CONSTRAINT_NAME AS `Name`, 
                        K.COLUMN_NAME AS `ColumnName`, K.`REFERENCED_TABLE_NAME` AS `ReferencedTableName`,K.`REFERENCED_COLUMN_NAME` AS `ReferencedColumnName`,
                        CASE RC.UPDATE_RULE WHEN 'CASCADE' THEN 1 ELSE 0 END AS `UpdateCascade`, 
                        CASE RC.`DELETE_RULE` WHEN 'CASCADE' THEN 1 ELSE 0 END AS `DeleteCascade`{commentColumn}
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K ON C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA AND C.TABLE_NAME = K.TABLE_NAME AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME
                        JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC ON RC.CONSTRAINT_SCHEMA=C.CONSTRAINT_SCHEMA AND RC.CONSTRAINT_NAME=C.CONSTRAINT_NAME AND C.TABLE_NAME=RC.TABLE_NAME                        
                        {commentJoin}
                        WHERE C.CONSTRAINT_TYPE = 'FOREIGN KEY'
                        AND K.`TABLE_SCHEMA` ='{this.ConnectionInfo.Database}'";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND C.TABLE_NAME IN ({strTableNames})";
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
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            string commentColumn = isSimpleMode ? "" : ",`INDEX_COMMENT` AS `Comment`";

            string sql = $@"SELECT TABLE_SCHEMA AS `Schema`,
	                        TABLE_NAME AS `TableName`,
	                        INDEX_NAME AS `Name`,
	                        COLUMN_NAME AS `ColumnName`,
                            CASE INDEX_NAME WHEN 'PRIMARY' THEN 1 ELSE 0 END AS `IsPrimary`,
	                        CASE  NON_UNIQUE WHEN 1 THEN 0 ELSE 1 END AS `IsUnique`,
                            INDEX_TYPE AS `Type`,
	                        SEQ_IN_INDEX  AS `Order`,    
	                        0 AS `IsDesc`{commentColumn}
	                        FROM INFORMATION_SCHEMA.STATISTICS                           
	                        WHERE INDEX_NAME NOT IN({(includePrimaryKey ? "" : "'PRIMARY',")} 'FOREIGN')                          
	                        AND TABLE_SCHEMA = '{this.ConnectionInfo.Database}'";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND TABLE_NAME IN ({strTableNames})";
            }

            return sql;
        }
        #endregion

        #region Table Trigger  
        public override Task<List<TableTrigger>> GetTableTriggersAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableTrigger>(this.GetSqlForTableTriggers(filter));
        }

        public override Task<List<TableTrigger>> GetTableTriggersAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableTrigger>(dbConnection, this.GetSqlForTableTriggers(filter));
        }

        private string GetSqlForTableTriggers(SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string definitionClause = $@"CONVERT(CONCAT('CREATE TRIGGER {this.NotCreateIfExistsClause} `', TRIGGER_SCHEMA, '`.`', TRIGGER_NAME, '` ', ACTION_TIMING, ' ', EVENT_MANIPULATION, ' ON ', TRIGGER_SCHEMA, '.', EVENT_OBJECT_TABLE, ' FOR EACH ', ACTION_ORIENTATION, '{Environment.NewLine}', ACTION_STATEMENT) USING UTF8)";

            string sql = $@"SELECT TRIGGER_NAME AS `Name`, TRIGGER_SCHEMA AS `Schema`, EVENT_OBJECT_TABLE AS `TableName`, 
                         {(isSimpleMode ? "''" : definitionClause)} AS `Definition`
                        FROM INFORMATION_SCHEMA.`TRIGGERS`
                        WHERE TRIGGER_SCHEMA = '{this.ConnectionInfo.Database}'
                        ";

            if (filter != null)
            {
                if (filter.TableNames != null && filter.TableNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                    sql += $" AND EVENT_OBJECT_TABLE IN ({strNames})";
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
            return base.GetDbObjectsAsync<TableConstraint>(this.GetSqlForTableConstraints(filter));
        }

        public override Task<List<TableConstraint>> GetTableConstraintsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableConstraint>(dbConnection, this.GetSqlForTableConstraints(filter));
        }

        private string GetSqlForTableConstraints(SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            string sql = "";

            if (isSimpleMode)
            {
                sql = @"SELECT TC.CONSTRAINT_SCHEMA AS `Schema`,TC.TABLE_NAME AS `TableName`, TC.CONSTRAINT_NAME AS `Name`
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC 
                        WHERE AND CONSTRAINT_TYPE='CHECK'";
            }
            else
            {
                sql = $@"SELECT TC.CONSTRAINT_SCHEMA AS `Schema`,TC.TABLE_NAME AS `TableName`, TC.CONSTRAINT_NAME AS `Name`,
                         REPLACE(REPLACE(REPLACE(C.CHECK_CLAUSE,'\\',''),(SELECT CONCAT('_',DEFAULT_CHARACTER_SET_NAME) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = ""INFORMATION_SCHEMA""),''),
                         (SELECT CONCAT('_',DEFAULT_CHARACTER_SET_NAME) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = ""{this.ConnectionInfo.Database}""),'') AS `Definition`
                         FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
                         JOIN INFORMATION_SCHEMA.CHECK_CONSTRAINTS C ON TC.CONSTRAINT_CATALOG=C.CONSTRAINT_CATALOG AND TC.CONSTRAINT_SCHEMA=C.CONSTRAINT_SCHEMA AND TC.CONSTRAINT_NAME=C.CONSTRAINT_NAME
                         WHERE CONSTRAINT_TYPE='CHECK'";
            }

            sql += $" AND TC.CONSTRAINT_SCHEMA='{this.ConnectionInfo.Database}'";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND TC.TABLE_NAME IN ({strTableNames})";
            }

            sql += " ORDER BY TC.CONSTRAINT_NAME";

            return sql;
        }
        #endregion

        #region View   
        public override Task<List<View>> GetViewsAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<View>(this.GetSqlForViews(filter));
        }

        public override Task<List<View>> GetViewsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<View>(dbConnection, this.GetSqlForViews(filter));
        }

        private string GetSqlForViews(SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string createViewClause = $"CONCAT('CREATE VIEW `',TABLE_SCHEMA, '`.`', TABLE_NAME,  '` AS','{Environment.NewLine}',VIEW_DEFINITION)";

            string sql = $@"SELECT TABLE_SCHEMA AS `Schema`,TABLE_NAME AS `Name`, {(isSimpleMode ? "''" : createViewClause)} AS `Definition` 
                        FROM INFORMATION_SCHEMA.`VIEWS`
                        WHERE TABLE_SCHEMA = '{this.ConnectionInfo.Database}'";

            if (filter != null && filter.ViewNames != null && filter.ViewNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.ViewNames);
                sql += $" AND TABLE_NAME IN ({strNames})";
            }

            sql += " ORDER BY TABLE_NAME";

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

        #region Datbase Operation

        public override Task<long> GetTableRecordCountAsync(DbConnection connection, Table table, string whereClause = "")
        {
            string sql = $"SELECT COUNT(1) FROM {this.GetQuotedDbObjectNameWithSchema(table)}";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += whereClause;
            }

            return base.GetTableRecordCountAsync(connection, sql);
        }
        #endregion

        #region BulkCopy
        public override async Task BulkCopyAsync(DbConnection connection, DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            if (dataTable == null || dataTable.Rows.Count <= 0)
            {
                return;
            }

            MySqlBulkCopy bulkCopy = new MySqlBulkCopy(connection as MySqlConnection, bulkCopyInfo.Transaction as MySqlTransaction);

            bulkCopy.DestinationTableName = this.GetQuotedString(bulkCopyInfo.DestinationTableName);

            int i = 0;
            foreach (DataColumn column in dataTable.Columns)
            {                
                bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, column.ColumnName));

                i++;
            }

            await this.OpenConnectionAsync(connection);
           
            await bulkCopy.WriteToServerAsync(this.ConvertDataTable(dataTable, bulkCopyInfo), bulkCopyInfo.CancellationToken);
        }

        private DataTable ConvertDataTable(DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            var columns = dataTable.Columns.Cast<DataColumn>();

            if (!columns.Any(item => DataTypeHelper.SpecialDataTypes.Contains(item.DataType.Name)
                || item.DataType.Name == nameof(BitArray)
                || item.DataType.Name == nameof(String)
                )
               )
            {
                return dataTable;
            }

            Func<DataColumn, TableColumn> getTableColumn = (column) =>
            {
                return bulkCopyInfo.Columns.FirstOrDefault(item => item.Name == column.ColumnName);
            };

            Dictionary<string, Type> dictColumnTypes = new Dictionary<string, Type>();
            Dictionary<int, DataTableColumnChangeInfo> changedColumns = new Dictionary<int, DataTableColumnChangeInfo>();
            Dictionary<(int RowIndex, int ColumnIndex), dynamic> changedValues = new Dictionary<(int RowIndex, int ColumnIndex), dynamic>();

            int rowIndex = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    var value = row[i];

                    if (value == null)
                    {
                        continue;
                    }

                    Type type = value.GetType();

                    if (type == typeof(DBNull))
                    {
                        continue;
                    }

                    Type newColumnType = null;
                    object newValue = null;

                    TableColumn tableColumn = getTableColumn(dataTable.Columns[i]);
                    string dataType = tableColumn.DataType.ToLower();

                    if (DataTypeHelper.IsCharType(dataType) || DataTypeHelper.IsTextType(dataType))
                    {
                        newColumnType = typeof(string);
                        newValue = value == null ? null : (type == typeof(string) ? value?.ToString() : Convert.ChangeType(value, type));
                    }
                    else if (type == typeof(BitArray))
                    {
                        var bitArray = value as BitArray;
                        byte[] bytes = new byte[bitArray.Length];
                        bitArray.CopyTo(bytes, 0);

                        newColumnType = typeof(Byte[]);
                        newValue = bytes;
                    }
                    else if (DataTypeHelper.IsBinaryType(dataType) || dataType.ToLower().Contains("blob"))
                    {
                        newColumnType = typeof(Byte[]);
                        newValue = value as byte[];
                    }
                    else if (dataType == "geometry")
                    {
                        newColumnType = typeof(MySqlGeometry);

                        if (value is SqlGeography geography)
                        {
                            newValue = GeometryHelper.SqlGeographyToMySqlGeometry(geography);
                        }
                        else if (value is SqlGeometry sqlGeom)
                        {
                            newValue = GeometryHelper.SqlGeometryToMySqlGeometry(sqlGeom);
                        }
                        else if (value is Geometry geom)
                        {
                            newValue = GeometryHelper.PostgresGeometryToMySqlGeometry(geom);
                        }
                        else if (value is byte[] bytes)
                        {
                            newValue = SqlGeometry.STGeomCollFromWKB(new System.Data.SqlTypes.SqlBytes(bytes), 0);
                        }
                        else if (value is string)
                        {
                            newValue = GeometryHelper.SqlGeometryToMySqlGeometry(SqlGeometry.STGeomFromText(new System.Data.SqlTypes.SqlChars(value as string), 0));
                        }
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
        protected override string GetSqlForPagination(string tableName, string columnNames, string orderColumns, string whereClause, long pageNumber, int pageSize)
        {
            var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

            var pagedSql = $@"SELECT {columnNames}
							  FROM {tableName}
                             {whereClause} 
                             ORDER BY {(!string.IsNullOrEmpty(orderColumns) ? orderColumns : this.GetDefaultOrder())}
                             LIMIT {startEndRowNumber.StartRowNumber - 1} , {pageSize}";

            return pagedSql;
        }

        public override string GetDefaultOrder()
        {
            return "1";
        }

        public override string GetLimitStatement(int limitStart, int limitCount)
        {
            return $"LIMIT {limitStart}, {limitCount}";
        }
        #endregion

        #region Parse Column & DataType 
        public override string ParseColumn(Table table, TableColumn column)
        {
            string dataType = this.ParseDataType(column);
            bool isChar = DataTypeHelper.IsCharType(dataType.ToLower());

            if (isChar || DataTypeHelper.IsTextType(dataType.ToLower()))
            {
                dataType += $" CHARACTER SET {DbCharset} COLLATE {DbCharsetCollation} ";
            }

            if (column.IsComputed)
            {
                string computeExpression = this.GetColumnComputeExpression(column);

                return $"{this.GetQuotedString(column.Name)} {dataType} AS {computeExpression}";
            }
            else
            {
                string requiredClause = (column.IsRequired ? "NOT NULL" : "NULL");
                string identityClause = (this.Option.TableScriptsGenerateOption.GenerateIdentity && column.IsIdentity ? $"AUTO_INCREMENT" : "");
                string commentClause = (!string.IsNullOrEmpty(column.Comment) && this.Option.TableScriptsGenerateOption.GenerateComment ? $"COMMENT '{this.ReplaceSplitChar(ValueHelper.TransferSingleQuotation(column.Comment))}'" : "");
                string defaultValueClause = this.Option.TableScriptsGenerateOption.GenerateDefaultValue && !string.IsNullOrEmpty(column.DefaultValue) && !column.DefaultValue.Contains("nextval") ? (" DEFAULT " + this.GetColumnDefaultValue(column)) : "";
                string scriptComment = string.IsNullOrEmpty(column.ScriptComment) ? "" : $"/*{column.ScriptComment}*/";

                return $"{this.GetQuotedString(column.Name)} {dataType} {requiredClause} {identityClause}{defaultValueClause} {scriptComment}{commentClause}";
            }
        }

        public override string ParseDataType(TableColumn column)
        {
            string dataType = column.DataType;

            if (dataType.IndexOf("(") < 0)
            {
                string dataLength = this.GetColumnDataLength(column);

                if (!string.IsNullOrEmpty(dataLength))
                {
                    dataType += $"({dataLength})";
                }
            }

            return dataType.Trim();
        }

        public override string GetColumnDataLength(TableColumn column)
        {
            string dataType = column.DataType;
            string dataLength = string.Empty;

            DataTypeInfo dataTypeInfo = DataTypeHelper.GetDataTypeInfo(this, dataType);
            bool isChar = DataTypeHelper.IsCharType(dataType);
            bool isBinary = DataTypeHelper.IsBinaryType(dataType);

            DataTypeSpecification dataTypeSpec = this.GetDataTypeSpecification(dataTypeInfo.DataType);

            if (dataTypeSpec != null)
            {
                if (!string.IsNullOrEmpty(dataTypeSpec.Args))
                {
                    if (string.IsNullOrEmpty(dataTypeInfo.Args))
                    {
                        if (isChar || isBinary)
                        {
                            dataLength = column.MaxLength.ToString();
                        }
                        else if (!this.IsNoLengthDataType(dataType))
                        {
                            dataLength = this.GetDataTypePrecisionScale(column, dataTypeInfo.DataType);
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
