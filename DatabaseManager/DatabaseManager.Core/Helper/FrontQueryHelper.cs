using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseManager.Helper
{
    public class FrontQueryHelper
    {
        public static bool NeedQuotedForSql(Type type)
        {
            string typeName = type.Name;

            if (type == typeof(char) ||
               type == typeof(string) ||
               type == typeof(Guid) ||               
               typeName == "SqlHierarchyId" ||
               DataTypeHelper.IsDateOrTimeType(typeName) ||
               DataTypeHelper.IsGeometryType(typeName)
               )
            {
                return true;
            }           

            return false;
        }

        public static string GetSafeValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            value = Regex.Replace(value, @";", string.Empty);
            value = Regex.Replace(value, @"'", string.Empty);
            value = Regex.Replace(value, @"&", string.Empty);
            value = Regex.Replace(value, @"%20", string.Empty);
            value = Regex.Replace(value, @"--", string.Empty);
            value = Regex.Replace(value, @"==", string.Empty);
            value = Regex.Replace(value, @"<", string.Empty);
            value = Regex.Replace(value, @">", string.Empty);
            value = Regex.Replace(value, @"%", string.Empty);

            return value;
        }
    }
}
