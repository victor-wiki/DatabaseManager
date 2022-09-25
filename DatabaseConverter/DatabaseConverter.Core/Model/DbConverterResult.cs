namespace DatabaseConverter.Model
{
    public class DbConverterResult
    {
        public DbConverterResultInfoType InfoType { get; internal set; }
        public string Message { get; internal set; }
    }

    public enum DbConverterResultInfoType
    {
        Information=0,
        Warnning=1,
        Error=2
    }
}
