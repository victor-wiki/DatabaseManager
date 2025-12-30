using DatabaseInterpreter.Model;
using System.Collections.Generic;

namespace DatabaseManager.Core.Model
{
    public class TableDesignerGenerateScriptsData
    {
        public Table Table { get; set; }
        public List<Script> Scripts { get; set; } = new List<Script>();
    }
}
