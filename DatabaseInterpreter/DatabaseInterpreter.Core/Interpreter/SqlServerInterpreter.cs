using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class SqlServerInterpreter : DbInterpreter
    {
        #region Field & Property
        public const string AzureSQLFlag = "SQL Azure";
        public override string CommandParameterChar => "@"; 
        public const char QuotedLeftChar = '[';
        public const char QuotedRightChar = ']';
        public override char QuotationLeftChar { get { return QuotedLeftChar; } }
        public override char QuotationRightChar { get { return QuotedRightChar; } }
        public override DatabaseType DatabaseType => DatabaseType.SqlServer;
        public override string DefaultDataType => "varchar";
        public override string DefaultSchema => "dbo";
        public override IndexType IndexType => IndexType.Primary | IndexType.Normal | IndexType.Unique | IndexType.ColumnStore;
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

        public override bool IsLowDbVersion(string version)
        {
            return this.IsLowDbVersion(version, 9);
        }
        #endregion

        #region Schema Information
        #region Database
        public override Task<List<Database>> GetDatabasesAsync()
        {
            string sql = $@"SELECT name AS [Name] FROM sys.databases {this.GetExcludeBuiltinDbNamesCondition("name")} ORDER BY name";

            return base.GetDbObjectsAsync<Database>(sql);
        }
        #endregion

        #region Database Schema
        public override Task<List<DatabaseSchema>> GetDatabaseSchemasAsync()
        {
            string sql = @"select name as [Name], name as [Schema]  from sys.schemas
                           where name not in ('guest', 'sys', 'INFORMATION_SCHEMA') and name not like 'db[_]%'";

            return base.GetDbObjectsAsync<DatabaseSchema>(sql);
        }
        #endregion

        #region User Defined Type 

        public override Task<List<UserDefinedTypeItem>> GetUserDefinedTypeItemsAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<UserDefinedTypeItem>(this.GetSqlForUserDefinedTypes(filter));
        }
        public override Task<List<UserDefinedTypeItem>> GetUserDefinedTypeItemsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<UserDefinedTypeItem>(dbConnection, this.GetSqlForUserDefinedTypes(filter));
        }

        private string GetSqlForUserDefinedTypes(SchemaInfoFilter filter = null)
        {
            var sb = this.CreateSqlBuilder();

            sb.Append(@"SELECT schema_name(T.schema_id) AS [Schema],T.name as [Name], T.name as [AttrName], ST.name AS [DataType], T.max_length AS [MaxLength], T.precision AS [Precision],T.scale AS [Scale],T.is_nullable AS IsNullable
                            FROM sys.types T JOIN sys.systypes ST ON T.system_type_id=ST.xusertype
                            WHERE is_user_defined=1");

            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(T.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.UserDefinedTypeNames, "T.name"));           

            sb.Append("ORDER BY T.name");

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

            sb.Append(@"select schema_name(s.schema_id) as [Schema],s.name as [Name],t.name as [DataType],
                            start_value as [StartValue],increment as [Increment],minimum_value as [MinValue],maximum_value as [MaxValue],
                            is_cycling as [Cycled],is_cached as [UseCache],cache_size as [CacheSize]
                            from sys.sequences s
                            join sys.types t on s.system_type_id = t.system_type_id and s.schema_id = t.schema_id");

            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(s.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.SequenceNames, "s.name"));          

            sb.Append("ORDER BY s.name");

            return sb.Content;
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
            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT o.name AS [Name], schema_name(o.schema_id) AS [Schema], 
                           {(isSimpleMode ? "''" : "OBJECT_DEFINITION(o.object_id)")} AS [Definition]
                           FROM sys.all_objects o 
                           WHERE o.type IN ('FN', 'IF', 'AF', 'FS', 'FT','TF')
                           AND SCHEMA_NAME(schema_id) NOT IN('sys')");

            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(o.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.FunctionNames, "o.name"));            

            sb.Append("ORDER BY o.name");

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
            var sb = this.CreateSqlBuilder();

            if (this.IsObjectFectchSimpleMode())
            {
                sb.Append($@"SELECT schema_name(t.schema_id) AS [Schema], t.name AS [Name],
                         IDENT_SEED(schema_name(t.schema_id)+'.'+t.name) AS [IdentitySeed],IDENT_INCR(schema_name(t.schema_id)+'.'+t.name) AS [IdentityIncrement]
                         FROM sys.tables t
                         WHERE 1=1");
            }
            else
            {
                sb.Append($@"SELECT schema_name(t.schema_id) AS [Schema], t.name AS [Name], ext2.value AS [Comment],
                        IDENT_SEED(schema_name(t.schema_id)+'.'+t.name) AS [IdentitySeed],IDENT_INCR(schema_name(t.schema_id)+'.'+t.name) AS [IdentityIncrement]
                        FROM sys.tables t
                        LEFT JOIN sys.extended_properties ext ON t.object_id=ext.major_id AND ext.minor_id=0 AND ext.class=1 AND ext.name='microsoft_database_tools_support'
                        LEFT JOIN sys.extended_properties ext2 ON t.object_id=ext2.major_id and ext2.minor_id=0 AND ext2.class_desc='OBJECT_OR_COLUMN' AND ext2.name='MS_Description'
                        WHERE t.is_ms_shipped=0 AND ext.class is null");
            }

            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(t.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "t.name"));         
            sb.Append("ORDER BY t.name");

            return sb.Content;
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

            var sb = this.CreateSqlBuilder();

            if (this.IsObjectFectchSimpleMode())
            {
                sb.Append(@"SELECT schema_name(t.schema_id) AS [Schema], 
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
                        JOIN sys.tables t ON c.object_id=t.object_id");
            }
            else
            {
                sb.Append(@"SELECT schema_name(t.schema_id) AS [Schema], 
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
                            schema_name(sty.schema_id) AS [DataTypeSchema],                           
                            cc.definition as [ComputeExp]
                        FROM sys.columns c 
                        JOIN sys.systypes st ON c.user_type_id = st.xusertype
                        JOIN sys.tables t ON c.object_id=t.object_id
                        LEFT JOIN sys.syscomments sco ON c.default_object_id=sco.id
                        LEFT JOIN sys.extended_properties ext on c.column_id=ext.minor_id AND c.object_id=ext.major_id AND ext.class_desc='OBJECT_OR_COLUMN' AND ext.name='MS_Description'
						LEFT JOIN sys.types sty on c.user_type_id = sty.user_type_id
                        LEFT JOIN sys.computed_columns cc on cc.object_id=c.object_id AND c.column_id= cc.column_id");
            }

            sb.Append("WHERE 1=1");
            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(t.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "t.name"));           

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
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string commentColumn = isSimpleMode ? "" : ",ext.value AS [Comment]";
            string commentJoin = isSimpleMode ? "" : "LEFT JOIN sys.extended_properties ext ON object_id(i.name, 'PK')=ext.major_id  AND ext.class_desc='OBJECT_OR_COLUMN' AND ext.name='MS_Description'";

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT schema_name(t.schema_id) AS [Schema], t.name AS [TableName],i.name AS [Name], 
                           c.name AS [ColumnName], ic.key_ordinal AS [Order],ic.is_descending_key AS [IsDesc],
                           CASE i.type WHEN 1 THEN 1 ELSE 0 END AS [Clustered]{commentColumn}
                         FROM sys.index_columns ic
                         JOIN sys.columns c ON ic.object_id=c.object_id AND ic.column_id=c.column_id						
                         JOIN sys.indexes i ON ic.object_id=i.object_id AND ic.index_id=i.index_id
                         JOIN sys.tables t ON c.object_id=t.object_id
                         {commentJoin}
                         WHERE i.is_primary_key=1");

            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(t.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "t.name"));           

            return sb.Content;
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

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT schema_name(t.schema_id) AS [Schema],object_name(fk.parent_object_id) AS [TableName],fk.name AS [Name],c.name AS [ColumnName],
                         schema_name(rt.schema_id) AS [ReferencedSchema], object_name(fck.referenced_object_id) AS [ReferencedTableName],rc.name AS [ReferencedColumnName],
                         fk.update_referential_action AS [UpdateCascade],fk.delete_referential_action AS [DeleteCascade]{commentColumn}
                         FROM sys.foreign_keys fk
                         JOIN sys.foreign_key_columns fck ON fk.object_id=fck.constraint_object_id
                         JOIN sys.columns c ON fk.parent_object_id=c.object_id AND  fck.parent_column_id=c.column_id
                         JOIN sys.columns rc ON fck.referenced_object_id= rc.object_id AND rc.column_id=fck.referenced_column_id
                         JOIN sys.tables t ON c.object_id=t.object_id
                         JOIN sys.tables rt ON rc.object_id=rt.object_id
                         {commentJoin}
                         WHERE 1=1");

            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(t.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "object_name(fk.parent_object_id)"));           

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
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            string commentColumn = isSimpleMode ? "" : ("," + (includePrimaryKey ? "ISNULL(ext.value,ext2.value)" : "ext.value")) + " AS [Comment]";
            string commentJoin = isSimpleMode ? "" : "LEFT JOIN sys.extended_properties ext on i.object_id=ext.major_id AND i.index_id= ext.minor_id AND ext.class_desc='INDEX' AND ext.name='MS_Description'";

            if (!isSimpleMode && includePrimaryKey)
            {
                commentJoin += Environment.NewLine + "LEFT JOIN sys.extended_properties ext2 on object_id(i.name, 'PK')=ext2.major_id  AND ext2.class_desc='OBJECT_OR_COLUMN' AND ext2.name='MS_Description'";
            }

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT schema_name(t.schema_id) AS [Schema],object_name(ic.object_id) AS TableName,i.name AS [Name], 
                          i.is_primary_key AS [IsPrimary], i.is_unique AS [IsUnique], c.name AS [ColumnName], ic.key_ordinal AS [Order],ic.is_descending_key AS [IsDesc],
                          CASE i.type WHEN 1 THEN 1 ELSE 0 END AS [Clustered]{commentColumn},
                          CASE WHEN i.is_primary_key=1 THEN 'Primary' WHEN i.is_unique=1 THEN 'Unique' WHEN i.type=6 THEN 'ColumnStore' ELSE 'Normal' END AS [Type]
                        FROM sys.index_columns ic
                        JOIN sys.columns c ON ic.object_id=c.object_id AND ic.column_id=c.column_id
                        JOIN sys.indexes i ON ic.object_id=i.object_id AND ic.index_id=i.index_id
                        JOIN sys.tables t ON c.object_id=t.object_id
                        {commentJoin}
                        WHERE {(includePrimaryKey ? "" : "i.is_primary_key=0 AND ")} i.type_desc<>'XML' AND (i.type= 6 OR (i.type <> 6 AND ic.key_ordinal > 0))");

            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(t.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "t.name"));           

            return sb.Content;
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

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT t.name AS [Name], object_schema_name(t.object_id) AS [Schema],object_name(t.parent_id) AS [TableName], 
                            {(isSimpleMode ? "''" : "object_definition(t.object_id)")} AS [Definition]
                            FROM sys.triggers t
                            WHERE t.parent_id >0");

            sb.Append(this.GetFilterSchemaCondition(filter, "object_schema_name(t.object_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "object_name(t.parent_id)"));

            if (filter != null)
            {
                if (filter.TableTriggerNames != null && filter.TableTriggerNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter.TableTriggerNames);
                    sb.Append($"AND t.name IN ({strNames})");
                }
            }

            sb.Append("ORDER BY t.name");

            return sb.Content;
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

            var sb = this.CreateSqlBuilder();

            sb.Append($@"select schema_name(t.schema_id) as [Schema], t.name as [TableName], col.name as [ColumnName], chk.name as [Name], 
                         chk.definition as [Definition] {commentColumn}
                         from sys.check_constraints chk
                         inner join sys.columns col on chk.parent_object_id = col.object_id and col.column_id = chk.parent_column_id
                         inner join sys.tables t on chk.parent_object_id = t.object_id
                         {commentJoin}
                         WHERE 1=1");

            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(t.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "t.name"));            

            return sb.Content;
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

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT v.name AS [Name], schema_name(v.schema_id) AS [Schema], {(isSimpleMode ? "''" : "OBJECT_DEFINITION(object_id)")} AS [Definition]
                            FROM sys.views v
                            WHERE 1=1");

            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(v.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.ViewNames, "v.name"));           

            sb.Append("ORDER BY v.name");

            return sb.Content;
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

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT name AS [Name], SCHEMA_NAME(schema_id) AS [Schema], 
                            {(isSimpleMode ? "''" : "OBJECT_DEFINITION(object_id)")} AS [Definition]
                            FROM sys.procedures
                            WHERE name not like 'sp[_]%'");

            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.ProcedureNames, "name"));           

            sb.Append("ORDER BY name");

            return sb.Content;
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
            var sb = this.CreateSqlBuilder();

            sb.Append($@"select schema_name(t.schema_id) as [Schema], t.name as [TableName], col.name as [ColumnName], c.name as [Name]
                        from sys.default_constraints c
                        inner join sys.columns col on c.parent_object_id = col.object_id and col.column_id = c.parent_column_id
                        inner join sys.tables t on c.parent_object_id = t.object_id");

            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(t.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "t.name"));            

            return sb.Content;
        }
        #endregion
        #endregion

        #region Database Operation

        public override Task<long> GetTableRecordCountAsync(DbConnection connection, Table table, string whereClause = "")
        {
            string sql = $"SELECT COUNT(1) FROM {this.GetQuotedDbObjectNameWithSchema(table)}";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += whereClause;
            }

            return base.GetTableRecordCountAsync(connection, sql);
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
                foreach(DataColumn column in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                await bulkCopy.WriteToServerAsync(this.ConvertDataTable(dataTable, bulkCopyInfo), bulkCopyInfo.CancellationToken);
            }
        }

        private DataTable ConvertDataTable(DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            var columns = dataTable.Columns.Cast<DataColumn>();

            if (!columns.Any(item => item.DataType == typeof(TimeSpan)
                   || item.DataType == typeof(byte[])
                   || item.DataType == typeof(decimal)
                   || item.DataType == typeof(String)
                   || item.DataType == typeof(System.Array)
                   || item.DataType == typeof(BitArray)
                   || item.DataType.Name.Contains("Geometry")
                   || item.DataType == typeof(NpgsqlTsVector)
                   || item.DataType == typeof(NpgsqlLine)
                   || item.DataType == typeof(NpgsqlBox)
                   || item.DataType == typeof(NpgsqlCircle)
                   || item.DataType == typeof(NpgsqlPath)
                   || item.DataType == typeof(NpgsqlLSeg)
                   )
                )
            {
                 return dataTable;
            }

            Dictionary<int, DataTableColumnChangeInfo> changedColumns = new Dictionary<int, DataTableColumnChangeInfo>();
            Dictionary<(int RowIndex, int ColumnIndex), dynamic> changedValues = new Dictionary<(int RowIndex, int ColumnIndex), dynamic>();
            
            int rowIndex = 0;

            Func<DataColumn, TableColumn> getTableColumn = (column) =>
            {
                return bulkCopyInfo.Columns.FirstOrDefault(item => item.Name == column.ColumnName);
            };

            foreach (DataRow row in dataTable.Rows)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    var value = row[i];

                    if (value != null)
                    {
                        Type type = value.GetType();

                        if (type != typeof(DBNull))
                        {
                            Type newColumnType = null;
                            int? newMaxLength = default(int?);
                            object newValue = null;

                            TableColumn tableColumn = getTableColumn(dataTable.Columns[i]);
                            string dataType = tableColumn.DataType.ToLower();

                            if (type == typeof(String))
                            {
                                if (dataType == "uniqueidentifier")
                                {
                                    newColumnType = typeof(Guid);
                                    newMaxLength = -1;
                                    newValue = new Guid(value.ToString());
                                }
                                else if(dataType == "geometry")
                                {
                                    newColumnType = typeof(SqlGeometry);
                                    newValue = SqlGeometry.STGeomFromText(new System.Data.SqlTypes.SqlChars(value as string), 0);
                                }
                            }
                            else if (type == typeof(TimeSpan))
                            {
                                TimeSpan ts = TimeSpan.Parse(value.ToString());

                                if (ts.Days > 0)
                                {
                                    if (dataType.Contains("datetime"))
                                    {
                                        DateTime dateTime = this.MinDateTime.AddSeconds(ts.TotalSeconds);

                                        newColumnType = typeof(DateTime);
                                        newValue = dateTime;
                                    }
                                    else if (DataTypeHelper.IsCharType(dataType))
                                    {
                                        newColumnType = typeof(string);
                                        newValue = ts.ToString();
                                    }
                                }
                            }
                            else if (type == typeof(byte[]))
                            {
                                if (dataType == "uniqueidentifier")
                                {
                                    newColumnType = typeof(Guid);
                                    newValue = ValueHelper.ConvertGuidBytesToString(value as byte[], this.DatabaseType, tableColumn.DataType, tableColumn.MaxLength, true);
                                }
                                else if (dataType == "geometry")
                                {
                                    newColumnType = typeof(SqlGeometry);
                                    newValue = GeometryHelper.MySqlGeometryBytesToSqlGeometry(value as byte[]);
                                }
                                else if (dataType == "geography")
                                {
                                    newColumnType = typeof(SqlGeography);

                                    SqlGeometry geometry = GeometryHelper.MySqlGeometryBytesToSqlGeometry(value as byte[]);

                                    newValue = SqlGeography.STGeomFromText(geometry.STAsText(), geometry.STSrid.Value);
                                }
                            }
                            else if(type== typeof(BitArray))
                            {
                                var bitArray = value as BitArray;
                                byte[] bytes = new byte[bitArray.Length];
                                bitArray.CopyTo(bytes, 0);

                                newColumnType = typeof(byte[]);
                                newValue = bytes;
                            }
                            else if (type == typeof(decimal))
                            {
                                if (dataType == "bigint")
                                {
                                    newColumnType = typeof(Int64);
                                }
                                else if (dataType == "int")
                                {
                                    newColumnType = typeof(Int32);

                                    if ((decimal)value > Int32.MaxValue)
                                    {
                                        newColumnType = typeof(Int64);
                                    }
                                }
                                else if (dataType == "smallint")
                                {
                                    newColumnType = typeof(Int16);

                                    if ((decimal)value > Int16.MaxValue)
                                    {
                                        newColumnType = typeof(Int32);
                                    }
                                }
                            }
                            else if (type.Name.EndsWith("[]")) //array type
                            {
                                if (DataTypeHelper.IsCharType(dataType))
                                {
                                    newColumnType = typeof(String);
                                    newValue = JsonConvert.SerializeObject(value);
                                }
                                else
                                {
                                    //
                                }
                            }
                            else if (value is NetTopologySuite.Geometries.Geometry)
                            {
                                if (dataType == "geography")
                                {
                                    newColumnType = typeof(SqlGeography);
                                    newValue = GeometryHelper.PostgresGeographyToSqlGeography(value as NetTopologySuite.Geometries.Geometry);
                                }
                                else
                                {
                                    newColumnType = typeof(SqlGeometry);
                                    newValue = GeometryHelper.PostgresGeometryToSqlGeometry(value as NetTopologySuite.Geometries.Geometry);
                                }
                            }
                            else if (value is NpgsqlTsVector || value is NpgsqlLine || value is NpgsqlBox || value is NpgsqlCircle
                                   ||value is NpgsqlPath || value is NpgsqlLSeg)
                            {
                                newColumnType = typeof(String);
                                newValue = value.ToString();
                            }

                            if (newColumnType != null && !changedColumns.ContainsKey(i))
                            {
                                changedColumns.Add(i, new DataTableColumnChangeInfo() { Type = newColumnType, MaxLength= newMaxLength });
                            }

                            if (newValue != null)
                            {
                                changedValues.Add((rowIndex, i), newValue);
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

        private async Task<SqlBulkCopy> GetBulkCopy(DbConnection connection, BulkCopyInfo bulkCopyInfo)
        {
            SqlBulkCopyOptions option = SqlBulkCopyOptions.Default;

            if (bulkCopyInfo.KeepIdentity)
            {
                option = SqlBulkCopyOptions.KeepIdentity;
            }

            SqlBulkCopy bulkCopy = new SqlBulkCopy(connection as SqlConnection, option, bulkCopyInfo.Transaction as SqlTransaction);

            await this.OpenConnectionAsync(connection);

            string tableName = this.GetQuotedDbObjectNameWithSchema(bulkCopyInfo.DestinationTableSchema, bulkCopyInfo.DestinationTableName);            

            bulkCopy.DestinationTableName = tableName;
            bulkCopy.BulkCopyTimeout = bulkCopyInfo.Timeout.HasValue ? bulkCopyInfo.Timeout.Value : Setting.CommandTimeout;
            bulkCopy.BatchSize = bulkCopyInfo.BatchSize.HasValue ? bulkCopyInfo.BatchSize.Value : this.DataBatchSize;

            return bulkCopy;
        }
        #endregion

        #region Parse Column & DataType
        public override string ParseColumn(Table table, TableColumn column)
        {
            if (column.IsUserDefined)
            {
                string dataType = string.IsNullOrEmpty(column.DataTypeSchema) ? this.GetQuotedString(column.DataType): $"{this.GetQuotedString(column.DataTypeSchema)}.{this.GetQuotedString(column.DataType)}";
                
                return $"{this.GetQuotedString(column.Name)} {dataType} {(column.IsRequired ? "NOT NULL" : "NULL")}";
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
            bool isChar = DataTypeHelper.IsCharType(dataType);
            bool isBinary = DataTypeHelper.IsBinaryType(dataType);

            DataTypeInfo dataTypeInfo = DataTypeHelper.GetDataTypeInfo(this, dataType);

            DataTypeSpecification dataTypeSpec = this.GetDataTypeSpecification(dataTypeInfo.DataType);

            if (dataTypeSpec != null)
            {
                string args = dataTypeSpec.Args.ToLower().Trim();

                if (string.IsNullOrEmpty(args))
                {
                    return string.Empty;
                }
                else if (isChar && DataTypeHelper.StartsWithN(dataType)) //ie. nchar, nvarchar
                {
                    if (column.MaxLength == -1 || column.MaxLength == null)
                    {
                        return "max";
                    }

                    return ((column.MaxLength ?? 0) / 2).ToString();
                }
                else if (isChar || isBinary)//ie. char, varchar, binary, varbinary
                {
                    if (column.MaxLength == -1 || column.MaxLength == null)
                    {
                        if (isChar || (dataType.ToLower() == "varbinary"))
                        {
                            return "max";
                        }
                        else
                        {
                            return dataTypeSpec.Range.Split('~')[1];
                        }
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
