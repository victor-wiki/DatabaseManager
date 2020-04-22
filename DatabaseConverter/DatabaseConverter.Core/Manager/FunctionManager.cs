using DatabaseConverter.Model;
using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseConverter.Core
{
    public class FunctionManager : ConfigManager
    {
        private static Dictionary<DatabaseType, List<FunctionSpecification>> _functionSpecifications;      

        public static List<FunctionSpecification> GetFunctionSpecifications(DatabaseType dbType)
        {
            if(_functionSpecifications!=null && _functionSpecifications.ContainsKey(dbType))
            {
                return _functionSpecifications[dbType];
            }

            string filePath = Path.Combine(ConfigRootFolder, $"FunctionSpecification/{dbType}.xml");

            XDocument doc = XDocument.Load(filePath);

            var functionSpecs = doc.Root.Elements("item").Select(item => new FunctionSpecification()
            {
                Name = item.Attribute("name").Value,
                Args = item.Attribute("args").Value,
                Delimiter = item.Attribute("delimiter")?.Value,
                NoParenthesess = item.Attribute("noParenthesess")?.Value == "1"
            }).ToList();

            if(_functionSpecifications==null)
            {
                _functionSpecifications = new Dictionary<DatabaseType, List<FunctionSpecification>>();               
            }

            _functionSpecifications.Add(dbType, functionSpecs);

            return functionSpecs;
        }      
    }
}
