using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DatabaseInterpreter.Core
{
    public class KeywordManager: ConfigManager
    {
        public static readonly string KeywordFolder = Path.Combine(ConfigRootFolder, "Keyword");

        public static IEnumerable<string> GetKeywords(DatabaseType databaseType)
        {
            string filePath = Path.Combine(KeywordFolder, $"{databaseType}.txt");

            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath).Where(item => item.Length > 0);
            }

            return Enumerable.Empty<string>();
        }
    }
}
