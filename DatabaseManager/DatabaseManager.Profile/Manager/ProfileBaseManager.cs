using System.IO;

namespace DatabaseManager.Profile
{
    public class ProfileBaseManager
    {
        public static string ProfileFolder => "Profiles";

        static ProfileBaseManager()
        {
            if (!Directory.Exists(ProfileFolder))
            {
                Directory.CreateDirectory(ProfileFolder);
            }
        }
    }
}
