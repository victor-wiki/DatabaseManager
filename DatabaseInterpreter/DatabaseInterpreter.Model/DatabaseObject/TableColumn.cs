namespace DatabaseInterpreter.Model
{
    public class TableColumn: TableChild
    {            
        public string DataType { get; set; }
        public string DataTypeSchema { get; set; }
        public bool IsRequired => !IsNullable;
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public long? MaxLength { get; set; }
        public long? Precision { get; set; }
        public long? Scale { get; set; }       
        public string DefaultValue { get; set; }      
        public bool IsUserDefined { get; set; }       
        public bool IsComputed => !string.IsNullOrEmpty(this.ComputeExp);
        public string ComputeExp { get; set; }
        public string ScriptComment { get; set; }
    }
}
