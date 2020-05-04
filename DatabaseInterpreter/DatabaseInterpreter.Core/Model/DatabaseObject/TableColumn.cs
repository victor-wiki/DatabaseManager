namespace DatabaseInterpreter.Model
{
    public class TableColumn: TableChild
    {            
        public string DataType { get; set; }
        public bool IsRequired => !IsNullable;
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public long? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
       
        public string DefaultValue { get; set; }
        public string Comment { get; set; }
        public bool IsUserDefined { get; set; }
        public string TypeOwner { get; set; }
        public bool IsComputed => !string.IsNullOrEmpty(this.ComputeExp);
        public string ComputeExp { get; set; }
        public string ScriptComment { get; set; }
    }
}
