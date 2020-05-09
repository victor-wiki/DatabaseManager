using System;
using System.Collections.Generic;
using System.Text;
using DatabaseInterpreter.Model;

namespace DatabaseManager.Model
{
    public class SchemaDesignerInfo: SchemaInfo
    {
        public TableDesignerInfo TableDesignerInfo { get; set; }
        public List<TableColumnDesingerInfo> TableColumnDesingerInfos = new List<TableColumnDesingerInfo>();
    }
}
