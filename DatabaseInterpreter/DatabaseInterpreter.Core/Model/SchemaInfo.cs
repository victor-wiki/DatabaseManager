using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class SchemaInfo
    {
        public List<UserDefinedType> UserDefinedTypes { get; set; } = new List<UserDefinedType>();
        public List<Function> Functions { get; set; } = new List<Function>();
        public List<Table> Tables { get; set; } = new List<Table>();
        public List<View> Views { get; set; } = new List<View>();
        public List<Trigger> Triggers { get; set; } = new List<Trigger>();       
        public List<Procedure> Procedures { get; set; } = new List<Procedure>();
        public List<TableColumn> TableColumns { get; set; } = new List<TableColumn>();
        public List<TablePrimaryKey> TablePrimaryKeys { get; set; }= new List<TablePrimaryKey>();
        public List<TableForeignKey> TableForeignKeys { get; set; }= new List<TableForeignKey>();
        public List<TableIndex> TableIndexes { get; set; } = new List<TableIndex>();       

        public Table PickupTable { get; set; }       
    }
}
