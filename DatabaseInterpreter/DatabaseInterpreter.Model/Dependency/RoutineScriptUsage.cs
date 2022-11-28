using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class RoutineScriptUsage : DbObjectUsage
    {        
        public string ObjectType { get; set; }
        public string RefObjectType { get; set; }
        public List<string> ColumnNames { get; set; } = new List<string>();
    }
}
