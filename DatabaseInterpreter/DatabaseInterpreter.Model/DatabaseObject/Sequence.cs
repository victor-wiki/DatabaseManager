namespace DatabaseInterpreter.Model
{
    public class Sequence : DatabaseObject
    {     
        public string DataType { get; set; }
        public int StartValue { get; set; }
        public int Increment { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public bool UseCache { get; set; }
        public int CacheSize { get; set; } 
        public bool Cycled { get; set; }
        public string OwnedByTable { get; set; }
        public string OwnedByColumn { get; set; }
        public bool Ordered { get; set; }
    }
}
