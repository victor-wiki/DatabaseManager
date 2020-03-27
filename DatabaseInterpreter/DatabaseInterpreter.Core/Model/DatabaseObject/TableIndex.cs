namespace DatabaseInterpreter.Model
{
    public class TableIndex : TableColumnChild
    {           
        public bool IsUnique { get; set; }       
        public bool IsDesc { get; set; }
    }
}
