namespace SqlAnalyser.Model
{
    public class AnalyseResult
    {
        public bool HasError { get; internal set; }
        public CommonScript Script { get; internal set; }
    }
}
