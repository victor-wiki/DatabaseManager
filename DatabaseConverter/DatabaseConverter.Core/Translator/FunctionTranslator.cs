using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using SqlAnalyser.Core;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static TSqlParser;

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

            List<FunctionFormula> formulas = GetFunctionFormulas(this.sourceDbInterpreter, value);

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
                        bool noParenthesess = false;
                        bool hasArgs = false;

                        var targetFuncSpec = this.targetFuncSpecs.FirstOrDefault(item => item.Name == targetFunctionInfo.Name);

                        if (targetFuncSpec != null)
                        {
                            #region Handle specials
                            if (!string.IsNullOrEmpty(targetFunctionInfo.Specials))
                            {
                                List<FunctionArgumentItemInfo> sourceArgItems = GetFunctionArgumentTokens(sourceFuncSpec, null);
                                List<FunctionArgumentItemInfo> targetArgItems = GetFunctionArgumentTokens(targetFuncSpec, targetFunctionInfo.Args);

                                var args = formula.GetArgs();

                                Func<string, string> getTrimedContent = (content) =>
                                {
                                    return content.Trim('\'');
                                };

                                foreach (var tai in targetArgItems)
                                {
                                    string upperContent = tai.Content.ToUpper();

                                    if (upperContent == "UNIT" || upperContent == "'UNIT'")
                                    {
                                        var sourceItem = sourceArgItems.FirstOrDefault(item => getTrimedContent(item.Content) == getTrimedContent(tai.Content));

                                        if (sourceItem != null && args.Count > sourceItem.Index)
                                        {
                                            string arg = args[sourceItem.Index];

                                            string[] specials = targetFunctionInfo.Specials.Split(';');

                                            foreach (var special in specials)
                                            {
                                                string[] items = special.Split(':');

                                                string[] subItems = items[0].Split('=');

                                                string k = subItems[0];
                                                string v = subItems[1];

                                                if (k.ToUpper() == upperContent)
                                                {
                                                    string mappedUnit = DatetimeHelper.GetMappedUnit(this.sourceDbType, this.targetDbType, arg);

                                                    if (mappedUnit == v)
                                                    {
                                                        targetFunctionInfo = new MappingFunctionInfo() { Name = items[1] };

                                                        targetFuncSpec = this.targetFuncSpecs.FirstOrDefault(item => item.Name == targetFunctionInfo.Name);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            } 
                            #endregion

                            noParenthesess = targetFuncSpec.NoParenthesess;

                            hasArgs = !string.IsNullOrEmpty(targetFuncSpec.Args);
                        }

                        string oldExp = formula.Expression;
                        //string newExp = ReplaceValue(formula.Expression, name, targetFunctionInfo.Name);
                        string newExp = $"{targetFunctionInfo.Name}{(formula.HasParentheses ? "(" : "")}{formula.Body}{(formula.HasParentheses ? ")" : "")}";

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
        
        public static List<FunctionFormula> GetFunctionFormulas(DbInterpreter dbInterpreter, string value, bool extractChildren = true)
        {
            value = StringHelper.GetBalanceParenthesisTrimedValue(value);

            var functionSpecifications = FunctionManager.GetFunctionSpecifications(dbInterpreter.DatabaseType);

            List<FunctionFormula> functions = new List<FunctionFormula>();

            var trimChars = TranslateHelper.GetTrimChars(dbInterpreter).ToArray();

            Func<string, bool> isValidFunction = (name) =>
            {
                return functionSpecifications.Any(item => item.Name.ToUpper() == name.Trim().Trim(trimChars).ToUpper());
            };

            if (value.IndexOf("(") < 0)
            {
                if (isValidFunction(value))
                {
                    functions.Add(new FunctionFormula(value, value));
                }
            }
            else
            {
                string select = "SELECT ";

                string sql = $"{select}{value}";

                if (dbInterpreter.DatabaseType == DatabaseType.Oracle)
                {
                    sql += " FROM DUAL";
                }

                SqlAnalyserBase sqlAnalyser = TranslateHelper.GetSqlAnalyser(dbInterpreter.DatabaseType, sql);

                sqlAnalyser.RuleAnalyser.Option.ParseTokenChildren = false;
                sqlAnalyser.RuleAnalyser.Option.ExtractFunctions = true;
                sqlAnalyser.RuleAnalyser.Option.ExtractFunctionChildren = extractChildren;
                sqlAnalyser.RuleAnalyser.Option.IsCommonScript = true;

                var result = sqlAnalyser.AnalyseCommon();

                if (!result.HasError)
                {
                    List<TokenInfo> tokens = result.Script.Functions;

                    foreach (TokenInfo token in tokens)
                    {
                        string symbol = token.Symbol;

                        string name = TranslateHelper.ExtractNameFromParenthesis(symbol);

                        if (isValidFunction(name))
                        {
                            TranslateHelper.RestoreTokenValue(sql, token);

                            FunctionFormula formula = new FunctionFormula(name, token.Symbol);

                            functions.Add(formula);
                        }
                    }
                }
            }

            return functions;
        }
    }
}
