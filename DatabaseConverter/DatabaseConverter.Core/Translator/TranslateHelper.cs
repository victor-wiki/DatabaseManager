using DatabaseInterpreter.Model;
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
                foreach(Match match in matches)
                {
                    if(!string.IsNullOrEmpty(match.Value))
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
                ).OrderByDescending(item=>item.Name.Length);

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
    }
}
