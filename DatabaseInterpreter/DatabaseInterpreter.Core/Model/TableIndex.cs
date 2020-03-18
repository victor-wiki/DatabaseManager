namespace DatabaseInterpreter.Model
{
    public class TableIndex : DatabaseObject
    {    
        public string TableName { get; set; }      
        public bool IsUnique { get; set; }
        public string ColumnName { get; set; }   
        public bool IsDesc { get; set; }
    }
}
