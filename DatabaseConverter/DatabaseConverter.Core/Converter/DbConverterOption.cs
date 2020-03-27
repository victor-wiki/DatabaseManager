using DatabaseInterpreter.Model;

namespace DatabaseConverter.Core
{
    public class DbConverterOption
    {       
        public bool PickupTable { get; set; }
        public bool EnsurePrimaryKeyNameUnique { get; set; } = true;
        public bool EnsureIndexNameUnique { get; set; } = true;
        public bool SplitScriptsToExecute { get; set; }
        public bool ExecuteScriptOnTargetServer { get; set; } = true;      
        public GenerateScriptMode GenerateScriptMode { get; set; } = GenerateScriptMode.Schema | GenerateScriptMode.Data;
        public bool BulkCopy { get; set; }
        public bool UseTransaction { get; set; }
        public bool SkipParseErrorForFunctionViewProcedure { get; set; }
    }
}
