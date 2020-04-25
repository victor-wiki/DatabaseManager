using DatabaseInterpreter.Model;
using System.Collections.Generic;

namespace DatabaseManager.Data
{
    public class DataStore
    {
        private static Dictionary<DatabaseType, SchemaInfo> _dictSchemaInfo;

        public static SchemaInfo GetSchemaInfo(DatabaseType databaseType)
        {
            if (_dictSchemaInfo != null && _dictSchemaInfo.ContainsKey(databaseType))
            {
                return _dictSchemaInfo[databaseType];
            }

            return null;
        }

        public static void SetSchemaInfo(DatabaseType databaseType, SchemaInfo schemaInfo)
        {
            if (_dictSchemaInfo == null)
            {
                _dictSchemaInfo = new Dictionary<DatabaseType, SchemaInfo>();
            }

            if (!_dictSchemaInfo.ContainsKey(databaseType))
            {
                _dictSchemaInfo.Add(databaseType, schemaInfo);
            }
            else
            {
                _dictSchemaInfo[databaseType] = schemaInfo;
            }
        }       
    }
}
