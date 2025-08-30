using DatabaseInterpreter.Model;


namespace DatabaseManager.Core
{
    public class PostgresDiagnosis : DbDiagnosis
    {
        public override DatabaseType DatabaseType => DatabaseType.Postgres;
        public PostgresDiagnosis(ConnectionInfo connectionInfo) : base(connectionInfo) { }        
    }
}
