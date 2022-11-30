using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class RoutineScriptUsage : DbObjectUsage
    {        
        public List<string> ColumnNames { get; set; } = new List<string>();
    }
}
