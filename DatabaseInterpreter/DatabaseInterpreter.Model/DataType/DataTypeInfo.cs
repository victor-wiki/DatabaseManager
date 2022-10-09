namespace DatabaseInterpreter.Model
{
    public class DataTypeInfo
    {
        public string DataType { get; set; }       
        public long? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public bool IsIdentity { get; set; }
        public string Args { get; set; }
       
    }
}
