using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace DatabaseManager.Core
{
    public class TableManager
    {
        private IObserver<FeedbackInfo> observer;
        private DbInterpreter dbInterpreter;
        private DbScriptGenerator scriptGenerator;

        public TableManager(DbInterpreter dbInterpreter)
        {
            this.dbInterpreter = dbInterpreter;

            this.scriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.dbInterpreter);
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
                    return this.GetFaultSaveResult("No changes need to save.");
                }

                ScriptRunner scriptRunner = new ScriptRunner();

                await scriptRunner.Run(this.dbInterpreter, scripts);
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

                List<Script> scripts = new List<Script>();

                if (isNew)
                {
                    ScriptBuilder scriptBuilder = this.scriptGenerator.GenerateSchemaScripts(schemaInfo);

                    scripts.AddRange(scriptBuilder.Scripts);
                }
                else
                {
                    #region Alter Table                   

                    TableDesignerInfo tableDesignerInfo = schemaDesignerInfo.TableDesignerInfo;

                    SchemaInfoFilter filter = new SchemaInfoFilter() { Strict = true };
                    filter.Schema = tableDesignerInfo.Schema;
                    filter.TableNames = new string[] { tableDesignerInfo.OldName };
                    filter.DatabaseObjectType = DatabaseObjectType.Table
                        | DatabaseObjectType.Column
                        | DatabaseObjectType.PrimaryKey
                        | DatabaseObjectType.ForeignKey
                        | DatabaseObjectType.Index
                        | DatabaseObjectType.Constraint;

                    this.dbInterpreter.Option.IncludePrimaryKeyWhenGetTableIndex = true;

                    SchemaInfo oldSchemaInfo = await this.dbInterpreter.GetSchemaInfoAsync(filter);
                    Table oldTable = oldSchemaInfo.Tables.FirstOrDefault();

                    if (oldTable == null)
                    {
                        return this.GetFaultSaveResult($"Table \"{tableDesignerInfo.OldName}\" is not existed");
                    }

                    if (tableDesignerInfo.OldName != tableDesignerInfo.Name)
                    {
                        scripts.Add(this.scriptGenerator.RenameTable(new Table() { Schema = tableDesignerInfo.Schema, Name = tableDesignerInfo.OldName }, tableDesignerInfo.Name));
                    }

                    if (!this.IsStringEquals(tableDesignerInfo.Comment, oldTable.Comment))
                    {
                        oldTable.Comment = tableDesignerInfo.Comment;
                        scripts.Add(this.scriptGenerator.SetTableComment(oldTable, string.IsNullOrEmpty(oldTable.Comment)));
                    }

                    #region Columns
                    List<TableColumnDesingerInfo> columnDesingerInfos = schemaDesignerInfo.TableColumnDesingerInfos;
                    List<TableColumn> oldColumns = oldSchemaInfo.TableColumns;

                    List<TableDefaultValueConstraint> defaultValueConstraints = await this.GetTableDefaultConstraints(filter);

                    List<string> renamedColNames = new List<string>();

                    foreach (TableColumnDesingerInfo columnDesignerInfo in columnDesingerInfos)
                    {
                        string oldName = columnDesignerInfo.OldName;

                        TableColumn oldColumn = oldColumns.FirstOrDefault(item => item.Name == oldName);
                        TableColumn newColumn = schemaInfo.TableColumns.FirstOrDefault(item => item.Name == columnDesignerInfo.Name);

                        if (oldColumn == null)
                        {
                            scripts.Add(this.scriptGenerator.AddTableColumn(table, newColumn));
                        }
                        else
                        {
                            if (this.IsNameChanged(columnDesignerInfo.OldName, columnDesignerInfo.Name))
                            {
                                scripts.Add(this.GetColumnRenameScript(table, oldColumn, newColumn));

                                renamedColNames.Add(oldColumn.Name);
                            }

                            scripts.AddRange(this.GetColumnAlterScripts(oldTable, table, oldColumn, newColumn, defaultValueConstraints));
                        }
                    }

                    foreach (TableColumn oldColumn in oldColumns)
                    {
                        if (!renamedColNames.Contains(oldColumn.Name) && !columnDesingerInfos.Any(item => item.Name == oldColumn.Name))
                        {
                            scripts.Add(this.scriptGenerator.DropTableColumn(oldColumn));
                        }
                    }
                    #endregion

                    #region Primary Key
                    TablePrimaryKey oldPrimaryKey = oldSchemaInfo.TablePrimaryKeys.FirstOrDefault();
                    TablePrimaryKey newPrimaryKey = schemaInfo.TablePrimaryKeys.FirstOrDefault();

                    scripts.AddRange(this.GetPrimaryKeyAlterScripts(oldPrimaryKey, newPrimaryKey, schemaDesignerInfo.IgnoreTableIndex));
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
                                && (this.IsValueEqualsIgnoreCase(indexDesignerInfo.OldType, indexDesignerInfo.Type) || (indexDesignerInfo.Type == nameof(IndexType.Unique) && oldIndex.IsUnique))
                              )
                            {
                                if (oldIndex != null && this.IsStringEquals(oldIndex.Comment, newIndex.Comment) && SchemaInfoHelper.IsIndexColumnsEquals(oldIndex.Columns, newIndex.Columns))
                                {
                                    continue;
                                }
                            }

                            scripts.AddRange(this.GetIndexAlterScripts(oldIndex, newIndex));
                        }

                        foreach (TableIndex oldIndex in oldIndexes)
                        {
                            if (!indexDesignerInfos.Any(item => item.Name == oldIndex.Name))
                            {
                                scripts.Add(this.scriptGenerator.DropIndex(oldIndex));
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
                                    && oldForeignKey.ReferencedSchema == newForeignKey.ReferencedSchema && oldForeignKey.ReferencedTableName == newForeignKey.ReferencedTableName
                                    && SchemaInfoHelper.IsForeignKeyColumnsEquals(oldForeignKey.Columns, newForeignKey.Columns))
                                {
                                    continue;
                                }
                            }

                            scripts.AddRange(this.GetForeignKeyAlterScripts(oldForeignKey, newForeignKey));
                        }

                        foreach (TableForeignKey oldForeignKey in oldForeignKeys)
                        {
                            if (!foreignKeyDesignerInfos.Any(item => item.Name == oldForeignKey.Name))
                            {
                                scripts.Add(this.scriptGenerator.DropForeignKey(oldForeignKey));
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
                                if (oldConstraint != null && this.IsStringEquals(oldConstraint.Comment, newConstraint.Comment)
                                    && this.IsStringEquals(oldConstraint.Definition, newConstraint.Definition))
                                {
                                    continue;
                                }
                            }

                            scripts.AddRange(this.GetConstraintAlterScripts(oldConstraint, newConstraint));
                        }

                        foreach (TableConstraint oldConstraint in oldConstraints)
                        {
                            if (!constraintDesignerInfos.Any(item => item.Name == oldConstraint.Name))
                            {
                                scripts.Add(this.scriptGenerator.DropCheckConstraint(oldConstraint));
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

        public Script GetColumnRenameScript(Table table, TableColumn oldColumn, TableColumn newColumn)
        {
            return this.scriptGenerator.RenameTableColumn(table, oldColumn, newColumn.Name);
        }

        public async Task<List<TableDefaultValueConstraint>> GetTableDefaultConstraints(SchemaInfoFilter filter)
        {
            List<TableDefaultValueConstraint> defaultValueConstraints = null;

            if (this.dbInterpreter.DatabaseType == DatabaseType.SqlServer)
            {
                defaultValueConstraints = await (this.dbInterpreter as SqlServerInterpreter).GetTableDefautValueConstraintsAsync(filter);
            }

            return defaultValueConstraints;
        }

        public List<Script> GetColumnAlterScripts(Table oldTable, Table newTable, TableColumn oldColumn, TableColumn newColumn, List<TableDefaultValueConstraint> defaultValueConstraints)
        {
            List<Script> scripts = new List<Script>();

            DatabaseType databaseType = this.dbInterpreter.DatabaseType;

            bool isDefaultValueEquals = ValueHelper.IsStringEquals(StringHelper.GetBalanceParenthesisTrimedValue(oldColumn.DefaultValue), newColumn.DefaultValue);

            if (!SchemaInfoHelper.IsTableColumnEquals(databaseType, oldColumn, newColumn)
                || !isDefaultValueEquals)
            {
                if (!isDefaultValueEquals)
                {
                    if (databaseType == DatabaseType.SqlServer)
                    {
                        SqlServerScriptGenerator sqlServerScriptGenerator = scriptGenerator as SqlServerScriptGenerator;

                        TableDefaultValueConstraint defaultValueConstraint = defaultValueConstraints?.FirstOrDefault(item => item.Schema == oldTable.Schema && item.TableName == oldTable.Name && item.ColumnName == oldColumn.Name);

                        if (defaultValueConstraint != null)
                        {
                            scripts.Add(sqlServerScriptGenerator.DropDefaultValueConstraint(defaultValueConstraint));
                        }

                        if (newColumn.DefaultValue != null)
                        {
                            scripts.Add(sqlServerScriptGenerator.AddDefaultValueConstraint(newColumn));
                        }
                    }
                }

                string oldColumnDefinition = this.dbInterpreter.ParseColumn(newTable, oldColumn);
                string newColumnDefinition = this.dbInterpreter.ParseColumn(newTable, newColumn);

                if (!this.IsDefinitionEquals(oldColumnDefinition, newColumnDefinition))
                {
                    Script alterColumnScript = scriptGenerator.AlterTableColumn(newTable, newColumn, oldColumn);

                    if (databaseType == DatabaseType.Oracle)
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
            }
            else if (!ValueHelper.IsStringEquals(newColumn.Comment, oldColumn.Comment))
            {
                scripts.Add(scriptGenerator.SetTableColumnComment(newTable, newColumn, string.IsNullOrEmpty(oldColumn.Comment)));
            }

            return scripts;
        }

        private bool IsDefinitionEquals(string definiton1, string defintion2)
        {
            return this.GetNoWhiteSpaceTrimedString(definiton1.ToUpper()) == this.GetNoWhiteSpaceTrimedString(defintion2.ToUpper());
        }

        private string GetNoWhiteSpaceTrimedString(string value)
        {
            return value.Replace(" ", "").Trim();
        }

        public List<Script> GetPrimaryKeyAlterScripts(TablePrimaryKey oldPrimaryKey, TablePrimaryKey newPrimaryKey, bool onlyCompareColumns)
        {
            List<Script> scripts = new List<Script>();

            bool primaryKeyChanged = !SchemaInfoHelper.IsPrimaryKeyEquals(oldPrimaryKey, newPrimaryKey, onlyCompareColumns, true);

            Action alterPrimaryKey = () =>
            {
                if (oldPrimaryKey != null)
                {
                    scripts.Add(this.scriptGenerator.DropPrimaryKey(oldPrimaryKey));
                }

                if (newPrimaryKey != null)
                {
                    scripts.Add(this.scriptGenerator.AddPrimaryKey(newPrimaryKey));

                    if (!string.IsNullOrEmpty(newPrimaryKey.Comment))
                    {
                        this.SetTableChildComment(scripts, this.scriptGenerator, newPrimaryKey, true);
                    }
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
                    this.SetTableChildComment(scripts, this.scriptGenerator, newPrimaryKey, string.IsNullOrEmpty(oldPrimaryKey?.Comment));
                }
                else
                {
                    alterPrimaryKey();
                }
            }

            return scripts;
        }

        public List<Script> GetForeignKeyAlterScripts(TableForeignKey oldForeignKey, TableForeignKey newForeignKey)
        {
            List<Script> scripts = new List<Script>();

            if (oldForeignKey != null)
            {
                scripts.Add(this.scriptGenerator.DropForeignKey(oldForeignKey));
            }

            scripts.Add(this.scriptGenerator.AddForeignKey(newForeignKey));

            if (!string.IsNullOrEmpty(newForeignKey.Comment))
            {
                this.SetTableChildComment(scripts, this.scriptGenerator, newForeignKey, true);
            }

            return scripts;
        }

        public List<Script> GetIndexAlterScripts(TableIndex oldIndex, TableIndex newIndex)
        {
            List<Script> scripts = new List<Script>();

            if (oldIndex != null)
            {
                scripts.Add(this.scriptGenerator.DropIndex(oldIndex));
            }

            scripts.Add(this.scriptGenerator.AddIndex(newIndex));

            if (!string.IsNullOrEmpty(newIndex.Comment))
            {
                this.SetTableChildComment(scripts, this.scriptGenerator, newIndex, true);
            }

            return scripts;
        }

        public List<Script> GetConstraintAlterScripts(TableConstraint oldConstraint, TableConstraint newConstraint)
        {
            List<Script> scripts = new List<Script>();

            if (oldConstraint != null)
            {
                scripts.Add(this.scriptGenerator.DropCheckConstraint(oldConstraint));
            }

            scripts.Add(this.scriptGenerator.AddCheckConstraint(newConstraint));

            if (!string.IsNullOrEmpty(newConstraint.Comment))
            {
                this.SetTableChildComment(scripts, this.scriptGenerator, newConstraint, true);
            }

            return scripts;
        }

        public bool IsValueEqualsIgnoreCase(string value1, string value2)
        {
            return !string.IsNullOrEmpty(value1) && !string.IsNullOrEmpty(value2) && value1.ToLower() == value2.ToLower();
        }

        public bool IsNameChanged(string name1, string name2)
        {
            if (SettingManager.Setting.DbObjectNameMode == DbObjectNameMode.WithoutQuotation)
            {
                if (this.IsValueEqualsIgnoreCase(name1, name2))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                DatabaseType databaseType = this.dbInterpreter.DatabaseType;

                if (name1 == name2)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
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

                if (!DataTypeHelper.IsUserDefinedType(tableColumn))
                {
                    ColumnManager.SetColumnLength(this.dbInterpreter.DatabaseType, tableColumn, column.Length);
                }

                if (column.IsPrimary)
                {
                    if (primaryKey == null)
                    {
                        primaryKey = new TablePrimaryKey() { Schema = table.Schema, TableName = table.Name, Name = IndexManager.GetPrimaryKeyDefaultName(table) };
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
                        TableIndex index = new TableIndex() { Schema = indexDesignerInfo.Schema, TableName = indexDesignerInfo.TableName };
                        index.Name = indexDesignerInfo.Name;

                        index.IsUnique = indexDesignerInfo.Type == IndexType.Unique.ToString();
                        index.Clustered = indexDesignerInfo.Clustered;
                        index.Comment = indexDesignerInfo.Comment;
                        index.Type = indexDesignerInfo.Type;

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
                    TableForeignKey foreignKey = new TableForeignKey() { Schema = keyDesignerInfo.Schema, TableName = keyDesignerInfo.TableName };
                    foreignKey.Name = keyDesignerInfo.Name;

                    foreignKey.ReferencedSchema = keyDesignerInfo.ReferencedSchema;
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
                    TableConstraint constraint = new TableConstraint() { Schema = constraintDesignerInfo.Schema, TableName = constraintDesignerInfo.TableName };
                    constraint.Name = constraintDesignerInfo.Name;
                    constraint.ColumnName = constraintDesignerInfo.ColumnName;
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

                if(!string.IsNullOrEmpty(column.Name) && !columnNames.Contains(column.Name))
                {
                    columnNames.Add(column.Name);
                }               
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

                    if(!string.IsNullOrEmpty(index.Name) && !indexNames.Contains(index.Name))
                    {
                        indexNames.Add(index.Name);
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

                    if (!string.IsNullOrEmpty(key.Name) && !keyNames.Contains(key.Name))
                    {
                        keyNames.Add(key.Name);
                    }
                }
            }
            #endregion

            #region Constraints
            if (!schemaDesignerInfo.IgnoreTableConstraint)
            {
                List<TableConstraintDesignerInfo> constraints = schemaDesignerInfo.TableConstraintDesignerInfos;

                List<string> constraintNames = new List<string>();
                List<string> constraintColumnNames = new List<string>();

                foreach (TableConstraintDesignerInfo constraint in constraints)
                {
                    if (string.IsNullOrEmpty(constraint.Name) && this.dbInterpreter.DatabaseType != DatabaseType.Sqlite)
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
                        message = "Constraint Expression can't be empty";
                        return false;
                    }
                    else if (string.IsNullOrEmpty(constraint.ColumnName) && this.dbInterpreter.DatabaseType == DatabaseType.Sqlite)
                    {
                        message = "Column Name can't be empty";
                        return false;
                    }
                    else if (!string.IsNullOrEmpty(constraint.ColumnName) && constraintColumnNames.Contains(constraint.ColumnName))
                    {
                        message = $"Column Name \"{constraint.ColumnName}\" is duplicated";
                        return false;
                    }

                    if (!string.IsNullOrEmpty(constraint.Name) && !constraintNames.Contains(constraint.Name))
                    {
                        constraintNames.Add(constraint.Name);
                    }

                    if (!string.IsNullOrEmpty(constraint.ColumnName) && !constraintColumnNames.Contains(constraint.ColumnName))
                    {
                        constraintColumnNames.Add(constraint.ColumnName);
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
