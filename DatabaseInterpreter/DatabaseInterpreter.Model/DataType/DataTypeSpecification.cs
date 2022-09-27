using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class DataTypeSpecification
    {
        public string Name { get; set; }
        public string Format { get; set; }
        public string Args { get; set; }
        public string Range { get; set; }
        public bool Optional { get; set; }
        public string Default { get; set; }
        public string DisplayDefault { get; set; }
        public bool AllowMax { get; set; }
        public string MapTo { get; set; }
        public bool IndexForbidden { get; set; }
        public bool AllowIdentity { get; set; }     
        public List<DataTypeArgument> Arugments { get; set; } = new List<DataTypeArgument>();
    }

    public struct DataTypeArgument
    {
        public string Name { get; set; }
        public ArgumentRange? Range { get; set; }
    }

    public struct ArgumentRange
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }
}
