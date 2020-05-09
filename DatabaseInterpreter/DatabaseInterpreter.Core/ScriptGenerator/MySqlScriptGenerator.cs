using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class MySqlScriptGenerator : DbScriptGenerator
    {
        public MySqlScriptGenerator(DbInterpreter dbInterpreter) : base(dbInterpreter) { }

        #region Generate Schema Scripts 

        public override ScriptBuilder GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            ScriptBuilder sb = new ScriptBuilder();
            MySqlInterpreter mySqlInterpreter = this.dbInterpreter as MySqlInterpreter;
            string dbCharSet = mySqlInterpreter.DbCharset;
            string notCreateIfExistsClause = mySqlInterpreter.NotCreateIfExistsClause;

            #region Function           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<Function>(schemaInfo.Functions));
            #endregion

            #region Create Table
            foreach (Table table in schemaInfo.Tables)
            {
                this.FeedbackInfo(OperationState.Begin, table);

                string tableName = table.Name;
                string quotedTableName = this.GetQuotedObjectName(table);

                IEnumerable<TableColumn> tableColumns = schemaInfo.TableColumns.Where(item => item.TableName == tableName).OrderBy(item => item.Order);

                string primaryKey = "";

                IEnumerable<TablePrimaryKey> primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.TableName == tableName);
                IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.TableName == tableName);
                IEnumerable<TableIndex> indexes = schemaInfo.TableIndexes.Where(item => item.TableName == tableName).OrderBy(item => item.Order);

                this.RestrictColumnLength(tableColumns, primaryKeys);
                this.RestrictColumnLength(tableColumns, foreignKeys);
                this.RestrictColumnLength(tableColumns, indexes);

                #region Primary Key
                if (this.option.TableScriptsGenerateOption.GeneratePrimaryKey && primaryKeys.Count() > 0)
                {
                    primaryKey =
$@"
,PRIMARY KEY
(
{string.Join(Environment.NewLine, primaryKeys.Select(item => $"{ this.GetQuotedString(item.ColumnName)},")).TrimEnd(',')}
)";
                }
                #endregion

                List<string> foreignKeysLines = new List<string>();

                #region Foreign Key
                if (this.option.TableScriptsGenerateOption.GenerateForeignKey)
                {
                    if (foreignKeys.Count() > 0)
                    {
                        ILookup<string, TableForeignKey> foreignKeyLookup = foreignKeys.ToLookup(item => item.Name);

                        IEnumerable<string> keyNames = foreignKeyLookup.Select(item => item.Key);

                        foreach (string keyName in keyNames)
                        {
                            TableForeignKey tableForeignKey = foreignKeyLookup[keyName].First();

                            string columnNames = string.Join(",", foreignKeyLookup[keyName].Select(item => this.GetQuotedString(item.ColumnName)));
                            string referenceColumnName = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{ this.GetQuotedString(item.ReferencedColumnName)}"));

                            string line = $"CONSTRAINT { this.GetQuotedString(this.GetRestrictedLengthName(keyName))} FOREIGN KEY ({columnNames}) REFERENCES { this.GetQuotedString(tableForeignKey.ReferencedTableName)}({referenceColumnName})";

                            if (tableForeignKey.UpdateCascade)
                            {
                                line += " ON UPDATE CASCADE";
                            }
                            else
                            {
                                line += " ON UPDATE NO ACTION";
                            }

                            if (tableForeignKey.DeleteCascade)
                            {
                                line += " ON DELETE CASCADE";
                            }
                            else
                            {
                                line += " ON DELETE NO ACTION";
                            }

                            foreignKeysLines.Add(line);
                        }
                    }
                }

                string foreignKey = $"{(foreignKeysLines.Count > 0 ? (Environment.NewLine + "," + string.Join("," + Environment.NewLine, foreignKeysLines)) : "")}";

                #endregion

                #region Table

                string tableScript =
