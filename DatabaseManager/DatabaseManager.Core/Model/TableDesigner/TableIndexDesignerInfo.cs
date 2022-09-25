using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.ComponentModel;

namespace DatabaseManager.Model
{
    public class TableIndexDesignerInfo
    {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string OldName { get; set; }
        public string Name { get; set; }
        public string OldType { get; set; }
        public bool IsPrimary { get; set; }
        public string Type { get; set; }
        public bool Clustered { get; set; }
        public List<IndexColumn> Columns { get; set; } = new List<IndexColumn>();
        public string Comment { get; set; }

        public TableIndexExtraPropertyInfo ExtraPropertyInfo { get; set; }
    }  

    public class TableIndexExtraPropertyInfo
    {
        [Category("Clustered"), Description("Clustered"), ReadOnly(false)]
        public bool Clustered { get; set; } = true;
    }
}
