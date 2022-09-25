namespace DatabaseInterpreter.Model
{
    public class TableColumnChild: TableChild
    {        
        public string ColumnName { get; set; }
    }

    public class SimpleColumn
    {
        public string ColumnName { get; set; }
        public int Order { get; set; }        
    }
}
