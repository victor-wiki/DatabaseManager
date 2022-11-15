using DatabaseInterpreter.Model;
using System.Text;

namespace DatabaseInterpreter.Core
{
    public class MySqlConnectionBuilder : IConnectionBuilder
    {
        public string BuildConntionString(ConnectionInfo connectionInfo)
        {
            StringBuilder sb = new StringBuilder($"server={connectionInfo.Server};port={connectionInfo.Port};database={connectionInfo.Database};Charset=utf8;AllowLoadLocalInfile=True;AllowZeroDateTime=True;AllowPublicKeyRetrieval=True;Allow User Variables=true;");

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
