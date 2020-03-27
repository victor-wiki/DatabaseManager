using System;

namespace DatabaseInterpreter.Model
{
    public class Script
    {
        public string ObjectType { get; protected set; }
        public string Content { get; set; }

        public Script()
        {
        }

        public Script(string script)
        {
            this.Content = script;
        }
    }
}
