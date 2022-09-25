namespace SqlAnalyser.Model
{
    public class LoopExitStatement : Statement
    {
        public TokenInfo Condition { get; set; }
        public bool IsCursorLoopExit { get; set; }
    }
}
