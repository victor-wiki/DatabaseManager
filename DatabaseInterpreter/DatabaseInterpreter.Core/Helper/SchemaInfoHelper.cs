using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
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

        public static void MapTableNames(SchemaInfo schemaInfo, Dictionary<string, string> tableNameMappings)
        {
            schemaInfo.Tables.ForEach(item => item.Name = GetMappedTableName(item.Name, tableNameMappings));
            schemaInfo.TableColumns.ForEach(item => item.TableName = GetMappedTableName(item.TableName, tableNameMappings));
            schemaInfo.TablePrimaryKeys.ForEach(item => item.TableName = GetMappedTableName(item.TableName, tableNameMappings));
            schemaInfo.TableIndexes.ForEach(item => item.TableName = GetMappedTableName(item.TableName, tableNameMappings));
            schemaInfo.TableConstraints.ForEach(item => item.TableName = GetMappedTableName(item.TableName, tableNameMappings));
            schemaInfo.TableTriggers.ForEach(item => item.TableName = GetMappedTableName(item.TableName, tableNameMappings));
            schemaInfo.TableForeignKeys.ForEach(item =>
                {
                    item.TableName = GetMappedTableName(item.TableName, tableNameMappings);
                    item.ReferencedTableName = GetMappedTableName(item.ReferencedTableName, tableNameMappings);
                }
            );
        }

        public static void RenameTableChildren(SchemaInfo schemaInfo)
        {
            schemaInfo.TablePrimaryKeys.ForEach(item => item.Name = Rename(item.Name));
            schemaInfo.TableIndexes.ForEach(item => item.Name = Rename(item.Name));
            schemaInfo.TableConstraints.ForEach(item => item.Name = Rename(item.Name));
            schemaInfo.TableForeignKeys.ForEach(item => item.Name = Rename(item.Name));
        }

        public static string Rename(string name)
        {
            return name + "1";
        }

        public static string GetMappedTableName(string tableName, Dictionary<string, string> tableNameMappings)
        {
            if (tableNameMappings != null && tableNameMappings.ContainsKey(tableName))
            {
                return tableNameMappings[tableName];
            }

            return tableName;
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

        public static void ForceRenameMySqlPrimaryKey(SchemaInfo schemaInfo)
        {
            List<string> keyNames = new List<string>();

            schemaInfo.TablePrimaryKeys.ForEach(item =>
            {
                if (item.Name == "PRIMARY")
                {
                    item.Name = "PK_" + item.TableName;
                }
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
            ExcludeDbObjects(source.Sequences, target.Sequences);
        }

        public static void ExcludeDbObjects<T>(List<T> sourceDbObjects, List<T> targetDbObjects) where T : DatabaseObject
        {
            if (sourceDbObjects.Count > 0 && targetDbObjects.Count > 0)
            {
                List<T> excludeDbObjects = new List<T>();

                foreach (T obj in sourceDbObjects)
                {
                    bool existed = false;

                    if (obj is TableForeignKey)
                    {
                        TableForeignKey tfk = obj as TableForeignKey;

                        existed = (targetDbObjects as List<TableForeignKey>).Any(item => item.TableName.ToLower() == tfk.TableName && item.ReferencedTableName.ToLower() == tfk.ReferencedTableName.ToLower()
                              && IsForeignKeyColumnsEquals(item.Columns, tfk.Columns));
                    }
                    else if (obj is TableColumnChild)
                    {
                        TableColumnChild tk = obj as TableColumnChild;

                        existed = (targetDbObjects.Cast<TableColumnChild>()).Any(item => item.TableName.ToLower() == tk.TableName && item.ColumnName.ToLower() == tk.ColumnName.ToLower());
                    }
                    else if (obj is TableChild)
                    {
                        TableChild tc = obj as TableChild;

                        existed = (targetDbObjects.Cast<TableChild>()).Any(item => item.TableName.ToLower() == tc.TableName && item.Name?.ToLower() == tc.Name?.ToLower());
                    }
                    else
                    {
                        existed = targetDbObjects.Any(item => item.Name.ToLower() == obj.Name.ToLower());
                    }

                    if (existed)
                    {
                        excludeDbObjects.Add(obj);
                    }
                }

                sourceDbObjects.RemoveAll(item => excludeDbObjects.Any(t => t == item));
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
                filter.SequenceNames = schemaInfo.Sequences.Select(item => item.Name).ToArray();
            }
        }

        public static List<TablePrimaryKey> GetTablePrimaryKeys(List<TablePrimaryKeyItem> primaryKeyItems)
        {
            List<TablePrimaryKey> primaryKeys = new List<TablePrimaryKey>();

            var groups = primaryKeyItems.GroupBy(item => new { item.Schema, item.TableName, item.Name, item.Clustered, item.Comment });

            foreach (var group in groups)
            {
                TablePrimaryKey primaryKey = new TablePrimaryKey()
                {
                    Schema = group.Key.Schema,
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

            var groups = indexItems.GroupBy(item => new { item.Schema, item.TableName, item.Name, item.IsPrimary, item.IsUnique, item.Clustered, item.Type, item.Comment });

            foreach (var group in groups)
            {
                TableIndex index = new TableIndex()
                {
                    Schema = group.Key.Schema,
                    TableName = group.Key.TableName,
                    Name = group.Key.Name,
                    IsPrimary = group.Key.IsPrimary,
                    IsUnique = group.Key.IsUnique,
                    Clustered = group.Key.Clustered,
                    Type = group.Key.Type,
                    Comment = group.Key.Comment
                };

                index.Columns.AddRange(group.Select(item => new IndexColumn() { ColumnName = item.ColumnName, IsDesc = item.IsDesc, Order = item.Order }).Where(item => item.ColumnName != null));

                indexes.Add(index);
            }

            return indexes;
        }

        public static List<UserDefinedType> GetUserDefinedTypes(List<UserDefinedTypeAttribute> userDefinedTypeAttrs)
        {
            List<UserDefinedType> userDefinedTypes = new List<UserDefinedType>();

            var groups = userDefinedTypeAttrs.GroupBy(item => new { item.Schema, item.TypeName });

            foreach (var group in groups)
            {
                UserDefinedType userDefinedType = new UserDefinedType()
                {
                    Schema = group.Key.Schema,
                    Name = group.Key.TypeName
                };

                userDefinedType.Attributes.AddRange(group.Select(item => new UserDefinedTypeAttribute()
                {
                    TypeName = item.TypeName,
                    Name = item.Name,
                    DataType = item.DataType,
                    MaxLength = item.MaxLength,
                    Precision = item.Precision,
                    Scale = item.Scale,
                    IsNullable = item.IsNullable
                }));

                userDefinedTypes.Add(userDefinedType);
            }

            return userDefinedTypes;
        }

        public static List<TableForeignKey> GetTableForeignKeys(List<TableForeignKeyItem> foreignKeyItems)
        {
            List<TableForeignKey> foreignKeys = new List<TableForeignKey>();

            var groups = foreignKeyItems.GroupBy(item => new { item.Schema, item.TableName, item.Name, item.ReferencedSchema, item.ReferencedTableName, item.UpdateCascade, item.DeleteCascade, item.Comment });

            foreach (var group in groups)
            {
                TableForeignKey foreignKey = new TableForeignKey()
                {
                    Schema = group.Key.Schema,
                    TableName = group.Key.TableName,
                    Name = group.Key.Name,
                    ReferencedSchema = group.Key.ReferencedSchema,
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
               && ValueHelper.IsStringEquals(StringHelper.GetBalanceParenthesisTrimedValue(column1.DefaultValue), StringHelper.GetBalanceParenthesisTrimedValue(column2.DefaultValue))
               && (excludeComment || (!excludeComment && ValueHelper.IsStringEquals(column1.Comment, column2.Comment)))
               && ValueHelper.IsStringEquals(column1.ComputeExp, column2.ComputeExp))
            {
                return true;
            }

            return false;
        }

        private static bool IsTableColumnDataTypeAndLengthEquals(DatabaseType databaseType, TableColumn column1, TableColumn column2)
        {
            if (DataTypeHelper.IsUserDefinedType(column1) != DataTypeHelper.IsUserDefinedType(column2))
            {
                return false;
            }
            else if (DataTypeHelper.IsUserDefinedType(column1) && DataTypeHelper.IsUserDefinedType(column2))
            {
                return column1.DataType == column2.DataType;
            }

            DataTypeInfo dataTypeInfo1 = DataTypeHelper.GetDataTypeInfo(column1.DataType);
            DataTypeInfo dataTypeInfo2 = DataTypeHelper.GetDataTypeInfo(column2.DataType);

            var dataTypeSpecs1 = DataTypeManager.GetDataTypeSpecifications(databaseType);
            var dataTypeSpecs2 = DataTypeManager.GetDataTypeSpecifications(databaseType);

            string dataType1 = dataTypeInfo1.DataType;
            string dataType2 = dataTypeInfo2.DataType;

            if (!dataTypeSpecs1.Any(item => item.Name == dataType1))
            {
                dataTypeInfo1 = DataTypeHelper.GetDataTypeInfoByRegex(column1.DataType.ToLower());
                dataType1 = dataTypeInfo1.DataType;
            }

            if (!dataTypeSpecs2.Any(item => item.Name == dataType2))
            {
                dataTypeInfo2 = DataTypeHelper.GetDataTypeInfoByRegex(column2.DataType.ToLower());
                dataType2 = dataTypeInfo2.DataType;
            }

            if (dataType1.ToLower() != dataType2.ToLower())
            {
                return false;
            }

            DataTypeSpecification dataTypeSpec1 = dataTypeSpecs1.FirstOrDefault(item => item.Name == dataType1);
            DataTypeSpecification dataTypeSpec2 = dataTypeSpecs2.FirstOrDefault(item => item.Name == dataType2);

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
            else if (isChar1 && isChar2 && DataTypeHelper.StartsWithN(dataType1) && DataTypeHelper.StartsWithN(dataType2))
            {
                return column1.MaxLength == column2.MaxLength;
            }
            else if ((column1.Precision == null && column1.Scale == null && column1.MaxLength == column2.Precision)
              || (column2.Precision == null && column2.Scale == null && column2.MaxLength == column1.Precision))
            {
                return true;
            }

            if (dataTypeSpec1.Name == dataTypeSpec2.Name && dataTypeSpec1.Args?.Contains("length") == false)
            {
                if (dataTypeSpec1.Args == "scale")
                {
                    return IsPrecisionScaleEquals(column1.Scale, column2.Scale);
                }
                else if (dataTypeSpec1.Args == "precision")
                {
                    return IsPrecisionScaleEquals(column1.Precision, column2.Precision);
                }
                else if (dataTypeSpec1.Args?.Contains("scale") == true || dataTypeSpec1.Args?.Contains("precision") == true)
                {
                    return IsPrecisionScaleEquals(column1.Precision, column2.Precision)
                           && IsPrecisionScaleEquals(column1.Scale, column2.Scale);
                }
            }

            return column1.MaxLength == column2.MaxLength
               && IsPrecisionScaleEquals(column1.Precision, column2.Precision)
               && IsPrecisionScaleEquals(column1.Scale, column2.Scale);
        }

        private static bool IsPrecisionScaleEquals(long? value1, long? value2)
        {
            if ((!value1.HasValue || value1 <= 0) && (!value2.HasValue || value2 <= 0))
            {
                return true;
            }

            return value1 == value2;
        }

        public static void MapDatabaseObjectSchema(SchemaInfo schemaInfo, List<SchemaMappingInfo> mappings)
        {
            foreach (var mapping in mappings)
            {
                bool isAllSourceSchema = string.IsNullOrEmpty(mapping.SourceSchema);
                string targetSchema = mapping.TargetSchema;

                var tables = schemaInfo.Tables.Where(item => item.Schema == mapping.SourceSchema || isAllSourceSchema).ToList();
                tables.ForEach(item => item.Schema = targetSchema);

                var views = schemaInfo.Views.Where(item => item.Schema == mapping.SourceSchema || isAllSourceSchema).ToList();
                views.ForEach(item => item.Schema = targetSchema);

                var columns = schemaInfo.TableColumns.Where(item => item.Schema == mapping.SourceSchema || isAllSourceSchema).ToList();
                columns.ForEach(item => item.Schema = targetSchema);

                var primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.Schema == mapping.SourceSchema || isAllSourceSchema).ToList();
                primaryKeys.ForEach(item => item.Schema = targetSchema);

                var foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.Schema == mapping.SourceSchema || isAllSourceSchema).ToList();
                foreignKeys.ForEach(item =>
                {
                    if (item.Schema == item.ReferencedSchema)
                    {
                        item.ReferencedSchema = targetSchema;
                    }
                    else
                    {
                        item.ReferencedSchema = mappings.FirstOrDefault(t => t.SourceSchema == item.ReferencedSchema)?.TargetSchema;

                        if (item.ReferencedSchema == null)
                        {
                            item.ReferencedSchema = targetSchema;
                        }
                    }
                    item.Schema = targetSchema;
                });

                var indexes = schemaInfo.TableIndexes.Where(item => item.Schema == mapping.SourceSchema || isAllSourceSchema).ToList();
                indexes.ForEach(item => item.Schema = targetSchema);

                var constraints = schemaInfo.TableConstraints.Where(item => item.Schema == mapping.SourceSchema || isAllSourceSchema).ToList();
                constraints.ForEach(item => item.Schema = targetSchema);

                var triggers = schemaInfo.TableTriggers.Where(item => item.Schema == mapping.SourceSchema || isAllSourceSchema).ToList();
                triggers.ForEach(item => item.Schema = targetSchema);

                var userDefinedTypes = schemaInfo.UserDefinedTypes.Where(item => item.Schema == mapping.SourceSchema || isAllSourceSchema).ToList();
                userDefinedTypes.ForEach(item => item.Schema = targetSchema);

                var functions = schemaInfo.Functions.Where(item => item.Schema == mapping.SourceSchema || isAllSourceSchema).ToList();
                functions.ForEach(item => item.Schema = targetSchema);

                var procedures = schemaInfo.Procedures.Where(item => item.Schema == mapping.SourceSchema || isAllSourceSchema).ToList();
                procedures.ForEach(item => item.Schema = targetSchema);

                var sequences = schemaInfo.Sequences.Where(item => item.Schema == mapping.SourceSchema || isAllSourceSchema).ToList();
                sequences.ForEach(item => item.Schema = targetSchema);
            }
        }

        public static string GetMappedSchema(string schema, List<SchemaMappingInfo> schemaMappings)
        {
            if (schema == null)
            {
                return null;
            }

            string mappedSchema = schemaMappings.FirstOrDefault(item => item.SourceSchema.ToUpper() == schema.ToUpper())?.TargetSchema;

            if (mappedSchema == null)
            {
                mappedSchema = schemaMappings.FirstOrDefault(item => string.IsNullOrEmpty(item.SourceSchema))?.TargetSchema;
            }

            return mappedSchema;
        }

        public static SchemaInfo GetSchemaInfoByDbObject(DatabaseObject dbObject)
        {
            SchemaInfo schemaInfo = new SchemaInfo();

            if (dbObject is Table)
            {
                schemaInfo.Tables.Add(dbObject as Table);
            }
            else if (dbObject is View)
            {
                schemaInfo.Views.Add(dbObject as DatabaseInterpreter.Model.View);
            }
            else if (dbObject is Function)
            {
                schemaInfo.Functions.Add(dbObject as Function);
            }
            else if (dbObject is Procedure)
            {
                schemaInfo.Procedures.Add(dbObject as Procedure);
            }
            else if (dbObject is TableTrigger)
            {
                schemaInfo.TableTriggers.Add(dbObject as TableTrigger);
            }
            else if (dbObject is Sequence)
            {
                schemaInfo.Sequences.Add(dbObject as Sequence);
            }
            else if (dbObject is UserDefinedType)
            {
                schemaInfo.UserDefinedTypes.Add(dbObject as UserDefinedType);
            }

            return schemaInfo;
        }

        public static bool IsSameTableColumnIgnoreCase(TableColumn column1, TableColumn column2)
        {
            return column1.TableName.ToLower() == column2.TableName.ToLower() && column1.Name.ToLower() == column2.Name.ToLower();
        }
    }
}
