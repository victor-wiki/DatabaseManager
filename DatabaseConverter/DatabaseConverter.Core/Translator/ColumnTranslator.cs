using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using NCalc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class ColumnTranslator : DbObjectTokenTranslator
    {
        private List<TableColumn> columns;
        private DatabaseType sourceDbType;
        private DatabaseType targetDbType;
        private IEnumerable<DataTypeSpecification> sourceDataTypeSpecs;
        private IEnumerable<DataTypeSpecification> targetDataTypeSpecs;
        private FunctionTranslator functionTranslator;

        public ColumnTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter, List<TableColumn> columns) : base(sourceInterpreter, targetInterpreter)
        {
            this.columns = columns;
            this.sourceDbType = sourceInterpreter.DatabaseType;
            this.targetDbType = targetInterpreter.DatabaseType;
            this.sourceDataTypeSpecs = DataTypeManager.GetDataTypeSpecifications(this.sourceDbType);
            this.targetDataTypeSpecs = DataTypeManager.GetDataTypeSpecifications(this.targetDbType);
            this.functionTranslator = new FunctionTranslator(this.sourceDbInterpreter, this.targetDbInterpreter);
        }

        public override void Translate()
        {
            if (this.sourceDbType == this.targetDbType)
            {
                return;
            }

            if (this.hasError)
            {
                return;
            }

            this.FeedbackInfo("Begin to translate columns.");

            this.LoadMappings();
            this.functionTranslator.LoadMappings();
            this.functionTranslator.LoadFunctionSpecifications();

            this.CheckComputeExpression();

            foreach (TableColumn column in this.columns)
            {
                this.ConvertDataType(column);

                if (!string.IsNullOrEmpty(column.DefaultValue))
                {
                    this.ConvertDefaultValue(column);
                }

                if (column.IsComputed)
                {
                    this.ConvertComputeExpression(column);
                }
            }

            this.FeedbackInfo("End translate columns.");
        }
        public void ConvertDataType(TableColumn column)
        {
            string originalDataType = column.DataType;

            DataTypeInfo dataTypeInfo = DataTypeHelper.GetDataTypeInfo(this.sourceDbInterpreter, originalDataType);
            string sourceDataType = dataTypeInfo.DataType;

            column.DataType = sourceDataType;

            DataTypeSpecification sourceDataTypeSpec = this.GetDataTypeSpecification(this.sourceDataTypeSpecs, sourceDataType);

            if (!string.IsNullOrEmpty(dataTypeInfo.Args))
            {
                if (sourceDataTypeSpec.Args == "scale")
                {
                    column.Scale = int.Parse(dataTypeInfo.Args);
                }
                else if (sourceDataTypeSpec.Args == "length")
                {
                    if (column.MaxLength == null)
                    {
                        column.MaxLength = int.Parse(dataTypeInfo.Args);
                    }
                }
            }

            DataTypeMapping dataTypeMapping = this.dataTypeMappings.FirstOrDefault(item => item.Source.Type?.ToLower() == column.DataType?.ToLower()
                 || (item.Source.IsExpression && Regex.IsMatch(column.DataType, item.Source.Type, RegexOptions.IgnoreCase))
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

                if (targetDataTypeSpec.IsIdentity)
                {
                    if (targetDbType == DatabaseType.SqlServer || targetDbType == DatabaseType.MySql)
                    {
                        column.IsIdentity = true;

                        if (column.DefaultValue.Contains("nextval"))
                        {
                            column.DefaultValue = null;
                        }

                        return;
                    }
                }

                column.DataType = targetDataType;

                bool isChar = DataTypeHelper.IsCharType(column.DataType);
                bool isBinary = DataTypeHelper.IsBinaryType(column.DataType);

                if (isChar || isBinary)
                {
                    if (isChar)
                    {
                        if (!string.IsNullOrEmpty(targetMapping.Length))
                        {
                            column.MaxLength = int.Parse(targetMapping.Length);

                            if (DataTypeHelper.StartWithN(targetDataType) && !DataTypeHelper.StartWithN(sourceDataType))
                            {
                                column.MaxLength *= 2;
                            }
                        }
                    }

                    if (dataTypeMapping.Specials != null && dataTypeMapping.Specials.Count > 0)
                    {
                        DataTypeMappingSpecial special = dataTypeMapping.Specials.FirstOrDefault(item => this.IsSpecialMaxLengthMatched(item, column));

                        if (special != null)
                        {
                            column.DataType = special.Type;

                            if (!string.IsNullOrEmpty(special.TargetMaxLength))
                            {
                                column.MaxLength = int.Parse(special.TargetMaxLength);
                            }
                        }
                    }

                    if (column.MaxLength == -1)
                    {
                        ArgumentRange? sourceLengthRange = DataTypeManager.GetArgumentRange(sourceDataTypeSpec, "length");

                        if (sourceLengthRange.HasValue)
                        {
                            column.MaxLength = sourceLengthRange.Value.Max;
                        }
                    }

                    ArgumentRange? targetLengthRange = DataTypeManager.GetArgumentRange(targetDataTypeSpec, "length");

                    if (targetLengthRange.HasValue)
                    {
                        int targetMaxLength = targetLengthRange.Value.Max;

                        if (column.MaxLength > targetMaxLength)
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
                                            column.DataType = substitute;
                                            break;
                                        }
                                        else
                                        {
                                            ArgumentRange? range = DataTypeManager.GetArgumentRange(dataTypeSpec, "length");

                                            if (range.HasValue && range.Value.Max >= column.MaxLength)
                                            {
                                                column.DataType = substitute;
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
                                matched = this.IsSpecialMaxLengthMatched(special, column);
                            }
                            else if (name.Contains("precision") || name.Contains("scale"))
                            {
                                matched = this.IsSpecialPrecisionOrScaleMatched(special, column);
                            }
                            else if (name == "expression")
                            {
                                matched = this.IsSpecialExpressionMatched(special, originalDataType);
                            }
                            else if (name == "isDentity")
                            {
                                matched = column.IsIdentity;
                            }

                            if (matched)
                            {
                                column.DataType = special.Type;
                            }

                            if (!string.IsNullOrEmpty(special.TargetMaxLength))
                            {
                                column.MaxLength = int.Parse(special.TargetMaxLength);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(targetDataTypeSpec.Format))
                    {
                        bool useConfigPrecisionScale = false;

                        if (!string.IsNullOrEmpty(targetMapping.Precision))
                        {
                            column.Precision = int.Parse(targetMapping.Precision);

                            useConfigPrecisionScale = true;
                        }

                        if (!string.IsNullOrEmpty(targetMapping.Scale))
                        {
                            column.Scale = int.Parse(targetMapping.Scale);

                            useConfigPrecisionScale = true;
                        }

                        if (!useConfigPrecisionScale)
                        {
                            if (sourceDataTypeSpec.Args == "scale")
                            {
                                column.Precision = default(int?);
                            }
                            else if (sourceDataTypeSpec.Args == "precision,scale" && sourceDataTypeSpec.Args == targetDataTypeSpec.Args)
                            {
                                ArgumentRange? precisionRange = DataTypeManager.GetArgumentRange(targetDataTypeSpec, "precision");
                                ArgumentRange? scaleRange = DataTypeManager.GetArgumentRange(targetDataTypeSpec, "scale");

                                if (precisionRange.HasValue && column.Precision > precisionRange.Value.Max)
                                {
                                    column.Precision = precisionRange.Value.Max;
                                }

                                if (scaleRange.HasValue && column.Scale > scaleRange.Value.Max)
                                {
                                    column.Scale = scaleRange.Value.Max;
                                }

                                if (column.Precision.HasValue)
                                {
                                    if (column.DataType.ToLower() == "int")
                                    {
                                        if (column.Precision.Value > 10)
                                        {
                                            column.DataType = "bigint";
                                        }
                                    }
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

                                int scale = column.Scale == null ? 0 : column.Scale.Value;

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

                        column.DataType = dataType;
                    }
                }
            }
            else
            {
                column.DataType = targetDbInterpreter.DefaultDataType;
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

        private bool IsSpecialMaxLengthMatched(DataTypeMappingSpecial special, TableColumn column)
        {
            string value = special.Value;

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (value == column.MaxLength?.ToString())
            {
                return true;
            }
            else if (column.MaxLength.HasValue && (value.StartsWith(">") || value.StartsWith("<")))
            {
                Expression exp = new Expression($"{column.MaxLength}{value}");

                if (!exp.HasErrors())
                {
                    object result = exp.Evaluate();

                    return result != null && result.GetType() == typeof(Boolean) && (bool)result == true;
                }
            }

            return false;
        }

        private bool IsSpecialPrecisionOrScaleMatched(DataTypeMappingSpecial special, TableColumn column)
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
                return this.IsValueEqual(precision, column.Precision) && this.IsValueEqual(scale, column.Scale);
            }
            else if (!string.IsNullOrEmpty(precision) && string.IsNullOrEmpty(scale))
            {
                return this.IsValueEqual(precision, column.Precision);
            }
            else if (string.IsNullOrEmpty(precision) && !string.IsNullOrEmpty(scale))
            {
                return this.IsValueEqual(scale, column.Scale);
            }
            return false;
        }

        private bool IsValueEqual(string value1, int? value2)
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

        public void ConvertDefaultValue(TableColumn column)
        {
            string defaultValue = ValueHelper.GetTrimedParenthesisValue(column.DefaultValue);

            IEnumerable<FunctionMapping> funcMappings = this.functionMappings.FirstOrDefault(item => item.Any(t => t.DbType == this.sourceDbType.ToString() && t.Function.Split(',').Any(m => m.Trim().ToLower() == defaultValue.Trim().ToLower())));

            if (funcMappings != null)
            {
                defaultValue = funcMappings.FirstOrDefault(item => item.DbType == this.targetDbType.ToString())?.Function.Split(',')?.FirstOrDefault();
            }
            else
            {
                if (this.sourceDbInterpreter.DatabaseType == DatabaseType.Postgres)
                {
                    if (defaultValue.Contains("::")) //remove Postgres type reference
                    {
                        string[] items = defaultValue.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        List<string> list = new List<string>();

                        foreach (var item in items)
                        {
                            list.Add(item.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                        }

                        defaultValue = string.Join(" ", list);
                    }

                    if (defaultValue.Trim() == "true" || defaultValue.Trim() == "false")
                    {
                        defaultValue = defaultValue.Replace("true", "1").Replace("false", "0");
                    }
                }
                else if (this.targetDbInterpreter.DatabaseType == DatabaseType.Postgres)
                {
                    if (column.DataType == "boolean")
                    {
                        defaultValue = defaultValue.Replace("0", "false").Replace("1", "true");
                    }
                }
            }

            column.DefaultValue = defaultValue;
        }

        public void ConvertComputeExpression(TableColumn column)
        {
            if (this.sourceDbType == DatabaseType.Oracle)
            {
                column.ComputeExp = column.ComputeExp.Replace("U'", "'");
            }
            else if (this.sourceDbType == DatabaseType.SqlServer)
            {
                column.ComputeExp = column.ComputeExp.Replace("N'", "'");
            }

            column.ComputeExp = this.ParseDefinition(column.ComputeExp);

            string computeExp = column.ComputeExp.ToLower();

            if (this.targetDbType == DatabaseType.Postgres)
            {
                //this is to avoid error when datatype like money uses coalesce(exp,0)
                if (computeExp.Contains("coalesce"))
                {
                    string exp = column.ComputeExp;

                    if (computeExp.StartsWith("("))
                    {
                        exp = exp.Substring(1, computeExp.Length - 1);
                    }

                    List<FunctionFomular> fomulars = FunctionTranslator.GetFunctionFomulars(exp);

                    if (fomulars.Count > 0 && fomulars.First().Args.Count > 0)
                    {
                        column.ComputeExp = fomulars.First().Args[0];
                    }
                }
            }

            column.ComputeExp = this.functionTranslator.GetMappedFunction(column.ComputeExp);

            if(this.targetDbInterpreter.DatabaseType == DatabaseType.Postgres)
            {
                if (column.DataType == "money" && !column.ComputeExp.ToLower().Contains("::money"))
                {
                    column.ComputeExp = TranslateHelper.ConvertNumberToPostgresMoney(column.ComputeExp);
                }
            }
            
            if(this.sourceDbInterpreter.DatabaseType == DatabaseType.Postgres)
            {
                if (column.ComputeExp.Contains("::")) //datatype convert operator
                {
                    column.ComputeExp = TranslateHelper.RemovePostgresDataTypeConvertExpression(column.ComputeExp, sourceDataTypeSpecs, this.targetDbInterpreter.QuotationLeftChar, this.targetDbInterpreter.QuotationRightChar);
                }
            }
        }

        private async void CheckComputeExpression()
        {
            IEnumerable<Function> customFunctions = this.SourceSchemaInfo?.Functions;

            foreach (TableColumn column in this.columns)
            {
                if (column.IsComputed)
                {
                    if (this.Option != null && !this.Option.ConvertComputeColumnExpression)
                    {
                        if (this.Option.OnlyCommentComputeColumnExpressionInScript)
                        {
                            column.ScriptComment = " AS " + column.ComputeExp;
                        }

                        column.ComputeExp = null;
                        continue;
                    }

                    bool setToNull = false;

                    var tableColumns = this.columns.Where(item => item.TableName == column.TableName);

                    bool isReferToSpecialDataType = tableColumns.Any(item => item.Name != column.Name
                                            && DataTypeHelper.SpecialDataTypes.Any(t => t.ToLower().Contains(item.DataType.ToLower()))
                                            && Regex.IsMatch(column.ComputeExp, $@"\b({item.Name})\b", RegexOptions.IgnoreCase));

                    if (isReferToSpecialDataType)
                    {
                        setToNull = true;
                    }

                    if (!setToNull && (this.targetDbInterpreter.DatabaseType == DatabaseType.MySql || this.targetDbInterpreter.DatabaseType == DatabaseType.Oracle))
                    {
                        if (customFunctions == null || customFunctions.Count() == 0)
                        {
                            customFunctions = await this.sourceDbInterpreter.GetFunctionsAsync();
                        }

                        if (customFunctions != null)
                        {
                            if (customFunctions.Any(item => column.ComputeExp.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                setToNull = true;
                            }
                        }
                    }

                    if (setToNull)
                    {
                        column.ScriptComment = " AS " + column.ComputeExp;
                        column.ComputeExp = null;
                    }
                }
            }
        }
    }
}
