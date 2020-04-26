using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace  DatabaseInterpreter.Core
{
    public class DbInterpreterHelper
    {
        public static DbInterpreter GetDbInterpreter(DatabaseType dbType, ConnectionInfo connectionInfo, DbInterpreterOption option)
        {
            DbInterpreter dbInterpreter = null;

            if(dbType == DatabaseType.SqlServer)
            {
                dbInterpreter = new SqlServerInterpreter(connectionInfo, option);
            }
            else if(dbType == DatabaseType.MySql)
            {
                dbInterpreter = new MySqlInterpreter(connectionInfo, option);
            }
            else if(dbType == DatabaseType.Oracle)
            {
                dbInterpreter = new OracleInterpreter(connectionInfo, option);
            }           

            return dbInterpreter;
        }

        public static string GetOwnerName(DbInterpreter dbInterpreter)
        {
            if (dbInterpreter.DatabaseType == DatabaseType.Oracle)
            {
                return dbInterpreter.ConnectionInfo.UserId;
            }
            else
            {
                if(dbInterpreter.DatabaseType==DatabaseType.SqlServer)
                {
                    return "dbo";
                }

                return dbInterpreter.ConnectionInfo.Database;
            }
        }

        public static IEnumerable<DatabaseType> GetDisplayDatabaseTypes()
        {
            return Enum.GetValues(typeof(DatabaseType)).Cast<DatabaseType>().Where(item=>item != DatabaseType.Unknown );
        }
    }
}
