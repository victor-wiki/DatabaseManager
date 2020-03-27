using DatabaseConverter.Model;
using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseConverter.Core
{
    public class DataTypeMappingManager : MappingManager
    {
        public static List<DataTypeMapping> GetDataTypeMappings(DatabaseType sourceDatabaseType, DatabaseType targetDatabaseType)
        {
            string dataTypeMappingFilePath = Path.Combine(ConfigRootFolder, $"DataTypeMapping/{sourceDatabaseType}2{targetDatabaseType}.xml");

            XDocument dataTypeMappingDoc = XDocument.Load(dataTypeMappingFilePath);

            return dataTypeMappingDoc.Root.Elements("mapping").Select(item =>
             new DataTypeMapping()
             {
                 Source = new DataTypeMappingSource(item),
                 Tareget = new DataTypeMappingTarget(item),
                 Specials = item.Elements("special")?.Select(t => new DataTypeMappingSpecial(t)).ToList()
             })
             .ToList();
        }
    }
}
