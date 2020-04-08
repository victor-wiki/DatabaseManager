using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class FunctionTranslator : DbObjectTranslator
    {
        private List<TokenInfo> functions;
        private DatabaseType sourceDbType;
        private DatabaseType targetDbType;

        private Regex parenthesesRegex = new Regex(@"\(.*\)");
        private Regex nameRegex = new Regex(@"\b([a-zA-Z]+)\b", RegexOptions.IgnoreCase);       

        public FunctionTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter, List<TokenInfo> functions) : base(sourceInterpreter, targetInterpreter)
        {
            this.functions = functions;
            this.sourceDbType = sourceInterpreter.DatabaseType;
            this.targetDbType = targetInterpreter.DatabaseType;
        }

        public override void Translate()
        {
            if (this.sourceDbType == this.targetDbType)
            {
                return;
            }

            this.LoadMappings();

            foreach (TokenInfo token in this.functions)
            {
                List<FunctionFomular> fomulars = this.GetFunctionFomulars(token.Symbol);

                foreach (FunctionFomular fomular in fomulars)
                {
                    string name = fomular.Name;

                    int leftParenthesesCount = 0;
                    int rightParenthesesCount = 0;
                    string dataType = "";
                    string newDataType = "";

                    switch (name.ToUpper())
                    {
                        case "CONVERT":

                            string body = fomular.Body;

                            int firstCommaIndex = body.IndexOf(',');

                            string[] args = new string[2] { body.Substring(0, firstCommaIndex), body.Substring(firstCommaIndex + 1) };

                            string expression = "";
                            dataType = "";

                            if (sourceDbInterpreter is SqlServerInterpreter)
                            {
                                dataType = args[0];
                                expression = args[1];
                            }
                            else if (sourceDbInterpreter is MySqlInterpreter)
                            {
                                dataType = args[1];
                                expression = args[0];
                            }

                            newDataType = this.GetNewDataType(dataTypeMappings, dataType);                           

                            string newExpression = "";

                            if (targetDbInterpreter is OracleInterpreter)
                            {
                                newExpression = $"CAST({expression} AS {newDataType})";
                            }
                            else if
                            (
                                (sourceDbInterpreter is SqlServerInterpreter || sourceDbInterpreter is MySqlInterpreter)
                                &&
                                (targetDbInterpreter is SqlServerInterpreter || targetDbInterpreter is MySqlInterpreter)
                            )
                            {
                                newExpression = this.ExchangeFunctionArgs(name, newDataType, expression);
                            }

                            token.Symbol = this.ReplaceValue(token.Symbol, fomular.Expression, newExpression);

                            break;

                        case "CAST":

                            int asIndex = fomular.Expression.ToUpper().IndexOf(" AS ");

                            string arg = fomular.Expression.Substring(name.Length, asIndex - 3);

                            int functionEndIndex = -1;
                            for (int i = asIndex + 3; i < fomular.Expression.Length; i++)
                            {
                                if (fomular.Expression[i] == '(')
                                {
                                    leftParenthesesCount++;
                                }
                                else if (fomular.Expression[i] == ')')
                                {
                                    rightParenthesesCount++;
                                }

                                if (rightParenthesesCount - leftParenthesesCount == 1)
                                {
                                    dataType = fomular.Expression.Substring(asIndex + 4, i - asIndex - 4);
                                    functionEndIndex = i;
                                    break;
                                }
                            }

                            newDataType = this.GetNewDataType(dataTypeMappings, dataType);                        

                            token.Symbol = this.ReplaceValue(token.Symbol, dataType, newDataType);

                            break;

                        default:

                            string text = name;
                            string textWithBrackets = name.ToLower() + "()";

                            if (this.functionMappings.Any(item => item.Any(t => t.Function.ToLower() == textWithBrackets)))
                            {
                                text = textWithBrackets;
                            }

                            IEnumerable<FunctionMapping> funcMappings = this.functionMappings.FirstOrDefault(item => item.Any(t => t.DbType == sourceDbInterpreter.DatabaseType.ToString() && t.Function.Split(',').Any(m => m.ToLower() == text.ToLower())));

                            if (funcMappings != null)
                            {
                                string targetFunction = funcMappings.FirstOrDefault(item => item.DbType == targetDbInterpreter.DatabaseType.ToString())?.Function.Split(',')?.FirstOrDefault();

                                token.Symbol = this.ReplaceValue(token.Symbol, name, targetFunction);
                            }

                            break;
                    }
                }
            }
        }        

        private List<FunctionFomular> GetFunctionFomulars(string value)
        {
            List<FunctionFomular> functions = new List<FunctionFomular>();

            string innerContent = value;

            while (parenthesesRegex.IsMatch(innerContent))
            {
                int firstLeftParenthesesIndex = innerContent.IndexOf('(');
                int lastRightParenthesesIndex = -1;

                int leftParenthesesCount = 0;
                int rightParenthesesCount = 0;

                for (int i = 0; i < innerContent.Length; i++)
                {
                    if (innerContent[i] == '(')
                    {
                        leftParenthesesCount++;
                    }
                    else if (innerContent[i] == ')')
                    {
                        rightParenthesesCount++;

                        if (rightParenthesesCount == leftParenthesesCount)
                        {
                            lastRightParenthesesIndex = i;
                            break;
                        }
                    }
                }

                if (lastRightParenthesesIndex == -1)
                {
                    break;
                }

                string leftContent = innerContent.Substring(0, firstLeftParenthesesIndex);

                var matches = nameRegex.Matches(leftContent);

                Match nameMatch = matches.Cast<Match>().LastOrDefault();

                if (nameMatch != null)
                {
                    string name = nameMatch.Value;

                    int startIndex = nameMatch.Index;

                    FunctionFomular func = new FunctionFomular()
                    {
                        Name = name,
                        StartIndex = startIndex,
                        StopIndex = lastRightParenthesesIndex
                    };

                    int length = func.Length;

                    string expression = innerContent.Substring(startIndex, length);

                    func.Expression = expression;

                    functions.Add(func);

                    innerContent = innerContent.Substring(firstLeftParenthesesIndex + 1, (func.StopIndex - 1) - (firstLeftParenthesesIndex + 1) + 1);
                }
                else
                {
                    break;
                }
            }

            return functions;
        }
    }    
}
