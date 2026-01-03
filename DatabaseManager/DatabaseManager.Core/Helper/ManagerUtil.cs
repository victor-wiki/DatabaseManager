using System;
using System.Diagnostics;
using Humanizer;
using DatabaseInterpreter.Model;

namespace DatabaseManager.Core
{
    public class ManagerUtil
    {
        public static DatabaseType GetDatabaseType(string dbType)
        {
            if(!string.IsNullOrEmpty(dbType))
            {
                return (DatabaseType)Enum.Parse(typeof(DatabaseType), dbType);
            }
            else
            {
                return DatabaseType.Unknown;
            }
        }

        public static bool IsFileConnection(DatabaseType databaseType)
        {
            if(databaseType == DatabaseType.Sqlite)
            {
                return true;
            }

            return false;
        }

        public static void OpenInExplorer(string filePath)
        {
            string cmd = "explorer.exe";
            string arg = "/select," + filePath;
            Process.Start(cmd, arg);
        }        

        public static string GetSingularString(string value)
        {
            return value.Singularize();
        }

        public static string GetPluralString(string value)
        {
            return value.Pluralize();
        } 

        public static string GetDisplayTitle(string value)
        {
            return value.Humanize(LetterCasing.Title);
        }
        
        public static bool SupportComment(DatabaseType databaseType)
        {
            if(databaseType == DatabaseType.Sqlite)
            {
                return false;
            }

            return true;
        }
    }
}
