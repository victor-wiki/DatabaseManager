using System;
using System.Collections.Generic;
using System.Text;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Core;
using DatabaseManager.Model;
using System.Linq;
using System.Threading.Tasks;
using DatabaseInterpreter.Utility;


namespace DatabaseManager.Core
{
    public class DbSynchro
    {
        private IObserver<FeedbackInfo> observer;
        private DbInterpreter sourceInterpreter;
        private DbInterpreter targetInterpreter;
        private DbScriptGenerator targetScriptGenerator;
        private TableManager tableManager;

        public DbSynchro(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter)
        {
            this.sourceInterpreter = sourceInterpreter;
            this.targetInterpreter = targetInterpreter;

            this.tableManager = new TableManager(this.targetInterpreter);
            this.targetScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(targetInterpreter);
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public async Task<ContentSaveResult> Sync(SchemaInfo schemaInfo, string targetDbSchema, IEnumerable<DbDifference> differences)
        {
            List<Script> scripts = await this.GenerateChangedScripts(schemaInfo, targetDbSchema, differences);

            if (scripts == null || scripts.Count == 0)
            {
                return this.GetFaultSaveResult("No changes need to save.");
            }

            try
            {
                ScriptRunner scriptRunner = new ScriptRunner();

                await scriptRunner.Run(this.targetInterpreter, scripts);

                return new ContentSaveResult() { IsOK = true };
            }
            catch (Exception ex)
            {
                string errMsg = ExceptionHelper.GetExceptionDetails(ex);

                this.FeedbackError(errMsg);

                return this.GetFaultSaveResult(errMsg);
            }
        }

        private ContentSaveResult GetFaultSaveResult(string message)
        {
            return new ContentSaveResult() { ResultData = message };
        }

        public async Task<List<Script>> GenerateChangedScripts(SchemaInfo schemaInfo, string targetDbSchema, IEnumerable<DbDifference> differences)
        {
            List<Script> scripts = new List<Script>();
            List<Script> tableScripts = new List<Script>();

            foreach (DbDifference difference in differences)
            {
                DbDifferenceType diffType = difference.DifferenceType;

                if (diffType == DbDifferenceType.None)
                {
                    continue;
                }

                switch (difference.DatabaseObjectType)
                {
                    case DatabaseObjectType.Table:
                        tableScripts.AddRange(await this.GenerateTableChangedScripts(schemaInfo, difference, targetDbSchema));
                        break;
                    case DatabaseObjectType.View:
                    case DatabaseObjectType.Function:
                    case DatabaseObjectType.Procedure:
                        tableScripts.AddRange(this.GenereateScriptDbObjectChangedScripts(difference, targetDbSchema));
                        break;
                }
            }

            scripts.InsertRange(0, tableScripts);

            return scripts;
        }

        public List<Script> GenereateScriptDbObjectChangedScripts(DbDifference difference, string targetDbSchema)
        {
            List<Script> scripts = new List<Script>();

            DbDifferenceType diffType = difference.DifferenceType;

            ScriptDbObject sourceScriptDbObject = difference.Source as ScriptDbObject;
            ScriptDbObject targetScriptDbObject = difference.Target as ScriptDbObject;

            if (diffType == DbDifferenceType.Added)
            {
                var cloneObj = this.CloneDbObject(sourceScriptDbObject, targetDbSchema);
                scripts.Add(new CreateDbObjectScript<ScriptDbObject>(cloneObj.Definition));
            }
            else if (diffType == DbDifferenceType.Deleted)
            {
                scripts.Add(this.targetScriptGenerator.Drop(sourceScriptDbObject));
            }
            else if (diffType == DbDifferenceType.Modified)
            {
                var cloneObj = this.CloneDbObject(sourceScriptDbObject, targetScriptDbObject.Schema);
                scripts.Add(this.targetScriptGenerator.Drop(targetScriptDbObject));
                scripts.Add(this.targetScriptGenerator.Create(cloneObj));
            }

            return scripts;
        }

        public List<Script> GenereateUserDefinedTypeChangedScripts(DbDifference difference, string targetDbSchema)
        {
            List<Script> scripts = new List<Script>();

            DbDifferenceType diffType = difference.DifferenceType;

            UserDefinedType source = difference.Source as UserDefinedType;
            UserDefinedType target = difference.Target as UserDefinedType;

            if (diffType == DbDifferenceType.Added)
            {
                var cloneObj = this.CloneDbObject(source, targetDbSchema);
                scripts.Add(this.targetScriptGenerator.CreateUserDefinedType(cloneObj));
            }
            else if (diffType == DbDifferenceType.Deleted)
            {
                scripts.Add(this.targetScriptGenerator.DropUserDefinedType(source));
            }
            else if (diffType == DbDifferenceType.Modified)
            {
                var cloneObj = this.CloneDbObject(source, target.Schema);
                scripts.Add(this.targetScriptGenerator.DropUserDefinedType(target));
                scripts.Add(this.targetScriptGenerator.CreateUserDefinedType(cloneObj));
            }

            return scripts;
        }

        public async Task<List<Script>> GenerateTableChangedScripts(SchemaInfo schemaInfo, DbDifference difference, string targetDbSchema)
        {
            List<Script> scripts = new List<Script>();

            DbDifferenceType diffType = difference.DifferenceType;

            Table sourceTable = difference.Source as Table;
            Table targetTable = difference.Target as Table;

            if (diffType == DbDifferenceType.Added)
            {
                List<TableColumn> columns = schemaInfo.TableColumns.Where(item => item.Schema == sourceTable.Schema && item.TableName == sourceTable.Name).OrderBy(item => item.Order).ToList();
                TablePrimaryKey primaryKey = schemaInfo.TablePrimaryKeys.FirstOrDefault(item => item.Schema == sourceTable.Schema && item.TableName == sourceTable.Name);
                List<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.Schema == sourceTable.Schema && item.TableName == sourceTable.Name).ToList();
                List<TableIndex> indexes = schemaInfo.TableIndexes.Where(item => item.Schema == sourceTable.Schema && item.TableName == sourceTable.Name).OrderBy(item => item.Order).ToList();
                List<TableConstraint> constraints = schemaInfo.TableConstraints.Where(item => item.Schema == sourceTable.Schema && item.TableName == sourceTable.Name).ToList();

                this.ChangeSchema(columns, targetDbSchema);
                primaryKey = this.CloneDbObject(primaryKey, targetDbSchema);
                this.ChangeSchema(foreignKeys, targetDbSchema);
                this.ChangeSchema(indexes, targetDbSchema);
                this.ChangeSchema(constraints, targetDbSchema);

                scripts.AddRange(this.targetScriptGenerator.CreateTable(sourceTable, columns, primaryKey, foreignKeys, indexes, constraints).Scripts);
            }
            else if (diffType == DbDifferenceType.Deleted)
            {
                scripts.Add(this.targetScriptGenerator.DropTable(targetTable));
            }
            else if (diffType == DbDifferenceType.Modified)
            {
                if (!ValueHelper.IsStringEquals(sourceTable.Comment, targetTable.Comment))
                {
                    scripts.Add(targetScriptGenerator.SetTableComment(sourceTable, string.IsNullOrEmpty(targetTable.Comment)));
                }

                foreach (DbDifference subDiff in difference.SubDifferences)
                {
                    DbDifferenceType subDiffType = subDiff.DifferenceType;

                    if (subDiffType == DbDifferenceType.None)
                    {
                        continue;
                    }

                    DatabaseObjectType subDbObjectType = subDiff.DatabaseObjectType;

                    switch (subDbObjectType)
                    {
                        case DatabaseObjectType.Column:
                        case DatabaseObjectType.PrimaryKey:
                        case DatabaseObjectType.ForeignKey:
                        case DatabaseObjectType.Index:
                        case DatabaseObjectType.Constraint:
                            scripts.AddRange(await this.GenerateTableChildChangedScripts(subDiff));
                            break;

                        case DatabaseObjectType.Trigger:
                            scripts.AddRange(this.GenereateScriptDbObjectChangedScripts(subDiff, targetDbSchema));
                            break;
                    }
                }
            }

            return scripts;
        }

