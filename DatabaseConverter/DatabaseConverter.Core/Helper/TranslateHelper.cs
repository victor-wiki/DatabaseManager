using DatabaseConverter.Core.Model;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Newtonsoft.Json.Linq;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TSQL.Tokens;

namespace DatabaseConverter.Core
{
    public class TranslateHelper
    {
        public static string ConvertNumberToPostgresMoney(string str)
        {
            var matches = Regex.Matches(str, RegexHelper.NumberRegexPattern);

            if (matches != null)
            {
                foreach (Match match in matches)
                {
                    if (!string.IsNullOrEmpty(match.Value))
                    {
                        str = str.Replace(match.Value, $"{match.Value}::money");
                    }
                }
            }

            return str;
        }

        public static string RemovePostgresDataTypeConvertExpression(string value, IEnumerable<DataTypeSpecification> dataTypeSpecifications, char quotationLeftChar, char quotationRightChar)
        {
            if (value.Contains("::")) //datatype convert operator
            {
                var specs = dataTypeSpecifications.Where(item =>
                value.ToLower().Contains($"::{item.Name}") ||
                value.ToLower().Contains($"{quotationLeftChar}{item.Name}{quotationRightChar}")
                ).OrderByDescending(item => item.Name.Length);

                if (specs != null)
                {
                    foreach (var spec in specs)
                    {
                        value = value.Replace($"::{quotationLeftChar}{spec.Name}{quotationRightChar}", "").Replace($"::{spec.Name}", "");
                    }
                }
            }

            return value;
        }


        public static IEnumerable<char> GetTrimChars(params DbInterpreter[] dbInterpreters)
        {
            foreach (var interpreter in dbInterpreters)
            {
                yield return interpreter.QuotationLeftChar;
                yield return interpreter.QuotationRightChar;
            }
        }

        public static TableColumn SimulateTableColumn(DbInterpreter dbInterpreter, string dataType, DbConverterOption option, List<UserDefinedType> userDefinedTypes, char[] trimChars)
        {
            TableColumn column = new TableColumn();

            if (userDefinedTypes != null && userDefinedTypes.Count > 0)
            {
                UserDefinedType userDefinedType = userDefinedTypes.FirstOrDefault(item => item.Name.Trim(trimChars).ToUpper() == dataType.Trim(trimChars).ToUpper());

                if (userDefinedType != null)
                {
                    var attr = userDefinedType.Attributes.First();

                    column.DataType = attr.DataType;
                    column.MaxLength = attr.MaxLength;

                    return column;
                }
            }

            DataTypeInfo dataTypeInfo = dbInterpreter.GetDataTypeInfo(dataType);

            string dataTypeName = dataTypeInfo.DataType.Trim().ToLower();

            column.DataType = dataTypeName;

            bool isChar = DataTypeHelper.IsCharType(dataTypeName);
            bool isBinary = DataTypeHelper.IsBinaryType(dataTypeName);

            string args = dataTypeInfo.Args;
            int? precision = default(int?);
            int? scale = default(int?);
            int maxLength = -1;

            if (!string.IsNullOrEmpty(args))
            {
                string[] argItems = args.Split(',');

                if (isChar || isBinary)
                {
                    maxLength = GetDataTypeArgumentValue(argItems[0].Trim(), true).Value;

                    if (isChar && DataTypeHelper.StartsWithN(dataTypeName) && maxLength > 0)
                    {
                        maxLength *= 2;
                    }
                }
                else
                {
                    int value = -1;

                    if (int.TryParse(argItems[0], out value))
                    {
                        if (value > 0)
                        {
                            DataTypeSpecification dataTypeSpecification = dbInterpreter.GetDataTypeSpecification(dataTypeName);

                            if (dataTypeSpecification != null)
                            {
                                string specArgs = dataTypeSpecification.Args;

                                if (specArgs == "scale")
                                {
                                    scale = GetDataTypeArgumentValue(argItems[0]);
                                }
                                else if (specArgs == "precision,scale")
                                {
                                    precision = GetDataTypeArgumentValue(argItems[0]);

                                    if (argItems.Length > 1)
                                    {
                                        scale = GetDataTypeArgumentValue(argItems[1]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            column.Precision = precision;
            column.Scale = scale;
            column.MaxLength = maxLength;

            return column;
        }

        private static int? GetDataTypeArgumentValue(string value, bool isChar = false)
        {
            int intValue = -1;

            if (int.TryParse(value, out intValue))
            {
                return intValue;
            }

            return isChar ? -1 : default(int?);
        }

        public static void TranslateTableColumnDataType(DataTypeTranslator dataTypeTranslator, TableColumn column)
        {
            DataTypeInfo dataTypeInfo = DataTypeHelper.GetDataTypeInfoByTableColumn(column);

            dataTypeTranslator.Translate(dataTypeInfo);

            DataTypeHelper.SetDataTypeInfoToTableColumn(dataTypeInfo, column);
        }

        public static void RestoreTokenValue(string definition, TokenInfo token)
        {
            if (token != null && token.StartIndex.HasValue && token.Length > 0)
            {
                token.Symbol = definition.Substring(token.StartIndex.Value, token.Length);
            }
        }


    }
}
