using System.Globalization;

namespace DatabaseInterpreter.Geometry
{
    public class GeometryUtility
    {
        public static string ToInvariantString(decimal? value)
        {
            if(value == null)
            {
                return string.Empty;
            }

            return value.Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// This is to create a reference relation to the calling assembly.
        /// The "Oracle.ManagedDataAccess" use "AppDomain.CurrentDomain.GetAssemblies" to look up custom udt class.
        /// </summary>
        public static void Hook()
        {
            //nothing to do
        }
    }
}
