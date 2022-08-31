using DatabaseInterpreter.Model;
using Newtonsoft.Json;
using System;

namespace DatabaseInterpreter.Profile
{
    public class ConnectionProfileInfo
    {       
        public Guid AccountProfileId { get; set; }      
        public string Name { get; set; }
        public string DatabaseType { get; set; }
        public string Database { get; set; }
        
        [JsonIgnore]
        public ConnectionInfo ConnectionInfo { get; set; }

        [JsonIgnore]
        public string ConnectionDescription => $"server={this.ConnectionInfo?.Server}{(string.IsNullOrEmpty(this.ConnectionInfo?.Port)?"":(":"+ this.ConnectionInfo?.Port))};database={this.ConnectionInfo?.Database}";

        [JsonIgnore]
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
