using DatabaseInterpreter.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseManager.Core
{
    public class ScriptParser
    {
        private readonly string selectPattern = "SELECT(.[\n]?)+(FROM)?";
        private readonly string dmlPattern = @"(INSERT|UPDATE|DELETE|TRUNCATE|ALTER|INTO)";
        private readonly string createAlterScriptPattern = "(CREATE|ALTER).+(TABLE|VIEW|FUNCTION|PROCEDURE|TRIGGER)";
        private DbInterpreter dbInterpreter;
        private string script;

        public ScriptParser(DbInterpreter dbInterpreter, string script)
        {
            this.dbInterpreter = dbInterpreter;
            this.script = script;
        }

        public string Parse()
        {
            StringBuilder sb = new StringBuilder();

            string []lines = this.script.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach(string line in lines)
            {
                if(line.StartsWith(this.dbInterpreter.CommentString))
                {
                    continue;
                }

                sb.AppendLine(line);
            }

            this.script = sb.ToString();

            return this.script;
        }

        public bool IsSelect()
        {
            if (Regex.IsMatch(this.script, selectPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline) && !Regex.IsMatch(this.script, dmlPattern, RegexOptions.IgnoreCase))
            {
                return true;
            }

            return false;
        }

        public bool IsCreateOrAlterScript()
        {
            return Regex.IsMatch(this.script, this.createAlterScriptPattern);
        }
    }
}
