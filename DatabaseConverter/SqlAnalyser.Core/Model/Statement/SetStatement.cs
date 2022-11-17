namespace SqlAnalyser.Model
{
    public class SetStatement : Statement
    {
        public TokenInfo Key { get; set; }
        public TokenInfo Value { get; set; }

        public bool IsSetUserVariable { get; set; }
    }
}
