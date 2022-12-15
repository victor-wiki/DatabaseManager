using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
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
            foreach (UserDefinedType userDefinedType in schemaInfo.UserDefinedTypes)
            {
                this.FeedbackInfo(OperationState.Begin, userDefinedType);                

                sb.AppendLine(this.CreateUserDefinedType(userDefinedType));
                sb.AppendLine(new SpliterScript(this.scriptsDelimiter));                

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
                    if (column.MaxLength == -1 && !column.IsComputed)
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
            return new ExecuteProcedureScript($"EXEC sp_rename '{this.GetQuotedDbObjectNameWithSchema(table)}', '{newName}'");
        }

        public override Script SetTableComment(Table table, bool isNew = true)
        {
            return new ExecuteProcedureScript($"EXEC {(isNew ? "sp_addextendedproperty" : "sp_updateextendedproperty")} N'MS_Description',N'{this.dbInterpreter.ReplaceSplitChar(this.TransferSingleQuotationString(table.Comment))}',N'SCHEMA',N'{table.Schema}',N'table',N'{table.Name}',NULL,NULL");
        }

        public override Script AddTableColumn(Table table, TableColumn column)
        {
            return new CreateDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedDbObjectNameWithSchema(table)} ADD { this.dbInterpreter.ParseColumn(table, column)}");
        }

        public override Script RenameTableColumn(Table table, TableColumn column, string newName)
        {
            return new ExecuteProcedureScript(this.GetRenameScript(table.Schema, "COLUMN", table.Name, column.Name, newName));
        }

        public override Script AlterTableColumn(Table table, TableColumn newColumn, TableColumn oldColumn)
        {
            return new AlterDbObjectScript<TableColumn>($"ALTER TABLE {this.GetQuotedFullTableName(table)} ALTER COLUMN {this.dbInterpreter.ParseColumn(table, newColumn)}");
        }

        private string GetRenameScript(string schema, string type, string tableName, string name, string newName)
        {
            return $"EXEC sp_rename N'{schema}.{this.GetQuotedString(tableName)}.{this.GetQuotedString(name)}', N'{newName}', N'{type}'";
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
            string pkName = string.IsNullOrEmpty(primaryKey.Name)? this.GetQuotedString($"PK_{primaryKey.TableName}"): this.GetQuotedString(primaryKey.Name);

            string script =
$@"ALTER TABLE {this.GetQuotedFullTableName(primaryKey)} ADD CONSTRAINT
{pkName} PRIMARY KEY {(primaryKey.Clustered ? "CLUSTERED" : "NONCLUSTERED")}
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
            string fkName = string.IsNullOrEmpty(foreignKey.Name)? this.GetQuotedString($"FK_{foreignKey.TableName}_{foreignKey.ReferencedTableName}"): this.GetQuotedString(foreignKey.Name);

            string columnNames = string.Join(",", foreignKey.Columns.Select(item => $"{ this.GetQuotedString(item.ColumnName)}"));
            string referencedColumnName = string.Join(",", foreignKey.Columns.Select(item => $"{ this.GetQuotedString(item.ReferencedColumnName)}"));

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(
$@"ALTER TABLE {quotedTableName} WITH CHECK ADD CONSTRAINT { fkName } FOREIGN KEY({columnNames})
REFERENCES {this.GetQuotedDbObjectNameWithSchema(foreignKey.ReferencedSchema, foreignKey.ReferencedTableName)} ({referencedColumnName})");

            if (foreignKey.UpdateCascade)
            {
                sb.AppendLine("ON UPDATE CASCADE");
            }

            if (foreignKey.DeleteCascade)
            {
                sb.AppendLine("ON DELETE CASCADE");
            }

            //sb.AppendLine($"ALTER TABLE {quotedTableName} CHECK CONSTRAINT {this.GetQuotedString(foreignKey.Name)}");

            return new CreateDbObjectScript<TableForeignKey>(sb.ToString());
        }

        public override Script DropForeignKey(TableForeignKey foreignKey)
        {
            return new DropDbObjectScript<TableForeignKey>(this.GetDropConstraintSql(foreignKey));
        }

        public override Script AddIndex(TableIndex index)
        {
            string columnNames = string.Join(",", index.Columns.Select(item => $"{this.GetQuotedString(item.ColumnName)}{(item.IsDesc?" DESC":"")}"));

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
            return $"ALTER TABLE {this.GetQuotedFullTableName(tableChild)} DROP CONSTRAINT {this.GetQuotedString(tableChild.Name)}";
        }

        public override Script AddCheckConstraint(TableConstraint constraint)
        {
            string ckName = string.IsNullOrEmpty(constraint.Name) ? this.GetQuotedString($"CK_{constraint.TableName}") : this.GetQuotedString(constraint.Name);

            return new CreateDbObjectScript<TableConstraint>($"ALTER TABLE {this.GetQuotedFullTableName(constraint)}  WITH CHECK ADD CONSTRAINT {ckName} CHECK  ({constraint.Definition})");
        }

        public override Script DropCheckConstraint(TableConstraint constraint)
        {
            return new DropDbObjectScript<TableConstraint>(this.GetDropConstraintSql(constraint));
        }

        public Script SetTableChildComment(TableChild tableChild, bool isNew)
        {
            string typeName = tableChild.GetType().Name;

            string type = "";

            if (typeName == nameof(TableColumn))
            {
                type = "COLUMN";
            }
            else if (typeName == nameof(TablePrimaryKey) || typeName == nameof(TableForeignKey) || typeName == nameof(TableConstraint))
            {
                type = "CONSTRAINT";
            }
            else if (typeName == nameof(TableIndex))
            {
                type = "INDEX";
            }

            string sql = $"EXEC {(isNew ? "sp_addextendedproperty" : "sp_updateextendedproperty")} N'MS_Description',N'{this.TransferSingleQuotationString(tableChild.Comment)}',N'SCHEMA',N'{tableChild.Schema}',N'table',N'{tableChild.TableName}',N'{type}',N'{tableChild.Name}'";
            return new ExecuteProcedureScript(sql);
        }

        public Script AddDefaultValueConstraint(TableColumn column)
        {
            string defaultValue = StringHelper.GetParenthesisedString(this.dbInterpreter.GetColumnDefaultValue(column));

            return new AlterDbObjectScript<Table>($"ALTER TABLE {this.GetQuotedFullTableName(column)} ADD CONSTRAINT {this.GetQuotedString($"DF_{column.TableName}_{column.Name}")}  DEFAULT {defaultValue} FOR {this.GetQuotedString(column.Name)}");
        }

        public Script DropDefaultValueConstraint(TableDefaultValueConstraint defaultValueConstraint)
        {
            return new DropDbObjectScript<TableDefaultValueConstraint>(this.GetDropConstraintSql(defaultValueConstraint));
        }

        public override Script SetIdentityEnabled(TableColumn column, bool enabled)
        {
            return new AlterDbObjectScript<Table>($"SET IDENTITY_INSERT { this.GetQuotedFullTableName(column) } {(enabled ? "OFF" : "ON")}");
        }

        #endregion

        #region Database Operation
        public override Script CreateSchema(DatabaseSchema schema) 
        {
            string script = $"CREATE SCHEMA {this.GetQuotedString(schema.Name)};";

            return new CreateDbObjectScript<DatabaseSchema>(script);
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

            bool hasBigDataType = columns.Any(item => this.IsBigDataType(item));

            string option = this.GetCreateTableOption();

            #region Create Table

            string existsClause = $"IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='{(table.Name)}')";

            string tableScript =
$@"
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

{(this.dbInterpreter.NotCreateIfExists ? existsClause : "")}
CREATE TABLE {quotedTableName}(
{string.Join("," + Environment.NewLine, columns.Select(item => this.dbInterpreter.ParseColumn(table, item)))}
) {option}{(hasBigDataType ? " TEXTIMAGE_ON [PRIMARY]" : "")}" + ";";

            sb.AppendLine(new CreateDbObjectScript<Table>(tableScript));

            #endregion

            #region Comment
            if(this.option.TableScriptsGenerateOption.GenerateComment)
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

            #region Default Value
            if (this.option.TableScriptsGenerateOption.GenerateDefaultValue)
            {
                IEnumerable<TableColumn> defaultValueColumns = columns.Where(item => item.Schema == table.Schema && item.TableName == tableName && !string.IsNullOrEmpty(item.DefaultValue));

                foreach (TableColumn column in defaultValueColumns)
                {
                    if(ValueHelper.IsSequenceNextVal(column.DefaultValue))
                    {
                        continue;
                    }
                    else if(column.DefaultValue.ToUpper().TrimStart().StartsWith("CREATE DEFAULT"))
                    {
                        continue;
                    }

                    sb.AppendLine(this.AddDefaultValueConstraint(column));
                }
            }
            #endregion            

            #region Primary Key
            if (this.option.TableScriptsGenerateOption.GeneratePrimaryKey && primaryKey != null)
            {
                sb.AppendLine(this.AddPrimaryKey(primaryKey));

                if (!string.IsNullOrEmpty(primaryKey.Comment))
                {
                    sb.AppendLine(this.SetTableChildComment(primaryKey, true));
                }
            }
            #endregion

            #region Foreign Key
            if (this.option.TableScriptsGenerateOption.GenerateForeignKey && foreignKeys != null)
            {
                foreach (TableForeignKey foreignKey in foreignKeys)
                {
                    sb.AppendLine(this.AddForeignKey(foreignKey));

                    if (!string.IsNullOrEmpty(foreignKey.Comment))
                    {
                        sb.AppendLine(this.SetTableChildComment(foreignKey, true));
                    }
                }
            }
            #endregion

            #region Index
            if (this.option.TableScriptsGenerateOption.GenerateIndex && indexes != null)
            {
                foreach (TableIndex index in indexes)
                {
                    sb.AppendLine(this.AddIndex(index));

                    if (!string.IsNullOrEmpty(index.Comment))
                    {
                        sb.AppendLine(this.SetTableChildComment(index, true));
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

                    if (!string.IsNullOrEmpty(constraint.Comment))
                    {
                        sb.AppendLine(this.SetTableChildComment(constraint, true));
                    }
                }
            }
            #endregion

            sb.Append(new SpliterScript(this.scriptsDelimiter));

            return sb;
        }

        public override Script CreateUserDefinedType(UserDefinedType userDefinedType)
        {
            //only fetch first one, because SQLServer UDT is single attribute
            UserDefinedTypeAttribute attribute = userDefinedType.Attributes.First();

            TableColumn column = new TableColumn() { DataType = attribute.DataType, MaxLength = attribute.MaxLength, Precision = attribute.Precision, Scale = attribute.Scale };
            string dataLength = this.dbInterpreter.GetColumnDataLength(column);

            string script = $@"CREATE TYPE {this.GetQuotedDbObjectNameWithSchema(userDefinedType)} FROM {this.GetQuotedString(attribute.DataType)}{(dataLength == "" ? "" : "(" + dataLength + ")")} {(attribute.IsRequired ? "NOT NULL" : "NULL")};";

            return new CreateDbObjectScript<UserDefinedType>(script);
        }

        public override Script CreateSequence(Sequence sequence)
        {
            string script = 
$@"CREATE SEQUENCE {this.GetQuotedDbObjectNameWithSchema(sequence)} AS {sequence.DataType} 
START WITH {sequence.StartValue}
INCREMENT BY {sequence.Increment}
MINVALUE {(long)sequence.MinValue}
MAXVALUE {(long)sequence.MaxValue}
{(sequence.Cycled? "CYCLE" : "")}
{(sequence.UseCache? "CACHE":"")}{(sequence.CacheSize>0? $" {sequence.CacheSize}" : "")};";

            return new CreateDbObjectScript<Sequence>(script);
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
            return $"DROP {typeName.ToUpper()} IF EXISTS {this.GetQuotedDbObjectNameWithSchema(dbObject)};";
        }

        public override IEnumerable<Script> SetConstrainsEnabled(bool enabled)
        {
            string procName = "sp_MSForEachTable";

            string sql =
$@"
IF ServerProperty('Edition') != '{SqlServerInterpreter.AzureSQLFlag}'
BEGIN
  EXEC {procName} 'ALTER TABLE ? {(enabled ? "CHECK" : "NOCHECK")} CONSTRAINT ALL';
  EXEC {procName} 'ALTER TABLE ? {(enabled ? "ENABLE" : "DISABLE")} TRIGGER ALL';
END
ELSE 
BEGIN
    DECLARE @owner NVARCHAR(50)
	DECLARE @tableName NVARCHAR(256)

	DECLARE table_cursor CURSOR  
    FOR SELECT SCHEMA_NAME(schema_id),name FROM sys.tables  
	OPEN table_cursor  

    FETCH NEXT FROM table_cursor INTO @owner,@tableName
  
    WHILE @@FETCH_STATUS = 0  
    BEGIN  
        EXEC('ALTER TABLE ['+ @owner + '].[' + @tableName +'] {(enabled ? "CHECK" : "NOCHECK")} CONSTRAINT ALL');
        EXEC('ALTER TABLE ['+ @owner + '].[' + @tableName +'] {(enabled ? "ENABLE" : "DISABLE")} TRIGGER ALL');

        FETCH NEXT FROM table_cursor INTO @owner,@tableName  
    END  
  
    CLOSE table_cursor  
    DEALLOCATE table_cursor   
END";

            yield return new ExecuteProcedureScript(sql);
        }

        #endregion
    }
}
