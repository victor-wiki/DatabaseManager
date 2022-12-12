using DatabaseConverter.Core.Model;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Linq;

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

            if (DateTime.TryParse(trimedValue, out var date))
            {
                value = date.ToString(isTimestamp ? DatetimeFormat : DateFormat);
            }

            return $"'{value}'";
        }

        public static string DecorateDatetimeString(DatabaseType databaseType, string value)
        {
            if (ValueHelper.IsStringValue(value))
            {
                bool isTimestamp = value.Contains(" ");

                if (databaseType == DatabaseType.Postgres)
                {
                    value = $"{value}::{(isTimestamp ? "TIMESTAMP" : "DATE")}";
                }
                else if (databaseType == DatabaseType.Oracle)
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

        public static string GetMappedUnit(DatabaseType sourceDbType, DatabaseType targetDbType, string unit)
        {
            string trimedUnit = unit?.Trim('\'');

            //Sqlserver: https://learn.microsoft.com/en-us/sql/t-sql/functions/datepart-transact-sql?view=sql-server-ver16
            //Postgres: https://www.postgresql.org/docs/current/functions-datetime.html
            //MySql: https://www.w3schools.com/Sql/func_mysql_extract.asp
            //SQLite:https://www.w3schools.blog/strftime-function-sqlite

            var mappings = DateUnitMappingManager.GetDateUnitMappings();

            if (mappings != null)
            {
                var mapping = mappings.FirstOrDefault(item => item.Items.Any(t => IsDateUnitMatched(sourceDbType, t, trimedUnit)));

                if (mapping != null)
                {
                    var target = mapping.Items.FirstOrDefault(item => item.DbType == targetDbType.ToString());

                    if(target == null || !target.Formal)
                    {
                        return mapping.Name;
                    }
                    else
                    {
                        return target.Unit;
                    }
                }
            }

            return unit;            
        }

        private static bool IsDateUnitMatched(DatabaseType dbType, DateUnitMappingItem mappingItem, string unit)
        {
            if (dbType.ToString() == mappingItem.DbType)
            {
                string[] unitItems = mappingItem.Unit.Split(',');

                if (!mappingItem.CaseSensitive)
                {
                    return unitItems.Any(item => item.ToUpper() == unit.ToUpper());
                }
                else
                {
                    return unitItems.Any(item => item == unit);
                }
            }

            return false;
        }

        public static string GetSqliteStrfTimeFormat(DatabaseType sourceDbType, string unit)
        {
            string mappedUnit = GetMappedUnit(sourceDbType, DatabaseType.Sqlite, unit);

            string format = "";

            switch (mappedUnit)
            {
                case "YEAR":
                    format = "Y";
                    break;
                case "MONTH":
                    format = "m";
                    break;
                case "WEEK":
                    format = "W";
                    break;
                case "WEEKDAY":
                    format = "w";
                    break;
                case "DAY":
                    format = "d";
                    break;
                case "HOUR":
                    format = "H";
                    break;
                case "MINUTE":
                    format = "M";
                    break;
                case "SECOND":
                    format = "S";
                    break;               
                case "DAYOFYEAR":
                    format = "j";
                    break;
            }

            return format;
        }
    }
}
