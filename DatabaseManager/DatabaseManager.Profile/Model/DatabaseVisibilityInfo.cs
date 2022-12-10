using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseManager.Model
{
    public class DatabaseVisibilityInfo
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Database { get; set; }
        public bool Visible { get; set; }
    }
}
