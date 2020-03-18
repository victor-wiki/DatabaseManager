using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Newtonsoft.Json;
using System.IO;

namespace  DatabaseInterpreter.Core
{
    public class SettingManager
    {
        public static Setting Setting { get; set; } = new Setting();

        static SettingManager()
        {
            LoadConfig();
        }
            
        public static string ConfigFolder
        {
            get
            {
                return Path.Combine(PathHelper.GetAssemblyFolder(), "Config");
            }
        }

        public static string ConfigFilePath
        {
            get
            {                               
                return Path.Combine(ConfigFolder, "Setting.json");
            }
        }

        public static void LoadConfig()
        {
            Setting = (Setting)JsonConvert.DeserializeObject(File.ReadAllText(ConfigFilePath), typeof(Setting));
        }

        public static void SaveConfig(Setting setting)
        {
            Setting = setting;
            string content = JsonConvert.SerializeObject(setting, Formatting.Indented);

            File.WriteAllText(ConfigFilePath, content);
        }
    }
}
