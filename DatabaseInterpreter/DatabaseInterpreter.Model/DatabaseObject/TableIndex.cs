using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class TableIndex : TableChild
    {
        public bool IsPrimary { get; set; }
        public bool IsUnique { get; set; }
        public bool Clustered { get; set; }
        public string Type { get; set; }

        public List<IndexColumn> Columns { get; set; } = new List<IndexColumn>();

        public TableIndexExtraInfo ExtraInfo { get; set; }
    }   

    public class TableIndexExtraInfo
    {
        public string IndexName { get; set; }
        public bool IsPartitioned { get; set; }
        public string Compression { get; set; }
        public bool IsLocal { get; set; }
        public string Tablespace { get; set; }
        public List<TableIndexPartitionInfo> Partitions { get; set; }
    }

    public class TableIndexPartitionInfo
    {
        public string IndexName { get; set; }
        public string Name { get; set; }
        public string Compression { get; set; }
        public int Order { get; set; }
    }

    public class IndexColumn : SimpleColumn
    {       
        public bool IsDesc { get; set; }     
    }

    public class TableIndexItem : TableColumnChild
    {
        public bool IsPrimary { get; set; }
        public bool IsUnique { get; set; }
        public bool IsDesc { get; set; }
        public bool Clustered { get; set; }
        public string Type { get; set; }
    }
}
