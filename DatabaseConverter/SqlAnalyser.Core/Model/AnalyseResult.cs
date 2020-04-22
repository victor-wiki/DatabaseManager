namespace SqlAnalyser.Model
{
    public class AnalyseResult
    {
        public SqlSyntaxError Error { get; internal set; }
        public bool HasError => this.Error != null;
        public CommonScript Script { get; internal set; }
    }
}
