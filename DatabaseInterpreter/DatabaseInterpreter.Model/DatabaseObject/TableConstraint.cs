namespace DatabaseInterpreter.Model
{
    public class TableConstraint: TableChild
    {        
        public string ColumnName { get; set; }
        public string Definition { get; set; }
    }
}
