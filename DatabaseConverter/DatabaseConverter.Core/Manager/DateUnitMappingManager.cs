using DatabaseConverter.Core.Model;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DatabaseConverter.Core
{
    public class DateUnitMappingManager : ConfigManager
    {
        public static string DateUnitMappingFilePath { get { return Path.Combine(ConfigRootFolder, "DateUnitMapping.xml"); } }

        public static List<DateUnitMapping> _dateUnitMappings;

        public static List<DateUnitMapping> DateUnitMappings
        {
            get
            {
                if (_dateUnitMappings == null)
                {
                    _dateUnitMappings = GetDateUnitMappings();
                }

                return _dateUnitMappings;
            }
        }

        public static List<DateUnitMapping> GetDateUnitMappings()
        {
            List<DateUnitMapping> mappings = new List<DateUnitMapping>();

            XDocument doc = XDocument.Load(DateUnitMappingFilePath);

            var elements = doc.Root.Elements("mapping");

            foreach(var element in elements)
            {
                DateUnitMapping mapping = new DateUnitMapping();

                mapping.Name = element.Attribute("name").Value;

                var items = element.Elements();

                foreach(var item in items)
                {
                    DateUnitMappingItem mappingItem = new DateUnitMappingItem();

                    mappingItem.DbType = item.Name.ToString();
                    mappingItem.Unit = item.Value;
                    mappingItem.CaseSensitive = ValueHelper.IsTrueValue(item.Attribute("caseSensitive")?.Value);
                    mappingItem.Formal = ValueHelper.IsTrueValue(item.Attribute("formal")?.Value);

                    mapping.Items.Add(mappingItem);
                }

                mappings.Add(mapping);
            }

            return mappings;
        }
    }
}
