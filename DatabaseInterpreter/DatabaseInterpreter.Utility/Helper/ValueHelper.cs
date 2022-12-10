using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseInterpreter.Utility
{
    public class ValueHelper
    {
        public static bool IsNullValue(object value, bool emptyAsNull = false)
        {
            if (value == null)
            {
                return true;
            }

            if (value.GetType() == typeof(DBNull))
            {
                return true;
            }

            if (emptyAsNull && value.ToString().Length == 0)
            {
                return true;
            }
            return false;
        }

        public static bool IsBytes(object value)
        {
            return (value != null && value.GetType() == typeof(byte[]));
        }

        public static string TransferSingleQuotation(string value)
        {
            return value?.Replace("'", "''");
        }

        public static string ConvertGuidBytesToString(byte[] value, DatabaseType databaseType, string dataType, long? length, bool bytesAsString)
        {
            string strValue = null;

            if (value != null && value.Length == 16)
            {
                if (databaseType == DatabaseType.SqlServer && dataType.ToLower() == "uniqueidentifier")
                {
                    strValue = new Guid((byte[])value).ToString();
                }
                else if (databaseType == DatabaseType.MySql && dataType == "char" && length == 36)
                {
                    strValue = new Guid((byte[])value).ToString();
                }
                else if (bytesAsString && databaseType == DatabaseType.Oracle && dataType.ToLower() == "raw" && length == 16)
                {
                    strValue = StringHelper.GuidToRaw(new Guid((byte[])value).ToString());
                }
            }

            return strValue;
        }

        public static string BytesToHexString(byte[] value)
        {
            if (value == null)
            {
                return null;
            }

            string hex = "0x" + string.Concat(value.Select(item => item.ToString("X2")));

            return hex;
        }

        public static byte[] HexStringToBytes(string value)
        {
            string content = value.Substring(2);

            return Enumerable.Range(0, content.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(content.Substring(x, 2), 16)).ToArray();
        }

        public static bool IsSequenceNextVal(string value)
        {
            return value?.Contains("nextval") == true;
        }

        public static bool IsTrueValue(string value, bool includeInteger = true)
        {
            return value?.ToLower() == "true" || (includeInteger && value?.Trim() == "1");
        }

        public static bool IsStringEquals(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) && string.IsNullOrEmpty(str2))
            {
                return true;
            }

            return str1 == str2;
        }

        public static bool IsStringValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                bool isBeginAndEndWith = value.EndsWith("\'") && (value.StartsWith("\'") || value.ToUpper().StartsWith("N\'"));

                if (!isBeginAndEndWith)
                {
                    return false;
                }
                else
                {
                    int firstIndex = value.IndexOf("'");
                    int lastIndex = value.LastIndexOf("'");

                    string innerContent = value.Substring(firstIndex + 1, lastIndex - firstIndex - 1);

                    if (innerContent.IndexOf("'") >= 0)
                    {
                        int count1 = innerContent.Count(item => item == '\'');
                        int count2 = Regex.Matches(innerContent, "''").Count;

                        if (count1 != count2 * 2)
                        {
                            return false;
                        }

                        return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static int BooleanToInteger(bool value)
        {
            if (value)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
