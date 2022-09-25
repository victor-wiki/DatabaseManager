﻿using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Microsoft.SqlServer.Types;
using MySqlConnector;
using NetTopologySuite.Geometries;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class PostgresInterpreter : DbInterpreter
    {
        #region Field & Property
        private string dbSchema;
        public const int DEFAULT_PORT = 5432;
        public const string CONNECT_CHAR = "||";
        public override string UnicodeInsertChar => "";
        public override string CommandParameterChar => ":";
        public const char QuotedLeftChar = '"';
        public const char QuotedRightChar = '"';
        public override char QuotationLeftChar { get { return QuotedLeftChar; } }
        public override char QuotationRightChar { get { return QuotedRightChar; } }
        public override string CommentString => "--";
        public override DatabaseType DatabaseType => DatabaseType.Postgres;
        public override string DefaultDataType => "character varying";
        public override string DefaultSchema => "public";
        public override IndexType IndexType => IndexType.Normal | IndexType.Unique | IndexType.BTree | IndexType.Brin | IndexType.Hash | IndexType.Gin | IndexType.GiST | IndexType.SP_GiST;
        public override bool SupportBulkCopy { get { return true; } }
        public List<string> SystemSchemas => new List<string> { "pg_catalog", "pg_toast", "information_schema" };
        #endregion

        #region Constructor
        public PostgresInterpreter(ConnectionInfo connectionInfo, DbInterpreterOption option) : base(connectionInfo, option)
        {
            this.dbSchema = connectionInfo.UserId;
            this.dbConnector = this.GetDbConnector();
        }
        #endregion

        #region Common Method  
        public override DbConnector GetDbConnector()
        {
            return new DbConnector(new PostgresProvider(), new PostgresConnectionBuilder(), this.ConnectionInfo);
        }

        public override bool IsLowDbVersion(string version)
        {
            if (version != null)
            {
                if (string.Compare(version.Substring(0, 1), "10") < 0)
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
            string sql = @"SELECT datname AS ""Name"" FROM pg_database WHERE datname NOT LIKE 'template%' AND datname NOT IN('postgres') ORDER BY datname";

            return base.GetDbObjectsAsync<Database>(sql);
        }

        public override Task<List<DatabaseSchema>> GetDatabaseSchemasAsync()
        {
            string sql = @"SELECT nspname AS ""Name"",nspname AS ""Schema"" FROM pg_catalog.pg_namespace WHERE nspname NOT IN ('information_schema','pg_catalog', 'pg_toast') ORDER BY nspname";

            return base.GetDbObjectsAsync<DatabaseSchema>(sql);
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
                sql = $@"SELECT n.nspname AS ""Schema"",t.typname AS ""Name""
                         FROM pg_catalog.pg_type t
                         JOIN pg_catalog.pg_namespace n ON  n.oid = t.typnamespace
                         WHERE 1=1";
            }
            else
            {
                sql = $@"SELECT n.nspname AS ""Schema"",t.typname AS ""Name"",a.attname AS ""AttrName"",
                        pg_catalog.format_type ( a.atttypid, a.atttypmod ) AS ""Type"",
                        CASE a.attnotnull WHEN true THEN 0 ELSE 1 END AS ""IsNullable""
                        FROM pg_catalog.pg_attribute a
                        JOIN pg_catalog.pg_type t ON a.attrelid = t.typrelid
                        JOIN pg_catalog.pg_namespace n ON  n.oid = t.typnamespace        
                        WHERE a.attnum > 0 AND NOT a.attisdropped";
            }

            sql += $@" AND t.typtype='c' AND ( t.typrelid = 0 OR ( SELECT c.relkind = 'c' FROM pg_catalog.pg_class c WHERE c.oid = t.typrelid ) )
                       AND NOT EXISTS (SELECT 1 FROM pg_catalog.pg_type el WHERE el.oid = t.typelem AND el.typarray = t.oid ) AND "
                       + this.GetExcludeSystemSchemasCondition("n.nspname");

            if (filter != null && filter.UserDefinedTypeNames != null && filter.UserDefinedTypeNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.UserDefinedTypeNames);
                sql += $" AND t.typname IN ({strNames})";
            }

            sql += $" ORDER BY t.typname{(isSimpleMode ? "" : ",a.attname")}";

            return sql;
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
            string sql = $@"SELECT sequence_schema AS ""Schema"",sequence_name AS ""Name"",'numeric' AS ""DataType"",
                            start_value AS ""StartValue"",increment AS ""Increment"",minimum_value AS ""MinValue"",maximum_value AS ""MaxValue"",
                            CASE cycle_option WHEN 'YES' THEN 1 ELSE 0 END AS ""Cycled"",
                            CASE WHEN ps.seqcache>0 THEN 1 ELSE 0 END AS ""UseCache"", ps.seqcache AS ""CacheSize"",
                            dc.relname AS ""OwnedByTable"",a.attname AS ""OwnedByColumn""
                            FROM pg_catalog.pg_sequence ps
                            JOIN pg_catalog.pg_class c ON c.oid=ps.seqrelid
                            JOIN information_schema.sequences s ON s.sequence_name=c.relname
                            LEFT JOIN pg_catalog.pg_depend d ON d.objid=c.oid AND d.deptype='a'
                            LEFT JOIN pg_catalog.pg_class dc ON d.refobjid=dc.oid
                            LEFT JOIN pg_catalog.pg_attribute a ON a.attrelid=dc.oid AND d.refobjsubid=a.attnum
                            WHERE UPPER(sequence_catalog)=UPPER('{this.ConnectionInfo.Database}')";

            if (filter != null && filter.SequenceNames != null && filter.SequenceNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.SequenceNames);
                sql += $" AND s.sequence_name in ({strNames})";
            }

            sql += " ORDER BY s.sequence_name";

            return sql;
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

        private string GetExcludeSystemSchemasCondition(string tableSchema)
        {
            string strSystemSchemas = this.SystemSchemas.Count > 0 ? string.Join(",", this.SystemSchemas.Select(item => $"'{item}'")) : "";
            return $" {tableSchema} NOT IN({strSystemSchemas})";
        }

        private string GetSqlForTables(SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql;

            if (isSimpleMode)
            {
                sql = $@"SELECT table_schema AS ""Schema"", table_name AS ""Name"" FROM information_schema.tables t";
            }
            else
            {
                sql = $@"SELECT n.nspname AS ""Schema"", c.relname AS ""Name"", d.description AS ""Comment"",
                        1 AS ""IdentitySeed"", 1 AS ""IdentityIncrement""
                        FROM information_schema.tables t
                        INNER JOIN pg_class c ON t.table_name=c.relname
                        INNER JOIN pg_catalog.pg_namespace n  ON c.relnamespace=n.oid
                        LEFT JOIN pg_description d ON d.objoid = c.oid AND d.objsubid=0
                        ";
            }

            sql += $" WHERE t.table_type='BASE TABLE' AND UPPER(t.table_catalog)=UPPER('{this.ConnectionInfo.Database}')";

            sql += $" AND {this.GetExcludeSystemSchemasCondition("t.table_schema")}";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND t.table_name IN ({strTableNames})";
            }

            sql += " ORDER BY t.table_name";

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
            string sql = @"SELECT c.table_schema AS ""Schema"",c.table_name AS ""TableName"",c.column_name AS ""Name"",
                           CASE c.data_type WHEN 'ARRAY' THEN CONCAT(et.data_type,'[]') WHEN 'USER-DEFINED' THEN pt.typname ELSE c.data_type END AS ""DataType"",
                           CASE c.data_type WHEN 'USER-DEFINED' THEN 1 ELSE 0 END AS ""IsUserDefined"",
                           CASE c.is_nullable WHEN 'YES' THEN 1 ELSE 0 END AS ""IsNullable"",COALESCE(c.character_maximum_length,-1) AS ""MaxLength"",
                           c.numeric_precision AS ""Precision"",c.numeric_scale AS ""Scale"",c.ordinal_position AS ""Order"", 
                           CASE c.is_identity WHEN 'YES' THEN 1 ELSE 0 END AS ""IsIdentity"", n.nspname AS ""DateTypeSchema"",
                           c.column_default AS ""DefaultValue"",c.generation_expression AS ""ComputeExp"",
                           d.description AS ""Comment""
                           FROM information_schema.columns c
                           INNER JOIN pg_class pc ON c.table_name = pc.relname
                           LEFT JOIN information_schema.element_types et ON et.object_catalog=c.table_catalog AND et.object_schema=c.table_schema AND et.object_name=c.table_name AND et.collection_type_identifier=c.ordinal_position::text
                           LEFT JOIN pg_catalog.pg_depend pd ON pd.objid= pc.oid AND pd.objsubid=c.ordinal_position AND (pd.deptype is null or pd.deptype='n')
                           LEFT JOIN pg_catalog.pg_type pt ON pd.refobjid=pt.oid  
                           LEFT JOIN pg_catalog.pg_namespace n ON n.oid=pt.typnamespace
                           LEFT JOIN pg_description d ON pc.oid = d.objoid AND c.ordinal_position = d.objsubid ";

            sql += $" WHERE UPPER(c.table_catalog)=UPPER('{this.ConnectionInfo.Database}')";

            sql += $" AND {this.GetExcludeSystemSchemasCondition("c.table_schema")}";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND c.table_name IN ({strTableNames})";
            }

            sql += " ORDER BY c.table_name,c.ordinal_position";

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
            string sql = $@"SELECT kcu.constraint_schema AS ""Schema"",kcu.table_name AS ""TableName"",kcu.column_name AS ""ColumnName"",
                            kcu.constraint_name AS ""Name"", col.ordinal_position AS ""Order"", 0 AS ""IsDesc""
                            FROM information_schema.key_column_usage kcu
                            INNER JOIN information_schema.columns col 
                            ON kcu.table_catalog=col.table_catalog AND kcu.table_schema=col.table_schema AND kcu.table_name=col.table_name AND kcu.column_name =col.column_name
                            INNER JOIN information_schema.table_constraints tc 
                            ON kcu.constraint_catalog=tc.constraint_catalog AND kcu.constraint_schema=tc.constraint_schema AND kcu.constraint_name=tc.constraint_name
                            WHERE tc.constraint_type='PRIMARY KEY'";

            sql += $" AND UPPER(col.table_catalog)=UPPER('{this.ConnectionInfo.Database}')";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND col.table_name IN ({strTableNames})";
            }

            sql += " ORDER BY col.ordinal_position";

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
            string sql = $@"SELECT kcu.constraint_schema as ""Schema"", kcu.constraint_name AS ""Name"", kcu.table_name AS ""TableName"",kcu.column_name AS ""ColumnName"",
                            fkcu.constraint_schema AS ""ReferencedSchema"",fkcu.table_name AS ""ReferencedTableName"",fkcu.column_name AS ""ReferencedColumnName"",
                            CASE rc.update_rule WHEN 'CASCADE' THEN 1 ELSE 0 END AS ""UpdateCascade"", CASE rc.delete_rule WHEN 'CASCADE' THEN 1 ELSE 0 END  AS ""DeleteCascade"" 
                            FROM information_schema.key_column_usage kcu 
                            INNER JOIN information_schema.referential_constraints rc ON kcu.constraint_name=rc.constraint_name
                            INNER JOIN information_schema.key_column_usage fkcu ON fkcu.constraint_name=rc.unique_constraint_name
                            WHERE kcu.ordinal_position=fkcu.ordinal_position";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND kcu.table_name IN ({strTableNames})";
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
            string sql = $@"SELECT n.nspname AS ""Schema"", c.relname AS ""TableName"",ccu.column_name AS ""ColumnName"",con.conname AS ""Name"",
                            CASE con.contype WHEN 'u' THEN 1 ELSE 0 END AS ""IsUnique"",CASE con.contype WHEN 'p' THEN 1 ELSE 0 END AS ""IsPrimary""
                            FROM pg_catalog.pg_constraint con 
                            JOIN pg_catalog.pg_class c ON c.oid=con.conrelid
                            JOIN pg_catalog.pg_namespace n ON c.relnamespace=n.oid
                            JOIN information_schema.constraint_column_usage ccu ON ccu.table_schema=n.nspname AND con.conname=ccu.constraint_name
                            WHERE con.contype NOT IN('c','f') {(includePrimaryKey ? "" : "AND con.contype<>'p'")} AND ccu.table_catalog='{this.ConnectionInfo.Database}'
