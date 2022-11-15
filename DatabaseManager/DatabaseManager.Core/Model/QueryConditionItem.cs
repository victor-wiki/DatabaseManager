using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Utility;
using DatabaseManager.Helper;

namespace DatabaseManager.Model
{
    public class QueryConditionItem
    {
        public string ColumnName { get; set; }
        public Type DataType { get; set; }
        public QueryConditionMode Mode { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public List<string> Values { get; set; } = new List<string>();
        public bool NeedQuoted => FrontQueryHelper.NeedQuotedForSql(this.DataType);

        private string GetValue(string value)
        {
            return this.NeedQuoted ? $"'{FrontQueryHelper.GetSafeValue(value)}'" : value;
        }

        public override string ToString()
        {
            string conditon = "";

            if (this.Mode == QueryConditionMode.Single)
            {
                string value = this.Operator.Contains("LIKE") ? $"'%{this.Value}%'" : this.GetValue(this.Value);

                conditon = $"{this.Operator} {value}";
            }
            else if (this.Mode == QueryConditionMode.Range)
            {
                conditon = $"BETWEEN {this.GetValue(this.From)} AND {this.GetValue(this.To)}";
            }
            else if (this.Mode == QueryConditionMode.Series)
            {
                conditon = $"IN({string.Join(",", this.Values.Select(item => this.GetValue(item)))})";
            }

            return conditon;
        }
    }

    public enum QueryConditionMode
    {
        Single = 0,
        Range = 1,
        Series = 2
    }
}
