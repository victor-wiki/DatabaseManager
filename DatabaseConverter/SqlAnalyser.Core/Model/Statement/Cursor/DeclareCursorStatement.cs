namespace SqlAnalyser.Model
{
    public class DeclareCursorStatement : Statement
    {
        public TokenInfo CursorName { get; set; }
        public SelectStatement SelectStatement { get; set; }
    }
}
