using System.Data.Common;
using System.Threading;
using System.Collections.Generic;

namespace DatabaseInterpreter.Model
{
    public class BulkCopyInfo
    {
        public string DestinationTableOwner { get; set; }
        public string DestinationTableName { get; set; }
        public int? Timeout { get; set; }
        public int? BatchSize { get; set; }
        public DbTransaction Transaction { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public bool DetectDateTimeTypeByValues { get; set; }
        public IEnumerable<TableColumn> Columns { get; set; }
        public bool KeepIdentity { get; set; }
    }
}
