using Antlr4.Runtime;
using DatabaseInterpreter.Utility;
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
        public static bool HasWord(string value, string word)
        {
            return Regex.IsMatch(value, $"({word})", RegexOptions.IgnoreCase);
        }

        public static bool IsSubquery(ParserRuleContext node)
        {
            if (node != null)
            {
                return IsSubquery(node.GetText());
            }
            else
            {
                return false;
            }
        }

        public static bool IsSubquery(string query)
        {
            return HasWord(query, "SELECT");
        }

        public static string ReplaceSymbol(string symbol, string oldValue, string newValue)
        {
            string pattern;

            if (Regex.IsMatch(oldValue, RegexHelper.NameRegexPattern))
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
