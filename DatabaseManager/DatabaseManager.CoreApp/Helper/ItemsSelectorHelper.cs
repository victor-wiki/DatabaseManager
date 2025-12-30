using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;

namespace DatabaseManager.Helper
{
    public class CheckItemInfo
    {
        public string Name { get; set; }
        public bool Checked { get; set; }
    }

    public class ItemsSelectorHelper
    {
        public static List<CheckItemInfo> GetDatabaseObjectTypeItems(DatabaseType databaseType, DatabaseObjectType supportDatabaseObjectType = DatabaseObjectType.None)
        {
            List<DatabaseObjectType> dbObjTypes = new List<DatabaseObjectType>()
            {
                DatabaseObjectType.Trigger,
                DatabaseObjectType.Table,
                DatabaseObjectType.View,
                DatabaseObjectType.Function,
                DatabaseObjectType.Procedure,
                DatabaseObjectType.Type,
                DatabaseObjectType.Sequence
            };

            List<CheckItemInfo> checkItems = new List<CheckItemInfo>();

            if (supportDatabaseObjectType != DatabaseObjectType.None)
            {
                foreach (var dbObjType in dbObjTypes)
                {
                    if (dbObjType == DatabaseObjectType.Trigger || supportDatabaseObjectType.HasFlag(dbObjType))
                    {
                        checkItems.Add(new CheckItemInfo() { Name = ManagerUtil.GetPluralString(dbObjType.ToString()), Checked = true });
                    }
                }
            }

            return checkItems;
        }

        public static DatabaseObjectType GetDatabaseObjectTypeByCheckItems(List<CheckItemInfo> items)
        {
            DatabaseObjectType databaseObjectType = DatabaseObjectType.None;

            foreach (var item in items)
            {
                DatabaseObjectType type = (DatabaseObjectType)Enum.Parse(typeof(DatabaseObjectType), ManagerUtil.GetSingularString(item.Name));

                databaseObjectType = databaseObjectType | type;
            }

            return databaseObjectType;
        }

        public static List<CheckItemInfo> GetDatabaseTypeItems(List<string> databaseTypes, bool checkedIfNotConfig = true)
        {
            List<CheckItemInfo> items = new List<CheckItemInfo>();

            var dbTypes = Enum.GetNames(typeof(DatabaseType));

            foreach (string dbType in dbTypes)
            {
                if (dbType != nameof(DatabaseType.Unknown))
                {
                    bool @checked = (checkedIfNotConfig && databaseTypes.Count == 0) || databaseTypes.Contains(dbType);

                    items.Add(new CheckItemInfo() { Name = dbType, Checked = @checked });
                }
            }

            return items;
        }
    }
}
