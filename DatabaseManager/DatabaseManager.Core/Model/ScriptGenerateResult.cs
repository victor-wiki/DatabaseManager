using DatabaseInterpreter.Model;
using System.Collections.Generic;

namespace DatabaseManager.Core.Model
{
    public class ScriptGenerateResult
    {
        public string Script { get; set; }
        public List<RoutineParameter> Parameters;
    }
}
