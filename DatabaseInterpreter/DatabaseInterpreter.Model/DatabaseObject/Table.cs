namespace DatabaseInterpreter.Model
{
    public class Table : DatabaseObject
    {
        public string Definition { get; set; }
        public string Comment { get; set; }       
        public int? IdentitySeed { get; set; }
        public int? IdentityIncrement { get; set; }   
        public TableExtraInfo ExtraInfo { get; set; }
    }

    public class TableExtraInfo
    {
        public bool IsPartitioned { get; set; }
        public bool IsInheritedPartition { get; set; }
        public string FilegroupName { get; set; }
        public PartitionScheme PartitionScheme { get; set; }     
        public PartitionSummary PartitionSummary { get; set; }
        public PartitionInfo PartitionInfo { get; set; }
        public string Tablespace { get; set; }
    }   
}
