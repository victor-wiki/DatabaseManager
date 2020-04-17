namespace SqlAnalyser.Model
{
    public class DeclareStatement : Statement
    {
        public TokenInfo Name { get; set; }
        public TokenInfo DataType { get; set; }
        public DeclareType Type { get; set; }
        public TemporaryTable Table { get; set; }
        public TokenInfo DefaultValue { get; set; }
    }

    public enum DeclareType
    {
        Variable = 0,
        Table = 1,
        Cursor = 2
    }
}
