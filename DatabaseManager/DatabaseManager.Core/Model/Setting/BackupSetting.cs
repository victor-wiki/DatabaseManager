using DatabaseInterpreter.Model;

namespace DatabaseManager.Core.Model
{
    public class BackupSetting
    {
        public string DatabaseType { get; set; }
        public string ClientToolFilePath { get; set; }
        public string SaveFolder { get; set; }
        public bool ZipFile { get; set; }
    }
}
