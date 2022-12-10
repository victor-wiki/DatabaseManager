using DatabaseInterpreter.Model;

namespace DatabaseInterpreter.Core
{
    public class DbScriptGeneratorHelper
    {
        public static DbScriptGenerator GetDbScriptGenerator(DbInterpreter dbInterpreter)
        {
            DbScriptGenerator dbScriptGenerator = null;

            switch(dbInterpreter.DatabaseType)
            {
                case DatabaseType.SqlServer:
                    dbScriptGenerator = new SqlServerScriptGenerator(dbInterpreter);
                    break;
                case DatabaseType.MySql:
                    dbScriptGenerator = new MySqlScriptGenerator(dbInterpreter);
                    break;
                case DatabaseType.Oracle:
                    dbScriptGenerator = new OracleScriptGenerator(dbInterpreter);
                    break;
                case DatabaseType.Postgres:
                    dbScriptGenerator = new PostgresScriptGenerator(dbInterpreter);
                    break;
                case DatabaseType.Sqlite:
                    dbScriptGenerator = new SqliteScriptGenerator(dbInterpreter);
                    break;

            }

            return dbScriptGenerator;
        }
    }
}
