using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class PostgresScriptGenerator : DbScriptGenerator
    {
        public string NotCreateIfExistsClause { get { return this.dbInterpreter.NotCreateIfExists ? "IF NOT EXISTS" : ""; } }

        public PostgresScriptGenerator(DbInterpreter dbInterpreter) : base(dbInterpreter) { }

        #region Schema Script 

        public override ScriptBuilder GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            ScriptBuilder sb = new ScriptBuilder();

            #region User Defined Type

            foreach (var userDefinedType in schemaInfo.UserDefinedTypes)
            {
                this.FeedbackInfo(OperationState.Begin, userDefinedType);

                sb.AppendLine(this.CreateUserDefinedType(userDefinedType));

                this.FeedbackInfo(OperationState.End, userDefinedType);
            }

            #endregion           

            #region Function           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<Function>(schemaInfo.Functions));
            #endregion

            #region Sequence          
            foreach (Sequence sequence in schemaInfo.Sequences)
            {
                this.FeedbackInfo(OperationState.Begin, sequence);

                sb.AppendLine(this.CreateSequence(sequence));

                this.FeedbackInfo(OperationState.End, sequence);
            }
            #endregion

            #region Table
            foreach (Table table in schemaInfo.Tables)
            {
                this.FeedbackInfo(OperationState.Begin, table);

                IEnumerable<TableColumn> columns = schemaInfo.TableColumns.Where(item => item.TableName == table.Name).OrderBy(item => item.Order);
                TablePrimaryKey primaryKey = schemaInfo.TablePrimaryKeys.FirstOrDefault(item => item.TableName == table.Name);
                IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.TableName == table.Name);
                IEnumerable<TableIndex> indexes = schemaInfo.TableIndexes.Where(item => item.TableName == table.Name).OrderBy(item => item.Order);
                IEnumerable<TableConstraint> constraints = schemaInfo.TableConstraints.Where(item => item.Schema == table.Schema && item.TableName == table.Name);

                foreach (TableIndex index in schemaInfo.TableIndexes)
                {
                    var indexName = index.Name;

                    if (schemaInfo.Tables.Any(item => item.Name == indexName))
                    {
                        string columnNames = string.Join("_", index.Columns.Select(item => item.ColumnName));

                        index.Name = $"IX_{index.TableName}_{columnNames}";
                    }
                }

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

            #region Procedure           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<Procedure>(schemaInfo.Procedures));
            #endregion

            if (this.option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
            {
                this.AppendScriptsToFile(sb.ToString(), GenerateScriptMode.Schema, true);
            }

            return sb;
        }
        #endregion

        #region Data Script        

        public override async Task<string> GenerateDataScriptsAsync(SchemaInfo schemaInfo)
        {
            return await base.GenerateDataScriptsAsync(schemaInfo);
        }

        protected override bool NeedInsertParameter(TableColumn column, object value)
        {
            if (value != null)
            {
                string dataType = column.DataType.ToLower();
                if (dataType == "bytea" || dataType == "bit" || dataType == "bit varying")
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
            return new AlterDbObjectScript<Table>($"ALTER TABLE {this.GetQuotedDbObjectNameWithSchema(table)} RENAME TO {this.GetQuotedString(newName)};");
        }

        public override Script SetTableComment(Table table, bool isNew = true)
        {
            return new AlterDbObjectScript<Table>($"COMMENT ON TABLE {this.GetQuotedFullTableName(table)} IS '{this.dbInterpreter.ReplaceSplitChar(this.TransferSingleQuotationString(table.Comment))}'" + this.scriptsDelimiter);
        }

        public override Script AddTableColumn(Table table, TableColumn column)
        {
            return new CreateDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedFullTableName(table)} ADD {this.dbInterpreter.ParseColumn(table, column)};");
        }

        public override Script RenameTableColumn(Table table, TableColumn column, string newName)
        {
            return new AlterDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedFullTableName(table)} RENAME {this.GetQuotedString(column.Name)} TO {this.GetQuotedString(newName)};");
        }

        public override Script AlterTableColumn(Table table, TableColumn newColumn, TableColumn oldColumn)
        {
            string alter = "";

            string newDataType = this.dbInterpreter.ParseDataType(newColumn);
            string oldDataType = this.dbInterpreter.ParseDataType(oldColumn);

            if (!string.IsNullOrEmpty(newColumn.ComputeExp))
            {
                alter = this.DropTableColumn(oldColumn).Content;

                alter += $"{Environment.NewLine}{this.AddTableColumn(table, newColumn).Content}";

                return new AlterDbObjectScript<TableColumn>(alter);
            }

            if (newDataType != oldDataType)
            {
                alter = $"TYPE {newDataType}";
            }

            if (newColumn.IsNullable && !oldColumn.IsNullable)
            {
                alter = "DROP NOT NULL";
            }

            if (!newColumn.IsNullable && oldColumn.IsNullable)
            {
                alter = "SET NOT NULL";
            }

            if (!string.IsNullOrEmpty(newColumn.DefaultValue) && string.IsNullOrEmpty(oldColumn.DefaultValue))
            {
                alter = $"SET DEFAULT {newColumn.DefaultValue}";
            }

            if (string.IsNullOrEmpty(newColumn.DefaultValue) && !string.IsNullOrEmpty(oldColumn.DefaultValue))
            {
                alter = "DROP DEFAULT";
            }

            return new AlterDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(table.Name)} ALTER COLUMN {this.GetQuotedString(newColumn.Name)} {alter};");
        }

        public override Script SetTableColumnComment(Table table, TableColumn column, bool isNew = true)
        {
            return new AlterDbObjectScript<TableColumn>($"COMMENT ON COLUMN {this.GetQuotedFullTableChildName(column)} IS '{this.dbInterpreter.ReplaceSplitChar(this.TransferSingleQuotationString(column.Comment))}'" + this.scriptsDelimiter);
        }

        public override Script DropTableColumn(TableColumn column)
        {
            return new DropDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(column.TableName)} DROP COLUMN {this.GetQuotedString(column.Name)};");
        }

        public override Script AddPrimaryKey(TablePrimaryKey primaryKey)
        {
            string pkName = string.IsNullOrEmpty(primaryKey.Name) ? this.GetQuotedString($"PK_{primaryKey.TableName}") : this.GetQuotedString(primaryKey.Name);

            string sql =
$@"
ALTER TABLE {this.GetQuotedFullTableName(primaryKey)} ADD CONSTRAINT {pkName} PRIMARY KEY 
(
{string.Join(Environment.NewLine, primaryKey.Columns.Select(item => $"{this.GetQuotedString(item.ColumnName)},")).TrimEnd(',')}
);";

            return new Script(sql);
        }

        public override Script DropPrimaryKey(TablePrimaryKey primaryKey)
        {
            return new DropDbObjectScript<TablePrimaryKey>(this.GetDropConstraintSql(primaryKey));
        }

        public override Script AddForeignKey(TableForeignKey foreignKey)
        {
            string columnNames = string.Join(",", foreignKey.Columns.Select(item => $"{this.GetQuotedString(item.ColumnName)}"));
            string referenceColumnName = string.Join(",", foreignKey.Columns.Select(item => $"{this.GetQuotedString(item.ReferencedColumnName)}"));

            string fkName = string.IsNullOrEmpty(foreignKey.Name) ? this.GetQuotedString($"FK_{foreignKey.TableName}_{foreignKey.ReferencedTableName}") : this.GetQuotedString(foreignKey.Name);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(
$@"
ALTER TABLE {this.GetQuotedFullTableName(foreignKey)} ADD CONSTRAINT {fkName} FOREIGN KEY ({columnNames})
REFERENCES {this.GetQuotedDbObjectNameWithSchema(foreignKey.ReferencedSchema, foreignKey.ReferencedTableName)}({referenceColumnName})");

            if (foreignKey.UpdateCascade)
            {
                sb.AppendLine("ON UPDATE CASCADE");
            }

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
            string columnNames = string.Join(",", index.Columns.Select(item => $"{this.GetQuotedString(item.ColumnName)}"));

            string indexName = string.IsNullOrEmpty(index.Name) ? this.GetQuotedString($"IX_{index.TableName}") : this.GetQuotedString(index.Name);

            string type = index.Type;
            IndexType indexType = IndexType.None;

            foreach (var name in Enum.GetNames(typeof(IndexType)))
            {
                if (name.ToUpper() == type?.ToUpper())
                {
                    indexType = (IndexType)Enum.Parse(typeof(IndexType), name);
                    break;
                }
            }

            if (indexType == IndexType.None || ((indexType | this.dbInterpreter.IndexType) != this.dbInterpreter.IndexType))
            {
                indexType = IndexType.BTree;
            }

            string sql = "";

            Action addNormOrUnique = () =>
            {
                if (type == IndexType.Unique.ToString())
                {
                    //use unique constraint, it can be used for foreign key reference.
                    sql = $"ALTER TABLE {this.GetQuotedFullTableName(index)} ADD CONSTRAINT {indexName} UNIQUE ({columnNames});";
                }
                else
                {
                    sql = $"CREATE INDEX {this.GetQuotedString(index.Name)} ON {this.GetQuotedFullTableName(index)}({columnNames});";
                }
            };

            if (indexType == IndexType.Unique)
            {
                addNormOrUnique();
            }
            else if (type != IndexType.Unique.ToString())
            {
                if ((indexType | this.dbInterpreter.IndexType) == this.dbInterpreter.IndexType)
                {
                    sql = $"CREATE INDEX {this.GetQuotedString(index.Name)} ON {this.GetQuotedFullTableName(index)} USING {indexType.ToString().ToUpper()}({columnNames});";
                }
                else
                {
                    addNormOrUnique();
                }
            }

            return new CreateDbObjectScript<TableIndex>(sql);
        }

        public override Script DropIndex(TableIndex index)
        {
            if (index.Type == IndexType.Unique.ToString())
            {
                return new CreateDbObjectScript<TableIndex>($"ALTER TABLE IF EXISTS {this.GetQuotedFullTableName(index)} DROP CONSTRAINT  {this.GetQuotedString(index.Name)};");
            }
            else
            {
                return new DropDbObjectScript<TableIndex>($"DROP INDEX {this.GetQuotedString(index.Name)};");
            }
        }

        public override Script AddCheckConstraint(TableConstraint constraint)
        {
            string ckName = string.IsNullOrEmpty(constraint.Name) ? this.GetQuotedString($"CK_{constraint.Name}") : this.GetQuotedString(constraint.Name);

            return new CreateDbObjectScript<TableConstraint>($"ALTER TABLE IF EXISTS {this.GetQuotedFullTableName(constraint)} ADD CONSTRAINT {ckName} CHECK {constraint.Definition};");
        }

        public override Script DropCheckConstraint(TableConstraint constraint)
        {
            return new DropDbObjectScript<TableConstraint>(this.GetDropConstraintSql(constraint));
        }

        public override Script SetIdentityEnabled(TableColumn column, bool enabled)
        {
            return new Script($"ALTER TABLE {this.GetQuotedFullTableName(column)} ALTER COLUMN {this.GetQuotedString(column.Name)} {(enabled ? "ADD" : "DROP")} IDENTITY IF EXISTS");
        }
        #endregion

        #region Database Operation
        public override Script CreateSchema(DatabaseSchema schema)
        {
            string script = $"CREATE SCHEMA IF NOT EXISTS {this.GetQuotedString(schema.Name)};";

            return new CreateDbObjectScript<DatabaseSchema>(script);
        }

        public override Script CreateUserDefinedType(UserDefinedType userDefinedType)
        {
            string dataTypes = string.Join(",", userDefinedType.Attributes.Select(item => $"{this.GetQuotedString(item.Name)} {this.dbInterpreter.ParseDataType(new TableColumn() { MaxLength = item.MaxLength, DataType = item.DataType, Precision = item.Precision, Scale = item.Scale })}"));

            string script = $"CREATE TYPE {this.GetQuotedString(userDefinedType.Name)} AS ({dataTypes})" + this.dbInterpreter.ScriptsDelimiter;

            return new CreateDbObjectScript<UserDefinedType>(script);
        }

        public override Script CreateSequence(Sequence sequence)
        {
            string script =
$@"CREATE SEQUENCE IF NOT EXISTS {this.GetQuotedDbObjectNameWithSchema(sequence)}
START {sequence.StartValue}
INCREMENT {sequence.Increment}
MINVALUE {(long)sequence.MinValue}
MAXVALUE {(long)sequence.MaxValue}
{(sequence.Cycled ? "CYCLE" : "")}
{(sequence.CacheSize > 0 ? $"CACHE {sequence.CacheSize}" : "")}
{(sequence.OwnedByTable == null ? "" : $"OWNED BY {this.GetQuotedString(sequence.OwnedByTable)}.{this.GetQuotedString(sequence.OwnedByColumn)}")};";

            return new CreateDbObjectScript<Sequence>(script);
        }
        public override ScriptBuilder CreateTable(Table table, IEnumerable<TableColumn> columns,
         TablePrimaryKey primaryKey,
         IEnumerable<TableForeignKey> foreignKeys,
         IEnumerable<TableIndex> indexes,
         IEnumerable<TableConstraint> constraints)
        {
            bool isLowDbVersion = this.dbInterpreter.IsLowDbVersion();

            ScriptBuilder sb = new ScriptBuilder();

            string tableName = table.Name;
            string quotedTableName = this.GetQuotedFullTableName(table);

            #region Create Table

            string option = this.GetCreateTableOption();

            string tableScript =
$@"
CREATE TABLE {this.NotCreateIfExistsClause} {quotedTableName}(
{string.Join("," + Environment.NewLine, columns.Select(item => this.dbInterpreter.ParseColumn(table, item))).TrimEnd(',')}
){(isLowDbVersion ? "" : option)};";

            sb.AppendLine(new CreateDbObjectScript<Table>(tableScript));

            #endregion

            sb.AppendLine();

            #region Comment
            if (this.option.TableScriptsGenerateOption.GenerateComment)
            {
                if (!string.IsNullOrEmpty(table.Comment))
                {
                    sb.AppendLine(this.SetTableComment(table));
                }

                foreach (TableColumn column in columns.Where(item => !string.IsNullOrEmpty(item.Comment)))
                {
                    sb.AppendLine(this.SetTableColumnComment(table, column, true));
                }
            }
            #endregion

            #region Primary Key
            if (this.option.TableScriptsGenerateOption.GeneratePrimaryKey && primaryKey != null)
            {
                sb.AppendLine(this.AddPrimaryKey(primaryKey));
            }
            #endregion

            #region Foreign Key
            if (this.option.TableScriptsGenerateOption.GenerateForeignKey && foreignKeys != null)
            {
                foreach (TableForeignKey foreignKey in foreignKeys)
                {
                    sb.AppendLine(this.AddForeignKey(foreignKey));
                }
            }
            #endregion

            #region Index
            if (this.option.TableScriptsGenerateOption.GenerateIndex && indexes != null)
            {
                List<string> indexColumns = new List<string>();

                foreach (TableIndex index in indexes)
                {
                    string columnNames = string.Join(",", index.Columns.OrderBy(item => item.ColumnName).Select(item => item.ColumnName));

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
            if (this.option.TableScriptsGenerateOption.GenerateConstraint && constraints != null)
            {
                foreach (TableConstraint constraint in constraints)
                {
                    sb.AppendLine(this.AddCheckConstraint(constraint));
                }
            }
            #endregion

            return sb;
        }
        public override Script DropUserDefinedType(UserDefinedType userDefinedType)
        {
            return new DropDbObjectScript<UserDefinedType>(this.GetDropSql(nameof(DatabaseObjectType.Type), userDefinedType));
        }

        public override Script DropSequence(Sequence sequence)
        {
            return new DropDbObjectScript<Sequence>(this.GetDropSql(nameof(DatabaseObjectType.Sequence), sequence));
        }

        public override Script DropTable(Table table)
        {
            return new DropDbObjectScript<Table>(this.GetDropSql(nameof(DatabaseObjectType.Table), table));
        }

        public override Script DropView(View view)
        {
            return new DropDbObjectScript<View>(this.GetDropSql(nameof(DatabaseObjectType.View), view));
        }

        public override Script DropTrigger(TableTrigger trigger)
        {
            return new DropDbObjectScript<View>(this.GetDropSql(nameof(DatabaseObjectType.Trigger), trigger));
        }

        public override Script DropFunction(Function function)
        {
            return new DropDbObjectScript<Function>(this.GetDropSql(nameof(DatabaseObjectType.Function), function));
        }

        public override Script DropProcedure(Procedure procedure)
        {
            return new DropDbObjectScript<Procedure>(this.GetDropSql(nameof(DatabaseObjectType.Procedure), procedure));
        }

        private string GetDropSql(string typeName, DatabaseObject dbObject)
        {
            string cascadeOption = (typeName == nameof(Table) || typeName == nameof(View) || typeName == nameof(Function)
                || typeName == nameof(Procedure)) ? " CASCADE" : "";

            return $"DROP {typeName.ToUpper()} IF EXISTS {this.GetQuotedDbObjectNameWithSchema(dbObject)}{cascadeOption};";
        }

        public override IEnumerable<Script> SetConstrainsEnabled(bool enabled)
        {
            List<string> sqls = new List<string>() { this.GetSqlForEnableTrigger(enabled) };
            List<string> cmds = new List<string>();

            using (DbConnection dbConnection = this.dbInterpreter.CreateConnection())
            {
                foreach (string sql in sqls)
                {
                    IDataReader reader = this.dbInterpreter.GetDataReader(dbConnection, sql);

                    while (reader.Read())
                    {
                        string cmd = reader[0].ToString();
                        cmds.Add(cmd);
                    }
                }

                foreach (string cmd in cmds)
                {
                    yield return new Script(cmd);
                }
            }

            yield return new Script(this.GetSqlForEnableConstraints(enabled));
        }

        private string GetSqlForEnableConstraints(bool enabled)
        {
            return $"SET session_replication_role = {(enabled ? "default" : "replica")};";
        }

        private string GetSqlForEnableTrigger(bool enabled)
        {
            return $@"SELECT CONCAT('ALTER TABLE ',event_object_schema,'.""',event_object_table,'"" {(enabled ? "ENABLE" : "DISABLE")} TRIGGER ALL;')
                      FROM information_schema.triggers
                      WHERE UPPER(trigger_catalog)= UPPER('{this.dbInterpreter.ConnectionInfo.Database}')";
        }
        #endregion
    }
}
