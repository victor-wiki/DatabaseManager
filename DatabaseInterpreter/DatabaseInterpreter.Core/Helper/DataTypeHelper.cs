using System.Linq;

namespace  DatabaseInterpreter.Core
{
    public class DataTypeHelper
    {
        public static readonly string[] CharTypeFlags = { "char" };
        public static readonly string[] TextTypeFlags = { "text" };
        public static readonly string[] DatetimeTypeFlags = { "date", "time" };

        public static bool IsCharType(string dataType)
        {
            return CharTypeFlags.Any(item => dataType.ToLower().Contains(item));
        }

        public static bool IsTextType(string dataType)
        {
            return TextTypeFlags.Any(item => dataType.ToLower().Contains(item));
        }

        public static bool IsDatetimeType(string dataType)
        {
            return DatetimeTypeFlags.Any(item => dataType.ToLower().Contains(item));
        }
    }
}
