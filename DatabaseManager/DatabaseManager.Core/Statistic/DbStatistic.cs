using Dapper;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Model;
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

        public FeedbackHandler OnFeedback;

        public DbStatistic(DatabaseType databaseType, ConnectionInfo connectionInfo)
        {
            this.databaseType = databaseType;
            this.connectionInfo = connectionInfo;
        }

        public virtual async Task<IEnumerable<TableRecordCount>> CountTableRecords()
        {
            IEnumerable<TableRecordCount> results = Enumerable.Empty<TableRecordCount>();

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.databaseType, this.connectionInfo, new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple });

            using (DbConnection connection = dbInterpreter.CreateConnection())
            {
                this.Feedback("Begin to get tables...");

                var tables = await dbInterpreter.GetTablesAsync(connection);

                this.Feedback($"Got {tables.Count} {(tables.Count > 1 ? "tables" : "table")}.");

                SqlBuilder sb = new SqlBuilder();

                int i = 0;

                foreach (var table in tables)
                {
                    if (i > 0 && i < tables.Count)
                    {
                        sb.Append("UNION ALL");
                    }

                    string tableName = dbInterpreter.GetQuotedDbObjectNameWithSchema(table);

                    sb.Append($"SELECT '{tableName}' AS {dbInterpreter.GetQuotedString("TableName")}, COUNT(1) AS {dbInterpreter.GetQuotedString("RecordCount")} FROM {tableName}");

                    i++;
                }

                this.Feedback("Begin to read records count...");

                results = (await connection.QueryAsync<TableRecordCount>(sb.Content));

                this.Feedback("End read records count.");
            }

            return results;
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
