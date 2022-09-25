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

            if (dbObject is Table)
            {
                dbInterpreter.Option.GetTableAllObjects = true;
            }

            DatabaseObjectType databaseObjectType = (DatabaseObjectType)Enum.Parse(typeof(DatabaseObjectType), typeName);

            SchemaInfoFilter filter = new SchemaInfoFilter() { DatabaseObjectType = databaseObjectType };
            filter.GetType().GetProperty($"{typeName}Names").SetValue(filter, new string[] { dbObject.Name });

            SchemaInfo schemaInfo = await dbInterpreter.GetSchemaInfoAsync(filter);

            DbScriptGenerator dbScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(dbInterpreter);

            List<Script> scripts = dbScriptGenerator.GenerateSchemaScripts(schemaInfo).Scripts;

            StringBuilder sbContent = new StringBuilder();

            DatabaseType databaseType = this.dbInterpreter.DatabaseType;

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
