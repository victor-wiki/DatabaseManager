using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseManager.Core
{
    public class ScriptParser
    {
        private readonly string selectPattern = "(SELECT).+(FROM)";
        private readonly string dmlPattern = @"(INSERT|UPDATE|DELETE|TRUNCATE|ALTER|INTO|SET)";
        private readonly string createAlterScriptPattern = "(CREATE|ALTER).+(TABLE|VIEW|FUNCTION|PROCEDURE|TRIGGER)";
        private string script;

        public ScriptParser(string script)
        {
            this.script = script;
        }

        public bool IsSelect()
        {
            if (Regex.IsMatch(this.script, selectPattern, RegexOptions.IgnoreCase) && !Regex.IsMatch(this.script, dmlPattern, RegexOptions.IgnoreCase))
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
