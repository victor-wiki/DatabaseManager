namespace DatabaseManager.Model
{
    public class QueryResult
    {
        public QueryResultType ResultType { get; set; }
        public object Result;   
        public bool HasError { get; set; }
        public bool DoNothing { get; set; }
    }

    public enum QueryResultType
    {
        Unknown = 0,
        Grid = 1,
        Text = 2
    }
}
