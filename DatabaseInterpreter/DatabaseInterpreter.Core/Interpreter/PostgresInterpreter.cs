using DatabaseInterpreter.Geometry;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Microsoft.SqlServer.Types;
using MySqlConnector;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using PgGeom = NetTopologySuite.Geometries;

namespace DatabaseInterpreter.Core
{
    public class PostgresInterpreter : DbInterpreter
    {
        #region Field & Property
        private string dbSchema;
        public const int DEFAULT_PORT = 5432;
        public override string STR_CONCAT_CHARS => "||";
        public override string UnicodeLeadingFlag => "";
        public override string CommandParameterChar => ":";
        public const char QuotedLeftChar = '"';
        public const char QuotedRightChar = '"';
        public override bool SupportQuotationChar => true;
        public override char QuotationLeftChar { get { return QuotedLeftChar; } }
        public override char QuotationRightChar { get { return QuotedRightChar; } }
        public override string CommentString => "--";
        public override DatabaseType DatabaseType => DatabaseType.Postgres;
        public override string DefaultDataType => "character varying";
        public override string DefaultSchema => "public";
        public override IndexType IndexType => IndexType.Primary | IndexType.Unique | IndexType.BTree | IndexType.Brin | IndexType.Hash | IndexType.Gin | IndexType.GiST | IndexType.SP_GiST;
        public override DatabaseObjectType SupportDbObjectType => DatabaseObjectType.Table | DatabaseObjectType.View | DatabaseObjectType.Function
                                                                | DatabaseObjectType.Procedure | DatabaseObjectType.Type | DatabaseObjectType.Sequence;
        public override bool SupportBulkCopy => true;
        public override bool SupportNchar => false;
        public override List<string> BuiltinDatabases => new List<string> { "postgres" };
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
            return this.IsLowDbVersion(version, "12");
        }
        #endregion

