namespace DatabaseInterpreter.Model
{
    public class RoutineParameter : DatabaseObject
    {
        public string RoutineType { get; set; }
        public string RountineName { get; set; }
        public string DataType { get; set; }
        public long? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public bool IsOutput { get; set; }
    } 
}
