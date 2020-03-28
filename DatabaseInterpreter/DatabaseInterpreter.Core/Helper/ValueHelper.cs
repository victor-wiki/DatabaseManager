using System;
using System.Data;

namespace  DatabaseInterpreter.Core
{
    public class ValueHelper
    {
        public static bool IsNullValue(object value, bool emptyAsNull = false)
        {
            if (value == null)
            {
                return true;
            }

            if(value.GetType() == typeof(DBNull))
            {
                return true;
            }

            if (emptyAsNull && value.ToString().Length == 0)
            {
                return true;
            }
            return false;
        }

        public static string TransferSingleQuotation(string value)
        {
            return value?.Replace("'", "''");
        }      
        
        public static bool NeedQuotedForSql(Type type)
        {
            string typeName = type.Name;

            if(type == typeof(string) ||
               type == typeof(DateTime) ||
               type == typeof(Guid) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               typeName == "SqlHierarchyId"
               )
            {
                return true;
            }

            return false;
        }
    }
}
