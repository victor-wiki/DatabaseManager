using DatabaseInterpreter.Model;

namespace DatabaseManager.Core
{
    public class SqlServerDiagnosis : DbDiagnosis
    {
        public override DatabaseType DatabaseType => DatabaseType.SqlServer;
        public SqlServerDiagnosis(ConnectionInfo connectionInfo) : base(connectionInfo) { }

        public override string GetStringLengthFunction()
        {
            return "LEN";
        }

        public override string GetStringNullFunction()
        {
            return "ISNULL";
        }
    }
}
