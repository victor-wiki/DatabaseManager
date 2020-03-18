using DatabaseInterpreter.Model;
using System;
using System.Linq;
using System.Reflection;

namespace  DatabaseInterpreter.Core
{
    public class DbInterpreterHelper
    {
        public static DbInterpreter GetDbInterpreter(DatabaseType dbType, ConnectionInfo connectionInfo, DbInterpreterOption option)
        {
            DbInterpreter dbInterpreter = null;

            var assembly = Assembly.GetExecutingAssembly();          
            var typeArray = assembly.ExportedTypes;

            var types = (from type in typeArray
                         where type.IsSubclassOf(typeof(DbInterpreter))
                         select type).ToList();

            foreach (var type in types)
            {
                dbInterpreter = (DbInterpreter)Activator.CreateInstance(type, connectionInfo, option);

                if (dbInterpreter.DatabaseType == dbType)
                {
                    return dbInterpreter;
                }
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
    }
}
