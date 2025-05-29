using Dapper;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Profile.Model;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DatabaseManager.Profile.Manager
{
    public class ProfileBaseManager
    {
        private readonly static string dataFileName = "profiles.db3";
        internal static string ProfileFolder => "Profiles";
        internal static string ProfileDataFile { get; private set; }

        static ProfileBaseManager()
        {
           
        }

        public static async void Init()
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string folder = Path.Combine(assemblyFolder, ProfileFolder);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string dataFilePath = Path.Combine(folder, dataFileName);

            string templateFilePath = Path.Combine(assemblyFolder, "Template", dataFileName);

            if (!File.Exists(templateFilePath))
            {
                return;
            }

            if (!File.Exists(dataFilePath))
            {
                if (!File.Exists(templateFilePath))
                {
                    throw new FileNotFoundException($@"File ""{templateFilePath}"" is not found.");
                }

                File.Copy(templateFilePath, dataFilePath);

                ProfileDataFile = dataFilePath;
            }
            else
            {
                string templateVersion = GetVersion(templateFilePath);
                string dataVersion = GetVersion(dataFilePath);

                if (!string.IsNullOrEmpty(templateVersion) && !string.IsNullOrEmpty(dataVersion) && templateVersion != dataVersion)
                {
                    string backupFileName = $"{Path.GetFileNameWithoutExtension(dataFilePath)}_{(DateTime.Now.ToString("yyyyMMddHH"))}{Path.GetExtension(dataFilePath)}";
                    string backupFilePath = Path.Combine(Path.GetDirectoryName(dataFilePath), backupFileName);

                    File.Copy(dataFilePath, backupFilePath, true);

                    File.Copy(templateFilePath, dataFilePath, true);

                    await ProfileDataManager.KeepUserData(backupFilePath, dataFilePath);
                }

                ProfileDataFile = dataFilePath;
            }
        }

        private static string GetVersion(string dataFilePath)
        {
            using (var connection = CreateDbConnection(dataFilePath))
            {
                connection.Open();

                string sql = "SELECT Version FROM VersionInfo";

                var cmd = connection.CreateCommand();

                cmd.CommandText = sql;

                return cmd.ExecuteScalar()?.ToString();
            }
        }

        private static ConnectionInfo GetConnectionInfo(string dataFilePath)
        {
            return new ConnectionInfo() { Database = dataFilePath };
        }

        protected static ConnectionInfo GetConnectionInfo()
        {
            return GetConnectionInfo(ProfileDataFile);
        }

        internal static DbInterpreter GetDbInterpreter(string dataFilePath = null)
        {
            if (dataFilePath == null)
            {
                dataFilePath = ProfileDataFile;
            }

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(DatabaseType.Sqlite, GetConnectionInfo(dataFilePath), new DbInterpreterOption());

            return dbInterpreter;
        }

        internal static SqliteConnection CreateDbConnection(string dataFilePath = null)
        {
            if (dataFilePath == null)
            {
                dataFilePath = ProfileDataFile;
            }

            return GetDbInterpreter(dataFilePath).CreateConnection() as SqliteConnection;
        }

        protected static object GetParameterValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return DBNull.Value;
            }

            return value;
        }

        protected static bool ExistsProfileDataFile()
        {
            return File.Exists(ProfileDataFile);
        }

        protected static bool ValidateIds(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                return true;
            }

            if (ids.Any(item => !Guid.TryParse(item, out _)))
            {
                throw new ArgumentException("Invalid id exists.");
            }

            return true;
        }

        public static async Task<int> UpdatePriorities(ProfileType profileType, Dictionary<string, int> dictPriorites)
        {
            if (dictPriorites == null)
            {
                return 0;
            }

            if (ExistsProfileDataFile())
            {
                using (var connection = CreateDbConnection())
                {
                    await connection.OpenAsync();

                    var trans = await connection.BeginTransactionAsync();

                    int affectedRows = 0;

                    foreach (var kvp in dictPriorites)
                    {
                        string sql = $"UPDATE {profileType.ToString()} SET Priority={kvp.Value} WHERE Id=@Id";

                        Dictionary<string, object> para = new Dictionary<string, object>();
                        para.Add("@Id", kvp.Key);

                        affectedRows += await connection.ExecuteAsync(sql, para);
                    }

                    await trans.CommitAsync();

                    return affectedRows;
                }
            }

            return 0;
        }
    }
}
