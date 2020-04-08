using DatabaseInterpreter.Model;
using System;

namespace DatabaseConverter.Core
{
    public class ScriptConvertException<T> : ConvertException
    {
        public override string ObjectType => typeof(T).Name;

        public ScriptConvertException(Exception ex) : base(ex) { }
    }
}
