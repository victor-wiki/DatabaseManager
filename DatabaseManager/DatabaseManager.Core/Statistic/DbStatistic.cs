using Dapper;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseManager.Core
{
    public class DbStatistic
    {
        protected DatabaseType databaseType;
        protected ConnectionInfo connectionInfo;
        private IObserver<FeedbackInfo> observer;

        public DbStatistic(DatabaseType databaseType, ConnectionInfo connectionInfo)
        {
            this.databaseType = databaseType;
            this.connectionInfo = connectionInfo;
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public virtual async Task<IEnumerable<TableRecordCount>> CountTableRecords()
        {
            IEnumerable<TableRecordCount> results = Enumerable.Empty<TableRecordCount>();

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.databaseType, this.connectionInfo, new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple });

            try
            {
                using (DbConnection connection = dbInterpreter.CreateConnection())
                {
                    this.FeedbackInfo("Begin to get tables...");

                    var tables = await dbInterpreter.GetTablesAsync(connection);

                    this.FeedbackInfo($"Got {tables.Count} {(tables.Count > 1 ? "tables" : "table")}.");

                    SqlBuilder sb = new SqlBuilder();

                    int i = 0;

                    foreach (var table in tables)
                    {
                        if (i > 0 && i < tables.Count)
                        {
                            sb.Append("UNION ALL");
                        }

                        string schema = table.Schema ?? "";
                        string tableName = dbInterpreter.GetQuotedDbObjectNameWithSchema(table);

                        sb.Append($"SELECT '{schema}' AS {dbInterpreter.GetQuotedString("Schema")},'{table.Name}' AS {dbInterpreter.GetQuotedString("TableName")}, COUNT(1) AS {dbInterpreter.GetQuotedString("RecordCount")} FROM {tableName}");

                        i++;
                    }

                    this.FeedbackInfo("Begin to read records count...");

                    results = (await connection.QueryAsync<TableRecordCount>(sb.Content));

                    this.FeedbackInfo("End read records count.");
                }
            }
            catch (Exception ex)
            {
                this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex));
            }

            return results;
        }

        public virtual async Task<IEnumerable<TableColumnContentMaxLength>> GetTableColumnContentLengths(SchemaInfoFilter filter = null)
        {
            List<TableColumnContentMaxLength> results = new List<TableColumnContentMaxLength>();

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.databaseType, this.connectionInfo, new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple });

            string sql = null;

            try
            {
                using (DbConnection connection = dbInterpreter.CreateConnection())
                {
                    this.FeedbackInfo("Begin to get tables...");

                    var tables = await dbInterpreter.GetTablesAsync(connection, filter);

                    this.FeedbackInfo($"Got {tables.Count} {(tables.Count > 1 ? "tables" : "table")}.");

                    int i = 0;

                    this.FeedbackInfo("Begin to read column length data...");

                    foreach (var table in tables)
                    {
                        string tableName = dbInterpreter.GetQuotedDbObjectNameWithSchema(table);

                        var columns = await dbInterpreter.GetTableColumnsAsync(connection, new SchemaInfoFilter() { Schema = table.Schema, TableNames = [table.Name] });

                        var charColumns = columns.Where(item => DataTypeHelper.IsCharType(item.DataType, this.databaseType == DatabaseType.Sqlite));

                        if (charColumns.Any())
                        {
                            foreach (var column in charColumns)
                            {
                                string columnName = dbInterpreter.GetQuotedString(column.Name);

                                sql = @$"select {dbInterpreter.StringCheckNullFunctionName}(MAX({dbInterpreter.StringLengthFunctionName}({columnName})),0) AS {dbInterpreter.GetQuotedString("ContentMaxLength")} FROM {tableName}";

                                int maxLength = (await connection.QueryAsync<int>(sql)).FirstOrDefault();

                                TableColumnContentMaxLength item = new TableColumnContentMaxLength() { Schema = table.Schema, TableName = table.Name, ColumnName = column.Name, ContentMaxLength = maxLength };

                                results.Add(item);
                            }
                        }

                        i++;
                    }

                    this.FeedbackInfo("End read column length data.");
                }
            }
            catch (Exception ex)
            {
                this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex));
            }

            return results;
        }

        public void Feedback(FeedbackInfoType infoType, string message)
        {
            FeedbackInfo info = new FeedbackInfo() { Owner = this, InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(message) };

            this.Feedback(info);
        }

        public void Feedback(FeedbackInfo info)
        {
            if (this.observer != null)
            {
                FeedbackHelper.Feedback(this.observer, info);
            }
        }

        public void FeedbackInfo(string message)
        {
            this.Feedback(FeedbackInfoType.Info, message);
        }

        public void FeedbackError(string message, bool skipError = false)
        {
            this.Feedback(new FeedbackInfo() { InfoType = FeedbackInfoType.Error, Message = message, IgnoreError = skipError });
        }
    }
}
