using System.Collections.Generic;

namespace DatabaseConverter.Model
{
    public class DbConvertResult
    {
        public DbConvertResultInfoType InfoType { get; internal set; }
        public string Message { get; internal set; }
        public List<TranslateResult> TranslateResults { get; set; } = new List<TranslateResult>();
    }

    public enum DbConvertResultInfoType
    {
        Information=0,
        Warnning=1,
        Error=2
    }
}
