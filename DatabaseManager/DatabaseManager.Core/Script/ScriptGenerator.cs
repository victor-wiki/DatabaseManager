using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Model;
using System;
using System.Linq;
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

            if (dbObject is Table)
            {
                dbInterpreter.Option.GetTableAllObjects = true;
            }

            DatabaseObjectType databaseObjectType = (DatabaseObjectType)Enum.Parse(typeof(DatabaseObjectType), typeName);

            SchemaInfoFilter filter = new SchemaInfoFilter() { DatabaseObjectType = databaseObjectType };
            filter.GetType().GetProperty($"{typeName}Names").SetValue(filter, new string[] { dbObject.Name });

            SchemaInfo schemaInfo = await dbInterpreter.GetSchemaInfoAsync(filter);

            DbScriptGenerator dbScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(dbInterpreter);

            Script dbObjScript = dbScriptGenerator.GenerateSchemaScripts(schemaInfo).Scripts.FirstOrDefault(item => item.ObjectType == typeName);

            if (dbObjScript != null)
            {
                string script = dbObjScript.Content;

                if (scriptAction == ScriptAction.ALTER && typeName != nameof(Table))
                {
                    string objType = typeName;

                    if (typeName == nameof(TableTrigger))
                    {
                        objType = "TRIGGER";
                    }

                    string createFlag = "CREATE ";
                    int createFlagIndex = this.GetCreateIndex(script, createFlag);

                    if (createFlagIndex >= 0)
                    {
                        DatabaseType databaseType = this.dbInterpreter.DatabaseType;

                        switch (databaseType)
                        {
                            case DatabaseType.SqlServer:
                                script = script.Substring(0, createFlagIndex) + "ALTER " + script.Substring(createFlagIndex + createFlag.Length);
                                break;
                            case DatabaseType.MySql:
                                script = $"DROP {objType} IF EXISTS {this.dbInterpreter.GetQuotedString(dbObject.Name)};" + Environment.NewLine + script;
                                break;
                            case DatabaseType.Oracle:
                                if (!Regex.IsMatch(script, @"^(CREATE[\s]+OR[\s]+REPLACE[\s]+)", RegexOptions.IgnoreCase))
                                {
                                    script = script.Substring(0, createFlagIndex) + "CREATE OR REPLACE " + script.Substring(createFlagIndex + createFlag.Length);
                                }
                                break;
                        }
                    }
                }

                return script;
            }

            return string.Empty;           
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
    }
}
