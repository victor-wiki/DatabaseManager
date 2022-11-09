using Newtonsoft.Json.Linq;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlAnalyser.Core
{
    public class AnalyserHelper
    {
        public const string NameRegexPattern = "^[a-zA-Z_][a-zA-Z0-9_]*$";

        public static string ReplaceSymbol(string symbol, string oldValue, string newValue)
        {
            string pattern;

            if (Regex.IsMatch(oldValue, NameRegexPattern))
            {
                pattern = $"\\b{oldValue}\\b";
            }
            else
            {
                pattern = $"({oldValue})";
            }

            return Regex.Replace(symbol, pattern, newValue, RegexOptions.Multiline);
        }

        public static bool IsFromItemsHaveJoin(List<FromItem> fromItems)
        {
            return fromItems != null && fromItems.Count > 0 && fromItems.Any(item => item.JoinItems != null && item.JoinItems.Count > 0);
        }
    }
}
