using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class TableInfo
    {
        public bool IsTemporary { get; set; }
        public bool IsGlobal { get; set; } = true;
        public TokenInfo Name { get; set; }
        public int? IdentitySeed { get; set; }
        public int? IdentityIncrement { get; set; }
        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();

        public List<ConstraintInfo> Constraints { get; set; }
        public SelectStatement SelectStatement { get; set; }

        public bool HasTableConstraints => this.Constraints != null && this.Constraints.Count > 0;
    }   
}
