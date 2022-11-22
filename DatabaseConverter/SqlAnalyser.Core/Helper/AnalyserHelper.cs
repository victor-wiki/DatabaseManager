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
            bool hasWord = HasWord(query, "SELECT");

            if(hasWord)
            {
                int index = query.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);

                int singleQuotationCharCount = query.Substring(0, index).Count(item => item == '\'');

                if(singleQuotationCharCount %2 == 0)
                {
                    return true;
                }                
            }

            return false;
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

        public static bool HasExitStatement(WhileStatement statement)
        {
            var types = new Type[] { typeof(BreakStatement), typeof(LoopExitStatement) };

            foreach (var st in statement.Statements)
            {
                if (types.Contains(st.GetType()))
                {
                    return true;
                }
                else if (st is IfStatement @if)
                {
                    foreach (var item in @if.Items)
                    {
                        foreach (var ist in item.Statements)
                        {
                            if (types.Contains(ist.GetType()))
                            {
                                return true;
                            }
                            else if(ist is WhileStatement @while)
                            {
                                return HasExitStatement(@while);
                            }
                        }
                    }
                }
                else if (st is WhileStatement @while)
                {
                    return HasExitStatement(@while);
                }
            }

            return false;
        }
    }
}
