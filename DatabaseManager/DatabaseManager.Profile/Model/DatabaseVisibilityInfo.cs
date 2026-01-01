using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseManager.Profile.Model
{
    public class DatabaseVisibilityInfo
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Database { get; set; }
        public bool Hidden { get; set; }
    }
}
