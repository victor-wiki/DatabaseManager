using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class PartitionSummary
    {
        public string TableName { get; set; }
        public string Type { get; set; }
        public List<PartitionColumn> Columns { get; set; }
        public string Expression { get; set; }
        public string Interval { get; set; }
        public string SubType { get; set; }
        public string SubInterval { get; set; }
        public List<PartitionColumn> SubColumns{ get; set; }
        public List<PartitionInfo> Partitions { get; set; }

        public bool HasPartition => this.Partitions != null && this.Partitions.Count > 0;
        public int PartitionCount => this.Partitions == null? 0 : this.Partitions.Count;        
    }   
}
