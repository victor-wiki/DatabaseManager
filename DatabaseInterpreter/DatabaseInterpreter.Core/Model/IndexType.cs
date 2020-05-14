using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseInterpreter.Model
{
    [Flags]
    public enum IndexType : int
    {
        None = 0,
        Normal = 2,
        Primary = 4,
        Unique = 8,
        ColumnStore = 16,
        FullText = 32,
        Bitmap = 64,
        Reverse = 128
    }
}
