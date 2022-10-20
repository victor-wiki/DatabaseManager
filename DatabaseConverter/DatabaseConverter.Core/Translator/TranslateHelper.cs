using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class TranslateHelper
    {
        private const string NumberRegexExpression = "(([0-9]\\d*\\.?\\d*)|(0\\.\\d*[0-9]))";

        public static string ConvertNumberToPostgresMoney(string str)
        {
            var matches = Regex.Matches(str, NumberRegexExpression);

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
                    && (hasCharColumn || items.Any(item => item.Trim().StartsWith('\'') || item.Trim().EndsWith('\'')));
            }

            return false;
        }

        public static string ConvertConcatChars(string symbol, string sourceConcatChars, string targetConcatChars, bool hasCharColumn = false)
        {
            if (HasConcatChars(symbol, sourceConcatChars, hasCharColumn))
            {
                if (!string.IsNullOrEmpty(targetConcatChars))
                {
                    return symbol.Replace(sourceConcatChars, targetConcatChars);
                }
                else
                {
                    string[] items = ValueHelper.GetTrimedParenthesisValue(symbol).Split(sourceConcatChars);

                    List<string> list = new List<string>();

                    foreach(var item in items)
                    {
                        var trimedItem = "";
                        
                        if(!ValueHelper.IsParenthesisBalanced(item))
                        {
                            trimedItem = item.Trim('(', ')', ' ');
                        }
                        else
                        {
                            trimedItem = ValueHelper.GetTrimedParenthesisValue(item.Trim());
                        }                            

                        list.Add(trimedItem);
                    }

                    return $"CONCAT({string.Join(",", list)})";
                }
            }

            return symbol;
        }       
    }
}
