using DatabaseManager.Model;
using System;
using System.Text;
using DatabaseInterpreter.Model;
using System.Threading.Tasks;
using DatabaseInterpreter.Core;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DatabaseInterpreter.Utility;

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

            throw new NotSupportedException($"Not support diagnose for {diagnoseType}");
        }

        public virtual async Task<DiagnoseResult> DiagnoseNotNullWithEmpty()
        {
            this.Feedback("Begin to diagnose not null fields with empty value...");

            DiagnoseResult result = new DiagnoseResult();

            DbInterpreterOption option = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple };

            DbInterpreter interpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, this.connectionInfo, option);

            this.Feedback("Begin to get table columns...");

            List<TableColumn> columns = await interpreter.GetTableColumnsAsync();

            this.Feedback("End get table columns.");

            var groups = columns.Where(item => DataTypeHelper.IsCharType(item.DataType) && !item.IsNullable)
                         .GroupBy(item => new { item.Owner, item.TableName });

            using (DbConnection dbConnection = interpreter.CreateConnection())
            {
                foreach (var group in groups)
                {
                    foreach (TableColumn column in group)
                    {
                        string countSql = this.GetTableColumnEmptySql(interpreter, column, true);

                        this.Feedback($@"Begin to get invalid record count for column ""{column.Name}"" of table ""{column.TableName}""...");

                        int count = Convert.ToInt32(await interpreter.GetScalarAsync(dbConnection, countSql));

                        this.Feedback($@"End get invalid record count for column ""{column.Name}"" of table ""{column.TableName}"", the count is {count}.");

                        if (count > 0)
                        {
                            result.Details.Add(new DiagnoseResultItem()
                            {
                                DatabaseObject = column,
                                RecordCount = count,
                                Sql = this.GetTableColumnEmptySql(interpreter, column, false)
                            });
                        }
                    }
                }
            }

            this.Feedback("End diagnose not null fields with empty value.");

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
                         .GroupBy(item => new { item.Owner, item.TableName });

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

            throw new NotImplementedException($"Not implemente diagnosis for {databaseType}.");
        }

        protected virtual string GetTableColumnEmptySql(DbInterpreter interpreter, TableColumn column, bool isCount)
        {
            string tableName = $"{column.Owner}.{interpreter.GetQuotedString(column.TableName)}";
            string selectColumn = isCount ? $"{this.GetStringNullFunction()}(COUNT(1),0) AS {interpreter.GetQuotedString("Count")}" : "*";

            string sql = $"SELECT {selectColumn} FROM {tableName} WHERE {this.GetStringLengthFunction()}({interpreter.GetQuotedString(column.Name)})=0";

            return sql;
        }

        protected virtual string GetTableColumnReferenceSql(DbInterpreter interpreter, TableForeignKey foreignKey, bool isCount)
        {
            string tableName = $"{foreignKey.Owner}.{interpreter.GetQuotedString(foreignKey.TableName)}";
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
