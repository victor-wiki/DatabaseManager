namespace DatabaseInterpreter.Model
{
    public class SelectionInfo
    {
        public string[] UserDefinedTypeNames { get; set; }
        public string[] FunctionNames { get; set; }
        public string[] TableNames { get; set; }       
        public string[] ViewNames { get; set; }
        public string[] TriggerNames { get; set; }      
        public string[] ProcedureNames { get; set; }
    }
}
