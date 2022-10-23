using System;

namespace DatabaseInterpreter.Model
{
    public class DbInterpreterOption
    {
        public bool SortObjectsByReference { get; set; } = false;       
        public int? DataGenerateThreshold { get; set; } = 10000000;
        public int InQueryItemLimitCount { get; set; } = 2000;
        public bool RemoveEmoji { get; set; }
        public bool TreatBytesAsNullForReading { get; set; }
        public bool TreatBytesAsNullForExecuting { get; set; }
        public bool TreatBytesAsHexStringForFile { get; set; }     
        public bool ExcludeGeometryForData { get; set; }
        public bool ExcludeIdentityForData { get; set; }
        public bool ShowTextForGeometry { get; set; }
        public DatabaseObjectFetchMode ObjectFetchMode = DatabaseObjectFetchMode.Details;
        public GenerateScriptMode ScriptMode { get; set; }
        public GenerateScriptOutputMode ScriptOutputMode { get; set; }
        public string ScriptOutputFolder { get; set; } = "output";
        public bool GetTableAllObjects { get; set; }

        public TableScriptsGenerateOption TableScriptsGenerateOption = new TableScriptsGenerateOption();
        public bool ThrowExceptionWhenErrorOccurs { get; set; } = true;
        public bool RequireInfoMessage { get; set; }
        public bool BulkCopy { get; set; }
        public bool DetectDateTimeTypeByValues { get; set; }
        public bool IncludePrimaryKeyWhenGetTableIndex { get; set; }
    }

    [Flags]
    public enum GenerateScriptMode : int
    {
        None = 0,
        Schema = 2,
        Data = 4
    }

    [Flags]
    public enum GenerateScriptOutputMode : int
    {
        None = 0,
        WriteToString = 2,
        WriteToFile = 4
    }

    public enum DatabaseObjectFetchMode
    {
        Details = 0,
        Simple = 1
    }   
}
