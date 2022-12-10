using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;

namespace DatabaseConverter.Core
{
    public class DatetimeHelper
    {
        public const string DateFormat = "yyyy-MM-dd";
        public const string DatetimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string OracleDatetimeFormat = "yyyy-MM-dd HH24:mi:ss";


        public static string GetOracleUniformDatetimeString(string value, bool isTimestamp)
        {
            string trimedValue = value.Trim('\'', ' ');

            if(DateTime.TryParse(trimedValue, out var date))
            {
                value = date.ToString(isTimestamp ? DatetimeFormat : DateFormat);
            }            

            return $"'{value}'";
        }

        public static string DecorateDatetimeString(DatabaseType databaseType, string value)
        {
            if(ValueHelper.IsStringValue(value))
            {
                bool isTimestamp = value.Contains(" ");

                if (databaseType == DatabaseType.Postgres)
                {
                    value = $"{value}::{(isTimestamp? "TIMESTAMP":"DATE")}";
                }
                else if(databaseType == DatabaseType.Oracle)
                {
                    value = GetOracleUniformDatetimeString(value, isTimestamp);

                    value = $"{(isTimestamp ? "TIMESTAMP" : "DATE")}{value}";
                }
            }           

            return value;
        }

        public static bool IsTimestampString(string value)
        {
            return value != null && value.Contains(" ");
        }

        public static string GetUniformUnit(string unit)
        {
            string upperUnit = unit?.ToUpper();

            //https://learn.microsoft.com/en-us/sql/t-sql/functions/datepart-transact-sql?view=sql-server-ver16
            switch (upperUnit)
            {
                case "YY":
                case "YYYY":
                    return "YEAR";
                case "Q":
                case "QQ":
                    return "QUARTER";
                case "M":
                case "MM":
                    return "MONTH";                 
                case "WK":
                case "WW":
                    return "WEEK";
                case "Y":
                case "DW":          
                    return "WEEKDAY";
                case "D":
                case "DD":
                    return "DAY";                    
                case "HH":
                    return  "HOUR";                 
                case "MI":
                case "N":
                    return  "MINUTE";                  
                case "S":
                case "SS":
                    return "SECOND";
                case "MS":
                    return "MILLISECOND";
                case "MCS":
                    return "MICROSECOND";
                default:
                    return upperUnit;
            }
        }
    }
}
