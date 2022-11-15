using System.Collections.Generic;
using DatabaseInterpreter.Model;

namespace SqlAnalyser.Model
{
    public class CreateStatement : Statement
    {
        public virtual DatabaseObjectType ObjectType { get; set; }
        public NameToken ObjectName { get; set; }       
    }
}
