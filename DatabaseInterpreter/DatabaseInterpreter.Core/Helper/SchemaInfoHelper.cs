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
            SchemaInfo cloneSchemaInfo =(SchemaInfo) JsonConvert.DeserializeObject(JsonConvert.SerializeObject(schemaInfo), typeof(SchemaInfo));           
            return cloneSchemaInfo;
        }
        public static void TransformOwner(SchemaInfo schemaInfo, string owner)
        {
            schemaInfo.UserDefinedTypes.ForEach(item =>
            {
                item.Owner = owner;
            });
            schemaInfo.Tables.ForEach(item =>
            {
                item.Owner = owner;
            });
            schemaInfo.TableColumns.ForEach(item =>
            {
                item.Owner = owner;
            });
            schemaInfo.TablePrimaryKeys.ForEach(item =>
            {
                item.Owner = owner;
            });
            schemaInfo.TableForeignKeys.ForEach(item =>
            {
                item.Owner = owner;
            });
            schemaInfo.TableIndexes.ForEach(item =>
            {
                item.Owner = owner;
            });
            schemaInfo.Views.ForEach(item =>
            {
                item.Owner = owner;
            });
        }

        public static void EnsurePrimaryKeyNameUnique(SchemaInfo schemaInfo)
        {
            List<string> keyNames = new List<string>();
            schemaInfo.TablePrimaryKeys.ForEach(item =>
            {
                if(keyNames.Contains(item.Name))
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
            foreach (var pair in dic)            {
               
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
                if(item.Name.Length>maxLength)
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
    }
}
