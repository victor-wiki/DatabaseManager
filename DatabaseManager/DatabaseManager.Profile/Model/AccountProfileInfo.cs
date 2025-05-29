using DatabaseInterpreter.Model;
using System;

namespace DatabaseManager.Profile.Model
{
    public class AccountProfileInfo : DatabaseAccountInfo
    {
        public string Id { get; set; }

        public string DatabaseType { get; set; }
        public int Priority { get; set; }
      
        public string Description
        {
            get
            {
                return $"{((!string.IsNullOrEmpty(this.UserId) ? this.UserId : "Integrated Security"))}({this.Server}{(string.IsNullOrEmpty(this.Port) ? "" : (":" + this.Port))})";
            }
        }
    }
}
