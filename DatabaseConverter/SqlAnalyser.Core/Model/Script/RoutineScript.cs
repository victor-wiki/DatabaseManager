using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class RoutineScript : CommonScript
    {
        public RoutineType Type { get; set; }

        public TokenInfo ReturnDataType { get; set; }

        public List<Parameter> Parameters { get; set; } = new List<Parameter>();

        public TemporaryTable ReturnTable { get; set; }
    }

    public enum RoutineType
    {
        UNKNOWN = 0,
        FUNCTION = 1,
        PROCEDURE = 2,
        TRIGGER = 3
    }
}
