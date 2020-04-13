using DatabaseConverter.Model;
using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseConverter.Core
{
    public class FunctionManager: ConfigManager
    {
        public static List<FunctionSpecification> GetFunctionSpecifications(DatabaseType dbType)
        {
            string filePath = Path.Combine(ConfigRootFolder, $"Function/{dbType}.xml");

            XDocument doc = XDocument.Load(filePath);

            return doc.Root.Elements("item").Select(item => new FunctionSpecification()
            {
                Name = item.Attribute("name").Value,
                Args = item.Attribute("args").Value,
                Delimiter= item.Attribute("delimiter")?.Value
            }).ToList();
        }
    }
}
