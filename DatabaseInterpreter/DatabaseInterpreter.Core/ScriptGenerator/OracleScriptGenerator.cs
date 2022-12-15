using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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

            string dbSchema = this.GetDbSchema();

            #region User Defined Type          

            foreach (var userDefinedType in schemaInfo.UserDefinedTypes)
            {
                this.FeedbackInfo(OperationState.Begin, userDefinedType);

                sb.AppendLine(this.CreateUserDefinedType(userDefinedType));

                this.FeedbackInfo(OperationState.End, userDefinedType);
            }

            #endregion

            #region Sequence          
            foreach (Sequence sequence in schemaInfo.Sequences)
            {
                this.FeedbackInfo(OperationState.Begin, sequence);

                sb.AppendLine(this.CreateSequence(sequence));

                this.FeedbackInfo(OperationState.End, sequence);
            }
            #endregion

            #region Function           
            sb.AppendRange(this.GenerateScriptDbObjectScripts<Function>(schemaInfo.Functions));
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

        private string GetDbSchema()
        {
            return (this.dbInterpreter as OracleInterpreter).GetDbSchema();
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

        protected override string GetBatchInsertItemBefore(string tableName, string columnNames, bool isFirstRow)
        {
            return isFirstRow ? "" : $"INTO {tableName}{(string.IsNullOrEmpty(columnNames) ? "" : $"({columnNames})")} VALUES";
        }

        protected override string GetBatchInsertItemEnd(bool isAllEnd)
        {
            return (isAllEnd ? $"{Environment.NewLine}SELECT 1 FROM DUAL;" : "");
        }

        protected override bool NeedInsertParameter(TableColumn column, object value)
        {
            if (value != null)
            {
                Type type = value.GetType();
                string dataType = column.DataType.ToLower();

                if (dataType == "clob")
                {
                    return true;
                }

                if (type == typeof(string))
                {
                    string str = value.ToString();

                    if (str.Length > 1000 || (str.Contains(OracleInterpreter.SEMICOLON_FUNC) && str.Length > 500))
                    {
                        return true;
                    }
                }
                else if (type.Name == nameof(TimeSpan))
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
            return new AlterDbObjectScript<Table>($"RENAME {this.GetQuotedFullTableName(table)} TO {this.GetQuotedString(newName)};");
        }

        public override Script SetTableComment(Table table, bool isNew = true)
        {
            return new AlterDbObjectScript<Table>($"COMMENT ON TABLE {this.GetQuotedFullTableName(table)} IS '{this.dbInterpreter.ReplaceSplitChar(this.TransferSingleQuotationString(table.Comment))}'" + this.scriptsDelimiter);
        }

        public override Script AddTableColumn(Table table, TableColumn column)
        {
            return new CreateDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(table.Name)} ADD {this.dbInterpreter.ParseColumn(table, column)};");
        }

        public override Script RenameTableColumn(Table table, TableColumn column, string newName)
        {
            return new AlterDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(table.Name)} RENAME COLUMN {this.GetQuotedString(column.Name)} TO {this.GetQuotedString(newName)};");
        }

        public override Script AlterTableColumn(Table table, TableColumn newColumn, TableColumn oldColumn)
        {
            string clause = this.dbInterpreter.ParseColumn(table, newColumn);

            if (DataTypeHelper.IsGeometryType(newColumn.DataType))
            {
                clause = clause.Replace(this.dbInterpreter.ParseDataType(newColumn), "");
            }

            return new AlterDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedString(table.Name)} MODIFY {clause}");
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
            string tablespace = this.dbInterpreter.ConnectionInfo.Database;
            string strTablespace = string.IsNullOrEmpty(tablespace) ? "" : $"TABLESPACE {tablespace}";
            string pkName = string.IsNullOrEmpty(primaryKey.Name) ? this.GetQuotedString($"PK_{primaryKey.TableName}") : this.GetQuotedString(primaryKey.Name);

            string sql =
