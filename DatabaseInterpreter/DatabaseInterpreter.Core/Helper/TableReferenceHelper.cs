using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class TableReferenceHelper
    {
        public static async Task<List<Table>> ResortTables(IEnumerable<Table> tables, List<TableForeignKey> tableForeignKeys, DbInterpreter dbInterpreter = null)
        {
            List<TableForeignKey> clonedTableForeignKeys = null;

            if (dbInterpreter != null)
            {
                DatabaseType databaseType = dbInterpreter.DatabaseType;

                #region Handle partition tables for Postgres 
                if (databaseType == DatabaseType.Postgres)
                {
                    clonedTableForeignKeys = tableForeignKeys.Select(item => ObjectHelper.CloneObject<TableForeignKey>(item)).ToList();

                    SchemaInfoFilter filter = null;

                    var partitionInfos = await (dbInterpreter as PostgresInterpreter).GetPartitionInfos(filter, true);

                    //simulate table foreign key by table reference
                    foreach (var patitionInfo in partitionInfos)
                    {
                        string tableSchema = patitionInfo.TableSchema;
                        string tableName = patitionInfo.TableName;
                        string parentTableSchema = patitionInfo.ParentTableSchema;
                        string parentTableName = patitionInfo.ParentTableName;

                        if (!clonedTableForeignKeys.Any(item => item.Schema == tableSchema && item.TableName == tableName && item.ReferencedSchema == parentTableSchema && item.ReferencedTableName == parentTableName))
                        {
                            TableForeignKey tableForeignKey = new TableForeignKey()
                            {
                                Name = Guid.NewGuid().ToString(),
                                Schema = tableSchema,
                                TableName = tableName,
                                ReferencedSchema = parentTableSchema,
                                ReferencedTableName = parentTableName
                            };

                            clonedTableForeignKeys.Add(tableForeignKey);
                        }
                    }
                } 
                #endregion
            }

            if (clonedTableForeignKeys == null)
            {
                clonedTableForeignKeys = tableForeignKeys;
            }

            List<Table> sortedTables = new List<Table>();

            var topReferencedTableInfos = GetTopReferencedTableInfos(clonedTableForeignKeys);

            var topReferencedTables = tables.Where(item => topReferencedTableInfos.Any(t => t.Schema == item.Schema && t.TableName == item.Name));

            sortedTables.AddRange(topReferencedTables.OrderBy(item => item.Name));

            var sortedTableInfos = sortedTables.Select(item => (item.Schema, item.Name));

            List<(string, string)> childTableInfos = new List<(string, string)>();

            foreach (var info in topReferencedTableInfos)
            {
                childTableInfos.AddRange(GetForeignTables(info.Schema, info.TableName, clonedTableForeignKeys, sortedTableInfos.Concat(childTableInfos)));
            }

            List<(string Schema, string TableName)> sortedChildTableInfos = GetSortedTableInfos(childTableInfos, clonedTableForeignKeys);

            sortedChildTableInfos = sortedChildTableInfos.Distinct().ToList();

            foreach (var sortedChildTableInfo in sortedChildTableInfos)
            {
                Table table = tables.FirstOrDefault(item => item.Schema == sortedChildTableInfo.Schema && item.Name == sortedChildTableInfo.TableName);

                if (table != null)
                {
                    sortedTables.Add(table);
                }
            }

            IEnumerable<(string Schema, string TableName)> selfReferencedTableInfos = clonedTableForeignKeys.Where(item => item.TableName == item.ReferencedTableName && item.Schema == item.ReferencedSchema)
                                                                                      .Select(item => (item.Schema, item.TableName));

            var selfReferencedTables = tables.Where(item => selfReferencedTableInfos.Any(t => t.Schema == item.Schema && t.TableName == item.Name) && !sortedTables.Contains(item)).OrderBy(item => item.Name);

            sortedTables.AddRange(selfReferencedTables);

            var hasNoRelationToForeignKeyTables = tables.Where(item => !clonedTableForeignKeys.Any(fk =>
                                                   (fk.ReferencedTableName == item.Name && fk.ReferencedSchema == item.Schema) ||
                                                   (fk.TableName == item.Name && fk.Schema == item.Schema))
                                                 );

            sortedTables.AddRange(hasNoRelationToForeignKeyTables.OrderBy(item => item.Name));

            int i = 1;

            foreach (var table in sortedTables)
            {
                table.Order = i;

                i++;
            }

            return sortedTables;
        }

        public static IEnumerable<(string Schema, string TableName)> GetTopReferencedTableInfos(IEnumerable<TableForeignKey> tableForeignKeys)
        {
            IEnumerable<(string, string)> foreignTableInfos = tableForeignKeys.Select(item => (item.Schema, item.TableName));

            IEnumerable<(string, string)> topReferencedTableInfos = tableForeignKeys.Where(item =>
                (!foreignTableInfos.Contains((item.ReferencedSchema, item.ReferencedTableName)))
                ||
                ((item.Schema == item.ReferencedSchema && item.TableName == item.ReferencedTableName)
                  && tableForeignKeys.Any(t => t.Name != item.Name && item.TableName == t.ReferencedTableName && item.Schema == t.ReferencedSchema)
                  && !tableForeignKeys.Any(t => t.Name != item.Name && item.ReferencedTableName == t.TableName && item.ReferencedSchema == t.Schema)
                ))
               .Select(item => (item.ReferencedSchema, item.ReferencedTableName)).Distinct();

            return topReferencedTableInfos;
        }

        private static List<(string Schema, string TableName)> GetSortedTableInfos(List<(string, string)> tableInfos, List<TableForeignKey> tableForeignKeys)
        {
            List<(string, string)> sortedTableInfos = new List<(string, string)>();

            for (int i = 0; i <= tableInfos.Count - 1; i++)
            {
                (string Schema, string TableName) tableInfo = tableInfos[i];

                IEnumerable<TableForeignKey> foreignKeys = tableForeignKeys.Where(item => (item.TableName == tableInfo.TableName && item.Schema == tableInfo.Schema) && !(item.TableName == item.ReferencedTableName && item.Schema == item.ReferencedSchema));

                if (foreignKeys.Any())
                {
                    foreach (TableForeignKey foreignKey in foreignKeys)
                    {
                        int referencedTableIndex = tableInfos.IndexOf((foreignKey.ReferencedSchema, foreignKey.ReferencedTableName));

                        if (referencedTableIndex >= 0 && referencedTableIndex > i)
                        {
                            sortedTableInfos.Add((foreignKey.ReferencedSchema, foreignKey.ReferencedTableName));
                        }
                    }
                }

                sortedTableInfos.Add(tableInfo);
            }

            sortedTableInfos = sortedTableInfos.Distinct().ToList();

            bool needSort = false;

            for (int i = 0; i <= sortedTableInfos.Count - 1; i++)
            {
                (string Schema, string TableName) tableInfo = sortedTableInfos[i];

                IEnumerable<TableForeignKey> foreignKeys = tableForeignKeys.Where(item => item.TableName == tableInfo.TableName && item.Schema == tableInfo.Schema && !(item.TableName == item.ReferencedTableName && item.Schema == item.ReferencedSchema));

                if (foreignKeys.Any())
                {
                    foreach (TableForeignKey foreignKey in foreignKeys)
                    {
                        int referencedTableIndex = sortedTableInfos.IndexOf((foreignKey.ReferencedSchema, foreignKey.ReferencedTableName));

                        if (referencedTableIndex >= 0 && referencedTableIndex > i)
                        {
                            needSort = true;
                            break;
                        }
                    }
                }

                if (needSort)
                {
                    return GetSortedTableInfos(sortedTableInfos, tableForeignKeys);
                }
            }

            return sortedTableInfos;
        }

        private static List<(string Schema, string TableName)> GetForeignTables(string schema, string tableName, List<TableForeignKey> tableForeignKeys, IEnumerable<(string, string)> sortedTableInfos)
        {
            List<(string, string)> tableInfos = new List<(string, string)>();

            IEnumerable<(string Schema, string TableName)> foreignTableInfos = tableForeignKeys.Where(item => item.ReferencedTableName == tableName && !(item.TableName == tableName && item.Schema == schema) && !sortedTableInfos.Contains((item.Schema, item.TableName))).Select(item => (item.Schema, item.TableName));

            tableInfos.AddRange(foreignTableInfos);

            IEnumerable<string> childForeignTableNames = tableForeignKeys.Where(item => foreignTableInfos.Contains((item.ReferencedSchema, item.ReferencedTableName))).Select(item => item.TableName);

            if (childForeignTableNames.Count() > 0)
            {
                List<(string, string)> childInfos = foreignTableInfos.SelectMany(item => GetForeignTables(item.Schema, item.TableName, tableForeignKeys, sortedTableInfos)).ToList();

                tableInfos.AddRange(childInfos.Where(item => !tableInfos.Contains(item)));
            }

            return tableInfos;
        }

        public static bool IsSelfReference(string tableName, List<TableForeignKey> tableForeignKeys)
        {
            return tableForeignKeys.Any(item => item.TableName == tableName && item.TableName == item.ReferencedTableName);
        }
    }
}
