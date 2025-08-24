namespace DatabaseManager.FileUtility
{
    public class ExportDataOption
    {
        public ExportFileType FileType { get; set; }
        public bool ShowColumnNames { get; set; } = true;
        public string FilePath { get; set; }
    }

    public enum ExportFileType
    {
        None = 0,
        CSV = 1,
        EXCEL = 2
    }
}
