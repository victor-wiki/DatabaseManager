using Dapper;
using DatabaseInterpreter.Core;
using DatabaseManager.Profile.Model;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseManager.Profile.Manager
{
    public class DatabaseVisibilityManager : ProfileBaseManager
    {
        public static async Task<IEnumerable<DatabaseVisibilityInfo>> GetVisibilities(string accountId)
        {
            IEnumerable<DatabaseVisibilityInfo> visibilities = Enumerable.Empty<DatabaseVisibilityInfo>();

            if (ExistsProfileDataFile())
            {
                using (var connection = CreateDbConnection())
                {
                    await connection.OpenAsync();

                    SqlBuilder sb = new SqlBuilder();

                    sb.Append(@"SELECT Id,AccountId,Database,Hidden FROM DatabaseVisibility
                                WHERE AccountId=@AccountId");

                    Dictionary<string, object> para = new Dictionary<string, object>();

                    sb.Append("AND AccountId=@AccountId");

                    para.Add("@AccountId", accountId);

                    visibilities = await connection.QueryAsync<DatabaseVisibilityInfo>(sb.Content, para);
                }
            }

            return visibilities;
        }

        public static async Task<bool> Save(string accountId, IEnumerable<DatabaseVisibilityInfo> visibilities)
        {
            if (visibilities == null)
            {
                return false;
            }

            var ids = visibilities.Select(item => item.Id).Union(visibilities.Select(item => item.AccountId));

            if (!ValidateIds(ids))
            {
                return false;
            }

            if (ExistsProfileDataFile())
            {
                var oldRecords = await GetVisibilities(accountId);

                List<DatabaseVisibilityInfo> inserts = new List<DatabaseVisibilityInfo>();
                List<DatabaseVisibilityInfo> deletes = new List<DatabaseVisibilityInfo>();           

                foreach (var oldRecord in oldRecords)
                {
                    var record = visibilities.FirstOrDefault(item => item.Id == oldRecord.Id);

                    if (record != null)
                    {
                        if (record.Hidden != oldRecord.Hidden)
                        {
                            deletes.Add(record);
                        }
                    }                    
                }

                foreach (var visibility in visibilities)
                {
                    if (!oldRecords.Any(item => item.Id == visibility.Id))
                    {
                        inserts.Add(visibility);
                    }
                }

                using (var connection = CreateDbConnection())
                {
                    await connection.OpenAsync();

                    var trans = await connection.BeginTransactionAsync();
        
                    SqlBuilder sbDelete = new SqlBuilder();
                    SqlBuilder sbInsert = new SqlBuilder();                   

                    if (deletes.Count > 0)
                    {
                        var visibleIds = deletes.Where(item => !item.Hidden).Select(item => item.Id);                       

                        string strVisibleIds = string.Join(",", visibleIds.Select(item => $"'{item}'"));

                        if(!string.IsNullOrEmpty(strVisibleIds))
                        {
                            sbDelete.Append($"DELETE FROM DatabaseVisibility WHERE Id IN({strVisibleIds});");
                        }                      
                    }

                    if (inserts.Count > 0)
                    {
                        sbInsert.Append($"INSERT INTO DatabaseVisibility(Id,AccountId,Database,Hidden) VALUES");

                        int i = 0;

                        foreach (var insert in inserts)
                        {
                            sbInsert.Append($"('{insert.Id}','{insert.AccountId}','{insert.Database}',{insert.Hidden}){(i < inserts.Count - 1 ? "," : "")}");

                            i++;
                        }
                    }

                    int count = 0;                   

                    if (deletes.Count > 0)
                    {
                        string[] commands = sbDelete.Content.Split(';');

                        foreach (var command in commands)
                        {
                            if(!string.IsNullOrEmpty(command))
                            {
                                count += await ExecuteCommand(connection, command);
                            }                            
                        }
                    }

                    if (inserts.Count > 0)
                    {
                        count += await ExecuteCommand(connection, sbInsert.Content);
                    }

                    if (count > 0)
                    {
                        await trans.CommitAsync();

                        return true;
                    }
                }
            }

            return false;
        }

        public static async Task<bool> Delete(IEnumerable<string> ids)
        {
            if (!ValidateIds(ids))
            {
                return false;
            }

            if (ExistsProfileDataFile())
            {
                using (var connection = CreateDbConnection())
                {
                    await connection.OpenAsync();

                    var trans = await connection.BeginTransactionAsync();

                    string strIds = string.Join(",", ids.Select(item => $"'{item}'"));

                    string sql = $@"DELETE FROM DatabaseVisibility
                                    WHERE Id IN({strIds})";

                    var cmd = connection.CreateCommand();
                    cmd.CommandText = sql;

                    int result = await cmd.ExecuteNonQueryAsync();

                    if (result > 0)
                    {
                        await trans.CommitAsync();

                        return true;
                    }
                }
            }

            return false;
        }

        private static async Task<int> ExecuteCommand(SqliteConnection connection, string commandText)
        {
            var cmd = connection.CreateCommand();

            cmd.CommandText = commandText;

            return await cmd.ExecuteNonQueryAsync();
        }
    }
}
