using System;
using System.Collections.Generic;
using System.Text;

namespace SqlAnalyser.Model
{
    public class UnionStatement: Statement
    {
        public UnionType Type { get; set; }
        public SelectStatement SelectStatement { get; set; }
    }

    public enum UnionType
    {
        UNION = 0,
        UNION_ALL = 1,
        INTERSECT = 2 ,       
        EXCEPT = 3,
        MINUS = 4
    }
}
