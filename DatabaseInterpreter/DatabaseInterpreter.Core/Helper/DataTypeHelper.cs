using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseInterpreter.Core
{
    public class DataTypeHelper
    {
        public static readonly string[] CharTypeFlags = { "char" };
        public static readonly string[] TextTypeFlags = { "text" };
        public static readonly string[] BinaryTypeFlags = { "binary" };
        public static readonly string[] DatetimeTypeFlags = { "date", "time" };
        public static List<string> SpecialDataTypes = new List<string>() { "SqlHierarchyId", "SqlGeography", "SqlGeometry" };

        public static bool IsCharType(string dataType)
        {
            return CharTypeFlags.Any(item => dataType.ToLower().Contains(item));
        }

        public static bool IsBinaryType(string dataType)
        {
            return BinaryTypeFlags.Any(item => dataType.ToLower().Contains(item));
        }


        public static bool StartWithN(string dataType)
        {
            return dataType.StartsWith("n", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTextType(string dataType)
        {
            return TextTypeFlags.Any(item => dataType.ToLower().Contains(item));
        }

        public static bool IsDatetimeType(string dataType)
        {
            return DatetimeTypeFlags.Any(item => dataType.ToLower().Contains(item));
        }

        public static DataTypeInfo GetDataTypeInfo(DbInterpreter dbInterpreter, string dataType)
        {
            DataTypeInfo dataTypeInfo = new DataTypeInfo();

            if(dbInterpreter!=null)
            {
                dataType = dataType.Trim(dbInterpreter.QuotationLeftChar, dbInterpreter.QuotationRightChar);
            }           

            int index = dataType.IndexOf("(");

            if (index > 0)
            {
                if (dataType.Substring(index + 1).IndexOf('(') > 0 || !dataType.Trim().EndsWith(")"))
                {
                    dataTypeInfo.DataType = dataType;
                }
                else
                {
                    dataTypeInfo.DataType = dataType.Substring(0, index);
                    dataTypeInfo.Args = dataType.Substring(index).Trim('(', ')').Trim();
                }
            }
            else
            {
                dataTypeInfo.DataType = dataType;
            }

            return dataTypeInfo;
        }

        public static DataTypeInfo GetDataTypeInfo(string dataType)
        {
            return GetDataTypeInfo(null, dataType);
        }
    }
}
