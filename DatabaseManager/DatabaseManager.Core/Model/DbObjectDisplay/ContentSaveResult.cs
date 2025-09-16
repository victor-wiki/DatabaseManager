namespace DatabaseManager.Model
{
    public class ContentSaveResult
    {
        public bool IsOK { get; set; }
        public object ResultData { get; set; }
        public string Message => this.ResultData?.ToString();
        public ContentSaveResultInfoType InfoType = ContentSaveResultInfoType.Info;
    }

    public enum ContentSaveResultInfoType
    {
        Info = 1,
        Warning = 2,
        Error = 3
    }
}
