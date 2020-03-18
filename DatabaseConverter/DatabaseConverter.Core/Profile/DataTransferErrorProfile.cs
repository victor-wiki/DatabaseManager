namespace DatabaseConverter.Profile
{
    public class DataTransferErrorProfile
    {
        public string SourceServer { get; set; }
        public string SourceDatabase { get; set; }
        public string SourceTableName { get; set; }

        public string TargetServer { get; set; }
        public string TargetDatabase { get; set; }
        public string TargetTableName { get; set; }
    }
}
