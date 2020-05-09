namespace DatabaseInterpreter.Model
{
    public class TablePrimaryKey : TableColumnChild
    {       
        public bool IsDesc { get; set; }
        public bool Clustered { get; set; }
    }
}
