using System;
using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class StatementScriptBuilderOption
    {
        internal bool NotBuildDeclareStatement { get; set; }
        internal bool CollectDeclareStatement { get; set;}
        public bool OutputRemindInformation { get; set; }
        internal List<Type> CollectSpecialStatementTypes { get; set; } = new List<Type>();
    }
}
