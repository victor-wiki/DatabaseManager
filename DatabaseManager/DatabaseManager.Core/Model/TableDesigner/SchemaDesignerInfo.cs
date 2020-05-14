using System;
using System.Collections.Generic;
using System.Text;
using DatabaseInterpreter.Model;

namespace DatabaseManager.Model
{
    public class SchemaDesignerInfo
    {
        public bool IgnoreTableIndex { get; set; }
        public bool IgnoreTableForeignKey { get; set; }
        public bool IgnoreTableConstraint { get; set; }
        public TableDesignerInfo TableDesignerInfo { get; set; }
        public List<TableColumnDesingerInfo> TableColumnDesingerInfos = new List<TableColumnDesingerInfo>();
        public List<TableIndexDesignerInfo> TableIndexDesingerInfos = new List<TableIndexDesignerInfo>();
        public List<TableForeignKeyDesignerInfo> TableForeignKeyDesignerInfos = new List<TableForeignKeyDesignerInfo>();
        public List<TableConstraintDesignerInfo> TableConstraintDesignerInfos = new List<TableConstraintDesignerInfo>();
    }
}
