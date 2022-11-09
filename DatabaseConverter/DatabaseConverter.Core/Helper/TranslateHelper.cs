using DatabaseConverter.Core.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using Microsoft.Identity.Client;
using NetTopologySuite.Algorithm;
using SqlAnalyser.Model;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class TranslateHelper
    {
        public const string NameRegexPattern = "^[a-zA-Z_][a-zA-Z0-9_]*$";
        public const string NumberRegexPattern = "(([0-9]\\d*\\.?\\d*)|(0\\.\\d*[0-9]))";
        public const string ParenthesesRegexPattern = @"\(.*\)";

        public static string ConvertNumberToPostgresMoney(string str)
        {
            var matches = Regex.Matches(str, NumberRegexPattern);

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

        public static bool HasConcatChars(string symbol, string concatChars, bool hasCharColumn = false)
        {
            if (!string.IsNullOrEmpty(symbol) && !string.IsNullOrEmpty(concatChars))
            {
                string[] items = symbol.Split(concatChars);

                return symbol.Contains(concatChars)
                    && (hasCharColumn || items.Any(item => item.Trim().StartsWith('\'') || item.Trim().EndsWith('\'') || DataTypeHelper.IsCharType(item)));
            }

            return false;
        }

        public static string ConvertConcatChars(string symbol, string sourceConcatChars, string targetConcatChars, bool hasCharColumn = false)
        {
            if (HasConcatChars(symbol, sourceConcatChars, hasCharColumn))
            {
                if (!string.IsNullOrEmpty(targetConcatChars))
                {
                    if (sourceConcatChars != "+")
                    {
                        return symbol.Replace(sourceConcatChars, targetConcatChars);
                    }
                    else //exclude math operator "+"
                    {
                        string[] items = symbol.Split(sourceConcatChars, System.StringSplitOptions.RemoveEmptyEntries);

                        StringBuilder sb = new StringBuilder();

                        for (int i = 0; i < items.Length; i++)
                        {
                            string concatChar = targetConcatChars;

                            if (i > 0)
                            {
                                char? previousLastChar = items[i - 1].Trim().FirstOrDefault();
                                char? currentFirstChar = items[i].Trim().FirstOrDefault();

                                if (previousLastChar.HasValue && currentFirstChar.HasValue
                                    && int.TryParse(previousLastChar.ToString(), out _) && int.TryParse(currentFirstChar.ToString(), out _))
                                {
                                    concatChar = sourceConcatChars;
                                }
                            }

                            if (items[i].Trim().Length > 0)
                            {
                                sb.Append($"{(i > 0 ? concatChar : "")}{items[i]}");
                            }                           
                            
                        }

                        return sb.ToString();
                    }
                }
                else
                {
                    string[] items = ValueHelper.GetTrimedParenthesisValue(symbol).Split(sourceConcatChars);

                    List<string> list = new List<string>();

                    foreach (var item in items)
                    {
                        var trimedItem = "";

                        if (!ValueHelper.IsParenthesisBalanced(item))
                        {
                            trimedItem = item.Trim('(', ')', ' ');
                        }
                        else
                        {
                            trimedItem = ValueHelper.GetTrimedParenthesisValue(item.Trim());
                        }

                        string[] subItems = trimedItem.Split(' ');

                        //this is to handle the "+" like this: CASE WHEN '1'='' THEN 'a' ELSE ISNULL('b1','') + 'b2' END
                        if (subItems.Length > 1)
                        {
                            string firstSubItem = subItems[0].Trim();
                            string lastSubItem = subItems[subItems.Length - 1].Trim();

                            if (lastSubItem.Trim(')').EndsWith("'"))
                            {
                                subItems[subItems.Length - 1] = $"CONCAT({lastSubItem}";
                            }

                            if (firstSubItem.Trim('(').StartsWith("\'") || firstSubItem.Trim('(').StartsWith("N\'"))
                            {
                                subItems[0] = firstSubItem + ")";
                            }
                        }

                        list.Add(string.Join(" ", subItems));
                    }

                    return $"CONCAT({string.Join(",", list)})";
                }
            }

            return symbol;
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

            if (userDefinedTypes != null && userDefinedTypes.Count>0)
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

            DataTypeInfo dataTypeInfo = DataTypeHelper.GetDataTypeInfo(dbInterpreter, dataType);

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
