using System.Collections.Generic;

namespace DatabaseManager.FileUtility.Model
{
    public class DataReadResult
    {
        public string[] HeaderColumns { get; set; }

        public Dictionary<int, Dictionary<int, object>> Data { get;  set; }
    }
}
