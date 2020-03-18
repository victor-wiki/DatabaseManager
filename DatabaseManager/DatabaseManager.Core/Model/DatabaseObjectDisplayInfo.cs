using DatabaseInterpreter.Model;

namespace DatabaseManager.Model
{
    public class DatabaseObjectDisplayInfo
    {
        public string Name { get; set; }
        public DatabaseObjectDisplayType DisplayType { get; set; } = DatabaseObjectDisplayType.Scripts;
        public DatabaseType DatabaseType { get; set; }
        public string Content { get; set; }
        public ConnectionInfo ConnectionInfo { get; set; }
    }

    public enum DatabaseObjectDisplayType
    {
        Scripts = 0,
        Data = 1
    }
}
