using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseInterpreter.Core
{
    public class SqliteScriptGenerator : DbScriptGenerator
    {
        public SqliteScriptGenerator(DbInterpreter dbInterpreter) : base(dbInterpreter) { }

        #region Schema Script   

        public override ScriptBuilder GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            ScriptBuilder sb = new ScriptBuilder();

            #region Table
            foreach (Table table in schemaInfo.Tables)
            {
                this.FeedbackInfo(OperationState.Begin, table);

                IEnumerable<TableColumn> columns = schemaInfo.TableColumns.Where(item => item.Schema == table.Schema && item.TableName == table.Name).OrderBy(item => item.Order);
                TablePrimaryKey primaryKey = schemaInfo.TablePrimaryKeys.FirstOrDefault(item => item.Schema == table.Schema && item.TableName == table.Name);
                IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.Schema == table.Schema && item.TableName == table.Name);
                IEnumerable<TableIndex> indexes = schemaInfo.TableIndexes.Where(item => item.Schema == table.Schema && item.TableName == table.Name).OrderBy(item => item.Order);
                IEnumerable<TableConstraint> constraints = schemaInfo.TableConstraints.Where(item => item.Schema == table.Schema && item.TableName == table.Name);

                ScriptBuilder sbTable = this.CreateTable(table, columns, primaryKey, foreignKeys, indexes, constraints);

                sb.AppendRange(sbTable.Scripts);

                this.FeedbackInfo(OperationState.End, table);
            }

            #endregion

            #region View           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<View>(schemaInfo.Views));
            #endregion

            #region Trigger           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<TableTrigger>(schemaInfo.TableTriggers));
            #endregion

            if (this.option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
            {
                this.AppendScriptsToFile(sb.ToString().Trim(), GenerateScriptMode.Schema, true);
            }

            return sb;
        }

        #endregion

        #region Alter Table
        public override Script DropCheckConstraint(TableConstraint constraint)
        {
            return new Script("");
        }

        public override Script DropForeignKey(TableForeignKey foreignKey)
        {
            return new Script("");
        }

        public override Script DropIndex(TableIndex index)
        {
            return new DropDbObjectScript<TableIndex>($"DROP INDEX IF EXISTS {this.GetQuotedString(index.Name)};");
        }

        public override Script DropPrimaryKey(TablePrimaryKey primaryKey)
        {
            return new Script("");
        }

        public override Script DropTableColumn(TableColumn column)
        {
            return new DropDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(column.TableName)} DROP COLUMN {this.GetQuotedString(column.Name)};");
        }

        public override Script DropTrigger(TableTrigger trigger)
        {
            return new DropDbObjectScript<View>(this.GetDropSql(nameof(DatabaseObjectType.Trigger), trigger));
        }

        private string GetDropSql(string typeName, DatabaseObject dbObject)
        {
            return $"DROP {typeName.ToUpper()} IF EXISTS {this.GetQuotedDbObjectNameWithSchema(dbObject)};";
        }

        public override Script RenameTable(Table table, string newName)
        {
            return new AlterDbObjectScript<Table>($"ALTER TABLE {this.GetQuotedString(table.Name)} RENAME TO {this.GetQuotedString(newName)};");
        }

        public override Script RenameTableColumn(Table table, TableColumn column, string newName)
        {
            return new AlterDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(table.Name)} CHANGE {this.GetQuotedString(column.Name)} {newName} {this.dbInterpreter.ParseDataType(column)};");
        }

        public override IEnumerable<Script> SetConstrainsEnabled(bool enabled)
        {
            return Enumerable.Empty<Script>();
        }

        public override Script SetIdentityEnabled(TableColumn column, bool enabled)
        {
            return new Script("");
        }

        public override Script SetTableColumnComment(Table table, TableColumn column, bool isNew = true)
        {
            return new Script("");
        }

        public override Script SetTableComment(Table table, bool isNew = true)
        {
            return new Script("");
        }

        public override Script AddCheckConstraint(TableConstraint constraint)
        {
            return new Script("");
        }

        public override Script AddForeignKey(TableForeignKey foreignKey)
        {
            return new Script("");
        }

        public override Script AddIndex(TableIndex index)
        {
            string columnNames = string.Join(",", index.Columns.Select(item => $"{this.GetQuotedString(item.ColumnName)}"));

            string type = "";

            if (index.Type == IndexType.Unique.ToString())
            {
                type = "UNIQUE";
            }

            string sql = $"CREATE {type} INDEX {this.GetQuotedString(index.Name)} ON {this.GetQuotedString(index.TableName)}({columnNames})";

            return new CreateDbObjectScript<TableIndex>(sql + this.scriptsDelimiter);
        }

        public override Script AddPrimaryKey(TablePrimaryKey primaryKey)
        {
            return new Script("");
        }

        public override Script AddTableColumn(Table table, TableColumn column)
        {
            return new CreateDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(table.Name)} ADD {this.dbInterpreter.ParseColumn(table, column)};");
        }

        public override Script AlterTableColumn(Table table, TableColumn newColumn, TableColumn oldColumn)
        {
            return new Script("");
        }
        #endregion

        #region Database Operation
        public override ScriptBuilder CreateTable(Table table, IEnumerable<TableColumn> columns, TablePrimaryKey primaryKey, IEnumerable<TableForeignKey> foreignKeys, IEnumerable<TableIndex> indexes, IEnumerable<TableConstraint> constraints)
        {
            ScriptBuilder sb = new ScriptBuilder();

            string tableName = table.Name;
            string quotedTableName = this.GetQuotedFullTableName(table);

            string option = this.GetCreateTableOption();

            #region Create Table

            string existsClause = this.dbInterpreter.NotCreateIfExists ? " IF NOT EXISTS " : "";

            #region Primary Key
            string primaryKeyConstraint = "";

            if (this.option.TableScriptsGenerateOption.GeneratePrimaryKey && primaryKey != null)
            {
                string primaryKeyName = primaryKey.Name ?? "";

                primaryKeyConstraint =
$@"
,CONSTRAINT {primaryKeyName} PRIMARY KEY
 (
  {string.Join(Environment.NewLine, primaryKey.Columns.Select(item => $"{this.GetQuotedString(item.ColumnName)},")).TrimEnd(',')}
 )";
            }
            #endregion

            #region Foreign Key

            StringBuilder foreginKeyConstraint = new StringBuilder();

            if (this.option.TableScriptsGenerateOption.GenerateForeignKey && foreignKeys != null)
            {
                foreach (TableForeignKey foreignKey in foreignKeys)
                {
                    string columnNames = string.Join(",", foreignKey.Columns.Select(item => this.GetQuotedString(item.ColumnName)));
                    string referenceColumnName = string.Join(",", foreignKey.Columns.Select(item => $"{this.GetQuotedString(item.ReferencedColumnName)}"));

                    string foreignKeyName = this.GetQuotedString(foreignKey.Name) ?? "";

                    StringBuilder sbForeignKeyItem = new StringBuilder();

                    string fkConstraint = "";

                    if (!string.IsNullOrEmpty(foreignKeyName))
                    {
                        fkConstraint = $"CONSTRAINT {foreignKeyName} ";
                    }

                    sbForeignKeyItem.Append($",{fkConstraint}FOREIGN KEY ({columnNames}) REFERENCES {this.GetQuotedString(foreignKey.ReferencedTableName)}({referenceColumnName})");

                    if (foreignKey.UpdateCascade)
                    {
                        sbForeignKeyItem.Append(" ON UPDATE CASCADE");
                    }
                    else
                    {
                        sbForeignKeyItem.Append(" ON UPDATE NO ACTION");
                    }

                    if (foreignKey.DeleteCascade)
                    {
                        sbForeignKeyItem.Append(" ON DELETE CASCADE");
                    }
                    else
                    {
                        sbForeignKeyItem.Append(" ON DELETE NO ACTION");
                    }

                    foreginKeyConstraint.AppendLine(sbForeignKeyItem.ToString());
                }
            }
            #endregion

            #region Index
            bool useColumnIndex = false;

            if (this.option.TableScriptsGenerateOption.GenerateIndex && indexes != null)
            {
                if (indexes.All(item => item.Columns.Count == 1 && item.IsUnique))
                {
                    useColumnIndex = true;
                }
            }
            #endregion

            #region Constraint
            bool useColumnCheckConstraint = false;

            if (this.option.TableScriptsGenerateOption.GenerateConstraint && constraints != null)
            {
                useColumnCheckConstraint = true;
            }
            #endregion  

            List<string> columnItems = new List<string>();

            foreach (var column in columns)
            {
                string parsedColumn = this.dbInterpreter.ParseColumn(table, column).Trim(';');

                if (useColumnIndex)
                {
                    TableIndex index = indexes.FirstOrDefault(item => item.IsUnique && item.Columns.Any(t => t.ColumnName == column.Name));

                    if (index != null)
                    {
                        parsedColumn += $"{Environment.NewLine}CONSTRAINT {(this.GetQuotedString(index.Name) ?? "")} UNIQUE";
                    }
                }

                if (useColumnCheckConstraint)
                {
                    TableConstraint checkConstraint = constraints.FirstOrDefault(item => item.ColumnName == column.Name);

                    if (checkConstraint != null)
                    {
                        parsedColumn += $"{Environment.NewLine}CONSTRAINT {(this.GetQuotedString(checkConstraint.Name) ?? "")} CHECK ({checkConstraint.Definition})";
                    }
                }

                columnItems.Add(parsedColumn);
            }

            string tableScript =
$@"CREATE TABLE{existsClause} {quotedTableName}(
{string.Join("," + Environment.NewLine, columnItems)}{primaryKeyConstraint}{(foreginKeyConstraint.Length > 0 ? Environment.NewLine : "")}{foreginKeyConstraint.ToString().Trim()}
){option};";

            sb.AppendLine(new CreateDbObjectScript<Table>(tableScript));

            #endregion

            #region Index
            if (this.option.TableScriptsGenerateOption.GenerateIndex && indexes != null && !useColumnIndex)
            {
                foreach (TableIndex index in indexes)
                {
                    sb.AppendLine(this.AddIndex(index));
                }
            }
            #endregion

            return sb;
        }

        public override Script CreateSchema(DatabaseSchema schema)
        {
            return new Script("");
        }

        public override Script CreateSequence(Sequence sequence)
        {
            return new Script("");
        }

        public override Script CreateUserDefinedType(UserDefinedType userDefinedType)
        {
            return new Script("");
        }

        public override Script DropTable(Table table)
        {
            return new DropDbObjectScript<Table>(this.GetDropSql(nameof(DatabaseObjectType.Table), table));
        }

        public override Script DropSequence(Sequence sequence)
        {
            return new Script("");
        }

        public override Script DropProcedure(Procedure procedure)
        {
            return new Script("");
        }

        public override Script DropFunction(Function function)
        {
            return new Script("");
        }

        public override Script DropUserDefinedType(UserDefinedType userDefinedType)
        {
            return new Script("");
        }

        public override Script DropView(View view)
        {
            return new DropDbObjectScript<Table>(this.GetDropSql(nameof(DatabaseObjectType.View), view));
        }
        #endregion
    }
}
