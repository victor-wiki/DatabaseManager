namespace DatabaseInterpreter.Model
{
    public class SchemaInfoFilter
    {
        public bool Strict { get; set; }
        public DatabaseObjectType DatabaseObjectType = DatabaseObjectType.None;
        public string[] UserDefinedTypeNames { get; set; }
        public string[] FunctionNames { get; set; }
        public string[] TableNames { get; set; }       
        public string[] ViewNames { get; set; }       
        public string[] ProcedureNames { get; set; }
    }
}
