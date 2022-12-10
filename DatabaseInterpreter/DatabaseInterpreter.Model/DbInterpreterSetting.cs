using DatabaseInterpreter.Model;

namespace DatabaseInterpreter.Model
{
    public class DbInterpreterSetting
    {
        public int CommandTimeout { get; set; } = 600;
        public int DataBatchSize { get; set; } = 500;
        public bool ShowBuiltinDatabase { get; set; }           
        public string MySqlCharset { get; set; } = "utf8mb4";
        public string MySqlCharsetCollation { get; set; } = "utf8mb4_bin";
        public bool ExcludePostgresExtensionObjects { get; set; } = true;       
        public bool EnableLog { get; set; } = true;
        public DbObjectNameMode DbObjectNameMode { get; set; } = DbObjectNameMode.WithQuotation;
        public LogType LogType { get; set; } = LogType.Info | LogType.Error;
        public bool NotCreateIfExists { get; set; }          
    }

    public enum DbObjectNameMode
    {
        WithQuotation = 0,
        WithoutQuotation = 1,       
    }
}
