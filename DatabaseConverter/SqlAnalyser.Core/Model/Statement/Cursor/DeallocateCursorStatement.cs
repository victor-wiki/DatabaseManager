namespace SqlAnalyser.Model
{
    public class DeallocateCursorStatement : Statement
    {
        public TokenInfo CursorName { get; set; }
    }
}
