using DatabaseInterpreter.Model;
using System;
using System.Diagnostics;
using Humanizer;

namespace DatabaseManager.Core
{
    public class ManagerUtil
    {
        public static DatabaseType GetDatabaseType(string dbType)
        {
            return (DatabaseType)Enum.Parse(typeof(DatabaseType), dbType);
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
    }
}
