namespace DatabaseInterpreter.Model
{
    public class TableForeignKey: TableColumnChild
    {             
        public string ReferencedTableName { get; set; }
        public string ReferencedColumnName { get; set; }
        public bool UpdateCascade { get; set; }
        public bool DeleteCascade { get; set; }

        public string TableFullName => this.Owner + "." + this.TableName;
        public string ReferencedTableFullName=> this.Owner + "." + this.ReferencedTableName;
    }
}
