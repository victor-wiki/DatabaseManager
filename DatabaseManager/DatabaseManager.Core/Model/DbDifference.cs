using System;
using System.Collections.Generic;
using DatabaseInterpreter.Model;


namespace DatabaseManager.Model
{
    public class DbDifference
    {               
        public string Type { get; set; }
        public DbDifference Parent { get; set; }
        public string ParentType { get; set; }
        public string ParentName { get; set; }
        public DatabaseObjectType DatabaseObjectType { get; set; }

        public DatabaseObject Source { get; set; }
        public DatabaseObject Target { get; set; }

        public string SourceName => this.Source?.Name;
        public string TargetName => this.Target?.Name;

        public DbDifferenceType DifferenceType { get; set; }          

        public List<DbDifference> SubDifferences { get; set; } = new List<DbDifference>();
    }

    public enum DbDifferenceType
    {
        None = 0,
        Added = 1,
        Modified = 2,
        Deleted = 4
    }
}
