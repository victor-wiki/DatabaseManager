namespace SqlAnalyser.Model
{
    public class SetStatement : Statement
    {
        public TokenInfo Key { get; set; }
        public TokenInfo Value { get; set; }

        public bool IsSetUserVariable { get; set; }
        public bool IsSetCursorVariable { get; set; }
        public SelectStatement ValueStatement { get; set; }
    }
}
