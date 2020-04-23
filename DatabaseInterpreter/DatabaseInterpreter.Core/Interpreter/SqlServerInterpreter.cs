using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class SqlServerInterpreter : DbInterpreter
    {
        #region Field & Property
        public override string CommandParameterChar { get { return "@"; } }
        public override char QuotationLeftChar { get { return '['; } }
        public override char QuotationRightChar { get { return ']'; } }
        public override DatabaseType DatabaseType { get { return DatabaseType.SqlServer; } }
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
            string sql = @"SELECT schema_name(T.schema_id) AS [Owner], 
                            T.name AS [TableName],
                            C.name AS [Name], 
                            ST.name AS [DataType],
                            C.is_nullable AS [IsNullable],
                            C.max_length AS [MaxLength], 
                            C.precision AS [Precision],
                            C.column_id as [Order], 
                            C.scale AS [Scale],
                            SCO.text As [DefaultValue], 
                            EXT.value AS [Comment],
                            C.is_identity AS [IsIdentity],
                            STY.is_user_defined AS [IsUserDefined],
                            schema_name(STY.schema_id) AS [TypeOwner]
                        FROM sys.columns C 
                        JOIN sys.systypes ST ON C.user_type_id = ST.xusertype
                        JOIN sys.tables T ON C.object_id=T.object_id
                        LEFT JOIN sys.syscomments SCO ON C.default_object_id=SCO.id
                        LEFT JOIN sys.extended_properties EXT on C.column_id=EXT.minor_id AND C.object_id=EXT.major_id AND EXT.class_desc='OBJECT_OR_COLUMN' AND EXT.name='MS_Description'
						LEFT JOIN sys.types STY on C.user_type_id = STY.user_type_id";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" WHERE T.name IN ({ strTableNames })";
            }

            return sql;
        }
        #endregion

        #region Table Primary Key
        public override Task<List<TablePrimaryKey>> GetTablePrimaryKeysAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TablePrimaryKey>(this.GetSqlForTablePrimaryKeys(filter));
        }

        public override Task<List<TablePrimaryKey>> GetTablePrimaryKeysAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TablePrimaryKey>(dbConnection, this.GetSqlForTablePrimaryKeys(filter));
        }

        private string GetSqlForTablePrimaryKeys(SchemaInfoFilter filter = null)
        {
            string sql = @"SELECT schema_name(T.schema_id) AS [Owner], object_name(IC.object_id) AS TableName,I.name AS [Name], 
                           C.name AS [ColumnName], IC.key_ordinal AS [Order],IC.is_descending_key AS [IsDesc]
                         FROM sys.index_columns IC
                         JOIN sys.columns C ON IC.object_id=C.object_id AND IC.column_id=C.column_id						
                         JOIN sys.indexes I ON IC.object_id=I.object_id AND IC.index_id=I.index_id
                         JOIN sys.tables T ON C.object_id=T.object_id
                         WHERE I.is_primary_key=1";

            if (filter!=null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND object_name(IC.object_id) IN ({ strTableNames })";
            }
            return sql;
        }
        #endregion

        #region Table Foreign Key
        public override Task<List<TableForeignKey>> GetTableForeignKeysAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableForeignKey>(this.GetSqlForTableForeignKeys(filter));
        }

        public override Task<List<TableForeignKey>> GetTableForeignKeysAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableForeignKey>(dbConnection, this.GetSqlForTableForeignKeys(filter));
        }

        private string GetSqlForTableForeignKeys(SchemaInfoFilter filter = null)
        {
            string sql = @"SELECT schema_name(T.schema_id) AS [Owner],object_name(FK.parent_object_id) AS TableName,FK.name AS [Name],C.name AS [ColumnName],
                         object_name(FKC.referenced_object_id) AS [ReferencedTableName],RC.name AS [ReferencedColumnName],
                         FK.update_referential_action AS [UpdateCascade],FK.delete_referential_action AS [DeleteCascade]
                         FROM sys.foreign_keys FK
                         JOIN sys.foreign_key_columns FKC ON FK.object_id=FKC.constraint_object_id
                         JOIN sys.columns C ON FK.parent_object_id=C.object_id AND  FKC.parent_column_id=C.column_id
                         JOIN sys.columns RC ON FKC.referenced_object_id= RC.object_id AND RC.column_id=FKC.referenced_column_id
                         JOIN sys.tables T ON C.object_id=T.object_id
                         JOIN sys.tables RT ON RC.object_id=RT.object_id
                         WHERE 1=1";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any() )
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND object_name(FK.parent_object_id) IN ({ strTableNames })";
            }

            return sql;
        }
        #endregion

        #region Table Index
        public override Task<List<TableIndex>> GetTableIndexesAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableIndex>(this.GetSqlForTableIndexes(filter));
        }

        public override Task<List<TableIndex>> GetTableIndexesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableIndex>(dbConnection, this.GetSqlForTableIndexes(filter));
        }

        private string GetSqlForTableIndexes(SchemaInfoFilter filter = null)
        {
            string sql = @"SELECT schema_name(T.schema_id) AS [Owner],object_name(IC.object_id) AS TableName,I.name AS [Name], 
                           I.is_unique AS [IsUnique], C.name AS [ColumnName], IC.key_ordinal AS [Order],IC.is_descending_key AS [IsDesc]
                        FROM sys.index_columns IC
                        JOIN sys.columns C ON IC.object_id=C.object_id AND IC.column_id=C.column_id
                        JOIN sys.indexes I ON IC.object_id=I.object_id AND IC.index_id=I.index_id
                        JOIN sys.tables T ON C.object_id=T.object_id
                        WHERE I.is_primary_key=0 and I.type_desc<>'XML' AND IC.key_ordinal > 0 ";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND object_name(IC.object_id) IN ({ strTableNames })";
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

            if (filter!= null)
            {
                if(filter.TableNames != null && filter.TableNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                    sql += $" AND object_name(t.parent_id) IN ({ strNames })";
                }

                if(filter.TableTriggerNames!=null && filter.TableTriggerNames.Any())
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
            string sql = @"select  schema_name(st.schema_id) AS [Owner], st.name as [TableName], col.name as [ColumnName], chk.name as [Name], chk.definition as [Definition]
                         from sys.check_constraints chk
                         inner join sys.columns col on chk.parent_object_id = col.object_id and col.column_id = chk.parent_column_id
                         inner join sys.tables st on chk.parent_object_id = st.object_id";

            if (filter != null && filter.TableNames!=null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND st.name IN ({ strTableNames })";
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

            if (filter!=null && filter.ViewNames != null && filter.ViewNames.Any())
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
                            WHERE 1=1";

            if (filter!= null && filter.ProcedureNames != null && filter.ProcedureNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.ProcedureNames);
                sql += $" AND name IN ({ strNames })";
            }

            sql += " ORDER BY name";

            return sql;
        }
        #endregion
        #endregion

        #region Database Operation
        public override async Task SetIdentityEnabled(DbConnection dbConnection, TableColumn column, bool enabled)
        {
            await this.ExecuteNonQueryAsync(dbConnection, $"SET IDENTITY_INSERT { this.GetQuotedObjectName(new Table() { Name = column.TableName, Owner = column.Owner })} {(enabled ? "OFF" : "ON")}", false);
        }

        public override async Task BulkCopyAsync(DbConnection connection, DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            SqlBulkCopy bulkCopy = await this.GetBulkCopy(connection, bulkCopyInfo);
            {
                await bulkCopy.WriteToServerAsync(dataTable, bulkCopyInfo.CancellationToken);
            }
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
            string sql = $@"EXEC sp_MSForEachTable 'ALTER TABLE ? {(enabled ? "CHECK" : "NOCHECK")} CONSTRAINT ALL';
                          EXEC sp_MSForEachTable 'ALTER TABLE ? {(enabled ? "ENABLE" : "DISABLE")} TRIGGER ALL';";

            return sql;
        }

        public override Task Drop<T>(DbConnection dbConnection, T dbObjet)
        {
            string sql = "";

            if (dbObjet is TableColumnChild || dbObjet is TableConstraint)
            {
                TableChild dbObj = dbObjet as TableChild;

                sql = $"ALTER TABLE {dbObj.Owner}.{this.GetQuotedString(dbObj.TableName)} DROP CONSTRAINT {this.GetQuotedString(dbObj.Name)};";
            }
            else if (dbObjet is TableIndex)
            {
                TableIndex index = dbObjet as TableIndex;

                sql = $"DROP INDEX {this.GetQuotedString(index.Name)} ON {index.Owner}.{this.GetQuotedString(index.TableName)};";
            }
            else
            {
                string typeName = dbObjet.GetType() == typeof(UserDefinedType) ? "TYPE" : dbObjet.GetType().Name;

                sql = $"DROP {typeName} IF EXISTS {this.GetQuotedObjectName(dbObjet)};";
            }

            return this.ExecuteNonQueryAsync(dbConnection, sql, false);
        }
        #endregion

        #region Generate Schema Script   

        public override ScriptBuilder GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            ScriptBuilder sb = new ScriptBuilder();

            #region User Defined Type
            List<string> userTypeNames = new List<string>();
            foreach (UserDefinedType userDefinedType in schemaInfo.UserDefinedTypes)
            {
                this.FeedbackInfo(OperationState.Begin, userDefinedType);

                string userTypeName = userDefinedType.Name;

                if(userTypeNames.Contains(userTypeName))
                {
                    userTypeName += "_"  + userDefinedType.AttrName;
                }

                TableColumn column = new TableColumn() { DataType = userDefinedType.Type, MaxLength = userDefinedType.MaxLength, Precision = userDefinedType.Precision, Scale = userDefinedType.Scale };
                string dataLength = this.GetColumnDataLength(column);

                string script = $@"CREATE TYPE {this.GetQuotedString(userDefinedType.Owner)}.{this.GetQuotedString(userTypeName)} FROM {this.GetQuotedString(userDefinedType.Type)}{(dataLength == "" ? "" : "(" + dataLength + ")")} {(userDefinedType.IsRequired ? "NOT NULL" : "NULL")};";

                sb.AppendLine(new CreateDbObjectScript<UserDefinedType>(script));
                sb.AppendLine(new SpliterScript(this.ScriptsDelimiter));

                userTypeNames.Add(userDefinedType.Name);

                this.FeedbackInfo(OperationState.End, userDefinedType);
            }

            #endregion

            #region Function           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<Function>(schemaInfo.Functions));
            #endregion

            #region Table
            foreach (Table table in schemaInfo.Tables)
            {
                this.FeedbackInfo(OperationState.Begin, table);

                string tableName = table.Name;
                string quotedTableName = this.GetQuotedObjectName(table);
                IEnumerable<TableColumn> tableColumns = schemaInfo.TableColumns.Where(item => item.Owner == table.Owner && item.TableName == tableName).OrderBy(item => item.Order);

                bool hasBigDataType = tableColumns.Any(item => this.IsBigDataType(item));

                string primaryKey = "";

                IEnumerable<TablePrimaryKey> primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.Owner == table.Owner && item.TableName == tableName);

                #region Primary Key
                if (this.Option.TableScriptsGenerateOption.GeneratePrimaryKey && primaryKeys.Count() > 0)
                {
                    primaryKey =
$@"
,CONSTRAINT {this.GetQuotedString(primaryKeys.First().Name)} PRIMARY KEY CLUSTERED 
(
{string.Join(Environment.NewLine, primaryKeys.Select(item => $"{this.GetQuotedString(item.ColumnName)} {(item.IsDesc ? "DESC" : "ASC")},")).TrimEnd(',')}
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]";

                }

                #endregion

                #region Create Table

                string existsClause = $"IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='{(table.Name)}')";

                string tableScript =
$@"
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

{(this.NotCreateIfExists ? existsClause : "")}
CREATE TABLE {quotedTableName}(
{string.Join("," + Environment.NewLine, tableColumns.Select(item => this.ParseColumn(table, item)))}{primaryKey}
) ON [PRIMARY]{(hasBigDataType ? " TEXTIMAGE_ON [PRIMARY]" : "")}" + ";";

                sb.AppendLine(new CreateDbObjectScript<Table>(tableScript));

                #endregion

                sb.AppendLine();

                #region Comment
                if (!string.IsNullOrEmpty(table.Comment))
                {
                    sb.AppendLine(new ExecuteProcedureScript($"EXECUTE sp_addextendedproperty N'MS_Description',N'{ValueHelper.TransferSingleQuotation(table.Comment)}',N'SCHEMA',N'{table.Owner}',N'table',N'{tableName}',NULL,NULL;"));
                }

                foreach (TableColumn column in tableColumns.Where(item => !string.IsNullOrEmpty(item.Comment)))
                {
                    sb.AppendLine(new ExecuteProcedureScript($"EXECUTE sp_addextendedproperty N'MS_Description',N'{ValueHelper.TransferSingleQuotation(column.Comment)}',N'SCHEMA',N'{table.Owner}',N'table',N'{tableName}',N'column',N'{column.Name}';"));
                }
                #endregion               

                #region Foreign Key
                if (this.Option.TableScriptsGenerateOption.GenerateForeignKey)
                {
                    IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.Owner == table.Owner && item.TableName == tableName);
                    if (foreignKeys.Count() > 0)
                    {
                        ILookup<string, TableForeignKey> foreignKeyLookup = foreignKeys.ToLookup(item => item.Name);

                        IEnumerable<string> keyNames = foreignKeyLookup.Select(item => item.Key);

                        foreach (string keyName in keyNames)
                        {
                            TableForeignKey tableForeignKey = foreignKeyLookup[keyName].First();

                            string columnNames = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{ this.GetQuotedString(item.ColumnName)}"));
                            string referenceColumnName = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{ this.GetQuotedString(item.ReferencedColumnName)}"));

                            StringBuilder sbForeignKey = new StringBuilder();

                            sbForeignKey.AppendLine(
$@"
ALTER TABLE {quotedTableName} WITH CHECK ADD CONSTRAINT { this.GetQuotedString(keyName)} FOREIGN KEY({columnNames})
REFERENCES {this.GetQuotedString(table.Owner)}.{this.GetQuotedString(tableForeignKey.ReferencedTableName)} ({referenceColumnName})
");

                            if (tableForeignKey.UpdateCascade)
                            {
                                sbForeignKey.AppendLine("ON UPDATE CASCADE");
                            }

                            if (tableForeignKey.DeleteCascade)
                            {
                                sbForeignKey.AppendLine("ON DELETE CASCADE");
                            }

                            sbForeignKey.AppendLine($"ALTER TABLE {quotedTableName} CHECK CONSTRAINT { this.GetQuotedString(keyName)};");

                            sb.AppendLine(new CreateDbObjectScript<TableForeignKey>(sbForeignKey.ToString()));
                        }
                    }
                }
                #endregion

                #region Index
                if (this.Option.TableScriptsGenerateOption.GenerateIndex)
                {
                    IEnumerable<TableIndex> indices = schemaInfo.TableIndexes.Where(item => item.Owner == table.Owner && item.TableName == tableName).OrderBy(item => item.Order);
                    if (indices.Count() > 0)
                    {
                        List<string> indexColumns = new List<string>();
                        ILookup<string, TableIndex> indexLookup = indices.ToLookup(item => item.Name);
                        IEnumerable<string> indexNames = indexLookup.Select(item => item.Key);
                        foreach (string indexName in indexNames)
                        {
                            TableIndex tableIndex = indexLookup[indexName].First();
                            string columnNames = string.Join(",", indexLookup[indexName].Select(item => $"{this.GetQuotedString(item.ColumnName)} {(item.IsDesc ? "DESC" : "ASC")}"));

                            if (indexColumns.Contains(columnNames))
                            {
                                continue;
                            }

                            sb.AppendLine(new CreateDbObjectScript<TableIndex>($"CREATE {(tableIndex.IsUnique ? "UNIQUE" : "")} INDEX {tableIndex.Name} ON {quotedTableName}({columnNames});"));

                            if (!indexColumns.Contains(columnNames))
                            {
                                indexColumns.Add(columnNames);
                            }
                        }
                    }
                }
                #endregion

                #region Default Value
                if (this.Option.TableScriptsGenerateOption.GenerateDefaultValue)
                {
                    IEnumerable<TableColumn> defaultValueColumns = schemaInfo.TableColumns.Where(item => item.Owner == table.Owner && item.TableName == tableName && !string.IsNullOrEmpty(item.DefaultValue));
                    foreach (TableColumn column in defaultValueColumns)
                    {
                        sb.AppendLine(new AlterDbObjectScript<Table>($"ALTER TABLE {quotedTableName} ADD CONSTRAINT {this.GetQuotedString($" DF_{tableName}_{column.Name}")}  DEFAULT {this.GetColumnDefaultValue(column)} FOR { this.GetQuotedString(column.Name)};"));
                    }
                }
                #endregion

                #region Constraint
                if (this.Option.TableScriptsGenerateOption.GenerateConstraint)
                {
                    var constraints = schemaInfo.TableConstraints.Where(item => item.Owner == table.Owner && item.TableName == tableName);

                    foreach (TableConstraint constraint in constraints)
                    {
                        sb.AppendLine(new CreateDbObjectScript<TableConstraint>($"ALTER TABLE {quotedTableName}  WITH CHECK ADD CONSTRAINT {this.GetQuotedString(constraint.Name)} CHECK  ({constraint.Definition});"));
                    }
                }
                #endregion

                sb.Append(new SpliterScript(this.ScriptsDelimiter));

                this.FeedbackInfo(OperationState.End, table);
            }

            #endregion

            #region View           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<View>(schemaInfo.Views));
            #endregion

            #region Trigger           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<TableTrigger>(schemaInfo.TableTriggers));
            #endregion

            #region Procedure           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<Procedure>(schemaInfo.Procedures));
            #endregion

            if (this.Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
            {
                this.AppendScriptsToFile(sb.ToString(), GenerateScriptMode.Schema, true);
            }

            return sb;
        }

        public override string ParseColumn(Table table, TableColumn column)
        {
            if (column.IsUserDefined)
            {
                return $@"{this.GetQuotedString(column.Name)} {this.GetQuotedString(column.TypeOwner)}.{this.GetQuotedString(column.DataType)} {(column.IsRequired ? "NOT NULL" : "NULL")}";
            }

            string dataType = this.ParseDataType(column);

            string identityClause = (this.Option.TableScriptsGenerateOption.GenerateIdentity && column.IsIdentity ? $"IDENTITY({table.IdentitySeed},{table.IdentityIncrement})" : "");
            string requireClause = (column.IsRequired ? "NOT NULL" : "NULL");

            return $@"{this.GetQuotedString(column.Name)} {dataType} {identityClause} {requireClause}";
        }

        public override string ParseDataType(TableColumn column)
        {
            string dataLength = this.GetColumnDataLength(column);

            if (!string.IsNullOrEmpty(dataLength))
            {
                dataLength = $"({dataLength})";
            }

            string dataType = $"{this.GetQuotedString(column.DataType)} {dataLength}";

            return dataType;
        }

        private string GetColumnDataLength(TableColumn column)
        {
            switch (column.DataType)
            {
                case "nchar":
                case "nvarchar":
                    if (column.MaxLength == -1)
                    {
                        return "max";
                    }
                    return ((column.MaxLength ?? 0) / 2).ToString();
                case "char":
                case "varchar":
                case "binary":
                case "varbinary":
                    if (column.MaxLength == -1)
                    {
                        return "max";
                    }
                    return column.MaxLength?.ToString();
                case "bit":
                case "tinyint":
                case "int":
                case "smallint":
                case "bigint":
                case "float":
                case "real":
                case "money":
                case "smallmoney":
                case "date":
                case "smalldatetime":
                case "datetime":
                case "timestamp":
                case "uniqueidentifier":
                case "xml":
                case "text":
                case "ntext":
                case "image":
                case "sql_variant":
                case "geography":
                case "geometry":
                case "hierarchyid":
                    return "";
                case "datetime2":
                case "datetitmeoffset":
                    return column.Scale?.ToString();
                case "decimal":
                case "numeric":
                    return $"{column.Precision},{column.Scale}";
            }

            return "";
        }

        private bool IsBigDataType(TableColumn column)
        {
            switch (column.DataType)
            {
                case "text":
                case "ntext":
                case "image":
                case "xml":
                    return true;
                case "varchar":
                case "nvarchar":
                case "varbinary":
                    if (column.MaxLength == -1)
                    {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        #endregion

        #region Generate Data Script       
        public override Task<long> GetTableRecordCountAsync(DbConnection connection, Table table, string whereClause = "")
        {
            string sql = $"SELECT COUNT(1) FROM {this.GetQuotedObjectName(table)}";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += whereClause;
            }

            return base.GetTableRecordCountAsync(connection, sql);
        }

        public override Task<string> GenerateDataScriptsAsync(SchemaInfo schemaInfo)
        {
            return base.GenerateDataScriptsAsync(schemaInfo);
        }

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

        protected override string GetBytesConvertHexString(object value, string dataType)
        {
            string hex = string.Concat(((byte[])value).Select(item => item.ToString("X2")));
            return $"CAST({"0x" + hex} AS {dataType})";
        }

        public override string GetLimitStatement(int limitStart, int limitCount)
        {
            return $"OFFSET {limitStart} ROWS FETCH NEXT {limitCount} ROWS ONLY";
        }

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
            SqlCommand command = dbCommand as SqlCommand;
            command.StatementCompleted += Command_StatementCompleted;
        }

        private void Command_StatementCompleted(object sender, StatementCompletedEventArgs e)
        {
            this.FeedbackInfo($"{e.RecordCount} row(s) affected.");
        }      
        #endregion
    }
}
