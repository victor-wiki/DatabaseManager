using DatabaseInterpreter.Model;
using System.Text;

namespace DatabaseInterpreter.Core
{
    public class OracleConnectionBuilder : IConnectionBuilder
    {
        public string BuildConntionString(ConnectionInfo connectionInfo)
        {
            string server = connectionInfo.Server;
            string serviceName = OracleInterpreter.DEFAULT_SERVICE_NAME;
            string port = connectionInfo.Port;

            if (string.IsNullOrEmpty(port))
            {
                port = OracleInterpreter.DEFAULT_PORT.ToString();
            }

            if (server != null && server.Contains("/"))
            {
                string[] serverService = connectionInfo.Server.Split('/');
                server = serverService[0];
                serviceName = serverService[1];
            }

            StringBuilder sb = new StringBuilder($"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={server})(PORT={port})))(CONNECT_DATA=(SERVICE_NAME={serviceName})));");

            if (connectionInfo.IntegratedSecurity)
            {
                sb.Append($"User Id=/;");
            }
            else
            {
                sb.Append($"User Id={connectionInfo.UserId};Password={connectionInfo.Password};");
            }

            if (connectionInfo.IsDba)
            {
                sb.Append("DBA PRIVILEGE=SYSDBA;");
            }

            return sb.ToString();
        }
    }
}
