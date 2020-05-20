using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class SqlServerInterpreter : DbInterpreter
    {
        #region Field & Property
        private readonly string azureSQLFlag = "SQL Azure";
        public override string CommandParameterChar { get { return "@"; } }
        public override char QuotationLeftChar { get { return '['; } }
        public override char QuotationRightChar { get { return ']'; } }
        public override DatabaseType DatabaseType => DatabaseType.SqlServer;
        public override IndexType IndexType => IndexType.Normal | IndexType.Unique | IndexType.ColumnStore | IndexType.Primary;
        public override bool SupportBulkCopy { get { return true; } }
        public override string ScriptsDelimiter => "GO" + Environment.NewLine;
        public override string CommentString => "--";
        public override List<string> BuiltinDatabases => new List<string> { "master", "model", "msdb", "tempdb" };
        #endregion

        #region Constructor
        public SqlServerInterpreter(ConnectionInfo connectionInfo, DbInterpreterOption option) : base(connectionInfo, option)
        {
            this.dbConnector = this.GetDbConnector();
        }
        #endregion

        #region Common Method
        public override DbConnector GetDbConnector()
        {
            return new DbConnector(new SqlServerProvider(), new SqlServerConnectionBuilder(), this.ConnectionInfo);
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
                notShowBuiltinDatabaseCondition = string.IsNullOrEmpty(strBuiltinDatabase) ? "" : $"WHERE name not in({strBuiltinDatabase})";
            }

            string sql = $@"SELECT name AS [Name] FROM sys.databases {notShowBuiltinDatabaseCondition} ORDER BY name";

            return base.GetDbObjectsAsync<Database>(sql);
        }
        #endregion

        #region Database Owner
        public override Task<List<DatabaseOwner>> GetDatabaseOwnersAsync()
        {
            string sql = @"select name as [Name], name as [Owner]  from sys.schemas
                           where name not in ('guest', 'sys', 'INFORMATION_SCHEMA') and name not like 'db[_]%'";

            return base.GetDbObjectsAsync<DatabaseOwner>(sql);
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
            string sql = @"SELECT schema_name(T.schema_id) AS [Owner],T.name as [Name], T.name as [AttrName], ST.name AS [Type], T.max_length AS [MaxLength], T.precision AS [Precision],T.scale AS [Scale],T.is_nullable AS IsNullable
                            FROM sys.types T JOIN sys.systypes ST ON T.system_type_id=ST.xusertype
                            WHERE is_user_defined=1";

            if (filter != null && filter.UserDefinedTypeNames != null && filter.UserDefinedTypeNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.UserDefinedTypeNames);
                sql += $" AND T.name in ({ strNames })";
            }

            return sql;
        }
        #endregion

        #region Function       

        public override Task<List<Function>> GetFunctionsAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Function>(this.GetSqlForFunctions(filter));
        }

        public override Task<List<Function>> GetFunctionsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Function>(dbConnection, this.GetSqlForFunctions(filter));
        }

        private string GetSqlForFunctions(SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = $@"SELECT o.name AS [Name], schema_name(o.schema_id) AS [Owner], 
                           {(isSimpleMode ? "''" : "OBJECT_DEFINITION(o.object_id)")} AS [Definition]
                           FROM sys.all_objects o 
                           WHERE o.type IN ('FN', 'IF', 'AF', 'FS', 'FT','TF')
                           AND SCHEMA_NAME(schema_id)='dbo'";

            if (filter != null && filter.FunctionNames != null && filter.FunctionNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.FunctionNames);
                sql += $" AND o.name IN ({ strNames })";
            }

            sql += " ORDER BY o.name";

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
            string sql = "";

            if (this.IsObjectFectchSimpleMode())
            {
                sql = $@"SELECT schema_name(t.schema_id) AS [Owner], t.name AS [Name]
                         FROM sys.tables t
                         WHERE 1=1";
            }
            else
            {
                sql = $@"SELECT schema_name(t.schema_id) AS [Owner], t.name AS [Name], ext2.value AS [Comment],
                        IDENT_SEED(schema_name(t.schema_id)+'.'+t.name) AS [IdentitySeed],IDENT_INCR(schema_name(t.schema_id)+'.'+t.name) AS [IdentityIncrement]
                        FROM sys.tables t
                        LEFT JOIN sys.extended_properties ext ON t.object_id=ext.major_id AND ext.minor_id=0 AND ext.class=1 AND ext.name='microsoft_database_tools_support'
                        LEFT JOIN sys.extended_properties ext2 ON t.object_id=ext2.major_id and ext2.minor_id=0 AND ext2.class_desc='OBJECT_OR_COLUMN' AND ext2.name='MS_Description'
                        WHERE t.is_ms_shipped=0 AND ext.class is null
                       ";
            }

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND t.name in ({ strTableNames })";
            }

            sql += " ORDER BY t.name";

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
            //Note: MaxLength consider char/nvarchar, ie. it's nvarchar(50), the max length is 100.

            string sql = "";

            if (this.IsObjectFectchSimpleMode())
            {
                sql = @"SELECT schema_name(t.schema_id) AS [Owner], 
                            t.name AS [TableName],
                            c.name AS [Name], 
                            st.name AS [DataType],
                            c.is_nullable AS [IsNullable],
                            c.max_length AS [MaxLength], 
                            c.precision AS [Precision],
                            c.column_id as [Order], 
                            c.scale AS [Scale],                           
                            c.is_identity AS [IsIdentity]                       
                        FROM sys.columns c 
                        JOIN sys.systypes st ON c.user_type_id = st.xusertype
                        JOIN sys.tables t ON c.object_id=t.object_id";
            }
            else
            {
                sql = @"SELECT schema_name(t.schema_id) AS [Owner], 
                            t.name AS [TableName],
                            c.name AS [Name], 
                            st.name AS [DataType],
                            c.is_nullable AS [IsNullable],
                            c.max_length AS [MaxLength], 
                            c.precision AS [Precision],
                            c.column_id as [Order], 
                            c.scale AS [Scale],
                            sco.text As [DefaultValue], 
                            ext.value AS [Comment],
                            c.is_identity AS [IsIdentity],
                            sty.is_user_defined AS [IsUserDefined],
                            schema_name(sty.schema_id) AS [TypeOwner],                           
                            cc.definition as [ComputeExp]
                        FROM sys.columns c 
                        JOIN sys.systypes st ON c.user_type_id = st.xusertype
                        JOIN sys.tables t ON c.object_id=t.object_id
                        LEFT JOIN sys.syscomments sco ON c.default_object_id=sco.id
                        LEFT JOIN sys.extended_properties ext on c.column_id=ext.minor_id AND c.object_id=ext.major_id AND ext.class_desc='OBJECT_OR_COLUMN' AND ext.name='MS_Description'
						LEFT JOIN sys.types sty on c.user_type_id = sty.user_type_id
                        LEFT JOIN sys.computed_columns cc on cc.object_id=c.object_id AND c.column_id= cc.column_id";
            }

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" WHERE t.name IN ({ strTableNames })";
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

            string commentColumn = isSimpleMode ? "" : ",ext.value AS [Comment]";
            string commentJoin = isSimpleMode ? "" : "LEFT JOIN sys.extended_properties ext ON object_id(i.name, 'PK')=ext.major_id  AND ext.class_desc='OBJECT_OR_COLUMN' AND ext.name='MS_Description'";

            string sql = $@"SELECT schema_name(t.schema_id) AS [Owner], t.name AS [TableName],i.name AS [Name], 
                           c.name AS [ColumnName], ic.key_ordinal AS [Order],ic.is_descending_key AS [IsDesc],
                           CASE i.type WHEN 1 THEN 1 ELSE 0 END AS [Clustered]{commentColumn}
                         FROM sys.index_columns ic
                         JOIN sys.columns c ON ic.object_id=c.object_id AND ic.column_id=c.column_id						
                         JOIN sys.indexes i ON ic.object_id=i.object_id AND ic.index_id=i.index_id
                         JOIN sys.tables t ON c.object_id=t.object_id
                         {commentJoin}
                         WHERE i.is_primary_key=1";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND t.name IN ({ strTableNames })";
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
            string commentColumn = isSimpleMode ? "" : ",ext.value AS [Comment]";
            string commentJoin = isSimpleMode ? "" : "LEFT JOIN sys.extended_properties ext ON object_id(fk.name, 'F')=ext.major_id  AND ext.class_desc='OBJECT_OR_COLUMN' AND ext.name='MS_Description'";

            string sql = $@"SELECT schema_name(t.schema_id) AS [Owner],object_name(fk.parent_object_id) AS TableName,fk.name AS [Name],c.name AS [ColumnName],
                         object_name(fck.referenced_object_id) AS [ReferencedTableName],rc.name AS [ReferencedColumnName],
                         fk.update_referential_action AS [UpdateCascade],fk.delete_referential_action AS [DeleteCascade]{commentColumn}
                         FROM sys.foreign_keys fk
                         JOIN sys.foreign_key_columns fck ON fk.object_id=fck.constraint_object_id
                         JOIN sys.columns c ON fk.parent_object_id=c.object_id AND  fck.parent_column_id=c.column_id
                         JOIN sys.columns rc ON fck.referenced_object_id= rc.object_id AND rc.column_id=fck.referenced_column_id
                         JOIN sys.tables t ON c.object_id=t.object_id
                         JOIN sys.tables rt ON rc.object_id=rt.object_id
                         {commentJoin}
                         WHERE 1=1";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND object_name(fk.parent_object_id) IN ({ strTableNames })";
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
            string commentColumn = isSimpleMode ? "" : ("," + (includePrimaryKey ? "ISNULL(ext.value,ext2.value)" : "ext.value")) + " AS [Comment]";
            string commentJoin = isSimpleMode ? "" : "LEFT JOIN sys.extended_properties ext on i.object_id=ext.major_id AND i.index_id= ext.minor_id AND ext.class_desc='INDEX' AND ext.name='MS_Description'";

            if (!isSimpleMode && includePrimaryKey)
            {
                commentJoin += Environment.NewLine + " LEFT JOIN sys.extended_properties ext2 on object_id(i.name, 'PK')=ext2.major_id  AND ext2.class_desc='OBJECT_OR_COLUMN' AND ext2.name='MS_Description'";
            }

            string sql = $@"SELECT schema_name(t.schema_id) AS [Owner],object_name(ic.object_id) AS TableName,i.name AS [Name], 
                          i.is_primary_key AS [IsPrimary], i.is_unique AS [IsUnique], c.name AS [ColumnName], ic.key_ordinal AS [Order],ic.is_descending_key AS [IsDesc],
                          CASE i.type WHEN 1 THEN 1 ELSE 0 END AS [Clustered]{commentColumn},
                          CASE WHEN i.is_primary_key=1 THEN 'Primary' WHEN i.is_unique=1 THEN 'Unique' WHEN i.type=6 THEN 'ColumnStore' ELSE 'Normal' END AS [Type]
                        FROM sys.index_columns ic
                        JOIN sys.columns c ON ic.object_id=c.object_id AND ic.column_id=c.column_id
                        JOIN sys.indexes i ON ic.object_id=i.object_id AND ic.index_id=i.index_id
                        JOIN sys.tables t ON c.object_id=t.object_id
                        {commentJoin}
                        WHERE {(includePrimaryKey ? "" : "i.is_primary_key=0 AND ")} i.type_desc<>'XML' AND (i.type= 6 OR (i.type <> 6 AND ic.key_ordinal > 0)) ";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND t.name IN ({ strTableNames })";
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

            string sql = $@"SELECT t.name AS [Name], OBJECT_SCHEMA_NAME(t.object_id) AS [Owner],object_name(t.parent_id) AS [TableName], 
                            {(isSimpleMode ? "''" : "OBJECT_DEFINITION(t.object_id)")} AS [Definition]
                            FROM sys.triggers t
                            WHERE t.parent_id >0";

            if (filter != null)
            {
                if (filter.TableNames != null && filter.TableNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                    sql += $" AND object_name(t.parent_id) IN ({ strNames })";
                }

                if (filter.TableTriggerNames != null && filter.TableTriggerNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter.TableTriggerNames);
                    sql += $" AND t.name IN ({ strNames })";
                }
            }

            sql += " ORDER BY t.name";

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
            string commentColumn = isSimpleMode ? "" : ",ext.value AS [Comment]";
            string commentJoin = isSimpleMode ? "" : "LEFT JOIN sys.extended_properties ext ON object_id(chk.name, 'C')=ext.major_id  AND ext.class_desc='OBJECT_OR_COLUMN' AND ext.name='MS_Description'";

            string sql = $@"select schema_name(t.schema_id) as [Owner], t.name as [TableName], col.name as [ColumnName], chk.name as [Name], 
                         chk.definition as [Definition] {commentColumn}
                         from sys.check_constraints chk
                         inner join sys.columns col on chk.parent_object_id = col.object_id and col.column_id = chk.parent_column_id
                         inner join sys.tables t on chk.parent_object_id = t.object_id
                         {commentJoin}
                         WHERE 1=1";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND t.name IN ({ strTableNames })";
            }

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

            string sql = $@"SELECT v.name AS [Name], schema_name(v.schema_id) AS [Owner], {(isSimpleMode ? "''" : "OBJECT_DEFINITION(object_id)")} AS [Definition]
                            FROM sys.views v
                            WHERE 1=1";

            if (filter != null && filter.ViewNames != null && filter.ViewNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.ViewNames);
                sql += $" AND v.name IN ({ strNames })";
            }

            sql += " ORDER BY v.name";

            return sql;
        }
        #endregion       

        #region Procedure       

        public override Task<List<Procedure>> GetProceduresAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Procedure>(this.GetSqlForProcedures(filter));
        }

        public override Task<List<Procedure>> GetProceduresAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Procedure>(dbConnection, this.GetSqlForProcedures(filter));
        }

        private string GetSqlForProcedures(SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = $@"SELECT name AS [Name], SCHEMA_NAME(schema_id) AS [Owner], 
                            {(isSimpleMode ? "''" : "OBJECT_DEFINITION(object_id)")} AS [Definition]
                            FROM sys.procedures
                            WHERE name not like 'sp[_]%'";

            if (filter != null && filter.ProcedureNames != null && filter.ProcedureNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.ProcedureNames);
                sql += $" AND name IN ({ strNames })";
            }

            sql += " ORDER BY name";

            return sql;
        }
        #endregion

        #region Table Constraint
        public Task<List<TableDefaultValueConstraint>> GetTableDefautValueConstraintsAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableDefaultValueConstraint>(this.GetSqlForTableDefaultValueConstraints(filter));
        }

        public Task<List<TableDefaultValueConstraint>> GetTableDefautValueConstraintsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableDefaultValueConstraint>(dbConnection, this.GetSqlForTableDefaultValueConstraints(filter));
        }

        private string GetSqlForTableDefaultValueConstraints(SchemaInfoFilter filter = null)
        {
            string sql = $@"select schema_name(t.schema_id) as [Owner], t.name as [TableName], col.name as [ColumnName], c.name as [Name]
                        from sys.default_constraints c
                        inner join sys.columns col on c.parent_object_id = col.object_id and col.column_id = c.parent_column_id
                        inner join sys.tables t on c.parent_object_id = t.object_id";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND t.name IN ({ strTableNames })";
            }

            return sql;
        }
        #endregion
        #endregion

        #region Database Operation

        public override Task<long> GetTableRecordCountAsync(DbConnection connection, Table table, string whereClause = "")
        {
            string sql = $"SELECT COUNT(1) FROM {this.GetQuotedObjectName(table)}";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += whereClause;
            }

            return base.GetTableRecordCountAsync(connection, sql);
        }

        public override async Task SetIdentityEnabled(DbConnection dbConnection, TableColumn column, bool enabled)
        {
            await this.ExecuteNonQueryAsync(dbConnection, $"SET IDENTITY_INSERT { this.GetQuotedObjectName(new Table() { Name = column.TableName, Owner = column.Owner })} {(enabled ? "OFF" : "ON")}", false);
        }

        public override Task SetConstrainsEnabled(bool enabled)
        {      
            return this.ExecuteNonQueryAsync(this.GetSqlForSetConstrainsEnabled(enabled));
        }

        public override Task SetConstrainsEnabled(DbConnection dbConnection, bool enabled)
        {
            return this.ExecuteNonQueryAsync(dbConnection, this.GetSqlForSetConstrainsEnabled(enabled), false);
        }      

        private string GetSqlForSetConstrainsEnabled(bool enabled)
        {
            string procName = "sp_MSForEachTable";

            string sql = 
$@"
IF ServerProperty('Edition') != '{this.azureSQLFlag}'
BEGIN
  EXEC {procName} 'ALTER TABLE ? {(enabled ? "CHECK" : "NOCHECK")} CONSTRAINT ALL';
  EXEC {procName} 'ALTER TABLE ? {(enabled ? "ENABLE" : "DISABLE")} TRIGGER ALL';
END
ELSE 
BEGIN
    DECLARE @owner NVARCHAR(50)
	DECLARE @tableName NVARCHAR(256)

	DECLARE table_cursor CURSOR  
    FOR SELECT SCHEMA_NAME(schema_id),name FROM sys.tables  
	OPEN table_cursor  

    FETCH NEXT FROM table_cursor INTO @owner,@tableName
  
    WHILE @@FETCH_STATUS = 0  
    BEGIN  
        EXEC('ALTER TABLE ['+ @owner + '].[' + @tableName +'] {(enabled ? "CHECK" : "NOCHECK")} CONSTRAINT ALL');
        EXEC('ALTER TABLE ['+ @owner + '].[' + @tableName +'] {(enabled ? "ENABLE" : "DISABLE")} TRIGGER ALL');

        FETCH NEXT FROM table_cursor INTO @owner,@tableName  
    END  
  
    CLOSE table_cursor  
    DEALLOCATE table_cursor   
END";           

            return sql;
        }

        private async Task<bool> IsProcedureExisted(DbConnection dbConnection, string procedureName)
        {
            object result = await this.GetScalarAsync(dbConnection, $"SELECT name FROM master.dbo.sysobjects WHERE name = '{procedureName}' AND type='P'");

            return result != null && result.ToString().ToLower() == procedureName.ToLower();
        }
        #endregion

        #region BulkCopy
        public override async Task BulkCopyAsync(DbConnection connection, DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            SqlBulkCopy bulkCopy = await this.GetBulkCopy(connection, bulkCopyInfo);
            {
                await bulkCopy.WriteToServerAsync(this.ConvertDataTable(dataTable, bulkCopyInfo), bulkCopyInfo.CancellationToken);
            }
        }

        private DataTable ConvertDataTable(DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            var columns = dataTable.Columns.Cast<DataColumn>();

            if (!columns.Any(item => item.DataType == typeof(TimeSpan)
                   || item.DataType == typeof(byte[])
                   || item.DataType == typeof(decimal)))
            {
                return dataTable;
            }

            Dictionary<int, Type> changedColumnTypes = new Dictionary<int, Type>();
            Dictionary<(int RowIndex, int ColumnIndex), object> changedValues = new Dictionary<(int RowIndex, int ColumnIndex), object>();

            DataTable dtChanged = dataTable.Clone();

            int rowIndex = 0;

            Func<DataColumn, TableColumn> getTableColumn = (column) =>
            {
                return bulkCopyInfo.Columns.FirstOrDefault(item => item.Name == column.ColumnName);
            };

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
                            if (type == typeof(TimeSpan))
                            {
                                TimeSpan ts = TimeSpan.Parse(value.ToString());

                                if (ts.Days > 0)
                                {
                                    TableColumn tableColumn = getTableColumn(dataTable.Columns[i]);

                                    string dataType = tableColumn.DataType.ToLower();

                                    Type columnType = null;

                                    if (dataType.Contains("datetime"))
                                    {
                                        DateTime dateTime = this.MinDateTime.AddSeconds(ts.TotalSeconds);

                                        columnType = typeof(DateTime);

                                        changedValues.Add((rowIndex, i), dateTime);
                                    }
                                    else if (DataTypeHelper.IsCharType(dataType))
                                    {
                                        columnType = typeof(string);

                                        changedValues.Add((rowIndex, i), ts.ToString());
                                    }

                                    if (columnType != null && !changedColumnTypes.ContainsKey(i))
                                    {
                                        changedColumnTypes.Add(i, columnType);
                                    }
                                }
                            }
                            else if (type == typeof(byte[]))
                            {
                                TableColumn tableColumn = getTableColumn(dataTable.Columns[i]);

                                if (tableColumn.DataType.ToLower() == "uniqueidentifier")
                                {
                                    changedValues.Add((rowIndex, i), ValueHelper.ConvertGuidBytesToString(value as byte[], this.DatabaseType, tableColumn.DataType, tableColumn.MaxLength, true));

                                    if (!changedColumnTypes.ContainsKey(i))
                                    {
                                        changedColumnTypes.Add(i, typeof(Guid));
                                    }
                                }
                            }
                            else if (type == typeof(decimal))
                            {
                                TableColumn tableColumn = getTableColumn(dataTable.Columns[i]);

                                string dataType = tableColumn.DataType.ToLower();

                                Type columnType = null;

                                if (dataType == "bigint")
                                {
                                    columnType = typeof(Int64);
                                }
                                else if (dataType == "int")
                                {
                                    columnType = typeof(Int32);

                                    if ((decimal)value > Int32.MaxValue)
                                    {
                                        columnType = typeof(Int64);
                                    }
                                }
                                else if (dataType == "smallint")
                                {
                                    columnType = typeof(Int16);

                                    if ((decimal)value > Int16.MaxValue)
                                    {
                                        columnType = typeof(Int32);
                                    }
                                }

                                if (columnType != null && !changedColumnTypes.ContainsKey(i))
                                {
                                    changedColumnTypes.Add(i, columnType);
                                }
                            }
                        }
                    }
                }

                rowIndex++;
            }

            if (changedColumnTypes.Count == 0)
            {
                return dataTable;
            }

            for (int i = 0; i < dtChanged.Columns.Count; i++)
            {
                if (changedColumnTypes.ContainsKey(i))
                {
                    dtChanged.Columns[i].DataType = changedColumnTypes[i];
                }
            }

            rowIndex = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                DataRow r = dtChanged.NewRow();

                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    var value = row[i];

                    if (changedValues.ContainsKey((rowIndex, i)))
                    {
                        r[i] = changedValues[(rowIndex, i)];
                    }
                    else
                    {
                        r[i] = value;
                    }
                }

                dtChanged.Rows.Add(r);

                rowIndex++;
            }

            return dtChanged;
        }

        private async Task<SqlBulkCopy> GetBulkCopy(DbConnection connection, BulkCopyInfo bulkCopyInfo)
        {
            SqlBulkCopy bulkCopy = new SqlBulkCopy(connection as SqlConnection, SqlBulkCopyOptions.Default, bulkCopyInfo.Transaction as SqlTransaction);

            await this.OpenConnectionAsync(connection);

            bulkCopy.DestinationTableName = this.GetQuotedString(bulkCopyInfo.DestinationTableName);
            bulkCopy.BulkCopyTimeout = bulkCopyInfo.Timeout.HasValue ? bulkCopyInfo.Timeout.Value : SettingManager.Setting.CommandTimeout;
            bulkCopy.BatchSize = bulkCopyInfo.BatchSize.HasValue ? bulkCopyInfo.BatchSize.Value : this.DataBatchSize;

            return bulkCopy;
        }
        #endregion

        #region Parse Column & DataType
        public override string ParseColumn(Table table, TableColumn column)
        {
            if (column.IsUserDefined)
            {
                return $"{this.GetQuotedString(column.Name)} {this.GetQuotedString(column.TypeOwner)}.{this.GetQuotedString(column.DataType)} {(column.IsRequired ? "NOT NULL" : "NULL")}";
            }

            bool isComputed = column.IsComputed;

            if (isComputed)
            {
                return $"{this.GetQuotedString(column.Name)} AS {this.GetColumnComputeExpression(column)}";
            }
            else
            {
                string dataType = this.ParseDataType(column);

                string identityClause = (this.Option.TableScriptsGenerateOption.GenerateIdentity && column.IsIdentity ? $"IDENTITY({table.IdentitySeed},{table.IdentityIncrement})" : "");
                string requireClause = (column.IsRequired ? "NOT NULL" : "NULL");
                //string defaultValueClause = this.Option.TableScriptsGenerateOption.GenerateDefaultValue && !string.IsNullOrEmpty(column.DefaultValue) ? (" DEFAULT " + this.GetColumnDefaultValue(column)) : "";
                string scriptComment = string.IsNullOrEmpty(column.ScriptComment) ? "" : $"/*{column.ScriptComment}*/";

                return $"{this.GetQuotedString(column.Name)} {dataType} {identityClause} {requireClause}{scriptComment}";
            }
        }

        public override string ParseDataType(TableColumn column)
        {
            string dataLength = this.GetColumnDataLength(column);

            if (!string.IsNullOrEmpty(dataLength))
            {
                dataLength = $"({dataLength})";
            }

            string dataType = $"{column.DataType} {dataLength}";

            return dataType.Trim();
        }

        public override string GetColumnDataLength(TableColumn column)
        {
            string dataType = column.DataType;
            DataTypeInfo dataTypeInfo = DataTypeHelper.GetDataTypeInfo(this, dataType);

            DataTypeSpecification dataTypeSpec = this.GetDataTypeSpecification(dataTypeInfo.DataType);

            if (dataTypeSpec != null)
            {
                string args = dataTypeSpec.Args.ToLower().Trim();

                if (string.IsNullOrEmpty(args))
                {
                    return string.Empty;
                }
                else if (DataTypeHelper.IsCharType(dataType) && DataTypeHelper.StartWithN(dataType)) //ie. nchar, nvarchar
                {
                    if (column.MaxLength == -1 || column.MaxLength == null)
                    {
                        return "max";
                    }

                    return ((column.MaxLength ?? 0) / 2).ToString();
                }
                else if (DataTypeHelper.IsCharType(dataType) || DataTypeHelper.IsBinaryType(dataType))//ie. char, varchar, binary, varbinary
                {
                    if (column.MaxLength == -1 || column.MaxLength == null)
                    {
                        return "max";
                    }

                    return column.MaxLength?.ToString();
                }
                else if (args.ToLower().Contains(","))//ie. numeric,decimal
                {
                    int scale = column.Scale == null ? 0 : column.Scale.Value;

                    return $"{column.Precision},{scale}";
                }
                else if (args == "precision" || args == "scale") //ie. datetime2,datetimeoffset
                {
                    return column.Scale == null ? "0" : column.Scale.ToString();
                }
                else if (args == "length")
                {
                    return column.MaxLength == null ? "0" : column.MaxLength.ToString();
                }
            }

            return string.Empty;
        }
        #endregion

        #region Sql Query Clause
        protected override string GetSqlForPagination(string tableName, string columnNames, string orderColumns, string whereClause, long pageNumber, int pageSize)
        {
            var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

            string orderClause = string.IsNullOrEmpty(orderColumns) ? this.GetDefaultOrder() : orderColumns;

            string pagedSql = $@"with PagedRecords as
								(
									SELECT TOP 100 PERCENT {columnNames}, ROW_NUMBER() OVER (ORDER BY {orderClause}) AS {RowNumberColumnName}
									FROM {tableName}
                                    {whereClause}
								)
								SELECT *
								FROM PagedRecords
								WHERE {RowNumberColumnName} BETWEEN {startEndRowNumber.StartRowNumber} AND {startEndRowNumber.EndRowNumber}";

            return pagedSql;
        }

        public override string GetDefaultOrder()
        {
            return "(SELECT 0)";
        }

        public override string GetLimitStatement(int limitStart, int limitCount)
        {
            return $"OFFSET {limitStart} ROWS FETCH NEXT {limitCount} ROWS ONLY";
        }
        #endregion      

        #region InfoMessage
        protected override void SubscribeInfoMessage(DbConnection dbConnection)
        {
            SqlConnection connection = dbConnection as SqlConnection;
            connection.InfoMessage += SqlConnection_InfoMessage;
        }

        private void SqlConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            this.FeedbackInfo(e.Message);
        }

        protected override void SubscribeInfoMessage(DbCommand dbCommand)
        {
            //SqlCommand command = dbCommand as SqlCommand;
            //command.StatementCompleted += Command_StatementCompleted;
        }

        private void Command_StatementCompleted(object sender, StatementCompletedEventArgs e)
        {
            this.FeedbackInfo($"{e.RecordCount} row(s) affected.");
        }
        #endregion      
    }
}
