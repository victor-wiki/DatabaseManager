using System;

namespace DatabaseInterpreter.Model
{
    public class DbInterpreterOption
    {
        public bool SortObjectsByReference { get; set; } = false;
        public bool InsertIdentityValue { get; set; } = true;
        public int? DataGenerateThreshold { get; set; } = 10000000;
        public int InQueryItemLimitCount { get; set; } = 2000;
        public bool RemoveEmoji { get; set; }
        public bool TreatBytesAsNullForReading { get; set; }
        public bool TreatBytesAsNullForExecuting { get; set; }
        public bool TreatBytesAsHexStringForFile { get; set; }      
        public DatabaseObjectFetchMode ObjectFetchMode = DatabaseObjectFetchMode.Details;
        public GenerateScriptMode ScriptMode { get; set; }
        public GenerateScriptOutputMode ScriptOutputMode { get; set; }
        public string ScriptOutputFolder { get; set; } = "output";
        public bool GetTableAllObjects { get; set; }
        public TableScriptsGenerateOption TableScriptsGenerateOption = new TableScriptsGenerateOption();
        public bool ThrowExceptionWhenErrorOccurs { get; set; } = true;
        public bool RequireInfoMessage { get; set; }
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

    [Flags]
    public enum DatabaseObjectType : int
    {
        None = 0,
        Table = 2,
        View = 4,
        UserDefinedType = 8,
        Function = 16,
        Procedure = 32,
        TableColumn = 64,
        TableTrigger = 128,
        TablePrimaryKey = 256,
        TableForeignKey = 512,
        TableIndex = 1024,
        TableConstraint = 2048
    }
}
