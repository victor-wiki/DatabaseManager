using SqlCodeEditor.Models;
using System;

namespace DatabaseManager.Model
{
    public class SqlWord
    {
        public SqlWordTokenType Type { get; set; }
        public string Text { get; set; }
        public object Source { get; set; }
    }
}
