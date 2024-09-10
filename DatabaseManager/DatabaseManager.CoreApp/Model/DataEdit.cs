using System.Collections.Generic;

namespace DatabaseManager.Model
{
    internal class UpdateDataItemInfo
    {
        internal string ColumnName { get; set; }
        internal object OldValue { get; set; }
        internal object NewValue { get; set; }
    }

    internal class ExecuteScriptInfo
    {
        internal string Script { get; set; }
        internal Dictionary<string, object> Parameters { get; set; } 
    }
}
