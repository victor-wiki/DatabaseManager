using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static DatabaseInterpreter.Model.Table;

namespace DatabaseManager.Core
{
    public class ScriptGenerator
    {
        private DbInterpreter dbInterpreter;

        public ScriptGenerator(DbInterpreter dbInterpreter)
        {
            this.dbInterpreter = dbInterpreter;
        }

        public async Task<ScriptGenerateResult> Generate(DatabaseObject dbObject, ScriptAction scriptAction)
        {
            ScriptGenerateResult result = new ScriptGenerateResult();

            string typeName = dbObject.GetType().Name;

            var databaseObjectType = DbObjectHelper.GetDatabaseObjectType(dbObject);

            ColumnType columnType = ColumnType.TableColumn;

            if (dbObject is Table)
            {
                if (scriptAction == ScriptAction.CREATE)
                {
                    dbInterpreter.Option.GetTableAllObjects = true;
                }
                else
                {
                    databaseObjectType |= DatabaseObjectType.Column;
                }
            }
            else if (dbObject is View)
            {
                if (scriptAction == ScriptAction.SELECT)
                {
                    databaseObjectType |= DatabaseObjectType.Column;

                    columnType = ColumnType.ViewColumn;
                }
            }

            SchemaInfoFilter filter = new SchemaInfoFilter() { DatabaseObjectType = databaseObjectType, ColumnType = columnType };

            filter.Schema = dbObject.Schema;

            if (columnType == ColumnType.ViewColumn)
            {
                filter.TableNames = new string[] { dbObject.Name };
            }
            else
            {
                filter.GetType().GetProperty($"{typeName}Names").SetValue(filter, new string[] { dbObject.Name });
            }

            if (dbObject is Function func)
            {
                if (scriptAction == ScriptAction.SELECT)
                {
                    return await this.GenerateRoutineCallScript(dbObject, filter);
                }
            }
            else if (dbObject is Procedure proc)
            {
                if (scriptAction == ScriptAction.EXECUTE)
                {
                    return await this.GenerateRoutineCallScript(dbObject, filter);
                }
            }

            SchemaInfo schemaInfo = await dbInterpreter.GetSchemaInfoAsync(filter);

            if (scriptAction == ScriptAction.CREATE || scriptAction == ScriptAction.ALTER)
            {
                DatabaseType databaseType = this.dbInterpreter.DatabaseType;

                if (scriptAction == ScriptAction.CREATE && dbObject is Table t)
                {
                    Table table = schemaInfo.Tables.FirstOrDefault(item => item.Name == t.Name && item.Schema == t.Schema);

                    if (table != null)
                    {
                        bool isPartitionedTable = await this.dbInterpreter.IsPartitionedTable(table);

                        await SetTableExtraInfo(this.dbInterpreter, schemaInfo, table, isPartitionedTable);
                    }
                }

                DbScriptGenerator dbScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(dbInterpreter);

                List<Script> scripts = dbScriptGenerator.GenerateSchemaScripts(schemaInfo).Scripts;

                StringBuilder sbContent = new StringBuilder();

                foreach (Script script in scripts)
                {
                    if (databaseType == DatabaseType.SqlServer && script is SpliterScript)
                    {
                        continue;
                    }

                    string content = script.Content;

                    if (scriptAction == ScriptAction.ALTER && typeName != nameof(Table))
                    {
                        string objType = typeName;

                        if (typeName == nameof(TableTrigger))
                        {
                            objType = "TRIGGER";
                        }

                        string createFlag = "CREATE ";
                        int createFlagIndex = this.GetCreateIndex(content, createFlag);

                        if (createFlagIndex >= 0)
                        {
                            switch (databaseType)
                            {
                                case DatabaseType.SqlServer:
                                    content = content.Substring(0, createFlagIndex) + "ALTER " + content.Substring(createFlagIndex + createFlag.Length);
                                    break;
                                case DatabaseType.MySql:
                                    content = $"DROP {objType} IF EXISTS {this.dbInterpreter.GetQuotedString(dbObject.Name)};" + Environment.NewLine + content;
                                    break;
                                case DatabaseType.Oracle:
                                    if (!Regex.IsMatch(content, @"^(CREATE[\s]+OR[\s]+REPLACE[\s]+)", RegexOptions.IgnoreCase))
                                    {
                                        content = content.Substring(0, createFlagIndex) + "CREATE OR REPLACE " + content.Substring(createFlagIndex + createFlag.Length);
                                    }
                                    break;
                            }
                        }
                    }

                    sbContent.AppendLine(content);
                }

                result.Script = StringHelper.ToSingleEmptyLine(sbContent.ToString().Trim());
            }
            else if (dbObject is Table table)
            {
                if (scriptAction == ScriptAction.SELECT || scriptAction == ScriptAction.INSERT || scriptAction == ScriptAction.UPDATE || scriptAction == ScriptAction.DELETE)
                {
                    result.Script = this.GenerateTableDMLScript(schemaInfo, table, scriptAction);
                }
                else if (scriptAction == ScriptAction.CREATE_PROCEDURE_INSERT || scriptAction == ScriptAction.CREATE_PROCEDURE_UPDATE || scriptAction == ScriptAction.CREATE_PROCEDURE_DELETE)
                {
                    result.Script = await this.GenerateTableProcedureScript(schemaInfo, table, scriptAction);
                }
            }
            else if (dbObject is View view)
            {
                result.Script = this.GenerateViewDMLScript(schemaInfo, view, scriptAction);
            }

            return result;
        }

