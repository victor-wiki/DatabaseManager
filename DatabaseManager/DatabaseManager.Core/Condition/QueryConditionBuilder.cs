using DatabaseManager.Model;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseManager.Core
{
    public class QueryConditionBuilder
    {
        public char QuotationLeftChar { get; set; }
        public char QuotationRightChar { get; set; }

        private List<QueryConditionItem> conditions = new List<QueryConditionItem>();

        public List<QueryConditionItem> Conditions => this.conditions;

        public void Add(QueryConditionItem condition)
        {
            this.conditions.Add(condition);
        }

        public override string ToString()
        {
            return string.Join(" AND ", this.conditions.Select(item=> $"({this.QuotationLeftChar}{item.ColumnName}{this.QuotationRightChar} {item.ToString()})" ));
        }
    }
}
