using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseInterpreter.Utility
{
    public class StringHelper
    {
        public static string GetSingleQuotedString(params string[] values)
        {
            if (values != null)
            {
                return string.Join(",", values.Select(item => $"'{item}'"));
            }
            return null;
        }

        public static string RemoveEmoji(string str)
        {
            return Regex.Replace(str, @"\p{Cs}", "");
        }

        public static string RawToGuid(string text)
        {
            byte[] bytes = ParseHex(text);
            Guid guid = new Guid(bytes);
            return guid.ToString("N").ToUpperInvariant();
        }

        public static string GuidToRaw(string text)
        {
            Guid guid = new Guid(text);
            return BitConverter.ToString(guid.ToByteArray()).Replace("-", "");
        }

        public static byte[] ParseHex(string text)
        {
            byte[] ret = new byte[text.Length / 2];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);
            }
            return ret;
        }

        public static string ToSingleEmptyLine(string value)
        {
            if (value != null)
            {
                return Regex.Replace(value, "(\\r\\n){3,}", Environment.NewLine + Environment.NewLine, RegexOptions.Multiline);
            }
            return value;
        }

        public static string GetFriendlyTypeName(string name)
        {
            Regex reg = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])");
            return reg.Replace(name, " ");
        }
    }
}
