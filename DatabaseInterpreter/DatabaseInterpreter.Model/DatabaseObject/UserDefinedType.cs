using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class UserDefinedTypeItem : DatabaseObject
    {
        public string AttrName { get; set; }
        public string DataType { get; set; }
        public long? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }

        public bool IsRequired => !IsNullable;

        public bool IsNullable { get; set; }
    }

    public class UserDefinedType : DatabaseObject
    {
        public List<UserDefinedTypeItem> Attributes = new List<UserDefinedTypeItem>();
    }
}
