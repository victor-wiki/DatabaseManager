namespace DatabaseManager.Model
{
    public class ContentSaveResult
    {
        public bool IsOK { get; set; }
        public object ResultData { get; set; }
        public string Message => this.ResultData?.ToString();
    }
}
