using System.Collections.Generic;
using DatabaseInterpreter.Model;

namespace SqlAnalyser.Model
{
    public class DropStatement : Statement
    {
        public DatabaseObjectType ObjectType { get; set; }
        public NameToken ObjectName { get; set; }
        public bool IsTemporaryTable { get; set; }
    }
}
