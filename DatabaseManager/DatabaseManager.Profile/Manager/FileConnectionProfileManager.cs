using Dapper;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Utility;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseManager.Profile
{
    public class FileConnectionProfileManager : ProfileBaseManager
    {
        public static async Task<IEnumerable<FileConnectionProfileInfo>> GetProfiles(string databaseType)
        {
            return await GetProfiles(new ProfileFilter() { DatabaseType = databaseType });
        }

        public static async Task<FileConnectionProfileInfo> GetProfileById(string id)
        {
            return (await GetProfiles(new ProfileFilter() { Id = id }))?.FirstOrDefault();
        }

        public static async Task<FileConnectionProfileInfo> GetProfileByDatabase(string databaseType, string database)
        {
            return (await GetProfiles(new ProfileFilter() { DatabaseType = databaseType, Database = database }))?.FirstOrDefault();
        }

        private static async Task<IEnumerable<FileConnectionProfileInfo>> GetProfiles(ProfileFilter filter)
        {
            IEnumerable<FileConnectionProfileInfo> profiles = Enumerable.Empty<FileConnectionProfileInfo>();

            if (ExistsProfileDataFile())
            {
                using (var connection = CreateDbConnection())
                {
                    await connection.OpenAsync();

                    SqlBuilder sb = new SqlBuilder();

                    sb.Append(@"SELECT Id,DatabaseType,SubType,Database,DatabaseVersion,EncryptionType,HasPassword,Password,Name 
                                FROM FileConnection
                                WHERE 1=1"
                    );

                    string dbType = filter?.DatabaseType;
                    string id = filter?.Id;
                    string database = filter?.Database;

                    Dictionary<string, object> para = new Dictionary<string, object>();                   

                    if (!string.IsNullOrEmpty(dbType))
                    {
                        sb.Append("AND DatabaseType=@DbType");

                        para.Add("@DbType", filter.DatabaseType);
                    }

                    if (!string.IsNullOrEmpty(id))
                    {
                        sb.Append("AND Id=@Id");
                        para.Add("@Id", id);
                    }

                    if (!string.IsNullOrEmpty(database))
                    {
                        sb.Append("AND Database=@Database");

                        para.Add("@Database", database);
                    }

                    profiles = await connection.QueryAsync<FileConnectionProfileInfo>(sb.Content, para);

                    foreach (var profile in profiles)
                    {
                        if (!string.IsNullOrEmpty(profile.Password))
                        {
                            profile.Password = AesHelper.Decrypt(profile.Password);
                        }
                    }
                }
            }

            return profiles;
        }

        public static async Task<string> Save(FileConnectionProfileInfo info, bool rememberPassword)
        {
            if (ExistsProfileDataFile() && info != null)
            {
                FileConnectionProfileInfo oldProfile = null;
                
                if(!string.IsNullOrEmpty(info.Id))
                {
                    oldProfile = await GetProfileById(info.Id);
                }
                else if(!string.IsNullOrEmpty(info.Database))
                {
                    oldProfile = await GetProfileByDatabase(info.DatabaseType, info.Database);
                }                 

                string password = info.Password;

                if (!string.IsNullOrEmpty(password) && rememberPassword)
                {
                    password = AesHelper.Encrypt(password);
                }
                else
                {
                    password = null;
                }

                using (var connection = CreateDbConnection())
                {
                    await connection.OpenAsync();

                    var trans = await connection.BeginTransactionAsync();

                    string id = null;

                    int result = -1;

                    if (oldProfile == null)
                    {
                        id = Guid.NewGuid().ToString();

                        string sql = $@"INSERT INTO FileConnection(Id,DatabaseType,SubType,Database,DatabaseVersion,EncryptionType,HasPassword,Password,Name)
                                       VALUES('{id}',@DbType,@SubType,@Database,@DatabaseVersion,@EncryptionType,{ValueHelper.BooleanToInteger(info.HasPassword)},@Password,@Name)";

                        var cmd = connection.CreateCommand();
                        cmd.CommandText = sql;

                        cmd.Parameters.AddWithValue("@DbType", info.DatabaseType);
                        cmd.Parameters.AddWithValue("@SubType", GetParameterValue(info.SubType));
                        cmd.Parameters.AddWithValue("@Database", GetParameterValue(info.Database));
                        cmd.Parameters.AddWithValue("@DatabaseVersion", GetParameterValue(info.DatabaseVersion));
                        cmd.Parameters.AddWithValue("@EncryptionType", GetParameterValue(info.EncryptionType));
                        cmd.Parameters.AddWithValue("@Password", GetParameterValue(password));
                        cmd.Parameters.AddWithValue("@Name", GetParameterValue(info.Name));

                        result = await cmd.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        id = oldProfile.Id;

                        string sql = @"UPDATE FileConnection SET SubType=SubType,Database=@Database,DatabaseVersion=@DatabaseVersion,
                                     EncryptionType=@EncryptionType,Password=@Password,Name=@Name
                                     WHERE ID=@Id";

                        var cmd = connection.CreateCommand();
                        cmd.CommandText = sql;

                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@SubType", GetParameterValue(info.SubType));
                        cmd.Parameters.AddWithValue("@Database", GetParameterValue(info.Database));
                        cmd.Parameters.AddWithValue("@DatabaseVersion", GetParameterValue(info.DatabaseVersion));
                        cmd.Parameters.AddWithValue("@EncryptionType", GetParameterValue(info.EncryptionType));
                        cmd.Parameters.AddWithValue("@Password", GetParameterValue(password));
                        cmd.Parameters.AddWithValue("@Name", GetParameterValue(info.Name));

                        result = await cmd.ExecuteNonQueryAsync();
                    }

                    if (result > 0)
                    {
                        await trans.CommitAsync();

                        return id;
                    }
                }
            }

            return string.Empty;
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

                    string sql = $@"DELETE FROM FileConnection
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
