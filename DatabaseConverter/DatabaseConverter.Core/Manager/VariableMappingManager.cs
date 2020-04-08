using DatabaseConverter.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseConverter.Core
{
    public class VariableMappingManager : MappingManager
    {
        public static string FunctionMappingFilePath { get { return Path.Combine(ConfigRootFolder, "VariableMapping.xml"); } }

        public static List<IEnumerable<VariableMapping>> GetVariableMappings()
        {
            XDocument functionMappingDoc = XDocument.Load(FunctionMappingFilePath);
            return functionMappingDoc.Root.Elements("mapping").Select(item =>
            item.Elements().Select(t => new VariableMapping() { DbType = t.Name.ToString(), Variable = t.Value }))
            .ToList();
        }
    }
}
