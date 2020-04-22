using DatabaseConverter.Model;
using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseConverter.Core
{
    public class DataTypeMappingManager : ConfigManager
    {
        private static Dictionary<(DatabaseType SourceDbType, DatabaseType TargetDbType), List<DataTypeMapping>> _dataTypeMappings;

        public static List<DataTypeMapping> GetDataTypeMappings(DatabaseType sourceDatabaseType, DatabaseType targetDatabaseType)
        {
            (DatabaseType sourceDbType, DatabaseType targetDbType) dbTypeMap = (sourceDatabaseType, targetDatabaseType);

            if (_dataTypeMappings != null && _dataTypeMappings.ContainsKey(dbTypeMap))
            {
                return _dataTypeMappings[dbTypeMap];
            }

            string dataTypeMappingFilePath = Path.Combine(ConfigRootFolder, $"DataTypeMapping/{sourceDatabaseType}2{targetDatabaseType}.xml");

            XDocument dataTypeMappingDoc = XDocument.Load(dataTypeMappingFilePath);

            var mappings = dataTypeMappingDoc.Root.Elements("mapping").Select(item =>
             new DataTypeMapping()
             {
                 Source = new DataTypeMappingSource(item),
                 Tareget = new DataTypeMappingTarget(item),
                 Specials = item.Elements("special")?.Select(t => new DataTypeMappingSpecial(t)).ToList()
             })
             .ToList();

            if (_dataTypeMappings == null)
            {
                _dataTypeMappings = new Dictionary<(DatabaseType SourceDbType, DatabaseType TargetDbType), List<DataTypeMapping>>();
            }

            _dataTypeMappings.Add(dbTypeMap, mappings);

            return mappings;
        }
    }
}
