using System.Collections.Generic;
using System.Data;

namespace DatabaseManager.Core.Model
{
    public class DataCompareDifferentRow
    {
        public int RowIndex { get; set; }
        public DataRow KeyRow { get; set; }
        public Dictionary<string, (object, object)> Details = new Dictionary<string, (object, object)>();
    }   
}
