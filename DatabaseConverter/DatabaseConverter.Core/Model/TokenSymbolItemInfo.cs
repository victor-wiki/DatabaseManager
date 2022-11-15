using System.Collections.Generic;

namespace DatabaseConverter.Model
{
    public class TokenSymbolItemInfo
    {
        public int Index { get; set; }
        public string Content { get; set; }
        public TokenSymbolItemType Type { get; set; } = TokenSymbolItemType.Single;
        public List<TokenSymbolItemInfo> Children = new List<TokenSymbolItemInfo>();
    }

    public class TokenSymbolItemGroupInfo
    {
        public int GroupId { get; set; }
        public List<TokenSymbolItemInfo> LeftSideItems = new List<TokenSymbolItemInfo>();
        public List<TokenSymbolItemInfo> RightSideItems = new List<TokenSymbolItemInfo>();
    }

    public enum TokenSymbolItemType
    {
        Single,
        Combination
    }
}
