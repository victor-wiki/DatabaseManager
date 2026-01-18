using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class PartitionScheme
    {
        public string SchemeName { get; set; }
        public string ColumnName { get; set; }
        public string FunctionName { get; set; }
        public List<string> Filegroups { get; set; }
    }
}
