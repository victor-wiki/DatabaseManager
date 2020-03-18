using System;

namespace DatabaseInterpreter.Model
{
    public class DbInterpreterOption
    {
        public bool SortTablesByKeyReference { get; set; } = true;
        public bool GenerateKey { get; set; } = true;
        public bool GenerateIndex { get; set; } = true;
        public bool GenerateDefaultValue { get; set; } = true;
        public bool GenerateComment { get; set; } = true;
        public bool GenerateIdentity { get; set; } = false;
        public bool InsertIdentityValue { get; set; } = true;
        public int? DataGenerateThreshold { get; set; } = 10000000;
        public int InQueryItemLimitCount { get; set; } = 2000;
        public GenerateScriptMode ScriptMode { get; set; }
        public GenerateScriptOutputMode ScriptOutputMode { get; set; }
        public string ScriptOutputFolder { get; set; } = "output";
        public int DataBatchSize { get; set; } = 500;
        public bool RemoveEmoji { get; set; }
        public bool TreatBytesAsNullForScript { get; set; }
        public bool TreatBytesAsNullForData { get; set; }
        public bool GetAllObjectsIfNotSpecified { get; set; }
        public DatabaseObjectFetchMode ObjectFetchMode = DatabaseObjectFetchMode.Details;
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
        UserDefinedTypes = 8,
        Function = 16,        
        Procedure = 32,
        Trigger = 64,
        TableColumn = 128,
        TablePrimaryKey = 256,
        TableForeignKey = 512,
        TableIndex = 1024
    }
}
