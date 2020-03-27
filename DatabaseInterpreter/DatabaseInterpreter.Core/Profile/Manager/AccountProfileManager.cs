using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using DatabaseInterpreter.Utility;
using DatabaseInterpreter.Model;
using System;

namespace DatabaseInterpreter.Profile
{
    public class AccountProfileManager : ProfileBaseManager
    {
        public static string ProfilePath => Path.Combine(ProfileFolder, "AccountProfile.json");

        public static Guid Save(AccountProfileInfo info, bool rememberPassword)
        {
            AccountProfileInfo cloneInfo = ObjectHelper.CloneObject<AccountProfileInfo>(info);

            if (!rememberPassword || info.IntegratedSecurity)
            {
                cloneInfo.Password = "";
            }
            else if (rememberPassword && !string.IsNullOrEmpty(info.Password))
            {
                cloneInfo.Password = AesHelper.Encrypt(info.Password);
            }

            List<AccountProfileInfo> profiles = new List<AccountProfileInfo>();

            if (File.Exists(ProfilePath))
            {
                profiles = (List<AccountProfileInfo>)JsonConvert.DeserializeObject(File.ReadAllText(ProfilePath), typeof(List<AccountProfileInfo>));
            }

            AccountProfileInfo oldProfile = profiles.FirstOrDefault(item => item.Id == info.Id);

            Guid id = info.Id;

            if (oldProfile == null)
            {
                profiles.Add(cloneInfo);
            }
            else
            {
                id = oldProfile.Id;

                ObjectHelper.CopyProperties(info, oldProfile);

                if (rememberPassword && !string.IsNullOrEmpty(info.Password))
                {
                    oldProfile.Password = AesHelper.Encrypt(info.Password);
                }
            }

            File.WriteAllText(ProfilePath, JsonConvert.SerializeObject(profiles, Formatting.Indented));

            return id;
        }

        public static IEnumerable<AccountProfileInfo> GetProfiles(string dbType)
        {
            IEnumerable<AccountProfileInfo> profiles = Enumerable.Empty<AccountProfileInfo>();

            if (File.Exists(ProfilePath))
            {
                profiles = ((IEnumerable<AccountProfileInfo>)JsonConvert.DeserializeObject(File.ReadAllText(ProfilePath), typeof(IEnumerable<AccountProfileInfo>)))
                            .Where(item => (item.DatabaseType == dbType || string.IsNullOrEmpty(dbType) ));

                foreach(var profile in profiles)
                {
                    if(!profile.IntegratedSecurity && !string.IsNullOrEmpty(profile.Password))
                    {
                        profile.Password = AesHelper.Decrypt(profile.Password);
                    }
                }
            }

            return profiles;
        }

        public static void Delete(IEnumerable<Guid> ids)
        {
            List<AccountProfileInfo> profiles = new List<AccountProfileInfo>();

            if (File.Exists(ProfilePath))
            {
                profiles = (List<AccountProfileInfo>)JsonConvert.DeserializeObject(File.ReadAllText(ProfilePath), typeof(List<AccountProfileInfo>));
            }

            var oldProfiles = profiles.Where(item => ids.Contains(item.Id));

            if (oldProfiles != null && oldProfiles.Count()>0 )
            {
                profiles.RemoveAll(item=> oldProfiles.Any(t=>item.Id == t.Id));

                var connectionProfiles = ConnectionProfileManager.GetProfiles(null, true).ToList();

                connectionProfiles.RemoveAll(item => ids.Contains(item.AccountProfileId));

                File.WriteAllText(ConnectionProfileManager.ProfilePath, JsonConvert.SerializeObject(connectionProfiles, Formatting.Indented));
            }          

            File.WriteAllText(ProfilePath, JsonConvert.SerializeObject(profiles, Formatting.Indented));
        }
    }
}
