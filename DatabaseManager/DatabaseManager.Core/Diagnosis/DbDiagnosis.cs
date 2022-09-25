using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;


namespace DatabaseManager.Core
{
    public abstract class DbDiagnosis
    {
        public abstract DatabaseType DatabaseType { get; }
        protected ConnectionInfo connectionInfo;

        public FeedbackHandler OnFeedback;

        public DbDiagnosis(ConnectionInfo connectionInfo)
        {
            this.connectionInfo = connectionInfo;
        }

        public virtual Task<DiagnoseResult> Diagnose(DiagnoseType diagnoseType)
        {
            if (diagnoseType == DiagnoseType.SelfReferenceSame)
            {
                return this.DiagnoseSelfReferenceSame();
            }
            else if (diagnoseType == DiagnoseType.NotNullWithEmpty)
            {
                return this.DiagnoseNotNullWithEmpty();
            }
            else if (diagnoseType == DiagnoseType.WithLeadingOrTrailingWhitespace)
            {
                return this.DiagnoseWithLeadingOrTrailingWhitespace();
            }

            throw new NotSupportedException($"Not support diagnose for {diagnoseType}");
        }

        public virtual async Task<DiagnoseResult> DiagnoseNotNullWithEmpty()
        {
            this.Feedback("Begin to diagnose not null fields with empty value...");

            DiagnoseResult result = await this.DiagnoseTableColumn(DiagnoseType.NotNullWithEmpty);

            this.Feedback("End diagnose not null fields with empty value.");

            return result;
        }


        public virtual async Task<DiagnoseResult> DiagnoseWithLeadingOrTrailingWhitespace()
        {
            this.Feedback("Begin to diagnose character fields with leading or trailing whitespace...");

            DiagnoseResult result = await this.DiagnoseTableColumn(DiagnoseType.WithLeadingOrTrailingWhitespace);     

            this.Feedback("End diagnose character fields with leading or trailing whitespace.");

            return result;
        }

        private async Task<DiagnoseResult> DiagnoseTableColumn(DiagnoseType diagnoseType)
        {
            DiagnoseResult result = new DiagnoseResult();

            DbInterpreterOption option = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple };

            DbInterpreter interpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, this.connectionInfo, option);

            this.Feedback("Begin to get table columns...");

            List<TableColumn> columns = await interpreter.GetTableColumnsAsync();

            this.Feedback("End get table columns.");

            dynamic groups = null;

            if (diagnoseType == DiagnoseType.NotNullWithEmpty)
            {
                groups = columns.Where(item => DataTypeHelper.IsCharType(item.DataType) && !item.DataType.EndsWith("[]") && !item.IsNullable)
                         .GroupBy(item => new { item.Schema, item.TableName });
            }
            else if (diagnoseType == DiagnoseType.WithLeadingOrTrailingWhitespace)
            {
                groups = columns.Where(item => DataTypeHelper.IsCharType(item.DataType) && !item.DataType.EndsWith("[]"))
                         .GroupBy(item => new { item.Schema, item.TableName });
            }


            using (DbConnection dbConnection = interpreter.CreateConnection())
            {
                foreach (var group in groups)
                {
                    foreach (TableColumn column in group)
                    {
                        string countSql = "";
                        
                        if(diagnoseType == DiagnoseType.NotNullWithEmpty)
                        {
                            countSql = this.GetTableColumnWithEmptyValueSql(interpreter, column, true);
                        }
                        else
                        {
                            countSql = this.GetTableColumnWithLeadingOrTrailingWhitespaceSql(interpreter, column, true);
                        }                      

                        this.Feedback($@"Begin to get invalid record count for column ""{column.Name}"" of table ""{column.TableName}""...");

                        int count = Convert.ToInt32(await interpreter.GetScalarAsync(dbConnection, countSql));

                        this.Feedback($@"End get invalid record count for column ""{column.Name}"" of table ""{column.TableName}"", the count is {count}.");

                        if (count > 0)
                        {
                            string sql = "";

                            if (diagnoseType == DiagnoseType.NotNullWithEmpty)
                            {
                                sql = this.GetTableColumnWithEmptyValueSql(interpreter, column, false);
                            }
                            else
                            {
                                sql = this.GetTableColumnWithLeadingOrTrailingWhitespaceSql(interpreter, column, false);
                            }

                            result.Details.Add(new DiagnoseResultItem()
                            {
                                DatabaseObject = column,
                                RecordCount = count,
                                Sql = sql
                            });
                        }
                    }
                }
            }

