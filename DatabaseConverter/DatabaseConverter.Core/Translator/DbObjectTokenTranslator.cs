using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSQL;
using TSQL.Tokens;

namespace DatabaseConverter.Core
{
    public class DbObjectTokenTranslator : DbObjectTranslator
    {
        private List<string> convertedDataTypes = new List<string>();
        private List<string> convertedFunctions = new List<string>();

        private List<FunctionSpecification> sourceFuncSpecs;
        private List<FunctionSpecification> targetFuncSpecs;

        public DbObjectTokenTranslator(DbInterpreter source, DbInterpreter target) : base(source, target) { }

        public override void Translate()
        {
            
        }

        public virtual string ParseDefinition(string definition)
        {
            var tokens = this.GetTokens(definition);
            bool changed = false;

            definition = this.HandleDefinition(definition, tokens, out changed);

            if (changed)
            {
                tokens = this.GetTokens(definition);
            }

            definition = this.BuildDefinition(tokens);

            return definition;
        }

        protected string HandleDefinition(string definition, List<TSQLToken> tokens, out bool changed)
        {
            this.sourceFuncSpecs = FunctionManager.GetFunctionSpecifications(this.sourceDbInterpreter.DatabaseType);
            this.targetFuncSpecs = FunctionManager.GetFunctionSpecifications(this.targetDbInterpreter.DatabaseType);

            changed = false;

            string newDefinition = definition;

            foreach (TSQLToken token in tokens)
            {
                string text = token.Text;
                string functionExpression = null;

                switch (token.Type)
                {
                    case TSQLTokenType.SystemIdentifier:                      
                    case TSQLTokenType.Identifier:

                        if (sourceFuncSpecs.Any(item => item.Name == text.ToUpper()))
                        {
                            functionExpression = this.GetFunctionExpression(token, definition);

                            break;
                        }

                        break;
                    case TSQLTokenType.Keyword:
                        break;
                }

                if (!string.IsNullOrEmpty(functionExpression))
                {
                    bool useBrackets = false;
                    MappingFunctionInfo targetFunctionInfo = GetMappingFunctionInfo(text, null, out useBrackets);

                    FunctionFormula formula = new FunctionFormula(functionExpression);

                    Dictionary<string, string> dictDataType = null;

                    if (formula.Name == null)
                    {
                        continue;
                    }

                    string newExpression = ParseFormula(this.sourceFuncSpecs, this.targetFuncSpecs, formula, targetFunctionInfo, out dictDataType);

                    if (newExpression != formula.Expression)
                    {
                        newDefinition = ReplaceValue(newDefinition, formula.Expression, newExpression);

                        changed = true;
                    }

                    if (dictDataType != null)
                    {
                        this.convertedDataTypes.AddRange(dictDataType.Values);
                    }

                    if (!string.IsNullOrEmpty(targetFunctionInfo.Args) && changed)
                    {
                        if (!this.convertedFunctions.Contains(targetFunctionInfo.Name))
                        {
                            this.convertedFunctions.Add(targetFunctionInfo.Name);
                        }
                    }
                }
            }         

            return newDefinition;
        }

        private string GetFunctionExpression(TSQLToken token, string definition)
        {
            int startIndex = startIndex = token.BeginPosition;
            int functionEndIndex = functionEndIndex = this.FindFunctionEndIndex(startIndex + token.Text.Length, definition);

            string functionExpression = null;

            if (functionEndIndex != -1)
            {
                functionExpression = definition.Substring(startIndex, functionEndIndex - startIndex + 1);
            }

            return functionExpression;
        }

        private int FindFunctionEndIndex(int startIndex, string definition)
        {
            int leftBracketCount = 0;
            int rightBracketCount = 0;
            int functionEndIndex = -1;

            for (int i = startIndex; i < definition.Length; i++)
            {
                if (definition[i] == '(')
                {
                    leftBracketCount++;
                }
                else if (definition[i] == ')')
                {
                    rightBracketCount++;
                }

                if (rightBracketCount == leftBracketCount)
                {
                    functionEndIndex = i;
                    break;
                }
            }

            return functionEndIndex;
        }

