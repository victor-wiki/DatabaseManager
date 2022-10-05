using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using SqlAnalyser.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class FunctionTranslator : DbObjectTranslator
    {
        private IEnumerable<TokenInfo> functions;
        private DatabaseType sourceDbType;
        private DatabaseType targetDbType;

        private const string ParenthesesExpression = @"\(.*\)";
        private const string NameExpression = @"\b([a-zA-Z]+)\b";

        private List<FunctionSpecification> sourceFuncSpecs;
        private List<FunctionSpecification> targetFuncSpecs;

        public FunctionTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter) : base(sourceInterpreter, targetInterpreter)
        {
            this.sourceDbType = sourceInterpreter.DatabaseType;
            this.targetDbType = targetInterpreter.DatabaseType;
        }

        public FunctionTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter, IEnumerable<TokenInfo> functions) : base(sourceInterpreter, targetInterpreter)
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
            this.LoadFunctionSpecifications();           

            if (this.functions != null)
            {
                foreach (TokenInfo token in this.functions)
                {
                    List<FunctionFomular> fomulars = GetFunctionFomulars(token.Symbol);

                    token.Symbol = GetMappedFunction(token.Symbol);
                }
            }
        }

        public void LoadFunctionSpecifications()
        {
            this.sourceFuncSpecs = FunctionManager.GetFunctionSpecifications(this.sourceDbType);
            this.targetFuncSpecs = FunctionManager.GetFunctionSpecifications(this.targetDbType);
        }

        public string GetMappedFunction(string value)
        {
            List<FunctionFomular> fomulars = GetFunctionFomulars(value);

            foreach (FunctionFomular fomular in fomulars)
            {
                string name = fomular.Name;

                if(string.IsNullOrEmpty(name))
                {
                    continue;
                }

                bool useBrackets = false;

                MappingFunctionInfo targetFunctionInfo = this.GetMappingFunctionInfo(name, out useBrackets);

                if (!string.IsNullOrEmpty(targetFunctionInfo.Name))
                {
                    if (targetFunctionInfo.Name.ToUpper().Trim() != name.ToUpper().Trim())
                    {
                        string oldExp = fomular.Expression;
                        string newExp = ReplaceValue(fomular.Expression, name, targetFunctionInfo.Name);

                        if (!targetFunctionInfo.Name.Contains("()"))
                        {
                            newExp = newExp.Replace("()", "");
                        }
                        else
                        {
                            newExp = newExp.Replace("()()", "()");
                        }

                        fomular.Expression = newExp;

                        value = ReplaceValue(value, oldExp, newExp);
                    }
                }

                Dictionary<string, string> dictDataType = null;
                string newExpression = ParseFomular(this.sourceFuncSpecs, this.targetFuncSpecs, fomular, targetFunctionInfo, out dictDataType);

                if (newExpression != fomular.Expression)
                {
                    value = ReplaceValue(value, fomular.Expression, newExpression);
                }
            }

            return value;
        }

        public static List<FunctionFomular> GetFunctionFomulars(string value)
        {
            value = StringHelper.TrimBracket(value);

            List<FunctionFomular> functions = new List<FunctionFomular>();

            if(value.IndexOf("(")<0)
            {
                functions.Add(new FunctionFomular(value, value) { StartIndex=0, StopIndex= value.Length-1 });
            }
            else
            {
                string innerContent = value;

                Regex parenthesesRegex = new Regex(ParenthesesExpression);

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

                    Regex nameRegex = new Regex(NameExpression, RegexOptions.IgnoreCase);
                    var matches = nameRegex.Matches(leftContent);

                    Match nameMatch = matches.Cast<Match>().LastOrDefault();

                    if (nameMatch != null)
                    {
                        string name = nameMatch.Value;

                        int startIndex = nameMatch.Index;

                        int length = lastRightParenthesesIndex - startIndex + 1;

                        string expression = innerContent.Substring(startIndex, length);

                        FunctionFomular func = new FunctionFomular(name, expression)
                        {
                            StartIndex = startIndex,
                            StopIndex = lastRightParenthesesIndex
                        };

                        functions.Add(func);

                        innerContent = innerContent.Substring(firstLeftParenthesesIndex + 1, (func.StopIndex - 1) - (firstLeftParenthesesIndex + 1) + 1);
                    }
                    else
                    {
                        if (functions.Count == 0 && !leftContent.Contains("("))
                        {
                            string name = value.Substring(0, firstLeftParenthesesIndex);

                            FunctionFomular func = new FunctionFomular(name, value)
                            {
                                StartIndex = 0,
                                StopIndex = value.Length - 1
                            };

                            functions.Add(func);
                        }

                        break;
                    }
                }
            }            

            return functions;
        }
    }
}
