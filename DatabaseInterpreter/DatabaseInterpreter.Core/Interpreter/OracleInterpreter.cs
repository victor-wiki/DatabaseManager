using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class OracleInterpreter : DbInterpreter
    {
        #region Field & Property
        private string dbOwner;
        public const string SEMICOLON_FUNC = "CHR(59)";
        public const string CONNECT_CHAR = "||";
        public override string UnicodeInsertChar => "";
        public override string CommandParameterChar { get { return ":"; } }
        public override char QuotationLeftChar { get { return '"'; } }
        public override char QuotationRightChar { get { return '"'; } }
        public override DatabaseType DatabaseType { get { return DatabaseType.Oracle; } }
        public override bool SupportBulkCopy { get { return true; } }
        public override List<string> BuiltinDatabases => new List<string> { "SYSTEM", "USERS", "TEMP", "SYSAUX" };
        #endregion

        #region Constructor
        public OracleInterpreter(ConnectionInfo connectionInfo, DbInterpreterOption option) : base(connectionInfo, option)
        {
            this.dbOwner = connectionInfo.UserId;
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
            return (await this.GetScalarAsync(this.CreateConnection(), sql))?.ToString();
        }

        private string GetDbOwner()
        {
            if (this.ConnectionInfo.IntegratedSecurity && (string.IsNullOrEmpty(this.dbOwner) || this.dbOwner == "/"))
            {
                try
                {
                    return this.dbOwner = this.GetCurrentUserName().Result;
                }
                catch (Exception ex)
                {
                    this.Feedback(FeedbackInfoType.Error, ExceptionHelper.GetExceptionDetails(ex));
                }
            }

            return this.dbOwner;
        }

        public override Task<List<Database>> GetDatabasesAsync()
        {
            string notShowBuiltinDatabaseCondition = "";

            if (!this.ShowBuiltinDatabase)
            {
                string strBuiltinDatabase = this.BuiltinDatabases.Count > 0 ? string.Join(",", this.BuiltinDatabases.Select(item => $"'{item}'")) : "";
                notShowBuiltinDatabaseCondition = string.IsNullOrEmpty(strBuiltinDatabase) ? "" : $"AND TABLESPACE_NAME NOT IN({strBuiltinDatabase}) AND CONTENTS <>'UNDO'";
            }

            string sql = $@"SELECT TABLESPACE_NAME AS ""Name"" FROM USER_TABLESPACES WHERE TABLESPACE_NAME IN(SELECT DEFAULT_TABLESPACE FROM USER_USERS WHERE UPPER(USERNAME)=UPPER('{this.GetDbOwner()}')) {notShowBuiltinDatabaseCondition}";

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
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            string sql = "";

            if (isSimpleMode)
            {
                sql = $@"SELECT T.OWNER AS ""Owner"",T.TYPE_NAME AS ""Name""
                        FROM ALL_TYPES T";
            }
            else
            {
                sql = $@"SELECT T.OWNER AS ""Owner"",T.TYPE_NAME AS ""Name"",TA.ATTR_NAME AS ""AttrName"", TA.ATTR_TYPE_NAME AS ""DataType"",TA.LENGTH AS ""MaxLength"",TA.PRECISION AS ""Precision"",TA.SCALE AS ""Scale""
                        FROM ALL_TYPES T
                        JOIN ALL_TYPE_ATTRS TA ON T.OWNER = TA.OWNER AND T.TYPE_NAME = TA.TYPE_NAME
                      ";
            }

            sql += $" WHERE UPPER(T.OWNER)=UPPER('{this.GetDbOwner()}')";

            if (filter != null && filter.UserDefinedTypeNames != null && filter.UserDefinedTypeNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.UserDefinedTypeNames);
                sql += $" AND T.TYPE_NAME IN ({ strNames })";
            }

            return sql;
        }
        #endregion

        #region Function  
        public override Task<List<Function>> GetFunctionsAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Function>(this.GetSqlForProcedures("FUNCTION", filter));
        }

        public override Task<List<Function>> GetFunctionsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Function>(dbConnection, this.GetSqlForProcedures("FUNCTION", filter));
        }

        private string GetSqlForProcedures(string type, SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            bool isFunction = type.ToUpper() == "FUNCTION";
            string sql = "";

            string ownerCondition = $" AND UPPER(P.OWNER) = UPPER('{this.GetDbOwner()}')";
            string nameCondition = "";
            string[] objectNames = isFunction ? filter?.FunctionNames : filter?.ProcedureNames;

            if (objectNames != null && objectNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(objectNames);
                nameCondition = $" AND P.OBJECT_NAME IN ({ strNames })";
            }

            if (isSimpleMode)
            {
                sql = $@"SELECT P.OBJECT_NAME AS ""Name"", P.OWNER AS ""Owner""
                         FROM ALL_PROCEDURES P 
                         WHERE P.OBJECT_TYPE='{type}' {ownerCondition}
                         {nameCondition}";
            }
            else
            {
                sql = $@"SELECT S.NAME AS ""Name"", P.OWNER AS ""Owner"", 'CREATE OR REPLACE ' || LISTAGG(TEXT,'') WITHIN GROUP(ORDER BY LINE) ""Definition""
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

            sql += $" WHERE UPPER(OWNER)=UPPER('{this.GetDbOwner()}')" + tablespaceCondition;

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND T.TABLE_NAME IN ({ strTableNames })";
            }

            sql += " ORDER BY T.TABLE_NAME";

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
            string sql = $@"SELECT OWNER AS ""Owner"", C.TABLE_NAME AS ""TableName"",C.COLUMN_NAME AS ""Name"",DATA_TYPE AS ""DataType"",CASE NULLABLE WHEN 'Y' THEN 1 ELSE 0 END AS ""IsNullable"", DATA_LENGTH AS ""MaxLength"",
                 DATA_PRECISION AS ""Precision"",DATA_SCALE AS ""Scale"", COLUMN_ID AS ""Order"", DATA_DEFAULT AS ""DefaultValue"", 0 AS ""IsIdentity"", CC.COMMENTS AS ""Comment"" , '' AS ""TypeOwner""
                 FROM ALL_TAB_COLUMNS C
                 LEFT JOIN USER_COL_COMMENTS CC ON C.TABLE_NAME=CC.TABLE_NAME AND C.COLUMN_NAME=CC.COLUMN_NAME
                 WHERE UPPER(OWNER)=UPPER('{this.GetDbOwner()}')";

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
            string sql = $@"SELECT UC.OWNER AS ""Owner"", UC.TABLE_NAME AS ""TableName"",UC.CONSTRAINT_NAME AS ""Name"",UCC.COLUMN_NAME AS ""ColumnName"", UCC.POSITION AS ""Order"", 0 AS ""IsDesc""
                        FROM USER_CONSTRAINTS UC
                        JOIN USER_CONS_COLUMNS UCC ON UC.OWNER=UCC.OWNER AND UC.TABLE_NAME=UCC.TABLE_NAME AND UC.CONSTRAINT_NAME=UCC.CONSTRAINT_NAME  
                        WHERE UC.CONSTRAINT_TYPE='P' AND UPPER(UC.OWNER)=UPPER('{this.GetDbOwner()}')";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND UC.TABLE_NAME IN ({ strTableNames })";
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
            string sql = $@"SELECT UC.OWNER AS ""Owner"", UC.TABLE_NAME AS ""TableName"", UC.CONSTRAINT_NAME AS ""Name"", UCC.column_name AS ""ColumnName"",
                        RUCC.TABLE_NAME AS ""ReferencedTableName"",RUCC.COLUMN_NAME AS ""ReferencedColumnName"",
                        0 AS ""UpdateCascade"", CASE UC.DELETE_RULE WHEN 'CASCADE' THEN 1 ELSE 0 END AS ""DeleteCascade"" 
                        FROM USER_CONSTRAINTS UC                       
                        JOIN USER_CONS_COLUMNS UCC ON UC.OWNER=UCC.OWNER AND UC.TABLE_NAME=UCC.TABLE_NAME AND UC.CONSTRAINT_NAME=UCC.CONSTRAINT_NAME                       
                        JOIN USER_CONS_COLUMNS RUCC ON UC.OWNER=RUCC.OWNER AND UC.R_CONSTRAINT_NAME=RUCC.CONSTRAINT_NAME AND UCC.POSITION=RUCC.POSITION
                        WHERE UC.CONSTRAINT_TYPE='R' AND UPPER(UC.OWNER)=UPPER('{this.GetDbOwner()}')";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND UC.TABLE_NAME IN ({ strTableNames })";
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
            string sql = $@"SELECT UC.owner AS ""Owner"", ui.table_name AS ""TableName"", ui.index_name AS ""Name"", uic.column_name AS ""ColumnName"", uic.column_position AS ""Order"",
                CASE uic.descend WHEN 'ASC' THEN 0 ELSE 1 END AS ""IsDesc"", CASE ui.uniqueness WHEN 'UNIQUE' THEN 1 ELSE 0 END AS ""IsUnique""
                FROM user_indexes ui
                JOIN user_ind_columns uic ON ui.index_name = uic.index_name AND ui.table_name = uic.table_name
                LEFT JOIN user_constraints uc ON ui.table_name = uc.table_name AND ui.table_owner = uc.owner AND ui.index_name = uc.constraint_name AND uc.constraint_type = 'P'
                WHERE uc.constraint_name IS NULL AND UPPER(UC.owner)=UPPER('{this.GetDbOwner()}')";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND UC.TABLE_NAME IN ({ strTableNames })";
            }

            return sql;
        }
        #endregion

        #region Table Trigger      
        public override Task<List<TableTrigger>> GetTableTriggersAsync(SchemaInfoFilter filter = null)
        {
            return this.SetTriggerDefinition(base.GetDbObjectsAsync<TableTrigger>(this.GetSqlForTableTriggers(filter)));
        }

        public override Task<List<TableTrigger>> GetTableTriggersAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return this.SetTriggerDefinition(base.GetDbObjectsAsync<TableTrigger>(dbConnection, this.GetSqlForTableTriggers(filter)));
        }

        private Task<List<TableTrigger>> SetTriggerDefinition(Task<List<TableTrigger>> tableTriggers)
        {
            foreach(TableTrigger trigger in tableTriggers.Result)
            {
                trigger.Definition = trigger.CreateClause + trigger.Definition;
            }

            return tableTriggers;
        }

        private string GetSqlForTableTriggers(SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = $@"SELECT TRIGGER_NAME AS ""Name"",TABLE_OWNER AS ""Owner"", TABLE_NAME AS ""TableName"", 
                         { (isSimpleMode ? "''" : "('CREATE OR REPLACE TRIGGER ' || DESCRIPTION)")} AS ""CreateClause"",
                         { (isSimpleMode ? "''" : "TRIGGER_BODY")} AS ""Definition""
                        FROM USER_TRIGGERS
                        WHERE UPPER(TABLE_OWNER) = UPPER('{this.GetDbOwner()}')
                        ";

            if (filter != null)
            {
                if (filter.TableNames != null && filter.TableNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                    sql += $" AND TABLE_NAME IN ({ strNames })";
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
            string sql = $@"SELECT OWNER AS ""Owner"", CONSTRAINT_NAME AS ""Name"", TABLE_NAME AS ""TableName"", SEARCH_CONDITION_VC AS ""Definition""
                         FROM ALL_CONSTRAINTS C
                         WHERE CONSTRAINT_TYPE = 'C' AND GENERATED = 'USER NAME'
                         ";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND TABLE_NAME IN ({ strNames })";
            }

            sql += " ORDER BY CONSTRAINT_NAME";

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

            string sql = $@"SELECT V.OWNER AS ""Owner"", V.VIEW_NAME AS ""Name"", {(isSimpleMode ? "''" : "'CREATE OR REPLACE VIEW '||V.VIEW_NAME||' AS ' || CHR(13) || TEXT_VC")} AS ""Definition"" 
                        FROM ALL_VIEWS V
                        WHERE UPPER(OWNER) = UPPER('{this.GetDbOwner()}')";

            if (filter != null && filter.ViewNames != null && filter.ViewNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.ViewNames);
                sql += $" AND V.VIEW_NAME IN ({ strNames })";
            }

            sql += " ORDER BY VIEW_NAME";

            return sql;
        }

        #endregion        

        #region Procedure     

        public override Task<List<Procedure>> GetProceduresAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Procedure>(this.GetSqlForProcedures("PROCEDURE", filter));
        }

        public override Task<List<Procedure>> GetProceduresAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Procedure>(dbConnection, this.GetSqlForProcedures("PROCEDURE", filter));
        }
        #endregion
        #endregion

        #region Database Operation
        public override async Task BulkCopyAsync(DbConnection connection, DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            if (!(connection is OracleConnection conn))
            {
                return;
            }

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            using (var bulkCopy = new OracleBulkCopy(conn, bulkCopyInfo.Transaction as OracleTransaction))
            {               
                bulkCopy.BatchSize = dataTable.Rows.Count;
                bulkCopy.DestinationTableName = this.GetQuotedString(bulkCopyInfo.DestinationTableName);
                bulkCopy.BulkCopyTimeout = bulkCopyInfo.Timeout.HasValue ? bulkCopyInfo.Timeout.Value : SettingManager.Setting.CommandTimeout; ;
                bulkCopy.ColumnNameNeedQuoted = this.DbObjectNameMode == DbObjectNameMode.WithQuotation;

                await bulkCopy.WriteToServerAsync(dataTable);
            }
        }

        public override async Task SetConstrainsEnabled(bool enabled)
        {
            using (DbConnection connection = this.CreateConnection())
            {
                await this.SetConstrainsEnabled(connection, enabled);
            }
        }

        public override async Task SetConstrainsEnabled(DbConnection dbConnection, bool enabled)
        {
            List<string> sqls = new List<string>() { this.GetSqlForEnableConstraints(enabled), this.GetSqlForEnableTrigger(enabled) };
            List<string> cmds = new List<string>();

            foreach (string sql in sqls)
            {
                DbDataReader reader = await this.GetDataReaderAsync(dbConnection, sql);

                while (reader.Read())
                {
                    string cmd = reader[0].ToString();
                    cmds.Add(cmd);
                }
            }

            foreach (string cmd in cmds)
            {
                await this.ExecuteNonQueryAsync(dbConnection, cmd, false);
            }
        }

        private string GetSqlForEnableConstraints(bool enabled)
        {
            return $@"SELECT 'ALTER TABLE ""'|| T.TABLE_NAME ||'"" {(enabled ? "ENABLE" : "DISABLE")} CONSTRAINT ""'||T.CONSTRAINT_NAME || '""' AS ""SQL""  
                            FROM USER_CONSTRAINTS T 
                            WHERE T.CONSTRAINT_TYPE = 'R'
                            AND UPPER(OWNER)= UPPER('{this.GetDbOwner()}')
                           ";
        }

        private string GetSqlForEnableTrigger(bool enabled)
        {
            return $@"SELECT 'ALTER TRIGGER ""'|| TRIGGER_NAME || '"" {(enabled ? "ENABLE" : "DISABLE")} '
                         FROM USER_TRIGGERS
                         WHERE UPPER(TABLE_OWNER)= UPPER('{this.GetDbOwner()}')";
        }

        public override Task Drop<T>(DbConnection dbConnection, T dbObjet)
        {
            string sql = "";

            if (dbObjet is TableColumnChild || dbObjet is TableConstraint)
            {
                TableChild dbObj = dbObjet as TableChild;

                sql = $"ALTER TABLE {this.GetQuotedString(dbObj.Owner)}.{this.GetQuotedString(dbObj.TableName)} DROP CONSTRAINT {this.GetQuotedString(dbObj.Name)}";
            }
            else if (dbObjet is TableIndex)
            {
                sql = $"DROP INDEX {this.GetQuotedObjectName(dbObjet)}";
            }
            else
            {
                string typeName = dbObjet.GetType().Name;
                sql = $"DROP { typeName } {this.GetQuotedObjectName(dbObjet)}";
            }

            return this.ExecuteNonQueryAsync(dbConnection, sql, false);
        }
        #endregion

        #region Generate Schema Script 

        public override ScriptBuilder GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            ScriptBuilder sb = new ScriptBuilder();

            #region User Defined Type

            //List<string> userTypeNames = schemaInfo.UserDefinedTypes.Select(item => item.Name).Distinct().ToList();

            //foreach (string userTypeName in userTypeNames)
            //{
            //    IEnumerable<UserDefinedType> userTypes = schemaInfo.UserDefinedTypes.Where(item => item.Name == userTypeName);

            //    this.FeedbackInfo(OperationState.Begin, userTypes.First());

            //    string dataTypes = string.Join(",", userTypes.Select(item => $"{item.AttrName} {this.ParseDataType(new TableColumn() { MaxLength = item.MaxLength, DataType = item.Type, Precision = item.Precision, Scale = item.Scale })}"));

            //    string script = $"CREATE TYPE {this.GetQuotedString(userTypeName)} AS OBJECT ({dataTypes})" + this.ScriptsSplitString;

            //    sb.AppendLine(new CreateDbObjectScript<UserDefinedType>(script));

            //    this.FeedbackInfo(OperationState.End, userTypes.First());
            //}

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

                IEnumerable<TableColumn> tableColumns = schemaInfo.TableColumns.Where(item => item.TableName == tableName).OrderBy(item => item.Order);

                IEnumerable<TablePrimaryKey> primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.TableName == tableName);

                #region Create Table

                string tableScript =
