using System.Collections.Generic;

namespace DatabaseManager.Core.Model
{
    public class DataCompareDifference
    {
        public string Type { get; set; }
        public DataCompareDifference Parent { get; set; }

        public string TableName { get; set; }
        public long? SourceRecordCount { get; set; }
        public long? TargetRecordCount { get; set; }
        public long? DifferentCount { get; set; }
        public long? OnlyInSourceCount { get; set; }
        public long? OnlyInTargetCount { get; set; }
        public long? IdenticalCount { get; set; }
        public bool IsIdentical { get; set; }
        public DataCompareResultDetail Detail { get; set; }

        public List<DataCompareDifference> SubDifferences { get; set; } = new List<DataCompareDifference>();

        public bool IsRoot => this.TableName == null || this.SubDifferences.Count > 0;
    }
}
