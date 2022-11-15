using DatabaseInterpreter.Model;
using DatabaseManager.Model;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseManager.Core
{
    public class QueryConditionBuilder
    {
        public DatabaseType DatabaseType { get; set; }
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
            return string.Join(" AND ", this.conditions.Select(item=> $"({this.GetConditionItemValue(item)})" ));
        }

        private string GetConditionItemValue(QueryConditionItem item)
        {
            string typeConvert = "";

            if(item.NeedQuoted)
            {
                if(this.DatabaseType == DatabaseType.Postgres)
                {
                    typeConvert = "::CHARACTER VARYING ";
                }
            }

            string value = $"{ this.QuotationLeftChar}{item.ColumnName}{ this.QuotationRightChar}{typeConvert}{ item.ToString()}";

            return value;            
        }
    }
}
