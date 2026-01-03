namespace DatabaseManager.Core.Model
{
    public class CustomColumn
    {
        public string ColumnName { get; set; }       
        public string DisplayName { get; set; }
    }

    public class DataExportColumn : CustomColumn
    {
        public string DataType { get; set; }
    }
}
