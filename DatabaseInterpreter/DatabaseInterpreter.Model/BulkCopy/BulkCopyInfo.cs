using System.Collections.Generic;
using System.Data.Common;
using System.Threading;

namespace DatabaseInterpreter.Model
{
    public class BulkCopyInfo
    {
        public DatabaseType SourceDatabaseType { get; set; } = DatabaseType.Unknown;
        public string DestinationTableSchema { get; set; }
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
