using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class PartitionInfo
    {
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public string Name { get; set; }
        public string ParentTableSchema { get; set; }
        public string ParentTableName { get; set; }
        public string HighValue { get; set; }        
        public string Tablespace { get; set; }
        public string Bound { get; set; }
        public int Order { get; set; }
        public List<PartitionInfo> SubPartitions { get; set; }
    }
}
