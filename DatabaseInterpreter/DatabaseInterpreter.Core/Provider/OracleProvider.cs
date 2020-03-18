namespace  DatabaseInterpreter.Core
{
    public class OracleProvider:IDbProvider
    {
        public string ProviderName => "Oracle.ManagedDataAccess.Client";      
    }
}