        public async Task<List<Script>> GenerateTableChildChangedScripts(DbDifference difference)
        {
            List<Script> scripts = new List<Script>();

            Table targetTable = difference.Parent.Target as Table;

            DbDifferenceType diffType = difference.DifferenceType;

            TableChild source = difference.Source as TableChild;
            TableChild target = difference.Target as TableChild;

            if (diffType == DbDifferenceType.Added)
            {
                scripts.Add(this.targetScriptGenerator.Create(this.CloneTableChild(source, difference.DatabaseObjectType, targetTable.Schema)));
            }
            else if (diffType == DbDifferenceType.Deleted)
            {
                scripts.Add(this.targetScriptGenerator.Drop(target));
            }
            else if (diffType == DbDifferenceType.Modified)
            {
                if (difference.DatabaseObjectType == DatabaseObjectType.Column)
                {
                    SchemaInfoFilter filter = new SchemaInfoFilter() { Schema = source.Schema, TableNames = new string[] { source.TableName } };
                    List<TableDefaultValueConstraint> defaultValueConstraints = await this.tableManager.GetTableDefaultConstraints(filter);

                    Table table = new Table() { Schema = targetTable.Schema, Name = target.TableName };

                    TableColumn sourceColumn = source as TableColumn;
                    TableColumn targetColumn = target as TableColumn;

                    if (tableManager.IsNameChanged(sourceColumn.Name, targetColumn.Name))
                    {
                        scripts.Add(this.tableManager.GetColumnRenameScript(table, sourceColumn, targetColumn));
                    }

                    scripts.AddRange(this.tableManager.GetColumnAlterScripts(table, table, targetColumn, sourceColumn, defaultValueConstraints));
                }
                else
                {
                    var clonedSource = this.CloneTableChild(difference.Source, difference.DatabaseObjectType, targetTable.Schema);

                    if (difference.DatabaseObjectType == DatabaseObjectType.PrimaryKey)
                    {
                        scripts.AddRange(this.tableManager.GetPrimaryKeyAlterScripts(target as TablePrimaryKey, clonedSource as TablePrimaryKey, false));
                    }
                    else if (difference.DatabaseObjectType == DatabaseObjectType.ForeignKey)
                    {
                        scripts.AddRange(this.tableManager.GetForeignKeyAlterScripts(target as TableForeignKey, clonedSource as TableForeignKey));
                    }
                    else if (difference.DatabaseObjectType == DatabaseObjectType.Index)
                    {
                        scripts.AddRange(this.tableManager.GetIndexAlterScripts(target as TableIndex, clonedSource as TableIndex));
                    }
                    else if (difference.DatabaseObjectType == DatabaseObjectType.Constraint)
                    {
                        scripts.AddRange(this.tableManager.GetConstraintAlterScripts(target as TableConstraint, clonedSource as TableConstraint));
                    }
                }
            }

            return scripts;
        }

