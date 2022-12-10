using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseInterpreter.Core
{
    public class DbObjectHelper
    {
        public static void Resort<T>(List<T> dbObjects)
            where T : ScriptDbObject
        {
            for (int i = 0; i < dbObjects.Count - 1; i++)
            {
                for (int j = i + 1; j < dbObjects.Count - 1; j++)
                {
                    if (!string.IsNullOrEmpty(dbObjects[i].Definition))
                    {
                        Regex nameRegex = new Regex($"\\b({dbObjects[j].Name})\\b", RegexOptions.IgnoreCase);

                        if (nameRegex.IsMatch(dbObjects[i].Definition))
                        {
                            var temp = dbObjects[j];
                            dbObjects[j] = dbObjects[i];
                            dbObjects[i] = temp;
                        }
                    }
                }
            }
        }

        public static List<TableColumn> ResortTableColumns(IEnumerable<Table> tables, List<TableColumn> columns)
        {
            if (tables.Count() == 0)
            {
                return columns;
            }

            List<TableColumn> sortedColumns = new List<TableColumn>();

            foreach (Table table in tables)
            {
                sortedColumns.AddRange(columns.Where(item => item.Schema == table.Schema && item.TableName == table.Name).OrderBy(item => item.Order));
            }

            if (sortedColumns.Count < columns.Count)
            {
                sortedColumns.AddRange(columns.Where(item => !sortedColumns.Contains(item)));
            }

            return sortedColumns;
        }

        public static DatabaseObjectType GetDatabaseObjectType(DatabaseObject dbObject)
        {
            string typeName = dbObject.GetType().Name;

            if (typeName == nameof(TablePrimaryKeyItem))
            {
                return DatabaseObjectType.PrimaryKey;
            }
            else if (typeName == nameof(TableForeignKeyItem))
            {
                return DatabaseObjectType.ForeignKey;
            }

            if (typeName.StartsWith(nameof(Table)) && typeName != nameof(Table))
            {
                typeName = typeName.Replace(nameof(Table), "");
            }

            if (typeName == nameof(UserDefinedType))
            {
                return DatabaseObjectType.Type;
            }            
            else
            {
                if (Enum.TryParse(typeof(DatabaseObjectType), typeName, out _))
                {
                    DatabaseObjectType databaseObjectType = (DatabaseObjectType)Enum.Parse(typeof(DatabaseObjectType), typeName);

                    return databaseObjectType;
                }
                else
                {
                    return DatabaseObjectType.None;
                }
            }
        }
    }
}
