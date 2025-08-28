using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseConverter.Core
{
    public class DataTypeMappingManager : ConvertConfigManager
    {
        private static Dictionary<string, List<DataTypeMapping>> _dataTypeMappings;

        public static string DataTypeMappingFolderName = "DataTypeMapping";

        public static string DataTypeMappingFolder => Path.Combine(ConfigRootFolder, DataTypeMappingFolderName);

        public static string CustomDataTypeMappingFolder => Path.Combine(CustomConfigRootFolder, "DataTypeMapping");

        public static string GetDataTypeMappingFilePath(DatabaseType sourceDatabaseType, DatabaseType targetDatabaseType)
        {
            return Path.Combine(DataTypeMappingFolder, $"{sourceDatabaseType}2{targetDatabaseType}.xml");
        }

        public static string GetDataTypeMappingCustomSubFolder(DatabaseType sourceDatabaseType, DatabaseType targetDatabaseType, string folder = null)
        {
            if(folder == null)
            {
                folder = CustomDataTypeMappingFolder;
            }

            return Path.Combine(folder, $"{sourceDatabaseType}2{targetDatabaseType}");
        }

        public static IEnumerable<string> GetCustomDataTypeMappingFileNames(string folder)
        {
            return GetFileNames(folder);
        }

        public static List<DataTypeMapping> GetDataTypeMappings(DatabaseType sourceDatabaseType, DatabaseType targetDatabaseType, string filePath = null)
        {
            if (filePath == null)
            {
                filePath = GetDataTypeMappingFilePath(sourceDatabaseType, targetDatabaseType);
            }

            if (!File.Exists(filePath))
            {
                throw new Exception($"No such file:{filePath}");
            }

            if (_dataTypeMappings != null && _dataTypeMappings.ContainsKey(filePath))
            {
                return _dataTypeMappings[filePath];
            }           

            XDocument dataTypeMappingDoc = XDocument.Load(filePath);

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
                _dataTypeMappings = new Dictionary<string, List<DataTypeMapping>>();
            }

            _dataTypeMappings.Add(filePath, mappings);

            return mappings;
        }

        private static DataTypeMappingTarget ParseTarget(XElement element)
        {
            DataTypeMappingTarget target = new DataTypeMappingTarget(element);

            if (!string.IsNullOrEmpty(target.Args))
            {
                string[] items = target.Args.Split(',');

                foreach (string item in items)
                {
                    string[] nvs = item.Split(':');

                    DataTypeMappingArgument arg = new DataTypeMappingArgument() { Name = nvs[0], Value = nvs[1] };

                    target.Arguments.Add(arg);
                }
            }

            return target;
        }

        public static void SaveDataTypeMappings(DatabaseType sourceDatabaseType, DatabaseType targetDatabaseType, List<DataTypeMapping> mappings, string filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = GetDataTypeMappingFilePath(sourceDatabaseType, targetDatabaseType);
            }

            string folder = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            XDocument doc = new XDocument();

            var rootElement = new XElement("mappings");

            doc.Add(rootElement);

            foreach (var mapping in mappings)
            {
                var source = mapping.Source;
                var target = mapping.Target;
                var specials = mapping.Specials;

                var mappingElement = new XElement("mapping");

                var sourceElement = new XElement("source");
                var targetElement = new XElement("target");

                sourceElement.Add(new XAttribute("type", source.Type));

                if (source.IsExpression)
                {
                    sourceElement.Add(new XAttribute("isExp", "true"));
                }

                targetElement.Add(new XAttribute("type", target.Type));

                if (!IsNullOrEmpty(target.Length))
                {
                    targetElement.Add(new XAttribute("length", target.Length));
                }

                if (!IsNullOrEmpty(target.Precision))
                {
                    targetElement.Add(new XAttribute("precision", target.Precision));
                }

                if (!IsNullOrEmpty(target.Scale))
                {
                    targetElement.Add(new XAttribute("scale", target.Scale));
                }

                if (!IsNullOrEmpty(target.Substitute))
                {
                    targetElement.Add(new XAttribute("substitute", target.Substitute));
                }

                if (!IsNullOrEmpty(target.Args))
                {
                    targetElement.Add(new XAttribute("args", target.Args));
                }

                mappingElement.Add(sourceElement);
                mappingElement.Add(targetElement);

                if (specials != null && specials.Count > 0)
                {
                    foreach (var special in specials)
                    {
                        var specialElement = new XElement("special");

                        specialElement.Add(new XAttribute("name", special.Name));

                        if (!IsNullOrEmpty(special.Value))
                        {
                            specialElement.Add(new XAttribute("value", special.Value));
                        }

                        if (!IsNullOrEmpty(special.Type))
                        {
                            specialElement.Add(new XAttribute("type", special.Type));
                        }

                        if (!IsNullOrEmpty(special.TargetMaxLength))
                        {
                            specialElement.Add(new XAttribute("targetMaxLength", special.TargetMaxLength));
                        }

                        if (!IsNullOrEmpty(special.Substitute))
                        {
                            specialElement.Add(new XAttribute("substitute", special.Substitute));
                        }

                        if (special.NoLength)
                        {
                            specialElement.Add(new XAttribute("noLength", special.NoLength.ToString().ToLower()));
                        }

                        if (!IsNullOrEmpty(special.Precision))
                        {
                            specialElement.Add(new XAttribute("precision", special.Precision));
                        }

                        if (!IsNullOrEmpty(special.Scale))
                        {
                            specialElement.Add(new XAttribute("scale", special.Scale));
                        }

                        mappingElement.Add(specialElement);
                    }
                }

                rootElement.Add(mappingElement);
            }

            doc.Save(filePath);

            if(_dataTypeMappings.ContainsKey(filePath))
            {
                _dataTypeMappings[filePath] = mappings;
            }
            else
            {
                _dataTypeMappings.Add(filePath, mappings);
            }
        }

        private static bool IsNullOrEmpty(string value)
        {
            return value == null || value.Trim() == string.Empty;
        }
    }
}
