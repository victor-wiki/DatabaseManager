using System;
using System.Collections.Generic;
using System.Text;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Core;
using DatabaseManager.Model;
using System.Linq;
using DatabaseInterpreter.Utility;
using System.Data.Common;
using System.Data;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DatabaseManager.Core
{
    public class TableManager
    {
        private IObserver<FeedbackInfo> observer;
        private DbInterpreter dbInterpreter;

        public TableManager(DbInterpreter dbInterpreter)
        {
            this.dbInterpreter = dbInterpreter;
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public async Task<ContentSaveResult> Save(SchemaDesignerInfo schemaDesignerInfo, bool isNew)
        {
            Table table = null;

            try
            {
                using (DbConnection dbConnection = this.dbInterpreter.CreateConnection())
                {
                    dbConnection.Open();

                    DbTransaction transaction = dbConnection.BeginTransaction();

                    ContentSaveResult result = await this.GenerateChangeScripts(schemaDesignerInfo, isNew);

                    if (!result.IsOK)
                    {
                        return result;
                    }

                    TableDesignerGenerateScriptsData scriptsData = result.ResultData as TableDesignerGenerateScriptsData;

                    List<Script> scripts = scriptsData.Scripts;

                    table = scriptsData.Table;

                    if (scripts == null || scripts.Count == 0)
                    {
                        return this.GetFaultSaveResult("No change need to save.");
                    }

                    Func<Script, bool> isValidScript = (s) =>
                    {
                        return !(s is NewLineSript || s is SpliterScript || string.IsNullOrEmpty(s.Content) || s.Content == this.dbInterpreter.ScriptsDelimiter);
                    };

                    int count = scripts.Where(item => isValidScript(item)).Count();
                    int i = 0;

                    foreach (Script s in scripts)
                    {
                        if (!isValidScript(s))
                        {
                            continue;
                        }

                        string sql = s.Content?.Trim();

                        if (!string.IsNullOrEmpty(sql) && sql != this.dbInterpreter.ScriptsDelimiter)
                        {
                            i++;

                            if (this.dbInterpreter.ScriptsDelimiter.Length == 1 && sql.EndsWith(this.dbInterpreter.ScriptsDelimiter))
                            {
                                sql = sql.TrimEnd(this.dbInterpreter.ScriptsDelimiter.ToArray());
                            }

                            if (!this.dbInterpreter.HasError)
                            {
                                CommandInfo commandInfo = new CommandInfo()
                                {
                                    CommandType = CommandType.Text,
                                    CommandText = sql,
                                    Transaction = transaction
                                };

                                await this.dbInterpreter.ExecuteNonQueryAsync(dbConnection, commandInfo);
                            }
                        }
                    }

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                string errMsg = ExceptionHelper.GetExceptionDetails(ex);

                this.FeedbackError(errMsg);

                return this.GetFaultSaveResult(errMsg);
            }

            return new ContentSaveResult() { IsOK = true, ResultData = table };
        }

        public async Task<ContentSaveResult> GenerateChangeScripts(SchemaDesignerInfo schemaDesignerInfo, bool isNew)
        {
            string validateMsg;

            Table table = null;

            try
            {
                bool isValid = this.ValidateModel(schemaDesignerInfo, out validateMsg);

                if (!isValid)
                {
                    return this.GetFaultSaveResult(validateMsg);
                }

                TableDesignerGenerateScriptsData scriptsData = new TableDesignerGenerateScriptsData();

                SchemaInfo schemaInfo = this.GetSchemaInfo(schemaDesignerInfo);

                table = schemaInfo.Tables.First();

                scriptsData.Table = table;

                DbScriptGenerator scriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.dbInterpreter);

                List<Script> scripts = new List<Script>();

                if (isNew)
                {
                    ScriptBuilder scriptBuilder = scriptGenerator.GenerateSchemaScripts(schemaInfo);

                    scripts = scriptBuilder.Scripts;
                }
                else
                {
                    #region Alter Table
                    scripts = new List<Script>();

                    TableDesignerInfo tableDesignerInfo = schemaDesignerInfo.TableDesignerInfo;

                    SchemaInfoFilter filter = new SchemaInfoFilter() { Strict = true };
                    filter.TableNames = new string[] { tableDesignerInfo.OldName };
                    filter.DatabaseObjectType = DatabaseObjectType.Table
                        | DatabaseObjectType.TableColumn
                        | DatabaseObjectType.TablePrimaryKey
                        | DatabaseObjectType.TableForeignKey
                        | DatabaseObjectType.TableIndex
                        | DatabaseObjectType.TableConstraint;

                    if (this.dbInterpreter.DatabaseType == DatabaseType.Oracle)
                    {
                        this.dbInterpreter.Option.IncludePrimaryKeyWhenGetTableIndex = true;
                    }

                    SchemaInfo oldSchemaInfo = await this.dbInterpreter.GetSchemaInfoAsync(filter);
                    Table oldTable = oldSchemaInfo.Tables.FirstOrDefault();

                    if (oldTable == null)
                    {
                        return this.GetFaultSaveResult($"Table \"{tableDesignerInfo.OldName}\" is not existed");
                    }

                    if (tableDesignerInfo.OldName != tableDesignerInfo.Name)
                    {
                        scripts.Add(scriptGenerator.RenameTable(new Table() { Owner = tableDesignerInfo.Owner, Name = tableDesignerInfo.OldName }, tableDesignerInfo.Name));
                    }

                    if (!this.IsStringEquals(tableDesignerInfo.Comment, oldTable.Comment))
                    {
                        oldTable.Comment = tableDesignerInfo.Comment;
                        scripts.Add(scriptGenerator.SetTableComment(oldTable, string.IsNullOrEmpty(oldTable.Comment)));
                    }

                    #region Columns
                    List<TableColumnDesingerInfo> columnDesingerInfos = schemaDesignerInfo.TableColumnDesingerInfos;
                    List<TableColumn> oldColumns = oldSchemaInfo.TableColumns;

                    List<TableDefaultValueConstraint> defaultValueConstraints = null;

                    if (this.dbInterpreter.DatabaseType == DatabaseType.SqlServer)
                    {
                        defaultValueConstraints = await (this.dbInterpreter as SqlServerInterpreter).GetTableDefautValueConstraintsAsync(filter);
                    }

                    foreach (TableColumnDesingerInfo columnDesignerInfo in columnDesingerInfos)
                    {
                        TableColumn oldColumn = oldColumns.FirstOrDefault(item => item.Name.ToLower() == columnDesignerInfo.Name.ToLower());
                        TableColumn newColumn = schemaInfo.TableColumns.FirstOrDefault(item => item.Name == columnDesignerInfo.Name);

                        if (oldColumn == null)
                        {
                            scripts.Add(scriptGenerator.AddTableColumn(table, newColumn));
                        }
                        else
                        {
                            if (!this.IsValueEqualsIgnoreCase(columnDesignerInfo.OldName, columnDesignerInfo.Name))
                            {
                                scripts.Add(scriptGenerator.RenameTableColumn(table, oldColumn, newColumn.Name));
                            }

                            bool isDefaultValueEquals = this.IsStringEquals(ValueHelper.GetTrimedDefaultValue(oldColumn.DefaultValue), newColumn.DefaultValue);

                            if (!SchemaInfoHelper.IsTableColumnEquals(this.dbInterpreter.DatabaseType, oldColumn, newColumn)
                                || !isDefaultValueEquals)
                            {
                                if (!isDefaultValueEquals)
                                {
                                    if (this.dbInterpreter.DatabaseType == DatabaseType.SqlServer)
                                    {
                                        SqlServerScriptGenerator sqlServerScriptGenerator = scriptGenerator as SqlServerScriptGenerator;

                                        TableDefaultValueConstraint defaultValueConstraint = defaultValueConstraints?.FirstOrDefault(item => item.Owner == oldTable.Owner && item.TableName == oldTable.Name && item.ColumnName == oldColumn.Name);

                                        if (defaultValueConstraint != null)
                                        {
                                            scripts.Add(sqlServerScriptGenerator.DropDefaultValueConstraint(defaultValueConstraint));
                                        }

                                        scripts.Add(sqlServerScriptGenerator.AddDefaultValueConstraint(newColumn));
                                    }
                                }

                                Script alterColumnScript = scriptGenerator.AlterTableColumn(table, newColumn);

                                if (this.dbInterpreter.DatabaseType == DatabaseType.Oracle)
                                {
                                    if (!oldColumn.IsNullable && !newColumn.IsNullable)
                                    {
                                        alterColumnScript.Content = Regex.Replace(alterColumnScript.Content, "NOT NULL", "", RegexOptions.IgnoreCase);
                                    }
                                    else if (oldColumn.IsNullable && newColumn.IsNullable)
                                    {
                                        alterColumnScript.Content = Regex.Replace(alterColumnScript.Content, "NULL", "", RegexOptions.IgnoreCase);
                                    }
                                }

                                scripts.Add(alterColumnScript);
                            }
                            else if (!this.IsStringEquals(columnDesignerInfo.Comment, oldColumn.Comment))
                            {
                                scripts.Add(scriptGenerator.SetTableColumnComment(table, newColumn, string.IsNullOrEmpty(oldColumn.Comment)));
                            }
                        }
                    }

                    foreach (TableColumn oldColumn in oldColumns)
                    {
                        if (!columnDesingerInfos.Any(item => item.Name == oldColumn.Name))
                        {
                            scripts.Add(scriptGenerator.DropTableColumn(oldColumn));
                        }
                    }
                    #endregion

                    #region Primary Key
                    TablePrimaryKey oldPrimaryKey = oldSchemaInfo.TablePrimaryKeys.FirstOrDefault();
                    TablePrimaryKey newPrimaryKey = schemaInfo.TablePrimaryKeys.FirstOrDefault();

                    bool primaryKeyChanged = !SchemaInfoHelper.IsPrimaryKeyEquals(oldPrimaryKey, newPrimaryKey, schemaDesignerInfo.IgnoreTableIndex, true);

                    Action alterPrimaryKey = () =>
                    {
                        if (oldPrimaryKey != null)
                        {
                            scripts.Add(scriptGenerator.DropPrimaryKey(oldPrimaryKey));
                        }

                        scripts.Add(scriptGenerator.AddPrimaryKey(newPrimaryKey));

                        if (!string.IsNullOrEmpty(newPrimaryKey.Comment))
                        {
                            this.SetTableChildComment(scripts, scriptGenerator, newPrimaryKey, true);
                        }
                    };

                    if (primaryKeyChanged)
                    {
                        alterPrimaryKey();
                    }
                    else if (!ValueHelper.IsStringEquals(oldPrimaryKey?.Comment, newPrimaryKey?.Comment))
                    {
                        if (this.dbInterpreter.DatabaseType == DatabaseType.SqlServer)
                        {
                            this.SetTableChildComment(scripts, scriptGenerator, newPrimaryKey, string.IsNullOrEmpty(oldPrimaryKey?.Comment));
                        }
                        else
                        {
                            alterPrimaryKey();
                        }
                    }
                    #endregion

                    #region Index
                    if (!schemaDesignerInfo.IgnoreTableIndex)
                    {
                        IEnumerable<TableIndex> oldIndexes = oldSchemaInfo.TableIndexes.Where(item => !item.IsPrimary);

                        IEnumerable<TableIndexDesignerInfo> indexDesignerInfos = schemaDesignerInfo.TableIndexDesingerInfos.Where(item => !item.IsPrimary);

                        foreach (TableIndexDesignerInfo indexDesignerInfo in indexDesignerInfos)
                        {
                            TableIndex newIndex = schemaInfo.TableIndexes.FirstOrDefault(item => item.Name == indexDesignerInfo.Name);

                            TableIndex oldIndex = oldIndexes.FirstOrDefault(item => item.Name == indexDesignerInfo.OldName);

                            if (this.IsValueEqualsIgnoreCase(indexDesignerInfo.OldName, indexDesignerInfo.Name)
                                && this.IsValueEqualsIgnoreCase(indexDesignerInfo.OldType, indexDesignerInfo.Type)
                              )
                            {
                                if (oldIndex != null && this.IsStringEquals(oldIndex.Comment, newIndex.Comment) && SchemaInfoHelper.IsIndexColumnsEquals(oldIndex.Columns, newIndex.Columns))
                                {
                                    continue;
                                }
                            }

                            if (oldIndex != null)
                            {
                                scripts.Add(scriptGenerator.DropIndex(oldIndex));
                            }

                            scripts.Add(scriptGenerator.AddIndex(newIndex));

                            if (!string.IsNullOrEmpty(newIndex.Comment))
                            {
                                this.SetTableChildComment(scripts, scriptGenerator, newIndex, true);
                            }
                        }

                        foreach (TableIndex oldIndex in oldIndexes)
                        {
                            if (!indexDesignerInfos.Any(item => item.Name == oldIndex.Name))
                            {
                                scripts.Add(scriptGenerator.DropIndex(oldIndex));
                            }
                        }
                    }
                    #endregion

                    #region Foreign Key
                    if (!schemaDesignerInfo.IgnoreTableForeignKey)
                    {
                        List<TableForeignKey> oldForeignKeys = oldSchemaInfo.TableForeignKeys;

                        IEnumerable<TableForeignKeyDesignerInfo> foreignKeyDesignerInfos = schemaDesignerInfo.TableForeignKeyDesignerInfos;

                        foreach (TableForeignKeyDesignerInfo foreignKeyDesignerInfo in foreignKeyDesignerInfos)
                        {
                            TableForeignKey newForeignKey = schemaInfo.TableForeignKeys.FirstOrDefault(item => item.Name == foreignKeyDesignerInfo.Name);

                            TableForeignKey oldForeignKey = oldForeignKeys.FirstOrDefault(item => item.Name == foreignKeyDesignerInfo.OldName);

                            if (this.IsValueEqualsIgnoreCase(foreignKeyDesignerInfo.OldName, foreignKeyDesignerInfo.Name) &&
                                foreignKeyDesignerInfo.UpdateCascade == oldForeignKey.UpdateCascade &&
                                foreignKeyDesignerInfo.DeleteCascade == oldForeignKey.DeleteCascade)
                            {
                                if (oldForeignKey != null && this.IsStringEquals(oldForeignKey.Comment, newForeignKey.Comment)
                                    && SchemaInfoHelper.IsForeignKeyColumnsEquals(oldForeignKey.Columns, newForeignKey.Columns))
                                {
                                    continue;
                                }
                            }

                            if (oldForeignKey != null)
                            {
                                scripts.Add(scriptGenerator.DropForeignKey(oldForeignKey));
                            }

                            scripts.Add(scriptGenerator.AddForeignKey(newForeignKey));

                            if (!string.IsNullOrEmpty(newForeignKey.Comment))
                            {
                                this.SetTableChildComment(scripts, scriptGenerator, newForeignKey, true);
                            }
                        }

                        foreach (TableForeignKey oldForeignKey in oldForeignKeys)
                        {
                            if (!foreignKeyDesignerInfos.Any(item => item.Name == oldForeignKey.Name))
                            {
                                scripts.Add(scriptGenerator.DropForeignKey(oldForeignKey));
                            }
                        }
                    }
                    #endregion

                    #region Constraint
                    if (!schemaDesignerInfo.IgnoreTableConstraint)
                    {
                        List<TableConstraint> oldConstraints = oldSchemaInfo.TableConstraints;

                        IEnumerable<TableConstraintDesignerInfo> constraintDesignerInfos = schemaDesignerInfo.TableConstraintDesignerInfos;

                        foreach (TableConstraintDesignerInfo constraintDesignerInfo in constraintDesignerInfos)
                        {
                            TableConstraint newConstraint = schemaInfo.TableConstraints.FirstOrDefault(item => item.Name == constraintDesignerInfo.Name);

                            TableConstraint oldConstraint = oldConstraints.FirstOrDefault(item => item.Name == constraintDesignerInfo.OldName);

                            if (this.IsValueEqualsIgnoreCase(constraintDesignerInfo.OldName, constraintDesignerInfo.Name))
                            {
                                if (oldConstraint != null && this.IsStringEquals(oldConstraint.Comment, newConstraint.Comment))
                                {
                                    continue;
                                }
                            }

                            if (oldConstraint != null)
                            {
                                scripts.Add(scriptGenerator.DropCheckConstraint(oldConstraint));
                            }

                            scripts.Add(scriptGenerator.AddCheckConstraint(newConstraint));

                            if (!string.IsNullOrEmpty(newConstraint.Comment))
                            {
                                this.SetTableChildComment(scripts, scriptGenerator, newConstraint, true);
                            }
                        }

                        foreach (TableConstraint oldConstraint in oldConstraints)
                        {
                            if (!constraintDesignerInfos.Any(item => item.Name == oldConstraint.Name))
                            {
                                scripts.Add(scriptGenerator.DropCheckConstraint(oldConstraint));
                            }
                        }
                    }
                    #endregion

                    #endregion
                }

                scriptsData.Scripts.AddRange(scripts);

                return new ContentSaveResult() { IsOK = true, ResultData = scriptsData };
            }
            catch (Exception ex)
            {
                return this.GetFaultSaveResult(ExceptionHelper.GetExceptionDetails(ex));
            }
        }

        private bool IsValueEqualsIgnoreCase(string value1, string value2)
        {
            return !string.IsNullOrEmpty(value1) && !string.IsNullOrEmpty(value2) && value1.ToLower() == value2.ToLower();
        }

        private void SetTableChildComment(List<Script> scripts, DbScriptGenerator scriptGenerator, TableChild tableChild, bool isNew)
        {
            if (this.dbInterpreter.DatabaseType == DatabaseType.SqlServer)
            {
                scripts.Add((scriptGenerator as SqlServerScriptGenerator).SetTableChildComment(tableChild, isNew));
            }
        }

        private bool IsStringEquals(string str1, string str2)
        {
            return ValueHelper.IsStringEquals(str1, str2);
        }

        private ContentSaveResult GetFaultSaveResult(string message)
        {
            return new ContentSaveResult() { ResultData = message };
        }

        public SchemaInfo GetSchemaInfo(SchemaDesignerInfo schemaDesignerInfo)
        {
            SchemaInfo schemaInfo = new SchemaInfo();

            Table table = new Table();
            ObjectHelper.CopyProperties(schemaDesignerInfo.TableDesignerInfo, table);

            schemaInfo.Tables.Add(table);

            #region Columns
            TablePrimaryKey primaryKey = null;

            foreach (TableColumnDesingerInfo column in schemaDesignerInfo.TableColumnDesingerInfos)
            {
                TableColumn tableColumn = new TableColumn();
                ObjectHelper.CopyProperties(column, tableColumn);

                if (!tableColumn.IsUserDefined)
                {
                    ColumnManager.SetColumnLength(this.dbInterpreter.DatabaseType, tableColumn, column.Length);
                }

                if (column.IsPrimary)
                {
                    if (primaryKey == null)
                    {
                        primaryKey = new TablePrimaryKey() { Owner = table.Owner, TableName = table.Name, Name = IndexManager.GetPrimaryKeyDefaultName(table) };
                    }

                    IndexColumn indexColumn = new IndexColumn() { ColumnName = column.Name, IsDesc = false, Order = primaryKey.Columns.Count + 1 };

                    if (!schemaDesignerInfo.IgnoreTableIndex)
                    {
                        TableIndexDesignerInfo indexDesignerInfo = schemaDesignerInfo.TableIndexDesingerInfos
                            .FirstOrDefault(item => item.Type == IndexType.Primary.ToString() && item.Columns.Any(t => t.ColumnName == column.Name));

                        if (indexDesignerInfo != null)
                        {
                            primaryKey.Name = indexDesignerInfo.Name;
                            primaryKey.Comment = indexDesignerInfo.Comment;

                            IndexColumn columnInfo = indexDesignerInfo.Columns.FirstOrDefault(item => item.ColumnName == column.Name);

                            if (columnInfo != null)
                            {
                                indexColumn.IsDesc = columnInfo.IsDesc;
                            }

                            if (indexDesignerInfo.ExtraPropertyInfo != null)
                            {
                                primaryKey.Clustered = indexDesignerInfo.ExtraPropertyInfo.Clustered;
                            }
                        }
                    }

                    primaryKey.Columns.Add(indexColumn);
                }

                TableColumnExtraPropertyInfo extralProperty = column.ExtraPropertyInfo;

                if (column.IsIdentity)
                {
                    if (extralProperty != null)
                    {
                        table.IdentitySeed = extralProperty.Seed;
                        table.IdentityIncrement = extralProperty.Increment;
                    }
                    else
                    {
                        table.IdentitySeed = 1;
                        table.IdentityIncrement = 1;
                    }
                }

                if (extralProperty?.Expression != null)
                {
                    tableColumn.ComputeExp = extralProperty.Expression;
                }

                schemaInfo.TableColumns.Add(tableColumn);
            }

            if (primaryKey != null)
            {
                schemaInfo.TablePrimaryKeys.Add(primaryKey);
            }
            #endregion

            #region Indexes
            if (!schemaDesignerInfo.IgnoreTableIndex)
            {
                foreach (TableIndexDesignerInfo indexDesignerInfo in schemaDesignerInfo.TableIndexDesingerInfos)
                {
                    if (!indexDesignerInfo.IsPrimary)
                    {
                        TableIndex index = new TableIndex() { Owner = indexDesignerInfo.Owner, TableName = indexDesignerInfo.TableName };
                        index.Name = indexDesignerInfo.Name;

                        index.IsUnique = indexDesignerInfo.Type == IndexType.Unique.ToString();
                        index.Clustered = indexDesignerInfo.Clustered;
                        index.Comment = indexDesignerInfo.Comment;

                        index.Columns.AddRange(indexDesignerInfo.Columns);

                        int order = 1;
                        index.Columns.ForEach(item => { item.Order = order++; });

                        schemaInfo.TableIndexes.Add(index);
                    }
                }
            }
            #endregion

            #region Foreign Keys
            if (!schemaDesignerInfo.IgnoreTableForeignKey)
            {
                foreach (TableForeignKeyDesignerInfo keyDesignerInfo in schemaDesignerInfo.TableForeignKeyDesignerInfos)
                {
                    TableForeignKey foreignKey = new TableForeignKey() { Owner = keyDesignerInfo.Owner, TableName = keyDesignerInfo.TableName };
                    foreignKey.Name = keyDesignerInfo.Name;

                    foreignKey.ReferencedTableName = keyDesignerInfo.ReferencedTableName;
                    foreignKey.UpdateCascade = keyDesignerInfo.UpdateCascade;
                    foreignKey.DeleteCascade = keyDesignerInfo.DeleteCascade;
                    foreignKey.Comment = keyDesignerInfo.Comment;

                    foreignKey.Columns.AddRange(keyDesignerInfo.Columns);

                    int order = 1;
                    foreignKey.Columns.ForEach(item => { item.Order = order++; });

                    schemaInfo.TableForeignKeys.Add(foreignKey);
                }
            }
            #endregion

            #region Constraint
            if (!schemaDesignerInfo.IgnoreTableConstraint)
            {
                foreach (TableConstraintDesignerInfo constraintDesignerInfo in schemaDesignerInfo.TableConstraintDesignerInfos)
                {
                    TableConstraint constraint = new TableConstraint() { Owner = constraintDesignerInfo.Owner, TableName = constraintDesignerInfo.TableName };
                    constraint.Name = constraintDesignerInfo.Name;
                    constraint.Definition = constraintDesignerInfo.Definition;
                    constraint.Comment = constraintDesignerInfo.Comment;

                    schemaInfo.TableConstraints.Add(constraint);
                }
            }
            #endregion

            return schemaInfo;
        }

        private bool ValidateModel(SchemaDesignerInfo schemaDesignerInfo, out string message)
        {
            message = "";

            if (schemaDesignerInfo == null)
            {
                message = "Argument can't be null";
                return false;
            }

            TableDesignerInfo table = schemaDesignerInfo.TableDesignerInfo;

            if (table == null)
            {
                message = "No table information";
                return false;
            }

            if (string.IsNullOrEmpty(table.Name))
            {
                message = "Table Name can't be empty";
                return false;
            }

            #region Columns
            List<TableColumnDesingerInfo> columns = schemaDesignerInfo.TableColumnDesingerInfos;

            List<string> columnNames = new List<string>();

            foreach (TableColumnDesingerInfo column in columns)
            {
                if (string.IsNullOrEmpty(column.Name))
                {
                    message = "Column Name can't be empty";
                    return false;
                }
                else if (string.IsNullOrEmpty(column.DataType))
                {
                    string computeExpression = column.ExtraPropertyInfo?.Expression;

                    if (string.IsNullOrEmpty(computeExpression) || this.dbInterpreter.DatabaseType == DatabaseType.MySql)
                    {
                        message = "Data Type can't be empty";
                        return false;
                    }
                }
                else if (columnNames.Contains(column.Name))
                {
                    message = $"Column Name \"{column.Name}\" is duplicated";
                    return false;
                }
                else if (!ColumnManager.ValidateDataType(this.dbInterpreter.DatabaseType, column, out message))
                {
                    return false;
                }

                columnNames.Add(column.Name);
            }
            #endregion

            #region Indexes
            if (!schemaDesignerInfo.IgnoreTableIndex)
            {
                List<TableIndexDesignerInfo> indexes = schemaDesignerInfo.TableIndexDesingerInfos;

                List<string> indexNames = new List<string>();

                int clursteredCount = 0;

                foreach (TableIndexDesignerInfo index in indexes)
                {
                    if (string.IsNullOrEmpty(index.Name))
                    {
                        message = "Index Name can't be empty";
                        return false;
                    }
                    else if (indexNames.Contains(index.Name))
                    {
                        message = $"Index Name \"{index.Name}\" is duplicated";
                        return false;
                    }
                    else if (index.Columns == null || index.Columns.Count == 0)
                    {
                        message = $"Index \"{index.Name}\" has no any column";
                        return false;
                    }

                    if (index.ExtraPropertyInfo != null)
                    {
                        if (index.ExtraPropertyInfo.Clustered)
                        {
                            clursteredCount++;
                        }
                    }
                }

                if (clursteredCount > 1)
                {
                    message = "The clurstered index count can't be more than one";
                    return false;
                }
            }
            #endregion

            #region Foreign Keys
            if (!schemaDesignerInfo.IgnoreTableForeignKey)
            {
                List<TableForeignKeyDesignerInfo> foreignKeys = schemaDesignerInfo.TableForeignKeyDesignerInfos;

                List<string> keyNames = new List<string>();

                foreach (TableForeignKeyDesignerInfo key in foreignKeys)
                {
                    if (string.IsNullOrEmpty(key.Name))
                    {
                        message = "Foreign Key Name can't be empty";
                        return false;
                    }
                    else if (keyNames.Contains(key.Name))
                    {
                        message = $"Foreign Key Name \"{key.Name}\" is duplicated";
                        return false;
                    }
                    else if (key.Columns == null || key.Columns.Count == 0)
                    {
                        message = $"The \"{key.Name}\" has no any column";
                        return false;
                    }
                }
            }
            #endregion

            #region Constraints
            if (!schemaDesignerInfo.IgnoreTableConstraint)
            {
                List<TableConstraintDesignerInfo> constraints = schemaDesignerInfo.TableConstraintDesignerInfos;

                List<string> constraintNames = new List<string>();

                foreach (TableConstraintDesignerInfo constraint in constraints)
                {
                    if (string.IsNullOrEmpty(constraint.Name))
                    {
                        message = "Constraint Name can't be empty";
                        return false;
                    }
                    else if (constraintNames.Contains(constraint.Name))
                    {
                        message = $"Constraint Name \"{constraint.Name}\" is duplicated";
                        return false;
                    }
                    else if (string.IsNullOrEmpty(constraint.Definition))
                    {
                        message = "Constraint Expressioni can't be empty";
                        return false;
                    }
                }
            }
            #endregion

            return true;
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
