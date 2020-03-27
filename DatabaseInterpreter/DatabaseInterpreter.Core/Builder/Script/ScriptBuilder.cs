using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DatabaseInterpreter.Core
{
    public class ScriptBuilder
    {
        private List<Script> scripts = new List<Script>();
        public List<Script> Scripts => this.scripts;

        public void Append(Script script)
        {
            this.scripts.Add(script);
        }

        public void AppendLine(Script script)
        {
            this.Append(script);
            this.AppendLine();
        }

        public void AppendLine()
        {
            this.Append(new NewLineSript());
        }

        public void AppendRange(IEnumerable<Script> scripts)
        {
            this.scripts.AddRange(scripts);
        }

        public override string ToString()
        {
            return string.Join("", this.scripts.Select(item => item.Content));
        }
    }
}
