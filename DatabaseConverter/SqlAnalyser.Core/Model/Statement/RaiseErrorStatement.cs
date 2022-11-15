namespace SqlAnalyser.Model
{
    public class RaiseErrorStatement: Statement
    {
        public string Severity { get; set; }
        public string State { get; set; }
        public TokenInfo ErrorCode { get; set; }
        public TokenInfo Content { get; set; }
    }
}
