using DatabaseInterpreter.Model;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;

namespace DatabaseInterpreter.Core
{
    public class DbConnector
    {
        private readonly IDbProvider _dbProvider;
        private readonly string _connectionString;

        public DbConnector(IDbProvider dbProvider, string connectionString)
        {
            this._dbProvider = dbProvider;
            this._connectionString = connectionString;
        }

        public DbConnector(IDbProvider dbProvider, IConnectionBuilder connectionBuilder, ConnectionInfo connectionInfo)
        {
            this._dbProvider = dbProvider;
            this._connectionString = connectionBuilder.BuildConntionString(connectionInfo);
        }

        public DbConnection CreateConnection()
        {
            DbProviderFactory factory = null;

            string lowerProviderName = this._dbProvider.ProviderName.ToLower();
            if (lowerProviderName.Contains("oracle"))
            {
                factory = new OracleClientFactory();
            }
            else if (lowerProviderName.Contains("mysql"))
            {
                factory = MySqlConnector.MySqlConnectorFactory.Instance;
            }
            else if (lowerProviderName.Contains("sqlclient"))
            {
                factory = SqlClientFactory.Instance;
            }
            else if (lowerProviderName.Contains("npgsql"))
            {
                factory = NpgsqlFactory.Instance;
            }
            else if(lowerProviderName.Contains("sqlite"))
            {
                factory = SqliteFactory.Instance;
            }
           
            DbConnection connection = factory.CreateConnection();

            if (connection != null)
            {
                connection.ConnectionString = this._connectionString;
                return connection;
            }
            else
            {
                return null;
            }
        }
    }
}