";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND ccu.table_name IN ({strTableNames})";
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

            string sql = $@"SELECT trigger_name AS ""Name"",event_object_schema AS ""TableSchema"",event_object_table AS ""TableName"",                         
                         {(isSimpleMode ? "''" : $@"CONCAT('CREATE TRIGGER ""',trigger_name,'"" ',action_timing,' ',event_manipulation,
                        ' ON ',event_object_schema,'.',event_object_table,'{Environment.NewLine}FOR EACH ',action_orientation, '{Environment.NewLine}',action_statement)")} AS ""Definition""
                        FROM information_schema.triggers
                        WHERE UPPER(trigger_catalog) = UPPER('{this.ConnectionInfo.Database}')
                        ";

            if (filter != null)
            {
                if (filter.TableNames != null && filter.TableNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                    sql += $" AND event_object_table IN ({strNames})";
                }

                if (filter.TableTriggerNames != null && filter.TableTriggerNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter.TableTriggerNames);
                    sql += $" AND trigger_name IN ({strNames})";
                }
            }

            sql += " ORDER BY trigger_name";

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
            string sql = $@"SELECT cc.constraint_schema AS ""Schema"",cc.constraint_name AS ""Name"",tc.table_name AS ""TableName"",cc.check_clause AS ""Definition""
                            FROM information_schema.check_constraints cc 
                            JOIN information_schema.table_constraints tc ON cc.constraint_catalog=tc.constraint_catalog AND cc.constraint_schema=tc.constraint_schema AND cc.constraint_name=tc.constraint_name
                            WHERE UPPER(cc.constraint_catalog) = UPPER('{this.ConnectionInfo.Database}') 
                            AND tc.constraint_name NOT LIKE '%_not_null'";

            if (filter != null && filter.TableNames != null && filter.TableNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.TableNames);
                sql += $" AND tc.table_name IN ({strNames})";
            }

            sql += " ORDER BY cc.constraint_name";

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

            string sql = $@"SELECT v.table_schema AS ""Schema"",v.table_name AS ""Name"",{(isSimpleMode ? "''" : $@"CONCAT('CREATE OR REPLACE VIEW ""',v.table_name,'"" AS{Environment.NewLine}',v.view_definition)")} AS ""Definition""
                            FROM information_schema.views v
                            WHERE UPPER(v.table_catalog) = UPPER('{this.ConnectionInfo.Database}')";

            sql += " AND " + this.GetExcludeSystemSchemasCondition("v.table_schema");

            if (filter != null && filter.ViewNames != null && filter.ViewNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(filter.ViewNames);
                sql += $" AND v.table_name IN ({strNames})";
            }

            sql += " ORDER BY v.table_name";

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

        private string GetSqlForRoutines(string type, SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            bool isFunction = type.ToUpper() == "FUNCTION";
            string strNamesCondition = "";

            string[] objectNames = isFunction ? filter?.FunctionNames : filter?.ProcedureNames;
            if (objectNames != null && objectNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(objectNames);
                strNamesCondition = $" AND r.routine_name IN ({strNames})";
            }

            string sql = "";

            if (isSimpleMode)
            {
                sql = $@"SELECT r.routine_schema AS ""Schema"", r.routine_name AS ""Name"",CASE WHEN r.data_type='trigger' THEN 1 ELSE 0 END AS ""IsTriggerFunction""
                         FROM information_schema.routines r
                         WHERE r.routine_type='{type}' AND UPPER(r.routine_catalog)=UPPER('{this.ConnectionInfo.Database}')
                         AND {this.GetExcludeSystemSchemasCondition("r.routine_schema")} {strNamesCondition}";
            }
            else
            {
                sql = $@"SELECT r.routine_schema AS ""Schema"", r.routine_name AS ""Name"",CASE WHEN r.data_type='trigger' THEN 1 ELSE 0 END AS ""IsTriggerFunction"",
                        concat('CREATE OR REPLACE {type} ', routine_schema,'.""',r.routine_name, '""(', array_to_string(
                        array_agg(concat(CASE p.parameter_mode WHEN 'IN' THEN '' ELSE p.parameter_mode END,' ',p.parameter_name,' ',p.data_type)),','),')'
                        ,r.routine_definition) AS ""Definition""
                        FROM information_schema.routines r
                        LEFT JOIN information_schema.parameters p ON p.specific_name=r.specific_name
                        WHERE r.routine_type='{type}' AND UPPER(r.routine_catalog)=UPPER('{this.ConnectionInfo.Database}')
                        AND {this.GetExcludeSystemSchemasCondition("r.routine_schema")} {strNamesCondition} 
                        GROUP BY r.routine_schema,r.routine_name,r.routine_definition,r.data_type
                       ";
            }

            sql += " ORDER BY r.routine_name";

            return sql;
        }
        #endregion
        #endregion

        #region BulkCopy
        public override async Task BulkCopyAsync(DbConnection connection, DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            if (!(connection is NpgsqlConnection conn))
            {
                return;
            }

            using (var bulkCopy = new PostgreBulkCopy(conn, bulkCopyInfo.Transaction as NpgsqlTransaction))
            {
                bulkCopy.DestinationTableName = this.GetQuotedDbObjectNameWithSchema(bulkCopyInfo.DestinationTableSchema, bulkCopyInfo.DestinationTableName);
                bulkCopy.BulkCopyTimeout = bulkCopyInfo.Timeout.HasValue ? bulkCopyInfo.Timeout.Value : SettingManager.Setting.CommandTimeout; ;
                bulkCopy.ColumnNameNeedQuoted = this.DbObjectNameMode == DbObjectNameMode.WithQuotation;
                bulkCopy.DetectDateTimeTypeByValues = bulkCopyInfo.DetectDateTimeTypeByValues;
                bulkCopy.TableColumns = bulkCopyInfo.Columns;

                await bulkCopy.WriteToServerAsync(this.ConvertDataTable(dataTable, bulkCopyInfo));
            }
        }

        private DataTable ConvertDataTable(DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            var columns = dataTable.Columns.Cast<DataColumn>();

            if (!columns.Any(item => item.DataType == typeof(MySqlDateTime)
                || item.DataType == typeof(SqlHierarchyId)
                || item.DataType == typeof(SqlGeography)
                || item.DataType == typeof(SqlGeometry)
                || item.DataType == typeof(byte[])
                || item.DataType == typeof(string)
                || item.DataType == typeof(Guid)
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

                        if (type != typeof(DBNull))
                        {
                            Type newColumnType = null;
                            TableColumn tableColumn = bulkCopyInfo.Columns.FirstOrDefault(item => item.Name == dataTable.Columns[i].ColumnName);
                            string dataType = tableColumn.DataType.ToLower();
                            dynamic newValue = null;

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
                            else if (type == typeof(SqlHierarchyId))
                            {
                                newValue = ((SqlHierarchyId)value).ToString();
                                newColumnType = typeof(String);
                            }
                            else if (type == typeof(SqlGeography))
                            {
                                SqlGeography geography = (SqlGeography)value;

                                newValue = GeometryHelper.SqlGeographyToPostgresGeography(geography);

                                newColumnType = typeof(Geometry);
                            }
                            else if (type == typeof(SqlGeometry))
                            {
                                newColumnType = typeof(Geometry);
                                SqlGeometry geometry = (SqlGeometry)value;
                                newValue = GeometryHelper.SqlGeometryToPostgresGeometry(geometry);
                            }
                            else if (type == typeof(byte[]))
                            {
                                if (dataType == "geometry")
                                {
                                    newColumnType = typeof(Geometry);
                                    newValue = GeometryHelper.MySqlGeometryToPostgresGeometry(value as byte[]);
                                }
                            }
                            else if (type == typeof(string))
                            {
                                if (dataType == "geometry")
                                {
                                    newColumnType = typeof(Geometry);
                                    newValue = GeometryHelper.SqlGeometryToPostgresGeometry(SqlGeometry.STGeomFromText(new System.Data.SqlTypes.SqlChars(value as string), 0));
                                }
                            }
                            else if (type == typeof(Guid))
                            {
                                newColumnType = typeof(string);
                                newValue = value.ToString();
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

        #region Sql Clause    
        public override Task<long> GetTableRecordCountAsync(DbConnection connection, Table table, string whereClause = "")
        {
            string sql = $@"SELECT COUNT(1) FROM {this.GetQuotedDbObjectNameWithSchema(table)}";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += whereClause;
            }

            return base.GetTableRecordCountAsync(connection, sql);
        }

        protected override string GetSqlForPagination(string tableName, string columnNames, string orderColumns, string whereClause, long pageNumber, int pageSize)
        {
            var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

            var pagedSql = $@"SELECT {columnNames}
							  FROM {tableName}
                             {whereClause} 
                             {(!string.IsNullOrEmpty(orderColumns) ? ("ORDER BY " + orderColumns) : "")} 
                             LIMIT {pageSize} OFFSET {startEndRowNumber.StartRowNumber - 1}";

            return pagedSql;
        }

        public override string GetLimitStatement(int limitStart, int limitCount)
        {
            return $"LIMIT {limitCount} OFFSET {limitStart}";
        }
        #endregion

        #region Parse Column & DataType
        public override string ParseColumn(Table table, TableColumn column)
        {
            if (column.IsComputed)
            {
                return $"{this.GetQuotedString(column.Name)} {column.DataType} GENERATED ALWAYS AS ({column.ComputeExp}) STORED";
            }
            else
            {
                string dataType = this.ParseDataType(column);
                string requiredClause = (column.IsRequired ? "NOT NULL" : "NULL");

                string identityClause = (this.Option.TableScriptsGenerateOption.GenerateIdentity && column.IsIdentity ? $" GENERATED ALWAYS AS IDENTITY " : "");

                string defaultValueClause = this.Option.TableScriptsGenerateOption.GenerateDefaultValue && !string.IsNullOrEmpty(column.DefaultValue) ? (" DEFAULT " + this.GetColumnDefaultValue(column)) : "";
                string scriptComment = string.IsNullOrEmpty(column.ScriptComment) ? "" : $"/*{column.ScriptComment}*/";

                string content = $"{this.GetQuotedString(column.Name)} {dataType}{defaultValueClause} {requiredClause}{identityClause}{scriptComment}";

                return content;
            }
        }

        public override string ParseDataType(TableColumn column)
        {
            string dataType = column.DataType;

            if (dataType != null && dataType.IndexOf("(") < 0)
            {
                DataTypeSpecification dataTypeSpec = this.GetDataTypeSpecification(dataType.ToLower());

                if (dataTypeSpec != null && !string.IsNullOrEmpty(dataTypeSpec.Format))
                {
                    string format = dataTypeSpec.Format;
                    string args = dataTypeSpec.Args;

                    if (!string.IsNullOrEmpty(args))
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
                    }
                }
                else
                {
                    string dataLength = this.GetColumnDataLength(column);

                    if (!string.IsNullOrEmpty(dataLength))
                    {
                        dataType += $"({dataLength})";
                    }
                }
            }

            return dataType?.Trim();
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