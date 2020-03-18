namespace DatabaseInterpreter.Model
{
    public class TableForeignKey: DatabaseObject
    {      
        public string TableName { get; set; }       
        public string ColumnName { get; set; }
        public string ReferencedTableName { get; set; }
        public string ReferencedColumnName { get; set; }
        public bool UpdateCascade { get; set; }
        public bool DeleteCascade { get; set; }

        public string TableFullName => this.Owner + "." + this.TableName;
        public string ReferencedTableFullName=> this.Owner + "." + this.ReferencedTableName;
    }
}