$@"
ALTER TABLE {this.GetQuotedFullTableName(primaryKey)} ADD CONSTRAINT {pkName} PRIMARY KEY 
(
{string.Join(Environment.NewLine, primaryKey.Columns.Select(item => $"{this.GetQuotedString(item.ColumnName)},")).TrimEnd(',')}
)
USING INDEX 
{strTablespace}{this.scriptsDelimiter}";

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
REFERENCES {this.GetQuotedString(foreignKey.ReferencedTableName)}({referenceColumnName})");

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

            return new CreateDbObjectScript<TableIndex>($"CREATE {type} INDEX {indexName} ON {this.GetQuotedFullTableName(index)} ({columnNames}){reverse};");
        }

        public override Script DropIndex(TableIndex index)
        {
            return new DropDbObjectScript<TableIndex>($"DROP INDEX {this.GetQuotedString(index.Name)};");
        }

        public override Script AddCheckConstraint(TableConstraint constraint)
        {
            string ckName = string.IsNullOrEmpty(constraint.Name) ? this.GetQuotedString($"CK_{constraint.TableName}") : this.GetQuotedString(constraint.Name);

            return new CreateDbObjectScript<TableConstraint>($"ALTER TABLE {this.GetQuotedFullTableName(constraint)} ADD CONSTRAINT {ckName} CHECK ({constraint.Definition});");
        }

        private Script AddUniqueConstraint(TableIndex index)
        {
            string columnNames = string.Join(",", index.Columns.Select(item => $"{this.GetQuotedString(item.ColumnName)}"));

            return new CreateDbObjectScript<TableConstraint>($"ALTER TABLE {this.GetQuotedFullTableName(index)} ADD CONSTRAINT {this.GetQuotedString(index.Name)} UNIQUE ({columnNames});");
        }

        public override Script DropCheckConstraint(TableConstraint constraint)
        {
            return new DropDbObjectScript<TableConstraint>(this.GetDropConstraintSql(constraint));
        }

        public override Script SetIdentityEnabled(TableColumn column, bool enabled)
        {
            return new Script("");
        }
        #endregion

        #region Database Operation
        public override Script CreateSchema(DatabaseSchema schema) { return new Script(""); }

        public override Script CreateUserDefinedType(UserDefinedType userDefinedType)
        {
            string dataTypes = string.Join(",", userDefinedType.Attributes.Select(item => $"{this.GetQuotedString(item.Name)} {this.dbInterpreter.ParseDataType(new TableColumn() { MaxLength = item.MaxLength, DataType = item.DataType, Precision = item.Precision, Scale = item.Scale })}"));

            string script = $"CREATE TYPE {this.GetQuotedString(userDefinedType.Name)} AS OBJECT ({dataTypes})" + this.dbInterpreter.ScriptsDelimiter;

            return new CreateDbObjectScript<UserDefinedType>(script);
        }

        public override Script CreateSequence(Sequence sequence)
        {
            string script =
$@"CREATE SEQUENCE {this.GetQuotedString(sequence.Name)}
START WITH {sequence.StartValue}
INCREMENT BY {sequence.Increment}
MINVALUE {sequence.MinValue}
MAXVALUE {sequence.MaxValue} 
{(sequence.CacheSize > 1 ? $"CACHE {sequence.CacheSize}" : "")}
{(sequence.Cycled ? "CYCLE" : "")}
{(sequence.Ordered ? "ORDER" : "")};";

            return new CreateDbObjectScript<Sequence>(script);
        }

        public override ScriptBuilder CreateTable(Table table, IEnumerable<TableColumn> columns,
         TablePrimaryKey primaryKey,
         IEnumerable<TableForeignKey> foreignKeys,
         IEnumerable<TableIndex> indexes,
         IEnumerable<TableConstraint> constraints)
        {
            ScriptBuilder sb = new ScriptBuilder();

            string tableName = table.Name;
            string quotedTableName = this.GetQuotedFullTableName(table);

            string tablespace = this.dbInterpreter.ConnectionInfo.Database;
            string strTablespace = string.IsNullOrEmpty(tablespace) ? "" : $"TABLESPACE {tablespace}";

            #region Create Table

            string option = this.GetCreateTableOption();

            string tableScript =
$@"
CREATE TABLE {quotedTableName}(
{string.Join("," + Environment.NewLine, columns.Select(item => this.dbInterpreter.ParseColumn(table, item))).TrimEnd(',')}
){strTablespace}" + (string.IsNullOrEmpty(option) ? "" : Environment.NewLine + option) + this.scriptsDelimiter;

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

                var primaryKeyColumnNames = primaryKey?.Columns?.OrderBy(item => item.ColumnName)?.Select(item => item.ColumnName);

                foreach (TableIndex index in indexes)
                {
                    var indexColumnNames = index.Columns.OrderBy(item => item.ColumnName).Select(item => item.ColumnName);

                    //primary key column can't be indexed twice if they have same name and same order
                    if (primaryKeyColumnNames != null && primaryKeyColumnNames.SequenceEqual(indexColumnNames))
                    {
                        continue;
                    }

                    string strIndexColumnNames = string.Join(",", indexColumnNames);

                    //Avoid duplicated indexes for one index.
                    if (indexColumns.Contains(strIndexColumnNames))
                    {
                        continue;
                    }

                    if (index.Type == nameof(IndexType.Unique) || index.IsUnique)
                    {
                        //create a constraint, if the column has foreign key, it's required.
                        sb.AppendLine(this.AddUniqueConstraint(index));
                    }
                    else
                    {
                        sb.AppendLine(this.AddIndex(index));
                    }

                    if (!indexColumns.Contains(strIndexColumnNames))
                    {
                        indexColumns.Add(strIndexColumnNames);
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
            bool isTable = dbObject is Table;

            return $@"DROP {typeName.ToUpper()} {this.GetQuotedDbObjectNameWithSchema(dbObject)}{(isTable ? " PURGE" : "")};";
        }

        public override IEnumerable<Script> SetConstrainsEnabled(bool enabled)
        {
            List<string> sqls = new List<string>() { this.GetSqlForEnableConstraints(enabled), this.GetSqlForEnableTrigger(enabled) };
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
        }

        private string GetSqlForEnableConstraints(bool enabled)
        {
            return $@"SELECT 'ALTER TABLE ""'|| T.TABLE_NAME ||'"" {(enabled ? "ENABLE" : "DISABLE")} CONSTRAINT ""'||T.CONSTRAINT_NAME || '""' AS ""SQL""  
                            FROM USER_CONSTRAINTS T 
                            WHERE T.CONSTRAINT_TYPE = 'R'
                            AND UPPER(OWNER)= UPPER('{this.GetDbSchema()}')
                           ";
        }

        private string GetSqlForEnableTrigger(bool enabled)
        {
            return $@"SELECT 'ALTER TRIGGER ""'|| TRIGGER_NAME || '"" {(enabled ? "ENABLE" : "DISABLE")} '
                         FROM USER_TRIGGERS
                         WHERE UPPER(TABLE_OWNER)= UPPER('{this.GetDbSchema()}')";
        }
        #endregion
    }
}
