namespace SqlAnalyser.Model
{
    public class SetStatement : Statement
    {
        public TokenInfo Key { get; set; }
        public TokenInfo Value { get; set; }
    }
}
