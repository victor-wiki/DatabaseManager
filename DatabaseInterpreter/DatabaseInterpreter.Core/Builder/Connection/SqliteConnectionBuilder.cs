using DatabaseInterpreter.Model;
using System.Text;

namespace DatabaseInterpreter.Core
{
    public class SqliteConnectionStringBuilder : IConnectionBuilder
    {
        public string BuildConntionString(ConnectionInfo connectionInfo)
        {
            StringBuilder sb = new StringBuilder($"Data Source={connectionInfo.Database};Password={connectionInfo.Password};Mode=ReadWriteCreate;");
            
            return sb.ToString();
        }
    }
}
