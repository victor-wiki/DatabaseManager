using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseInterpreter.Core
{
    public class SqlBuilder
    {
        private List<string> lines = new List<string>();
        public string Content => this.ToString();

        public void Append(string sql)
        {
            string[] items = sql.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in items)
            {
                string line = item.Trim();

                if (line.Length > 0)
                {
                    this.lines.Add(line);
                }
            }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, this.lines.Select(item => item));
        }
    }
}
