using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class TableForeignKey:TableChild
    {
        public string ReferencedSchema { get; set; }
        public string ReferencedTableName { get; set; }
        public bool UpdateCascade { get; set; }
        public bool DeleteCascade { get; set; }

        public List<ForeignKeyColumn> Columns { get; set; } = new List<ForeignKeyColumn>();
    }

    public class ForeignKeyColumn: SimpleColumn
    {      
        public string ReferencedColumnName { get; set; }       
    }

    public class TableForeignKeyItem: TableColumnChild
    {
        public string ReferencedSchema { get; set; }
        public string ReferencedTableName { get; set; }
        public string ReferencedColumnName { get; set; }
        public bool UpdateCascade { get; set; }
        public bool DeleteCascade { get; set; }

        public string TableFullName => string.IsNullOrEmpty(this.Schema)? this.TableName:  this.Schema + "." + this.TableName;
        public string ReferencedTableFullName => string.IsNullOrEmpty(this.Schema) ? this.ReferencedTableName :  this.Schema + "." + this.ReferencedTableName;
    }
}
