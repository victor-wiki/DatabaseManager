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
        private readonly string dmlPattern = @"(CREATE|ALTER|INSERT|UPDATE|DELETE|TRUNCATE|INTO)";
        private readonly string createAlterScriptPattern = "(CREATE|ALTER).+(VIEW|FUNCTION|PROCEDURE|TRIGGER)";
        private DbInterpreter dbInterpreter;
        private string originalScript;
        private string cleanScript;

        public string Script => this.originalScript;
        public string CleanScript => this.cleanScript;

        public ScriptParser(DbInterpreter dbInterpreter, string script)
        {
            this.dbInterpreter = dbInterpreter;
            this.originalScript = script;

            this.Parse();
        }

        private string Parse()
        {
            StringBuilder sb = new StringBuilder();

            string []lines = this.originalScript.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach(string line in lines)
            {
                if (line.Trim().StartsWith(this.dbInterpreter.CommentString))
                {
                    continue;
                }

                sb.AppendLine(line);
            }

            this.cleanScript = sb.ToString();

            return this.cleanScript;
        }

        public bool IsSelect()
        {
            if (Regex.IsMatch(this.cleanScript, selectPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline) && !Regex.IsMatch(this.originalScript, dmlPattern, RegexOptions.IgnoreCase))
            {
                return true;
            }

            return false;
        }

        public bool IsCreateOrAlterScript()
        {
            return Regex.IsMatch(this.cleanScript, this.createAlterScriptPattern);
        }
    }
}