        #region Schema Information
        #region Database
        public override Task<List<Database>> GetDatabasesAsync()
        {
            string sql = $@"SELECT datname AS ""Name"" FROM pg_database WHERE datname NOT LIKE 'template%'{this.GetExcludeBuiltinDbNamesCondition("datname", false)} ORDER BY datname";

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
            string sql = @"SELECT nspname AS ""Name"",nspname AS ""Schema"" FROM pg_catalog.pg_namespace WHERE nspname NOT IN ('information_schema','pg_catalog', 'pg_toast') ORDER BY nspname";

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
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            var sb = this.CreateSqlBuilder();

            if (isSimpleMode)
            {
                sb.Append($@"SELECT n.nspname AS ""Schema"",t.typname AS ""TypeName""
                         FROM pg_catalog.pg_type t
                         JOIN pg_catalog.pg_namespace n ON  n.oid = t.typnamespace
                         WHERE 1=1");
            }
            else
            {
                sb.Append($@"SELECT n.nspname AS ""Schema"",t.typname AS ""TypeName"",a.attname AS ""Name"",
                        pg_catalog.format_type(a.atttypid, null) AS ""DataType"",
                        COALESCE(information_schema._pg_char_max_length(a.atttypid, a.atttypmod),-1) AS ""MaxLength"",
                        information_schema._pg_numeric_precision(a.atttypid, a.atttypmod) AS ""Precision"",
                        information_schema._pg_numeric_scale(a.atttypid, a.atttypmod) AS ""Scale"",
                        CASE a.attnotnull WHEN true THEN 0 ELSE 1 END AS ""IsNullable""
                        FROM pg_catalog.pg_attribute a
                        JOIN pg_catalog.pg_type t ON a.attrelid = t.typrelid
                        JOIN pg_catalog.pg_namespace n ON  n.oid = t.typnamespace        
                        WHERE a.attnum > 0 AND NOT a.attisdropped");
            }

            sb.Append($@"AND t.typtype='c' AND ( t.typrelid = 0 OR ( SELECT c.relkind = 'c' FROM pg_catalog.pg_class c WHERE c.oid = t.typrelid ) )
                       AND NOT EXISTS (SELECT 1 FROM pg_catalog.pg_type el WHERE el.oid = t.typelem AND el.typarray = t.oid )");

            sb.Append(this.GetExcludeSystemSchemasCondition("n.nspname"));
            sb.Append(this.GetFilterSchemaCondition(filter, "n.nspname"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.UserDefinedTypeNames, "t.typname"));

            if (Setting.ExcludePostgresExtensionObjects)
            {
                sb.Append(this.GetSqlForExcludeExtensionUserDefinedTypes("t.typname"));
            }

            sb.Append($"ORDER BY t.typname{(isSimpleMode ? "" : ",a.attname")}");

            return sb.Content;
        }

        private string GetSqlForExcludeExtensionUserDefinedTypes(string columnName)
        {
            string sql = $@"AND {columnName} NOT IN(
                            SELECT t.typname  
                            FROM pg_catalog.pg_extension e
                            JOIN pg_catalog.pg_depend d ON (d.refobjid = e.oid)
                            JOIN pg_catalog.pg_type t ON t.oid = d.objid                            
                            WHERE d.deptype = 'e' AND t.typtype='c'
                       )";

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
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            var sb = this.CreateSqlBuilder();

            bool isLowDbVersion = this.IsLowDbVersion(this.GetDbVersion(), "10");

            if (!isLowDbVersion)
            {
                sb.Append($@"SELECT sequence_schema AS ""Schema"",sequence_name AS ""Name"",'numeric' AS ""DataType"",
                            start_value AS ""StartValue"",increment AS ""Increment"",minimum_value AS ""MinValue"",maximum_value AS ""MaxValue"",
                            CASE cycle_option WHEN 'YES' THEN 1 ELSE 0 END AS ""Cycled"",
                            CASE WHEN ps.seqcache>0 THEN 1 ELSE 0 END AS ""UseCache"", ps.seqcache AS ""CacheSize"",
                            dc.relname AS ""OwnedByTable"",a.attname AS ""OwnedByColumn""
                            FROM pg_catalog.pg_sequence ps
                            JOIN pg_catalog.pg_class c ON c.oid=ps.seqrelid
                            JOIN information_schema.sequences s ON s.sequence_name=c.relname
                            LEFT JOIN pg_catalog.pg_depend d ON d.objid=c.oid AND d.deptype='a'
                            LEFT JOIN pg_catalog.pg_class dc ON d.refobjid=dc.oid
                            LEFT JOIN pg_catalog.pg_attribute a ON a.attrelid=dc.oid AND d.refobjsubid=a.attnum");
            }
            else
            {
                //low version has no table "pg_catalog.pg_sequence"

                bool isSingle = filter.SequenceNames?.Length == 1;

                string sequenceName = this.GetQuotedDbObjectNameWithSchema(filter.Schema, filter.SequenceNames?.FirstOrDefault());
                string cacheClause = (!isSimpleMode && isSingle) ? $@"{Environment.NewLine},(SELECT CASE WHEN cache_value>0 THEN 1 ELSE 0 END FROM {sequenceName}) AS ""UseCache"",(SELECT cache_value FROM {sequenceName}) AS ""CacheSize""" : "";

                sb.Append($@"SELECT sequence_schema AS ""Schema"",sequence_name AS ""Name"",('numeric' :: CHARACTER VARYING) AS ""DataType"",
                start_value AS ""StartValue"",increment AS ""Increment"",minimum_value AS ""MinValue"",maximum_value AS ""MaxValue"",
                CASE cycle_option WHEN 'YES' THEN 1 ELSE 0 END AS ""Cycled"",dc.relname AS ""OwnedByTable"",a.attname AS ""OwnedByColumn""{cacheClause}
                FROM information_schema.sequences s
                JOIN pg_catalog.pg_class c ON c.relname=s.sequence_name AND c.relkind = 'S' :: ""char""
                LEFT JOIN pg_catalog.pg_depend d ON d.objid=c.oid AND d.deptype='a'
                LEFT JOIN pg_catalog.pg_class dc ON d.refobjid=dc.oid
                LEFT JOIN pg_catalog.pg_attribute a ON a.attrelid=dc.oid AND d.refobjsubid=a.attnum");
            }

            sb.Append($"WHERE UPPER(sequence_catalog)=UPPER('{this.ConnectionInfo.Database}')");
            sb.Append(this.GetFilterSchemaCondition(filter, "sequence_schema"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.SequenceNames, "s.sequence_name"));

            if (Setting.ExcludePostgresExtensionObjects)
            {
                sb.Append(this.GetSqlForExcludeExtensionObjects(DatabaseObjectType.Sequence, "s.sequence_name"));
            }

            sb.Append("ORDER BY s.sequence_name");

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
            string database = this.ConnectionInfo.Database;

            if (!this.BuiltinDatabases.Contains(database))
            {
                string strSystemSchemas = this.SystemSchemas.Count > 0 ? string.Join(",", this.SystemSchemas.Select(item => $"'{item}'")) : "";
                return $"AND {tableSchema} NOT IN({strSystemSchemas})";
            }

            return string.Empty;
        }

        private string GetSqlForTables(SchemaInfoFilter filter = null)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            var sb = this.CreateSqlBuilder();

            if (isSimpleMode)
            {
                sb.Append($@"SELECT table_schema AS ""Schema"", table_name AS ""Name"" FROM information_schema.tables t");
            }
            else
            {
                sb.Append($@"SELECT n1.nspname AS ""Schema"", c.relname AS ""Name"", d.description AS ""Comment"",
                        1 AS ""IdentitySeed"", 1 AS ""IdentityIncrement""
                        FROM information_schema.tables t
                        INNER JOIN pg_catalog.pg_namespace n1 ON t.table_schema = n1.nspname
                        INNER JOIN pg_catalog.pg_class c ON n1.oid = c.relnamespace AND t.table_name=c.relname
                        INNER JOIN pg_catalog.pg_namespace n2  ON c.relnamespace=n2.oid
                        LEFT JOIN pg_description d ON d.objoid = c.oid AND d.objsubid=0");
            }

            sb.Append($"WHERE t.table_type='BASE TABLE' AND UPPER(t.table_catalog)=UPPER('{this.ConnectionInfo.Database}')");

            sb.Append(this.GetExcludeSystemSchemasCondition("t.table_schema"));
            sb.Append(this.GetFilterSchemaCondition(filter, "t.table_schema"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "t.table_name"));

            if (Setting.ExcludePostgresExtensionObjects)
            {
                sb.Append(this.GetSqlForExcludeExtensionObjects(DatabaseObjectType.Table, "t.table_name"));
            }

            sb.Append("ORDER BY t.table_name");

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
            bool isSimpleMode = this.IsObjectFectchSimpleMode();
            bool isForView = this.IsForViewColumnFilter(filter);

            string kind = !isForView ? "r" : "v";

            string detailColumns = (isForView || isSimpleMode) ? "" : @",c.column_default AS ""DefaultValue"",c.generation_expression AS ""ComputeExp"",
                        d.description AS ""Comment""";

            string descriptionJoin = (isForView || isSimpleMode) ? "" : "LEFT JOIN pg_description d ON pc.oid = d.objoid AND c.ordinal_position = d.objsubid";

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT c.table_schema AS ""Schema"",c.table_name AS ""TableName"",c.column_name AS ""Name"",
                        CASE c.data_type WHEN 'ARRAY' THEN CONCAT(et.data_type,'[]') WHEN 'USER-DEFINED' THEN pt.typname ELSE c.data_type END AS ""DataType"",
                        CASE c.data_type WHEN 'USER-DEFINED' THEN 1 ELSE 0 END AS ""IsUserDefined"",
                        CASE c.is_nullable WHEN 'YES' THEN 1 ELSE 0 END AS ""IsNullable"",COALESCE(c.character_maximum_length,-1) AS ""MaxLength"",
                        c.numeric_precision AS ""Precision"",c.numeric_scale AS ""Scale"",c.ordinal_position AS ""Order"", 
                        CASE c.is_identity WHEN 'YES' THEN 1 ELSE 0 END AS ""IsIdentity"", n.nspname AS ""DateTypeSchema""
                        {detailColumns}
                        FROM information_schema.columns c
                        INNER JOIN pg_catalog.pg_namespace pn ON c.table_schema=pn.nspname
                        INNER JOIN pg_catalog.pg_class pc ON pn.oid=pc.relnamespace AND c.table_name = pc.relname AND pc.relkind='{kind}'
                        LEFT JOIN information_schema.element_types et ON et.object_catalog=c.table_catalog AND et.object_schema=c.table_schema AND et.object_name=c.table_name AND et.collection_type_identifier=c.ordinal_position::text
                        LEFT JOIN pg_catalog.pg_depend pd ON pd.objid= pc.oid AND pd.objsubid=c.ordinal_position AND (pd.deptype is null or pd.deptype='n')
                        LEFT JOIN pg_catalog.pg_type pt ON pd.refobjid=pt.oid  
                        LEFT JOIN pg_catalog.pg_namespace n ON n.oid=pt.typnamespace
                        {descriptionJoin}");

            sb.Append($"WHERE UPPER(c.table_catalog)=UPPER('{this.ConnectionInfo.Database}')");
            sb.Append(this.GetExcludeSystemSchemasCondition("c.table_schema"));
            sb.Append(this.GetFilterSchemaCondition(filter, "c.table_schema"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "c.table_name"));

            sb.Append("ORDER BY c.table_name,c.ordinal_position");

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
            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT kcu.constraint_schema AS ""Schema"",kcu.table_name AS ""TableName"",kcu.column_name AS ""ColumnName"",
                            kcu.constraint_name AS ""Name"", col.ordinal_position AS ""Order"", 0 AS ""IsDesc""
                            FROM information_schema.key_column_usage kcu
                            INNER JOIN information_schema.columns col 
                            ON kcu.table_catalog=col.table_catalog AND kcu.table_schema=col.table_schema AND kcu.table_name=col.table_name AND kcu.column_name =col.column_name
                            INNER JOIN information_schema.table_constraints tc 
                            ON kcu.constraint_catalog=tc.constraint_catalog AND kcu.constraint_schema=tc.constraint_schema AND kcu.constraint_name=tc.constraint_name
                            WHERE tc.constraint_type='PRIMARY KEY'");

            sb.Append($"AND UPPER(col.table_catalog)=UPPER('{this.ConnectionInfo.Database}')");
            sb.Append(this.GetFilterSchemaCondition(filter, "col.table_schema"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "col.table_name"));

            sb.Append("ORDER BY col.ordinal_position");

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
            string tableAlias = !isFilterForReferenced ? "kcu" : "fkcu";

            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT kcu.constraint_schema AS ""Schema"", kcu.constraint_name AS ""Name"", kcu.table_name AS ""TableName"",kcu.column_name AS ""ColumnName"",
                            fkcu.constraint_schema AS ""ReferencedSchema"",fkcu.table_name AS ""ReferencedTableName"",fkcu.column_name AS ""ReferencedColumnName"",
                            CASE rc.update_rule WHEN 'CASCADE' THEN 1 ELSE 0 END AS ""UpdateCascade"", CASE rc.delete_rule WHEN 'CASCADE' THEN 1 ELSE 0 END  AS ""DeleteCascade"" 
                            FROM information_schema.key_column_usage kcu 
                            INNER JOIN information_schema.referential_constraints rc ON kcu.constraint_name=rc.constraint_name
                            INNER JOIN information_schema.key_column_usage fkcu ON fkcu.constraint_name=rc.unique_constraint_name
                            WHERE kcu.ordinal_position=fkcu.ordinal_position");

            sb.Append(this.GetFilterSchemaCondition(filter, $"{tableAlias}.constraint_schema"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, $"{tableAlias}.table_name"));

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
            var sb = this.CreateSqlBuilder();

            //the table pg_constraint only has pk, fk and unique index.
            sb.Append($@"SELECT n.nspname AS ""Schema"", c.relname AS ""TableName"",ccu.column_name AS ""ColumnName"",con.conname AS ""Name"",
                        CASE con.contype WHEN 'u' THEN 'Unique' WHEN 'p' THEN 'Primary' ELSE '' END AS ""Type"", 
                        CASE con.contype WHEN 'u' THEN 1 ELSE 0 END AS ""IsUnique"",CASE con.contype WHEN 'p' THEN 1 ELSE 0 END AS ""IsPrimary"",
                        CASE con.contype WHEN 'p' THEN 1 ELSE 0 END AS ""Clustered""
                        FROM pg_catalog.pg_constraint con 
                        JOIN pg_catalog.pg_class c ON con.connamespace=c.relnamespace AND c.oid=con.conrelid
                        JOIN pg_catalog.pg_namespace n ON c.relnamespace=n.oid
                        JOIN information_schema.constraint_column_usage ccu ON ccu.table_schema=n.nspname AND con.conname=ccu.constraint_name
                        WHERE con.contype NOT IN('c','f') {(includePrimaryKey ? "" : "AND con.contype<>'p'")} AND ccu.table_catalog='{this.ConnectionInfo.Database}'");

            sb.Append(this.GetFilterSchemaCondition(filter, "n.nspname"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "ccu.table_name"));

            //common index, such as:btree
            sb.Append($@"UNION ALL
                        SELECT n.nspname AS ""Schema"", c1.relname AS ""TableName"", a.attname AS ""ColumnName"", c2.relname AS ""Name"", am.amname AS ""Type"",
                        0 AS ""IsUnique"",0 AS ""IsPrimary"",0 AS ""Clustered""
                        FROM pg_index i
                        JOIN pg_class c1 ON i.indrelid=c1.oid
                        JOIN pg_class c2 ON i.indexrelid=c2.oid
                        JOIN pg_namespace n ON c1.relnamespace = n.oid
                        JOIN pg_catalog.pg_am am ON am.oid=c2.relam
                        JOIN pg_catalog.pg_depend d ON d.objid= c2.oid AND d.refobjid=c1.oid
                        JOIN pg_attribute a ON a.attrelid=d.refobjid AND d.refobjsubid=a.attnum
                        WHERE c2.relkind IN('i','I')");

            sb.Append(this.GetFilterSchemaCondition(filter, "n.nspname"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "c1.relname"));

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

            sb.Append($@"SELECT trigger_name AS ""Name"",event_object_schema AS ""TableSchema"",event_object_table AS ""TableName"",                         
                         ({(isSimpleMode ? "''" : $@"CONCAT('CREATE TRIGGER ""',trigger_name,'"" ',action_timing,' ', array_to_string(array_agg(event_manipulation),' OR '),
                        ' ON ',event_object_schema,'.""',event_object_table,'""{Environment.NewLine}FOR EACH ',action_orientation, '{Environment.NewLine}',action_statement)")}:: CHARACTER VARYING) AS ""Definition""
                        FROM information_schema.triggers
                        WHERE UPPER(trigger_catalog) = UPPER('{this.ConnectionInfo.Database}')");

            if (filter != null)
            {
                sb.Append(this.GetFilterSchemaCondition(filter, "event_object_schema"));
                sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "event_object_table"));

                if (filter.TableTriggerNames != null && filter.TableTriggerNames.Any())
                {
                    string strNames = StringHelper.GetSingleQuotedString(filter?.TableTriggerNames);
                    sb.Append($"AND trigger_name IN ({strNames})");
                }
            }

            sb.Append("GROUP BY event_object_schema,trigger_name,event_object_table,action_timing,action_orientation,action_statement");

            sb.Append("ORDER BY trigger_name");

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
            var sb = this.CreateSqlBuilder();

            sb.Append($@"SELECT cc.constraint_schema AS ""Schema"",cc.constraint_name AS ""Name"",tc.table_name AS ""TableName"",cc.check_clause AS ""Definition""
                            FROM information_schema.check_constraints cc 
                            JOIN information_schema.table_constraints tc ON cc.constraint_catalog=tc.constraint_catalog AND cc.constraint_schema=tc.constraint_schema AND cc.constraint_name=tc.constraint_name
                            WHERE UPPER(cc.constraint_catalog) = UPPER('{this.ConnectionInfo.Database}') 
                            AND tc.constraint_name NOT LIKE '%_not_null'");

            sb.Append(this.GetFilterSchemaCondition(filter, "cc.constraint_schema"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.TableNames, "tc.table_name"));

            sb.Append("ORDER BY cc.constraint_name");

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

            string definition = "";

            if (!isSimpleMode)
            {
                definition = $@",CONCAT('CREATE OR REPLACE VIEW ""',v.table_schema,'"".""',v.table_name,'"" AS{Environment.NewLine}',v.view_definition) AS ""Definition""";
            }

            sb.Append($@"SELECT v.table_schema AS ""Schema"",v.table_name AS ""Name""{definition}
                         FROM information_schema.views v
                         WHERE UPPER(v.table_catalog) = UPPER('{this.ConnectionInfo.Database}')");

            sb.Append(this.GetExcludeSystemSchemasCondition("v.table_schema"));

            sb.Append(this.GetFilterSchemaCondition(filter, "v.table_schema"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.ViewNames, "v.table_name"));

            if (Setting.ExcludePostgresExtensionObjects)
            {
                sb.Append(this.GetSqlForExcludeExtensionObjects(DatabaseObjectType.View, "v.table_name"));
            }

            sb.Append("ORDER BY v.table_name");

            return sb.Content;
        }

        private string GetSqlForExcludeExtensionObjects(DatabaseObjectType databaseObjectType, string columnName)
        {
            string kind = "";

            //https://www.postgresql.org/docs/9.3/catalog-pg-class.html
            switch (databaseObjectType)
            {
                case DatabaseObjectType.Table:
                    kind = "r";
                    break;
                case DatabaseObjectType.View:
                    kind = "v";
                    break;
                case DatabaseObjectType.Sequence:
                    kind = "s";
                    break;
            }

            if (string.IsNullOrEmpty(kind))
            {
                return string.Empty;
            }

            string sql = $@"AND {columnName} NOT IN(
                                 SELECT c.relname
                                 FROM pg_catalog.pg_extension e
                                 JOIN pg_catalog.pg_depend d ON d.refobjid = e.oid
                                 JOIN pg_catalog.pg_class c ON c.oid = d.objid                                
                                 WHERE d.deptype = 'e' AND c.relkind = '{kind}'
                      )";

            return sql;
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
            string type = isFunction ? "FUNCTION" : "PROCEDURE";
            string strNamesCondition = "";

            string[] objectNames = isFunction ? filter?.FunctionNames : filter?.ProcedureNames;

            var sb = this.CreateSqlBuilder();

            Action appendCondition = () =>
            {
                sb.Append(this.GetFilterSchemaCondition(filter, "r.routine_schema"));
                sb.Append(this.GetFilterNamesCondition(filter, objectNames, " r.routine_name"));
            };

            if (isSimpleMode)
            {
                sb.Append($@"SELECT r.routine_schema AS ""Schema"", r.routine_name AS ""Name"",r.data_type AS ""DataType"",
                         CASE WHEN r.data_type='trigger' THEN 1 ELSE 0 END AS ""IsTriggerFunction""
                         FROM information_schema.routines r
                         WHERE r.routine_type='{type}' AND UPPER(r.routine_catalog)=UPPER('{this.ConnectionInfo.Database}')");

                sb.Append(this.GetExcludeSystemSchemasCondition("r.routine_schema"));

                appendCondition();

                if (Setting.ExcludePostgresExtensionObjects)
                {
                    sb.Append(this.GetSqlForExcludeExtensionsRoutines("r.routine_name", isFunction));
                }
            }
            else
            {
                sb.Append($@"SELECT r.routine_schema AS ""Schema"", r.routine_name AS ""Name"",r.data_type AS ""DataType"",
                        CASE WHEN r.data_type='trigger' THEN 1 ELSE 0 END AS ""IsTriggerFunction"",
                        concat('CREATE OR REPLACE {type} ', routine_schema,'.""',r.routine_name, '""(', array_to_string(
                        array_agg(concat(CASE p.parameter_mode WHEN 'IN' THEN '' ELSE p.parameter_mode END,' ',p.parameter_name,' ',p.data_type)),','),')'
                        {(isFunction ? "' RETURNS ',r.data_type" : "")},CHR(10),'LANGUAGE ''plpgsql''',CHR(10),'AS ','$$',CHR(10),r.routine_definition,CHR(10),'$$;') AS ""Definition""
                        FROM information_schema.routines r
                        LEFT JOIN information_schema.parameters p ON p.specific_name=r.specific_name
                        WHERE r.routine_type='{type}' AND UPPER(r.routine_catalog)=UPPER('{this.ConnectionInfo.Database}')
                        {this.GetExcludeSystemSchemasCondition("r.routine_schema")} {strNamesCondition}");

                appendCondition();

                sb.Append("GROUP BY r.routine_schema,r.routine_name,r.routine_definition,r.data_type");
            }

            sb.Append("ORDER BY r.routine_name");

            return sb.Content;
        }

        private string GetSqlForExcludeExtensionsRoutines(string columnName, bool isFunction)
        {
            string sql = $@"AND {columnName} NOT IN(SELECT proname
                            FROM pg_catalog.pg_extension e
                            JOIN pg_catalog.pg_depend d ON d.refobjid = e.oid
                            JOIN pg_catalog.pg_proc p ON p.oid = d.objid
                            JOIN pg_catalog.pg_namespace ne ON ne.oid = e.extnamespace
                            JOIN pg_catalog.pg_namespace np ON np.oid = p.pronamespace
                            WHERE d.deptype = 'e' AND p.prokind = '{(isFunction ? "f" : "p")}')";

            return sql;
        }

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
            string routineType = isFunction ? "FUNCTION" : "PROCEDURE";

            sb.Append($@"SELECT p.specific_schema AS ""Schema"",r.routine_name AS ""RoutineName"",
                        p.parameter_name AS ""Name"",p.data_type AS ""DataType"",p.character_maximum_length AS ""MaxLength"",
                        p.numeric_precision AS ""Precision"", p.numeric_scale AS ""Scale"",
                        p.ordinal_position AS ""Order"",
                        CASE p.parameter_mode WHEN 'IN' THEN 0 ELSE 1 END AS ""IsOutput""
                        FROM information_schema.parameters p
                        JOIN information_schema.routines r ON p.specific_schema = r.specific_schema AND p.specific_name = r.specific_name
                        WHERE UPPER(p.specific_catalog) = UPPER('{this.ConnectionInfo.Database}') AND r.routine_type='{routineType}'");

            sb.Append(this.GetExcludeSystemSchemasCondition("p.specific_schema"));

            sb.Append(this.GetFilterSchemaCondition(filter, "p.specific_schema"));
            sb.Append(this.GetFilterNamesCondition(filter, routineNames, "r.routine_name"));

            if (Setting.ExcludePostgresExtensionObjects)
            {
                sb.Append(this.GetSqlForExcludeExtensionObjects(databaseObjectType, "r.routine_name"));
            }

            sb.Append("ORDER BY p.specific_name,p.ordinal_position");

            return sb.Content;
        }
        #endregion
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

            sb.Append($@"SELECT vt.view_catalog AS ""ObjectCatalog"", vt.view_schema AS ""ObjectSchema"",vt.view_name AS ""ObjectName"",
                        vt.table_catalog AS ""RefObjectCatalog"", vt.table_schema AS ""RefObjectSchema"", vt.table_name AS ""RefObjectName""
                        FROM INFORMATION_SCHEMA.VIEW_TABLE_USAGE vt
                        WHERE UPPER(vt.view_catalog) = UPPER('{this.ConnectionInfo.Database}')");

            string schemaColumn = !isFilterForReferenced ? "vt.view_schema" : "vt.table_schema";

            sb.Append(this.GetExcludeSystemSchemasCondition(schemaColumn));
            sb.Append(this.GetFilterSchemaCondition(filter, schemaColumn));
            sb.Append(this.GetFilterNamesCondition(filter, !isFilterForReferenced ? filter?.ViewNames : filter?.TableNames, !isFilterForReferenced ? "vt.view_name" : "vt.table_name"));

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

            sb.Append($@"SELECT vc.view_catalog AS ""ObjectCatalog"", vc.view_schema AS ""ObjectSchema"",vc.view_name AS ""ObjectName"",
                        vc.table_catalog AS ""RefObjectCatalog"", vc.table_schema AS ""RefObjectSchema"", vc.table_name AS ""RefObjectName"",vc.column_name AS ""ColumnName""
                        FROM INFORMATION_SCHEMA.VIEW_COLUMN_USAGE vc
                        WHERE UPPER(vc.view_catalog) = UPPER('{this.ConnectionInfo.Database}')");

            sb.Append(this.GetExcludeSystemSchemasCondition("vc.view_schema"));

            sb.Append(this.GetFilterSchemaCondition(filter, "vc.view_schema"));
            sb.Append(this.GetFilterNamesCondition(filter, filter?.ViewNames, "vc.view_name"));

            return sb.Content;
        }
        #endregion

        #region Routine Script Usage
        public override Task<List<RoutineScriptUsage>> GetRoutineScriptUsages(SchemaInfoFilter filter = null, bool isFilterForReferenced = false, bool includeViewTableUsages = false)
        {
            return base.GetDbObjectUsagesAsync<RoutineScriptUsage>("");
        }

        public override Task<List<RoutineScriptUsage>> GetRoutineScriptUsages(DbConnection dbConnection, SchemaInfoFilter filter = null, bool isFilterForReferenced = false, bool includeViewTableUsages = false)
        {
            return base.GetDbObjectUsagesAsync<RoutineScriptUsage>(dbConnection, "");
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
                bulkCopy.BulkCopyTimeout = bulkCopyInfo.Timeout.HasValue ? bulkCopyInfo.Timeout.Value : Setting.CommandTimeout; ;
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
                || item.DataType == typeof(byte[])
                || item.DataType == typeof(string)
                || item.DataType == typeof(Guid)
                || item.DataType == typeof(Decimal)
                || item.DataType == typeof(SqlHierarchyId)
                || DataTypeHelper.IsGeometryType(item.DataType.Name)
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
                                newColumnType = typeof(PgGeom.Geometry);

                                SqlGeography geography = (SqlGeography)value;

                                if (!geography.IsNull)
                                {
                                    if (dataType == "geography")
                                    {
                                        newValue = SqlGeographyHelper.ToPostgresGeography(geography);
                                    }
                                    else if (dataType == "geometry")
                                    {
                                        newValue = SqlGeographyHelper.ToPostgresGeometry(geography);
                                    }
                                }
                                else
                                {
                                    newValue = DBNull.Value;
                                }
                            }
                            else if (type == typeof(SqlGeometry))
                            {
                                newColumnType = typeof(PgGeom.Geometry);
                                SqlGeometry geometry = (SqlGeometry)value;

                                if (!geometry.IsNull)
                                {
                                    if (dataType == "geography")
                                    {
                                        newValue = SqlGeometryHelper.ToPostgresGeography(geometry);
                                    }
                                    else if (dataType == "geometry")
                                    {
                                        newValue = SqlGeometryHelper.ToPostgresGeometry(geometry);
                                    }
                                }
                                else
                                {
                                    newValue = DBNull.Value;
                                }
                            }
                            else if (type == typeof(byte[]))
                            {
                                DatabaseType sourcedDbType = bulkCopyInfo.SourceDatabaseType;

                                if (sourcedDbType == DatabaseType.MySql)
                                {
                                    if (dataType == "geography")
                                    {
                                        newColumnType = typeof(PgGeom.Geometry);
                                        newValue = MySqlGeometryHelper.ToPostgresGeography(value as byte[]);
                                    }
                                    else if (dataType == "geometry")
                                    {
                                        newColumnType = typeof(PgGeom.Geometry);
                                        newValue = MySqlGeometryHelper.ToPostgresGeometry(value as byte[]);
                                    }
                                }
                            }
                            else if (value is SdoGeometry)
                            {
                                newColumnType = typeof(PgGeom.Geometry);

                                if (dataType == "geography")
                                {
                                    newValue = OracleSdoGeometryHelper.ToPostgresGeography(value as SdoGeometry);
                                }
                                else if (dataType == "geometry")
                                {
                                    newValue = OracleSdoGeometryHelper.ToPostgresGeometry(value as SdoGeometry);
                                }
                            }
                            else if (value is StGeometry)
                            {
                                newColumnType = typeof(PgGeom.Geometry);

                                if (dataType == "geography")
                                {
                                    newValue = OracleStGeometryHelper.ToPostgresGeography(value as StGeometry);
                                }
                                else if (dataType == "geometry")
                                {
                                    newValue = OracleStGeometryHelper.ToPostgresGeometry(value as StGeometry);
                                }
                            }
                            else if (type == typeof(string))
                            {
                                if (dataType == "geography")
                                {
                                    newColumnType = typeof(PgGeom.Geometry);
                                    newValue = SqlGeometryHelper.ToPostgresGeography(value as string);
                                }
                                else if (dataType == "geometry")
                                {
                                    newColumnType = typeof(PgGeom.Geometry);
                                    newValue = SqlGeometryHelper.ToPostgresGeometry(value as string);
                                }
                            }
                            else if (type == typeof(Guid))
                            {
                                newColumnType = typeof(string);
                                newValue = value.ToString();
                            }
                            else if (type == typeof(decimal))
                            {
                                if (dataType == "real")
                                {
                                    newColumnType = typeof(float);
                                    newValue = Convert.ToSingle(value);
                                }
                                else if (dataType == "double precision")
                                {
                                    newColumnType = typeof(double);
                                    newValue = Convert.ToDouble(value);
                                }
                            }

                            if (DataTypeHelper.IsGeometryType(dataType) && newColumnType != null && newValue == null)
                            {
                                newValue = DBNull.Value;
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
            bool isLowDbVersion = this.IsLowDbVersion();
            string requiredClause = (column.IsRequired ? "NOT NULL" : "NULL");

            //the "GENERATED ALWAYS" can be used since v12.

            if (column.IsComputed)
            {
                if (!isLowDbVersion)
                {
                    return $"{this.GetQuotedString(column.Name)} {column.DataType} {requiredClause} GENERATED ALWAYS AS ({column.ComputeExp}) STORED";
                }
                else
                {
                    return $"{this.GetQuotedString(column.Name)} {column.DataType} NULL";
                }
            }
            else
            {
                string dataType = this.ParseDataType(column);               
                bool isIdentity = this.Option.TableScriptsGenerateOption.GenerateIdentity && column.IsIdentity;
                bool allowIdentity = this.AllowIdentity(column);

                string identityClause = "";

                if (isIdentity)
                {
                    if (!isLowDbVersion && allowIdentity)
                    {
                        identityClause = " GENERATED ALWAYS AS IDENTITY ";
                    }
                    else
                    {
                        if (dataType == "integer")
                        {
                            dataType = "serial";
                        }
                        else if (dataType == "bigint")
                        {
                            dataType = "bigserial";
                        }
                        else if (dataType == "smallint")
                        {
                            dataType = "smallserial";
                        }
                    }
                }

                string defaultValueClause = this.Option.TableScriptsGenerateOption.GenerateDefaultValue && !string.IsNullOrEmpty(column.DefaultValue) ? (" DEFAULT " + StringHelper.GetParenthesisedString(this.GetColumnDefaultValue(column))) : "";
                string scriptComment = string.IsNullOrEmpty(column.ScriptComment) ? "" : $"/*{column.ScriptComment}*/";

                string content = $"{this.GetQuotedString(column.Name)} {dataType}{defaultValueClause} {requiredClause}{identityClause}{scriptComment}";

                return content;
            }
        }

        public override string ParseDataType(TableColumn column)
        {
            if (DataTypeHelper.IsUserDefinedType(column))
            {
                return this.GetQuotedString(column.DataType);
            }

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

                    if (!string.IsNullOrEmpty(dataLength) && dataLength != "-1")
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

            DataTypeInfo dataTypeInfo = this.GetDataTypeInfo(dataType);
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

        private bool AllowIdentity(TableColumn column)
        {
            DataTypeSpecification dataTypeSpec = this.GetDataTypeSpecification(column.DataType.ToLower());

            if (dataTypeSpec != null)
            {
                return dataTypeSpec.AllowIdentity;
            }

            return false;
        }
        #endregion
    }
}
