namespace DatabaseManager.Core.Model
{
    public class DataImportColumnMapping
    {
        public string TableColumName { get; set; }
        public string FileColumnName { get; set; }
        public string ReferencedTableName { get; set; }
        public string ReferencedTableColumnName { get; set; }
        public string ReferencedTableColumnDataType { get; set; }
    }
}
