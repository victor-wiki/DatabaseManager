using DatabaseInterpreter.Model;


namespace DatabaseManager.Core
{
    public class OracleDiagnosis : DbDiagnosis
    {
        public override DatabaseType DatabaseType => DatabaseType.Oracle;
        public OracleDiagnosis(ConnectionInfo connectionInfo) : base(connectionInfo) { }

        public override string GetStringLengthFunction()
        {
            return "LENGTH";
        }

        public override string GetStringNullFunction()
        {
            return "NVL";
        }
    }
}
