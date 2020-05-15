using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class OracleScriptGenerator : DbScriptGenerator
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
                    sb.AppendLine(this.AddPrimaryKey(primaryKeys.First()));
                }
                #endregion

                #region Foreign Key
                if (this.option.TableScriptsGenerateOption.GeneratePrimaryKey)
                {
                    IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.TableName == tableName);

                    foreach (TableForeignKey foreignKey in foreignKeys)
                    {
                        sb.AppendLine(this.AddForeignKey(foreignKey));
                    }
                }
                #endregion

                #region Index
                if (this.option.TableScriptsGenerateOption.GenerateIndex)
                {
                    IEnumerable<TableIndex> indexes = schemaInfo.TableIndexes.Where(item => item.TableName == tableName).OrderBy(item => item.Order);

                    List<string> indexColumns = new List<string>();
                   
                    foreach (TableIndex index in indexes)
                    {
                        string columnNames = string.Join(",", index.Columns.OrderBy(item=>item.ColumnName).Select(item=>item.ColumnName));

                        //Avoid duplicated indexes for one index.
                        if (indexColumns.Contains(columnNames))
                        {
                            continue;
                        }

                        sb.AppendLine(this.AddIndex(index));

                        if (!indexColumns.Contains(columnNames))
                        {
                            indexColumns.Add(columnNames);
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
                        sb.AppendLine(this.AddCheckConstraint(constraint));
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
            return new AlterDbObjectScript<Table>($"COMMENT ON TABLE {table.Owner}.{this.GetQuotedString(table.Name)} IS '{this.dbInterpreter.ReplaceSplitChar(this.TransferSingleQuotationString(table.Comment))}'" + this.scriptsDelimiter);
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
            return new AlterDbObjectScript<TableColumn>($"COMMENT ON COLUMN {column.Owner}.{this.GetQuotedString(column.TableName)}.{this.GetQuotedString(column.Name)} IS '{this.dbInterpreter.ReplaceSplitChar(this.TransferSingleQuotationString(column.Comment))}'" + this.scriptsDelimiter);
        }

        public override Script DropTableColumn(TableColumn column)
        {
            return new DropDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(column.TableName)} DROP COLUMN {this.GetQuotedString(column.Name)};");
        }

        public override Script AddPrimaryKey(TablePrimaryKey primaryKey)
        {
            string sql =
$@"
ALTER TABLE {this.GetQuotedFullTableName(primaryKey)} ADD CONSTRAINT {this.GetQuotedString(primaryKey.Name)} PRIMARY KEY 
(
{string.Join(Environment.NewLine, primaryKey.Columns.Select(item => $"{ this.GetQuotedString(item.ColumnName)},")).TrimEnd(',')}
)
USING INDEX 
TABLESPACE
{this.dbInterpreter.ConnectionInfo.Database}{this.scriptsDelimiter}";

            return new Script(sql);
        }

        public override Script DropPrimaryKey(TablePrimaryKey primaryKey)
        {
            return new DropDbObjectScript<TablePrimaryKey>(this.GetDropConstraintSql(primaryKey));
        }

        public override Script AddForeignKey(TableForeignKey foreignKey)
        {
            string columnNames = string.Join(",", foreignKey.Columns.Select(item => $"{ this.GetQuotedString(item.ColumnName)}"));
            string referenceColumnName = string.Join(",", foreignKey.Columns.Select(item => $"{ this.GetQuotedString(item.ReferencedColumnName)}"));

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(
$@"
ALTER TABLE {this.GetQuotedFullTableName(foreignKey)} ADD CONSTRAINT { this.GetQuotedString(foreignKey.Name)} FOREIGN KEY ({columnNames})
REFERENCES { this.GetQuotedString(foreignKey.ReferencedTableName)}({referenceColumnName})");

            if (foreignKey.DeleteCascade)
            {
                sb.AppendLine("ON DELETE CASCADE");
            }

            sb.Append(this.scriptsDelimiter);

            return new CreateDbObjectScript<TableForeignKey>(sb.ToString());
        }

        public override Script DropForeignKey(TableForeignKey foreignKey)
        {
            return new DropDbObjectScript<TableForeignKey>(this.GetDropConstraintSql(foreignKey));
        }

        private string GetDropConstraintSql(TableChild tableChild)
        {
            return $"ALTER TABLE {this.GetQuotedFullTableName(tableChild)} DROP CONSTRAINT {this.GetQuotedString(tableChild.Name)};";
        }

        public override Script AddIndex(TableIndex index)
        {
            string columnNames = string.Join(",", index.Columns.Select(item => $"{ this.GetQuotedString(item.ColumnName)}"));

            string type = "";

            if (index.Type == IndexType.Unique.ToString())
            {
                type = "UNIQUE";
            }
            else if (index.Type == IndexType.Bitmap.ToString())
            {
                type = "BITMAP";
            }

            string reverse = index.Type == IndexType.Reverse.ToString() ? "REVERSE" : "";

            return new CreateDbObjectScript<TableIndex>($"CREATE {type} INDEX { this.GetQuotedString(index.Name)} ON { this.GetQuotedFullTableName(index)} ({columnNames}){reverse};");
        }

        public override Script DropIndex(TableIndex index)
        {
            return new DropDbObjectScript<TableIndex>($"DROP INDEX {this.GetQuotedString(index.Name)};");
        }

        public override Script AddCheckConstraint(TableConstraint constraint)
        {
            return new CreateDbObjectScript<TableConstraint>($"ALTER TABLE {this.GetQuotedFullTableName(constraint)} ADD CONSTRAINT {this.GetQuotedString(constraint.Name)} CHECK ({constraint.Definition});");
        }

        public override Script DropCheckConstraint(TableConstraint constraint)
        {
            return new DropDbObjectScript<TableConstraint>(this.GetDropConstraintSql(constraint));
        }
        #endregion

        #region Database Operation

        public override Script DropUserDefinedType(UserDefinedType userDefinedType)
        {
            return new Script("");
        }

        public override Script DropTable(Table table)
        {
            return new DropDbObjectScript<Table>(this.GetDropSql(nameof(Table), table));
        }

        public override Script DropView(View view)
        {
            return new DropDbObjectScript<View>(this.GetDropSql(nameof(View), view));
        }

        public override Script DropTrigger(TableTrigger trigger)
        {
            return new DropDbObjectScript<View>(this.GetDropSql("trigger", trigger));
        }

        public override Script DropFunction(Function function)
        {
            return new DropDbObjectScript<Function>(this.GetDropSql(nameof(Function), function));
        }

        public override Script DropProcedure(Procedure procedure)
        {
            return new DropDbObjectScript<Procedure>(this.GetDropSql(nameof(Procedure), procedure));
        }

        private string GetDropSql(string typeName, DatabaseObject dbObject)
        {
            return $"DROP {typeName.ToUpper()} {this.GetQuotedObjectName(dbObject)};";
        }
        #endregion
    }
}
