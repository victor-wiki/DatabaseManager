namespace DatabaseInterpreter.Model
{
    public class TableIndex : TableChild
    {           
        public bool IsUnique { get; set; }
        public string ColumnName { get; set; }   
        public bool IsDesc { get; set; }
    }
}
