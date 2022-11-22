using System.Collections.Generic;

namespace DatabaseConverter.Model
{
    public class TokenSymbolItemInfo
    {
        public int Index { get; set; }
        public string Content { get; set; }        
        public TokenSymbolItemType Type { get; set; } = TokenSymbolItemType.Content;
        public List<TokenSymbolItemInfo> Children = new List<TokenSymbolItemInfo>();
    }

    public enum TokenSymbolItemType
    {
        Content,
        Keyword
    }
}
