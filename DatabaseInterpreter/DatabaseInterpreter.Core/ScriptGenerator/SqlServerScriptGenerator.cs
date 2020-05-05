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

                string primaryKey = "";

                IEnumerable<TablePrimaryKey> primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.Owner == table.Owner && item.TableName == tableName);

                #region Primary Key
                if (this.option.TableScriptsGenerateOption.GeneratePrimaryKey && primaryKeys.Count() > 0)
                {
                    primaryKey =
$@"
,CONSTRAINT {this.GetQuotedString(primaryKeys.First().Name)} PRIMARY KEY CLUSTERED 
(
{string.Join(Environment.NewLine, primaryKeys.Select(item => $"{this.GetQuotedString(item.ColumnName)} {(item.IsDesc ? "DESC" : "ASC")},")).TrimEnd(',')}
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]";

                }

                #endregion

                #region Create Table

                string existsClause = $"IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='{(table.Name)}')";

                string tableScript =
$@"
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

{(this.dbInterpreter.NotCreateIfExists ? existsClause : "")}
CREATE TABLE {quotedTableName}(
{string.Join("," + Environment.NewLine, tableColumns.Select(item => this.dbInterpreter.ParseColumn(table, item)))}{primaryKey}
) ON [PRIMARY]{(hasBigDataType ? " TEXTIMAGE_ON [PRIMARY]" : "")}" + ";";

                sb.AppendLine(new CreateDbObjectScript<Table>(tableScript));

                #endregion

                sb.AppendLine();

                #region Comment
                if (!string.IsNullOrEmpty(table.Comment))
                {
                    sb.AppendLine(new ExecuteProcedureScript($"EXECUTE sp_addextendedproperty N'MS_Description',N'{ValueHelper.TransferSingleQuotation(table.Comment)}',N'SCHEMA',N'{table.Owner}',N'table',N'{tableName}',NULL,NULL;"));
                }

                foreach (TableColumn column in tableColumns.Where(item => !string.IsNullOrEmpty(item.Comment)))
                {
                    sb.AppendLine(new ExecuteProcedureScript($"EXECUTE sp_addextendedproperty N'MS_Description',N'{ValueHelper.TransferSingleQuotation(column.Comment)}',N'SCHEMA',N'{table.Owner}',N'table',N'{tableName}',N'column',N'{column.Name}';"));
                }
                #endregion               

                #region Foreign Key
                if (this.option.TableScriptsGenerateOption.GenerateForeignKey)
                {
                    IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.Owner == table.Owner && item.TableName == tableName);
                    if (foreignKeys.Count() > 0)
                    {
                        ILookup<string, TableForeignKey> foreignKeyLookup = foreignKeys.ToLookup(item => item.Name);

                        IEnumerable<string> keyNames = foreignKeyLookup.Select(item => item.Key);

                        foreach (string keyName in keyNames)
                        {
                            TableForeignKey tableForeignKey = foreignKeyLookup[keyName].First();

                            string columnNames = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{ this.GetQuotedString(item.ColumnName)}"));
                            string referenceColumnName = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{ this.GetQuotedString(item.ReferencedColumnName)}"));

                            StringBuilder sbForeignKey = new StringBuilder();

                            sbForeignKey.AppendLine(
$@"
ALTER TABLE {quotedTableName} WITH CHECK ADD CONSTRAINT { this.GetQuotedString(keyName)} FOREIGN KEY({columnNames})
REFERENCES {this.GetQuotedString(table.Owner)}.{this.GetQuotedString(tableForeignKey.ReferencedTableName)} ({referenceColumnName})
");

                            if (tableForeignKey.UpdateCascade)
                            {
                                sbForeignKey.AppendLine("ON UPDATE CASCADE");
                            }

                            if (tableForeignKey.DeleteCascade)
                            {
                                sbForeignKey.AppendLine("ON DELETE CASCADE");
                            }

                            sbForeignKey.AppendLine($"ALTER TABLE {quotedTableName} CHECK CONSTRAINT { this.GetQuotedString(keyName)};");

                            sb.AppendLine(new CreateDbObjectScript<TableForeignKey>(sbForeignKey.ToString()));
                        }
                    }
                }
                #endregion

                #region Index
                if (this.option.TableScriptsGenerateOption.GenerateIndex)
                {
                    IEnumerable<TableIndex> indices = schemaInfo.TableIndexes.Where(item => item.Owner == table.Owner && item.TableName == tableName).OrderBy(item => item.Order);
                    if (indices.Count() > 0)
                    {
                        List<string> indexColumns = new List<string>();
                        ILookup<string, TableIndex> indexLookup = indices.ToLookup(item => item.Name);
                        IEnumerable<string> indexNames = indexLookup.Select(item => item.Key);
                        foreach (string indexName in indexNames)
                        {
                            TableIndex tableIndex = indexLookup[indexName].First();
                            string columnNames = string.Join(",", indexLookup[indexName].Select(item => $"{this.GetQuotedString(item.ColumnName)} {(item.IsDesc ? "DESC" : "ASC")}"));

                            if (indexColumns.Contains(columnNames))
                            {
                                continue;
                            }

                            sb.AppendLine(new CreateDbObjectScript<TableIndex>($"CREATE {(tableIndex.IsUnique ? "UNIQUE" : "")} INDEX {tableIndex.Name} ON {quotedTableName}({columnNames});"));

                            if (!indexColumns.Contains(columnNames))
                            {
                                indexColumns.Add(columnNames);
                            }
                        }
                    }
                }
                #endregion

                #region Default Value
                if (this.option.TableScriptsGenerateOption.GenerateDefaultValue)
                {
                    IEnumerable<TableColumn> defaultValueColumns = schemaInfo.TableColumns.Where(item => item.Owner == table.Owner && item.TableName == tableName && !string.IsNullOrEmpty(item.DefaultValue));
                    foreach (TableColumn column in defaultValueColumns)
                    {
                        sb.AppendLine(new AlterDbObjectScript<Table>($"ALTER TABLE {quotedTableName} ADD CONSTRAINT {this.GetQuotedString($"DF_{tableName}_{column.Name}")}  DEFAULT {this.dbInterpreter.GetColumnDefaultValue(column)} FOR { this.GetQuotedString(column.Name)};"));
                    }
                }
                #endregion

                #region Constraint
                if (this.option.TableScriptsGenerateOption.GenerateConstraint)
                {
                    var constraints = schemaInfo.TableConstraints.Where(item => item.Owner == table.Owner && item.TableName == tableName);

                    foreach (TableConstraint constraint in constraints)
                    {
                        sb.AppendLine(new CreateDbObjectScript<TableConstraint>($"ALTER TABLE {quotedTableName}  WITH CHECK ADD CONSTRAINT {this.GetQuotedString(constraint.Name)} CHECK  ({constraint.Definition});"));
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
    }
}
