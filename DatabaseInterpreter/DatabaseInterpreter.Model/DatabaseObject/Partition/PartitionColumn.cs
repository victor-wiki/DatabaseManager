namespace DatabaseInterpreter.Model
{
    public class PartitionColumn
    {
        public string TableName {  get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
    }
}
