using Antlr.Runtime.Tree;
using Antlr4.Runtime.Misc;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class DataTypeTranslator : DbObjectTokenTranslator
    {
        private IEnumerable<DataTypeSpecification> sourceDataTypeSpecs;
        private IEnumerable<DataTypeSpecification> targetDataTypeSpecs;

        public DataTypeTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter) : base(sourceInterpreter, targetInterpreter)
        {
            this.sourceDataTypeSpecs = DataTypeManager.GetDataTypeSpecifications(this.sourceDbType);
            this.targetDataTypeSpecs = DataTypeManager.GetDataTypeSpecifications(this.targetDbType);

            this.LoadMappings();
        }

        public void Translate(DataTypeInfo dataTypeInfo)
        {
            string originalDataType = dataTypeInfo.DataType;

            DataTypeInfo dti = this.sourceDbInterpreter.GetDataTypeInfo(originalDataType);
            string sourceDataType = dti.DataType;

            dataTypeInfo.DataType = sourceDataType;

            DataTypeSpecification sourceDataTypeSpec = this.GetDataTypeSpecification(this.sourceDataTypeSpecs, sourceDataType);

            if (!string.IsNullOrEmpty(dti.Args))
            {
                if (sourceDataTypeSpec.Args == "scale")
                {
                    dataTypeInfo.Scale = int.Parse(dti.Args);
                }
                else if (sourceDataTypeSpec.Args == "length")
                {
                    if (dataTypeInfo.MaxLength == null)
                    {
                        dataTypeInfo.MaxLength = int.Parse(dti.Args);
                    }
                }
            }

            DataTypeMapping dataTypeMapping = this.dataTypeMappings.FirstOrDefault(item => item.Source.Type?.ToLower() == dataTypeInfo.DataType?.ToLower()
                 || (item.Source.IsExpression && Regex.IsMatch(dataTypeInfo.DataType, item.Source.Type, RegexOptions.IgnoreCase))
            );

            if (dataTypeMapping != null)
            {
                DataTypeMappingSource sourceMapping = dataTypeMapping.Source;
                DataTypeMappingTarget targetMapping = dataTypeMapping.Target;
                string targetDataType = targetMapping.Type;

                DataTypeSpecification targetDataTypeSpec = this.GetDataTypeSpecification(this.targetDataTypeSpecs, targetDataType);

                if (targetDataTypeSpec == null)
                {
                    throw new Exception($"No type '{targetDataType}' defined for '{targetDbType}'.");
                }

                dataTypeInfo.DataType = targetDataType;

                bool isChar = DataTypeHelper.IsCharType(dataTypeInfo.DataType);
                bool isBinary = DataTypeHelper.IsBinaryType(dataTypeInfo.DataType);

                if (isChar || isBinary)
                {
                    bool noLength = false;

                    if (isChar)
                    {
                        if (!string.IsNullOrEmpty(targetMapping.Length))
                        {
                            dataTypeInfo.MaxLength = int.Parse(targetMapping.Length);

                            if (!DataTypeHelper.StartsWithN(sourceDataType) && DataTypeHelper.StartsWithN(targetDataType))
                            {
                                dataTypeInfo.MaxLength *= 2;
                            }
                        }
                        else
                        {
                            if (DataTypeHelper.StartsWithN(sourceDataType) && !DataTypeHelper.StartsWithN(targetDataType))
                            {
                                if (!this.Option?.NcharToDoubleChar == true)
                                {
                                    if (dataTypeInfo.MaxLength > 0 && dataTypeInfo.MaxLength % 2 == 0)
                                    {
                                        dataTypeInfo.MaxLength /= 2;
                                    }
                                }
                            }
                        }
                    }

                    if (dataTypeMapping.Specials != null && dataTypeMapping.Specials.Count > 0)
                    {
                        DataTypeMappingSpecial special = dataTypeMapping.Specials.FirstOrDefault(item => this.IsSpecialMaxLengthMatched(item, dataTypeInfo));

                        if (special != null)
                        {
                            if (!string.IsNullOrEmpty(special.Type))
                            {
                                dataTypeInfo.DataType = special.Type;
                            }

                            if (!string.IsNullOrEmpty(special.TargetMaxLength))
                            {
                                dataTypeInfo.MaxLength = int.Parse(special.TargetMaxLength);
                            }
                            else
                            {
                                noLength = special.NoLength;
                                dataTypeInfo.MaxLength = -1;
                            }
                        }
                    }

                    if (!noLength)
                    {
                        if (dataTypeInfo.MaxLength == -1)
                        {
                            ArgumentRange? sourceLengthRange = DataTypeManager.GetArgumentRange(sourceDataTypeSpec, "length");

                            if (sourceLengthRange.HasValue)
                            {
                                dataTypeInfo.MaxLength = sourceLengthRange.Value.Max;
                            }
                        }
                    }

                    ArgumentRange? targetLengthRange = DataTypeManager.GetArgumentRange(targetDataTypeSpec, "length");

                    if (targetLengthRange.HasValue)
                    {
                        int targetMaxLength = targetLengthRange.Value.Max;

                        if (DataTypeHelper.StartsWithN(targetDataTypeSpec.Name))
                        {
                            targetMaxLength *= 2;
                        }

                        if (dataTypeInfo.MaxLength > targetMaxLength)
                        {
                            if (!string.IsNullOrEmpty(targetMapping.Substitute))
                            {
                                string[] substitutes = targetMapping.Substitute.Split(',');

                                foreach (string substitute in substitutes)
                                {
                                    DataTypeSpecification dataTypeSpec = this.GetDataTypeSpecification(this.targetDataTypeSpecs, substitute.Trim());

                                    if (dataTypeSpec != null)
                                    {
                                        if (string.IsNullOrEmpty(dataTypeSpec.Args))
                                        {
                                            dataTypeInfo.DataType = substitute;
                                            break;
                                        }
                                        else
                                        {
                                            ArgumentRange? range = DataTypeManager.GetArgumentRange(dataTypeSpec, "length");

                                            if (range.HasValue && range.Value.Max >= dataTypeInfo.MaxLength)
                                            {
                                                dataTypeInfo.DataType = substitute;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (dataTypeMapping.Specials != null && dataTypeMapping.Specials.Count > 0)
                    {
                        foreach (DataTypeMappingSpecial special in dataTypeMapping.Specials)
                        {
                            string name = special.Name;
                            bool matched = false;

                            if (name == "maxLength")
                            {
                                matched = this.IsSpecialMaxLengthMatched(special, dataTypeInfo);
                            } 
                            else if (name == "precisionScale")
                            {
                                matched = this.IsSpecialPrecisionAndScaleMatched(special, dataTypeInfo);
                            }
                            else if (name.Contains("precision") || name.Contains("scale"))
                            {
                                matched = this.IsSpecialPrecisionOrScaleMatched(special, dataTypeInfo);
                            }                           
                            else if (name == "expression")
                            {
                                matched = this.IsSpecialExpressionMatched(special, originalDataType);
                            }
                            else if (name == "isIdentity")
                            {
                                matched = dataTypeInfo.IsIdentity;
                            }
                            

                            if (matched)
                            {
                                dataTypeInfo.DataType = special.Type;
                            }

                            if (!string.IsNullOrEmpty(special.TargetMaxLength))
                            {
                                dataTypeInfo.MaxLength = int.Parse(special.TargetMaxLength);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(targetDataTypeSpec.Format))
                    {
                        bool useConfigPrecisionScale = false;

                        if (!string.IsNullOrEmpty(targetMapping.Precision))
                        {
                            dataTypeInfo.Precision = int.Parse(targetMapping.Precision);

                            useConfigPrecisionScale = true;
                        }

                        if (!string.IsNullOrEmpty(targetMapping.Scale))
                        {
                            dataTypeInfo.Scale = int.Parse(targetMapping.Scale);

                            useConfigPrecisionScale = true;
                        }

                        if (!useConfigPrecisionScale)
                        {
                            if (sourceDataTypeSpec.Args == targetDataTypeSpec.Args)
                            {
                                ArgumentRange? precisionRange = DataTypeManager.GetArgumentRange(targetDataTypeSpec, "precision");
                                ArgumentRange? scaleRange = DataTypeManager.GetArgumentRange(targetDataTypeSpec, "scale");

                                if (precisionRange.HasValue && dataTypeInfo.Precision > precisionRange.Value.Max)
                                {
                                    dataTypeInfo.Precision = precisionRange.Value.Max;
                                }

                                if (scaleRange.HasValue && dataTypeInfo.Scale > scaleRange.Value.Max)
                                {
                                    dataTypeInfo.Scale = scaleRange.Value.Max;
                                }

                                if (dataTypeInfo.Precision.HasValue)
                                {
                                    if (dataTypeInfo.DataType.ToLower() == "int")
                                    {
                                        if (dataTypeInfo.Precision.Value > 10)
                                        {
                                            dataTypeInfo.DataType = "bigint";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                string[] defaultValues = targetDataTypeSpec.Default?.Split(',');

                                bool hasDefaultValues = defaultValues != null && defaultValues.Length > 0;

                                string args = targetDataTypeSpec.Args;

                                if (hasDefaultValues)
                                {
                                    if(args == "precision,scale" && defaultValues.Length == 2)
                                    {
                                        dataTypeInfo.Precision = int.Parse(defaultValues[0]);
                                        dataTypeInfo.Scale = int.Parse(defaultValues[1]);
                                    }
                                    else if(args == "scale" && defaultValues.Length == 1)
                                    {
                                        dataTypeInfo.Scale = int.Parse(defaultValues[0]);
                                    }
                                }
                                else
                                {
                                    dataTypeInfo.Precision = default(int?);
                                }
                            }
                        }
                    }
                    else
                    {
                        string format = targetDataTypeSpec.Format;
                        string dataType = format;

                        string[] defaultValues = targetDataTypeSpec.Default?.Split(',');
                        string targetMappingArgs = targetMapping.Args;

                        int i = 0;
                        foreach (DataTypeArgument arg in targetDataTypeSpec.Arugments)
                        {
                            if (arg.Name.ToLower() == "scale")
                            {
                                ArgumentRange? targetScaleRange = DataTypeManager.GetArgumentRange(targetDataTypeSpec, "scale");

                                long scale = dataTypeInfo.Scale == null ? 0 : dataTypeInfo.Scale.Value;

                                if (targetScaleRange.HasValue && scale > targetScaleRange.Value.Max)
                                {
                                    scale = targetScaleRange.Value.Max;
                                }

                                dataType = dataType.Replace("$scale$", scale.ToString());
                            }
                            else
                            {
                                string defaultValue = defaultValues != null && defaultValues.Length > i ? defaultValues[i] : "";

                                string value = defaultValue;

                                if (targetMapping.Arguments.Any(item => item.Name == arg.Name))
                                {
                                    value = targetMapping.Arguments.FirstOrDefault(item => item.Name == arg.Name).Value;
                                }

                                dataType = dataType.Replace($"${arg.Name}$", value);
                            }

                            i++;
                        }

                        dataTypeInfo.DataType = dataType;
                    }
                }
            }
            else
            {
                dataTypeInfo.DataType = targetDbInterpreter.DefaultDataType;
            }
        }

        private DataTypeSpecification GetDataTypeSpecification(IEnumerable<DataTypeSpecification> dataTypeSpecifications, string dataType)
        {
            Regex regex = new Regex(@"([(][^(^)]+[)])", RegexOptions.IgnoreCase);

            if (regex.IsMatch(dataType))
            {
                MatchCollection matches = regex.Matches(dataType);

                foreach (Match match in matches)
                {
                    dataType = regex.Replace(dataType, "");
                }
            }

            return dataTypeSpecifications.FirstOrDefault(item => item.Name.ToLower() == dataType.ToLower().Trim());
        }

        private bool IsSpecialMaxLengthMatched(DataTypeMappingSpecial special, DataTypeInfo dataTypeInfo)
        {
            string value = special.Value;

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (value == dataTypeInfo.MaxLength?.ToString())
            {
                return true;
            }
            else if (dataTypeInfo.MaxLength.HasValue && (value.StartsWith(">") || value.StartsWith("<")))
            {
                Expression exp = new Expression($"{dataTypeInfo.MaxLength}{value}");

                if (!exp.HasErrors())
                {
                    object result = exp.Evaluate();

                    return result != null && result.GetType() == typeof(Boolean) && (bool)result == true;
                }
            }

            return false;
        }

        private bool IsSpecialPrecisionOrScaleMatched(DataTypeMappingSpecial special, DataTypeInfo dataTypeInfo)
        {
            string[] names = special.Name.Split(',');
            string[] values = special.Value.Split(',');

            string precision = null;
            string scale = null;

            int i = 0;
            foreach (string name in names)
            {
                if (name == "precision")
                {
                    precision = values[i];
                }
                else if (name == "scale")
                {
                    scale = values[i];
                }

                i++;
            }

            if (!string.IsNullOrEmpty(precision) && !string.IsNullOrEmpty(scale))
            {
                return this.IsValueEqual(precision, dataTypeInfo.Precision) && this.IsValueEqual(scale, dataTypeInfo.Scale);
            }
            else if (!string.IsNullOrEmpty(precision) && string.IsNullOrEmpty(scale))
            {
                return this.IsValueEqual(precision, dataTypeInfo.Precision);
            }
            else if (string.IsNullOrEmpty(precision) && !string.IsNullOrEmpty(scale))
            {
                return this.IsValueEqual(scale, dataTypeInfo.Scale);
            }
            return false;
        }

        private bool IsSpecialPrecisionAndScaleMatched(DataTypeMappingSpecial special, DataTypeInfo dataTypeInfo)
        {
            string precision = special.Precison;
            string scale = special.Scale;

            
            if((precision == "isNullOrZero" && this.IsNullOrZero(dataTypeInfo.Precision))
               &&(scale == "isNullOrZero" && this.IsNullOrZero(dataTypeInfo.Scale)))
            {
                return true;
            }

            return false;
        }

        private bool IsNullOrZero(long? value)
        {
            return value == null || value == 0;
        }

        private bool IsValueEqual(string value1, long? value2)
        {
            string v2 = value2?.ToString();
            if (value1 == v2)
            {
                return true;
            }
            else if (value1 == "0" && (v2 is null || v2 == ""))
            {
                return true;
            }

            return false;
        }

        private bool IsSpecialExpressionMatched(DataTypeMappingSpecial special, string dataType)
        {
            string value = special.Value;

            if (Regex.IsMatch(dataType, value, RegexOptions.IgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
