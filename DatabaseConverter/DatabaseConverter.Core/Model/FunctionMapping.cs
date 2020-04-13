namespace DatabaseConverter.Model
{
    public class FunctionMapping
    {
        public string DbType { get; set; }
        public string Function { get; set; }
        public FunctionMappingDirection Direction { get; set; }
    }

    public enum FunctionMappingDirection
    {
        INOUT = 0,
        IN = 1,
        OUT = 2
    }
}
