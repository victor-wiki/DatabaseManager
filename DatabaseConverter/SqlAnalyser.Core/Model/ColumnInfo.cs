using System;
using System.Collections.Generic;
using System.Text;

namespace SqlAnalyser.Model
{
    public class ColumnInfo
    {
        public TokenInfo Expression { get; set; }
        public TokenInfo Alias { get; set; }
        public TokenInfo Name { get; set; }
        public TokenInfo DataType { get; set; }
    }
}
