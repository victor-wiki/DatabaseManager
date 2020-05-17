namespace DatabaseInterpreter.Model
{
    public class DatabaseAccountInfo : DatabaseServerInfo
    {
        public bool IntegratedSecurity { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public bool IsDba { get; set; }
        public bool UseSsl { get; set; }
    }
}
