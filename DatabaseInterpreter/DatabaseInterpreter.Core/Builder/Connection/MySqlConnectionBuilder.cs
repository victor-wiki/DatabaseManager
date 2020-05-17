using DatabaseInterpreter.Model;
using System.Text;

namespace DatabaseInterpreter.Core
{
    public class MySqlConnectionBuilder : IConnectionBuilder
    {
        public string BuildConntionString(ConnectionInfo connectionInfo)
        {
            StringBuilder sb = new StringBuilder($"server={connectionInfo.Server};database={connectionInfo.Database};Charset=utf8;AllowLoadLocalInfile=True;AllowZeroDateTime=True;");

            if(connectionInfo.IntegratedSecurity)
            {
                sb.Append($"IntegratedSecurity=yes;Uid=auth_windows;");
            }
            else
            {
                sb.Append($"user id={connectionInfo.UserId};password={connectionInfo.Password};SslMode={(connectionInfo.UseSsl? "Preferred" : "none")};");
            }

            return sb.ToString();
        }
    }
}
