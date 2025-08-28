using DatabaseInterpreter.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DatabaseConverter.Core
{
    public class ConvertConfigManager: ConfigManager
    {
        public static string CustomConfigRootFolder => Path.Combine(ConfigRootFolder, "Custom");

        static ConvertConfigManager()
        {
            if (!Directory.Exists(CustomConfigRootFolder))
            {
                Directory.CreateDirectory(CustomConfigRootFolder);
            }
        }

        public static IEnumerable<string> GetFileNames(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return Enumerable.Empty<string>();
            }

            DirectoryInfo di = new DirectoryInfo(folder);

            if (di.Exists)
            {
                var files = di.GetFiles();

                return files.Select(item => item.Name);
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}
