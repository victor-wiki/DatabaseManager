using System.Collections.Generic;

namespace DatabaseManager.Core.Model
{
    public class GenerateColumnDocumentationOption
    {
        public string FilePath { get; set; }
        public bool ShowTableComment  { get; set; }
        public string GridColumnHeaderBackgroundColor { get; set; } = "#8D78D8";
        public string GridColumnHeaderForegroundColor { get; set; } = "#FFFFFF";
        public bool GridColumnHeaderFontIsBold { get; set; } = true;

        public List<CustomProperty> Properties { get; set; } = new List<CustomProperty>();
    } 
}
