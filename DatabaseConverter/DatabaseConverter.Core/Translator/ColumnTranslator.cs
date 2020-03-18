using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseConverter.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseConverter.Core
{
    public class ColumnTranslator
    {
        private List<TableColumn> columns;
        private DatabaseType sourceDbType;
        private DatabaseType targetDbType;

        public ColumnTranslator(List<TableColumn> columns, DatabaseType sourceDbType, DatabaseType targetDbType)
        {
            this.columns = columns;
            this.sourceDbType = sourceDbType;
            this.targetDbType = targetDbType;
        }

        public List<TableColumn> Translate()
        {
            if (this.sourceDbType == this.targetDbType)
            {
                return this.columns;
            }

            string configRootFolder = Path.Combine(PathHelper.GetAssemblyFolder(), "Config");
            string dataTypeMappingFilePath = Path.Combine(configRootFolder, $"DataTypeMapping/{this.sourceDbType.ToString()}2{this.targetDbType.ToString()}.xml");
            string functionMappingFilePath = Path.Combine(configRootFolder, "FunctionMapping.xml");

            #region DataType Mapping
            XDocument dataTypeMappingDoc = XDocument.Load(dataTypeMappingFilePath);

            List<DataTypeMapping> dataTypeMappings = dataTypeMappingDoc.Root.Elements("mapping").Select(item =>
             new DataTypeMapping()
             {
                 Source = new DataTypeMappingSource(item),
                 Tareget = new DataTypeMappingTarget(item),
                 Specials = item.Elements("special")?.Select(t => new DataTypeMappingSpecial(t)).ToList()
             })
             .ToList();
            #endregion

            #region Function Mapping
            XDocument functionMappingDoc = XDocument.Load(functionMappingFilePath);
            List<IEnumerable<FunctionMapping>> functionMappings = functionMappingDoc.Root.Elements("mapping").Select(item =>
            item.Elements().Select(t => new FunctionMapping() { DbType = t.Name.ToString(), Function = t.Value }))
            .ToList();
            #endregion

            foreach (TableColumn column in this.columns)
            {
                string sourceDataType = this.GetTrimedDataType(column);
                column.DataType = sourceDataType;
                DataTypeMapping dataTypeMapping = dataTypeMappings.FirstOrDefault(item => item.Source.Type?.ToLower() == column.DataType?.ToLower());
                if (dataTypeMapping != null)
                {
                    column.DataType = dataTypeMapping.Tareget.Type;

                    bool isChar = DataTypeHelper.IsCharType(column.DataType);

                    if (isChar)
                    {
                        if (!string.IsNullOrEmpty(dataTypeMapping.Tareget.Length))
                        {
                            column.MaxLength = int.Parse(dataTypeMapping.Tareget.Length);
                        }

                        bool hasSpecial = false;
                        if (dataTypeMapping.Specials != null && dataTypeMapping.Specials.Count > 0)
                        {
                            DataTypeMappingSpecial special = dataTypeMapping.Specials.FirstOrDefault(item => item.SourceMaxLength == column.MaxLength.ToString());
                            if (special != null)
                            {
                                column.DataType = special.Type;
                                hasSpecial = true;

                                if (!string.IsNullOrEmpty(special.TargetMaxLength))
                                {
                                    column.MaxLength = int.Parse(special.TargetMaxLength);
                                }
                            }
                        }

                        if (!hasSpecial && column.DataType.ToLower().StartsWith("n")) //nchar,nvarchar
                        {
                            if (column.MaxLength > 0 && (!sourceDataType.ToLower().StartsWith("n") || this.targetDbType == DatabaseType.MySql)) //MySql doesn't have nvarchar
                            {
                                column.MaxLength = column.MaxLength / 2;
                            }
                        }                                       
                    }
                    else
                    {
                        if (dataTypeMapping.Specials != null && dataTypeMapping.Specials.Count > 0)
                        {
                            DataTypeMappingSpecial special = dataTypeMapping.Specials.FirstOrDefault(item => item.SourceMaxLength == column.MaxLength.ToString());
                            if (special != null)
                            {
                                column.DataType = special.Type;
                            }
                            else
                            {
                                special = dataTypeMapping.Specials.FirstOrDefault(item => item.SourcePrecision == column.Precision?.ToString() && item.SourceScale == column.Scale?.ToString());
                                if (special != null)
                                {
                                    column.DataType = special.Type;
                                }
                            }

                            if (special != null && !string.IsNullOrEmpty(special.TargetMaxLength))
                            {
                                column.MaxLength = int.Parse(special.TargetMaxLength);
                            }
                        }

                        if (!string.IsNullOrEmpty(dataTypeMapping.Tareget.Precision))
                        {
                            column.Precision = int.Parse(dataTypeMapping.Tareget.Precision);
                        }

                        if (!string.IsNullOrEmpty(dataTypeMapping.Tareget.Scale))
                        {
                            column.Scale = int.Parse(dataTypeMapping.Tareget.Scale);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(column.DefaultValue))
                {
                    string defaultValue = this.GetTrimedDefaultValue(column.DefaultValue);
                    IEnumerable<FunctionMapping> funcMappings = functionMappings.FirstOrDefault(item => item.Any(t => t.DbType == this.sourceDbType.ToString() && t.Function.Split(',').Any(m=> m.Trim().ToLower() == defaultValue.Trim().ToLower())));
                    if (funcMappings != null)
                    {
                        defaultValue = funcMappings.FirstOrDefault(item => item.DbType == this.targetDbType.ToString())?.Function.Split(',')?.FirstOrDefault();
                    }
                    column.DefaultValue = defaultValue;
                }
            }

            return columns;
        }

        private string GetTrimedDataType(TableColumn column)
        {
            string dataType = column.DataType;
            int index = dataType.IndexOf("(");
            if (index > 0)
            {
                return dataType.Substring(0, index);
            }
            return dataType;
        }

        private string GetTrimedDefaultValue(string defaultValue)
        {
            if (!string.IsNullOrEmpty(defaultValue))
            {
                defaultValue = defaultValue.TrimStart('(').TrimEnd(')');
                if (defaultValue.EndsWith("("))
                {
                    defaultValue += ")";
                }
                return defaultValue;
            }
            return defaultValue;
        }       
    }
}