$@"
CREATE TABLE {notCreateIfExistsClause} {quotedTableName}(
{string.Join("," + Environment.NewLine, tableColumns.Select(item => this.dbInterpreter.ParseColumn(table, item)))}{primaryKey}{foreignKey}
){(!string.IsNullOrEmpty(table.Comment) ? ($"comment='{this.dbInterpreter.ReplaceSplitChar(ValueHelper.TransferSingleQuotation(table.Comment))}'") : "")}
DEFAULT CHARSET={dbCharSet}" + this.scriptsDelimiter;

                sb.AppendLine(new CreateDbObjectScript<Table>(tableScript));

                #endregion               

                #region Index
                if (this.option.TableScriptsGenerateOption.GenerateIndex)
                {
                    if (indexes.Count() > 0)
                    {
                        sb.AppendLine();

                        List<string> indexColumns = new List<string>();

                        ILookup<string, TableIndex> indexLookup = indexes.ToLookup(item => item.Name);
                        IEnumerable<string> indexNames = indexLookup.Select(item => item.Key);

                        foreach (string indexName in indexNames)
                        {
                            TableIndex tableIndex = indexLookup[indexName].First();

                            string columnNames = string.Join(",", indexLookup[indexName].Select(item => $"{this.GetQuotedString(item.ColumnName)}"));

                            if (indexColumns.Contains(columnNames))
                            {
                                continue;
                            }

                            var tempIndexName = tableIndex.Name;
                            if (tempIndexName.Contains("-"))
                            {
                                tempIndexName = tempIndexName.Replace("-", "_");
                            }

                            tempIndexName = this.GetRestrictedLengthName(tempIndexName);

                            sb.AppendLine(new CreateDbObjectScript<TableIndex>($"ALTER TABLE {quotedTableName} ADD {(tableIndex.IsUnique ? "UNIQUE" : "")} INDEX { this.GetQuotedString(tempIndexName)} ({columnNames});"));

                            if (!indexColumns.Contains(columnNames))
                            {
                                indexColumns.Add(columnNames);
                            }
                        }
                    }
                }
                #endregion              

                sb.AppendLine();

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

        private void RestrictColumnLength<T>(IEnumerable<TableColumn> columns, IEnumerable<T> children) where T : TableColumnChild
        {
            var childColumns = columns.Where(item => children.Any(t => item.Name == t.ColumnName)).ToList();

            childColumns.ForEach(item =>
            {
                if (DataTypeHelper.IsCharType(item.DataType) && item.MaxLength > MySqlInterpreter.KeyIndexColumnMaxLength)
                {
                    item.MaxLength = MySqlInterpreter.KeyIndexColumnMaxLength;
                }
            });
        }

        private string GetRestrictedLengthName(string name)
        {
            if (name.Length > MySqlInterpreter.NameMaxLength)
            {
                return name.Substring(0, MySqlInterpreter.NameMaxLength);
            }

            return name;
        }

        #endregion

        #region Data Script    

        public override Task<string> GenerateDataScriptsAsync(SchemaInfo schemaInfo)
        {
            return base.GenerateDataScriptsAsync(schemaInfo);
        }     

        protected override string GetBytesConvertHexString(object value, string dataType)
        {
            string hex = string.Concat(((byte[])value).Select(item => item.ToString("X2")));
            return $"UNHEX('{hex}')";
        }
        #endregion

        #region Alter Table
        public override Script RenameTable(Table table, string newName)
        {
            return new AlterDbObjectScript<Table>($"ALTER TABLE {this.GetQuotedString(table.Name)} RENAME {this.GetQuotedString(newName)};");
        }

        public override Script SetTableComment(Table table, bool isNew = true)
        {
            return new AlterDbObjectScript<Table>($"ALTER TABLE {this.GetQuotedString(table.Name)} COMMENT = '{this.dbInterpreter.ReplaceSplitChar(ValueHelper.TransferSingleQuotation(table.Comment))}';");
        }

        public override Script AddTableColumn(Table table, TableColumn column)
        {
            return new CreateDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(table.Name)} ADD { this.dbInterpreter.ParseColumn(table, column)};");
        }

        public override Script RenameTableColumn(Table table, TableColumn column, string newName)
        {
            return new AlterDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(table.Name)} CHANGE {this.GetQuotedString(column.Name)} {newName} {this.dbInterpreter.ParseDataType(column)};");
        }

        public override Script AlterTableColumn(Table table, TableColumn column)
        {
            return new AlterDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(table.Name)} MODIFY COLUMN {this.dbInterpreter.ParseColumn(table, column)}");
        }

        public override Script SetTableColumnComment(Table table, TableColumn column, bool isNew = true)
        {
            return new AlterDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(table.Name)} MODIFY COLUMN {this.dbInterpreter.ParseColumn(table, column)}");
        }

        public override Script DropTableColumn(TableColumn column)
        {
            return new DropDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(column.TableName)} DROP COLUMN {this.GetQuotedString(column.Name)};");
        }
        #endregion
    }
}
