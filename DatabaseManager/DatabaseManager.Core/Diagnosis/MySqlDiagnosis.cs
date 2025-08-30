using DatabaseInterpreter.Model;


namespace DatabaseManager.Core
{
    public class MySqlDiagnosis : DbDiagnosis
    {
        public override DatabaseType DatabaseType => DatabaseType.MySql;
        public MySqlDiagnosis(ConnectionInfo connectionInfo) : base(connectionInfo) { }        
    }
}
