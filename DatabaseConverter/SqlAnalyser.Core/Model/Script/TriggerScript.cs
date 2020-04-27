using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class TriggerScript : CommonScript
    {
        public TokenInfo TableName { get; set; }
        public TriggerTime Time { get; set; }
        public List<TriggerEvent> Events { get; set; } = new List<TriggerEvent>();
        public string Behavior { get; set; }

        public TokenInfo OtherTriggerName { get; set; }
        public TokenInfo Condition { get; set; }
    }

    public enum TriggerTime
    {
        BEFORE,
        AFTER,
        INSTEAD_OF
    }

    public enum TriggerEvent
    {
        INSERT,
        UPDATE,
        DELETE
    }
}
