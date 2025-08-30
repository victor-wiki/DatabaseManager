using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;

namespace DatabaseConverter.Model
{
    public class DbConvertResult
    {
        public DbConvertResultInfoType InfoType { get; internal set; }     
        public string Message { get; internal set; }
        public Type ExceptionType { get; set; }
        public List<TranslateResult> TranslateResults { get; set; } = new List<TranslateResult>();
        public SchemaInfo TranslatedSchemaInfo { get; set; }
    }

    public enum DbConvertResultInfoType
    {
        Information=0,
        Warnning=1,
        Error=2
    }
}
