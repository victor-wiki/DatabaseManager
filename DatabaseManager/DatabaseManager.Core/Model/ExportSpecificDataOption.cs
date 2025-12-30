using DatabaseManager.FileUtility;
using System.Collections.Generic;

namespace DatabaseManager.Core.Model
{
    public class ExportSpecificDataOption : ExportDataOption
    {
        public bool ExportAllThatMeetCondition { get; set; }
        public List<long> PageNumbers { get; set; } = new List<long>();
        public long PageCount { get; set; }
        public int PageSize { get; set; }
        public string OrderColumns { get; set; }
        public string ConditionClause { get; set; }
    }
}
