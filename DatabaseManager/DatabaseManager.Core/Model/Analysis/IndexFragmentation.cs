namespace DatabaseManager.Core.Model
{
    public class IndexFragmentation
    {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string IndexName { get; set; }
        public string FragmentationPercent { get; set; }
    }
}
