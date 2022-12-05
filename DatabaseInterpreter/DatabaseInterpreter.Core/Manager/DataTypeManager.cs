using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseInterpreter.Core
{
    public class DataTypeManager : ConfigManager
    {
        public const char ArugumentRangeItemDelimiter = ',';
        public const char ArugumentRangeValueDelimiter = '~';
        private static Dictionary<DatabaseType, List<DataTypeSpecification>> _dataTypeSpecifications;

        public static IEnumerable<DataTypeSpecification> GetDataTypeSpecifications(DatabaseType databaseType)
        {
            if (_dataTypeSpecifications != null && _dataTypeSpecifications.ContainsKey(databaseType))
            {
                return _dataTypeSpecifications[databaseType];
            }

            string filePath = Path.Combine(ConfigRootFolder, $"DataTypeSpecification/{databaseType}.xml");

            if (!File.Exists(filePath))
            {
                return Enumerable.Empty<DataTypeSpecification>();
            }

            XDocument doc = XDocument.Load(filePath);

            var functionSpecs = doc.Root.Elements("item").Select(item => new DataTypeSpecification()
            {
                Name = item.Attribute("name").Value,
                Format = item.Attribute("format")?.Value,
                Args = item.Attribute("args")?.Value,
                Range = item.Attribute("range")?.Value,
                Optional = IsTrueValue(item.Attribute("optional")),
                Default = item.Attribute("default")?.Value,
                DisplayDefault = item.Attribute("displayDefault")?.Value,
                AllowMax = IsTrueValue(item.Attribute("allowMax")),
                MapTo = item.Attribute("mapTo")?.Value,
                IndexForbidden = IsTrueValue(item.Attribute("indexForbidden")),
                AllowIdentity = IsTrueValue(item.Attribute("allowIdentity"))
            }).ToList();

            functionSpecs.ForEach(item => ParseArgument(item));

            if (_dataTypeSpecifications == null)
            {
                _dataTypeSpecifications = new Dictionary<DatabaseType, List<DataTypeSpecification>>();
            }

            _dataTypeSpecifications.Add(databaseType, functionSpecs);

            return functionSpecs;
        }

        public static DataTypeSpecification GetDataTypeSpecification(DatabaseType databaseType, string dataType)
        {
            return DataTypeManager.GetDataTypeSpecifications(databaseType).FirstOrDefault(item => item.Name.ToLower() == dataType.ToLower().Trim());
        }

        private static bool IsTrueValue(XAttribute attribute)
        {
            return ValueHelper.IsTrueValue(attribute?.Value);
        }

        public static DataTypeSpecification ParseArgument(DataTypeSpecification dataTypeSpecification)
        {
            if (string.IsNullOrEmpty(dataTypeSpecification.Args) || dataTypeSpecification.Arugments.Count > 0)
            {
                return dataTypeSpecification;
            }

            if (!string.IsNullOrEmpty(dataTypeSpecification.Range))
            {
                string[] argItems = dataTypeSpecification.Args.Split(ArugumentRangeItemDelimiter);
                string[] rangeItems = dataTypeSpecification.Range.Split(ArugumentRangeItemDelimiter);

                int i = 0;
                foreach (string argItem in argItems)
                {
                    DataTypeArgument argument = new DataTypeArgument() { Name = argItem };

                    if (i < rangeItems.Length)
                    {
                        ArgumentRange range = new ArgumentRange();

                        string[] rangeValues = rangeItems[i].Split(ArugumentRangeValueDelimiter);

                        range.Min = int.Parse(rangeValues[0]);

                        if (rangeValues.Length > 1)
                        {
                            range.Max = int.Parse(rangeValues[1]);
                        }
                        else
                        {
                            range.Max = range.Min;
                        }

                        argument.Range = range;
                    }

                    dataTypeSpecification.Arugments.Add(argument);

                    i++;
                }
            }

            return dataTypeSpecification;
        }

        public static ArgumentRange? GetArgumentRange(DataTypeSpecification dataTypeSpecification, string argumentName)
        {
            ArgumentRange? range = default(ArgumentRange?);

            if (dataTypeSpecification.Arugments.Any(item => item.Name.ToLower() == argumentName.ToLower()))
            {
                return dataTypeSpecification.Arugments.FirstOrDefault(item => item.Name.ToLower() == argumentName.ToLower()).Range;
            }

            return range;
        }
    }
}
