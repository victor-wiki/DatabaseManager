using DatabaseInterpreter.Model;

namespace DatabaseManager.Profile
{
    public class ConnectionProfileInfo : ConnectionInfo
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Name { get; set; }
        public string DatabaseType { get; set; }
        public bool Visible { get; set; } = true;

        public string ConnectionDescription => $"server={this.Server}{(string.IsNullOrEmpty(this.Port) ? "" : (":" + this.Port))};database={this.Database}";

        public string Description
        {
            get
            {
                string connectionDescription = this.ConnectionDescription;

                if (this.Name == connectionDescription)
                {
                    return this.Name;
                }
                else
                {
                    return $"{this.Name}({connectionDescription})";
                }
            }
        }
    }
}
