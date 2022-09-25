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
