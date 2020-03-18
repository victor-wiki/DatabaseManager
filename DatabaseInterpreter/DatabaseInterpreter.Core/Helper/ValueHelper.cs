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
    }
}
