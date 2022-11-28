using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                DbScriptGenerator dbScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(dbInterpreter);

                List<Script> scripts = dbScriptGenerator.GenerateSchemaScripts(schemaInfo).Scripts;

                DatabaseType databaseType = this.dbInterpreter.DatabaseType;

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
                result.Script = this.GenerateTableDMLScript(schemaInfo, table, scriptAction);
            }
            else if (dbObject is View view)
            {
                result.Script = this.GenerateViewDMLScript(schemaInfo, view, scriptAction);
            }

            return result;
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

            if(!isSqlServerProcedure)
            {
                sb.Append("(");
            }           

            sb.AppendLine();

            if (parameters != null && parameters.Count > 0)
            {
                sb.AppendLine(String.Join(" ," + Environment.NewLine, parameters.Select(item => this.GetRoutineParameterItem(item, isFunction))));
            }

            if(!isSqlServerProcedure)
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
