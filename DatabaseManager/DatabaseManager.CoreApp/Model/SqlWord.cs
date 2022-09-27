using System;

namespace DatabaseManager.Model
{
    public class SqlWord
    {
        public SqlWordTokenType Type { get; set; }
        public string Text { get; set; }
        public object Source { get; set; }
    }

    [Flags]
    public enum SqlWordTokenType : int
    {
        None = 0,
        Keyword = 2,
        BuiltinFunction = 4,
        Schema = 8,
        Function = 16,
        Table = 32,
        View = 64,
        TableColumn = 128,
        String = 256,
        Comment = 512,
        Number = 1024
    }

    public class SqlWordToken
    {
        public SqlWordTokenType Type { get; set; }
        public int StartIndex { get; set; }
        public int StopIndex { get; set; }
        public string Text { get; set; }
    }
}
