namespace SqlAnalyser.Model
{
    public class OpenCursorStatement : Statement
    {
        public TokenInfo CursorName { get; set; }
    }
}
