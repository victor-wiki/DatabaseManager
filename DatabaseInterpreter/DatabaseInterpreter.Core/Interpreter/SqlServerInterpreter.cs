using DatabaseInterpreter.Geometry;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using NpgsqlTypes;
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
    public class SqlServerInterpreter : DbInterpreter
    {
        #region Field & Property
        public const string AzureSQLFlag = "SQL Azure";
        public override string CommandParameterChar => "@";
        public override bool SupportQuotationChar => true;
        public override char QuotationLeftChar => '[';
        public override char QuotationRightChar => ']';
        public override DatabaseType DatabaseType => DatabaseType.SqlServer;
        public override string DefaultDataType => "varchar";
        public override string DefaultSchema => "dbo";
        public override string STR_CONCAT_CHARS => "+";
        public override IndexType IndexType => IndexType.Primary | IndexType.Normal | IndexType.Unique | IndexType.ColumnStore;
        public override DatabaseObjectType SupportDbObjectType => DatabaseObjectType.Table | DatabaseObjectType.View | DatabaseObjectType.Function
                                                                 | DatabaseObjectType.Procedure | DatabaseObjectType.Type | DatabaseObjectType.Sequence;
        public override bool SupportBulkCopy => true;
        public override bool SupportNchar => true;
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
            return this.IsLowDbVersion(version, "9");
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
            return base.GetDbObjectsAsync<DatabaseSchema>(this.GetSqlForDatabaseSchemas());
        }

        public override Task<List<DatabaseSchema>> GetDatabaseSchemasAsync(DbConnection dbConnection)
        {
            return base.GetDbObjectsAsync<DatabaseSchema>(dbConnection, this.GetSqlForDatabaseSchemas());
        }

        private string GetSqlForDatabaseSchemas()
        {
            string sql = "select name as [Name], name as [Schema] from sys.schemas  where name not in ('guest', 'sys', 'INFORMATION_SCHEMA') and name not like 'db[_]%'";

            return sql;
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
            var sb = this.CreateSqlBuilder();

            sb.Append(@"SELECT schema_name(T.schema_id) AS [Schema],T.name as [TypeName], T.name as [Name], ST.name AS [DataType], T.max_length AS [MaxLength], T.precision AS [Precision],T.scale AS [Scale],T.is_nullable AS IsNullable
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
                            join sys.types t on s.system_type_id = t.system_type_id");

            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(s.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.SequenceNames, "s.name"));

            sb.Append("ORDER BY s.name");

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

            string condition = "WHERE t.is_ms_shipped=0";

            if (this.IsObjectFectchSimpleMode())
            {
                sb.Append($@"SELECT schema_name(t.schema_id) AS [Schema], t.name AS [Name],
                         IDENT_SEED(schema_name(t.schema_id)+'.'+t.name) AS [IdentitySeed],IDENT_INCR(schema_name(t.schema_id)+'.'+t.name) AS [IdentityIncrement]
                         FROM sys.tables t
                         {condition}");
            }
            else
            {
                sb.Append($@"SELECT schema_name(t.schema_id) AS [Schema], t.name AS [Name], ext2.value AS [Comment],
                        IDENT_SEED(schema_name(t.schema_id)+'.'+t.name) AS [IdentitySeed],IDENT_INCR(schema_name(t.schema_id)+'.'+t.name) AS [IdentityIncrement]
                        FROM sys.tables t
                        LEFT JOIN sys.extended_properties ext ON t.object_id=ext.major_id AND ext.minor_id=0 AND ext.class=1 AND ext.name='microsoft_database_tools_support'
                        LEFT JOIN sys.extended_properties ext2 ON t.object_id=ext2.major_id and ext2.minor_id=0 AND ext2.class_desc='OBJECT_OR_COLUMN' AND ext2.name='MS_Description'
                        {condition} AND ext.class is null");
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

            bool isForView = this.IsForViewColumnFilter(filter);

            string tableAlias = !isForView ? "t" : "v";
            string joinTable = !isForView ? "JOIN sys.tables t ON c.object_id=t.object_id" : "JOIN sys.views v ON c.object_id=v.object_id";

            var sb = this.CreateSqlBuilder();

            string simpleColumns = $@"schema_name({tableAlias}.schema_id) AS [Schema], 
                            {tableAlias}.name AS [TableName],
                            c.name AS [Name], 
                            st.name AS [DataType],
                            c.is_nullable AS [IsNullable],
                            c.max_length AS [MaxLength], 
                            c.precision AS [Precision],
                            c.column_id as [Order], 
                            c.scale AS [Scale],                           
                            c.is_identity AS [IsIdentity]";

            string detailsColumns = @"sco.text As [DefaultValue], 
                            ext.value AS [Comment],                           
                            sty.is_user_defined AS [IsUserDefined],
                            schema_name(sty.schema_id) AS [DataTypeSchema],                           
                            cc.definition as [ComputeExp]";

            if (this.IsObjectFectchSimpleMode())
            {
                sb.Append($@"SELECT {simpleColumns}                  
                            FROM sys.columns c 
                            JOIN sys.systypes st ON c.user_type_id = st.xusertype
                            {joinTable}");
            }
            else
            {
                sb.Append($@"SELECT {simpleColumns},
                           {detailsColumns}
                            FROM sys.columns c 
                            JOIN sys.systypes st ON c.user_type_id = st.xusertype
                            {joinTable}
                            LEFT JOIN sys.syscomments sco ON c.default_object_id=sco.id
                            LEFT JOIN sys.extended_properties ext on c.column_id=ext.minor_id AND c.object_id=ext.major_id AND ext.class_desc='OBJECT_OR_COLUMN' AND ext.name='MS_Description'
						    LEFT JOIN sys.types sty on c.user_type_id = sty.user_type_id
                            LEFT JOIN sys.computed_columns cc on cc.object_id=c.object_id AND c.column_id= cc.column_id");
            }

            sb.Append("WHERE 1=1");
            sb.Append(this.GetFilterSchemaCondition(filter, $"schema_name({tableAlias}.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, $"{tableAlias}.name"));

            sb.Append($"ORDER BY {tableAlias}.name, c.column_id");

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
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            string commentColumn = isSimpleMode ? "" : ",ext.value AS [Comment]";
            string commentJoin = isSimpleMode ? "" : "LEFT JOIN sys.extended_properties ext ON object_id(fk.name, 'F')=ext.major_id  AND ext.class_desc='OBJECT_OR_COLUMN' AND ext.name='MS_Description'";
            string tableAlias = !isFilterForReferenced ? "t" : "rt";

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT schema_name(t.schema_id) AS [Schema],object_name(t.object_id) AS [TableName],fk.name AS [Name],c.name AS [ColumnName],
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

            sb.Append(this.GetFilterSchemaCondition(filter, $"schema_name({tableAlias}.schema_id)"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, $"object_name({tableAlias}.object_id)"));

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

        private string GetSqlForTableIndexItems(SchemaInfoFilter filter = null, bool includePrimaryKey = false, bool isForView = false)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            string commentColumn = isSimpleMode ? "" : ("," + (includePrimaryKey ? "ISNULL(ext.value,ext2.value)" : "ext.value")) + " AS [Comment]";
            string commentJoin = isSimpleMode ? "" : "LEFT JOIN sys.extended_properties ext on i.object_id=ext.major_id AND i.index_id= ext.minor_id AND ext.class_desc='INDEX' AND ext.name='MS_Description'";
            string tableOrViewName = filter?.IsForView != true ? "tables":"views";

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
                          JOIN sys.{tableOrViewName} t ON c.object_id=t.object_id
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

        #region Function
        public override Task<List<Function>> GetFunctionsAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Function>(this.GetSqlForRoutines(DatabaseObjectType.Function, filter));
        }

        public override Task<List<Function>> GetFunctionsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Function>(dbConnection, this.GetSqlForRoutines(DatabaseObjectType.Function, filter));
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

        private string GetSqlForRoutines(DatabaseObjectType databaseObjectType, SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            bool isFunction = databaseObjectType == DatabaseObjectType.Function;
            string[] routineNames = isFunction ? filter?.FunctionNames : filter?.ProcedureNames;
            string routineType = isFunction ? "FUNCTION" : "PROCEDURE";

            string definition = isSimpleMode ? "" : ", OBJECT_DEFINITION(OBJECT_ID(r.ROUTINE_SCHEMA+'.'+r.ROUTINE_NAME)) AS [Definition]";

            var sb = this.CreateSqlBuilder();

            //can't use r.ROUTINE_DEFINITION directly, it may be incomplete.

            sb.Append($@"SELECT r.ROUTINE_SCHEMA AS [Schema],r.ROUTINE_NAME AS [Name],r.DATA_TYPE AS [DataType]{definition}
                        FROM INFORMATION_SCHEMA.ROUTINES r
                        WHERE ROUTINE_TYPE = '{routineType}'");

            sb.Append(this.GetFilterSchemaCondition(filter, "r.ROUTINE_SCHEMA"));
            sb.Append(this.GetFilterNamesCondition(filter, routineNames, "r.ROUTINE_NAME"));

            sb.Append("ORDER BY r.ROUTINE_NAME");

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

            var routineNames = isFunction ? filter?.FunctionNames : filter?.ProcedureNames;

            sb.Append(@"SELECT schema_name(schema_id) AS [Schema],o.name AS [RoutineName],		   
                        p.name AS [Name],type_name(p.user_type_id) AS [DataType],p.max_length AS [MaxLength],
                        p.precision AS [Precision],p.scale AS [Scale],p.parameter_id AS [Order],p.is_output AS [IsOutput]
                        FROM sys.objects o
                        INNER JOIN sys.parameters p ON o.object_id=p.object_id
                        WHERE isnull(p.name,'')<>'' AND o.type IN ('P','FN', 'IF', 'AF', 'FS', 'FT','TF') 
                        AND schema_name(schema_id) NOT IN('sys') AND o.name NOT IN('fn_diagramobjects')");

            sb.Append(this.GetFilterSchemaCondition(filter, "schema_name(schema_id)"));

            sb.Append(this.GetFilterNamesCondition(filter, routineNames, "o.name"));

            sb.Append("ORDER BY o.name, p.parameter_id");

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

            sb.Append(@"SELECT vt.VIEW_CATALOG AS [ObjectCatalog],vt.VIEW_SCHEMA AS [ObjectSchema],vt.[VIEW_NAME] AS [ObjectName],
                        vt.TABLE_CATALOG AS [RefObjectCatalog], vt.TABLE_SCHEMA AS[RefObjectSchema], vt.TABLE_NAME AS [RefObjectName]
                        FROM INFORMATION_SCHEMA.VIEW_TABLE_USAGE vt
                        WHERE 1=1");

            sb.Append(this.GetFilterSchemaCondition(filter, !isFilterForReferenced ? "vt.VIEW_SCHEMA" : "vt.TABLE_SCHEMA"));
            sb.Append(this.GetFilterNamesCondition(filter, !isFilterForReferenced ? filter?.ViewNames : filter?.TableNames, !isFilterForReferenced ? "vt.VIEW_NAME" : "vt.TABLE_NAME"));

            return sb.Content;
        }
        #endregion

        #region View->Column Usage
        public override Task<List<ViewColumnUsage>> GetViewColumnUsages(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectUsagesAsync<ViewColumnUsage>(this.GetSqlForViewColumnUsages(filter));
        }

        public override Task<List<ViewColumnUsage>> GetViewColumnUsages(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectUsagesAsync<ViewColumnUsage>(dbConnection, this.GetSqlForViewColumnUsages(filter));
        }

        private string GetSqlForViewColumnUsages(SchemaInfoFilter filter = null)
        {
            SqlBuilder sb = new SqlBuilder();

            sb.Append(@"SELECT vc.VIEW_CATALOG AS [ObjectCatalog],vc.VIEW_SCHEMA AS [ObjectSchema],vc.VIEW_NAME AS [ObjectName],
                        vc.TABLE_CATALOG AS [RefObjectCatalog], vc.TABLE_SCHEMA AS[RefObjectSchema], vc.TABLE_NAME AS [RefObjectName], vc.COLUMN_NAME AS [ColumnName]
                        FROM INFORMATION_SCHEMA.VIEW_COLUMN_USAGE vc
                        WHERE 1=1");

            sb.Append(this.GetFilterSchemaCondition(filter, "vc.VIEW_SCHEMA"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.ViewNames, "vc.VIEW_NAME"));

            return sb.Content;
        }
        #endregion

        #region Routine Script Usage
        public override Task<List<RoutineScriptUsage>> GetRoutineScriptUsages(SchemaInfoFilter filter = null, bool isFilterForReferenced = false, bool includeViewTableUsages = false)
        {
            return base.GetDbObjectUsagesAsync<RoutineScriptUsage>(this.GetSqlForRoutineScriptUsages(filter, isFilterForReferenced, includeViewTableUsages));
        }

        public override Task<List<RoutineScriptUsage>> GetRoutineScriptUsages(DbConnection dbConnection, SchemaInfoFilter filter = null, bool isFilterForReferenced = false, bool includeViewTableUsages = false)
        {
            return base.GetDbObjectUsagesAsync<RoutineScriptUsage>(dbConnection, this.GetSqlForRoutineScriptUsages(filter, isFilterForReferenced, includeViewTableUsages));
        }

        private string GetSqlForRoutineScriptUsages(SchemaInfoFilter filter = null, bool isFilterForReferenced = false, bool includeViewTableUsages = false)
        {
            SqlBuilder sb = new SqlBuilder();

            sb.Append(@"SELECT CASE o.type WHEN 'P' THEN 'Procedure' WHEN 'FN' THEN 'Function' WHEN 'TF' THEN 'Function' WHEN 'U' THEN 'Table' WHEN 'V' THEN 'View' END AS [ObjectType],
                        CASE ro.type WHEN 'P' THEN 'Procedure' WHEN 'FN' THEN 'Function' WHEN 'TF' THEN 'Function' WHEN 'U' THEN 'Table' WHEN 'V' THEN 'View' END AS [RefObjectType],
                        schema_name(o.schema_id) AS [ObjectSchema],o.name AS [ObjectName],
                        schema_name(ro.schema_id) AS [RefObjectSchema],referenced_entity_name AS [RefObjectName]
                        FROM sys.sql_expression_dependencies d
                        JOIN sys.objects o ON d.referencing_id = o.object_id
                        JOIN sys.objects ro ON d.referenced_id=ro.object_id
                        WHERE o.type IN ('P','FN','TF','U','V') AND ro.type IN ('P','FN','TF','U','V')");

            if (!includeViewTableUsages)
            {
                sb.Append("AND NOT (o.type= 'U' AND ro.type='U') AND NOT (o.type= 'V' AND ro.type='U') AND NOT (o.type= 'V' AND ro.type='V')");
            }

            string referenceTable = !isFilterForReferenced ? "o" : "ro";
            string type = null;
            string[] filterNames = null;

            if (filter?.DatabaseObjectType == DatabaseObjectType.Procedure)
            {
                type = "'P'";
                filterNames = filter?.ProcedureNames;
            }
            else if (filter?.DatabaseObjectType == DatabaseObjectType.Function)
            {
                type = "'FN','TF'";
                filterNames = filter?.FunctionNames;
            }
            else if (filter.DatabaseObjectType == DatabaseObjectType.View)
            {
                type = "'V'";
                filterNames = filter?.ViewNames;
            }
            else if (filter.DatabaseObjectType == DatabaseObjectType.Table)
            {
                type = "'U'";
                filterNames = filter?.TableNames;
            }

            if (type != null)
            {
                sb.Append($"AND {referenceTable}.type in({type})");
            }

            sb.Append(this.GetFilterNamesCondition(filter, filterNames, $"{referenceTable}.name"));

            sb.Append(this.GetFilterSchemaCondition(filter, $"schema_name({referenceTable}.schema_id)"));

            return sb.Content;
        }
        #endregion
        #endregion

        #region Database Operation

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
                foreach (DataColumn column in dataTable.Columns)
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
                   || item.DataType == typeof(SdoGeometry)
                   || item.DataType == typeof(StGeometry)
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
                                else if (dataType == "geometry")
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

                                DatabaseType sourcedDbType = bulkCopyInfo.SourceDatabaseType;

                                if (sourcedDbType == DatabaseType.MySql)
                                {
                                    if (dataType == "geometry")
                                    {
                                        newColumnType = typeof(SqlGeometry);
                                        newValue = MySqlGeometryHelper.ToSqlGeometry(value as byte[]);
                                    }
                                    else if (dataType == "geography")
                                    {
                                        newColumnType = typeof(SqlGeography);
                                        newValue = MySqlGeometryHelper.ToSqlGeography(value as byte[]);
                                    }
                                }
                            }
                            else if (type == typeof(BitArray))
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
                            else if (value is PgGeom.Geometry)
                            {
                                if (dataType == "geography")
                                {
                                    newColumnType = typeof(SqlGeography);
                                    newValue = PostgresGeometryHelper.ToSqlGeography(value as PgGeom.Geometry);
                                }
                                else
                                {
                                    newColumnType = typeof(SqlGeometry);
                                    newValue = PostgresGeometryHelper.ToSqlGeometry(value as PgGeom.Geometry);
                                }
                            }
                            else if (value is NpgsqlTsVector || value is NpgsqlLine || value is NpgsqlBox || value is NpgsqlCircle
                                   || value is NpgsqlPath || value is NpgsqlLSeg)
                            {
                                newColumnType = typeof(String);
                                newValue = value.ToString();
                            }
                            else if (value is SdoGeometry)
                            {
                                if (dataType == "geography")
                                {
                                    newColumnType = typeof(SqlGeography);
                                    newValue = OracleSdoGeometryHelper.ToSqlGeography(value as SdoGeometry);
                                }
                                else
                                {
                                    newColumnType = typeof(SqlGeometry);
                                    newValue = OracleSdoGeometryHelper.ToSqlGeometry(value as SdoGeometry);
                                }
                            }
                            else if (value is StGeometry)
                            {
                                if (dataType == "geography")
                                {
                                    newColumnType = typeof(SqlGeography);
                                    newValue = OracleStGeometryHelper.ToSqlGeography(value as StGeometry);
                                }
                                else
                                {
                                    newColumnType = typeof(SqlGeometry);
                                    newValue = OracleStGeometryHelper.ToSqlGeometry(value as StGeometry);
                                }
                            }

                            if (newColumnType != null && !changedColumns.ContainsKey(i))
                            {
                                changedColumns.Add(i, new DataTableColumnChangeInfo() { Type = newColumnType, MaxLength = newMaxLength });
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

            if (connection.State != ConnectionState.Open)
            {
                await this.OpenConnectionAsync(connection);
            }

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
                string dataType = string.IsNullOrEmpty(column.DataTypeSchema) ? this.GetQuotedString(column.DataType) : $"{this.GetQuotedString(column.DataTypeSchema)}.{this.GetQuotedString(column.DataType)}";

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

                string identityClause = (this.Option.TableScriptsGenerateOption.GenerateIdentity && column.IsIdentity && column.IsRequired ? $"IDENTITY({table.IdentitySeed??1},{table.IdentityIncrement??1})" : "");
                string requireClause = (column.IsRequired ? "NOT NULL" : "NULL");
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

            DataTypeInfo dataTypeInfo = this.GetDataTypeInfo(dataType);

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
                    long precision = column.Precision == null ? 0 : column.Precision.Value;
                    long scale = column.Scale == null ? 0 : column.Scale.Value;

                    return $"{precision},{scale}";
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
