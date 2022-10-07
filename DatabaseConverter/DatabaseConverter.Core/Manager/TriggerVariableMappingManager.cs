using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseConverter.Core
{
    public class TriggerVariableMappingManager : ConfigManager
    {
        public static string TriggerVariableMappingFilePath { get { return Path.Combine(ConfigRootFolder, "TriggerVariableMapping.xml"); } }

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
            XDocument doc = XDocument.Load(TriggerVariableMappingFilePath);
            return doc.Root.Elements("mapping").Select(item =>
            item.Elements().Select(t => new VariableMapping() { DbType = t.Name.ToString(), Variable = t.Value }))
            .ToList();
        }
    }
}
