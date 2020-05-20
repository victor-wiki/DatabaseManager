using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DatabaseInterpreter.Core;
using DatabaseManager.Model;
using Newtonsoft.Json;

namespace DatabaseManager.Core
{
    public class BackupSettingManager : ConfigManager
    {
        static BackupSettingManager()
        {           
        }

        public static string ConfigFilePath
        {
            get
            {
                return Path.Combine(ConfigRootFolder, "BackupSetting.json");
            }
        }

        public static List<BackupSetting> GetSettings()
        {
            if (File.Exists(ConfigFilePath))
            {
                return  (List<BackupSetting>)JsonConvert.DeserializeObject(File.ReadAllText(ConfigFilePath), typeof(List<BackupSetting>));
            }

            return new List<BackupSetting>();
        }

        public static void SaveConfig(List<BackupSetting> settings)
        {           
            string content = JsonConvert.SerializeObject(settings, Formatting.Indented);

            File.WriteAllText(ConfigFilePath, content);
        }
    }
}
