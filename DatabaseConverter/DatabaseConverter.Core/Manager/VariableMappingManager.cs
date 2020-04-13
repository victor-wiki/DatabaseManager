using DatabaseConverter.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseConverter.Core
{
    public class VariableMappingManager : ConfigManager
    {
        public static string FunctionMappingFilePath { get { return Path.Combine(ConfigRootFolder, "VariableMapping.xml"); } }

        public static List<IEnumerable<VariableMapping>> GetVariableMappings()
        {
            XDocument doc = XDocument.Load(FunctionMappingFilePath);
            return doc.Root.Elements("mapping").Select(item =>
            item.Elements().Select(t => new VariableMapping() { DbType = t.Name.ToString(), Variable = t.Value }))
            .ToList();
        }
    }
}
