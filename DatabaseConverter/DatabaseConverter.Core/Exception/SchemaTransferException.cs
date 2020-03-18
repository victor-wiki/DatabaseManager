using DatabaseInterpreter.Model;
using System;

namespace DatabaseConverter.Core
{
    public class SchemaTransferException : ConvertException
    {
        public override string ObjectType => nameof(DatabaseObject);

        public SchemaTransferException(Exception ex) : base(ex) { }
    }
}