$@"
CREATE TABLE {quotedTableName}(
{string.Join("," + Environment.NewLine, tableColumns.Select(item => this.ParseColumn(table, item))).TrimEnd(',')}
)
TABLESPACE
{this.ConnectionInfo.Database}" + this.ScriptsSplitString;

                sb.AppendLine(new CreateDbObjectScript<Table>(tableScript));

                #endregion

                sb.AppendLine();

                #region Comment
                if (!string.IsNullOrEmpty(table.Comment))
                {
                    sb.AppendLine(new AlterDbObjectScript<Table>($"COMMENT ON TABLE {this.GetDbOwner()}.{this.GetQuotedString(tableName)} IS '{this.ReplaceSplitChar(ValueHelper.TransferSingleQuotation(table.Comment))}'" + this.ScriptsSplitString));
                }

                foreach (TableColumn column in tableColumns.Where(item => !string.IsNullOrEmpty(item.Comment)))
                {
                    sb.AppendLine(new AlterDbObjectScript<TableColumn>($"COMMENT ON COLUMN {this.GetDbOwner()}.{this.GetQuotedString(tableName)}.{this.GetQuotedString(column.Name)} IS '{this.ReplaceSplitChar(ValueHelper.TransferSingleQuotation(column.Comment))}'" + this.ScriptsSplitString));
                }
                #endregion

                #region Primary Key
                if (this.Option.TableScriptsGenerateOption.GeneratePrimaryKey && primaryKeys.Count() > 0)
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

                    sb.AppendLine(new CreateDbObjectScript<TablePrimaryKey>(primaryKey));
                }
                #endregion

                #region Foreign Key
                if (this.Option.TableScriptsGenerateOption.GeneratePrimaryKey)
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

                            StringBuilder foreignKeyScript = new StringBuilder();

                            foreignKeyScript.AppendLine(
$@"
ALTER TABLE {quotedTableName} ADD CONSTRAINT { this.GetQuotedString(keyName)} FOREIGN KEY ({columnNames})
REFERENCES { this.GetQuotedString(tableForeignKey.ReferencedTableName)}({referenceColumnName})");

                            if (tableForeignKey.DeleteCascade)
                            {
                                foreignKeyScript.AppendLine("ON DELETE CASCADE");
                            }

                            foreignKeyScript.Append(this.ScriptsSplitString);

                            sb.AppendLine(new CreateDbObjectScript<TableForeignKey>(foreignKeyScript.ToString()));
                        }
                    }
                }
                #endregion

                #region Index
                if (this.Option.TableScriptsGenerateOption.GenerateIndex)
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

                            sb.AppendLine(new CreateDbObjectScript<TableIndex>($"CREATE {(tableIndex.IsUnique ? "UNIQUE" : "")} INDEX { this.GetQuotedString(tableIndex.Name)} ON { this.GetQuotedString(tableName)} ({columnNames})" + this.ScriptsSplitString));

                            if (!indexColumns.Contains(columnNames))
                            {
                                indexColumns.Add(columnNames);
                            }
                        }
                    }
                }
                #endregion               

                #region Constraint
                if (this.Option.TableScriptsGenerateOption.GenerateConstraint)
                {
                    var constraints = schemaInfo.TableConstraints.Where(item => item.Owner == table.Owner && item.TableName == tableName);

                    foreach (TableConstraint constraint in constraints)
                    {
                        sb.AppendLine();
                        sb.AppendLine(new CreateDbObjectScript<TableConstraint>($"ALTER TABLE {quotedTableName} ADD CONSTRAINT {this.GetQuotedString(constraint.Name)} CHECK ({constraint.Definition})" + this.ScriptsSplitString));
                    }
                }
                #endregion

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
            string defaultValueClause = (string.IsNullOrEmpty(column.DefaultValue) ? "" : " DEFAULT " + this.GetColumnDefaultValue(column));
            string requiredClause = (column.IsRequired ? "NOT NULL" : "NULL");

            return $@"{ this.GetQuotedString(column.Name)} {dataType} {defaultValueClause} {requiredClause}";
        }

        public override string ParseDataType(TableColumn column)
        {
            bool isChar = DataTypeHelper.IsCharType(column.DataType.ToLower());
            string dataType = column.DataType;
            if (column.DataType.IndexOf("(") < 0)
            {
                if (isChar)
                {
                    long? dataLength = column.MaxLength;

                    if (dataLength > 0 && dataType.ToLower().StartsWith("n"))
                    {
                        dataLength = dataLength / 2;
                    }

                    dataType = $"{dataType}({dataLength.ToString()})";
                }
                else if (!this.IsNoLengthDataType(dataType))
                {
                    dataType = $"{dataType}";

                    if (!((column.Precision == null || column.Precision == 0) && (column.Scale == null || column.Scale == 0)))
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

            return dataType;
        }

        private bool IsNoLengthDataType(string dataType)
        {
            string[] flags = { "date", "time", "int", "text", "clob", "blob", "binary_double" };

            return flags.Any(item => dataType.ToLower().Contains(item));
        }

        #endregion

        #region Generate Data Script       

        public override Task<long> GetTableRecordCountAsync(DbConnection connection, Table table, string whereClause = "")
        {
            string sql = $@"SELECT COUNT(1) FROM {this.GetDbOwner()}.{ this.GetQuotedString(table.Name)}";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += whereClause;
            }

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

        protected override string GetSqlForPagination(string tableName, string columnNames, string orderColumns, string whereClause, long pageNumber, int pageSize)
        {
            var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

            string orderClause = string.IsNullOrEmpty(orderColumns) ? "(SELECT 0 FROM DUAL)" : orderColumns;

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

        protected override bool NeedInsertParameter(TableColumn column, object value)
        {
            if (value != null)
            {
                if (column.DataType.ToLower() == "clob")
                {
                    return true;
                }
                if (value.GetType() == typeof(string))
                {
                    string str = value.ToString();
                    if (str.Length > 1000 || (str.Contains(SEMICOLON_FUNC) && str.Length > 500))
                    {
                        return true;
                    }
                }
                else if (value.GetType().Name == nameof(TimeSpan))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
