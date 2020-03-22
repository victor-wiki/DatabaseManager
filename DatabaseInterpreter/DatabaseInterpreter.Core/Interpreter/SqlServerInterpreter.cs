using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
        public override string ScriptsSplitString => "GO" + Environment.NewLine;
        public override List<string> BuiltinDatabases => new List<string> { "master", "model", "msdb", "tempdb" };
        #endregion

        #region Constructor
        public SqlServerInterpreter(ConnectionInfo connectionInfo, DbInterpreterOption options) : base(connectionInfo, options)
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

        public override Task<List<UserDefinedType>> GetUserDefinedTypesAsync(params string[] userDefinedTypeNames)
        {
            return base.GetDbObjectsAsync<UserDefinedType>(this.GetSqlForUserDefinedTypes(userDefinedTypeNames));
        }
        public override Task<List<UserDefinedType>> GetUserDefinedTypesAsync(DbConnection dbConnection, params string[] userDefinedTypeNames)
        {
            return base.GetDbObjectsAsync<UserDefinedType>(dbConnection, this.GetSqlForUserDefinedTypes(userDefinedTypeNames));
        }

        private string GetSqlForUserDefinedTypes(params string[] userDefinedTypeNames)
        {
            string sql = @"SELECT schema_name(T.schema_id) AS [Owner],T.name as Name, ST.name AS Type, T.max_length AS MaxLength, T.precision AS Precision,T.scale AS Scale,T.is_nullable AS IsNullable
                            FROM sys.types T JOIN sys.systypes ST ON T.system_type_id=ST.xusertype
                            WHERE is_user_defined=1";

            if (userDefinedTypeNames != null && userDefinedTypeNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(userDefinedTypeNames);
                sql += $" AND T.name in ({ strNames })";
            }

            return sql;
        }
        #endregion

        #region Function       

        public override Task<List<Function>> GetFunctionsAsync(params string[] functionNames)
        {
            return base.GetDbObjectsAsync<Function>(this.GetSqlForFunctions(functionNames));
        }

        public override Task<List<Function>> GetFunctionsAsync(DbConnection dbConnection, params string[] functionNames)
        {
            return base.GetDbObjectsAsync<Function>(dbConnection, this.GetSqlForFunctions(functionNames));
        }

        private string GetSqlForFunctions(params string[] functionNames)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = $@"SELECT o.name AS [Name], schema_name(o.schema_id) AS [Owner], 
                           {(isSimpleMode ? "''" : "OBJECT_DEFINITION(o.object_id)")} AS [Definition]
                           FROM sys.all_objects o 
                           WHERE o.type IN ('FN', 'IF', 'FN', 'AF', 'FS', 'FT')
                           AND SCHEMA_NAME(schema_id)='dbo'";

            if (functionNames != null && functionNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(functionNames);
                sql += $" AND o.name IN ({ strNames })";
            }

            sql += " ORDER BY o.name";

            return sql;
        }
        #endregion

        #region Table      

        public override Task<List<Table>> GetTablesAsync(params string[] tableNames)
        {
            return base.GetDbObjectsAsync<Table>(this.GetSqlForTables(tableNames));
        }

        public override Task<List<Table>> GetTablesAsync(DbConnection dbConnection, params string[] tableNames)
        {
            return base.GetDbObjectsAsync<Table>(dbConnection, this.GetSqlForTables(tableNames));
        }

        private string GetSqlForTables(params string[] tableNames)
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

            if (tableNames != null && tableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND t.name in ({ strTableNames })";
            }

            sql += " ORDER BY t.name";

            return sql;
        }
        #endregion

        #region Table Column
        public override Task<List<TableColumn>> GetTableColumnsAsync(params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableColumn>(this.GetSqlForTableColumns(tableNames));
        }

        public override Task<List<TableColumn>> GetTableColumnsAsync(DbConnection dbConnection, params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableColumn>(dbConnection, this.GetSqlForTableColumns(tableNames));
        }

        private string GetSqlForTableColumns(params string[] tableNames)
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

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" WHERE T.name IN ({ strTableNames })";
            }

            return sql;
        }
        #endregion

        #region Table Primary Key
        public override Task<List<TablePrimaryKey>> GetTablePrimaryKeysAsync(params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TablePrimaryKey>(this.GetSqlForTablePrimaryKeys(tableNames));
        }

        public override Task<List<TablePrimaryKey>> GetTablePrimaryKeysAsync(DbConnection dbConnection, params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TablePrimaryKey>(dbConnection, this.GetSqlForTablePrimaryKeys(tableNames));
        }

        private string GetSqlForTablePrimaryKeys(params string[] tableNames)
        {
            string sql = @"SELECT schema_name(T.schema_id) AS [Owner], object_name(IC.object_id) AS TableName,I.name AS [Name], 
                           C.name AS [ColumnName], IC.key_ordinal AS [Order],IC.is_descending_key AS [IsDesc]
                         FROM sys.index_columns IC
                         JOIN sys.columns C ON IC.object_id=C.object_id AND IC.column_id=C.column_id						
                         JOIN sys.indexes I ON IC.object_id=I.object_id AND IC.index_id=I.index_id
                         JOIN sys.tables T ON C.object_id=T.object_id
                         WHERE I.is_primary_key=1";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND object_name(IC.object_id) IN ({ strTableNames })";
            }
            return sql;
        }
        #endregion

        #region Table Foreign Key
        public override Task<List<TableForeignKey>> GetTableForeignKeysAsync(params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableForeignKey>(this.GetSqlForTableForeignKeys(tableNames));
        }

        public override Task<List<TableForeignKey>> GetTableForeignKeysAsync(DbConnection dbConnection, params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableForeignKey>(dbConnection, this.GetSqlForTableForeignKeys(tableNames));
        }

        private string GetSqlForTableForeignKeys(params string[] tableNames)
        {
            string sql = @"SELECT schema_name(T.schema_id) AS [Owner],object_name(FK.parent_object_id) AS TableName,FK.name AS [Name],C.name AS [ColumnName],
                         object_name(FKC.referenced_object_id) AS [ReferencedTableName],RC.name AS [ReferencedColumnName],
                         FK.update_referential_action AS [UpdateCascade],FK.delete_referential_action AS [DeleteCascade]
                         FROM sys.foreign_keys FK
                         JOIN sys.foreign_key_columns FKC ON FK.object_id=FKC.constraint_object_id AND FK.object_id=FKC.constraint_object_id
                         JOIN sys.columns C ON FK.parent_object_id=C.object_id AND  FKC.parent_column_id=C.column_id
                         JOIN sys.columns RC ON FKC.referenced_object_id= RC.object_id AND RC.column_id=FKC.referenced_column_id
                         JOIN sys.tables T ON C.object_id=T.object_id
                         JOIN sys.tables RT ON RC.object_id=RT.object_id AND RT.schema_id=T.schema_id";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND object_name(FK.parent_object_id) IN ({ strTableNames })";
            }

            return sql;
        }
        #endregion

        #region Table Index
        public override Task<List<TableIndex>> GetTableIndexesAsync(params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableIndex>(this.GetSqlForTableIndexes(tableNames));
        }

        public override Task<List<TableIndex>> GetTableIndexesAsync(DbConnection dbConnection, params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableIndex>(dbConnection, this.GetSqlForTableIndexes(tableNames));
        }

        private string GetSqlForTableIndexes(params string[] tableNames)
        {
            string sql = @"SELECT schema_name(T.schema_id) AS [Owner],object_name(IC.object_id) AS TableName,I.name AS [Name], 
                           I.is_unique AS [IsUnique], C.name AS [ColumnName], IC.key_ordinal AS [Order],IC.is_descending_key AS [IsDesc]
                        FROM sys.index_columns IC
                        JOIN sys.columns C ON IC.object_id=C.object_id AND IC.column_id=C.column_id
                        JOIN sys.indexes I ON IC.object_id=I.object_id AND IC.index_id=I.index_id
                        JOIN sys.tables T ON C.object_id=T.object_id
                        WHERE I.is_primary_key=0 and I.type_desc<>'XML'";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND object_name(IC.object_id) IN ({ strTableNames })";
            }

            return sql;
        }
        #endregion

        #region Table Trigger       

        public override Task<List<TableTrigger>> GetTableTriggersAsync(params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableTrigger>(this.GetSqlForTableTriggers(tableNames));
        }

        public override Task<List<TableTrigger>> GetTableTriggersAsync(DbConnection dbConnection, params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableTrigger>(dbConnection, this.GetSqlForTableTriggers(tableNames));
        }

        private string GetSqlForTableTriggers(params string[] tableNames)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = $@"SELECT t.name AS [Name], OBJECT_SCHEMA_NAME(t.object_id) AS [Owner],object_name(t.parent_id) AS [TableName], 
                            {(isSimpleMode ? "''" : "OBJECT_DEFINITION(t.object_id)")} AS [Definition]
                            FROM sys.triggers t
                            WHERE 1=1";

            if (tableNames != null && tableNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND object_name(t.parent_id) IN ({ strNames })";
            }

            sql += " ORDER BY t.name";

            return sql;
        }
        #endregion

        #region Table Constraint
        public override Task<List<TableConstraint>> GetTableConstraintsAsync(params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableConstraint>(this.GetSqlForTableConstraints(tableNames));
        }

        public override Task<List<TableConstraint>> GetTableConstraintsAsync(DbConnection dbConnection, params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableConstraint>(dbConnection, this.GetSqlForTableConstraints(tableNames));
        }

        private string GetSqlForTableConstraints(params string[] tableNames)
        {
            string sql = @"select  schema_name(st.schema_id) AS [Owner], st.name as [TableName], col.name as [ColumnName], chk.name as [Name], chk.definition as [Definition]
                         from sys.check_constraints chk
                         inner join sys.columns col on chk.parent_object_id = col.object_id and col.column_id = chk.parent_column_id
                         inner join sys.tables st on chk.parent_object_id = st.object_id";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND st.name IN ({ strTableNames })";
            }

            return sql;
        }
        #endregion

        #region View       

        public override Task<List<View>> GetViewsAsync(params string[] viewNames)
        {
            return base.GetDbObjectsAsync<View>(this.GetSqlForViews(viewNames));
        }

        public override Task<List<View>> GetViewsAsync(DbConnection dbConnection, params string[] viewNames)
        {
            return base.GetDbObjectsAsync<View>(dbConnection, this.GetSqlForViews(viewNames));
        }

        private string GetSqlForViews(params string[] viewNames)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = $@"SELECT v.name AS [Name], schema_name(v.schema_id) AS [Owner], {(isSimpleMode ? "''" : "OBJECT_DEFINITION(object_id)")} AS [Definition]
                            FROM sys.views v
                            WHERE 1=1";

            if (viewNames != null && viewNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(viewNames);
                sql += $" AND v.name IN ({ strNames })";
            }

            sql += " ORDER BY v.name";

            return sql;
        }
        #endregion       

        #region Procedure       

        public override Task<List<Procedure>> GetProceduresAsync(params string[] procedureNames)
        {
            return base.GetDbObjectsAsync<Procedure>(this.GetSqlForProcedures(procedureNames));
        }

        public override Task<List<Procedure>> GetProceduresAsync(DbConnection dbConnection, params string[] procedureNames)
        {
            return base.GetDbObjectsAsync<Procedure>(dbConnection, this.GetSqlForProcedures(procedureNames));
        }

        private string GetSqlForProcedures(params string[] procedureNames)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = $@"SELECT name AS [Name], SCHEMA_NAME(schema_id) AS [Owner], 
                            {(isSimpleMode ? "''" : "OBJECT_DEFINITION(object_id)")} AS [Definition]
                            FROM sys.procedures
                            WHERE 1=1";

            if (procedureNames != null && procedureNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(procedureNames);
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
            await this.ExecuteNonQueryAsync(dbConnection, $"SET IDENTITY_INSERT {GetQuotedObjectName(new Table() { Name = column.TableName, Owner = column.Owner })} {(enabled ? "OFF" : "ON")}");
        }

        public override async Task<int> BulkCopyAsync(DbConnection connection, DataTable dataTable, string destinationTableName = null, int? bulkCopyTimeout = null, int? batchSize = null)
        {
            SqlBulkCopy bulkCopy = await this.GetBulkCopy(connection, destinationTableName, bulkCopyTimeout, batchSize);
            {
                await bulkCopy.WriteToServerAsync(dataTable);
            }

            return 0;
        }

        private async Task<SqlBulkCopy> GetBulkCopy(DbConnection connection, string destinationTableName = null, int? bulkCopyTimeout = null, int? batchSize = null)
        {
            SqlBulkCopy bulkCopy = new SqlBulkCopy(connection as SqlConnection);

            if (connection.State == ConnectionState.Closed)
            {
                await connection.OpenAsync();
            }

            bulkCopy.DestinationTableName = this.GetQuotedString(destinationTableName);
            bulkCopy.BulkCopyTimeout = bulkCopyTimeout.HasValue ? bulkCopyTimeout.Value : SettingManager.Setting.CommandTimeout;
            bulkCopy.BatchSize = batchSize.HasValue ? batchSize.Value : this.DataBatchSize;

            return bulkCopy;
        }

        public override Task SetConstrainsEnabled(bool enabled)
        {
            string sql = $@"EXEC sp_MSForEachTable 'ALTER TABLE ? {(enabled ? "CHECK" : "NOCHECK")} CONSTRAINT ALL';
                          EXEC sp_MSForEachTable 'ALTER TABLE ? {(enabled ? "ENABLE" : "DISABLE")} TRIGGER ALL';";

            return this.ExecuteNonQueryAsync(sql);
        }

        public override Task Drop<T>(DbConnection dbConnection, T dbObjet)
        {
            string sql = "";

            if(dbObjet is TableKey || dbObjet is TableConstraint)
            {
                TableChild dbObj = dbObjet as TableChild;

                sql = $"ALTER TABLE {dbObj.Owner}.{this.GetQuotedString(dbObj.TableName)} DROP CONSTRAINT {this.GetQuotedString(dbObj.Name)};";
            }
            else if(dbObjet is TableIndex)
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

        public override string GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            StringBuilder sb = new StringBuilder();

            #region User Defined Type
            foreach (UserDefinedType userDefinedType in schemaInfo.UserDefinedTypes)
            {
                this.FeedbackInfo(OperationState.Begin, userDefinedType);

                TableColumn column = new TableColumn() { DataType = userDefinedType.Type, MaxLength = userDefinedType.MaxLength, Precision = userDefinedType.Precision, Scale = userDefinedType.Scale };
                string dataLength = this.GetColumnDataLength(column);

                sb.AppendLine($@"CREATE TYPE {this.GetQuotedString(userDefinedType.Owner)}.{this.GetQuotedString(userDefinedType.Name)} FROM {this.GetQuotedString(userDefinedType.Type)}{(dataLength == "" ? "" : "(" + dataLength + ")")} {(userDefinedType.IsRequired ? "NOT NULL" : "NULL")};");

                sb.Append(this.ScriptsSplitString);

                sb.Append(this.ScriptsSplitString);

                this.FeedbackInfo(OperationState.End, userDefinedType);
            }

            #endregion

            #region Function           
            sb.Append(this.GenerateScriptDbObjectScripts<Function>(schemaInfo.Functions));
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
                sb.AppendLine(
$@"
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE TABLE {quotedTableName}(
{string.Join("," + Environment.NewLine, tableColumns.Select(item => this.ParseColumn(table, item)))}{primaryKey}
) ON [PRIMARY]{(hasBigDataType ? " TEXTIMAGE_ON [PRIMARY]" : "")}" + this.ScriptsSplitString);
                #endregion

                #region Comment
                if (!string.IsNullOrEmpty(table.Comment))
                {
                    sb.AppendLine($"EXECUTE sp_addextendedproperty N'MS_Description',N'{ValueHelper.TransferSingleQuotation(table.Comment)}',N'SCHEMA',N'{table.Owner}',N'table',N'{tableName}',NULL,NULL;");
                }

                foreach (TableColumn column in tableColumns.Where(item => !string.IsNullOrEmpty(item.Comment)))
                {
                    sb.AppendLine($"EXECUTE sp_addextendedproperty N'MS_Description',N'{ValueHelper.TransferSingleQuotation(column.Comment)}',N'SCHEMA',N'{table.Owner}',N'table',N'{tableName}',N'column',N'{column.Name}';");
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

                            sb.AppendLine(
$@"
ALTER TABLE {quotedTableName} WITH CHECK ADD CONSTRAINT { this.GetQuotedString(keyName)} FOREIGN KEY({columnNames})
REFERENCES {this.GetQuotedString(table.Owner)}.{this.GetQuotedString(tableForeignKey.ReferencedTableName)} ({referenceColumnName})
");

                            if (tableForeignKey.UpdateCascade)
                            {
                                sb.AppendLine("ON UPDATE CASCADE");
                            }

                            if (tableForeignKey.DeleteCascade)
                            {
                                sb.AppendLine("ON DELETE CASCADE");
                            }

                            sb.AppendLine($"ALTER TABLE {quotedTableName} CHECK CONSTRAINT { this.GetQuotedString(keyName)};");
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
                        sb.AppendLine();

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
                            sb.AppendLine($"CREATE {(tableIndex.IsUnique ? "UNIQUE" : "")} INDEX {tableIndex.Name} ON {quotedTableName}({columnNames});");
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
                        sb.AppendLine($"ALTER TABLE {quotedTableName} ADD CONSTRAINT {this.GetQuotedString($" DF_{tableName}_{column.Name}")}  DEFAULT {this.GetColumnDefaultValue(column)} FOR { this.GetQuotedString(column.Name)};");
                    }
                }
                #endregion

                #region Constraint
                if (this.Option.TableScriptsGenerateOption.GenerateConstraint)
                {
                    var constraints = schemaInfo.TableConstraints.Where(item => item.Owner == table.Owner && item.TableName == tableName);

                    foreach (TableConstraint constraint in constraints)
                    {
                        sb.AppendLine($"ALTER TABLE {quotedTableName}  WITH CHECK ADD CONSTRAINT {this.GetQuotedString(constraint.Name)} CHECK  ({constraint.Definition});");
                    }
                }
                #endregion

                sb.Append(this.ScriptsSplitString);

                this.FeedbackInfo(OperationState.End, table);
            }

            #endregion

            #region View           
            sb.Append(this.GenerateScriptDbObjectScripts<View>(schemaInfo.Views));
            #endregion

            #region Trigger           
            sb.Append(this.GenerateScriptDbObjectScripts<TableTrigger>(schemaInfo.TableTriggers));
            #endregion

            #region Procedure           
            sb.Append(this.GenerateScriptDbObjectScripts<Procedure>(schemaInfo.Procedures));
            #endregion

            if (this.Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
            {
                this.AppendScriptsToFile(sb.ToString(), GenerateScriptMode.Schema, true);
            }

            return sb.ToString();
        }

        public override string ParseColumn(Table table, TableColumn column)
        {
            if (column.IsUserDefined)
            {
                return $@"{this.GetQuotedString(column.Name)} {this.GetQuotedString(column.TypeOwner)}.{this.GetQuotedString(column.DataType)} {(column.IsRequired ? "NOT NULL" : "NULL")}";
            }

            string dataLength = this.GetColumnDataLength(column);

            if (!string.IsNullOrEmpty(dataLength))
            {
                dataLength = $"({dataLength})";
            }

            string identityClause = (this.Option.TableScriptsGenerateOption.GenerateIdentity && column.IsIdentity ? $"IDENTITY({table.IdentitySeed},{table.IdentityIncrement})" : "");
            string requireClause = (column.IsRequired ? "NOT NULL" : "NULL");

            return $@"{this.GetQuotedString(column.Name)} {this.GetQuotedString(column.DataType)} {dataLength} {identityClause} {requireClause}";
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
        public override Task<long> GetTableRecordCountAsync(DbConnection connection, Table table)
        {
            string sql = $"SELECT COUNT(1) FROM {this.GetQuotedObjectName(table)}";

            return base.GetTableRecordCountAsync(connection, sql);
        }        

        public override Task<string> GenerateDataScriptsAsync(SchemaInfo schemaInfo)
        {
            return base.GenerateDataScriptsAsync(schemaInfo);
        }

        protected override string GetSqlForPagination(string tableName, string columnNames, string primaryKeyColumns, string whereClause, long pageNumber, int pageSize)
        {
            var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

            string pagedSql = $@"with PagedRecords as
								(
									SELECT TOP 100 PERCENT {columnNames}, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS {RowNumberColumnName}
									FROM {tableName}
                                    {whereClause}
								)
								SELECT *
								FROM PagedRecords
								WHERE {RowNumberColumnName} BETWEEN {startEndRowNumber.StartRowNumber} AND {startEndRowNumber.EndRowNumber}";

            return pagedSql;
        }
        #endregion       
    }
}