        public static async Task<List<Table>> HandlePartitionedTable(DbInterpreter dbInterpreter, SchemaInfo schemaInfo, SchemaInfoFilter filter)
        {
            DatabaseType databaseType = dbInterpreter.DatabaseType;

            var partitionedTables = await dbInterpreter.GetPartitionedTables(filter);

            foreach (var partitionTable in partitionedTables)
            {
                Table table = schemaInfo.Tables.FirstOrDefault(item => (item.Schema == partitionTable.Schema || partitionTable.Schema == null) && item.Name == partitionTable.Name);

                if (table != null)
                {
                    await SetTableExtraInfo(dbInterpreter, schemaInfo, table, true);
                }
            }

            if (databaseType == DatabaseType.Postgres)
            {
                var partitionInfos = (await (dbInterpreter as PostgresInterpreter).GetPartitionInfos(filter, true));

                foreach (var partitionInfo in partitionInfos)
                {
                    var table = schemaInfo.Tables.Where(item => partitionInfo.TableSchema == item.Schema && partitionInfo.TableName == item.Name).FirstOrDefault();

                    if (table != null)
                    {
                        SetTableExtraInfoForPostgresTable(table, true, partitionInfo);
                    }
                }
            }

            return partitionedTables;
        }

        public static async Task SetTableExtraInfo(DbInterpreter dbInterpreter, SchemaInfo schemaInfo, Table table, bool isPartitionedTable)
        {
            DatabaseType databaseType = dbInterpreter.DatabaseType;

            table.ExtraInfo = new TableExtraInfo() { IsPartitioned = isPartitionedTable };

            if (databaseType == DatabaseType.SqlServer)
            {
                SqlServerInterpreter sqlserverInterpreter = dbInterpreter as SqlServerInterpreter;

                if (!isPartitionedTable)
                {
                    table.ExtraInfo.FilegroupName = await sqlserverInterpreter.GetTableFilegroupName(table);
                }
                else
                {
                    table.ExtraInfo.PartitionScheme = await sqlserverInterpreter.GetPartitionSchemeByTable(table);
                }
            }
            else if (databaseType == DatabaseType.Oracle)
            {
                OracleInterpreter oracleInterpreter = dbInterpreter as OracleInterpreter;

                if (isPartitionedTable)
                {
                    table.ExtraInfo.PartitionSummary = await oracleInterpreter.GetPartitionSummary(table, true);
                }

                var tableIndexExtraInfos = await oracleInterpreter.GetTableIndexExtraInfos(table, true);

                foreach (var indexExtraInfo in tableIndexExtraInfos)
                {
                    var index = schemaInfo.TableIndexes.FirstOrDefault(item => item.TableName == table.Name && item.Schema == table.Schema);

                    if (index != null)
                    {
                        index.ExtraInfo = indexExtraInfo;
                    }
                }
            }
            else if (databaseType == DatabaseType.MySql)
            {
                MySqlInterpreter mySqlInterpreter = dbInterpreter as MySqlInterpreter;

                if (isPartitionedTable)
                {
                    table.ExtraInfo.PartitionSummary = await mySqlInterpreter.GetPartitionSummary(table, true);
                }
            }
            else if (databaseType == DatabaseType.Postgres)
            {
                PostgresInterpreter postgresInterpreter = dbInterpreter as PostgresInterpreter;

                if (isPartitionedTable)
                {
                    table.ExtraInfo.PartitionSummary = await postgresInterpreter.GetPartitionSummary(table, true);
                }
                else
                {
                    bool isInheritedPartitionTable = await postgresInterpreter.IsInheritedPartitionTable(table);

                    if (isInheritedPartitionTable)
                    {
                        PartitionInfo partitionInfo = (await postgresInterpreter.GetPartitionInfos(table, true)).FirstOrDefault();

                        SetTableExtraInfoForPostgresTable(table, isInheritedPartitionTable, partitionInfo);
                    }
                }
            }
        }

