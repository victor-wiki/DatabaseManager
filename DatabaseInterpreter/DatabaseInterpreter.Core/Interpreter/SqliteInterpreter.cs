using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class SqliteInterpreter : DbInterpreter
    {
        #region Field & Property      
        public override string CommentString => "--";
        public override string CommandParameterChar => "@";
        public override string UnicodeLeadingFlag => "";
        public override bool SupportQuotationChar => false;
        public override DatabaseType DatabaseType => DatabaseType.Sqlite;
        public override IndexType IndexType => IndexType.Primary | IndexType.Normal | IndexType.Unique;
        public override DatabaseObjectType SupportDbObjectType => DatabaseObjectType.Table | DatabaseObjectType.View;
        public override string DefaultDataType => "TEXT";
        public override string DefaultSchema => this.ConnectionInfo.Database;
        public override bool SupportBulkCopy => false;
        public override bool SupportNchar => false;
        public override string STR_CONCAT_CHARS => "||";
        #endregion

        #region Constructor
        public SqliteInterpreter(ConnectionInfo connectionInfo, DbInterpreterOption option) : base(connectionInfo, option)
        {
            this.dbConnector = this.GetDbConnector();
        }
        #endregion

        #region Schema Information

        #region Database & Schema
        public override async Task<List<Database>> GetDatabasesAsync()
        {
            var databases = new List<Database>() { new Database() { Name = this.ConnectionInfo.Database } };

            return await Task.Run(() => { return databases; });
        }

        public override async Task<List<DatabaseSchema>> GetDatabaseSchemasAsync()
        {
            string database = this.ConnectionInfo.Database;

            List<DatabaseSchema> databaseSchemas = new List<DatabaseSchema>() { new DatabaseSchema() { Schema = database, Name = database } };

            return await Task.Run(() => { return databaseSchemas; });
        }

        public override Task<List<DatabaseSchema>> GetDatabaseSchemasAsync(DbConnection dbConnection)
        {
            return this.GetDatabaseSchemasAsync();
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
            return this.GetSqlForTableViews(DatabaseObjectType.Table, filter);
        }

        private string GetSqlForTableViews(DatabaseObjectType dbObjectType, SchemaInfoFilter filter = null, bool includeDefinition = false)
        {
            SqlBuilder sb = new SqlBuilder();

            string type = dbObjectType.ToString().ToLower();
            string[] objectNames = null;
            bool isScriptDbObject = false;
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            if (dbObjectType == DatabaseObjectType.Table)
            {
                objectNames = filter?.TableNames;
            }
            else if (dbObjectType == DatabaseObjectType.View)
            {
                objectNames = filter?.ViewNames;
                isScriptDbObject = true;
            }

            string definition = ((isScriptDbObject && !isSimpleMode) || includeDefinition) ? ",sql as Definition" : "";

            sb.Append($@"SELECT name AS Name{definition}                         
                        FROM sqlite_schema WHERE type= '{type}'");

            sb.Append(this.GetFilterNamesCondition(filter, objectNames, "name"));

            sb.Append("ORDER BY name");

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
            return this.GetSqlForTableViews(DatabaseObjectType.View, filter);
        }
        #endregion

        #region Trigger
        public override Task<List<TableTrigger>> GetTableTriggersAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableTrigger>(this.GetSqlForTriggers(filter));
        }

        public override Task<List<TableTrigger>> GetTableTriggersAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<TableTrigger>(dbConnection, this.GetSqlForTriggers(filter));
        }

        private string GetSqlForTriggers(SchemaInfoFilter filter = null)
        {
            return this.GetSqlForTableChildren(DatabaseObjectType.Trigger, filter);
        }

        private string GetSqlForTableChildren(DatabaseObjectType dbObjectType, SchemaInfoFilter filter = null)
        {
            SqlBuilder sb = new SqlBuilder();

            string type = dbObjectType.ToString().ToLower();
            bool isScriptDbObject = false;
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string[] tableNames = filter?.TableNames;
            string[] childrenNames = null;


            if (dbObjectType == DatabaseObjectType.Trigger)
            {
                isScriptDbObject = true;
                childrenNames = filter?.TableTriggerNames;
            }
            else if (dbObjectType == DatabaseObjectType.Column)
            {
                childrenNames = filter?.TableTriggerNames;
            }

            string definition = isScriptDbObject && !isSimpleMode ? ",sql as Definition" : "";

            string unique = dbObjectType == DatabaseObjectType.Index ? ",CASE WHEN INSTR(sql, 'UNIQUE')>0 THEN 1 ELSE 0 END AS IsUnique" : "";

            sb.Append($@"SELECT name AS Name,tbl_name AS TableName{definition}{unique}                         
                        FROM sqlite_schema WHERE type= '{type}'");

            sb.Append(this.GetFilterNamesCondition(filter, tableNames, "tbl_name"));
            sb.Append(this.GetFilterNamesCondition(filter, childrenNames, "name"));

            if (dbObjectType == DatabaseObjectType.Index)
            {
                sb.Append("AND name not like 'sqlite_autoindex%'");
            }

            sb.Append("ORDER BY tbl_name,name");

            return sb.Content;
        }
        #endregion

        #region Column
        public override Task<List<TableColumn>> GetTableColumnsAsync(SchemaInfoFilter filter = null)
        {
            return this.GetTableColumnsAsync(this.CreateConnection(), filter);
        }

        public override async Task<List<TableColumn>> GetTableColumnsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            if (filter?.TableNames == null)
            {
                if(filter == null)
                {
                    filter = new SchemaInfoFilter();
                }               

                filter.TableNames= (await this.GetTableNamesAsync());
            }

            var columns = await base.GetDbObjectsAsync<TableColumn>(dbConnection, this.GetSqlForTableColumns(filter));

            return columns;
        }

        private string GetSqlForTableColumns(SchemaInfoFilter filter = null)
        {
            SqlBuilder sb = new SqlBuilder();

            string[] tableNames = filter?.TableNames;

            if(tableNames!=null)
            {
                for (int i = 0; i < tableNames.Length; i++)
                {
                    string tableName = tableNames[i];

                    if (i > 0)
                    {
                        sb.Append("UNION ALL");
                    }

                    sb.Append($@"SELECT name AS Name,'{tableName}' AS TableName,
                                TRIM(REPLACE(type,'AUTO_INCREMENT','')) AS DataType,
                                CASE WHEN INSTR(UPPER(type),'NUMERIC')>=1 AND INSTR(type,'(')>0 THEN CAST(TRIM(SUBSTR(type,INSTR(type,'(')+1,IIF(INSTR(type,',')==0, INSTR(type,')'),INSTR(type,','))-INSTR(type,'(')-1)) AS INTEGER) ELSE NULL END AS Precision,
                                CASE WHEN INSTR(UPPER(type),'NUMERIC')>=1 AND INSTR(type,',')>0 THEN CAST(TRIM(SUBSTR(type,INSTR(type,',')+1,INSTR(type,')')-INSTR(type,',')-1)) AS INTEGER) ELSE NULL END AS Scale,
                                CASE WHEN INSTR(type,'AUTO_INCREMENT')>0 THEN 1 ELSE 0 END AS IsIdentity,
                                CASE WHEN ""notnull""=1 THEN 0 ELSE 1 END AS IsNullable,
                                dflt_value AS DefaultValue, pk AS IsPrimaryKey, cid AS ""Order""
                                FROM PRAGMA_TABLE_INFO('{tableName}')");
                }
            }        
            
            return sb.Content;
        }
        #endregion

        #region Primary Key
        public override Task<List<TablePrimaryKeyItem>> GetTablePrimaryKeyItemsAsync(SchemaInfoFilter filter = null)
        {
            return this.GetTablePrimaryKeyItemsAsync(this.CreateConnection(), filter);
        }

        public override async Task<List<TablePrimaryKeyItem>> GetTablePrimaryKeyItemsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            if (filter?.TableNames == null)
            {
                if (filter == null)
                {
                    filter = new SchemaInfoFilter();
                }

                filter.TableNames = (await this.GetTableNamesAsync());
            }

            List<TablePrimaryKeyItem> primaryKeyItems = await base.GetDbObjectsAsync<TablePrimaryKeyItem>(dbConnection, this.GetSqlForPrimaryKeys(filter));

            await this.MakeupTableChildrenNames(primaryKeyItems, filter);

            return primaryKeyItems;
        }

        private string GetSqlForPrimaryKeys(SchemaInfoFilter filter = null)
        {
            SqlBuilder sb = new SqlBuilder();

            string[] tableNames = filter?.TableNames;

            if (tableNames != null)
            {
                for (int i = 0; i < tableNames.Length; i++)
                {
                    string tableName = tableNames[i];

                    if (i > 0)
                    {
                        sb.Append("UNION ALL");
                    }

                    sb.Append($@"SELECT '{tableName}' as TableName, name AS ColumnName
                        FROM PRAGMA_TABLE_INFO('{tableName}')
                        WHERE pk>0");
                }
            }

            return sb.Content;
        }

        private async Task MakeupTableChildrenNames(IEnumerable<TableColumnChild> columnChildren, SchemaInfoFilter filter = null)
        {
            if (columnChildren == null || columnChildren.Count() == 0)
            {
                return;
            }

            var tablesSql = this.GetSqlForTableViews(DatabaseObjectType.Table, filter, true);

            var tables = await this.GetDbObjectsAsync<Table>(tablesSql);

            foreach (var table in tables)
            {
                string tableName = table.Name;
                string definition = table.Definition;

                List<List<string>> columnDetails = null;

                string name = this.ExtractNameFromTableDefinition(definition, DbObjectHelper.GetDatabaseObjectType(columnChildren.FirstOrDefault()), out columnDetails);

                if (!string.IsNullOrEmpty(name))
                {
                    var children = columnChildren.Where(item => item.TableName == tableName);

                    foreach (var child in children)
                    {
                        child.Name = name;
                    }
                }
            }
        }

        private async Task<List<DatabaseObject>> GetTableChildren(DatabaseObjectType dbObjectType, DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            List<DatabaseObject> databaseObjects = new List<DatabaseObject>();

            if (dbConnection == null)
            {
                dbConnection = this.CreateConnection();
            }

            var tablesSql = this.GetSqlForTableViews(DatabaseObjectType.Table, filter, true);

            var tables = await this.GetDbObjectsAsync<Table>(dbConnection, tablesSql);

            string flag = "";

            if (dbObjectType == DatabaseObjectType.Index) //unique
            {
                flag = "UNIQUE";
            }
            else if (dbObjectType == DatabaseObjectType.Constraint) //check constraint
            {
                flag = "CHECK";
            }

            foreach (var table in tables)
            {
                string tableName = table.Name;
                string definition = table.Definition;

                List<List<string>> columns = this.GetTableColumnDetails(definition);

                IEnumerable<List<string>> matchedColumns = columns.Where(item => item.Any(t => t.Trim().ToUpper().StartsWith(flag)));

                foreach (List<string> columnItems in matchedColumns)
                {
                    bool isTableConstraint = columnItems.First().ToUpper().Trim() == "CONSTRAINT";

                    string objName = null;
                    List<string> columNames = new List<string>();

                    int index = this.FindIndexInList(columnItems, flag);

                    string name = this.ExtractNameFromColumnDefintion(columnItems, dbObjectType);

                    if (!string.IsNullOrEmpty(name))
                    {
                        objName = name;
                    }

                    if (isTableConstraint)
                    {
                        if (dbObjectType == DatabaseObjectType.Index)
                        {
                            columNames = this.ExtractColumnNamesFromTableConstraint(columnItems, index);
                        }
                    }
                    else
                    {
                        columNames.Add(columnItems.First());
                    }

                    if (dbObjectType == DatabaseObjectType.Index) //unique
                    {
                        for (int i = 0; i < columNames.Count; i++)
                        {
                            TableIndexItem tableIndexItem = new TableIndexItem()
                            {
                                Name = objName,
                                ColumnName = columNames[i],
                                TableName = table.Name,
                                Type = "Unique",
                                IsUnique = true,
                                Order = i + 1
                            };

                            databaseObjects.Add(tableIndexItem);
                        }
                    }
                    else if (dbObjectType == DatabaseObjectType.Constraint) //check constraint
                    {
                        string columnName = columNames.FirstOrDefault();

                        TableConstraint tableConstraint = new TableConstraint()
                        {
                            Name = objName,
                            ColumnName = columnName,
                            TableName = table.Name,
                            Definition = this.ExtractDefinitionFromTableConstraint(columnItems, index)
                        };

                        databaseObjects.Add(tableConstraint);
                    }
                }
            }

            return databaseObjects;
        }
        #endregion

        #region Foreign Key
        public override Task<List<TableForeignKeyItem>> GetTableForeignKeyItemsAsync(SchemaInfoFilter filter = null, bool isFilterForReferenced = false)
        {
            return this.GetTableForeignKeyItemsAsync(this.CreateConnection(), filter, isFilterForReferenced);
        }

        public override async Task<List<TableForeignKeyItem>> GetTableForeignKeyItemsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null, bool isFilterForReferenced = false)
        {
            if (filter?.TableNames == null)
            {
                if (filter == null)
                {
                    filter = new SchemaInfoFilter();
                }

                filter = new SchemaInfoFilter() { TableNames =  await GetTableNamesAsync() };
            }

            List<TableForeignKeyItem> foreignKeyItems = await base.GetDbObjectsAsync<TableForeignKeyItem>(dbConnection, this.GetSqlForForeignKeys(filter, isFilterForReferenced));

            await this.MakeupTableChildrenNames(foreignKeyItems, filter);

            return foreignKeyItems;
        }

        private async Task<string[]> GetTableNamesAsync()
        {
            var tables = await this.GetTablesAsync();

            return tables.Select(item => item.Name).ToArray();
        }

        private string GetSqlForForeignKeys(SchemaInfoFilter filter = null, bool isFilterForReferenced = false)
        {
            SqlBuilder sb = new SqlBuilder();

            if (!isFilterForReferenced)
            {
                string[] tableNames = filter?.TableNames;

                if (tableNames != null)
                {
                    for (int i = 0; i < tableNames.Length; i++)
                    {
                        string tableName = tableNames[i];

                        if (i > 0)
                        {
                            sb.Append("UNION ALL");
                        }

                        sb.Append($@"SELECT ""table"" AS ReferencedTableName, ""to"" AS ReferencedColumnName,
                                '{tableName}' AS TableName, ""from"" AS ColumnName,
                                CASE WHEN on_update='CASCADE' THEN 1 ELSE 0 END AS UpdateCascade,
                                CASE WHEN on_delete='CASCADE' THEN 1 ELSE 0 END AS DeleteCascade
                                FROM PRAGMA_foreign_key_list('{tableName}')");
                    }
                }
            }

            return sb.Content;
        }
        #endregion

        #region Index
        public override Task<List<TableIndexItem>> GetTableIndexItemsAsync(SchemaInfoFilter filter = null, bool includePrimaryKey = false)
        {
            return this.GetTableIndexItemsAsync(this.CreateConnection(), filter);
        }

        public override async Task<List<TableIndexItem>> GetTableIndexItemsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null, bool includePrimaryKey = false)
        {
            var items = await base.GetDbObjectsAsync<TableIndexItem>(dbConnection, this.GetSqlForIndexes(filter));

            items.ForEach(item =>
            {
                if(item.Type == null)
                {
                    if(item.IsUnique)
                    {
                        item.Type = "Unique";
                    }
                    else
                    {
                        item.Type = "Normal";
                    }
                }
            });

            var columns = await base.GetDbObjectsAsync<TableIndexItem>(this.GetSqlForIndexColumns(items));

            foreach (var item in items)
            {
                var column = columns.FirstOrDefault(item => item.Name == item.Name);

                if (column != null)
                {
                    item.ColumnName = column.ColumnName;
                }
            }

            var uniqueIndexes = await this.GetTableChildren(DatabaseObjectType.Index, null, filter);

            List<TableIndexItem> uniqueItems = new List<TableIndexItem>();

            foreach (var unique in uniqueIndexes)
            {
                if (!string.IsNullOrEmpty(unique.Name) && items.Any(item => item.TableName == (unique as TableIndexItem).TableName && item.Name == unique.Name))
                {
                    continue;
                }

                uniqueItems.Add(unique as TableIndexItem);
            }

            items.AddRange(uniqueItems);

            return items;
        }

        private string GetSqlForIndexes(SchemaInfoFilter filter = null)
        {
            return this.GetSqlForTableChildren(DatabaseObjectType.Index, filter);
        }

        private string GetSqlForIndexColumns(List<TableIndexItem> indexes)
        {
            SqlBuilder sb = new SqlBuilder();

            int i = 0;

            foreach (var item in indexes)
            {
                if (i > 0)
                {
                    sb.Append("UNION ALL");
                }

                sb.Append($"SELECT '{item.Name}' AS Name, name AS ColumnName FROM PRAGMA_INDEX_INFO('{item.Name}')");

                i++;
            }

            return sb.Content;
        }
        #endregion   

        #region Constraint
        public override Task<List<TableConstraint>> GetTableConstraintsAsync(SchemaInfoFilter filter = null)
        {
            return this.GetTableConstraintsAsync(this.CreateConnection(), filter);
        }

        public override async Task<List<TableConstraint>> GetTableConstraintsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            List<TableConstraint> constraints = new List<TableConstraint>();

            List<DatabaseObject> dbObjects = await this.GetTableChildren(DatabaseObjectType.Constraint, dbConnection, filter);

            foreach (var dbObject in dbObjects)
            {
                constraints.Add(dbObject as TableConstraint);
            }

            return constraints;
        }
        #endregion

        #region Sequence
        public override Task<List<Sequence>> GetSequencesAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Sequence>("");
        }

        public override Task<List<Sequence>> GetSequencesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Sequence>(dbConnection, "");
        }
        #endregion

        #region User Defined Type
        public override Task<List<UserDefinedTypeAttribute>> GetUserDefinedTypeAttributesAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<UserDefinedTypeAttribute>("");
        }

        public override Task<List<UserDefinedTypeAttribute>> GetUserDefinedTypeAttributesAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<UserDefinedTypeAttribute>(dbConnection, "");
        }
        #endregion        

        #region Function
        public override Task<List<Function>> GetFunctionsAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Function>("");
        }

        public override Task<List<Function>> GetFunctionsAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Function>(dbConnection, "");
        }
        #endregion

        #region Procedure
        public override Task<List<Procedure>> GetProceduresAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Procedure>("");
        }

        public override Task<List<Procedure>> GetProceduresAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<Procedure>(dbConnection, "");
        }
        #endregion     

        #region Routine Parameter
        public override Task<List<RoutineParameter>> GetFunctionParametersAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<RoutineParameter>("");
        }

        public override Task<List<RoutineParameter>> GetFunctionParametersAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<RoutineParameter>(dbConnection, "");
        }

        public override Task<List<RoutineParameter>> GetProcedureParametersAsync(SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<RoutineParameter>("");
        }

        public override Task<List<RoutineParameter>> GetProcedureParametersAsync(DbConnection dbConnection, SchemaInfoFilter filter = null)
        {
            return base.GetDbObjectsAsync<RoutineParameter>(dbConnection, "");
        }
        #endregion        

        #endregion

        #region Dependency
        #region View->Column Usage
        public override Task<List<ViewColumnUsage>> GetViewColumnUsages(SchemaInfoFilter filter)
        {
            return base.GetDbObjectUsagesAsync<ViewColumnUsage>("");
        }

        public override Task<List<ViewColumnUsage>> GetViewColumnUsages(DbConnection dbConnection, SchemaInfoFilter filter)
        {
            return base.GetDbObjectUsagesAsync<ViewColumnUsage>(dbConnection, "");
        }
        #endregion

        #region View->Table Usage
        public override Task<List<ViewTableUsage>> GetViewTableUsages(SchemaInfoFilter filter, bool isFilterForReferenced = false)
        {
            return base.GetDbObjectUsagesAsync<ViewTableUsage>("");
        }

        public override Task<List<ViewTableUsage>> GetViewTableUsages(DbConnection dbConnection, SchemaInfoFilter filter, bool isFilterForReferenced = false)
        {
            return base.GetDbObjectUsagesAsync<ViewTableUsage>(dbConnection, "");
        }
        #endregion

        #region Routine Script Usage
        public override Task<List<RoutineScriptUsage>> GetRoutineScriptUsages(SchemaInfoFilter filter, bool isFilterForReferenced = false, bool includeViewTableUsages = false)
        {
            return base.GetDbObjectUsagesAsync<RoutineScriptUsage>("");
        }

        public override Task<List<RoutineScriptUsage>> GetRoutineScriptUsages(DbConnection dbConnection, SchemaInfoFilter filter, bool isFilterForReferenced = false, bool includeViewTableUsages = false)
        {
            return base.GetDbObjectUsagesAsync<RoutineScriptUsage>(dbConnection, "");
        }
        #endregion
        #endregion

        #region BulkCopy
        public override Task BulkCopyAsync(DbConnection connection, DataTable dataTable, BulkCopyInfo bulkCopyInfo)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Common Method
        public override bool IsLowDbVersion(string serverVersion)
        {
            return this.IsLowDbVersion(serverVersion, "3");
        }

        public override DbConnector GetDbConnector()
        {
            return new DbConnector(new SqliteProvider(), new SqliteConnectionStringBuilder(), this.ConnectionInfo);
        }

        private List<List<string>> GetTableColumnDetails(string definition)
        {
            List<List<string>> columnDetails = new List<List<string>>();

            int firstIndex = definition.IndexOf("(");
            int lastIndex = definition.LastIndexOf(")", definition.Length - 1);

            string innerContent = definition.Substring(firstIndex + 1, lastIndex - firstIndex - 1);

            int singleQuotationCharCount = 0;
            int leftParenthesisesCount = 0;
            int rightParenthesisesCount = 0;

            StringBuilder sb = new StringBuilder();

            #region Extract Columns
            List<string> columns = new List<string>();

            for (int i = 0; i < innerContent.Length; i++)
            {
                var c = innerContent[i];

                if (c == '\'')
                {
                    singleQuotationCharCount++;
                }
                else if (c == '(')
                {
                    leftParenthesisesCount++;
                }
                else if (c == ')')
                {
                    rightParenthesisesCount++;
                }
                else if (c == ',')
                {
                    if (singleQuotationCharCount % 2 == 0 && leftParenthesisesCount == rightParenthesisesCount)
                    {
                        columns.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                }

                sb.Append(c);
            }

            if (sb.Length > 0)
            {
                columns.Add(sb.ToString());
            }
            #endregion

            sb.Clear();

            #region Parse Columns
            foreach (string column in columns)
            {
                singleQuotationCharCount = 0;
                leftParenthesisesCount = 0;
                rightParenthesisesCount = 0;

                List<string> columnItems = new List<string>();

                foreach (var c in column)
                {
                    if (c == '\'')
                    {
                        singleQuotationCharCount++;
                    }
                    else if (c == '(')
                    {
                        leftParenthesisesCount++;
                    }
                    else if (c == ')')
                    {
                        rightParenthesisesCount++;
                    }
                    else if (c == ' ')
                    {
                        if (singleQuotationCharCount % 2 == 0 && leftParenthesisesCount == rightParenthesisesCount)
                        {
                            string item = sb.ToString().Trim();

                            if (item.Length > 0)
                            {
                                columnItems.Add(item);
                            }

                            sb.Clear();
                            continue;
                        }
                    }

                    sb.Append(c);
                }

                if (sb.Length > 0)
                {
                    columnItems.Add(sb.ToString().Trim());
                    sb.Clear();
                }

                columnDetails.Add(columnItems);
            }
            #endregion

            sb.Clear();

            return columnDetails;
        }

        private string ExtractNameFromTableDefinition(string definition, DatabaseObjectType dbObjectType, out List<List<string>> columnDetails)
        {
            columnDetails = this.GetTableColumnDetails(definition);

            foreach (List<string> item in columnDetails)
            {
                string name = this.ExtractNameFromColumnDefintion(item, dbObjectType);

                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }

            return string.Empty;
        }

        private string ExtractNameFromColumnDefintion(List<string> columnItems, DatabaseObjectType dbObjectType)
        {
            IEnumerable<int> indexes = this.FindAllIndexesInList(columnItems, "CONSTRAINT");

            if (indexes.Count() > 0)
            {
                bool matched = false;
                int index = -1;

                if (dbObjectType == DatabaseObjectType.PrimaryKey && (index= this.FindIndexInList(columnItems, "PRIMARY")) > 0)
                {
                    matched = true;
                }
                else if (dbObjectType == DatabaseObjectType.ForeignKey && (index = this.FindIndexInList(columnItems, "REFERENCES")) > 0)
                {
                    matched = true;
                }
                else if (dbObjectType == DatabaseObjectType.Constraint && (index = this.FindIndexInList(columnItems, "CHECK")) > 0)
                {
                    matched = true;
                }
                else if (dbObjectType == DatabaseObjectType.Index && (index = this.FindIndexInList(columnItems, "UNIQUE")) > 0)
                {
                    matched = true;
                }

                if (matched)
                {
                    int closestConstraintIndex = indexes.Where(item => item < index).Max();

                    return this.ExtractTableChildName(columnItems, closestConstraintIndex + 1);
                }
            }

            return string.Empty;
        }

        private List<string> ExtractColumnNamesFromTableConstraint(List<string> columnItems, int startIndex)
        {
            List<string> columNames = new List<string>();

            string item = columnItems[startIndex];

            if (item.Contains("("))
            {
                int index = item.IndexOf("(");

                columNames = item.Substring(index + 1).Trim().TrimEnd(')').Split(',').Select(item => item.Trim()).ToList();
            }
            else
            {
                item = columnItems.Skip(startIndex + 1).FirstOrDefault(item => item.Trim().StartsWith("("));

                columNames = item.Trim().Trim('(', ')').Split(',').Select(item => item.Trim()).ToList();
            }

            return columNames;
        }

        private string ExtractDefinitionFromTableConstraint(List<string> columnItems, int startIndex)
        {
            string item = columnItems[startIndex];

            if (item.Contains("("))
            {
                int index = item.IndexOf("(");

                return item.Substring(index + 1);
            }
            else
            {
                return columnItems.Skip(startIndex + 1).FirstOrDefault(item => item.Trim().Length > 0);
            }
        }

        private int FindIndexInList(List<string> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string item = list[i].Trim().ToUpper();

                if (item == value || item.StartsWith($"{value}("))
                {
                    return i;
                }
            }

            return -1;
        }

        private IEnumerable<int> FindAllIndexesInList(List<string> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string item = list[i].Trim().ToUpper();

                if (item == value || item.StartsWith($"{value}("))
                {
                    yield return i;
                }
            }
        }

        private string ExtractTableChildName(List<string> items, int startIndex)
        {
            var keywords = KeywordManager.GetKeywords(this.DatabaseType);

            return items.Skip(startIndex).FirstOrDefault(item => item.Length > 0 && !keywords.Contains(item));
        }
        #endregion

        #region Parse Column & DataType
        public override string ParseColumn(Table table, TableColumn column)
        {
            string dataType = this.ParseDataType(column);
            string requiredClause = (column.IsRequired ? "NOT NULL" : "NULL");

            if (column.IsComputed)
            {
                return $"{this.GetQuotedString(column.Name)} {dataType} GENERATED ALWAYS AS ({column.ComputeExp}) STORED {requiredClause}";
            }
            else
            {
                string identityClause = (this.Option.TableScriptsGenerateOption.GenerateIdentity && column.IsIdentity ? $"AUTO_INCREMENT" : "");
                string commentClause = (!string.IsNullOrEmpty(column.Comment) && this.Option.TableScriptsGenerateOption.GenerateComment ? $"COMMENT '{this.ReplaceSplitChar(ValueHelper.TransferSingleQuotation(column.Comment))}'" : "");
                string defaultValueClause = this.Option.TableScriptsGenerateOption.GenerateDefaultValue && !string.IsNullOrEmpty(column.DefaultValue) && !ValueHelper.IsSequenceNextVal(column.DefaultValue) ? (" DEFAULT " + StringHelper.GetParenthesisedString(this.GetColumnDefaultValue(column))) : "";
                string scriptComment = string.IsNullOrEmpty(column.ScriptComment) ? "" : $"/*{column.ScriptComment}*/";

                return $"{this.GetQuotedString(column.Name)} {dataType} {identityClause} {requiredClause} {defaultValueClause} {scriptComment}{commentClause}";
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
                else if (args == "precision,scale")
                {
                    if (dataTypeInfo.Args != null && !dataTypeInfo.Args.Contains(","))
                    {
                        return $"{column.Precision ?? 0},{column.Scale ?? 0}";
                    }
                    else if (column.Precision.HasValue && column.Scale.HasValue)
                    {
                        return $"{column.Precision},{column.Scale}";
                    }
                }
            }

            return string.Empty;
        }
        #endregion

        #region Sql Query Clause
        protected override string GetSqlForPagination(string tableName, string columnNames, string orderColumns, string whereClause, long pageNumber, int pageSize)
        {
            var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

            string orderByColumns = (!string.IsNullOrEmpty(orderColumns) ? orderColumns : this.GetDefaultOrder());

            string orderBy = !string.IsNullOrEmpty(orderByColumns) ? $" ORDER BY {orderByColumns}" : "";

            SqlBuilder sb = new SqlBuilder();

            sb.Append($@"SELECT {columnNames}
							  FROM {tableName}
                             {whereClause} 
                             {orderBy}
                             LIMIT {pageSize} OFFSET {startEndRowNumber.StartRowNumber - 1}");

            return sb.Content;
        }
        #endregion
    }
}
