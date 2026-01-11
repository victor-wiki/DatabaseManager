namespace DatabaseManager.Core.Model
{
    public enum ScriptAction
    {
        NONE = 0,
        CREATE = 1,
        ALTER = 2,
        SELECT = 3,
        INSERT = 4,
        UPDATE = 5,
        DELETE = 6,
        EXECUTE = 7,
        CREATE_PROCEDURE_INSERT = 8,
        CREATE_PROCEDURE_UPDATE = 9,
        CREATE_PROCEDURE_DELETE = 10
    }
}
