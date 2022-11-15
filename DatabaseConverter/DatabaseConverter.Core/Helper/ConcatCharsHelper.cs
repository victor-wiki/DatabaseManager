using Antlr.Runtime.Tree;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class ConcatCharsHelper
    {
        public static bool HasConcatChars(string symbol, string concatChars, bool hasCharColumn = false)
        {
            if (!string.IsNullOrEmpty(symbol) && !string.IsNullOrEmpty(concatChars))
            {
                string[] items = symbol.Split(concatChars);

                return symbol.Contains(concatChars)
                    && (hasCharColumn || items.Any(item => item.Trim().StartsWith('\'') || item.Trim().EndsWith('\'') || DataTypeHelper.IsCharType(item))
                    && !items.Any(item => decimal.TryParse(item.Trim().Trim(','), out _)));
            }

            return false;
        }

        public static string ConvertConcatChars(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, string symbol, bool hasCharColumn = false)
        {
            string sourceConcatChars = sourceDbInterpreter.STR_CONCAT_CHARS;
            string targetConcatChars = targetDbInterpreter.STR_CONCAT_CHARS;

            if (HasConcatChars(symbol, sourceConcatChars, hasCharColumn))
            {
                if (!string.IsNullOrEmpty(targetConcatChars))
                {
                    return ConvertConcatChars(symbol, sourceConcatChars, targetConcatChars);
                }
                else
                {
                    IEnumerable<string> keywords = KeywordManager.GetKeywords(sourceDbInterpreter.DatabaseType);
                    var quotationChars = TranslateHelper.GetTrimChars(sourceDbInterpreter, targetDbInterpreter);

                    string wrappedSymbol = FormatSymbolWithSpace(symbol, sourceConcatChars);

                    string[] items = SplitToArray(wrappedSymbol, ' ');

                    int singleQuotationCharCount = 0;

                    Func<string, bool> isMatched = (value) =>
                    {
                        if (!keywords.Contains(value.ToUpper()) &&
                        (ValueHelper.IsStringValue(value.Trim()) 
                        || value == sourceConcatChars                         
                        || value.StartsWith("@"))
                        || Regex.IsMatch(value.Trim('(', ')'), RegexHelper.NameRegexPattern)
                        || (value.Contains(".") && value.Split(".").All(item=> Regex.IsMatch(ReplaceQuotationChars(item ,quotationChars), RegexHelper.NameRegexPattern)))
                        )
                        {
                            return true;
                        }

                        return false;
                    };

                    List<TokenSymbolItemGroupInfo> groups = new List<TokenSymbolItemGroupInfo>();

                    Action<TokenSymbolItemGroupInfo, int, string, bool> addGroupItem = (group, index, value, isLeftSide) =>
                    {
                        TokenSymbolItemInfo tsi = new TokenSymbolItemInfo() { Index = index, Type = TokenSymbolItemType.Combination };

                        tsi.Children.Add(new TokenSymbolItemInfo() { Index = index, Content = value });

                        if (isLeftSide)
                        {
                            group.LeftSideItems.Add(tsi);
                        }
                        else
                        {
                            group.RightSideItems.Add(tsi);
                        }
                    };

                    for (int i = 0; i < items.Length; i++)
                    {
                        singleQuotationCharCount = string.Join("", items.Take(i)).Count(t => t == '\'');

                        string item = items[i];

                        if (singleQuotationCharCount % 2 == 0)
                        {
                            if (item.Trim() == sourceConcatChars)
                            {
                                TokenSymbolItemGroupInfo group = new TokenSymbolItemGroupInfo() { GroupId = i };

                                groups.Add(group);

                                #region backward
                                StringBuilder sbBackward = new StringBuilder();
                                bool needBackwardParenthesis = false;
                                bool needBackwardSingleQuotationChar = false;

                                for (int j = i - 1; j >= 0; j--)
                                {
                                    string value = items[j];

                                    if (value.Trim() == "," && !needBackwardParenthesis && !needBackwardSingleQuotationChar)
                                    {
                                        break;
                                    }

                                    if (needBackwardParenthesis)
                                    {
                                        sbBackward.Insert(0, value);

                                        group.LeftSideItems.Last().Children.Insert(0, new TokenSymbolItemInfo() { Index = j, Content = value });

                                        if (StringHelper.IsParenthesisBalanced(sbBackward.ToString()))
                                        {
                                            needBackwardParenthesis = false;
                                        }

                                        continue;
                                    }
                                    else if (needBackwardSingleQuotationChar)
                                    {
                                        sbBackward.Insert(0, value);

                                        group.LeftSideItems.Last().Children.Add(new TokenSymbolItemInfo() { Index = j, Content = value });

                                        if (ValueHelper.IsStringValue(sbBackward.ToString().Trim()))
                                        {
                                            needBackwardSingleQuotationChar = false;
                                        }

                                        continue;
                                    }

                                    if ((value.Contains(")") && !value.Trim().EndsWith(",")))
                                    {
                                        sbBackward.Insert(0, value);

                                        addGroupItem(group, j, value, true);

                                        if (!StringHelper.IsParenthesisBalanced(sbBackward.ToString()))
                                        {
                                            needBackwardParenthesis = true;
                                        }
                                    }
                                    else if (StringHelper.IsEndWithSingleQuotationChar(value.Trim()) && !StringHelper.IsStartWithSingleQuotationChar(value.Trim()))
                                    {
                                        sbBackward.Insert(0, value);

                                        addGroupItem(group, j, value, true);

                                        needBackwardSingleQuotationChar = true;
                                    }
                                    else if (StringHelper.IsEndWithSingleQuotationChar(value) || isMatched(value))
                                    {
                                        group.LeftSideItems.Add(new TokenSymbolItemInfo() { Index = j, Content = value });
                                    }
                                    else if (value == "\n" || value.Length == 0)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                #endregion

                                #region forward
                                StringBuilder sbForward = new StringBuilder();
                                bool needForwardParenthesis = false;
                                bool needFowardSingleQuotationChar = false;

                                for (int k = i + 1; k < items.Length; k++)
                                {
                                    string value = items[k];

                                    if (value.Trim() == "," && !needForwardParenthesis && !needFowardSingleQuotationChar)
                                    {
                                        break;
                                    }

                                    if (needForwardParenthesis)
                                    {
                                        sbForward.Append(value);

                                        group.RightSideItems.Last().Children.Add(new TokenSymbolItemInfo() { Index = k, Content = value });

                                        if (StringHelper.IsParenthesisBalanced(sbForward.ToString()))
                                        {
                                            needForwardParenthesis = false;
                                        }

                                        if (StringHelper.IsParenthesisBalanced(sbForward.ToString()))
                                        {
                                            needForwardParenthesis = false;
                                            i = k;
                                        }

                                        continue;
                                    }
                                    else if (needFowardSingleQuotationChar)
                                    {
                                        sbForward.Append(value);

                                        group.RightSideItems.Last().Children.Add(new TokenSymbolItemInfo() { Index = k, Content = value });

                                        if (ValueHelper.IsStringValue(sbForward.ToString().Trim()))
                                        {
                                            needFowardSingleQuotationChar = false;
                                            i = k;
                                        }

                                        continue;
                                    }

                                    if ((value.Contains("(") && !value.Trim().StartsWith(",")))
                                    {
                                        sbForward.Append(value);

                                        addGroupItem(group, k, value, false);

                                        if (!StringHelper.IsParenthesisBalanced(sbForward.ToString()))
                                        {
                                            needForwardParenthesis = true;
                                        }
                                    }
                                    else if (StringHelper.IsStartWithSingleQuotationChar(value.Trim()) && !StringHelper.IsEndWithSingleQuotationChar(value.Trim()))
                                    {
                                        sbForward.Append(value);

                                        addGroupItem(group, k, value, false);

                                        needFowardSingleQuotationChar = true;
                                    }                                    
                                    else if (value == "\n" || value.Length == 0)
                                    {
                                        continue;
                                    }
                                    else if (value == sourceConcatChars && (group.RightSideItems.Count == 0 || group.RightSideItems.LastOrDefault()?.Content == sourceConcatChars))
                                    {
                                        continue;
                                    }
                                    else if (StringHelper.IsStartWithSingleQuotationChar(value) || isMatched(value))
                                    {
                                        group.RightSideItems.Add(new TokenSymbolItemInfo() { Index = k, Content = value });

                                        i = k;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                #endregion
                            }
                        }
                    }

                    StringBuilder sb = new StringBuilder();

                    Action<TokenSymbolItemInfo> appendGroupItem = (gi) =>
                    {
                        if (gi.Children.Count == 0)
                        {
                            sb.Append(gi.Content);
                        }
                        else
                        {
                            sb.Append(String.Join(" ", gi.Children.OrderBy(item => item.Index).Select(item => item.Content)));
                        }
                    };

                    for (int i = 0; i < items.Length; i++)
                    {
                        string item = items[i];

                        if (IsTokenSymbolGroupItem(groups, i))
                        {
                            var group = FindSymbolItemGroupByIndex(groups, i);

                            TokenSymbolItemInfo firstItem = GetFirstTokenSymbolItem(group.LeftSideItems);
                            int backwardParenthesisCount = GetParenthesisCount(group.LeftSideItems, '(');
                            int forwardParenthesisCount = GetParenthesisCount(group.RightSideItems, ')');

                            bool isFirstItemEndWithParenthesis = firstItem != null && firstItem.Content.EndsWith("(");

                            string concat = "CONCAT(";

                            if (isFirstItemEndWithParenthesis && backwardParenthesisCount == forwardParenthesisCount)
                            {
                                //exchange 

                                sb.Append(firstItem.Content);

                                firstItem.Content = concat;
                            }
                            else
                            {
                                sb.Append(concat);
                            }

                            int j = 0;

                            foreach (var gi in group.LeftSideItems.OrderBy(item => item.Index))
                            {
                                appendGroupItem(gi);

                                if (j < group.LeftSideItems.Count - 1)
                                {
                                    sb.Append(" ");
                                }

                                j++;
                            }

                            sb.Append(",");

                            int k = 0;

                            foreach (var gi in group.RightSideItems)
                            {
                                if (gi.Content == sourceConcatChars)
                                {
                                    sb.Append(",");
                                }
                                else
                                {
                                    appendGroupItem(gi);
                                }

                                if (k < group.RightSideItems.Count - 1)
                                {
                                    sb.Append(" ");
                                }

                                k++;
                            }

                            sb.Append(") ");

                            if (group.RightSideItems.Any())
                            {
                                i = GetMaxIndexOfSymbolGroup(group);
                            }
                        }
                        else
                        {
                            sb.Append(item + " ");
                        }
                    }

                    return sb.ToString().Trim();
                }
            }

            return symbol;
        }

        private static string ReplaceQuotationChars(string value, IEnumerable<char> quotationChars)
        {
            foreach(var c in quotationChars)
            {
                value = value.Replace(c.ToString(), "");
            }

            return value;
        }

        private static TokenSymbolItemInfo GetFirstTokenSymbolItem(List<TokenSymbolItemInfo> items)
        {
            if (items.Count > 0)
            {
                var item = items.OrderBy(item => item.Index).First();

                if (item.Children.Count == 0)
                {
                    return item;
                }
                else
                {
                    return item.Children.OrderBy(item => item.Index).First();
                }
            }

            return null;
        }

        private static int GetParenthesisCount(List<TokenSymbolItemInfo> items, char parenthesis)
        {
            int count = 0;

            foreach (var item in items)
            {
                if (item.Children.Count == 0)
                {
                    count += item.Content.Count(t => t == parenthesis);
                }
                else
                {
                    count += item.Children.Sum(t => t.Content.Count(c => c == parenthesis));
                }
            }

            return count;
        }

        private static int GetMaxIndexOfSymbolGroup(TokenSymbolItemGroupInfo group)
        {
            int index = 0;

            foreach (var gi in group.RightSideItems)
            {
                if (gi.Children.Count == 0)
                {
                    if (gi.Index > index)
                    {
                        index = gi.Index;
                    }
                }
                else
                {
                    int maxIndex = gi.Children.Max(item => item.Index);

                    if (maxIndex > index)
                    {
                        index = maxIndex;
                    }
                }
            }

            return index;
        }

        private static bool IsTokenSymbolGroupItem(List<TokenSymbolItemGroupInfo> groups, int index)
        {
            foreach (var group in groups)
            {
                if (IsTokenSymbolGroupItem(group.LeftSideItems, index))
                {
                    return true;
                }

                if (IsTokenSymbolGroupItem(group.RightSideItems, index))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTokenSymbolGroupItem(List<TokenSymbolItemInfo> items, int index)
        {
            if (items.Any(item => item.Index == index || (item.Children.Count > 0 && item.Children.Any(t => t.Index == index))))
            {
                return true;
            }

            return false;
        }

        private static TokenSymbolItemGroupInfo FindSymbolItemGroupByIndex(List<TokenSymbolItemGroupInfo> groups, int index)
        {
            foreach (var group in groups)
            {
                if (IsTokenSymbolGroupItem(group.LeftSideItems, index))
                {
                    return group; ;
                }

                if (IsTokenSymbolGroupItem(group.RightSideItems, index))
                {
                    return group;
                }
            }

            return null;
        }

        private static string FormatSymbolWithSpace(string value, string sourceConcatChars)
        {
            char concatFirstChar = sourceConcatChars.First();
            int concatCharsLength = sourceConcatChars.Length;
            int singleQuotationCharCount = 0;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (c == '\'')
                {
                    singleQuotationCharCount++;
                }

                if (c == concatFirstChar)
                {
                    string fowardChars = concatCharsLength == 1 ? sourceConcatChars : (i + concatCharsLength <= value.Length - 1) ? value.Substring(i, concatCharsLength)
                           : value.Substring(i);

                    if (fowardChars == sourceConcatChars)
                    {
                        if (singleQuotationCharCount % 2 == 0)
                        {
                            if (i > 0 && value[i - 1] != ' ')
                            {
                                sb.Append(" ");
                            }

                            sb.Append(sourceConcatChars);

                            i = i + concatCharsLength - 1;

                            if (i < value.Length - 1 && value[i + 1] != ' ')
                            {
                                sb.Append(" ");
                            }

                            continue;
                        }
                    }
                }
                else if (c == ',' && singleQuotationCharCount % 2 == 0)
                {
                    if (i > 0 && value[i - 1] != ' ')
                    {
                        sb.Append(" ");
                    }

                    sb.Append(c);

                    if (i < value.Length - 1 && value[i + 1] != ' ')
                    {
                        sb.Append(" ");
                    }

                    continue;
                }
                else if (c == '(')
                {
                    if (i < value.Length - 1 && (value[i + 1] != '(' && value[i + 1] != ' '))
                    {
                        sb.Append(c);
                        sb.Append(" ");
                        continue;
                    }
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        private static string ConvertConcatChars(string value, string sourceConcatChars, string targetConcatChars)
        {
            char concatFirstChar = sourceConcatChars.First();
            int concatCharsLength = sourceConcatChars.Length;
            int singleQuotationCharCount = 0;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (c == '\'')
                {
                    singleQuotationCharCount++;
                }

                if (c == concatFirstChar)
                {
                    string fowardChars = concatCharsLength == 1 ? sourceConcatChars : (i + concatCharsLength <= value.Length - 1) ? value.Substring(i, concatCharsLength)
                           : value.Substring(i);

                    if (fowardChars == sourceConcatChars)
                    {
                        if (singleQuotationCharCount % 2 == 0)
                        {
                            if (sb.ToString().Trim().EndsWith(targetConcatChars))
                            {
                                continue;
                            }

                            sb.Append(targetConcatChars);

                            i = i + concatCharsLength - 1;

                            continue;
                        }
                    }
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        private static bool ContainsConcatChars(string value, string concatChars)
        {
            int index = -1;

            while ((index = value.IndexOf(concatChars)) > 0)
            {
                int singleQuotationCharCount = value.Substring(0, index).Count(item => item == '\'');

                if (singleQuotationCharCount % 2 == 0)
                {
                    return true;
                }
                else
                {
                    int nextIndex = index + concatChars.Length + 1;

                    value = nextIndex < value.Length ? value.Substring(nextIndex) : "";
                }
            }

            return false;
        }

        public static string[] SplitToArray(string value, char separator)
        {
            int singleQuotationCharCount = 0;

            List<string> list = new List<string>();

            int lastIndex = -1;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (c == '\'')
                {
                    singleQuotationCharCount++;
                }

                if (c == separator && singleQuotationCharCount % 2 == 0)
                {
                    list.Add(value.Substring(lastIndex + 1, i - lastIndex - 1));

                    lastIndex = i;
                }
                else if (i == value.Length - 1 && lastIndex < value.Length - 1)
                {
                    list.Add(value.Substring(lastIndex + 1));
                }
            }

            return list.ToArray();
        }
    }
}
