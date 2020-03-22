using DatabaseInterpreter.Model;

namespace DatabaseManager.Model
{
    public class DatabaseObjectDisplayInfo
    {
        public string Name { get; set; }
        public DatabaseObject DatabaseObject { get; set; }
        public DatabaseObjectDisplayType DisplayType { get; set; } = DatabaseObjectDisplayType.Script;
        public DatabaseType DatabaseType { get; set; }
        public string Content { get; set; }
        public ConnectionInfo ConnectionInfo { get; set; }
    }

    public enum DatabaseObjectDisplayType
    {
        Script = 0,
        Data = 1,
        Schema =2
    }
}
