using System.IO;
using System.Reflection;

namespace DatabaseInterpreter.Utility
{
    public class PathHelper
    {
        public static string GetAssemblyFolder()
        {
            string dllFolder = Assembly.GetExecutingAssembly().CodeBase;
            return Path.GetDirectoryName(dllFolder.Substring(8, dllFolder.Length - 8));
        }
    }
}
