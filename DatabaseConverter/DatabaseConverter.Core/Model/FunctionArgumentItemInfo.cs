using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseConverter.Model
{
    public class FunctionArgumentItemInfo
    {
        public int Index { get; set; }
        public string Content { get; set; }

        public List<FunctionArgumentItemDetailInfo> Details = new List<FunctionArgumentItemDetailInfo>();
    }

    public class FunctionArgumentItemDetailInfo
    {
        public FunctionArgumentItemDetailType Type { get; set; }
        public string Content { get; set; }
    }

    public enum FunctionArgumentItemDetailType
    { 
        Text,      
        Whitespace
    }
}
