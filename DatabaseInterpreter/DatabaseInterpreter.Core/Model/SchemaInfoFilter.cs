using System;

namespace DatabaseInterpreter.Model
{
    public class SchemaInfoFilter
    {
        public bool Strict { get; set; }
        public DatabaseObjectType DatabaseObjectType = DatabaseObjectType.None;
        public string[] UserDefinedTypeNames { get; set; }
        public string[] FunctionNames { get; set; }
        public string[] TableNames { get; set; }       
        public string[] ViewNames { get; set; }       
        public string[] ProcedureNames { get; set; }
        public string[] TableTriggerNames { get; set; }        
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
