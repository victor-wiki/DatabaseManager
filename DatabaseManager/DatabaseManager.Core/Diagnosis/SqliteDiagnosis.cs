using DatabaseInterpreter.Model;


namespace DatabaseManager.Core
{
    public class SqliteDiagnosis : DbDiagnosis
    {
        public override DatabaseType DatabaseType => DatabaseType.Sqlite;
        public SqliteDiagnosis(ConnectionInfo connectionInfo) : base(connectionInfo) { }

        public override string GetStringLengthFunction()
        {
            return "LENGTH";
        }

        public override string GetStringNullFunction()
        {
            return "ISNULL";
        }
    }
}
