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
        private List<FunctionSpecification> sourceFuncSpecs;
        private List<FunctionSpecification> targetFuncSpecs;

        public RoutineType RoutineType { get; set; }

        public FunctionTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter) : base(sourceInterpreter, targetInterpreter)
        {
        }

        public FunctionTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter, IEnumerable<TokenInfo> functions) : base(sourceInterpreter, targetInterpreter)
        {
            this.functions = functions;
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
                    token.Symbol = this.GetMappedFunction(token.Symbol);
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
            if (this.sourceDbType == DatabaseType.Postgres)
            {
                value = value.Replace(@"""substring""", "substring", System.StringComparison.OrdinalIgnoreCase);
            }

            List<FunctionFormula> formulas = GetFunctionFormulas(value);

            foreach (FunctionFormula formula in formulas)
            {
                string name = formula.Name;

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var sourceFuncSpec = this.sourceFuncSpecs.FirstOrDefault(item => item.Name == name.ToUpper());

                if (sourceFuncSpec == null)
                {
                    continue;
                }

                bool useBrackets = false;

                MappingFunctionInfo targetFunctionInfo = this.GetMappingFunctionInfo(name, formula.Body, out useBrackets);

                if (!string.IsNullOrEmpty(targetFunctionInfo.Name))
                {
                    if (targetFunctionInfo.Name.ToUpper().Trim() != name.ToUpper().Trim())
                    {
                        string oldExp = formula.Expression;
                        //string newExp = ReplaceValue(formula.Expression, name, targetFunctionInfo.Name);
                        string newExp = $"{targetFunctionInfo.Name}{(formula.HasParentheses ? "(" : "")}{formula.Body}{(formula.HasParentheses ? ")" : "")}";

                        bool noParenthesess = false;
                        bool hasArgs = false;
                        var targetFuncSpec = this.targetFuncSpecs.FirstOrDefault(item => item.Name == targetFunctionInfo.Name);

                        if (targetFuncSpec != null)
                        {
                            noParenthesess = targetFuncSpec.NoParenthesess;

                            hasArgs = !string.IsNullOrEmpty(targetFuncSpec.Args);
                        }

                        if (!hasArgs && !string.IsNullOrEmpty(formula.Body))
                        {
                            newExp = $"{targetFunctionInfo.Name}()";
                        }

                        if (noParenthesess)
                        {
                            newExp = newExp.Replace("()", "");
                        }
                        else
                        {
                            if (sourceFuncSpec.NoParenthesess && targetFuncSpec != null && string.IsNullOrEmpty(targetFuncSpec.Args))
                            {
                                newExp += "()";
                            }
                        }

                        newExp = newExp.Replace("()()", "()");

                        formula.Expression = newExp;

                        value = ReplaceValue(value, oldExp, newExp);
                    }
                }

                Dictionary<string, string> dictDataType = null;

                string newExpression = this.ParseFormula(this.sourceFuncSpecs, this.targetFuncSpecs, formula, targetFunctionInfo, out dictDataType, this.RoutineType);

                if (newExpression != formula.Expression)
                {
                    value = ReplaceValue(value, formula.Expression, newExpression);
                }
            }

            return value;
        }

        public static List<FunctionFormula> GetFunctionFormulas(string value)
        {
            value = StringHelper.TrimParenthesis(value);

            List<FunctionFormula> functions = new List<FunctionFormula>();

            if (value.IndexOf("(") < 0)
            {
                functions.Add(new FunctionFormula(value, value) { StartIndex = 0, StopIndex = value.Length - 1 });
            }
            else
            {
                string innerContent = value;

                Regex parenthesesRegex = new Regex(RegexHelper.ParenthesesRegexPattern, RegexOptions.Multiline);

                int count = 0;

                while (parenthesesRegex.IsMatch(innerContent))
                {
                    count++;

                    int firstLeftParenthesesIndex = innerContent.IndexOf('(');
                    int lastRightParenthesesIndex = -1;

                    int leftParenthesesCount = 0;
                    int rightParenthesesCount = 0;
                    int singleQuotationCharCount = 0;

                    for (int i = 0; i < innerContent.Length; i++)
                    {
                        var c = innerContent[i];

                        if (c == '(')
                        {
                            if (singleQuotationCharCount % 2 == 0)
                            {
                                leftParenthesesCount++;
                            }
                        }
                        else if (c == ')')
                        {
                            if (singleQuotationCharCount % 2 == 0)
                            {
                                rightParenthesesCount++;

                                if (rightParenthesesCount == leftParenthesesCount)
                                {
                                    lastRightParenthesesIndex = i;
                                    break;
                                }
                            }
                        }
                        else if (c == '\'')
                        {
                            singleQuotationCharCount++;
                        }
                    }

                    if (lastRightParenthesesIndex == -1)
                    {
                        break;
                    }

                    string leftContent = innerContent.Substring(0, firstLeftParenthesesIndex);

                    string cleanName = ExtractName(leftContent.Trim());

                    Regex nameRegex = new Regex(RegexHelper.NameRegexPattern, RegexOptions.IgnoreCase);
                    var matches = nameRegex.Matches(cleanName);

                    Match nameMatch = matches.Cast<Match>().LastOrDefault();

                    if (nameMatch != null)
                    {
                        string name = nameMatch.Value;

                        int startIndex = leftContent.LastIndexOf(cleanName, leftContent.Length - 1, System.StringComparison.OrdinalIgnoreCase);

                        int length = lastRightParenthesesIndex - startIndex + 1;

                        string expression = innerContent.Substring(startIndex, length);

                        FunctionFormula func = new FunctionFormula(name, expression)
                        {
                            StartIndex = startIndex,
                            StopIndex = lastRightParenthesesIndex
                        };

                        functions.Add(func);

                        string oldInnerContent = innerContent;

                        innerContent = oldInnerContent.Substring(firstLeftParenthesesIndex + 1, (func.StopIndex - 1) - (firstLeftParenthesesIndex + 1) + 1);

                        if (!parenthesesRegex.IsMatch(innerContent) || IsNotFunction(innerContent))
                        {
                            if (lastRightParenthesesIndex < value.Length - 1)
                            {
                                string rightContent = oldInnerContent.Substring(lastRightParenthesesIndex + 1).Trim(' ', ',');

                                innerContent = rightContent;
                            }
                        }
                    }
                    else
                    {
                        if (lastRightParenthesesIndex < innerContent.Length - 1)
                        {
                            innerContent = innerContent.Substring(lastRightParenthesesIndex + 1).Trim(' ', ',');
                        }
                    }

                    if (count > 1000) //avoid infinite loop
                    {
                        break;
                    }
                }
            }

            return functions;
        }

        private static bool IsNotFunction(string value)
        {
            MatchCollection matches = Regex.Matches(value, RegexHelper.ParenthesesRegexPattern);

            int notFunctionCount = 0;

            foreach (Match match in matches)
            {
                var mv = StringHelper.GetBalanceParenthesisTrimedValue(match.Value);

                if (mv.Contains(","))
                {
                    string[] items = mv.Split(',');

                    if (items.Length == 2 && items.All(item => int.TryParse(item.Trim(), out _)))
                    {
                        notFunctionCount++;
                    }
                }
                else
                {
                    if (int.TryParse(mv, out _) || mv.ToLower() == "max")
                    {
                        notFunctionCount++;
                    }
                }
            }

            return notFunctionCount == matches.Count;
        }

        private static string ExtractName(string value)
        {
            var chars = value.ToArray();

            List<char> name = new List<char>();

            for (int i = chars.Length - 1; i >= 0; i--)
            {
                if (Regex.IsMatch(chars[i].ToString(), RegexHelper.NameRegexPattern))
                {
                    name.Add(chars[i]);
                }
                else
                {
                    break;
                }
            }

            name.Reverse();

            return string.Join("", name);
        }
    }
}
