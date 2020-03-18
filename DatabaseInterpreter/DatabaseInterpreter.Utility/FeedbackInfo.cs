namespace DatabaseInterpreter.Utility
{
    public class FeedbackInfo
    {
        public object Owner { get; set; }
        public FeedbackInfoType InfoType { get; set; }
        public string Message { get; set; }
    }

    public enum FeedbackInfoType
    {
        Info=0,
        Error=1
    }
}