        private static void SetTableExtraInfoForPostgresTable(Table table, bool isInheritedPartitionTable, PartitionInfo partitionInfo)
        {
            if (table.ExtraInfo == null)
            {
                table.ExtraInfo = new TableExtraInfo();
            }

            table.ExtraInfo.IsInheritedPartition = isInheritedPartitionTable;
            table.ExtraInfo.PartitionInfo = partitionInfo;
        }

        private int GetCreateIndex(string script, string createFlag)
        {
            string[] lines = script.Split('\n');

            int count = 0;

            foreach (string line in lines)
            {
                if (line.StartsWith(createFlag, StringComparison.OrdinalIgnoreCase))
                {
                    return count;
                }

                count += line.Length + 1;
            }

            return -1;
        }

        public string GenerateTableDMLScript(SchemaInfo schemaInfo, Table table, ScriptAction scriptAction)
        {
            string script = "";
            string tableName = this.dbInterpreter.GetQuotedDbObjectNameWithSchema(table);
            var columns = schemaInfo.TableColumns;

            switch (scriptAction)
            {
                case ScriptAction.SELECT:
                    string columnNames = this.dbInterpreter.GetQuotedColumnNames(columns);
                    script = $"SELECT {columnNames}{Environment.NewLine}FROM {tableName};";
                    break;
                case ScriptAction.INSERT:
                    var insertColumns = columns.Where(item => !item.IsIdentity && !item.IsComputed);
                    string insertColumnNames = string.Join(",", insertColumns.Select(item => this.dbInterpreter.GetQuotedString(item.Name)));
                    string insertValues = string.Join(",", insertColumns.Select(item => "?"));

                    script = $"INSERT INTO {tableName}({insertColumnNames}){Environment.NewLine}VALUES({insertValues});";
                    break;
                case ScriptAction.UPDATE:
                    var updateColumns = columns.Where(item => !item.IsIdentity && !item.IsComputed);
                    string setNameValues = string.Join(",", updateColumns.Select(item => $"{this.dbInterpreter.GetQuotedString(item.Name)}=?"));

                    script = $"UPDATE {tableName}{Environment.NewLine}SET {setNameValues}{Environment.NewLine}WHERE <condition>;";
                    break;
                case ScriptAction.DELETE:
                    script = $"DELETE FROM {tableName}{Environment.NewLine}WHERE <condition>;";
                    break;
            }

            return script;
        }

