using DatabaseInterpreter.Utility;
using System.IO;

namespace DatabaseConverter.Core
{
    public class MappingManager
    {
        public static string ConfigRootFolder => Path.Combine(PathHelper.GetAssemblyFolder(), "Config");
    }
}
