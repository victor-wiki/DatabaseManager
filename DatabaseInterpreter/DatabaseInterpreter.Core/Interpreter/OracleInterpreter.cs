using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class OracleInterpreter : DbInterpreter
    {
        #region Field & Property
        public const string SEMICOLON_FUNC = "CHR(59)";
        public const string CONNECT_CHAR = "||";
        public override string UnicodeInsertChar => "";
        public override string CommandParameterChar { get { return ":"; } }
        public override char QuotationLeftChar { get { return '"'; } }
        public override char QuotationRightChar { get { return '"'; } }
        public override DatabaseType DatabaseType { get { return DatabaseType.Oracle; } }
        public override bool SupportBulkCopy { get { return false; } }
        public override List<string> BuiltinDatabases => new List<string> { "SYSTEM", "USERS", "TEMP", "SYSAUX" };
        #endregion

        #region Constructor
        public OracleInterpreter(ConnectionInfo connectionInfo, DbInterpreterOption options) : base(connectionInfo, options)
        {
            this.dbConnector = this.GetDbConnector();           
        }
        #endregion

        #region Common Method  
        public override DbConnector GetDbConnector()
        {
            return new DbConnector(new OracleProvider(), new OracleConnectionBuilder(), this.ConnectionInfo);
        }
        #endregion

        #region Schema Information
        #region Database

        public async Task<string> GetCurrentUserName()
        {
            string sql = "SELECT sys_context('USERENV', 'CURRENT_USER') FROM dual";
            return (await this.GetScalarAsync(this.dbConnector.CreateConnection(), sql))?.ToString();
        }

        private string GetOwner()
        {
            if (this.ConnectionInfo.IntegratedSecurity && (string.IsNullOrEmpty(this.ConnectionInfo.UserId) || this.ConnectionInfo.UserId == "/"))
            {
                try
                {
                    return this.ConnectionInfo.UserId = this.GetCurrentUserName().Result;                    
                }
                catch (Exception ex)
                {
                    this.Feedback(FeedbackInfoType.Error, ExceptionHelper.GetExceptionDetails(ex));
                }
            }

            return this.ConnectionInfo.UserId;
        }

        public override Task<List<Database>> GetDatabasesAsync()
        {
            string notShowBuiltinDatabaseCondition = "";

            if (!this.ShowBuiltinDatabase)
            {
                string strBuiltinDatabase = this.BuiltinDatabases.Count > 0 ? string.Join(",", this.BuiltinDatabases.Select(item => $"'{item}'")) : "";
                notShowBuiltinDatabaseCondition = string.IsNullOrEmpty(strBuiltinDatabase) ? "" : $"AND TABLESPACE_NAME NOT IN({strBuiltinDatabase}) AND CONTENTS <>'UNDO'";
            }

            string sql = $@"SELECT TABLESPACE_NAME AS ""Name"" FROM USER_TABLESPACES WHERE TABLESPACE_NAME IN(SELECT DEFAULT_TABLESPACE FROM USER_USERS WHERE UPPER(USERNAME)=UPPER('{this.GetOwner()}')) {notShowBuiltinDatabaseCondition}";

            return base.GetDbObjectsAsync<Database>(sql);
        }
        #endregion        

        #region User Defined Type       

        public override Task<List<UserDefinedType>> GetUserDefinedTypesAsync(params string[] typeNames)
        {
            return base.GetDbObjectsAsync<UserDefinedType>("");
        }

        public override Task<List<UserDefinedType>> GetUserDefinedTypesAsync(DbConnection dbConnection, params string[] typeNames)
        {
            return base.GetDbObjectsAsync<UserDefinedType>(dbConnection, "");
        }
        #endregion

        #region Function  
        public override Task<List<Function>> GetFunctionsAsync(params string[] functionNames)
        {
            return base.GetDbObjectsAsync<Function>(this.GetSqlForProcedures("FUNCTION", functionNames));
        }

        public override Task<List<Function>> GetFunctionsAsync(DbConnection dbConnection, params string[] functionNames)
        {
            return base.GetDbObjectsAsync<Function>(dbConnection, this.GetSqlForProcedures("FUNCTION", functionNames));
        }

        private string GetSqlForProcedures(string type, params string[] objectNames)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = "";

            string ownerCondition = $" AND UPPER(P.OWNER) = UPPER('{this.GetOwner()}')";
            string nameCondition = "";

            if (objectNames != null && objectNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(objectNames);
                nameCondition = $" AND P.OBJECT_NAME IN ({ strNames })";
            }

            if (isSimpleMode)
            {
                sql = $@"SELECT P.OBJECT_NAME AS ""Name"", P.OWNER AS ""Owner""
                         FROM ALL_PROCEDURES P 
                         WHERE P.OBJECT_TYPE='FUNCTION' {ownerCondition}
                         {nameCondition}";
            }
            else
            {
                sql = $@"SELECT S.NAME AS ""Name"", P.OWNER AS ""Owner"", LISTAGG(TEXT,'') WITHIN GROUP(ORDER BY LINE) ""Definition""
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
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = "";
            string tablespaceCondition = string.IsNullOrEmpty(this.ConnectionInfo.Database) ? "" : $" AND UPPER(T.TABLESPACE_NAME)=UPPER('{this.ConnectionInfo.Database}')";

            if (isSimpleMode)
            {
                sql = $@"SELECT T.OWNER AS ""Owner"", T.TABLE_NAME AS ""Name""
                         FROM ALL_TABLES T 
                        ";
            }
            else
            {
                sql = $@"SELECT T.OWNER AS ""Owner"", T.TABLE_NAME AS ""Name"", C.COMMENTS AS ""Comment"",
                          1 AS ""IdentitySeed"", 1 AS ""IdentityIncrement""
                          FROM ALL_TABLES T
                          LEFT JOIN USER_TAB_COMMENTS C ON T.TABLE_NAME= C.TABLE_NAME
                     ";
            }

            sql += $" WHERE UPPER(OWNER)=UPPER('{this.GetOwner()}')" + tablespaceCondition;

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND T.TABLE_NAME IN ({ strTableNames })";
            }

            sql += " ORDER BY T.TABLE_NAME";

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
            string sql = $@"SELECT OWNER AS ""Owner"", C.TABLE_NAME AS ""TableName"",C.COLUMN_NAME AS ""Name"",DATA_TYPE AS ""DataType"",CASE NULLABLE WHEN 'Y' THEN 1 ELSE 0 END AS ""IsNullable"", DATA_LENGTH AS ""MaxLength"",
                 DATA_PRECISION AS ""Precision"",DATA_SCALE AS ""Scale"", COLUMN_ID AS ""Order"", DATA_DEFAULT AS ""DefaultValue"", 0 AS ""IsIdentity"", CC.COMMENTS AS ""Comment"" , '' AS ""TypeOwner""
                 FROM ALL_TAB_COLUMNS C
                 LEFT JOIN USER_COL_COMMENTS CC ON C.TABLE_NAME=CC.TABLE_NAME AND C.COLUMN_NAME=CC.COLUMN_NAME
                 WHERE UPPER(OWNER)=UPPER('{this.GetOwner()}')";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND C.TABLE_NAME IN ({ strTableNames })";
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
            string sql = $@"SELECT UC.OWNER AS ""Owner"", UC.TABLE_NAME AS ""TableName"",UC.CONSTRAINT_NAME AS ""Name"",UCC.COLUMN_NAME AS ""ColumnName"", UCC.POSITION AS ""Order"", 0 AS ""IsDesc""
                        FROM USER_CONSTRAINTS UC
                        JOIN USER_CONS_COLUMNS UCC ON UC.OWNER=UCC.OWNER AND UC.TABLE_NAME=UCC.TABLE_NAME AND UC.CONSTRAINT_NAME=UCC.CONSTRAINT_NAME  
                        WHERE UC.CONSTRAINT_TYPE='P' AND UPPER(UC.OWNER)=UPPER('{this.GetOwner()}')";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND UC.TABLE_NAME IN ({ strTableNames })";
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
            string sql = $@"SELECT UC.OWNER AS ""Owner"", UC.TABLE_NAME AS ""TableName"", UC.CONSTRAINT_NAME AS ""Name"", UCC.column_name AS ""ColumnName"",
                        RUCC.TABLE_NAME AS ""ReferencedTableName"",RUCC.COLUMN_NAME AS ""ReferencedColumnName"",
                        0 AS ""UpdateCascade"", CASE UC.DELETE_RULE WHEN 'CASCADE' THEN 1 ELSE 0 END AS ""DeleteCascade"" 
                        FROM USER_CONSTRAINTS UC                       
                        JOIN USER_CONS_COLUMNS UCC ON UC.OWNER=UCC.OWNER AND UC.TABLE_NAME=UCC.TABLE_NAME AND UC.CONSTRAINT_NAME=UCC.CONSTRAINT_NAME                       
                        JOIN USER_CONS_COLUMNS RUCC ON UC.OWNER=RUCC.OWNER AND UC.R_CONSTRAINT_NAME=RUCC.CONSTRAINT_NAME AND UCC.POSITION=RUCC.POSITION
                        WHERE UC.CONSTRAINT_TYPE='R' AND UPPER(UC.OWNER)=UPPER('{this.GetOwner()}')";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND UC.TABLE_NAME IN ({ strTableNames })";
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
            string sql = $@"SELECT UC.owner AS ""Owner"", ui.table_name AS ""TableName"", ui.index_name AS ""Name"", uic.column_name AS ""ColumnName"", uic.column_position AS ""Order"",
                CASE uic.descend WHEN 'ASC' THEN 0 ELSE 1 END AS ""IsDesc"", CASE ui.uniqueness WHEN 'UNIQUE' THEN 1 ELSE 0 END AS ""IsUnique""
                FROM user_indexes ui
                JOIN user_ind_columns uic ON ui.index_name = uic.index_name AND ui.table_name = uic.table_name
                LEFT JOIN user_constraints uc ON ui.table_name = uc.table_name AND ui.table_owner = uc.owner AND ui.index_name = uc.constraint_name AND uc.constraint_type = 'P'
                WHERE uc.constraint_name IS NULL AND UPPER(UC.owner)=UPPER('{this.GetOwner()}')";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND UC.TABLE_NAME IN ({ strTableNames })";
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

            string sql = $@"SELECT V.OWNER AS ""Owner"", V.VIEW_NAME AS ""Name"", {(isSimpleMode ? "''" : "TEXT_VC")} AS ""Definition"" 
                        FROM ALL_VIEWS V
                        WHERE UPPER(OWNER) = UPPER('{this.GetOwner()}')";

            if (viewNames != null && viewNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(viewNames);
                sql += $" AND V.VIEW_NAME IN ({ strNames })";
            }

            sql += " ORDER BY VIEW_NAME";

            return sql;
        }

        #endregion

        #region Trigger      
        public override Task<List<Trigger>> GetTriggersAsync(params string[] triggerNames)
        {
            return base.GetDbObjectsAsync<Trigger>(this.GetSqlForTriggers(triggerNames));
        }

        public override Task<List<Trigger>> GetTriggersAsync(DbConnection dbConnection, params string[] triggerNames)
        {
            return base.GetDbObjectsAsync<Trigger>(dbConnection, this.GetSqlForTriggers(triggerNames));
        }

        private string GetSqlForTriggers(params string[] triggerNames)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = $@"SELECT TRIGGER_NAME AS ""Name"",TABLE_OWNER AS ""Owner"", TABLE_NAME AS ""TableName"", 
                         { (isSimpleMode ? "''" : "TRIGGER_BODY")} AS ""Definition""
                        FROM USER_TRIGGERS
                        WHERE UPPER(TABLE_OWNER) = UPPER('{this.GetOwner()}')
                        ";

            if (triggerNames != null && triggerNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(triggerNames);
                sql += $" AND TRIGGER_NAME IN ({ strNames })";
            }

            sql += " ORDER BY TRIGGER_NAME";

            return sql;
        }
        #endregion

        #region Procedure     

        public override Task<List<Procedure>> GetProceduresAsync(params string[] procedureNames)
        {
            return base.GetDbObjectsAsync<Procedure>(this.GetSqlForProcedures("PROCEDURE", procedureNames));
        }

        public override Task<List<Procedure>> GetProceduresAsync(DbConnection dbConnection, params string[] procedureNames)
        {
            return base.GetDbObjectsAsync<Procedure>(dbConnection, this.GetSqlForProcedures("PROCEDURE", procedureNames));
        }
        #endregion
        #endregion

        #region Database Operation
        public override Task<int> BulkCopyAsync(DbConnection connection, DataTable dataTable, string destinationTableName = null, int? bulkCopyTimeout = null, int? batchSize = null)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Generate Schema Script 

        public override string GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            StringBuilder sb = new StringBuilder();

            #region Function           
            sb.Append(this.GenerateScriptDbObjectScripts<Function>(schemaInfo.Functions));
            #endregion

            #region Table
            foreach (Table table in schemaInfo.Tables)
            {
                this.FeedbackInfo(OperationState.Begin, table);

                string tableName = table.Name;
                string quotedTableName = this.GetQuotedObjectName(table);

                IEnumerable<TableColumn> tableColumns = schemaInfo.TableColumns.Where(item => item.TableName == tableName).OrderBy(item => item.Order);

                IEnumerable<TablePrimaryKey> primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.TableName == tableName);

                #region Create Table

                sb.AppendLine(
$@"
CREATE TABLE {quotedTableName}(
{string.Join("," + Environment.NewLine, tableColumns.Select(item => this.ParseColumn(table, item))).TrimEnd(',')}
)
TABLESPACE
{this.ConnectionInfo.Database}" + this.ScriptsSplitString);
                #endregion

                sb.AppendLine();

                #region Comment
                if (!string.IsNullOrEmpty(table.Comment))
                {
                    sb.AppendLine($"COMMENT ON TABLE {this.GetOwner()}.{GetQuotedString(tableName)} IS '{ValueHelper.TransferSingleQuotation(table.Comment)}'" + this.ScriptsSplitString);
                }

                foreach (TableColumn column in tableColumns.Where(item => !string.IsNullOrEmpty(item.Comment)))
                {
                    sb.AppendLine($"COMMENT ON COLUMN {this.GetOwner()}.{GetQuotedString(tableName)}.{GetQuotedString(column.Name)} IS '{ValueHelper.TransferSingleQuotation(column.Comment)}'" + this.ScriptsSplitString);
                }
                #endregion

                #region Primary Key
                if (this.Option.GenerateKey && primaryKeys.Count() > 0)
                {
                    string primaryKey =
$@"
ALTER TABLE {quotedTableName} ADD CONSTRAINT {this.GetQuotedString(primaryKeys.FirstOrDefault().Name)} PRIMARY KEY 
(
{string.Join(Environment.NewLine, primaryKeys.Select(item => $"{ this.GetQuotedString(item.ColumnName)},")).TrimEnd(',')}
)
USING INDEX 
TABLESPACE
{this.ConnectionInfo.Database}{this.ScriptsSplitString}";

                    sb.AppendLine(primaryKey);
                }
                #endregion

                #region Foreign Key
                if (this.Option.GenerateKey)
                {
                    IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.TableName == tableName);
                    if (foreignKeys.Count() > 0)
                    {
                        sb.AppendLine();
                        ILookup<string, TableForeignKey> foreignKeyLookup = foreignKeys.ToLookup(item => item.Name);

                        IEnumerable<string> keyNames = foreignKeyLookup.Select(item => item.Key);

                        foreach (string keyName in keyNames)
                        {
                            TableForeignKey tableForeignKey = foreignKeyLookup[keyName].First();

                            string columnNames = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{ this.GetQuotedString(item.ColumnName)}"));
                            string referenceColumnName = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{ this.GetQuotedString(item.ReferencedColumnName)}"));

                            sb.AppendLine(
$@"
ALTER TABLE {quotedTableName} ADD CONSTRAINT { this.GetQuotedString(keyName)} FOREIGN KEY ({columnNames})
REFERENCES { this.GetQuotedString(tableForeignKey.ReferencedTableName)}({referenceColumnName})");

                            if (tableForeignKey.DeleteCascade)
                            {
                                sb.AppendLine("ON DELETE CASCADE");
                            }

                            sb.Append(this.ScriptsSplitString);
                        }
                    }
                }
                #endregion

                #region Index
                if (this.Option.GenerateIndex)
                {
                    IEnumerable<TableIndex> indices = schemaInfo.TableIndexes.Where(item => item.TableName == tableName).OrderBy(item => item.Order);
                    if (indices.Count() > 0)
                    {
                        sb.AppendLine();

                        List<string> indexColumns = new List<string>();

                        ILookup<string, TableIndex> indexLookup = indices.ToLookup(item => item.Name);
                        IEnumerable<string> indexNames = indexLookup.Select(item => item.Key);
                        foreach (string indexName in indexNames)
                        {
                            TableIndex tableIndex = indexLookup[indexName].First();

                            string columnNames = string.Join(",", indexLookup[indexName].Select(item => $"{ this.GetQuotedString(item.ColumnName)}"));

                            if (indexColumns.Contains(columnNames))
                            {
                                continue;
                            }

                            sb.AppendLine($"CREATE {(tableIndex.IsUnique ? "UNIQUE" : "")} INDEX { this.GetQuotedString(tableIndex.Name)} ON { this.GetQuotedString(tableName)} ({columnNames})" + this.ScriptsSplitString);

                            if (!indexColumns.Contains(columnNames))
                            {
                                indexColumns.Add(columnNames);
                            }
                        }
                    }
                }
                #endregion

                //#region Default Value
                //if (options.GenerateDefaultValue)
                //{
                //    IEnumerable<TableColumn> defaultValueColumns = columns.Where(item => item.TableName == tableName && !string.IsNullOrEmpty(item.DefaultValue));
                //    foreach (TableColumn column in defaultValueColumns)
                //    {
                //        sb.AppendLine($"ALTER TABLE \"{tableName}\" MODIFY \"{column.ColumnName}\" DEFAULT {column.DefaultValue};");
                //    }
                //}
                //#endregion

                this.FeedbackInfo(OperationState.End, table);
            }
            #endregion

            #region View           
            sb.Append(this.GenerateScriptDbObjectScripts<View>(schemaInfo.Views));
            #endregion

            #region Trigger           
            sb.Append(this.GenerateScriptDbObjectScripts<Trigger>(schemaInfo.Triggers));
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
            bool isChar = column.DataType.ToLower().IndexOf("char") >= 0;
            string dataType = column.DataType;
            if (column.DataType.IndexOf("(") < 0)
            {
                if (isChar)
                {
                    long? dataLength = column.MaxLength;
                    if (dataLength > 0 && dataType.StartsWith("n"))
                    {
                        dataLength = dataLength / 2;
                    }

                    dataType = $"{dataType}({dataLength.ToString()})";
                }
                else if (!this.IsNoLengthDataType(dataType))
                {
                    dataType = $"{dataType}";
                    if (!(column.Precision == 0 && column.Scale == 0))
                    {
                        long precision = column.Precision.HasValue ? column.Precision.Value : column.MaxLength.Value;
                        int scale = column.Scale.HasValue ? column.Scale.Value : 0;

                        if (dataType == "raw")
                        {
                            dataType = $"{dataType}({precision})";
                        }
                        else
                        {
                            dataType = $"{dataType}({precision},{scale})";
                        }
                    }
                    else if (column.MaxLength > 0)
                    {
                        dataType += $"({column.MaxLength})";
                    }
                }
            }

            return $@"{ this.GetQuotedString(column.Name)} {dataType} {(string.IsNullOrEmpty(column.DefaultValue) ? "" : " DEFAULT " + this.GetColumnDefaultValue(column))} {(column.IsRequired ? "NOT NULL" : "NULL")}";
        }

        private bool IsNoLengthDataType(string dataType)
        {
            string[] flags = { "date", "time", "int", "text", "clob", "blob", "binary_double" };

            return flags.Any(item => dataType.ToLower().Contains(item));
        }

        #endregion

        #region Generate Data Script
        public override long GetTableRecordCount(DbConnection connection, Table table)
        {
            string sql = $@"SELECT COUNT(1) FROM {this.GetOwner()}.{ this.GetQuotedString(table.Name)}";

            return base.GetTableRecordCount(connection, sql);
        }

        public override Task<long> GetTableRecordCountAsync(DbConnection connection, Table table)
        {
            string sql = $@"SELECT COUNT(1) FROM {this.GetOwner()}.{ this.GetQuotedString(table.Name)}";

            return base.GetTableRecordCountAsync(connection, sql);
        }

        public override async Task<string> GenerateDataScriptsAsync(SchemaInfo schemaInfo)
        {
            return await base.GenerateDataScriptsAsync(schemaInfo);
        }

        protected override string GetBatchInsertPrefix()
        {
            return "INSERT ALL INTO";
        }

        protected override string GetBatchInsertItemBefore(string tableName, bool isFirstRow)
        {
            return isFirstRow ? "" : $"INTO {tableName} VALUES";
        }

        protected override string GetBatchInsertItemEnd(bool isAllEnd)
        {
            return (isAllEnd ? $"{Environment.NewLine}SELECT 1 FROM DUAL;" : "");
        }

        protected override string GetPagedSql(string tableName, string columnNames, string primaryKeyColumns, string whereClause, long pageNumber, int pageSize)
        {
            var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

            string pagedSql = $@"with PagedRecords as
								(
									SELECT {columnNames}, ROW_NUMBER() OVER (ORDER BY (SELECT 0 FROM DUAL)) AS ROWNUMBER
									FROM {tableName}
                                    {whereClause}
								)
								SELECT *
								FROM PagedRecords
								WHERE ROWNUMBER BETWEEN {startEndRowNumber.StartRowNumber} AND {startEndRowNumber.EndRowNumber}";

            return pagedSql;
        }

        protected override bool NeedInsertParameter(object value)
        {
            if (value != null && value.GetType() == typeof(string))
            {
                string str = value.ToString();
                if (str.Length > 4000 || (str.Contains(SEMICOLON_FUNC) && str.Length > 2000))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
