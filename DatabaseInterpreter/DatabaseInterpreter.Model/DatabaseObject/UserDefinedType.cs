using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class UserDefinedType : DatabaseObject
    {
        public List<UserDefinedTypeAttribute> Attributes = new List<UserDefinedTypeAttribute>();
    }

    public class UserDefinedTypeAttribute : DatabaseObject
    {
        public string TypeName { get; set; }
        public string DataType { get; set; }
        public long? MaxLength { get; set; }
        public long? Precision { get; set; }
        public long? Scale { get; set; }

        public bool IsRequired => !IsNullable;

        public bool IsNullable { get; set; }
    }
}
