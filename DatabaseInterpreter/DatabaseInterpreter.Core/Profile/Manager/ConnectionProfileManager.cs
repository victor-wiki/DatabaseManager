using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace DatabaseInterpreter.Profile
{
    public class ConnectionProfileManager : ProfileBaseManager
    {
        public static string ProfilePath => Path.Combine(ProfileFolder, "ConnectionProfile.json");

        public static string Save(ConnectionProfileInfo info, bool rememberPassword)
        {
            ConnectionInfo connectionInfo = ObjectHelper.CloneObject<ConnectionInfo>(info.ConnectionInfo);
            info.Database = connectionInfo.Database;

            if (!rememberPassword)
            {
                connectionInfo.Password = "";
            }

            string encrptedPassword = "";

            if (rememberPassword && !string.IsNullOrEmpty(connectionInfo.Password))
            {
                encrptedPassword = AesHelper.Encrypt(connectionInfo.Password);
            }

            string profileName = info.Name;
            if (string.IsNullOrEmpty(profileName))
            {
                profileName = info.ConnectionDescription;
            }

            var accountProfiles = AccountProfileManager.GetProfiles(info.DatabaseType);

            AccountProfileInfo accountProfile = accountProfiles.FirstOrDefault(item => item.Server == connectionInfo.Server
                                                && item.Port == connectionInfo.Port
                                                && item.UserId == connectionInfo.UserId
                                                && item.IntegratedSecurity == connectionInfo.IntegratedSecurity);

            bool changed = false;
            if (accountProfile != null)
            {
                if (!accountProfile.IntegratedSecurity && accountProfile.Password != encrptedPassword)
                {
                    changed = true;
                    accountProfile.Password = connectionInfo.Password;
                }
            }
            else
            {
                changed = true;
                accountProfile = new AccountProfileInfo() { DatabaseType = info.DatabaseType };
                ObjectHelper.CopyProperties(connectionInfo, accountProfile);
            }

            if (changed)
            {
                AccountProfileManager.Save(accountProfile, rememberPassword);
            }

            List<ConnectionProfileInfo> profiles = new List<ConnectionProfileInfo>();

            if (File.Exists(ProfilePath))
            {
                profiles = (List<ConnectionProfileInfo>)JsonConvert.DeserializeObject(File.ReadAllText(ProfilePath), typeof(List<ConnectionProfileInfo>));
            }

            ConnectionProfileInfo oldProfile = profiles.FirstOrDefault(item => item.Name == info.Name && item.DatabaseType == info.DatabaseType);

            if (oldProfile == null)
            {
                info.AccountProfileId = accountProfile.Id;
                profiles.Add(info);
            }
            else
            {
                ObjectHelper.CopyProperties(info, oldProfile);
            }

            File.WriteAllText(ProfilePath, JsonConvert.SerializeObject(profiles, Formatting.Indented));

            return profileName;
        }

        public static IEnumerable<ConnectionProfileInfo> GetProfiles(string dbType, bool isSampleMode = false)
        {
            IEnumerable<ConnectionProfileInfo> profiles = Enumerable.Empty<ConnectionProfileInfo>();

            string filePath = ProfilePath;

            if (File.Exists(filePath))
            {
                profiles = ((IEnumerable<ConnectionProfileInfo>)JsonConvert.DeserializeObject(File.ReadAllText(ProfilePath), typeof(IEnumerable<ConnectionProfileInfo>)))
                    .Where(item => (item.DatabaseType == dbType || string.IsNullOrEmpty(dbType)));

                if (!isSampleMode )
                {
                    var accountProfiles = AccountProfileManager.GetProfiles(dbType);
                    foreach (var profile in profiles)
                    {
                        AccountProfileInfo accountProfile = accountProfiles.FirstOrDefault(item => item.Id == profile.AccountProfileId);

                        if (accountProfile != null)
                        {
                            profile.ConnectionInfo = GetConnectionInfo(accountProfile);
                            profile.ConnectionInfo.Database = profile.Database;
                        }
                    }
                }
            }

            return profiles;
        }

        public static ConnectionInfo GetConnectionInfo(string dbType, string profileName)
        {
            ConnectionInfo connectionInfo = null;

            string filePath = ProfilePath;
            if (File.Exists(filePath))
            {
                IEnumerable<ConnectionProfileInfo> profiles = (IEnumerable<ConnectionProfileInfo>)JsonConvert.DeserializeObject(File.ReadAllText(ProfilePath), typeof(IEnumerable<ConnectionProfileInfo>));

                ConnectionProfileInfo profile = profiles.FirstOrDefault(item => item.DatabaseType == dbType.ToString() && profileName == item.Name);

                if (profile != null)
                {
                    AccountProfileInfo accountProfile = AccountProfileManager.GetProfiles(dbType).FirstOrDefault(item => item.Id == profile.AccountProfileId);

                    if (accountProfile != null)
                    {
                        connectionInfo = GetConnectionInfo(accountProfile);
                    }

                    if (connectionInfo != null)
                    {
                        connectionInfo.Database = profile.Database;
                    }
                }
            }

            return connectionInfo;
        }

        private static ConnectionInfo GetConnectionInfo(AccountProfileInfo accountProfile)
        {
            ConnectionInfo connectionInfo = new ConnectionInfo();

            ObjectHelper.CopyProperties(accountProfile, connectionInfo);

            return connectionInfo;
        }

        public static bool Delete(DatabaseType dbType, string profileName)
        {
            string filePath = ProfilePath;

            if (File.Exists(filePath))
            {
                List<ConnectionProfileInfo> profiles = (List<ConnectionProfileInfo>)JsonConvert.DeserializeObject(File.ReadAllText(ProfilePath), typeof(List<ConnectionProfileInfo>));
                ConnectionProfileInfo oldProfile = profiles.FirstOrDefault(item => item.Name == profileName && item.DatabaseType == dbType.ToString());

                if (oldProfile != null)
                {
                    profiles.Remove(oldProfile);

                    File.WriteAllText(ProfilePath, JsonConvert.SerializeObject(profiles, Formatting.Indented));

                    return true;
                }
            }

            return false;
        }
    }
}
