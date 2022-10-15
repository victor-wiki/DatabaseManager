using DatabaseInterpreter.Model;
using DatabaseManager.Profile;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseManager.Data
{
    public class DataStore
    {
        public static String LockPassword;
        private static Dictionary<DatabaseType, SchemaInfo> _dictSchemaInfo;
        private static List<AccountProfileInfo> _accountProfileInfos;

        #region SchemaInfo
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
        #endregion

        #region AccountProfileInfo
        public static AccountProfileInfo GetAccountProfileInfo(Guid id)
        {
            if (_accountProfileInfos != null)
            {
                return _accountProfileInfos.FirstOrDefault(item => item.Id == id);
            }

            return null;
        }

        public static void SetAccountProfileInfo(AccountProfileInfo accountProfileInfo)
        {
            if (_accountProfileInfos == null)
            {
                _accountProfileInfos = new List<AccountProfileInfo>();
            }

            AccountProfileInfo oldInfo = _accountProfileInfos.FirstOrDefault(item => item.Id == accountProfileInfo.Id);

            if (oldInfo != null)
            {
                oldInfo = accountProfileInfo;
            }
            else
            {
                _accountProfileInfos.Add(accountProfileInfo);
            }
        }
        #endregion
    }
}
