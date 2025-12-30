namespace DatabaseManager.Core.Model
{
    public class TableColumnContentMaxLength
    {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int ContentMaxLength { get; set; }
    }
}
