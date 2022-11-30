using DatabaseInterpreter.Utility;
using SqlAnalyser.Model;
using System.Collections.Generic;
using System.Linq;

namespace SqlAnalyser.Core
{
    public class MySqlAnalyserHelper
    {
        public static void RearrangeStatements(List<Statement> statements)
        {
            FetchCursorStatement fetchCursorStatement = null;          

            List<FetchCursorStatement> statementsNeedToRemove = new List<FetchCursorStatement>();

            foreach (Statement statement in statements)
            {
                if (statement is FetchCursorStatement fetch)
                {
                    fetchCursorStatement = fetch;
                    continue;
                }
                else if (statement is WhileStatement @while)
                {
                    FetchCursorStatement fs = @while.Statements.FirstOrDefault(item => item is FetchCursorStatement) as FetchCursorStatement;

                    if (fetchCursorStatement != null && fs != null)
                    {
                        statementsNeedToRemove.Add(fetchCursorStatement);

                        @while.Condition.Symbol = "FINISHED = 0";

                        int index = @while.Statements.IndexOf(fs);

                        @while.Statements.Insert(0, fs);

                        @while.Statements.RemoveAt(index + 1);
                    }                    
                }
            }

            statements.RemoveAll(item => statementsNeedToRemove.Contains(item));
        }

        public static UserVariableDataType DetectUserVariableDataType(string value)
        {
            if (DataTypeHelper.StartsWithN(value) || ValueHelper.IsStringValue(value))
            {
                return UserVariableDataType.String;
            }
            else if(int.TryParse(value, out _))
            {
                return UserVariableDataType.Integer;
            }
            else if(decimal.TryParse(value, out _))
            {
                return UserVariableDataType.Decimal;
            }

            return UserVariableDataType.Unknown;
        }
    }
}
