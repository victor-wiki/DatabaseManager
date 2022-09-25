using System;

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
        Reverse = 128,
        BTree = 256,
        Brin = 512,
        Hash = 1024,
        Gin = 2048,
        GiST = 4096,
        SP_GiST = 8192
    }
}
