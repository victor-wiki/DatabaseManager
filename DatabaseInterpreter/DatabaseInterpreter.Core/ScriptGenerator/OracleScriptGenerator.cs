using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class OracleScriptGenerator:DbScriptGenerator
    {
        public OracleScriptGenerator(DbInterpreter dbInterpreter) : base(dbInterpreter) { }

        #region Schema Script 

        public override ScriptBuilder GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            ScriptBuilder sb = new ScriptBuilder();

            string dbOwner = this.GetDbOwner();

            #region User Defined Type

            //List<string> userTypeNames = schemaInfo.UserDefinedTypes.Select(item => item.Name).Distinct().ToList();

            //foreach (string userTypeName in userTypeNames)
            //{
            //    IEnumerable<UserDefinedType> userTypes = schemaInfo.UserDefinedTypes.Where(item => item.Name == userTypeName);

            //    this.FeedbackInfo(OperationState.Begin, userTypes.First());

            //    string dataTypes = string.Join(",", userTypes.Select(item => $"{item.AttrName} {this.ParseDataType(new TableColumn() { MaxLength = item.MaxLength, DataType = item.Type, Precision = item.Precision, Scale = item.Scale })}"));

            //    string script = $"CREATE TYPE {this.GetQuotedString(userTypeName)} AS OBJECT ({dataTypes})" + this.ScriptsSplitString;

            //    sb.AppendLine(new CreateDbObjectScript<UserDefinedType>(script));

            //    this.FeedbackInfo(OperationState.End, userTypes.First());
            //}

            #endregion

            #region Function           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<Function>(schemaInfo.Functions));
            #endregion

            #region Table
            foreach (Table table in schemaInfo.Tables)
            {
                this.FeedbackInfo(OperationState.Begin, table);

                string tableName = table.Name;
                string quotedTableName = this.GetQuotedObjectName(table);

                IEnumerable<TableColumn> tableColumns = schemaInfo.TableColumns.Where(item => item.TableName == tableName).OrderBy(item => item.Order);

                IEnumerable<TablePrimaryKey> primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.TableName == tableName);

                #region Create Table

                string tableScript =
$@"
CREATE TABLE {quotedTableName}(
{string.Join("," + Environment.NewLine, tableColumns.Select(item => this.dbInterpreter.ParseColumn(table, item))).TrimEnd(',')}
)
TABLESPACE
{this.dbInterpreter.ConnectionInfo.Database}" + this.scriptsDelimiter;

                sb.AppendLine(new CreateDbObjectScript<Table>(tableScript));

                #endregion

                sb.AppendLine();

                #region Comment
                if (!string.IsNullOrEmpty(table.Comment))
                {
                    sb.AppendLine(this.SetTableComment(table));
                }

                foreach (TableColumn column in tableColumns.Where(item => !string.IsNullOrEmpty(item.Comment)))
                {
                    sb.AppendLine(this.SetTableColumnComment(table, column, true));
                }
                #endregion

                #region Primary Key
                if (this.option.TableScriptsGenerateOption.GeneratePrimaryKey && primaryKeys.Count() > 0)
                {
                    string primaryKey =
$@"
ALTER TABLE {quotedTableName} ADD CONSTRAINT {this.GetQuotedString(primaryKeys.FirstOrDefault().Name)} PRIMARY KEY 
(
{string.Join(Environment.NewLine, primaryKeys.Select(item => $"{ this.GetQuotedString(item.ColumnName)},")).TrimEnd(',')}
)
USING INDEX 
TABLESPACE
{this.dbInterpreter.ConnectionInfo.Database}{this.scriptsDelimiter}";

                    sb.AppendLine(new CreateDbObjectScript<TablePrimaryKey>(primaryKey));
                }
                #endregion

                #region Foreign Key
                if (this.option.TableScriptsGenerateOption.GeneratePrimaryKey)
                {
                    IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.TableName == tableName);
                    if (foreignKeys.Count() > 0)
                    {
                        sb.AppendLine();
                        ILookup<string, TableForeignKey> foreignKeyLookup = foreignKeys.ToLookup(item => item.Name);

                        IEnumerable<string> keyNames = foreignKeyLookup.Select(item => item.Key);

                        foreach (string keyName in keyNames)
                        {
                            TableForeignKey tableForeignKey = foreignKeyLookup[keyName].First();

                            string columnNames = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{ this.GetQuotedString(item.ColumnName)}"));
                            string referenceColumnName = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{ this.GetQuotedString(item.ReferencedColumnName)}"));

                            StringBuilder foreignKeyScript = new StringBuilder();

                            foreignKeyScript.AppendLine(
$@"
ALTER TABLE {quotedTableName} ADD CONSTRAINT { this.GetQuotedString(keyName)} FOREIGN KEY ({columnNames})
REFERENCES { this.GetQuotedString(tableForeignKey.ReferencedTableName)}({referenceColumnName})");

                            if (tableForeignKey.DeleteCascade)
                            {
                                foreignKeyScript.AppendLine("ON DELETE CASCADE");
                            }

                            foreignKeyScript.Append(this.scriptsDelimiter);

                            sb.AppendLine(new CreateDbObjectScript<TableForeignKey>(foreignKeyScript.ToString()));
                        }
                    }
                }
                #endregion

                #region Index
                if (this.option.TableScriptsGenerateOption.GenerateIndex)
                {
                    IEnumerable<TableIndex> indices = schemaInfo.TableIndexes.Where(item => item.TableName == tableName).OrderBy(item => item.Order);

                    if (indices.Count() > 0)
                    {
                        sb.AppendLine();

                        List<string> indexColumns = new List<string>();

                        ILookup<string, TableIndex> indexLookup = indices.ToLookup(item => item.Name);
                        IEnumerable<string> indexNames = indexLookup.Select(item => item.Key);
                        foreach (string indexName in indexNames)
                        {
                            TableIndex tableIndex = indexLookup[indexName].First();

                            string columnNames = string.Join(",", indexLookup[indexName].Select(item => $"{ this.GetQuotedString(item.ColumnName)}"));

                            if (indexColumns.Contains(columnNames))
                            {
                                continue;
                            }

                            sb.AppendLine(new CreateDbObjectScript<TableIndex>($"CREATE {(tableIndex.IsUnique ? "UNIQUE" : "")} INDEX { this.GetQuotedString(tableIndex.Name)} ON { this.GetQuotedString(tableName)} ({columnNames})" + this.scriptsDelimiter));

                            if (!indexColumns.Contains(columnNames))
                            {
                                indexColumns.Add(columnNames);
                            }
                        }
                    }
                }
                #endregion               

                #region Constraint
                if (this.option.TableScriptsGenerateOption.GenerateConstraint)
                {
                    var constraints = schemaInfo.TableConstraints.Where(item => item.Owner == table.Owner && item.TableName == tableName);

                    foreach (TableConstraint constraint in constraints)
                    {
                        sb.AppendLine();
                        sb.AppendLine(new CreateDbObjectScript<TableConstraint>($"ALTER TABLE {quotedTableName} ADD CONSTRAINT {this.GetQuotedString(constraint.Name)} CHECK ({constraint.Definition})" + this.scriptsDelimiter));
                    }
                }
                #endregion

                this.FeedbackInfo(OperationState.End, table);
            }
            #endregion

            #region View           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<View>(schemaInfo.Views));
            #endregion

            #region Trigger           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<TableTrigger>(schemaInfo.TableTriggers));
            #endregion

            #region Procedure           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<Procedure>(schemaInfo.Procedures));
            #endregion

            if (this.option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
            {
                this.AppendScriptsToFile(sb.ToString(), GenerateScriptMode.Schema, true);
            }

            return sb;
        }     
        
        private string GetDbOwner()
        {
            return (this.dbInterpreter as OracleInterpreter).GetDbOwner();
        }
        #endregion

        #region Data Script        

        public override async Task<string> GenerateDataScriptsAsync(SchemaInfo schemaInfo)
        {
            return await base.GenerateDataScriptsAsync(schemaInfo);
        }

        protected override string GetBatchInsertPrefix()
        {
            return "INSERT ALL INTO";
        }

        protected override string GetBatchInsertItemBefore(string tableName, bool isFirstRow)
        {
            return isFirstRow ? "" : $"INTO {tableName} VALUES";
        }

        protected override string GetBatchInsertItemEnd(bool isAllEnd)
        {
            return (isAllEnd ? $"{Environment.NewLine}SELECT 1 FROM DUAL;" : "");
        }       

        protected override bool NeedInsertParameter(TableColumn column, object value)
        {
            if (value != null)
            {
                if (column.DataType.ToLower() == "clob")
                {
                    return true;
                }
                if (value.GetType() == typeof(string))
                {
                    string str = value.ToString();
                    if (str.Length > 1000 || (str.Contains(OracleInterpreter.SEMICOLON_FUNC) && str.Length > 500))
                    {
                        return true;
                    }
                }
                else if (value.GetType().Name == nameof(TimeSpan))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Alter Table
        public override Script RenameTable(Table table, string newName)
        {
            return new AlterDbObjectScript<Table>($"RENAME TABLE {this.GetQuotedObjectName(table)} TO {this.GetQuotedString(newName)};");
        }

        public override Script SetTableComment(Table table, bool isNew = true)
        {
            return new AlterDbObjectScript<Table>($"COMMENT ON TABLE {table.Owner}.{this.GetQuotedString(table.Name)} IS '{this.dbInterpreter.ReplaceSplitChar(ValueHelper.TransferSingleQuotation(table.Comment))}'" + this.scriptsDelimiter);
        }

        public override Script AddTableColumn(Table table, TableColumn column)
        {
            return new CreateDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(table.Name)} ADD { this.dbInterpreter.ParseColumn(table, column)};");
        }

        public override Script RenameTableColumn(Table table, TableColumn column, string newName)
        {
            return new AlterDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(table.Name)} RENAME COLUMN {this.GetQuotedString(column.Name)} TO {newName};");
        }

        public override Script AlterTableColumn(Table table, TableColumn column)
        {
            return new AlterDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(table.Name)} MODIFY {this.dbInterpreter.ParseColumn(table, column)}");
        }

        public override Script SetTableColumnComment(Table table, TableColumn column, bool isNew = true)
        {
            return new AlterDbObjectScript<TableColumn>($"COMMENT ON COLUMN {column.Owner}.{this.GetQuotedString(column.TableName)}.{this.GetQuotedString(column.Name)} IS '{this.dbInterpreter.ReplaceSplitChar(ValueHelper.TransferSingleQuotation(column.Comment))}'" + this.scriptsDelimiter);
        }

        public override Script DropTableColumn(TableColumn column)
        {
            return new DropDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(column.TableName)} DROP COLUMN {this.GetQuotedString(column.Name)};");
        }
        #endregion
    }
}
