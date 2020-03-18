namespace  DatabaseInterpreter.Core
{
    public class MySqlProvider : IDbProvider
    {
        public string ProviderName => "MySql.Data.MySqlClient";
    }
}
