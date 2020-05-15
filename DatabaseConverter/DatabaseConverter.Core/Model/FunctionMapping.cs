namespace DatabaseConverter.Model
{
    public class FunctionMapping
    {
        public string DbType { get; set; }
        public string Function { get; set; }
        public FunctionMappingDirection Direction { get; set; }
        public string Args { get; set; }
    }

    public enum FunctionMappingDirection
    {
        INOUT = 0,
        IN = 1,
        OUT = 2
    }

    public struct MappingFunctionInfo
    {
        public string Name { get; set; }
        public string Args { get; set; }
    }
}
