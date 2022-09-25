namespace  DatabaseInterpreter.Core
{
    public class PostgresProvider : IDbProvider
    {
        public string ProviderName => "Npgsql";           
    }
}