        public async Task<string> GenerateTableProcedureScript(SchemaInfo schemaInfo, Table table, ScriptAction scriptAction)
        {
            DatabaseType databaseType = this.dbInterpreter.DatabaseType;
            string script = "";
            string scriptBody = "";
            string tableName = this.dbInterpreter.GetQuotedDbObjectNameWithSchema(table);
            string procedureName = null;
            var columns = schemaInfo.TableColumns;
            TablePrimaryKey primaryKey = null;

            List<string> parameters = new List<string>();

            Func<string, string> getParameterName = (columnName) =>
            {
                if (databaseType == DatabaseType.SqlServer)
                {
                    return "@" + columnName;
                }

                return $"V_{columnName}";
            };

            Func<TableColumn, string> getParameterDataType = (column) =>
            {
                string dataType = this.dbInterpreter.ParseDataType(column);

                if (databaseType == DatabaseType.Oracle || databaseType == DatabaseType.Postgres)
                {
                    int parenthesesIndex = dataType.IndexOf("(");

                    if (parenthesesIndex > 0)
                    {
                        dataType = dataType.Substring(0, parenthesesIndex);
                    }
                }

                return dataType;
            };

            Func<Task<TablePrimaryKey>> getPrimaryKey = async () =>
            {
                return (await this.dbInterpreter.GetTablePrimaryKeysAsync(new SchemaInfoFilter() { TableNames = [table.Name], Schema = table.Schema })).FirstOrDefault();
            };

            switch (scriptAction)
            {
                case ScriptAction.CREATE_PROCEDURE_INSERT:

                    procedureName = this.dbInterpreter.GetQuotedString($"Insert{table.Name}");

                    var insertColumns = columns.Where(item => !item.IsIdentity && !item.IsComputed);

                    parameters.AddRange(insertColumns.Select(item => $"{getParameterName(item.Name)} {getParameterDataType(item)}"));

                    string insertColumnNames = string.Join(",", insertColumns.Select(item => this.dbInterpreter.GetQuotedString(item.Name)));
                    string insertValues = string.Join(",", insertColumns.Select(item => getParameterName(item.Name)));

                    scriptBody = $"\tINSERT INTO {tableName}({insertColumnNames}){Environment.NewLine}\tVALUES({insertValues});";
                    break;
                case ScriptAction.CREATE_PROCEDURE_UPDATE:

                    procedureName = this.dbInterpreter.GetQuotedString($"Update{table.Name}");

                    primaryKey = await getPrimaryKey();

                    if (primaryKey != null)
                    {
                        var parameterColumns = columns.Where(item => primaryKey.Columns.Any(t => t.ColumnName == item.Name))
                            .Concat(columns.Where(item => !item.IsComputed && !primaryKey.Columns.Any(t => t.ColumnName == item.Name)));

                        var updateColumns = columns.Where(item => !item.IsIdentity && !item.IsComputed && !primaryKey.Columns.Any(t => t.ColumnName == item.Name));

                        parameters.AddRange(parameterColumns.Select(item => $"{getParameterName(item.Name)} {getParameterDataType(item)}"));

                        string setNameValues = string.Join(",", updateColumns.Select(item => $"{this.dbInterpreter.GetQuotedString(item.Name)}={getParameterName(item.Name)}"));

                        var keyColumns = primaryKey.Columns;

                        string condition = string.Join(" AND ", keyColumns.Select(item => $"{this.dbInterpreter.GetQuotedString(item.ColumnName)}={getParameterName(item.ColumnName)}"));

                        scriptBody = $"\tUPDATE {tableName}{Environment.NewLine}\tSET {setNameValues}{Environment.NewLine}\tWHERE {condition};";
                    }

                    break;
                case ScriptAction.CREATE_PROCEDURE_DELETE:
                    procedureName = this.dbInterpreter.GetQuotedString($"Delete{table.Name}");

                    primaryKey = await getPrimaryKey();

                    if (primaryKey != null)
                    {
                        var keyColumns = primaryKey.Columns;

                        var parameterColumns = columns.Where(item => primaryKey.Columns.Any(t => t.ColumnName == item.Name));

                        parameters.AddRange(parameterColumns.Select(item => $"{getParameterName(item.Name)} {getParameterDataType(item)}"));

                        string condition = string.Join(" AND ", keyColumns.Select(item => $"{this.dbInterpreter.GetQuotedString(item.ColumnName)}={getParameterName(item.ColumnName)}"));

                        scriptBody = $"\tDELETE FROM {tableName}{Environment.NewLine}\tWHERE {condition};";
                    }

                    break;
            }

            string strParameters = string.Join(Environment.NewLine, parameters.Select(item => $"{item},")).TrimEnd(',');

            if (table.Schema != null && table.Schema != this.dbInterpreter.DefaultSchema)
            {
                procedureName = $"{this.dbInterpreter.GetQuotedString(table.Schema)}.{this.dbInterpreter.GetQuotedString(procedureName)}";
            }

            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    script =
 $@"CREATE PROCEDURE {procedureName}
(
{strParameters}
)
AS
BEGIN
{scriptBody}
END";
                    break;
                case DatabaseType.MySql:
                    script =
  $@"CREATE PROCEDURE {procedureName}
(
{strParameters}
)
BEGIN
{scriptBody}
END;";
                    break;
                case DatabaseType.Oracle:
                    script =
$@"CREATE OR REPLACE PROCEDURE {procedureName}
(
{strParameters}
)
AS
BEGIN
{scriptBody}
END {procedureName};";
                    break;
                case DatabaseType.Postgres:
                    script =
$@"CREATE OR REPLACE PROCEDURE {procedureName}
(
{strParameters}
)
LANGUAGE 'plpgsql'
AS $$
BEGIN
{scriptBody}
END
$$;";
                    break;
            }