        public string BuildDefinition(List<TSQL.Tokens.TSQLToken> tokens)
        {
            StringBuilder sb = new StringBuilder();

            this.sourceSchemaName = sourceDbInterpreter.DefaultSchema;

            var sourceDataTypeSpecs = DataTypeManager.GetDataTypeSpecifications(this.sourceDbInterpreter.DatabaseType);
            var targetDataTypeSpecs = DataTypeManager.GetDataTypeSpecifications(this.targetDbInterpreter.DatabaseType);

            int ignoreCount = 0;

            TSQLTokenType previousType = TSQLTokenType.Whitespace;
            string previousText = "";

            for (int i = 0; i < tokens.Count; i++)
            {
                if (ignoreCount > 0)
                {
                    ignoreCount--;
                    continue;
                }

                var token = tokens[i];

                var tokenType = token.Type;
                string text = token.Text;

                switch (tokenType)
                {
                    case TSQLTokenType.Identifier:

                        var nextToken = i + 1 < tokens.Count ? tokens[i + 1] : null;

                        if (convertedDataTypes.Contains(text))
                        {
                            sb.Append(text);
                            continue;
                        }

                        //Remove schema name
                        if (nextToken != null && nextToken.Text.Trim() != "(" &&
                            text.Trim('"') == sourceSchemaName && i + 1 < tokens.Count && tokens[i + 1].Text == "."
                            )
                        {
                            ignoreCount++;
                            continue;
                        }
                        else if (nextToken != null && nextToken.Text.Trim() == "(") //function handle
                        {
                            if (this.convertedFunctions.Contains(text))
                            {
                                sb.Append(text);
                                continue;
                            }

                            string textWithBrackets = text.ToLower() + "()";

                            bool useBrackets = false;

                            MappingFunctionInfo targetFunctionInfo = GetMappingFunctionInfo(text, null, out useBrackets);

                            if (targetFunctionInfo.Name.ToLower() != text.ToLower())
                            {
                                string targetFunction = targetFunctionInfo.Name;

                                if (!string.IsNullOrEmpty(targetFunction))
                                {
                                    sb.Append(targetFunction);
                                }
                                else
                                {
                                    sb.Append(text); //reserve original function name
                                }

                                if (useBrackets)
                                {
                                    ignoreCount += 2;
                                }
                            }
                            else
                            {
                                if (text.StartsWith(this.sourceDbInterpreter.QuotationLeftChar.ToString()) && text.EndsWith(this.sourceDbInterpreter.QuotationRightChar.ToString()))
                                {
                                    sb.Append(this.GetQuotedString(text.Trim(this.sourceDbInterpreter.QuotationLeftChar, this.sourceDbInterpreter.QuotationRightChar)));
                                }
                                else
                                {
                                    sb.Append(text);
                                }
                            }
                        }
                        else
                        {
                            if ((sourceDataTypeSpecs != null && sourceDataTypeSpecs.Any(item => item.Name == text))
                                || (targetDataTypeSpecs != null && targetDataTypeSpecs.Any(item => item.Name == text)))
                            {
                                sb.Append(text);
                            }
                            else
                            {
                                sb.Append(this.GetQuotedString(text));
                            }
                        }
                        break;
                    case TSQLTokenType.StringLiteral:
                        if (previousType != TSQLTokenType.Whitespace && previousText.ToLower() == "as")
                        {
                            sb.Append(this.GetQuotedString(text));
                        }
                        else
                        {
                            sb.Append(text);
                        }
                        break;
                    case TSQLTokenType.SingleLineComment:
                    case TSQLTokenType.MultilineComment:
                        continue;
                    case TSQLTokenType.Keyword:
                        switch (text.ToUpper())
                        {
                            case "AS":
                                if (targetDbInterpreter is OracleInterpreter)
                                {
                                    var previousKeyword = (from t in tokens where t.Type == TSQLTokenType.Keyword && t.EndPosition < token.BeginPosition select t).LastOrDefault();
                                    if (previousKeyword != null && previousKeyword.Text.ToUpper() == "FROM")
                                    {
                                        continue;
                                    }
                                }
                                break;
                        }
                        sb.Append(text);
                        break;
                    default:
                        sb.Append(text);
                        break;
                }

                if (!string.IsNullOrWhiteSpace(text))
                {
                    previousText = text;
                    previousType = tokenType;
                }
            }

            return sb.ToString();
        }

        private string GetQuotedString(string text)
        {
            if (!text.StartsWith(this.targetDbInterpreter.QuotationLeftChar.ToString()) && !text.EndsWith(this.targetDbInterpreter.QuotationRightChar.ToString()))
            {
                return this.targetDbInterpreter.GetQuotedString(text.Trim('\'', '"', this.sourceDbInterpreter.QuotationLeftChar, this.sourceDbInterpreter.QuotationRightChar));
            }

            return text;
        }

        public List<TSQL.Tokens.TSQLToken> GetTokens(string sql)
        {
            return TSQLTokenizer.ParseTokens(sql, true, true);
        }
    }
}
