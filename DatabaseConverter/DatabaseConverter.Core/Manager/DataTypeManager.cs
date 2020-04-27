using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DatabaseConverter.Core
{
    public class DataTypeManager : ConfigManager
    {
        public static readonly string KeywordFolder = Path.Combine(ConfigRootFolder, "DataType");

        public static IEnumerable<string> GetDataTypes(DatabaseType databaseType)
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
