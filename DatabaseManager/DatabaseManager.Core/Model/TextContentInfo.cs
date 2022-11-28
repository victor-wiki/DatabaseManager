using System.Collections.Generic;

namespace DatabaseManager.Model
{
    public class ScriptContentInfo
    {
        public List<TextLineInfo> Lines = new List<TextLineInfo>();
    }

    public class TextLineInfo
    {
        public int Index { get; set; }
        public int FirstCharIndex { get; set; }
        public int Length { get; set; }

        public TextLineType Type { get; set; } = TextLineType.Text;
    }

    public enum TextLineType
    {
        Text,
        Comment
    }
}
