using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseInterpreter.Utility
{
    public class RegexHelper
    {
        public const string NameRegexPattern = "^[a-zA-Z_][a-zA-Z0-9_]*$";
        public const string NumberRegexPattern = "(([0-9]\\d*\\.?\\d*)|(0\\.\\d*[0-9]))";
        public const string ParenthesesRegexPattern = @"\((.|\n|\r)*\)";
        public const string EscapeChars = @".()[^$+*?|\{";

        public static string Replace(string input, string pattern, string replacement, RegexOptions options)
        {
            string escapedPattern = Regex.Escape(pattern);
            string escapedReplacement = CheckReplacement(replacement);

            return Regex.Replace(input, escapedPattern, escapedReplacement, options);
        }

        public static string CheckReplacement(string replacement)
        {
            //https://learn.microsoft.com/en-us/dotnet/standard/base-types/substitutions-in-regular-expressions
            if (replacement != null && replacement.Contains("$"))
            {
                return replacement.Replace("$", "$$");
            }

            return replacement;
        }
    }
}
