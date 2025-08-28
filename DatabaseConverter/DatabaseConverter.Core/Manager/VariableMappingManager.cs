using DatabaseConverter.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseConverter.Core
{
    public class VariableMappingManager : ConvertConfigManager
    {
        public static string VariableMappingFilePath { get { return Path.Combine(ConfigRootFolder, "VariableMapping.xml"); } }

        private static List<IEnumerable<VariableMapping>> _variableMappings;
        public static List<IEnumerable<VariableMapping>> VariableMappings
        {
            get
            {
                if (_variableMappings == null)
                {
                    _variableMappings = GetVariableMappings();
                }

                return _variableMappings;
            }
        }

        public static List<IEnumerable<VariableMapping>> GetVariableMappings()
        {
            XDocument doc = XDocument.Load(VariableMappingFilePath);
            return doc.Root.Elements("mapping").Select(item =>
            item.Elements().Select(t => new VariableMapping() { DbType = t.Name.ToString(), Variable = t.Value }))
            .ToList();
        }
    }
}
