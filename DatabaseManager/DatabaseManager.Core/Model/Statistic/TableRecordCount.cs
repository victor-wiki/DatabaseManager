namespace DatabaseManager.Core.Model
{
    public class TableRecordCount
    {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public int RecordCount { get; set; }
    }
}
