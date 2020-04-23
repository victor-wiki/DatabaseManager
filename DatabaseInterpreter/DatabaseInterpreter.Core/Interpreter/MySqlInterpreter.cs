using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class MySqlInterpreter : DbInterpreter
    {
        #region Field & Property       
        public override string UnicodeInsertChar => "";
        public override string CommandParameterChar { get { return "@"; } }
        public override char QuotationLeftChar { get { return '`'; } }
        public override char QuotationRightChar { get { return '`'; } }
        public override string CommentString => "#";
        public override DatabaseType DatabaseType { get { return DatabaseType.MySql; } }
        public int NameMaxLength => 64;
        public int KeyIndexColumnMaxLength => 500;
        public override bool SupportBulkCopy { get { return true; } }
        public override List<string> BuiltinDatabases => new List<string> { "sys", "mysql", "information_schema", "performance_schema" };
        public readonly string DbCharset = SettingManager.Setting.MySqlCharset;
        public readonly string DbCharsetCollation = SettingManager.Setting.MySqlCharsetCollation;
        public string NotCreateIfExistsCluase { get { return this.NotCreateIfExists ? "IF NOT EXISTS" : ""; } }
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

            if (isSimpleMode)
            {
                sql = $@"SELECT ROUTINE_NAME AS `Name`, ROUTINE_SCHEMA AS `Owner`                        
                        FROM INFORMATION_SCHEMA.`ROUTINES`
                        WHERE ROUTINE_TYPE = '{type}' AND ROUTINE_SCHEMA = '{this.ConnectionInfo.Database}'
                       ";
            }
            else
            {

                string functionReturns = isFunction ? ",' RETURNS ', returns " : "";
                sql = $@"SELECT db AS `Owner`, NAME AS `Name`,
                        CONVERT(CONCAT('CREATE {type} {this.NotCreateIfExistsCluase} `', db , '`.`' , name, '`(' , param_list, ')' {functionReturns} ,'{Environment.NewLine}', body) USING utf8)  AS `Definition`
                        FROM mysql.proc WHERE db='{this.ConnectionInfo.Database}' AND TYPE='{type}'
                        ";
            }

            string[] objectNames = type == "FUNCTION" ? filter?.FunctionNames : filter?.ProcedureNames;

            if (objectNames != null && objectNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(objectNames);
                sql += $" AND {nameColumn} IN ({ strNames })";
            }

            sql += $" ORDER BY {nameColumn}";

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

            string sql = $@"SELECT TABLE_SCHEMA AS `Owner`, TABLE_NAME AS `Name` {(isSimpleMode ? "" : ", TABLE_COMMENT AS `Comment`, 1 AS `IdentitySeed`, 1 AS `IdentityIncrement`")}
                        FROM INFORMATION_SCHEMA.`TABLES`
                        WHERE TABLE_TYPE ='BASE TABLE' AND TABLE_SCHEMA ='{this.ConnectionInfo.Database}' 
                        ";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND TABLE_NAME IN ({ strTableNames })";
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
            string sql = $@"SELECT C.TABLE_SCHEMA AS `Owner`, C.TABLE_NAME AS `TableName`, COLUMN_NAME AS `Name`, COLUMN_TYPE AS `DataType`, 
                        CHARACTER_MAXIMUM_LENGTH AS `MaxLength`, CASE IS_NULLABLE WHEN 'YES' THEN 1 ELSE 0 END AS `IsNullable`,ORDINAL_POSITION AS `Order`,
                        NUMERIC_PRECISION AS `Precision`,NUMERIC_SCALE AS `Scale`, COLUMN_DEFAULT AS `DefaultValue`,COLUMN_COMMENT AS `Comment`,
                        CASE EXTRA WHEN 'auto_increment' THEN 1 ELSE 0 END AS `IsIdentity`,'' AS `TypeOwner`
                        FROM INFORMATION_SCHEMA.`COLUMNS` AS C
                        JOIN INFORMATION_SCHEMA.`TABLES` AS T ON T.`TABLE_NAME`= C.`TABLE_NAME` AND T.TABLE_TYPE='BASE TABLE' AND T.TABLE_SCHEMA=C.TABLE_SCHEMA
                        WHERE C.TABLE_SCHEMA ='{this.ConnectionInfo.Database}'";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND C.TABLE_NAME IN ({ strTableNames })";
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
            //Note:TABLE_SCHEMA of INFORMATION_SCHEMA.KEY_COLUMN_USAGE will improve performance when it's used in where clause, just use CONSTRAINT_SCHEMA in join on clause because it equals to TABLE_SCHEMA.

            string sql = $@"SELECT C.`CONSTRAINT_SCHEMA` AS `Owner`, K.TABLE_NAME AS `TableName`, K.CONSTRAINT_NAME AS `Name`, 
                            K.COLUMN_NAME AS `ColumnName`, K.`ORDINAL_POSITION` AS `Order`, 0 AS `IsDesc`
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K ON C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA AND C.TABLE_NAME = K.TABLE_NAME AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME
                        WHERE C.CONSTRAINT_TYPE = 'PRIMARY KEY'
                        AND K.`TABLE_SCHEMA` ='{this.ConnectionInfo.Database}'";

            if (filter.TableNames != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND C.TABLE_NAME IN ({ strTableNames })";
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
            string sql = $@"SELECT C.`CONSTRAINT_SCHEMA` AS `Owner`, K.TABLE_NAME AS `TableName`, K.CONSTRAINT_NAME AS `Name`, 
                        K.COLUMN_NAME AS `ColumnName`, K.`REFERENCED_TABLE_NAME` AS `ReferencedTableName`,K.`REFERENCED_COLUMN_NAME` AS `ReferencedColumnName`,
                        CASE RC.UPDATE_RULE WHEN 'CASCADE' THEN 1 ELSE 0 END AS `UpdateCascade`, 
                        CASE RC.`DELETE_RULE` WHEN 'CASCADE' THEN 1 ELSE 0 END AS `DeleteCascade`
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K ON C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA AND C.TABLE_NAME = K.TABLE_NAME AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME
                        JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC ON RC.CONSTRAINT_SCHEMA=C.CONSTRAINT_SCHEMA AND RC.CONSTRAINT_NAME=C.CONSTRAINT_NAME AND C.TABLE_NAME=RC.TABLE_NAME                        
                        WHERE C.CONSTRAINT_TYPE = 'FOREIGN KEY'
                        AND K.`TABLE_SCHEMA` ='{this.ConnectionInfo.Database}'";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND C.TABLE_NAME IN ({ strTableNames })";
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
            string sql = $@"SELECT TABLE_SCHEMA AS `Owner`,
	                        TABLE_NAME AS `TableName`,
	                        INDEX_NAME AS `Name`,
	                        COLUMN_NAME AS `ColumnName`,
	                        CASE  NON_UNIQUE WHEN 1 THEN 0 ELSE 1 END AS `IsUnique`,
	                        SEQ_IN_INDEX  AS `Order`,
	                        0 AS `IsDesc`
	                        FROM INFORMATION_SCHEMA.STATISTICS 
	                        WHERE INDEX_NAME NOT IN('PRIMARY', 'FOREIGN')
	                        AND TABLE_SCHEMA = '{this.ConnectionInfo.Database}'";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND TABLE_NAME IN ({ strTableNames })";
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

            string definitionClause = $@"CONVERT(CONCAT('CREATE TRIGGER {this.NotCreateIfExistsCluase} `', TRIGGER_SCHEMA, '`.`', TRIGGER_NAME, '` ', ACTION_TIMING, ' ', EVENT_MANIPULATION, ' ON ', TRIGGER_SCHEMA, '.', EVENT_OBJECT_TABLE, ' FOR EACH ', ACTION_ORIENTATION, '{Environment.NewLine}', ACTION_STATEMENT) USING UTF8)";

            string sql = $@"SELECT TRIGGER_NAME AS `Name`, TRIGGER_SCHEMA AS `Owner`, EVENT_OBJECT_TABLE AS `TableName`, 
                         {(isSimpleMode ? "''" : definitionClause)} AS `Definition`
                        FROM INFORMATION_SCHEMA.`TRIGGERS`
                        WHERE TRIGGER_SCHEMA = '{this.ConnectionInfo.Database}'
                        ";

            if (filter != null)
            {
                if (filter.TableNames != null && filter.TableNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                    sql += $" AND EVENT_OBJECT_TABLE IN ({ strNames })";
                }

                if (filter.TableTriggerNames != null && filter.TableTriggerNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter.TableTriggerNames);
                    sql += $" AND TRIGGER_NAME IN ({ strNames })";
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
            return string.Empty;
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

            string sql = $@"SELECT TABLE_SCHEMA AS `Owner`,TABLE_NAME AS `Name`, {(isSimpleMode ? "''" : createViewClause)} AS `Definition` 
                        FROM INFORMATION_SCHEMA.`VIEWS`
                        WHERE TABLE_SCHEMA = '{this.ConnectionInfo.Database}'";

            if (filter != null && filter.ViewNames != null && filter.ViewNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.ViewNames);
                sql += $" AND TABLE_NAME IN ({ strNames })";
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
        public override async Task SetIdentityEnabled(DbConnection dbConnection, TableColumn column, bool enabled)
        {
            Table table = new Table() { Name = column.TableName, Owner = column.Owner };
            await this.ExecuteNonQueryAsync(dbConnection, $"ALTER TABLE {GetQuotedObjectName(table)} MODIFY COLUMN {ParseColumn(table, column)} {(enabled ? "AUTO_INCREMENT" : "")}", false);
        }

        public override async Task BulkCopyAsync(DbConnection connection, DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            if (dataTable == null || dataTable.Rows.Count <= 0)
            {
                return;
            }

            MySqlBulkCopy bulkCopy = new MySqlBulkCopy(connection as MySqlConnection, bulkCopyInfo.Transaction as MySqlTransaction);

            bulkCopy.DestinationTableName = bulkCopyInfo.DestinationTableName;

            await this.OpenConnectionAsync(connection);

            await bulkCopy.WriteToServerAsync(this.ConvertDataTable(dataTable), bulkCopyInfo.CancellationToken);
        }

        private Dictionary<string, Type> dictSpecialDataType = new Dictionary<string, Type>() { { "SqlHierarchyId", typeof(string) }, { "SqlGeography", typeof(string) } };

        private DataTable ConvertDataTable(DataTable dataTable)
        {
            bool hasSpecialColumn = false;

            foreach (DataColumn column in dataTable.Columns)
            {
                if (this.dictSpecialDataType.Keys.Contains(column.DataType.Name))
                {
                    hasSpecialColumn = true;
                    break;
                }
            }

            if (hasSpecialColumn)
            {
                DataTable dtSpecial = dataTable.Clone();

                foreach (DataColumn column in dtSpecial.Columns)
                {
                    if (this.dictSpecialDataType.Keys.Contains(column.DataType.Name))
                    {
                        column.DataType = this.dictSpecialDataType[column.DataType.Name];
                    }
                }

                foreach (DataRow row in dataTable.Rows)
                {
                    DataRow r = dtSpecial.NewRow();

                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        var value = row[i];

                        if (this.dictSpecialDataType.Keys.Contains(dataTable.Columns[i].DataType.Name))
                        {
                            Type type = this.dictSpecialDataType[dataTable.Columns[i].DataType.Name];

                            r[i] = value == null ? null : (type == typeof(string) ? value?.ToString() : Convert.ChangeType(value, type));
                        }
                        else
                        {
                            r[i] = value;
                        }

                    }

                    dtSpecial.Rows.Add(r);

                    return dtSpecial;
                }
            }

            return dataTable;
        }

        public override async Task SetConstrainsEnabled(bool enabled)
        {
            await this.ExecuteNonQueryAsync(this.GetSqlForSetConstrainsEnabled(enabled));
        }

        public override async Task SetConstrainsEnabled(DbConnection dbConnection, bool enabled)
        {
            await this.ExecuteNonQueryAsync(dbConnection, this.GetSqlForSetConstrainsEnabled(enabled), false);
        }

        private string GetSqlForSetConstrainsEnabled(bool enabled)
        {
            return $"SET FOREIGN_KEY_CHECKS = { (enabled ? 1 : 0)};";
        }

        public override Task Drop<T>(DbConnection dbConnection, T dbObjet)
        {
            string sql = "";

            if (dbObjet is TablePrimaryKey)
            {
                TablePrimaryKey primaryKey = dbObjet as TablePrimaryKey;

                sql = $"ALTER TABLE {this.GetQuotedString(primaryKey.Owner)}.{this.GetQuotedString(primaryKey.TableName)} DROP PRIMARY KEY;";
            }
            if (dbObjet is TableForeignKey)
            {
                TableForeignKey foreignKey = dbObjet as TableForeignKey;

                sql = $"ALTER TABLE {this.GetQuotedString(foreignKey.Owner)}.{this.GetQuotedString(foreignKey.TableName)} DROP FOREIGN KEY {this.GetQuotedString(foreignKey.Name)};";
            }
            else if (dbObjet is TableIndex)
            {
                TableIndex index = dbObjet as TableIndex;

                sql = $"ALTER TABLE {index.Owner}.{this.GetQuotedString(index.TableName)} DROP INDEX {this.GetQuotedString(index.Name)};";
            }
            else
            {
                string typeName = dbObjet.GetType().Name;
                sql = $"DROP { typeName } IF EXISTS {this.GetQuotedObjectName(dbObjet)};";
            }

            return this.ExecuteNonQueryAsync(dbConnection, sql, false);
        }
        #endregion

        #region Generate Schema Scripts 

        public override ScriptBuilder GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            ScriptBuilder sb = new ScriptBuilder();

            #region Function           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<Function>(schemaInfo.Functions));
            #endregion

            #region Create Table
            foreach (Table table in schemaInfo.Tables)
            {
                this.FeedbackInfo(OperationState.Begin, table);

                string tableName = table.Name;
                string quotedTableName = this.GetQuotedObjectName(table);

                IEnumerable<TableColumn> tableColumns = schemaInfo.TableColumns.Where(item => item.TableName == tableName).OrderBy(item => item.Order);

                string primaryKey = "";

                IEnumerable<TablePrimaryKey> primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.TableName == tableName);
                IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.TableName == tableName);
                IEnumerable<TableIndex> indexes = schemaInfo.TableIndexes.Where(item => item.TableName == tableName).OrderBy(item => item.Order);

                this.RestrictColumnLength(tableColumns, primaryKeys);
                this.RestrictColumnLength(tableColumns, foreignKeys);
                this.RestrictColumnLength(tableColumns, indexes);

                #region Primary Key
                if (this.Option.TableScriptsGenerateOption.GeneratePrimaryKey && primaryKeys.Count() > 0)
                {
                    primaryKey =
$@"
,PRIMARY KEY
(
{string.Join(Environment.NewLine, primaryKeys.Select(item => $"{ this.GetQuotedString(item.ColumnName)},")).TrimEnd(',')}
)";
                }
                #endregion

                List<string> foreignKeysLines = new List<string>();

                #region Foreign Key
                if (this.Option.TableScriptsGenerateOption.GenerateForeignKey)
                {
                    if (foreignKeys.Count() > 0)
                    {
                        ILookup<string, TableForeignKey> foreignKeyLookup = foreignKeys.ToLookup(item => item.Name);

                        IEnumerable<string> keyNames = foreignKeyLookup.Select(item => item.Key);

                        foreach (string keyName in keyNames)
                        {
                            TableForeignKey tableForeignKey = foreignKeyLookup[keyName].First();

                            string columnNames = string.Join(",", foreignKeyLookup[keyName].Select(item => this.GetQuotedString(item.ColumnName)));
                            string referenceColumnName = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{ this.GetQuotedString(item.ReferencedColumnName)}"));

                            string line = $"CONSTRAINT { this.GetQuotedString(this.GetRestrictedLengthName(keyName))} FOREIGN KEY ({columnNames}) REFERENCES { this.GetQuotedString(tableForeignKey.ReferencedTableName)}({referenceColumnName})";

                            if (tableForeignKey.UpdateCascade)
                            {
                                line += " ON UPDATE CASCADE";
                            }
                            else
                            {
                                line += " ON UPDATE NO ACTION";
                            }

                            if (tableForeignKey.DeleteCascade)
                            {
                                line += " ON DELETE CASCADE";
                            }
                            else
                            {
                                line += " ON DELETE NO ACTION";
                            }

                            foreignKeysLines.Add(line);
                        }
                    }
                }

                string foreignKey = $"{(foreignKeysLines.Count > 0 ? (Environment.NewLine + "," + string.Join("," + Environment.NewLine, foreignKeysLines)) : "")}";

                #endregion

                #region Table

                string tableScript =
$@"
CREATE TABLE {this.NotCreateIfExistsCluase} {quotedTableName}(
{string.Join("," + Environment.NewLine, tableColumns.Select(item => this.ParseColumn(table, item)))}{primaryKey}{foreignKey}
){(!string.IsNullOrEmpty(table.Comment) ? ($"comment='{this.ReplaceSplitChar(ValueHelper.TransferSingleQuotation(table.Comment))}'") : "")}
DEFAULT CHARSET={DbCharset}" + this.ScriptsDelimiter;

                sb.AppendLine(new CreateDbObjectScript<Table>(tableScript));

                #endregion               

                #region Index
                if (this.Option.TableScriptsGenerateOption.GenerateIndex)
                {
                    if (indexes.Count() > 0)
                    {
                        sb.AppendLine();

                        List<string> indexColumns = new List<string>();

                        ILookup<string, TableIndex> indexLookup = indexes.ToLookup(item => item.Name);
                        IEnumerable<string> indexNames = indexLookup.Select(item => item.Key);

                        foreach (string indexName in indexNames)
                        {
                            TableIndex tableIndex = indexLookup[indexName].First();

                            string columnNames = string.Join(",", indexLookup[indexName].Select(item => $"{this.GetQuotedString(item.ColumnName)}"));

                            if (indexColumns.Contains(columnNames))
                            {
                                continue;
                            }

                            var tempIndexName = tableIndex.Name;
                            if (tempIndexName.Contains("-"))
                            {
                                tempIndexName = tempIndexName.Replace("-", "_");
                            }

                            tempIndexName = this.GetRestrictedLengthName(tempIndexName);

                            sb.AppendLine(new CreateDbObjectScript<TableIndex>($"ALTER TABLE {quotedTableName} ADD {(tableIndex.IsUnique ? "UNIQUE" : "")} INDEX { this.GetQuotedString(tempIndexName)} ({columnNames});"));

                            if (!indexColumns.Contains(columnNames))
                            {
                                indexColumns.Add(columnNames);
                            }
                        }
                    }
                }
                #endregion              

                sb.AppendLine();

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
            string dataType = this.ParseDataType(column);
            bool isChar = DataTypeHelper.IsCharType(dataType.ToLower());

            if (isChar || DataTypeHelper.IsTextType(dataType.ToLower()))
            {
                dataType += $" CHARACTER SET {DbCharset} COLLATE {DbCharsetCollation} ";
            }

            string requiredClause = (column.IsRequired ? "NOT NULL" : "NULL");
            string identityClause = (this.Option.TableScriptsGenerateOption.GenerateIdentity && column.IsIdentity ? $"AUTO_INCREMENT" : "");
            string defaultValueClause = (string.IsNullOrEmpty(column.DefaultValue) ? "" : " DEFAULT " + this.GetColumnDefaultValue(column));
            string commentClause = (!string.IsNullOrEmpty(column.Comment) ? $"comment '{this.ReplaceSplitChar(ValueHelper.TransferSingleQuotation(column.Comment))}'" : "");

            return $@"{this.GetQuotedString(column.Name)} {dataType} {requiredClause} {identityClause} {defaultValueClause} {commentClause}";
        }

        public override string ParseDataType(TableColumn column)
        {
            string dataType = column.DataType;
            bool isChar = DataTypeHelper.IsCharType(dataType.ToLower());

            if (column.DataType.IndexOf("(") < 0)
            {
                if (isChar)
                {
                    dataType = $"{dataType}({column.MaxLength.ToString()})";
                }
                else if (!this.IsNoLengthDataType(dataType))
                {
                    long precision = column.Precision.HasValue ? column.Precision.Value : (column.MaxLength.HasValue ? column.MaxLength.Value : 0);
                    int scale = column.Scale.HasValue ? column.Scale.Value : 0;

                    dataType = $"{dataType}({precision},{scale})";
                }
            }

            return dataType;
        }

        private void RestrictColumnLength<T>(IEnumerable<TableColumn> columns, IEnumerable<T> children) where T : TableColumnChild
        {
            var childColumns = columns.Where(item => children.Any(t => item.Name == t.ColumnName)).ToList();

            childColumns.ForEach(item =>
            {
                if (DataTypeHelper.IsCharType(item.DataType) && item.MaxLength > this.KeyIndexColumnMaxLength)
                {
                    item.MaxLength = this.KeyIndexColumnMaxLength;
                }
            });
        }

        private string GetRestrictedLengthName(string name)
        {
            if (name.Length > this.NameMaxLength)
            {
                return name.Substring(0, this.NameMaxLength);
            }

            return name;
        }

        private bool IsNoLengthDataType(string dataType)
        {
            string[] flags = { "date", "time", "int", "text", "longblob", "longtext", "binary" };

            return flags.Any(item => dataType.ToLower().Contains(item));
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

            var pagedSql = $@"SELECT {columnNames}
							  FROM {tableName}
                             {whereClause} 
                             ORDER BY {(!string.IsNullOrEmpty(orderColumns) ? orderColumns : this.GetDefaultOrder())}
                             LIMIT { startEndRowNumber.StartRowNumber - 1 } , {pageSize}";

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

        protected override string GetBytesConvertHexString(object value, string dataType)
        {
            string hex = string.Concat(((byte[])value).Select(item => item.ToString("X2")));
            return $"UNHEX('{hex}')";
        }       
        #endregion       
    }
}
