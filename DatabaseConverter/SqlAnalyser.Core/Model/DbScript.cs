using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class DbScript
    {
        public TokenInfo Owner { get; set; }
        public TokenInfo Name { get; set; }

        public string FullName
        {
            get
            {
                if (this.Owner == null || this.Owner.Symbol == null)
                {
                    return this.Name.ToString();
                }
                else
                {
                    return this.Owner + "." + this.Name;
                }
            }
        }
    }

    public class CommonScript : DbScript
    {
        public List<TokenInfo> Functions { get; set; } = new List<TokenInfo>();
        public List<Statement> Statements { get; set; } = new List<Statement>();
    }

    public class ViewScript : CommonScript
    {
    }

    public class RoutineScript : CommonScript
    {
        public RoutineType Type { get; set; }

        public TokenInfo ReturnDataType { get; set; }

        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
       
        public TemporaryTable ReturnTable{ get; set; }
    }

    public class TriggerScript : CommonScript
    {
        public TokenInfo TableName { get; set; }
        public TriggerTime Time { get; set; }
        public List<TriggerEvent> Events { get; set; } = new List<TriggerEvent>();
        public string Behavior { get; set; }

        public TokenInfo OtherTriggerName { get; set; }
    }

    public enum TriggerTime
    {
        BEFORE,
        AFTER
    }

    public enum TriggerEvent
    {
        INSERT,
        UPDATE,
        DELETE
    }

    public enum RoutineType
    {
        UNKNOWN = 0,
        FUNCTION = 1,
        PROCEDURE = 2
    }
}