            return script;
        }

        public string GenerateViewDMLScript(SchemaInfo schemaInfo, View view, ScriptAction scriptAction)
        {
            string script = "";
            string viewName = this.dbInterpreter.GetQuotedDbObjectNameWithSchema(view);
            var columns = schemaInfo.TableColumns;

            switch (scriptAction)
            {
                case ScriptAction.SELECT:
                    string columnNames = this.dbInterpreter.GetQuotedColumnNames(columns);
                    script = $"SELECT {columnNames}{Environment.NewLine}FROM {viewName};";
                    break;
            }

            return script;
        }

        public async Task<ScriptGenerateResult> GenerateRoutineCallScript(DatabaseObject dbObject, SchemaInfoFilter filter)
        {
            ScriptGenerateResult result = new ScriptGenerateResult();

            string routineName = this.dbInterpreter.GetQuotedDbObjectNameWithSchema(dbObject);
            List<RoutineParameter> parameters = null;
            bool isFunction = dbObject is Function;
            bool isProcedure = dbObject is Procedure;
            DatabaseType databaseType = this.dbInterpreter.DatabaseType;

            StringBuilder sb = new StringBuilder();

            string action = "";
            bool isTableFunction = false;
            bool isSqlServerProcedure = false;

            if (isFunction)
            {
                action = "SELECT";

                isTableFunction = (dbObject as Function).DataType?.ToUpper() == "TABLE";
            }
            else
            {
                if (databaseType == DatabaseType.MySql || databaseType == DatabaseType.Postgres)
                {
                    action = "CALL";
                }
                else if (databaseType == DatabaseType.SqlServer)
                {
                    action = "EXEC";

                    isSqlServerProcedure = true;
                }
            }

            if (isTableFunction)
            {
                sb.Append("* FROM ");
            }

            if (isFunction)
            {
                parameters = await this.dbInterpreter.GetFunctionParametersAsync(filter);
            }
            else if (isProcedure)
            {
                parameters = await this.dbInterpreter.GetProcedureParametersAsync(filter);
            }

            result.Parameters = parameters;

            sb.Append($"{action}{(string.IsNullOrEmpty(action) ? "" : " ")}{routineName}");

            if (!isSqlServerProcedure)
            {
                sb.Append("(");
            }

            sb.AppendLine();

            if (parameters != null && parameters.Count > 0)
            {
                sb.AppendLine(String.Join(" ," + Environment.NewLine, parameters.Select(item => this.GetRoutineParameterItem(item, isFunction))));
            }

            if (!isSqlServerProcedure)
            {
                sb.AppendLine(")");
            }

            if (isFunction && !isTableFunction && databaseType == DatabaseType.Oracle)
            {
                sb.Append("FROM DUAL");
            }

            result.Script = sb.ToString();

            return result;
        }

        public string GetRoutineParameterItem(RoutineParameter parameter, bool isFunction)
        {
            string strInOut = isFunction ? "" : (parameter.IsOutput ? "OUT " : "IN ");

            return $"<{strInOut}{parameter.Name} {parameter.DataType}>";
        }
    }
}
