using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DatabaseInterpreter.Core
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

        public static bool NeedQuotedForSql(Type type)
        {
            string typeName = type.Name;

            if (type == typeof(string) ||
               type == typeof(DateTime) ||
               type == typeof(Guid) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               typeName == nameof(SqlHierarchyId)
               )
            {
                return true;
            }

            return false;
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

        public static string GetTrimedParenthesisValue(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.StartsWith('(') && value.EndsWith(')'))
            {
                while (value.StartsWith('(') && value.EndsWith(')') && IsParenthesisBalanced(value))
                {
                    string trimedValue = value.Substring(1, value.Length - 2);

                    if(!IsParenthesisBalanced(trimedValue))
                    {
                        return value;
                    }
                    else
                    {
                        value = trimedValue;
                    }
                }

                return value;
            }

            return value;
        }

        public static bool IsParenthesisBalanced(string value)
        {
            Dictionary<char, char> pairs = new Dictionary<char, char>() { { '(', ')' } };

            Stack<char> parenthesis = new Stack<char>();

            try
            {
                foreach (char c in value)
                {
                    if (pairs.Keys.Contains(c))
                    {
                        parenthesis.Push(c);
                    }
                    else
                    {
                        if (pairs.Values.Contains(c))
                        {
                            if (c == pairs[parenthesis.First()])
                            {
                                parenthesis.Pop();
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return parenthesis.Count() == 0 ? true : false;
        }

        public static bool IsStringEquals(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) && string.IsNullOrEmpty(str2))
            {
                return true;
            }

            return str1 == str2;
        }

        public static bool IsSequenceNextVal(string value)
        {
            return value?.Contains("nextval") == true;
        }
    }
}
