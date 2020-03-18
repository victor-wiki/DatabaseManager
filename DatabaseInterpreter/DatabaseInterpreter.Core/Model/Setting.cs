namespace DatabaseInterpreter.Model
{
    public class Setting
    {
        public int CommandTimeout { get; set; } = 600;
        public int DataBatchSize { get; set; } = 500;
        public bool ShowBuiltinDatabase { get; set; }
        public string MySqlCharset { get; set; } = "utf8mb4";
        public string MySqlCharsetCollation { get; set; } = "utf8mb4_bin";
        public bool EnableLog { get; set; } = true;      
    }
}
