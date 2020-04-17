namespace SqlAnalyser.Model
{
    public class CloseCursorStatement : Statement
    {
        public TokenInfo CursorName { get; set; }
        public bool IsEnd { get; set; }
    }
}
