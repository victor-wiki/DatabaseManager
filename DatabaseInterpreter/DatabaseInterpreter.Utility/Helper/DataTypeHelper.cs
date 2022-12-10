using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseInterpreter.Utility
{
    public class DataTypeHelper
    {
        public static readonly string[] CharTypeFlags = { "char" };
        public static readonly string[] TextTypeFlags = { "text" };
        public static readonly string[] BinaryTypeFlags = { "binary", "bytea", "raw", "blob" };
        public static readonly string[] DateOrTimeTypeFlags = { "date", "time" };
        public static readonly string[] DatetimeOrTimestampTypeFlags = { "datetime", "timestamp"};
        public static readonly string[] GeometryTypeFlags = { "geometry", "geography", "point", "line", "circle", "polygon" };
        public static readonly string[] SpecialDataTypeFlags = { "sqlhierarchyid","geography","geometry","byte[]"};

        public static bool IsCharType(string dataType)
        {
            return IsContainsFlag(dataType, CharTypeFlags);
        }

        public static bool IsBinaryType(string dataType)
        {
            return IsContainsFlag(dataType, BinaryTypeFlags);
        }

        public static bool IsGeometryType(string dataType)
        {
            return IsContainsFlag(dataType, GeometryTypeFlags);
        }

        public static bool StartsWithN(string dataType)
        {
            return dataType.StartsWith("n", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTextType(string dataType)
        {
            return IsContainsFlag(dataType, TextTypeFlags);
        }

        public static bool IsDateOrTimeType(string dataType)
        {
            return IsContainsFlag(dataType, DateOrTimeTypeFlags); 
        }

        public static bool IsDatetimeOrTimestampType(string dataType)
        {
            return IsContainsFlag(dataType, DatetimeOrTimestampTypeFlags);
        }

        public static bool IsSpecialDataType(string dataType)
        {
            return IsContainsFlag(dataType, SpecialDataTypeFlags);
        }

        private static bool IsContainsFlag(string value, string[] flags)
        {
            return flags.Any(item => value.ToLower().Contains(item.ToLower()));
        }

        public static bool IsUserDefinedType(TableColumn column)
        {
            string dataType = column.DataType;

            //although for its owned database, these are udt, but as a whole, they are not.
            if (dataType == "geography" || dataType == "geometry" || dataType == "st_geometry") 
            {
                return false;
            }

            return column.IsUserDefined;
        }       

        public static DataTypeInfo GetDataTypeInfo(string dataType)
        {
            DataTypeInfo dataTypeInfo = new DataTypeInfo();           

            int index = dataType.IndexOf("(");

            if (index > 0)
            {
                if (dataType.Substring(index + 1).IndexOf('(') > 0 || !dataType.Trim().EndsWith(")"))
                {
                    dataTypeInfo.DataType = dataType;
                }
                else
                {
                    dataTypeInfo.DataType = dataType.Substring(0, index).Trim();
                    dataTypeInfo.Args = dataType.Substring(index).Trim('(', ')').Trim();
                }
            }
            else
            {
                dataTypeInfo.DataType = dataType;
            }

            return dataTypeInfo;
        }

        public static DataTypeInfo GetDataTypeInfoByRegex(string dataType)
        {
            DataTypeInfo dataTypeInfo = new DataTypeInfo();

            Regex regex = new Regex("([(][0-9]+[)])");

            var matches = regex.Matches(dataType);

            List<string> args = new List<string>();

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    dataType = dataType.Replace(match.Value, "");
                    args.Add(match.Value.Trim('(', ')'));
                }
            }

            dataTypeInfo.DataType = dataType;

            if (args.Count > 0)
            {
                dataTypeInfo.Args = string.Join(",", args);
            }

            return dataTypeInfo;
        }

        public static DataTypeInfo GetDataTypeInfoByTableColumn(TableColumn column)
        {
            return  new DataTypeInfo()
            {
                DataType = column.DataType,
                MaxLength = column.MaxLength,
                Precision = column.Precision,
                Scale = column.Scale,
                IsIdentity = column.IsIdentity
            };
        }  
        
        public static void SetDataTypeInfoToTableColumn(DataTypeInfo dataTypeInfo, TableColumn column)
        {
            column.DataType = dataTypeInfo.DataType;
            column.MaxLength = dataTypeInfo.MaxLength;
            column.Precision = dataTypeInfo.Precision;
            column.Scale = dataTypeInfo.Scale;
        }
    }
}
