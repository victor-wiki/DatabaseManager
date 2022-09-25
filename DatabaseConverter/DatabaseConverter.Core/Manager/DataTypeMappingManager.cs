using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System;
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

            if(!File.Exists(dataTypeMappingFilePath))
            {
                throw new Exception($"No such file:{dataTypeMappingFilePath}");
            }

            XDocument dataTypeMappingDoc = XDocument.Load(dataTypeMappingFilePath);

            var mappings = dataTypeMappingDoc.Root.Elements("mapping").Select(item =>
             new DataTypeMapping()
             {
                 Source = new DataTypeMappingSource(item),
                 Target = ParseTarget(item),
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

        private static DataTypeMappingTarget ParseTarget(XElement element)
        {
            DataTypeMappingTarget target = new DataTypeMappingTarget(element);

            if(!string.IsNullOrEmpty(target.Args))
            {
                string[] items = target.Args.Split(',');

                foreach(string item in items)
                {
                    string[] nvs = item.Split(':');

                    DataTypeMappingArgument arg = new DataTypeMappingArgument() { Name=nvs[0], Value=nvs[1] };

                    target.Arguments.Add(arg);
                }
            }

            return target;
        }
    }
}
