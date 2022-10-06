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

        public async Task<string> Generate(DatabaseObject dbObject, ScriptAction scriptAction)
        {
            string typeName = dbObject.GetType().Name;

            DatabaseObjectType databaseObjectType = (DatabaseObjectType)Enum.Parse(typeof(DatabaseObjectType), typeName);

            if (dbObject is Table)
            {
                if (scriptAction == ScriptAction.CREATE)
                {
                    dbInterpreter.Option.GetTableAllObjects = true;
                }
                else
                {
                    databaseObjectType |= DatabaseObjectType.TableColumn;
                }
            }

            SchemaInfoFilter filter = new SchemaInfoFilter() { DatabaseObjectType = databaseObjectType };

            filter.Schema = dbObject.Schema;
            filter.GetType().GetProperty($"{typeName}Names").SetValue(filter, new string[] { dbObject.Name });           

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

                return StringHelper.ToSingleEmptyLine(sbContent.ToString());
            }
            else if (dbObject is Table table)
            {
                return this.GenerateTableDMLScript(schemaInfo, table, scriptAction);
            }
            else if (dbObject is View view)
            {
                return this.GenerateViewDMLScript(schemaInfo, view, scriptAction);
            }

            return String.Empty;
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

            switch (scriptAction)
            {
                case ScriptAction.SELECT:
                    script = $"SELECT * {Environment.NewLine}FROM {viewName}";
                    break;
            }

            return script;
        }
    }
}
