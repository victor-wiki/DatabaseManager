using DatabaseInterpreter.Model;
using System;

namespace DatabaseConverter.Core
{
    public class DataTransferException: ConvertException
    {
        public override string ObjectType => nameof(Table);

        public DataTransferException(Exception ex) : base(ex) { }
    }
}
