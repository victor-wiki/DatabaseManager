using System;
using System.Collections;
using System.IO;

namespace DatabaseManager.Helper
{
    public class ProfileFileHelper
    {
        public static readonly string ProfilesFolder = "Profiles";
        public static readonly string LayoutName = "Layout.xml";
        public static readonly string RecentFileName = "RecentFile.txt";

        public static string LayoutFilePath => Path.Combine(ProfilesFolder, LayoutName);
        public static string RecentFilePath => Path.Combine(ProfilesFolder, RecentFileName);

        static ProfileFileHelper()
        {
            if (!Directory.Exists(ProfilesFolder))
            {
                Directory.CreateDirectory(ProfilesFolder);
            }
        }

        public static void ResetRecentFile()
        {
            if(File.Exists(RecentFilePath))
            {
                File.WriteAllText(RecentFilePath, string.Empty);
            }
        }

        public static string[] GetRecentFiles()
        {
            if (File.Exists(RecentFilePath))
            {
                return File.ReadAllLines(RecentFilePath);
            }

            return Array.Empty<string>();
        }
    }
}
