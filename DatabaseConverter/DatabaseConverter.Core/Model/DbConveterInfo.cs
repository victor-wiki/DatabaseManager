using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System.Collections.Generic;

namespace DatabaseConverter.Model
{
    public class DbConveterInfo
    {
        public DbInterpreter DbInterpreter { get; set; }      

        public Dictionary<string, string> TableNameMappings = new Dictionary<string, string>();

        public DatabaseObjectType DatabaseObjectType = DatabaseObjectType.None;
    }
}
