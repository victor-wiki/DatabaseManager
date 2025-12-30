using SqlAnalyser.Model;

namespace DatabaseManager.Core.Model
{
    public class SelectScriptAnalyseResult
    {
        public string Script { get; set; }
        public bool HasTableName { get; set; }
        public bool HasAlias { get; set; }
        public string CountScript { get; set; }
        public bool HasLimitCount { get; set; }
        public SelectStatement SelectStatement { get; set; }
        public string SpecialPaginationScript { get; set; }
    }
}
