using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;

namespace DatabaseInterpreter.Model
{
    public class CommandInfo
    {
        public CommandType CommandType { get; set; } = CommandType.Text;
        public string CommandText { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public DbTransaction Transaction { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public bool ContinueWhenErrorOccurs { get; set; }
        public bool HasError { get; set; }
        public bool TransactionRollbacked { get; set; }
    }
}
