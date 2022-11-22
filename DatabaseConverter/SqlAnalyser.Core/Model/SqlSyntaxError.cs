using System.Collections.Generic;
using System.Text;

namespace SqlAnalyser.Model
{
    public class SqlSyntaxError
    {
        public bool HasError => this.Items != null && this.Items.Count > 0;

        public List<SqlSyntaxErrorItem> Items = new List<SqlSyntaxErrorItem>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (SqlSyntaxErrorItem item in this.Items)
            {
                sb.AppendLine($"{item.Text}(Line={item.Line},Column={item.Column},StartIndex={item.StartIndex},StopIndex={item.StopIndex}):{item.Message};");
            }

            return sb.ToString();
        }
    }

    public class SqlSyntaxErrorItem
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public int StartIndex { get; set; }
        public int StopIndex { get; set; }
        public string Text { get; set; }
        public string Message { get; set; }
    }
}
