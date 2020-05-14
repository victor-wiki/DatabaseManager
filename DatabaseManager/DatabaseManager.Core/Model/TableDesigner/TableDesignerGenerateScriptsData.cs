using DatabaseInterpreter.Model;
using System.Collections.Generic;

namespace DatabaseManager.Model
{
    public class TableDesignerGenerateScriptsData
    {
        public Table Table { get; set; }
        public List<Script> Scripts { get; set; } = new List<Script>();
    }
}
