namespace SqlAnalyser.Model
{
    public class SqlRuleAnalyserOption
    {
        public bool ParseTokenChildren { get; set; }
        public bool ExtractFunctions { get; set; }
        public bool ExtractFunctionChildren { get; set; }
        public bool IsCommonScript { get; set; }
    }
}
