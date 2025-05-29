using Dapper;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Profile.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DatabaseManager.Profile.Manager
{
    public class ConnectionProfileManager : ProfileBaseManager
    {

        public static async Task<IEnumerable<ConnectionProfileInfo>> GetProfiles(string databaseType)
        {
            return await GetProfiles(new ProfileFilter() { DatabaseType = databaseType });
        }

        public static async Task<IEnumerable<ConnectionProfileInfo>> GetProfilesByAccountId(string accountId)
        {
            return await GetProfiles(new ProfileFilter() { AccountId = accountId });
        }

        public static async Task<ConnectionProfileInfo> GetProfileById(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return null;
            }

            return (await GetProfiles(new ProfileFilter() { Id = id }))?.FirstOrDefault();
        }

        private static async Task<IEnumerable<ConnectionProfileInfo>> GetProfiles(ProfileFilter filter)
        {
            IEnumerable<ConnectionProfileInfo> profiles = Enumerable.Empty<ConnectionProfileInfo>();

            if (ExistsProfileDataFile())
            {
                using (var connection = CreateDbConnection())
                {
                    await connection.OpenAsync();

                    SqlBuilder sb = new SqlBuilder();

                    sb.Append($@"SELECT c.Id,c.AccountId, DatabaseType,Server,ServerVersion,Port,IntegratedSecurity,UserId,Password,IsDba,UseSsl,
                                c.Name,c.Database,c.Visible,c.Priority
                                FROM Connection c 
                                JOIN Account a ON a.Id=c.AccountId
                                WHERE 1=1");


                    string dbType = filter?.DatabaseType;
                    string accountId = filter?.AccountId;
                    string id = filter?.Id;
                    string name = filter?.Name;

                    Dictionary<string, object> para = new Dictionary<string, object>();

                    if (!string.IsNullOrEmpty(dbType))
                    {
                        sb.Append("AND a.DatabaseType=@DbType");

                        para.Add("@DbType", filter.DatabaseType);
                    }

                    if (!string.IsNullOrEmpty(accountId))
                    {
                        sb.Append("AND AccountId=@AccountId");

                        para.Add("@AccountId", accountId);
                    }

                    if (!string.IsNullOrEmpty(id))
                    {
                        sb.Append("AND c.Id=@Id");

                        para.Add("@Id", id);
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        sb.Append("AND c.Name=@Name");

                        para.Add("@Name", name);
                    }

                    sb.Append(Environment.NewLine + "ORDER BY a.Priority,c.Priority");

                    profiles = await connection.QueryAsync<ConnectionProfileInfo>(sb.Content, para);

                    if (profiles != null)
                    {
                        foreach (var profile in profiles)
                        {
                            if (!string.IsNullOrEmpty(profile.Password))
                            {
                                profile.Password = AesHelper.Decrypt(profile.Password);
                            }
                        }
                    }
                }
            }

            return profiles;
        }     

        public static ConnectionInfo GetConnectionInfoByProfileInfo(ConnectionProfileInfo profile)
        {
            if (profile != null)
            {
                ConnectionInfo connectionInfo = new ConnectionInfo()
                {
                    Server = profile.Server,
                    IntegratedSecurity = profile.IntegratedSecurity,
                    Port = profile.Port,
                    UserId = profile.UserId,
                    Password = profile.Password,
                    IsDba = profile.IsDba,
                    UseSsl = profile.UseSsl,
                    Database = profile.Database,
                    ServerVersion = profile.ServerVersion
                };

                return connectionInfo;
            }

            return null;
        }

        public static async Task<bool> IsNameExisted(bool isAdd, string accountId,  string name, string id)
        {
            if (ExistsProfileDataFile())
            {
                using (var connection = CreateDbConnection())
                {
                    string sql = "SELECT 1 FROM Connection WHERE LOWER(Name)=LOWER(@Name) AND AccountId=@AccountId";

                    if(!isAdd)
                    {
                        sql += " AND Id<>@Id";
                    }

                    Dictionary<string, object> para = new Dictionary<string, object>();

                    para.Add("@AccountId", accountId);
                    para.Add("@Id", id);
                    para.Add("@Name", name);

                    int? result = (await connection.QueryAsync<int>(sql, para))?.FirstOrDefault();

                    return result > 0;
                }
            }

            return false;
        }

        public static async Task<string> Save(ConnectionProfileInfo info, bool rememberPassword)
        {
            string profileId = null;              

            ConnectionProfileInfo oldProfile = null;

            if(!string.IsNullOrEmpty(info.Id))
            {
                oldProfile = await GetProfileById(info.Id);
            }

            using (var connection = CreateDbConnection())
            {
                await connection.OpenAsync();

                var trans = await connection.BeginTransactionAsync();
               
                int affectedRows = 0;

                if (oldProfile == null)
                {
                    string accountProfileId = info.AccountId;

                    if (string.IsNullOrEmpty(accountProfileId))
                    {
                        AccountProfileInfo accountProfile = new AccountProfileInfo()
                        {
                            DatabaseType = info.DatabaseType,
                            Server = info.Server,
                            Port = info.Port,
                            IntegratedSecurity = info.IntegratedSecurity,
                            ServerVersion = info.ServerVersion,
                            UserId = info.UserId,
                            Password = info.Password,
                            IsDba = info.IsDba,
                            UseSsl = info.UseSsl
                        };

                        SaveResultInfo resultInfo = await AccountProfileManager.Save(accountProfile, rememberPassword, connection);

                        accountProfileId = resultInfo.Id;
                    }       
                    
                    string sql = $"SELECT IFNULL(MAX(Priority),0) as MaxId FROM Connection where AccountId=@AccountId";

                    Dictionary<string, object> para = new Dictionary<string, object>();
                    para.Add("@AccountId", accountProfileId);

                    int? maxId = (await connection.QueryAsync<int>(sql, para))?.FirstOrDefault();

                    if (maxId.HasValue == false)
                    {
                        maxId = 0;
                    }

                    int priority = maxId.Value + 1;

                    string id = Guid.NewGuid().ToString();

                    sql = $@"INSERT INTO Connection(Id,AccountId,Name,Database,Priority)
                                    VALUES(@Id,@AccountId,@Name,@Database,{priority})";

                    var cmd = connection.CreateCommand();
                    cmd.CommandText = sql;

                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@AccountId", accountProfileId);
                    cmd.Parameters.AddWithValue("@Name", info.Name);
                    cmd.Parameters.AddWithValue("@Database", info.Database);

                    affectedRows = await cmd.ExecuteNonQueryAsync();        
                    
                    if(affectedRows>0)
                    {
                        profileId = id;
                    }
                }
                else
                {
                    string sql = $@"UPDATE Connection
                                    SET Name=@Name,Database=@Database
                                    WHERE ID=@Id";

                    var cmd = connection.CreateCommand();
                    cmd.CommandText = sql;

                    cmd.Parameters.AddWithValue("@Id", oldProfile.Id);
                    cmd.Parameters.AddWithValue("@Name", info.Name);
                    cmd.Parameters.AddWithValue("@Database", info.Database);

                    affectedRows = await cmd.ExecuteNonQueryAsync();

                    if(affectedRows>0)
                    {
                        profileId = oldProfile.Id;
                    }
                }

                await trans.CommitAsync();

                return profileId;
            }          
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

                    string sql = $@"DELETE FROM Connection
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
    }
}
