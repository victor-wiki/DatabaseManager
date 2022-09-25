using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class TablePrimaryKey : TableChild
    {
        public bool Clustered { get; set; } = true;
        public List<IndexColumn> Columns { get; set; } = new List<IndexColumn>();
    }

    public class TablePrimaryKeyItem : TableColumnChild
    {
        public bool IsDesc { get; set; }
        public bool Clustered { get; set; } = true;          
    }
}
