using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseConverter.Core
{
    public class DatetimeHelper
    {
        public static string GetOracleUniformDatetimeString(string value, bool isTimestamp)
        {
            value = DateTime.Parse(value.Trim('\'', ' ')).ToString(isTimestamp ? "yyyy-MM-dd HH:mm:ss" : "yyyy-MM-dd");

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
    }
}
