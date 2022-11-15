using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseInterpreter.Utility
{
    public class RegexHelper
    {
        public const string NameRegexPattern = "^[a-zA-Z_][a-zA-Z0-9_]*$";
        public const string NumberRegexPattern = "(([0-9]\\d*\\.?\\d*)|(0\\.\\d*[0-9]))";
        public const string ParenthesesRegexPattern = @"\((.|\n|\r)*\)";
    }
}
