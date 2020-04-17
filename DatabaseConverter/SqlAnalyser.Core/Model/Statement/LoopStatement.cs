namespace SqlAnalyser.Model
{
    public class LoopStatement : WhileStatement
    {
        public LoopType Type { get; set; }
        public TokenInfo Name { get; set; }
    }

    public enum LoopType
    {
        LOOP = 0,
        WHILE = 1,
        FOR = 2
    }
}
