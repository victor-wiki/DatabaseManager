using DatabaseInterpreter.Core;
using System.Collections.Generic;

namespace DatabaseConverter.Core
{
    public class DbConveterInfo
    {
        public DbInterpreter DbInterpreter { get; set; }       
        public string DbOwner { get; set; }

        public Dictionary<string, string> TableNameMappings = new Dictionary<string, string>();
    }
}
