using DatabaseInterpreter.Model;
using DatabaseManager.Model;
using KellermanSoftware.CompareNetObjects;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseManager.Core
{
    public class DbCompare
    {
        private SchemaInfo sourceShemaInfo;
        private SchemaInfo targetSchemaInfo;    

        public DbCompare(SchemaInfo sourceShemaInfo, SchemaInfo targetSchemaInfo)
        {
            this.sourceShemaInfo = sourceShemaInfo;
            this.targetSchemaInfo = targetSchemaInfo;           
        }

        public List<DbDifference> Compare()
        {
            List<DbDifference> differences = new List<DbDifference>();

            differences.AddRange(this.CompareDatabaseObjects<UserDefinedType>(nameof(UserDefinedType), DatabaseObjectType.Type, this.sourceShemaInfo.UserDefinedTypes, targetSchemaInfo.UserDefinedTypes));

            #region Table
            foreach (Table target in targetSchemaInfo.Tables)
            {
                DbDifference difference = new DbDifference() { Type = nameof(Table), DatabaseObjectType = DatabaseObjectType.Table };

                Table source = this.sourceShemaInfo.Tables.FirstOrDefault(item => this.IsNameEquals(item.Name, target.Name));

                if (source == null)
                {
                    difference.DifferenceType = DbDifferenceType.Deleted;
                    difference.Target = target;

                    differences.Add(difference);
                }
                else
                {
                    difference.DifferenceType = DbDifferenceType.None;
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

                        foreach(var triggerDiff in triggerDifferences)
                        {
                            triggerDiff.ParentName = target.Name;
                        }

                        difference.SubDifferences.AddRange(triggerDifferences);
                        #endregion

                        #region Index
                        IEnumerable<TableIndex> sourceIndexes = this.sourceShemaInfo.TableIndexes.Where(item => item.Schema == source.Schema && item.TableName == source.Name);
                        IEnumerable<TableIndex> targetIndexes = this.targetSchemaInfo.TableIndexes.Where(item => item.Schema == target.Schema && item.TableName == source.Name);

                        var indexDifferences = this.CompareTableChildren<TableIndex>("Index", DatabaseObjectType.Index, sourceIndexes, targetIndexes);

                        difference.SubDifferences.AddRange(indexDifferences);
                        #endregion

                        #region Primary Key
                        IEnumerable<TablePrimaryKey> sourcePrimaryKeys = this.sourceShemaInfo.TablePrimaryKeys.Where(item => item.Schema == source.Schema && item.TableName == source.Name);
                        IEnumerable<TablePrimaryKey> targetPrimaryKeys = this.targetSchemaInfo.TablePrimaryKeys.Where(item => item.Schema == target.Schema && item.TableName == source.Name);

                        var primaryKeyDifferences = this.CompareTableChildren<TablePrimaryKey>("Primary Key", DatabaseObjectType.PrimaryKey, sourcePrimaryKeys, targetPrimaryKeys);

                        difference.SubDifferences.AddRange(primaryKeyDifferences);
                        #endregion

                        #region Foreign Key
                        IEnumerable<TableForeignKey> sourceForeignKeys = this.sourceShemaInfo.TableForeignKeys.Where(item => item.Schema == source.Schema && item.TableName == source.Name);
                        IEnumerable<TableForeignKey> targetForeignKeys = this.targetSchemaInfo.TableForeignKeys.Where(item => item.Schema == target.Schema && item.TableName == source.Name);

                        var foreignKeyDifferences = this.CompareTableChildren<TableForeignKey>("Foreign Key", DatabaseObjectType.ForeignKey, sourceForeignKeys, targetForeignKeys);

                        difference.SubDifferences.AddRange(foreignKeyDifferences);
                        #endregion

                        #region Constraint
                        IEnumerable<TableConstraint> sourceConstraints= this.sourceShemaInfo.TableConstraints.Where(item => item.Schema == source.Schema && item.TableName == source.Name);
                        IEnumerable<TableConstraint> targetConstraints = this.targetSchemaInfo.TableConstraints.Where(item => item.Schema == target.Schema && item.TableName == source.Name);

                        var constraintDifferences = this.CompareTableChildren<TableConstraint>("Constraint", DatabaseObjectType.Constraint, sourceConstraints, targetConstraints);

                        difference.SubDifferences.AddRange(constraintDifferences);
                        #endregion

                        difference.SubDifferences.ForEach(item => item.Parent = difference);

                        if (difference.SubDifferences.Any(item => item.DifferenceType != DbDifferenceType.None))
                        {
                            difference.DifferenceType = DbDifferenceType.Modified;
                        }
                    }
                }
            }

            foreach (Table source in this.sourceShemaInfo.Tables)
            {
                if (!targetSchemaInfo.Tables.Any(item => this.IsNameEquals(item.Name, source.Name)))
                {
                    DbDifference difference = new DbDifference() { Type = nameof(Table), DatabaseObjectType = DatabaseObjectType.Table };
                    difference.DifferenceType = DbDifferenceType.Added;
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

        private List<DbDifference> CompareTableChildren<T>(string type, DatabaseObjectType databaseObjectType, IEnumerable<T> sourceObjects, IEnumerable<T> targetObjects)
            where T : TableChild
        {
            List<DbDifference> differences = new List<DbDifference>();

            foreach (T target in targetObjects)
            {
                DbDifference difference = new DbDifference() { Type = type, DatabaseObjectType = databaseObjectType, ParentName = target.TableName };

                T source = sourceObjects.FirstOrDefault(item => this.IsNameEquals(item.Name, target.Name));

                if (source == null)
                {
                    difference.DifferenceType = DbDifferenceType.Deleted;
                    difference.Target = target;

                    differences.Add(difference);
                }
                else
                {
                    difference.Source = source;
                    difference.Target = target;

                    if (!this.IsDbObjectEquals(source, target))
                    {
                        difference.DifferenceType = DbDifferenceType.Modified;
                    }

                    differences.Add(difference);
                }
            }

            foreach (T source in sourceObjects)
            {
                if (!targetObjects.Any(item => this.IsNameEquals(item.Name, source.Name)))
                {
                    DbDifference difference = new DbDifference() { Type = type, DatabaseObjectType = databaseObjectType, ParentName = source.TableName };
                    difference.DifferenceType = DbDifferenceType.Added;
                    difference.Source = source;

                    differences.Add(difference);
                }
            }

            return differences;
        }

        private List<DbDifference> CompareDatabaseObjects<T>(string type, DatabaseObjectType databaseObjectType, IEnumerable<T> sourceObjects, IEnumerable<T> targetObjects)
            where T : DatabaseObject
        {
            List<DbDifference> differences = new List<DbDifference>();

            foreach (T target in targetObjects)
            {
                DbDifference difference = new DbDifference() { Type = type, DatabaseObjectType = databaseObjectType };

                T source = sourceObjects.FirstOrDefault(item => this.IsNameEquals(item.Name, target.Name));

                if (source == null)
                {
                    difference.DifferenceType = DbDifferenceType.Deleted;
                    difference.Target = target;

                    differences.Add(difference);
                }
                else
                {
                    difference.Source = source;
                    difference.Target = target;

                    if (!this.IsDbObjectEquals(source, target))
                    {
                        difference.DifferenceType = DbDifferenceType.Modified;
                    }

                    differences.Add(difference);
                }
            }

            foreach (T source in sourceObjects)
            {
                if (!targetObjects.Any(item => this.IsNameEquals(item.Name, source.Name)))
                {
                    DbDifference difference = new DbDifference() { Type = type, DatabaseObjectType = databaseObjectType };
                    difference.DifferenceType = DbDifferenceType.Added;
                    difference.Source = source;

                    differences.Add(difference);
                }
            }

            return differences;
        }

        private bool IsNameEquals(string name1, string name2)
        {
            return name1.ToLower() == name2.ToLower();
        }

        private bool IsDbObjectEquals(DatabaseObject source, DatabaseObject target)
        {
            ComparisonConfig config = new ComparisonConfig();
            config.MembersToIgnore = new List<string>() { nameof(DatabaseObject.Schema), nameof(DatabaseObject.Order) };
            config.CaseSensitive = false;
            config.IgnoreStringLeadingTrailingWhitespace = true;
            config.TreatStringEmptyAndNullTheSame = true;
            config.CustomComparers.Add(new TableColumnComparer(RootComparerFactory.GetRootComparer()));

            CompareLogic compareLogic = new CompareLogic(config);

            ComparisonResult result = compareLogic.Compare(source, target);

            return result.AreEqual;
        }
    }
}
