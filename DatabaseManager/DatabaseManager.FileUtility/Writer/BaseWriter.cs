using System.IO;
using System.Reflection;

namespace DatabaseManager.FileUtility
{
    public class BaseWriter
    {
        public readonly string AssemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public readonly string DefaultSaveFolder = "export";
        public readonly string TemporaryFolder = "temp";

        protected void CheckFolder(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }       
    }
}
