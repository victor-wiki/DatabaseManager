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
            List<string> indexNames = new List<string>();

            foreach (TableIndex index in schemaInfo.TableIndexes)
            {
                var indexName = index.Name;

                if (indexNames.Contains(indexName))
                {
                    string columnNames = string.Join("_", index.Columns.Select(item => item.ColumnName));

                    index.Name = $"IX_{index.TableName}_{columnNames}";
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

                foreach (T obj in sourceObjs)
                {
                    bool existed = false;

                    if (obj is TableForeignKey)
                    {
                        TableForeignKey tfk = obj as TableForeignKey;

                        existed = (targetObjs as List<TableForeignKey>).Any(item => item.TableName.ToLower() == tfk.TableName && item.ReferencedTableName.ToLower() == tfk.ReferencedTableName.ToLower()
                              && IsForeignKeyColumnsEquals(item.Columns, tfk.Columns));
                    }
                    else if (obj is TableColumnChild)
                    {
                        TableColumnChild tk = obj as TableColumnChild;

                        existed = (targetObjs.Cast<TableColumnChild>()).Any(item => item.TableName.ToLower() == tk.TableName && item.ColumnName.ToLower() == tk.ColumnName.ToLower());
                    }
                    else if (obj is TableChild)
                    {
                        TableChild tc = obj as TableChild;

                        existed = (targetObjs.Cast<TableChild>()).Any(item => item.TableName.ToLower() == tc.TableName && item.Name.ToLower() == tc.Name.ToLower());
                    }
                    else
                    {
                        existed = targetObjs.Any(item => item.Name.ToLower() == obj.Name.ToLower());
                    }

                    if (existed)
                    {
                        excludeObjs.Add(obj);
                    }
                }

                sourceObjs.RemoveAll(item => excludeObjs.Any(t => t == item));
            }
        }

        public static bool IsForeignKeyColumnsEquals(List<ForeignKeyColumn> columns1, List<ForeignKeyColumn> columns2)
        {
            if (columns1.Count != columns2.Count)
            {
                return false;
            }
            else
            {
                int count = 0;

                foreach (ForeignKeyColumn column in columns1)
                {
                    if (columns2.Any(item => item.ReferencedColumnName.ToLower() == column.ReferencedColumnName.ToLower() &&
                      item.ColumnName.ToLower() == column.ColumnName.ToLower()
                    ))
                    {
                        count++;
                    }
                }

                return count == columns1.Count;
            }
        }

        public static bool IsPrimaryKeyEquals(TablePrimaryKey primaryKey1, TablePrimaryKey primaryKey2, bool onlyComapreColums = false, bool excludeComment = true)
        {
            if (primaryKey1 == null && primaryKey2 == null)
            {
                return true;
            }
            else if ((primaryKey1 != null && primaryKey2 == null) || (primaryKey1 == null && primaryKey2 != null))
            {
                return false;
            }
            else
            {
                if (IsIndexColumnsEquals(primaryKey1.Columns, primaryKey2.Columns))
                {
                    if (onlyComapreColums)
                    {
                        return true;
                    }
                    else
                    {
                        if (primaryKey1.Name == primaryKey2.Name && primaryKey1.Clustered == primaryKey2.Clustered
                            && (excludeComment || (!excludeComment && ValueHelper.IsStringEquals(primaryKey1.Comment, primaryKey2.Comment)))
                           )
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool IsIndexColumnsEquals(List<IndexColumn> columns1, List<IndexColumn> columns2)
        {
            if (columns1.Count != columns2.Count)
            {
                return false;
            }
            else
            {
                int count = 0;

                foreach (IndexColumn column in columns1)
                {
                    if (columns2.Any(item => item.ColumnName.ToLower() == column.ColumnName.ToLower() && item.IsDesc == column.IsDesc))
                    {
                        count++;
                    }
                }

                return count == columns1.Count;
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
                filter.TableTriggerNames = schemaInfo.TableTriggers.Select(item => item.Name).ToArray();
            }
        }

        public static List<TablePrimaryKey> GetTablePrimaryKeys(List<TablePrimaryKeyItem> primaryKeyItems)
        {
            List<TablePrimaryKey> primaryKeys = new List<TablePrimaryKey>();

            var groups = primaryKeyItems.GroupBy(item => new { item.Owner, item.TableName, item.Name, item.Clustered, item.Comment });

            foreach (var group in groups)
            {
                TablePrimaryKey primaryKey = new TablePrimaryKey()
                {
                    Owner = group.Key.Owner,
                    TableName = group.Key.TableName,
                    Name = group.Key.Name,
                    Clustered = group.Key.Clustered,
                    Comment = group.Key.Comment
                };

                primaryKey.Columns.AddRange(group.Select(item => new IndexColumn() { ColumnName = item.ColumnName, IsDesc = item.IsDesc }));

                primaryKeys.Add(primaryKey);
            }

            return primaryKeys;
        }

        public static List<TableIndex> GetTableIndexes(List<TableIndexItem> indexItems)
        {
            List<TableIndex> indexes = new List<TableIndex>();

            var groups = indexItems.GroupBy(item => new { item.Owner, item.TableName, item.Name, item.IsPrimary, item.IsUnique, item.Clustered, item.Type, item.Comment });

            foreach (var group in groups)
            {
                TableIndex index = new TableIndex()
                {
                    Owner = group.Key.Owner,
                    TableName = group.Key.TableName,
                    Name = group.Key.Name,
                    IsPrimary = group.Key.IsPrimary,
                    IsUnique = group.Key.IsUnique,
                    Clustered = group.Key.Clustered,
                    Type = group.Key.Type,
                    Comment = group.Key.Comment
                };

                index.Columns.AddRange(group.Select(item => new IndexColumn() { ColumnName = item.ColumnName, IsDesc = item.IsDesc, Order = item.Order }));

                indexes.Add(index);
            }

            return indexes;
        }

        public static List<TableForeignKey> GetTableForeignKeys(List<TableForeignKeyItem> foreignKeyItems)
        {
            List<TableForeignKey> foreignKeys = new List<TableForeignKey>();

            var groups = foreignKeyItems.GroupBy(item => new { item.Owner, item.TableName, item.Name, item.ReferencedTableName, item.UpdateCascade, item.DeleteCascade, item.Comment });

            foreach (var group in groups)
            {
                TableForeignKey foreignKey = new TableForeignKey()
                {
                    Owner = group.Key.Owner,
                    TableName = group.Key.TableName,
                    Name = group.Key.Name,
                    ReferencedTableName = group.Key.ReferencedTableName,
                    UpdateCascade = group.Key.UpdateCascade,
                    DeleteCascade = group.Key.DeleteCascade,
                    Comment = group.Key.Comment
                };

                foreignKey.Columns.AddRange(group.Select(item => new ForeignKeyColumn() { ColumnName = item.ColumnName, ReferencedColumnName = item.ReferencedColumnName, Order = item.Order }));

                foreignKeys.Add(foreignKey);
            }

            return foreignKeys;
        }

        public static bool IsTableColumnEquals(DatabaseType databaseType, TableColumn column1, TableColumn column2, bool excludeComment = true)
        {
            if (IsTableColumnDataTypeAndLengthEquals(databaseType, column1, column2)
               && column1.IsNullable == column2.IsNullable
               && column1.IsIdentity == column2.IsIdentity
               && ValueHelper.IsStringEquals(ValueHelper.GetTrimedDefaultValue(column1.DefaultValue), ValueHelper.GetTrimedDefaultValue(column2.DefaultValue))
               && (excludeComment || (!excludeComment && ValueHelper.IsStringEquals(column1.Comment, column2.Comment)))
               && ValueHelper.IsStringEquals(column1.ComputeExp, column2.ComputeExp))
            {
                return true;
            }

            return false;
        }

        private static bool IsTableColumnDataTypeAndLengthEquals(DatabaseType databaseType, TableColumn column1, TableColumn column2)
        {
            DataTypeInfo dataTypeInfo1 = DataTypeHelper.GetDataTypeInfo(column1.DataType);
            DataTypeInfo dataTypeInfo2 = DataTypeHelper.GetDataTypeInfo(column2.DataType);

            string dataType1 = dataTypeInfo1.DataType;
            string dataType2 = dataTypeInfo2.DataType;

            if (dataType1.ToLower() != dataType2.ToLower())
            {
                return false;
            }

            DataTypeSpecification dataTypeSpec1 = DataTypeManager.GetDataTypeSpecification(databaseType, dataType1);
            DataTypeSpecification dataTypeSpec2 = DataTypeManager.GetDataTypeSpecification(databaseType, dataType2);

            if (dataTypeInfo1.DataType == dataTypeInfo2.DataType && string.IsNullOrEmpty(dataTypeSpec1.Args) && string.IsNullOrEmpty(dataTypeSpec2.Args))
            {
                return true;
            }

            bool isChar1 = DataTypeHelper.IsCharType(dataType1);
            bool isChar2 = DataTypeHelper.IsCharType(dataType2);

            bool isBytes1 = DataTypeHelper.IsCharType(dataType1);
            bool isBytes2 = DataTypeHelper.IsCharType(dataType2);

            if (isBytes1 && isBytes2)
            {
                return column1.MaxLength == column2.MaxLength;
            }
            else if (isChar1 && isChar2 && DataTypeHelper.StartWithN(dataType1) && DataTypeHelper.StartWithN(dataType2))
            {
                return column1.MaxLength == column2.MaxLength;
            }
            else if ((column1.Precision == null && column1.Scale == null && column1.MaxLength == column2.Precision)
              || (column2.Precision == null && column2.Scale == null && column2.MaxLength == column1.Precision))
            {
                return true;
            }

            return column1.MaxLength == column2.MaxLength
               && IsPrecisionScaleEquals(column1.Precision, column2.Precision)
               && IsPrecisionScaleEquals(column1.Scale, column2.Scale);
        }

        private static bool IsPrecisionScaleEquals(int? value1, int? value2)
        {
            if ((!value1.HasValue || value1 <= 0) && (!value2.HasValue || value2 <= 0))
            {
                return true;
            }

            return value1 == value2;
        }
    }
}
