namespace DatabaseManager.Profile
{
    public class FileConnectionProfileInfo
    {
        public string Id { get; set; }

        public string DatabaseType { get; set; }

        public string SubType { get; set; }

        public string Database { get; set; }
        public string DatabaseVersion { get; set; }
        public string EncryptionType { get; set; }
        public bool HasPassword { get; set; }
        public string Password { get; set; }

        public string Name { get; set; }

        public string Description
        {
            get
            {
                return $"{this.Name}({this.Database})";
            }
        }
    }
}
