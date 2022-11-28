using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Newtonsoft.Json.Linq;
using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class ConcatCharsHelper
    {
        public const string KEYWORDS = "AND|NOT|NULL|IS|CASE|WHEN|THEN|ELSE|END|LIKE|FOR|IN|OR";

        public static string ConvertConcatChars(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, string symbol, IEnumerable<string> charItems = null)
        {
            string sourceConcatChars = sourceDbInterpreter.STR_CONCAT_CHARS;
            string targetConcatChars = targetDbInterpreter.STR_CONCAT_CHARS;

            if (!symbol.Contains(sourceConcatChars))
            {
                return symbol;
            }

            var quotationChars = TranslateHelper.GetTrimChars(sourceDbInterpreter, targetDbInterpreter).ToArray();
            bool hasParenthesis = symbol.Trim().StartsWith("(");

            DatabaseType targetDbType = targetDbInterpreter.DatabaseType;

            var items = SplitByKeywords(StringHelper.GetBalanceParenthesisTrimedValue(symbol));

            StringBuilder sb = new StringBuilder();

            foreach (var item in items)
            {
                if (item.Index > 0)
                {
                    sb.Append(" ");
                }

                if (item.Type == TokenSymbolItemType.Keyword || item.Content.Trim().Length == 0)
                {
                    sb.Append(item.Content);
                }
                else
                {
                    string res = InternalConvertConcatChars(sourceDbInterpreter, targetDbInterpreter, item.Content, charItems);

                    sb.Append(res);
                }
            }

            string result = sb.ToString();

            if (StringHelper.IsParenthesisBalanced(result))
            {
                if (result.Contains(sourceConcatChars)) //check the final result
                {
                    sb.Clear();

                    if (!string.IsNullOrEmpty(targetConcatChars))
                    {
                        var symbolItems = SplitByConcatChars(StringHelper.GetBalanceParenthesisTrimedValue(result), sourceConcatChars);

                        for (int i = 0; i < symbolItems.Count; i++)
                        {
                            string content = symbolItems[i].Content;

                            sb.Append(content);

                            if (i < symbolItems.Count - 1)
                            {
                                string nextContent = (i + 1 <= symbolItems.Count - 1) ? symbolItems[i + 1].Content : null;

                                bool currentContentIsMatched = IsStringValueMatched(content, targetDbType, quotationChars, charItems);

                                bool nextContentIsMatched = nextContent != null && IsStringValueMatched(nextContent, targetDbType, quotationChars, charItems);

                                if (currentContentIsMatched || nextContentIsMatched)
                                {
                                    sb.Append(targetConcatChars);
                                }
                                else
                                {
                                    sb.Append(sourceConcatChars);
                                }
                            }
                        }
                    }
                    else
                    {
                        bool isAssignClause = TranslateHelper.IsAssignClause(result, quotationChars);
                        string strAssign = "";
                        string parseContent = result;

                        if (isAssignClause)
                        {
                            int equalMarkIndex = result.IndexOf("=");

                            strAssign = result.Substring(0, equalMarkIndex + 1);
                            parseContent = result.Substring(equalMarkIndex + 1); 
                        }

                        var symbolItems = SplitByConcatChars(StringHelper.GetBalanceParenthesisTrimedValue(parseContent), sourceConcatChars);

                        bool hasStringValue = false;

                        for (int i = 0; i < symbolItems.Count; i++)
                        {
                            string content = symbolItems[i].Content;

                            bool isMatched = IsStringValueMatched(content, targetDbType, quotationChars, charItems);

                            if (isMatched)
                            {
                                hasStringValue = true;
                            }
                            else
                            {
                                string upperContent = content.Trim('(', ')').Trim().ToUpper();

                                if (upperContent.StartsWith("CASE") && upperContent.EndsWith("END"))
                                {
                                    var subItems = SplitByConcatChars(upperContent, " ");

                                    hasStringValue = subItems.Any(item => IsStringValueMatched(item.Content, targetDbType, quotationChars, charItems));
                                }
                            }

                            if (hasStringValue)
                            {
                                break;
                            }
                        }

                        if (hasStringValue)
                        {
                            sb.Append($"{strAssign}CONCAT({string.Join(",", symbolItems.Select(item => item.Content))})");
                        }
                        else
                        {
                            sb.Append(result);
                        }
                    }

                    result = sb.ToString();
                }

                return GetOriginalValue(result, hasParenthesis);
            }

            return symbol;
        }

        private static string GetOriginalValue(string value, bool hasParenthesis)
        {
            if (!hasParenthesis)
            {
                return value;
            }
            else
            {
                if (!value.Trim().StartsWith("("))
                {
                    return $"({value})";
                }
            }

            return value;
        }

        private static string InternalConvertConcatChars(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, string symbol, IEnumerable<string> charItems = null)
        {
            string sourceConcatChars = sourceDbInterpreter.STR_CONCAT_CHARS;
            string targetConcatChars = targetDbInterpreter.STR_CONCAT_CHARS;

            if (!symbol.Contains(sourceConcatChars))
            {
                return symbol;
            }

            DatabaseType sourceDbType = sourceDbInterpreter.DatabaseType;
            DatabaseType targetDbType = targetDbInterpreter.DatabaseType;

            bool hasParenthesis = symbol.Trim().StartsWith("(");

            var quotationChars = TranslateHelper.GetTrimChars(sourceDbInterpreter, targetDbInterpreter).ToArray();

            var symbolItems = SplitByConcatChars(StringHelper.GetBalanceParenthesisTrimedValue(symbol), sourceConcatChars);

            if (symbolItems.Count == 1)
            {
                string content = symbolItems[0].Content;

                if (!content.Contains("("))
                {
                    return symbol;
                }
                else
                {
                    int equalMarkIndex = content.IndexOf("=");
                    int parenthesisIndex = content.IndexOf("(");

                    string assignName = "";
                    string functionName = "";

                    if (equalMarkIndex >= 0 && equalMarkIndex < parenthesisIndex)
                    {
                        assignName = content.Substring(0, equalMarkIndex);
                        functionName = content.Substring(equalMarkIndex + 1, parenthesisIndex - equalMarkIndex - 1);
                    }
                    else
                    {
                        functionName = content.Substring(0, parenthesisIndex);
                    }

                    var spec = FunctionManager.GetFunctionSpecifications(targetDbType).FirstOrDefault(item => item.Name.ToUpper() == functionName.Trim().ToUpper());

                    if (spec == null) //if no target function specification, use the source instead.
                    {
                        spec = FunctionManager.GetFunctionSpecifications(sourceDbType).FirstOrDefault(item => item.Name.ToUpper() == functionName.Trim().ToUpper());
                    }

                    if (spec != null)
                    {
                        var formular = new FunctionFormula(content);

                        if (formular != null)
                        {
                            var args = formular.GetArgs(spec.Delimiter ?? ",");

                            List<string> results = new List<string>();

                            foreach (var arg in args)
                            {
                                results.Add(ConvertConcatChars(sourceDbInterpreter, targetDbInterpreter, arg, charItems));
                            }

                            string delimiter = spec.Delimiter == "," ? "," : $" {spec.Delimiter} ";
                            string strAssign = !string.IsNullOrEmpty(assignName) ? assignName + "=" : "";

                            return $"{strAssign}{functionName}({string.Join(delimiter, results)})";
                        }
                    }

                    return GetOriginalValue(content, hasParenthesis);
                }
            }

            foreach (var item in symbolItems)
            {
                if (item.Content != symbol && item.Content.Contains("("))
                {
                    item.Content = ConvertConcatChars(sourceDbInterpreter, targetDbInterpreter, item.Content, charItems);
                }
            }

            if (sourceConcatChars == "+")
            {
                if (symbolItems.Any(item => decimal.TryParse(item.Content.Trim(',', ' '), out _)))
                {
                    return symbol;
                }
            }

            Func<string, bool> isMatched = (value) =>
            {
                return IsStringValueMatched(value, targetDbType, quotationChars, charItems);
            };

            bool hasStringValue = symbolItems.Any(item => isMatched(item.Content.Trim()));

            StringBuilder sb = new StringBuilder();

            if (hasStringValue)
            {
                var items = symbolItems.Select(item => item.Content).ToArray();

                if (!string.IsNullOrEmpty(targetConcatChars))
                {
                    sb.Append(string.Join(targetConcatChars, items));
                }
                else
                {
                    bool hasInvalid = false;

                    for (int i = 0; i < items.Length; i++)
                    {
                        string value = items[i];

                        if (isMatched(value.Trim()))
                        {
                            sb.Append(value);
                        }
                        else
                        {
                            if (value.StartsWith("@")
                              || Regex.IsMatch(value.Trim('(', ')', ' '), RegexHelper.NameRegexPattern)
                              || (value.Contains(".") && value.Split(".").All(item => Regex.IsMatch(item.Trim().Trim(quotationChars), RegexHelper.NameRegexPattern)))
                              || Regex.IsMatch(TranslateHelper.ExtractNameFromParenthesis(value.Trim()), RegexHelper.NameRegexPattern)
                              )
                            {
                                sb.Append(value);
                            }
                            else
                            {
                                hasInvalid = true;
                                break;
                            }
                        }

                        if (i < items.Length - 1)
                        {
                            sb.Append(",");
                        }
                    }

                    if (!hasInvalid)
                    {
                        sb.Insert(0, "CONCAT(");
                        sb.Append(")");
                    }
                    else
                    {
                        return symbol;
                    }
                }
            }

            if (symbol.Trim().EndsWith(sourceConcatChars))
            {
                if (!string.IsNullOrEmpty(targetConcatChars))
                {
                    sb.Append(targetConcatChars);
                }
                else
                {
                    sb.Append(sourceConcatChars);
                }
            }

            string result = sb.ToString().Trim();

            if (result.Length > 0 && StringHelper.IsParenthesisBalanced(result))
            {
                return GetOriginalValue(result, hasParenthesis);
            }

            return symbol;
        }

        private static bool IsStringValueMatched(string value, DatabaseType targetDbType, char[] quotationChars, IEnumerable<string> charItems = null)
        {
            return ValueHelper.IsStringValue(value.Trim('(', ')'))
                   || IsStringFunction(targetDbType, value)
                   || (charItems != null && charItems.Any(c => c.Trim(quotationChars) == value.Trim(quotationChars)));
        }

        private static List<TokenSymbolItemInfo> SplitByKeywords(string value)
        {
            string keywordsRegex = $@"\b({KEYWORDS})\b";

            var matches = Regex.Matches(value, keywordsRegex, RegexOptions.IgnoreCase);

            List<TokenSymbolItemInfo> tokenSymbolItems = new List<TokenSymbolItemInfo>();

            Action<string, TokenSymbolItemType> addItem = (content, type) =>
            {
                TokenSymbolItemInfo contentItem = new TokenSymbolItemInfo();
                contentItem.Index = tokenSymbolItems.Count();
                contentItem.Content = content;
                contentItem.Type = type;

                tokenSymbolItems.Add(contentItem);
            };

            List<Match> matchList = new List<Match>();

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];

                if (match.Index > 0)
                {
                    if ("@" + match.Value == value.Substring(match.Index - 1, match.Length + 1))
                    {
                        continue;
                    }

                    int singleQuotationCharCount = value.Substring(0, match.Index).Count(item => item == '\'');

                    if (singleQuotationCharCount % 2 != 0)
                    {
                        continue;
                    }
                }

                matchList.Add(match);
            }

            for (int i = 0; i < matchList.Count; i++)
            {
                var match = matchList[i];

                int index = match.Index;

                if (index > 0)
                {
                    if ("@" + match.Value == value.Substring(match.Index - 1, match.Length + 1))
                    {
                        continue;
                    }
                }

                int singleQuotaionCharCount = value.Substring(0, index).Count(item => item == '\'');

                if (singleQuotaionCharCount % 2 == 0)
                {
                    if (i == 0 && match.Index > 0)
                    {
                        addItem(value.Substring(0, match.Index), TokenSymbolItemType.Content);
                    }

                    addItem(match.Value, TokenSymbolItemType.Keyword);

                    if (i < matchList.Count - 1)
                    {
                        int nextMatchIndex = matchList[i + 1].Index;

                        string content = value.Substring(index + match.Length, nextMatchIndex - index - match.Length);

                        addItem(content, TokenSymbolItemType.Content);
                    }
                    else if (i == matchList.Count - 1)
                    {
                        int startIndex = match.Index + match.Length;

                        if (startIndex < value.Length)
                        {
                            string content = value.Substring(startIndex);

                            addItem(content, TokenSymbolItemType.Content);
                        }
                    }
                }
            }

            if (tokenSymbolItems.Count == 0)
            {
                addItem(value, TokenSymbolItemType.Content);
            }

            return tokenSymbolItems;
        }

        private static bool IsStringFunction(DatabaseType databaseType, string value)
        {
            string functionName = TranslateHelper.ExtractNameFromParenthesis(value.Trim()).ToUpper();
            FunctionSpecification specification = FunctionManager.GetFunctionSpecifications(databaseType).FirstOrDefault(item => item.Name.ToUpper() == functionName);

            if (specification != null)
            {
                if (specification.IsString)
                {
                    return true;
                }
                else
                {
                    return IsStringConvertFunction(databaseType, specification, value);
                }
            }

            return false;
        }

        private static bool IsStringConvertFunction(DatabaseType databaseType, FunctionSpecification specification, string value)
        {
            string name = specification.Name.Trim().ToUpper();

            if (name != "CONVERT" && name != "CAST")
            {
                return false;
            }

            if (!string.IsNullOrEmpty(specification.Args))
            {
                int index = value.IndexOf("(");

                string content = StringHelper.GetBalanceParenthesisTrimedValue(value.Substring(index));

                string[] items = content.Split(specification.Delimiter ?? ",");

                string dataType = null;

                if (name == "CONVERT")
                {
                    if (databaseType == DatabaseType.MySql)
                    {
                        dataType = items.LastOrDefault()?.Trim();
                    }
                    else
                    {
                        dataType = items.FirstOrDefault()?.Trim();
                    }
                }
                else if (name == "CAST")
                {
                    dataType = items.LastOrDefault()?.Trim();
                }

                if (dataType != null && DataTypeHelper.IsCharType(dataType))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<TokenSymbolItemInfo> SplitByConcatChars(string value, string concatChars)
        {
            List<TokenSymbolItemInfo> items = new List<TokenSymbolItemInfo>();

            char concatFirstChar = concatChars.First();
            int concatCharsLength = concatChars.Length;
            int singleQuotationCharCount = 0;
            int leftParenthesisCount = 0;
            int rightParenthesisCount = 0;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (c == '\'')
                {
                    singleQuotationCharCount++;
                }
                else if (c == '(')
                {
                    if (singleQuotationCharCount % 2 == 0)
                    {
                        leftParenthesisCount++;
                    }
                }
                else if (c == ')')
                {
                    if (singleQuotationCharCount % 2 == 0)
                    {
                        rightParenthesisCount++;
                    }
                }

                if (c == concatFirstChar)
                {
                    string fowardChars = concatCharsLength == 1 ? concatChars : (i + concatCharsLength <= value.Length - 1) ? value.Substring(i, concatCharsLength)
                    : value.Substring(i);

                    if (fowardChars == concatChars)
                    {
                        if (singleQuotationCharCount % 2 == 0 && (leftParenthesisCount == rightParenthesisCount))
                        {
                            TokenSymbolItemInfo item = new TokenSymbolItemInfo();
                            item.Content = sb.ToString();
                            item.Index = items.Count;

                            items.Add(item);

                            i = i + concatCharsLength - 1;

                            sb.Clear();

                            continue;
                        }
                    }
                }

                sb.Append(c);
            }

            if (sb.Length > 0) //last one
            {
                TokenSymbolItemInfo item = new TokenSymbolItemInfo();
                item.Content = sb.ToString();
                item.Index = items.Count;

                items.Add(item);
            }

            return items;
        }
    }
}
