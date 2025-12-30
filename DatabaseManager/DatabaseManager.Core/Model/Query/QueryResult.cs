using DatabaseManager.Core.Model;

namespace DatabaseManager.Core.Model
{
    public class QueryResult
    {
        public QueryResultType ResultType { get; set; }
        public object Result { get; set; }
        public bool HasError { get; set; }
        public bool DoNothing { get; set; }
        public long? TotalCount { get; set; }
        public SelectScriptAnalyseResult SelectScriptAnalyseResult { get; set; }
    }

    public enum QueryResultType
    {
        Unknown = 0,
        Grid = 1,
        Text = 2
    }
}
