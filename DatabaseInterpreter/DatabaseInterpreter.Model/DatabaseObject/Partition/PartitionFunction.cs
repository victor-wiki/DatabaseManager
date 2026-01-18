using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class PartitionFunction
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public PartitionFunctionDataTypeInfo DataTypeInfo { get;set; }
        public bool IsOnRight { get; set; }
        public List<string> Values { get; set; }
    }    
}
