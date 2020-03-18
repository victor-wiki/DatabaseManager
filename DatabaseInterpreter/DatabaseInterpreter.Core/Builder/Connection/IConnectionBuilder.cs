using DatabaseInterpreter.Model;

namespace DatabaseInterpreter.Core
{
    public interface IConnectionBuilder
    {
        string BuildConntionString(ConnectionInfo connectionInfo);
    }
}