            return result;
        }

        public virtual async Task<DiagnoseResult> DiagnoseSelfReferenceSame()
        {
            this.Feedback("Begin to diagnose self reference with same value...");

            DiagnoseResult result = new DiagnoseResult();

            DbInterpreterOption option = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Details };

            DbInterpreter interpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, this.connectionInfo, option);

            this.Feedback("Begin to get foreign keys...");

            List<TableForeignKey> foreignKeys = await interpreter.GetTableForeignKeysAsync();

            this.Feedback("End get foreign keys.");

            var groups = foreignKeys.Where(item => item.ReferencedTableName == item.TableName)
                         .GroupBy(item => new { item.Schema, item.TableName });

            using (DbConnection dbConnection = interpreter.CreateConnection())
            {
                foreach (var group in groups)
                {
                    foreach (TableForeignKey foreignKey in group)
                    {
                        string countSql = this.GetTableColumnReferenceSql(interpreter, foreignKey, true);

                        this.Feedback($@"Begin to get invalid record count for foreign key ""{foreignKey.Name}"" of table ""{foreignKey.TableName}""...");

                        int count = Convert.ToInt32(await interpreter.GetScalarAsync(dbConnection, countSql));

                        this.Feedback($@"End get invalid record count for column ""{foreignKey.Name}"" of table ""{foreignKey.TableName}"", the count is {count}.");

                        if (count > 0)
                        {
                            result.Details.Add(new DiagnoseResultItem()
                            {
                                DatabaseObject = foreignKey,
                                RecordCount = count,
                                Sql = this.GetTableColumnReferenceSql(interpreter, foreignKey, false)
                            });
                        }
                    }
                }
            }

            this.Feedback("End diagnose self reference with same value.");

            return result;
        }

        public abstract string GetStringLengthFunction();
        public abstract string GetStringNullFunction();

        public static DbDiagnosis GetInstance(DatabaseType databaseType, ConnectionInfo connectionInfo)
        {
            if (databaseType == DatabaseType.SqlServer)
            {
                return new SqlServerDiagnosis(connectionInfo);
            }
            else if (databaseType == DatabaseType.MySql)
            {
                return new MySqlDiagnosis(connectionInfo);
            }
            else if (databaseType == DatabaseType.Oracle)
            {
                return new OracleDiagnosis(connectionInfo);
            }
            else if(databaseType == DatabaseType.Postgres)
            {
                return new PostgresDiagnosis(connectionInfo);
            }

            throw new NotImplementedException($"Not implemente diagnosis for {databaseType}.");
        }

        protected virtual string GetTableColumnWithEmptyValueSql(DbInterpreter interpreter, TableColumn column, bool isCount)
        {
            string tableName = $"{column.Schema}.{interpreter.GetQuotedString(column.TableName)}";
            string selectColumn = isCount ? $"{this.GetStringNullFunction()}(COUNT(1),0) AS {interpreter.GetQuotedString("Count")}" : "*";

            string sql = $"SELECT {selectColumn} FROM {tableName} WHERE {this.GetStringLengthFunction()}({interpreter.GetQuotedString(column.Name)})=0";

            return sql;
        }

        protected virtual string GetTableColumnWithLeadingOrTrailingWhitespaceSql(DbInterpreter interpreter, TableColumn column, bool isCount)
        {
            string tableName = $"{column.Schema}.{interpreter.GetQuotedString(column.TableName)}";
            string selectColumn = isCount ? $"{this.GetStringNullFunction()}(COUNT(1),0) AS {interpreter.GetQuotedString("Count")}" : "*";
            string columnName = interpreter.GetQuotedString(column.Name);
            string lengthFunName = this.GetStringLengthFunction();

            string sql = $"SELECT {selectColumn} FROM {tableName} WHERE {lengthFunName}(TRIM({columnName}))<{lengthFunName}({columnName})";

            if(interpreter.DatabaseType == DatabaseType.Postgres)
            {
                if(column.DataType == "character")
                {
                    sql += $" OR {lengthFunName}({columnName})<>{column.MaxLength}";
                }
            }

            return sql;
        }

        protected virtual string GetTableColumnReferenceSql(DbInterpreter interpreter, TableForeignKey foreignKey, bool isCount)
        {
            string tableName = $"{foreignKey.Schema}.{interpreter.GetQuotedString(foreignKey.TableName)}";
            string selectColumn = isCount ? $"{this.GetStringNullFunction()}(COUNT(1),0) AS {interpreter.GetQuotedString("Count")}" : "*";
            string whereClause = string.Join(" AND ", foreignKey.Columns.Select(item => $"{interpreter.GetQuotedString(item.ColumnName)}={interpreter.GetQuotedString(item.ReferencedColumnName)}"));

            string sql = $"SELECT {selectColumn} FROM {tableName} WHERE ({whereClause})";

            return sql;
        }

        protected void Feedback(string message)
        {
            if (this.OnFeedback != null)
            {
                this.OnFeedback(new FeedbackInfo() { InfoType = FeedbackInfoType.Info, Message = message, Owner = this });
            }
        }
    }
}