        private DatabaseObject CloneTableChild(DatabaseObject tableChild, DatabaseObjectType databaseObjectType, string targetSchema)
        {
            if (databaseObjectType == DatabaseObjectType.PrimaryKey)
            {
                return this.CloneDbObject(tableChild as TablePrimaryKey, targetSchema);
            }
            else if (databaseObjectType == DatabaseObjectType.ForeignKey)
            {
                return this.CloneDbObject(tableChild as TableForeignKey, targetSchema);
            }
            else if (databaseObjectType == DatabaseObjectType.Index)
            {
                return this.CloneDbObject(tableChild as TableIndex, targetSchema);
            }
            else if (databaseObjectType == DatabaseObjectType.Constraint)
            {
                return this.CloneDbObject(tableChild as TableConstraint, targetSchema);
            }

            return tableChild;
        }

        private T CloneDbObject<T>(T dbObject, string owner) where T : DatabaseObject
        {
            if (dbObject == null)
            {
                return null;
            }

            T clonedObj = ObjectHelper.CloneObject<T>(dbObject);
            clonedObj.Schema = owner;

            return clonedObj;
        }

        private void ChangeSchema<T>(List<T> dbObjects, string schema) where T : DatabaseObject
        {
            dbObjects.ForEach(item => item = this.CloneDbObject<T>(item, schema));
        }

        public void Feedback(FeedbackInfoType infoType, string message)
        {
            FeedbackInfo info = new FeedbackInfo() { Owner = this, InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(message) };

            if (this.observer != null)
            {
                FeedbackHelper.Feedback(this.observer, info);
            }
        }

        public void FeedbackInfo(string message)
        {
            this.Feedback(FeedbackInfoType.Info, message);
        }

        public void FeedbackError(string message)
        {
            this.Feedback(FeedbackInfoType.Error, message);
        }
    }
}
