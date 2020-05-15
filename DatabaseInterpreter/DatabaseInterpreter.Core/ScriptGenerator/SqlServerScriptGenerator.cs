using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class SqlServerScriptGenerator : DbScriptGenerator
    {
        public SqlServerScriptGenerator(DbInterpreter dbInterpreter) : base(dbInterpreter) { }

        #region Schema Script   

        public override ScriptBuilder GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            ScriptBuilder sb = new ScriptBuilder();

            #region User Defined Type
            List<string> userTypeNames = new List<string>();
            foreach (UserDefinedType userDefinedType in schemaInfo.UserDefinedTypes)
            {
                this.FeedbackInfo(OperationState.Begin, userDefinedType);

                string userTypeName = userDefinedType.Name;

                if (userTypeNames.Contains(userTypeName))
                {
                    userTypeName += "_" + userDefinedType.AttrName;
                }

                TableColumn column = new TableColumn() { DataType = userDefinedType.Type, MaxLength = userDefinedType.MaxLength, Precision = userDefinedType.Precision, Scale = userDefinedType.Scale };
                string dataLength = this.dbInterpreter.GetColumnDataLength(column);

                string script = $@"CREATE TYPE {this.GetQuotedString(userDefinedType.Owner)}.{this.GetQuotedString(userTypeName)} FROM {this.GetQuotedString(userDefinedType.Type)}{(dataLength == "" ? "" : "(" + dataLength + ")")} {(userDefinedType.IsRequired ? "NOT NULL" : "NULL")};";

                sb.AppendLine(new CreateDbObjectScript<UserDefinedType>(script));
                sb.AppendLine(new SpliterScript(this.scriptsDelimiter));

                userTypeNames.Add(userDefinedType.Name);

                this.FeedbackInfo(OperationState.End, userDefinedType);
            }

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
                IEnumerable<TableColumn> tableColumns = schemaInfo.TableColumns.Where(item => item.Owner == table.Owner && item.TableName == tableName).OrderBy(item => item.Order);

                bool hasBigDataType = tableColumns.Any(item => this.IsBigDataType(item));

                #region Create Table

                string existsClause = $"IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='{(table.Name)}')";

                string tableScript =
$@"
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

{(this.dbInterpreter.NotCreateIfExists ? existsClause : "")}
CREATE TABLE {quotedTableName}(
{string.Join("," + Environment.NewLine, tableColumns.Select(item => this.dbInterpreter.ParseColumn(table, item)))}
) ON [PRIMARY]{(hasBigDataType ? " TEXTIMAGE_ON [PRIMARY]" : "")}" + ";";

                sb.AppendLine(new CreateDbObjectScript<Table>(tableScript));

                #endregion

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

                #region Default Value
                if (this.option.TableScriptsGenerateOption.GenerateDefaultValue)
                {
                    IEnumerable<TableColumn> defaultValueColumns = schemaInfo.TableColumns.Where(item => item.Owner == table.Owner && item.TableName == tableName && !string.IsNullOrEmpty(item.DefaultValue));

                    foreach (TableColumn column in defaultValueColumns)
                    {
                        sb.AppendLine(this.AddDefaultValueConstraint(column));
                    }
                }
                #endregion

                TablePrimaryKey primaryKey = schemaInfo.TablePrimaryKeys.FirstOrDefault(item => item.Owner == table.Owner && item.TableName == tableName);

                #region Primary Key
                if (this.option.TableScriptsGenerateOption.GeneratePrimaryKey && primaryKey != null)
                {
                    sb.AppendLine(this.AddPrimaryKey(primaryKey));

                    if(!string.IsNullOrEmpty(primaryKey.Comment))
                    {
                        sb.AppendLine(this.SetTableChildComment(primaryKey, true));
                    }
                }
                #endregion                    

                #region Foreign Key
                if (this.option.TableScriptsGenerateOption.GenerateForeignKey)
                {
                    IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.Owner == table.Owner && item.TableName == tableName);

                    foreach (TableForeignKey foreignKey in foreignKeys)
                    {
                        sb.AppendLine(this.AddForeignKey(foreignKey));

                        if(!string.IsNullOrEmpty(foreignKey.Comment))
                        {
                            sb.AppendLine(this.SetTableChildComment(foreignKey, true));
                        }
                    }
                }
                #endregion

                #region Index
                if (this.option.TableScriptsGenerateOption.GenerateIndex)
                {
                    IEnumerable<TableIndex> indexes = schemaInfo.TableIndexes.Where(item => item.Owner == table.Owner && item.TableName == tableName).OrderBy(item => item.Order);

                    foreach (TableIndex index in indexes)
                    {
                        sb.AppendLine(this.AddIndex(index));

                        if(!string.IsNullOrEmpty(index.Comment))
                        {
                            sb.AppendLine(this.SetTableChildComment(index, true));
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

                        if(!string.IsNullOrEmpty(constraint.Comment))
                        {
                            sb.AppendLine(this.SetTableChildComment(constraint, true));
                        }
                    }
                }
                #endregion

                sb.Append(new SpliterScript(this.scriptsDelimiter));

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
                this.AppendScriptsToFile(sb.ToString().Trim(), GenerateScriptMode.Schema, true);
            }

            return sb;
        }

        private bool IsBigDataType(TableColumn column)
        {
            switch (column.DataType)
            {
                case "text":
                case "ntext":
                case "image":
                case "xml":
                    return true;
                case "varchar":
                case "nvarchar":
                case "varbinary":
                    if (column.MaxLength == -1)
                    {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        #endregion

        #region Data Script      

        public override Task<string> GenerateDataScriptsAsync(SchemaInfo schemaInfo)
        {
            return base.GenerateDataScriptsAsync(schemaInfo);
        }

        protected override string GetBytesConvertHexString(object value, string dataType)
        {
            string hex = ValueHelper.BytesToHexString(value as byte[]);
            return $"CAST({hex} AS {dataType})";
        }
        #endregion

        #region Alter Table

        public override Script RenameTable(Table table, string newName)
        {
            return new ExecuteProcedureScript($"EXEC sp_rename '{this.GetQuotedObjectName(table)}', '{newName}'");
        }

        public override Script SetTableComment(Table table, bool isNew = true)
        {
            return new ExecuteProcedureScript($"EXEC {(isNew ? "sp_addextendedproperty" : "sp_updateextendedproperty")} N'MS_Description',N'{this.dbInterpreter.ReplaceSplitChar(this.TransferSingleQuotationString(table.Comment))}',N'SCHEMA',N'{table.Owner}',N'table',N'{table.Name}',NULL,NULL");
        }

        public override Script AddTableColumn(Table table, TableColumn column)
        {
            return new CreateDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedObjectName(table)} ADD { this.dbInterpreter.ParseColumn(table, column)}");
        }

        public override Script RenameTableColumn(Table table, TableColumn column, string newName)
        {
            return new ExecuteProcedureScript(this.GetRenameScript(table.Owner, "COLUMN", table.Name, column.Name, newName));
        }

        public override Script AlterTableColumn(Table table, TableColumn column)
        {
            return new AlterDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedObjectName(table)} ALTER COLUMN {this.dbInterpreter.ParseColumn(table, column)}");
        }

        private string GetRenameScript(string owner, string type, string tableName, string name, string newName)
        {
            return $"EXEC sp_rename N'{owner}.{this.GetQuotedString(tableName)}.{this.GetQuotedString(name)}', N'{newName}', N'{type}'";
        }

        public override Script SetTableColumnComment(Table table, TableColumn column, bool isNew = true)
        {
            return this.SetTableChildComment(column, isNew);
        }

        public override Script DropTableColumn(TableColumn column)
        {
            return new DropDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedFullTableName(column)} DROP COLUMN {this.GetQuotedString(column.Name)}");
        }

        public override Script AddPrimaryKey(TablePrimaryKey primaryKey)
        {
            string script =
$@"ALTER TABLE {this.GetQuotedFullTableName(primaryKey)} ADD CONSTRAINT
{this.GetQuotedString(primaryKey.Name)} PRIMARY KEY {(primaryKey.Clustered ? "CLUSTERED" : "NONCLUSTERED")}
(
    {string.Join(",", primaryKey.Columns.Select(item => $"{this.GetQuotedString(item.ColumnName)} {(item.IsDesc ? "DESC" : "")}"))}
) WITH(STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]";

            return new CreateDbObjectScript<TablePrimaryKey>(script);
        }

        public override Script DropPrimaryKey(TablePrimaryKey primaryKey)
        {
            return new DropDbObjectScript<TablePrimaryKey>(this.GetDropConstraintSql(primaryKey));
        }

        public override Script AddForeignKey(TableForeignKey foreignKey)
        {
            string quotedTableName = this.GetQuotedFullTableName(foreignKey);

            string columnNames = string.Join(",", foreignKey.Columns.Select(item => $"{ this.GetQuotedString(item.ColumnName)}"));
            string referencedColumnName = string.Join(",", foreignKey.Columns.Select(item => $"{ this.GetQuotedString(item.ReferencedColumnName)}"));

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(
$@"ALTER TABLE {quotedTableName} WITH CHECK ADD CONSTRAINT { this.GetQuotedString(foreignKey.Name)} FOREIGN KEY({columnNames})
REFERENCES {this.GetQuotedString(foreignKey.Owner)}.{this.GetQuotedString(foreignKey.ReferencedTableName)} ({referencedColumnName})");

            if (foreignKey.UpdateCascade)
            {
                sb.AppendLine("ON UPDATE CASCADE");
            }

            if (foreignKey.DeleteCascade)
            {
                sb.AppendLine("ON DELETE CASCADE");
            }

            sb.AppendLine($"ALTER TABLE {quotedTableName} CHECK CONSTRAINT {this.GetQuotedString(foreignKey.Name)}");

            return new CreateDbObjectScript<TableForeignKey>(sb.ToString());
        }

        public override Script DropForeignKey(TableForeignKey foreignKey)
        {
            return new DropDbObjectScript<TableForeignKey>(this.GetDropConstraintSql(foreignKey));
        }

        public override Script AddIndex(TableIndex index)
        {
            string columnNames = string.Join(",", index.Columns.Select(item => this.GetQuotedString(item.ColumnName)));

            string unique = index.IsUnique ? "UNIQUE" : "";
            string clustered = index.Clustered ? "CLUSTERED" : "NONCLUSTERED";
            string type = index.Type == IndexType.ColumnStore.ToString() ? "COLUMNSTORE" : "";

            string sql = $@"CREATE {unique} {clustered} {type} INDEX {this.GetQuotedString(index.Name)} ON {this.GetQuotedFullTableName(index)}({columnNames})";

            return new CreateDbObjectScript<TableIndex>(sql);
        }

        public override Script DropIndex(TableIndex index)
        {
            return new DropDbObjectScript<TableIndex>($"DROP INDEX {this.GetQuotedString(index.Name)} ON {this.GetQuotedFullTableName(index)}");
        }

        private string GetDropConstraintSql(TableChild tableChild)
        {
            return $"ALTER TABLE {this.GetQuotedString(tableChild.TableName)} DROP CONSTRAINT {this.GetQuotedString(tableChild.Name)}";
        }

        public override Script AddCheckConstraint(TableConstraint constraint)
        {
            return new CreateDbObjectScript<TableConstraint>($"ALTER TABLE {constraint.Owner}.{this.GetQuotedString(constraint.TableName)}  WITH CHECK ADD CONSTRAINT {this.GetQuotedString(constraint.Name)} CHECK  ({constraint.Definition})");
        }

        public override Script DropCheckConstraint(TableConstraint constraint)
        {
            return new DropDbObjectScript<TableConstraint>(this.GetDropConstraintSql(constraint));
        }

        public Script SetTableChildComment(TableChild tableChild, bool isNew)
        {
            string typeName = tableChild.GetType().Name;

            string type = "";

            if(typeName == nameof(TableColumn))
            {
                type = "COLUMN";
            }
            else if(typeName == nameof(TablePrimaryKey) || typeName == nameof(TableForeignKey) || typeName== nameof(TableConstraint))
            {
                type = "CONSTRAINT";
            }
            else if(typeName == nameof(TableIndex))
            {
                type = "INDEX";
            }

            string sql = $"EXEC {(isNew ? "sp_addextendedproperty" : "sp_updateextendedproperty")} N'MS_Description',N'{this.TransferSingleQuotationString(tableChild.Comment)}',N'SCHEMA',N'{tableChild.Owner}',N'table',N'{tableChild.TableName}',N'{type}',N'{tableChild.Name}'";
            return new ExecuteProcedureScript(sql);
        }

        public Script AddDefaultValueConstraint(TableColumn column)
        {
            return new AlterDbObjectScript<Table>($"ALTER TABLE {this.GetQuotedFullTableName(column)} ADD CONSTRAINT {this.GetQuotedString($"DF_{column.TableName}_{column.Name}")}  DEFAULT {this.dbInterpreter.GetColumnDefaultValue(column)} FOR { this.GetQuotedString(column.Name)}");
        }

        public Script DropDefaultValueConstraint(TableDefaultValueConstraint defaultValueConstraint)
        {
            return new DropDbObjectScript<TableDefaultValueConstraint>(this.GetDropConstraintSql(defaultValueConstraint));
        }

        #endregion

        #region Database Operation

        public override Script DropUserDefinedType(UserDefinedType userDefinedType)
        {
            return new DropDbObjectScript<UserDefinedType>(this.GetDropSql("type", userDefinedType));
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
            return $"DROP {typeName.ToUpper()} IF EXISTS {this.GetQuotedObjectName(dbObject)};";
        }      

        #endregion
    }
}
