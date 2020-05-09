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
            string validateMsg;

            Table table = null;

            try
            {
                bool isValid = this.ValidateModel(schemaDesignerInfo, out validateMsg);

                if (!isValid)
                {
                    return this.GetFaultSaveResult(validateMsg);
                }

                SchemaInfo schemaInfo = this.GetSchemaInfo(schemaDesignerInfo);

                table = schemaInfo.Tables.First();

                DbScriptGenerator scriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.dbInterpreter);

                using (DbConnection dbConnection = this.dbInterpreter.CreateConnection())
                {
                    dbConnection.Open();

                    DbTransaction transaction = dbConnection.BeginTransaction();
                    List<Script> scripts;

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
                        filter.DatabaseObjectType = DatabaseObjectType.Table | DatabaseObjectType.TableColumn;

                        SchemaInfo oldSchemaInfo = await dbInterpreter.GetSchemaInfoAsync(filter);
                        Table oldTable = oldSchemaInfo.Tables.FirstOrDefault();

                        if (oldTable == null)
                        {
                            return this.GetFaultSaveResult($"Table \"{tableDesignerInfo.OldName}\" is not existed");
                        }

                        if (tableDesignerInfo.OldName != tableDesignerInfo.Name)
                        {
                            scripts.Add(scriptGenerator.RenameTable(new Table() { Owner = tableDesignerInfo.Owner, Name = tableDesignerInfo.OldName }, tableDesignerInfo.Name));
                        }

                        if (tableDesignerInfo.Comment != oldTable.Comment)
                        {
                            oldTable.Comment = tableDesignerInfo.Comment;
                            scripts.Add(scriptGenerator.SetTableComment(oldTable, string.IsNullOrEmpty(oldTable.Comment)));
                        }

                        List<TableColumnDesingerInfo> columnDesingerInfos = schemaDesignerInfo.TableColumnDesingerInfos;
                        List<TableColumn> oldColumns = oldSchemaInfo.TableColumns;

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
                                if (columnDesignerInfo.OldName != columnDesignerInfo.Name)
                                {
                                    scripts.Add(scriptGenerator.RenameTableColumn(table, oldColumn, newColumn.Name));
                                }

                                if (this.dbInterpreter.ParseColumn(oldTable, oldColumn) != this.dbInterpreter.ParseColumn(oldTable, newColumn))
                                {
                                    scripts.Add(scriptGenerator.AlterTableColumn(table, newColumn));
                                }
                                else if (columnDesignerInfo.Comment != oldColumn.Comment)
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
                    }

                    if (scripts == null || scripts.Count == 0)
                    {
                        return this.GetFaultSaveResult("The script to create schema is empty.");
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

            foreach (TableColumnDesingerInfo column in schemaDesignerInfo.TableColumnDesingerInfos)
            {
                TableColumn tableColumn = new TableColumn();
                ObjectHelper.CopyProperties(column, tableColumn);

                if (!string.IsNullOrEmpty(column.Length))
                {
                    ColumnManager.SetColumnLength(this.dbInterpreter.DatabaseType, tableColumn, column.Length);
                }

                if (column.IsPrimary)
                {
                    TablePrimaryKey primaryKey = new TablePrimaryKey() { Owner = table.Owner, TableName = table.Name, Name = $"PK_{table.Name}", ColumnName = column.Name };

                    schemaInfo.TablePrimaryKeys.Add(primaryKey);
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
