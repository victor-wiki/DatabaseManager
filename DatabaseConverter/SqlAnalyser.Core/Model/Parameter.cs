using System;

namespace SqlAnalyser.Model
{
    public class Parameter
    {
        public TokenInfo Name { get; set; }
        public ParameterType ParameterType { get; set; }
        public TokenInfo DataType { get; set; }
        public TokenInfo DefaultValue { get; set; }
    }  

    [Flags]
    public enum ParameterType : int
    {
        NONE = 0,
        IN = 2,
        OUT = 4
    }
}
