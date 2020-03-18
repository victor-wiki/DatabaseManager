using System.IO;

namespace DatabaseInterpreter.Profile
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
