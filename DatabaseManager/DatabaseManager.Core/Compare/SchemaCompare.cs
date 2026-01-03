using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Core.Model;
using KellermanSoftware.CompareNetObjects;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseManager.Core
{
    public class SchemaCompare
    {
        private DatabaseType databaseType;
        private SchemaInfo sourceShemaInfo;
        private SchemaInfo targetSchemaInfo;
        private bool ignoreUnnamedTableChildDifference;

        public SchemaCompare(DatabaseType databaseType, SchemaInfo sourceShemaInfo, SchemaInfo targetSchemaInfo)
        {
            this.databaseType = databaseType;
            this.sourceShemaInfo = sourceShemaInfo;
            this.targetSchemaInfo = targetSchemaInfo;

            this.ignoreUnnamedTableChildDifference = SettingManager.Setting.IgnoreUnnamedTableChildDifference;
        }

        public List<SchemaCompareDifference> Compare()
        {
            List<SchemaCompareDifference> differences = new List<SchemaCompareDifference>();

            differences.AddRange(this.CompareDatabaseObjects<UserDefinedType>(nameof(UserDefinedType), DatabaseObjectType.Type, this.sourceShemaInfo.UserDefinedTypes, targetSchemaInfo.UserDefinedTypes));

            var sortedTargetTables = TableReferenceHelper.ResortTables(targetSchemaInfo.Tables, targetSchemaInfo.TableForeignKeys);

            #region Table
            foreach (Table target in sortedTargetTables)
            {
                SchemaCompareDifference difference = new SchemaCompareDifference() { Type = nameof(Table), DatabaseObjectType = DatabaseObjectType.Table };

                Table source = this.sourceShemaInfo.Tables.FirstOrDefault(item => this.IsNameEquals(item.Name, target.Name));

                if (source == null)
                {
                    difference.DifferenceType = SchemaCompareDifferenceType.Deleted;
                    difference.Target = target;

                    differences.Add(difference);
                }
                else
                {
                    difference.DifferenceType = SchemaCompareDifferenceType.None;
                    difference.Source = source;
                    difference.Target = target;

                    differences.Add(difference);

                    bool isTableEquals = this.IsDbObjectEquals(source, target);

                    if (isTableEquals)
                    {
                        #region Column
                        IEnumerable<TableColumn> sourceColumns = this.sourceShemaInfo.TableColumns.Where(item => item.Schema == source.Schema && item.TableName == source.Name);
                        IEnumerable<TableColumn> targetColumns = this.targetSchemaInfo.TableColumns.Where(item => item.Schema == target.Schema && item.TableName == source.Name);

                        var columnDifferences = this.CompareTableChildren<TableColumn>("Column", DatabaseObjectType.Column, sourceColumns, targetColumns);

                        difference.SubDifferences.AddRange(columnDifferences);
                        #endregion

                        #region Trigger
                        IEnumerable<TableTrigger> sourceTriggers = this.sourceShemaInfo.TableTriggers.Where(item => item.Schema == source.Schema && item.TableName == source.Name);
                        IEnumerable<TableTrigger> targetTriggers = this.targetSchemaInfo.TableTriggers.Where(item => item.Schema == target.Schema && item.TableName == source.Name);

                        var triggerDifferences = this.CompareDatabaseObjects<TableTrigger>("Trigger", DatabaseObjectType.Trigger, sourceTriggers, targetTriggers);

                        foreach (var triggerDiff in triggerDifferences)
                        {
                            triggerDiff.ParentName = target.Name;
                        }

                        difference.SubDifferences.AddRange(triggerDifferences);
                        #endregion

                        #region Index
                        IEnumerable<TableIndex> sourceIndexes = this.sourceShemaInfo.TableIndexes.Where(item => item.Schema == source.Schema && item.TableName == source.Name);
                        IEnumerable<TableIndex> targetIndexes = this.targetSchemaInfo.TableIndexes.Where(item => item.Schema == target.Schema && item.TableName == source.Name);

                        var indexDifferences = this.CompareTableIndex(sourceIndexes, targetIndexes);

                        difference.SubDifferences.AddRange(indexDifferences);
                        #endregion

                        #region Primary Key
                        IEnumerable<TablePrimaryKey> sourcePrimaryKeys = this.sourceShemaInfo.TablePrimaryKeys.Where(item => item.Schema == source.Schema && item.TableName == source.Name);
                        IEnumerable<TablePrimaryKey> targetPrimaryKeys = this.targetSchemaInfo.TablePrimaryKeys.Where(item => item.Schema == target.Schema && item.TableName == source.Name);

                        var primaryKeyDifferences = this.CompareTablePrimaryKey(sourcePrimaryKeys, targetPrimaryKeys);

                        difference.SubDifferences.AddRange(primaryKeyDifferences);
                        #endregion

                        #region Foreign Key
                        IEnumerable<TableForeignKey> sourceForeignKeys = this.sourceShemaInfo.TableForeignKeys.Where(item => item.Schema == source.Schema && item.TableName == source.Name);
                        IEnumerable<TableForeignKey> targetForeignKeys = this.targetSchemaInfo.TableForeignKeys.Where(item => item.Schema == target.Schema && item.TableName == source.Name);

                        var foreignKeyDifferences = this.CompareTableForeignKey(sourceForeignKeys, targetForeignKeys);

                        difference.SubDifferences.AddRange(foreignKeyDifferences);
                        #endregion

                        #region Constraint
                        IEnumerable<TableConstraint> sourceConstraints = this.sourceShemaInfo.TableConstraints.Where(item => item.Schema == source.Schema && item.TableName == source.Name);
                        IEnumerable<TableConstraint> targetConstraints = this.targetSchemaInfo.TableConstraints.Where(item => item.Schema == target.Schema && item.TableName == source.Name);

                        var constraintDifferences = this.CompareTableChildren<TableConstraint>("Constraint", DatabaseObjectType.Constraint, sourceConstraints, targetConstraints);

                        difference.SubDifferences.AddRange(constraintDifferences);
                        #endregion

                        difference.SubDifferences.ForEach(item => item.Parent = difference);

                        if (difference.SubDifferences.Any(item => item.DifferenceType != SchemaCompareDifferenceType.None))
                        {
                            difference.DifferenceType = SchemaCompareDifferenceType.Modified;
                        }
                    }
                }
            }

            var sortedSourceTables = TableReferenceHelper.ResortTables(sourceShemaInfo.Tables, sourceShemaInfo.TableForeignKeys);

            foreach (var source in sortedSourceTables)
            {
                if (!targetSchemaInfo.Tables.Any(item => this.IsNameEquals(item.Name, source.Name)))
                {
                    SchemaCompareDifference difference = new SchemaCompareDifference() { Type = nameof(Table), DatabaseObjectType = DatabaseObjectType.Table };
                    difference.DifferenceType = SchemaCompareDifferenceType.Added;
                    difference.Source = source;

                    differences.Add(difference);
                }
            }

            #endregion

            differences.AddRange(this.CompareDatabaseObjects<View>(nameof(View), DatabaseObjectType.View, this.sourceShemaInfo.Views, targetSchemaInfo.Views));
            differences.AddRange(this.CompareDatabaseObjects<Function>(nameof(Function), DatabaseObjectType.Function, this.sourceShemaInfo.Functions, targetSchemaInfo.Functions));
            differences.AddRange(this.CompareDatabaseObjects<Procedure>(nameof(Procedure), DatabaseObjectType.Procedure, this.sourceShemaInfo.Procedures, targetSchemaInfo.Procedures));

            return differences;
        }

        private List<SchemaCompareDifference> CompareDatabaseObjects<T>(string type, DatabaseObjectType databaseObjectType, IEnumerable<T> sourceObjects, IEnumerable<T> targetObjects)
            where T : DatabaseObject
        {
            List<SchemaCompareDifference> differences = new List<SchemaCompareDifference>();

            foreach (T target in targetObjects)
            {
                SchemaCompareDifference difference = new SchemaCompareDifference() { Type = type, DatabaseObjectType = databaseObjectType };

                T source = sourceObjects.FirstOrDefault(item => this.IsNameEquals(item.Name, target.Name));

                if (source == null)
                {
                    difference.DifferenceType = SchemaCompareDifferenceType.Deleted;
                    difference.Target = target;

                    differences.Add(difference);
                }
                else
                {
                    difference.Source = source;
                    difference.Target = target;

                    if (!this.IsDbObjectEquals(source, target))
                    {
                        difference.DifferenceType = SchemaCompareDifferenceType.Modified;
                    }

                    differences.Add(difference);
                }
            }

            foreach (T source in sourceObjects)
            {
                if (!targetObjects.Any(item => this.IsNameEquals(item.Name, source.Name)))
                {
                    SchemaCompareDifference difference = new SchemaCompareDifference() { Type = type, DatabaseObjectType = databaseObjectType };
                    difference.DifferenceType = SchemaCompareDifferenceType.Added;
                    difference.Source = source;

                    differences.Add(difference);
                }
            }

            return differences;
        }


        private List<SchemaCompareDifference> CompareTableChildren<T>(string type, DatabaseObjectType databaseObjectType, IEnumerable<T> sourceObjects, IEnumerable<T> targetObjects)
            where T : TableChild
        {
            List<SchemaCompareDifference> differences = new List<SchemaCompareDifference>();

            foreach (T target in targetObjects)
            {
                SchemaCompareDifference difference = new SchemaCompareDifference() { Type = type, DatabaseObjectType = databaseObjectType, ParentName = target.TableName };

                T source = sourceObjects.FirstOrDefault(item => this.IsNameEquals(item.Name, target.Name));

                if (source == null)
                {
                    difference.DifferenceType = SchemaCompareDifferenceType.Deleted;
                    difference.Target = target;

                    differences.Add(difference);
                }
                else
                {
                    difference.Source = source;
                    difference.Target = target;

                    if (!this.IsDbObjectEquals(source, target))
                    {
                        difference.DifferenceType = SchemaCompareDifferenceType.Modified;

                        differences.Add(difference);
                    }
                }
            }

            foreach (T source in sourceObjects)
            {
                if (!targetObjects.Any(item => this.IsNameEquals(item.Name, source.Name)))
                {
                    SchemaCompareDifference difference = new SchemaCompareDifference() { Type = type, DatabaseObjectType = databaseObjectType, ParentName = source.TableName };
                    difference.DifferenceType = SchemaCompareDifferenceType.Added;
                    difference.Source = source;

                    differences.Add(difference);
                }
            }

            return differences;
        }

        private List<SchemaCompareDifference> CompareTablePrimaryKey(IEnumerable<TablePrimaryKey> sourceObjects, IEnumerable<TablePrimaryKey> targetObjects)
        {
            string type = "Primary Key";
            DatabaseObjectType databaseObjectType = DatabaseObjectType.PrimaryKey;

            List<SchemaCompareDifference> differences = new List<SchemaCompareDifference>();

            foreach (TablePrimaryKey target in targetObjects)
            {
                SchemaCompareDifference difference = new SchemaCompareDifference() { Type = type, DatabaseObjectType = databaseObjectType, ParentName = target.TableName };

                TablePrimaryKey source = null;

                if(!string.IsNullOrEmpty(target.Name))
                {
                    source = sourceObjects.FirstOrDefault(item=>item.Name == target.Name);
                }

                if(source == null)
                {
                    source = sourceObjects.FirstOrDefault(item => SchemaInfoHelper.IsIndexColumnsEquals(item.Columns, target.Columns));
                }                   

                if (source == null)
                {
                    difference.DifferenceType = SchemaCompareDifferenceType.Deleted;
                    difference.Target = target;

                    differences.Add(difference);
                }
                else
                {
                    difference.Source = source;
                    difference.Target = target;

                    List<string> ignorePropertyNames = new List<string>();

                    if (source.Name == null || target.Name == null)
                    {
                        if(this.ignoreUnnamedTableChildDifference)
                        {
                            ignorePropertyNames.Add("Name");
                        }                       
                    }

                    if (!this.IsDbObjectEquals(source, target, ignorePropertyNames))
                    {
                        difference.DifferenceType = SchemaCompareDifferenceType.Modified;

                        differences.Add(difference);
                    }
                }
            }

            foreach (TablePrimaryKey source in sourceObjects)
            {
                TablePrimaryKey target = null;
                
                if(!string.IsNullOrEmpty(source.Name))
                {
                    target = targetObjects.FirstOrDefault(item => item.Name == source.Name);
                }

                if(target ==null)
                {
                    target = targetObjects.FirstOrDefault(item => SchemaInfoHelper.IsIndexColumnsEquals(item.Columns, source.Columns));
                }               

                if (target == null)
                {
                    SchemaCompareDifference difference = new SchemaCompareDifference() { Type = type, DatabaseObjectType = databaseObjectType, ParentName = source.TableName };
                    difference.DifferenceType = SchemaCompareDifferenceType.Added;
                    difference.Source = source;

                    differences.Add(difference);
                }
            }

            return differences;
        }

        private List<SchemaCompareDifference> CompareTableIndex(IEnumerable<TableIndex> sourceObjects, IEnumerable<TableIndex> targetObjects)
        {
            string type = "Index";
            DatabaseObjectType databaseObjectType = DatabaseObjectType.Index;

            List<SchemaCompareDifference> differences = new List<SchemaCompareDifference>();

            foreach (TableIndex target in targetObjects)
            {
                SchemaCompareDifference difference = new SchemaCompareDifference() { Type = type, DatabaseObjectType = databaseObjectType, ParentName = target.TableName };

                TableIndex source = null;

                if(!string.IsNullOrEmpty(target.Name))
                {
                    source = sourceObjects.FirstOrDefault(item => item.Name == target.Name);
                }

                if(source == null)
                {
                    source = sourceObjects.FirstOrDefault(item => SchemaInfoHelper.IsIndexColumnsEquals(item.Columns, target.Columns));
                }                  

                if (source == null)
                {
                    difference.DifferenceType = SchemaCompareDifferenceType.Deleted;
                    difference.Target = target;

                    differences.Add(difference);
                }
                else
                {
                    difference.Source = source;
                    difference.Target = target;

                    List<string> ignorePropertyNames = new List<string>();

                    if (source.Name == null || target.Name == null)
                    {
                        if (this.ignoreUnnamedTableChildDifference)
                        {
                            ignorePropertyNames.Add("Name");
                        }
                    }

                    if(this.databaseType == DatabaseType.Sqlite)
                    {
                        if(source.IsUnique && target.IsUnique)
                        {
                            ignorePropertyNames.Add("Type");
                        }
                    }

                    if (!this.IsDbObjectEquals(source, target, ignorePropertyNames))
                    {
                        difference.DifferenceType = SchemaCompareDifferenceType.Modified;

                        differences.Add(difference);
                    }
                }
            }

            foreach (TableIndex source in sourceObjects)
            {
                TableIndex target = null;
                
                if(!string.IsNullOrEmpty(source.Name))
                {
                    target = targetObjects.FirstOrDefault(item => item.Name == source.Name);
                }
                
                if(target == null)
                {
                    target = targetObjects.FirstOrDefault(item => SchemaInfoHelper.IsIndexColumnsEquals(item.Columns, source.Columns));
                }              

                if (target == null)
                {
                    SchemaCompareDifference difference = new SchemaCompareDifference() { Type = type, DatabaseObjectType = databaseObjectType, ParentName = source.TableName };
                    difference.DifferenceType = SchemaCompareDifferenceType.Added;
                    difference.Source = source;

                    differences.Add(difference);
                }
            }

            return differences;
        }

        private List<SchemaCompareDifference> CompareTableForeignKey(IEnumerable<TableForeignKey> sourceObjects, IEnumerable<TableForeignKey> targetObjects)
        {
            string type = "Foreign Key";
            DatabaseObjectType databaseObjectType = DatabaseObjectType.ForeignKey;

            List<SchemaCompareDifference> differences = new List<SchemaCompareDifference>();

            foreach (TableForeignKey target in targetObjects)
            {
                SchemaCompareDifference difference = new SchemaCompareDifference() { Type = type, DatabaseObjectType = databaseObjectType, ParentName = target.TableName };

                TableForeignKey source = null;
                
                if(!string.IsNullOrEmpty(target.Name))
                {
                    source = sourceObjects.FirstOrDefault(item => item.Name == target.Name);
                }
                
                if(source == null)
                {
                    source = sourceObjects.FirstOrDefault(item => item.ReferencedTableName == target.ReferencedTableName && SchemaInfoHelper.IsForeignKeyColumnsEquals(item.Columns, target.Columns));
                }               

                if (source == null)
                {
                    difference.DifferenceType = SchemaCompareDifferenceType.Deleted;
                    difference.Target = target;

                    differences.Add(difference);
                }
                else
                {
                    difference.Source = source;
                    difference.Target = target;

                    List<string> ignorePropertyNames = new List<string>();

                    if(source.Name == null || target.Name == null)
                    {
                        if (this.ignoreUnnamedTableChildDifference)
                        {
                            ignorePropertyNames.Add("Name");
                        }
                    }

                    if (!this.IsDbObjectEquals(source, target, ignorePropertyNames))
                    {
                        difference.DifferenceType = SchemaCompareDifferenceType.Modified;

                        differences.Add(difference);
                    }
                }
            }

            foreach (TableForeignKey source in sourceObjects)
            {
                TableForeignKey target = null;
                
                if(!string.IsNullOrEmpty(source.Name))
                {
                    target = targetObjects.FirstOrDefault(item => item.Name == source.Name);
                }

                if(target == null)
                {
                    target = targetObjects.FirstOrDefault(item => item.ReferencedTableName == source.ReferencedTableName && SchemaInfoHelper.IsForeignKeyColumnsEquals(item.Columns, source.Columns));
                }               

                if (target == null)
                {
                    SchemaCompareDifference difference = new SchemaCompareDifference() { Type = type, DatabaseObjectType = databaseObjectType, ParentName = source.TableName };
                    difference.DifferenceType = SchemaCompareDifferenceType.Added;
                    difference.Source = source;

                    differences.Add(difference);
                }
            }

            return differences;
        }

        private bool IsNameEquals(string name1, string name2)
        {
            return name1?.ToLower() == name2?.ToLower();
        }

        private bool IsDbObjectEquals(DatabaseObject source, DatabaseObject target, List<string> ignorePropertyNames = null)
        {
            ComparisonConfig config = new ComparisonConfig();
            config.MembersToIgnore = new List<string>() { nameof(DatabaseObject.Schema), nameof(DatabaseObject.Order) };
            config.CaseSensitive = false;
            config.IgnoreStringLeadingTrailingWhitespace = true;
            config.TreatStringEmptyAndNullTheSame = true;
            config.CustomComparers.Add(new TableColumnComparer(RootComparerFactory.GetRootComparer()));

            if(ignorePropertyNames!=null && ignorePropertyNames.Count>0)
            {
                config.MembersToIgnore.AddRange(ignorePropertyNames);
            }

            CompareLogic compareLogic = new CompareLogic(config);

            ComparisonResult result = compareLogic.Compare(source, target);

            return result.AreEqual;
        }
    }
}
