using DatabaseInterpreter.Model;
using System.Text;

namespace DatabaseInterpreter.Core
{
    public class SqlServerConnectionBuilder : IConnectionBuilder
    {
        public string BuildConntionString(ConnectionInfo connectionInfo)
        {
            StringBuilder sb = new StringBuilder($"Data Source={connectionInfo.Server};Initial Catalog={connectionInfo.Database};TrustServerCertificate=true;");

            if(connectionInfo.IntegratedSecurity)
            {
                sb.Append("Integrated Security=true;");
            }
            else
            {
                sb.Append($"User Id={connectionInfo.UserId};Password={connectionInfo.Password};");
            }

            return sb.ToString();
        }
    }
}
