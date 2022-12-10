namespace  DatabaseInterpreter.Core
{
    public class SqlServerProvider:IDbProvider
    {
        public string ProviderName => "Microsoft.Data.SqlClient";           
    }
}
