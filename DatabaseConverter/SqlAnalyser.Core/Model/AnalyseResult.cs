namespace SqlAnalyser.Model
{
    public class AnalyseResult
    {
        public SqlSyntaxError Error { get; set; }
        public bool HasError => this.Error != null;
        public CommonScript Script { get; internal set; }
    }
}
