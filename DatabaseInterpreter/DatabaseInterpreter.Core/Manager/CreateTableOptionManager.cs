using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseInterpreter.Core
{
    public class CreateTableOptionManager : ConfigManager
    {
        public const char OptionValueItemsSeperator = ';';
        
        private static Dictionary<DatabaseType, CreateTableOption> dictCreateTableOption;

        public static CreateTableOption GetCreateTableOption(DatabaseType databaseType)
        {
            if (dictCreateTableOption != null && dictCreateTableOption.ContainsKey(databaseType))
            {
                return dictCreateTableOption[databaseType];
            }

            string filePath = Path.Combine(ConfigRootFolder, $"Option/CreateTableOption/{databaseType}.xml");

            if (!File.Exists(filePath))
            {
                return null;
            }

            XElement root = XDocument.Load(filePath).Root;

            CreateTableOption option = new CreateTableOption();

            option.Items = root.Elements("item").Select(item => item.Value).ToList();

            if(dictCreateTableOption ==null)
            {
                dictCreateTableOption = new Dictionary<DatabaseType, CreateTableOption>();
            }

            dictCreateTableOption.Add(databaseType, option);

            return option;
        }       
    }
}
