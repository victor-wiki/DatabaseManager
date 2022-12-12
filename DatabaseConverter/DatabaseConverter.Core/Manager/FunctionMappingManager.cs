using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseConverter.Core
{
    public class FunctionMappingManager : ConfigManager
    {
        public static string FunctionMappingFilePath { get { return Path.Combine(ConfigRootFolder, "FunctionMapping.xml"); } }

        public static List<IEnumerable<FunctionMapping>> _functionMappings;

        public static List<IEnumerable<FunctionMapping>> FunctionMappings
        {
            get
            {
                if (_functionMappings == null)
                {
                    _functionMappings = GetFunctionMappings();
                }

                return _functionMappings;
            }
        }

        public static List<IEnumerable<FunctionMapping>> GetFunctionMappings()
        {
            XDocument functionMappingDoc = XDocument.Load(FunctionMappingFilePath);

            return functionMappingDoc.Root.Elements("mapping").Select(item =>
            item.Elements().Select(t =>
            new FunctionMapping()
            {
                DbType = t.Name.ToString(),
                Function = t.Value,
                Direction = ParseDirection(t),
                Args = t.Attribute("args")?.Value,
                IsFixedArgs = ValueHelper.IsTrueValue(t.Attribute("isFixedArgs")?.Value),
                Expression = t.Attribute("expression")?.Value,
                Replacements = t.Attribute("replacements")?.Value,
                Defaults = t.Attribute("defaults")?.Value,
                Translator = t.Attribute("translator")?.Value,
                Specials = t.Attribute("specials")?.Value
            }))
            .ToList();
        }

        private static FunctionMappingDirection ParseDirection(XElement element)
        {
            return string.IsNullOrEmpty(element.Attribute("direction")?.Value) ?
                FunctionMappingDirection.INOUT :
               (FunctionMappingDirection)Enum.Parse(typeof(FunctionMappingDirection), element.Attribute("direction").Value.ToUpper());
        }
    }
}
