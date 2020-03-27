using DatabaseInterpreter.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseInterpreter.Core
{
    public class SchemaInfoHelper
    {
        public static SchemaInfo Clone(SchemaInfo schemaInfo)
        {
            SchemaInfo cloneSchemaInfo = (SchemaInfo)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(schemaInfo), typeof(SchemaInfo));
            return cloneSchemaInfo;
        }

        public static void EnsurePrimaryKeyNameUnique(SchemaInfo schemaInfo)
        {
            List<string> keyNames = new List<string>();
            schemaInfo.TablePrimaryKeys.ForEach(item =>
            {
                if (keyNames.Contains(item.Name))
                {
                    item.Name = "PK_" + item.TableName;
                }

                keyNames.Add(item.Name);
            });
        }

        public static void EnsureIndexNameUnique(SchemaInfo schemaInfo)
        {
            var dic = schemaInfo.TableIndexes.GroupBy(_ => new { _.Owner, _.TableName, _.Name })
                .ToDictionary(_ => _.Key, __ => __.ToList());

            List<string> indexNames = new List<string>();
            foreach (var pair in dic)
            {

                var indexName = pair.Key.Name;
                if (indexNames.Contains(pair.Key.Name))
                {
                    string columnNames = string.Join("_", schemaInfo.TableIndexes.Where(t => t.Owner == pair.Key.Owner && t.TableName == pair.Key.TableName).Select(t => t.ColumnName));

                    indexName = $"IX_{pair.Key.TableName}_{columnNames}";

                    foreach (var item in schemaInfo.TableIndexes)
                    {
                        if (item.TableName == pair.Key.TableName
                            && item.Owner == pair.Key.Owner
                            && item.Name == pair.Key.Name)
                        {
                            item.Name = indexName;
                        }
                    }
                }

                indexNames.Add(indexName);
            }
        }

        public static void RistrictNameLength(SchemaInfo schemaInfo, int maxLength)
        {
            schemaInfo.TablePrimaryKeys.ForEach(item =>
            {
                if (item.Name.Length > maxLength)
                {
                    item.Name = item.Name.Substring(0, maxLength);
                }
            });

            schemaInfo.TableForeignKeys.ForEach(item =>
            {
                if (item.Name.Length > maxLength)
                {
                    item.Name = item.Name.Substring(0, maxLength);
                }
            });

            schemaInfo.TableIndexes.ForEach(item =>
            {
                if (item.Name.Length > maxLength)
                {
                    item.Name = item.Name.Substring(0, maxLength);
                }
            });
        }

        public static void ExcludeExistingObjects(SchemaInfo source, SchemaInfo target)
        {
            ExcludeDbObjects(source.UserDefinedTypes, target.UserDefinedTypes);
            ExcludeDbObjects(source.Functions, target.Functions);
            ExcludeDbObjects(source.Tables, target.Tables);
            ExcludeDbObjects(source.TableColumns, target.TableColumns);
            ExcludeDbObjects(source.TablePrimaryKeys, target.TablePrimaryKeys);
            ExcludeDbObjects(source.TableForeignKeys, target.TableForeignKeys);
            ExcludeDbObjects(source.TableIndexes, target.TableIndexes);
            ExcludeDbObjects(source.TableConstraints, target.TableConstraints);
            ExcludeDbObjects(source.TableTriggers, target.TableTriggers);
            ExcludeDbObjects(source.Procedures, target.Procedures);
        }

        public static void ExcludeDbObjects<T>(List<T> sourceObjs, List<T> targetObjs) where T : DatabaseObject
        {
            if (sourceObjs.Count > 0 && targetObjs.Count > 0)
            {
                List<T> excludeObjs = new List<T>();

                foreach(T obj in sourceObjs)
                {
                    bool existed = false;

                    if(obj is TableForeignKey)
                    {
                        TableForeignKey tfk = obj as TableForeignKey;

                        existed = (targetObjs as List<TableForeignKey>).Any(item => item.TableName.ToLower() == tfk.TableName && item.ColumnName.ToLower() == tfk.ColumnName.ToLower()
                                   && item.ReferencedTableName.ToLower()== tfk.ReferencedTableName.ToLower() && item.ReferencedColumnName.ToLower()== tfk.ReferencedColumnName.ToLower());
                    }
                    else if (obj is TableColumnChild)
                    {
                        TableColumnChild tk = obj as TableColumnChild;

                        existed = (targetObjs.Cast<TableColumnChild>()).Any(item => item.TableName.ToLower() == tk.TableName && item.ColumnName.ToLower() == tk.ColumnName.ToLower());
                    }                   
                    else if(obj is TableChild)
                    {
                        TableChild tc = obj as TableChild;

                        existed = (targetObjs.Cast<TableChild>()).Any(item => item.TableName.ToLower() == tc.TableName && item.Name.ToLower() == tc.Name.ToLower());
                    }
                    else
                    {
                        existed = targetObjs.Any(item => item.Name.ToLower() == obj.Name.ToLower());
                    }

                    if(existed)
                    {
                        excludeObjs.Add(obj);
                    }
                }

                sourceObjs.RemoveAll(item => excludeObjs.Any(t => t == item));               
            }           
        }

        public static void SetSchemaInfoFilterValues(SchemaInfoFilter filter, SchemaInfo schemaInfo)
        {
            if (schemaInfo != null)
            {
                filter.UserDefinedTypeNames = schemaInfo.UserDefinedTypes.Select(item => item.Name).ToArray();
                filter.TableNames = schemaInfo.Tables.Select(t => t.Name).ToArray();
                filter.ViewNames = schemaInfo.Views.Select(item => item.Name).ToArray();
                filter.FunctionNames = schemaInfo.Functions.Select(item => item.Name).ToArray();
                filter.ProcedureNames = schemaInfo.Procedures.Select(item => item.Name).ToArray();
            }
        }
    }
}
