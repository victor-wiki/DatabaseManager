using DatabaseInterpreter.Model;
using System.Text;

namespace DatabaseInterpreter.Core
{
    public class PostgresConnectionBuilder : IConnectionBuilder
    {
        public string BuildConntionString(ConnectionInfo connectionInfo)
        {
            string server = connectionInfo.Server;          
            string port = connectionInfo.Port;
            int timeout = DbInterpreter.Setting.CommandTimeout;

            if (string.IsNullOrEmpty(port))
            {
                port = PostgresInterpreter.DEFAULT_PORT.ToString();
            }          

            StringBuilder sb = new StringBuilder($"Host={server};Port={port};Database={connectionInfo.Database};CommandTimeout={timeout};");

            if (connectionInfo.IntegratedSecurity)
            {
                sb.Append($"Integrated Security=True;Username={connectionInfo.UserId};");
            }
            else
            {
                sb.Append($"Username={connectionInfo.UserId};Password={connectionInfo.Password};");
            }

            return sb.ToString();
        }
    }
}
