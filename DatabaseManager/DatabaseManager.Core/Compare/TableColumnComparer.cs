using DatabaseInterpreter.Model;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using System;
using System.Collections.Generic;
using System.Text;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;
using Utility = DatabaseInterpreter.Utility;

namespace DatabaseManager.Core
{
    public class TableColumnComparer : BaseTypeComparer
    {
        public TableColumnComparer(RootComparer rootComparer) : base(rootComparer)
        {

        }

        public override void CompareType(CompareParms parms)
        {
            var column1 = (TableColumn)parms.Object1;
            var column2 = (TableColumn)parms.Object2;

            if (!this.IsEquals(column1, column2))
            {
                this.AddDifference(parms);
            }
        }

        private bool IsEquals(TableColumn column1, TableColumn column2)
        {
            if (column1.Name != column2.Name
              || column1.DataType != column2.DataType
              || column1.IsNullable != column2.IsNullable
              || column1.IsIdentity != column2.IsIdentity
              || column1.MaxLength != column2.MaxLength
              || column2.Precision != column2.Precision
              || column2.Scale != column2.Scale
              || column1.Comment != column2.Comment
              )
            {
                return false;
            }

            if (!this.IsEqualsWithParenthesis(column1.DefaultValue, column2.DefaultValue)
               || !this.IsEqualsWithParenthesis(column1.ComputeExp, column2.ComputeExp)
                )
            {
                return false;
            }

            return true;
        }

        private bool IsEqualsWithParenthesis(string value1, string value2)
        {
            return Utility.StringHelper.GetBalanceParenthesisTrimedValue(value1) == Utility.StringHelper.GetBalanceParenthesisTrimedValue(value2);
        }

        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return type1 == type2 && type1 == typeof(TableColumn);
        }
    }
}
