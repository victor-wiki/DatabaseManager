using System.Collections.Generic;

namespace DatabaseConverter.Core.Model
{
    public class DateUnitMapping
    {
        public string Name { get; set; }
        public List<DateUnitMappingItem> Items { get; set; } = new List<DateUnitMappingItem>();
    }

    public class DateUnitMappingItem
    {
        public string DbType { get; set; }
        public string Unit { get; set; }
        public bool CaseSensitive { get; set; }
        public bool Formal { get; set; }
    }
}
