namespace DatabaseInterpreter.Model
{
    public class TablePrimaryKey:DatabaseObject
    {       
        public string TableName { get; set; }       
        public string ColumnName { get; set; }        
        public bool IsDesc { get; set; }
    }
}
