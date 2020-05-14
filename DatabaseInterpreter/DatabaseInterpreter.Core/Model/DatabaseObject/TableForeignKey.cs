using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class TableForeignKey:TableChild
    {
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
        public string ReferencedTableName { get; set; }
        public string ReferencedColumnName { get; set; }
        public bool UpdateCascade { get; set; }
        public bool DeleteCascade { get; set; }

        public string TableFullName => this.Owner + "." + this.TableName;
        public string ReferencedTableFullName=> this.Owner + "." + this.ReferencedTableName;
    }
}
