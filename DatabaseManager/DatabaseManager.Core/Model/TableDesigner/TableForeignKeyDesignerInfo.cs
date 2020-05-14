using System;
using System.Collections.Generic;
using System.Text;
using DatabaseInterpreter.Model;

namespace DatabaseManager.Model
{
    public class TableForeignKeyDesignerInfo: TableForeignKey
    {
        public string OldName { get; set; }
    }
}
