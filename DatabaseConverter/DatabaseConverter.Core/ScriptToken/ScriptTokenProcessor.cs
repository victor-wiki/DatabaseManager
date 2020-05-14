using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class ScriptTokenProcessor
    {
        private ColumnTranslator columnTranslator;
        private Regex identifierRegex = new Regex($@"([`""\[][ _0-9a-zA-Z]+[`""\]])");
        private Regex nameRegex = new Regex(@"\b(^[_a-zA-Z][ _0-9a-zA-Z]*$)\b");
        private bool removeDbOwner => this.TargetInterpreter.DatabaseType != DatabaseType.SqlServer;
        private bool nameWithQuotation = SettingManager.Setting.DbObjectNameMode == DbObjectNameMode.WithQuotation;
        public CommonScript Script { get; set; }
        public ScriptDbObject DbObject { get; set; }
        public DbInterpreter SourceInterpreter { get; set; }
        public DbInterpreter TargetInterpreter { get; set; }
        public List<UserDefinedType> UserDefinedTypes { get; set; } = new List<UserDefinedType>();
        public string TargetDbOwner { get; set; }
        public Dictionary<string, string> ReplacedVariables { get; private set; } = new Dictionary<string, string>();

        public char[] TrimChars
        {
            get
            {
                return new char[] { '"', this.SourceInterpreter.QuotationLeftChar, this.SourceInterpreter.QuotationRightChar, this.TargetInterpreter.QuotationLeftChar, this.TargetInterpreter.QuotationRightChar };
            }
        }

        public ScriptTokenProcessor(CommonScript script, ScriptDbObject dbObject, DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter)
        {
            this.Script = script;
            this.DbObject = dbObject;
            this.SourceInterpreter = sourceInterpreter;
            this.TargetInterpreter = targetInterpreter;
        }

        public void Process()
        {
            this.ProcessTokens();
        }

        private void ProcessTokens()
        {
            ScriptTokenExtracter tokenExtracter = new ScriptTokenExtracter(this.Script);
            IEnumerable<TokenInfo> tokens = tokenExtracter.Extract();

            IEnumerable<string> keywords = KeywordManager.GetKeywords(this.TargetInterpreter.DatabaseType);
            IEnumerable<string> functions = FunctionManager.GetFunctionSpecifications(this.TargetInterpreter.DatabaseType).Select(item => item.Name);

            this.columnTranslator = new ColumnTranslator(this.SourceInterpreter, this.TargetInterpreter, null);
            columnTranslator.LoadMappings();

            Func<TokenInfo, bool> changeValue = (token) =>
            {
                string oldSymbol = token.Symbol;

                bool hasChanged = false;

                if (this.TargetInterpreter.DatabaseType == DatabaseType.SqlServer)
                {
                    token.Symbol = "@" + token.Symbol.TrimStart('@');
                    hasChanged = true;

                    if (!this.ReplacedVariables.ContainsKey(oldSymbol))
                    {
                        this.ReplacedVariables.Add(oldSymbol, token.Symbol);
                    }
                }
                else if (this.SourceInterpreter.DatabaseType == DatabaseType.SqlServer)
                {
                    token.Symbol = token.Symbol.TrimStart('@');

                    if (keywords.Contains(token.Symbol.ToUpper()) || functions.Contains(token.Symbol.ToUpper()))
                    {
                        token.Symbol = "_" + token.Symbol;
                    }

                    hasChanged = true;

                    if (!this.ReplacedVariables.ContainsKey(oldSymbol))
                    {
                        this.ReplacedVariables.Add(oldSymbol, token.Symbol);
                    }
                }

                return hasChanged;
            };

            this.ReplaceTokens(tokens);

            foreach (TokenInfo token in tokens.Where(item => item != null && (item.Type == TokenType.ParameterName || item.Type == TokenType.VariableName)))
            {
                changeValue(token);
            }

            IEnumerable<string> aliases = tokens.Where(item => item.Symbol != null && item.Type == TokenType.Alias).Select(item => item.Symbol);

            foreach (TokenInfo token in tokens)
            {
                if (token.Symbol == null)
                {
                    continue;
                }

                if (token.Type == TokenType.DataType)
                {
                    TableColumn tableColumn = this.CreateTableColumn(token.Symbol);

                    columnTranslator.ConvertDataType(tableColumn);

                    token.Symbol = this.TargetInterpreter.ParseDataType(tableColumn);
                }
                else if (token is TableName tableName)
                {
                    if (tableName.Name != null && tableName.Name.Symbol != null)
                    {
                        string alias = tableName.Alias == null ? "" : tableName.Alias.ToString();

                        token.Symbol = $"{ this.GetQuotedName(tableName.Name.ToString(), token.Type)}" + (string.IsNullOrEmpty(alias) ? "" : " " + alias);
                    }
                }
                else if (token is ColumnName columnName)
                {
                    string columnContent = "";

                    if (columnName.Name != null)
                    {
                        string strTableName = "";

                        if (columnName.TableName != null)
                        {
                            strTableName = (!aliases.Contains(columnName.TableName.Symbol) ?
                                           this.GetQuotedName(columnName.TableName.ToString().Trim('.'), token.Type) :
                                           columnName.TableName.Symbol)
                                           + ".";

                        }

                        string strColName = this.GetQuotedName(columnName.Name.ToString().Trim('.'), token.Type);

                        columnContent = $"{strTableName}{strColName}";

                        if (columnName.Alias != null && !string.IsNullOrEmpty(columnName.Alias.Symbol))
                        {
                            string alias = columnName.Alias.ToString();

                            columnContent += $" AS {this.GetQuotedName(alias, token.Type)}";
                        }

                        token.Symbol = columnContent;
                    }
                }
                else if (token.Type == TokenType.TableName || token.Type == TokenType.ColumnName)
                {
                    if (!aliases.Contains(token.Symbol))
                    {
                        token.Symbol = this.GetQuotedName(token.Symbol.Trim('.'), token.Type);
                    }
                }
                else if (token.Type == TokenType.RoutineName)
                {
                    token.Symbol = this.GetQuotedName(token.Symbol, token.Type);
                }
                else if (token.Type == TokenType.Condition ||
                        token.Type == TokenType.OrderBy ||
                        token.Type == TokenType.GroupBy
                       )
                {
                    if (token.Tokens.Count == 0)
                    {
                        token.Symbol = this.GetQuotedString(token.Symbol);
                    }
                }
                else if (token.Type == TokenType.Alias)
                {
                    if (token.Symbol != null && !token.Symbol.Contains(" "))
                    {
                        token.Symbol = token.Symbol.Trim(this.TrimChars);
                    }
                }

                #region Replace parameter and variable name
                if (token.Type != TokenType.ParameterName && token.Type != TokenType.VariableName)
                {
                    if (this.ReplacedVariables.ContainsKey(token.Symbol))
                    {
                        token.Symbol = this.ReplacedVariables[token.Symbol];
                    }
                    else if (this.ReplacedVariables.Any(item => token.Symbol.Contains(item.Key)))
                    {
                        foreach (var kp in this.ReplacedVariables)
                        {
                            string prefix = "";

                            foreach (var c in kp.Key)
                            {
                                if (!Regex.IsMatch(c.ToString(), "[_a-zA-Z]"))
                                {
                                    prefix += c;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            string excludePattern = $@"[`""\[]\b({kp.Key})\b[`""\]]";
                            string pattern = "";

                            if (prefix.Length == 0)
                            {
                                pattern = $@"\b({kp.Key})\b";
                            }
                            else
                            {
                                pattern = $@"([{prefix}]\b({kp.Key.Substring(prefix.Length)})\b)";
                            }

                            if (!Regex.IsMatch(token.Symbol, excludePattern))
                            {
                                foreach (Match match in Regex.Matches(token.Symbol, pattern))
                                {
                                    token.Symbol = Regex.Replace(token.Symbol, pattern, kp.Value);
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            #region Nested token handle
            if (this.nameWithQuotation)
            {
                var nestedTokens = tokens.Where(item => this.IsNestedToken(item));

                foreach (TokenInfo nestedToken in nestedTokens)
                {
                    List<string> replacedSymbols = new List<string>();

                    var childTokens = this.GetNestedTokenChildren(nestedToken);

                    foreach (var token in childTokens)
                    {
                        if (token.Symbol == null)
                        {
                            continue;
                        }

                        string[] items = token.Symbol.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string item in items)
                        {
                            string trimedItem = item.Trim(this.TrimChars);

                            if (aliases.Contains(trimedItem))
                            {
                                continue;
                            }

                            if (nameRegex.IsMatch(trimedItem))
                            {
                                Regex doubleQuotationRegex = new Regex($@"[""]\b({trimedItem})\b[""]");
                                Regex matchRegex = new Regex($@"[{this.SourceInterpreter.QuotationLeftChar}]?\b({trimedItem})\b[{this.SourceInterpreter.QuotationRightChar}]?");

                                string quotedValue = $"{this.TargetInterpreter.QuotationLeftChar}{trimedItem}{this.TargetInterpreter.QuotationRightChar}";

                                if (!nestedToken.Symbol.Contains(quotedValue))
                                {
                                    bool doubleQuotationMatched = doubleQuotationRegex.IsMatch(nestedToken.Symbol);

                                    if (doubleQuotationMatched)
                                    {
                                        nestedToken.Symbol = doubleQuotationRegex.Replace(nestedToken.Symbol, trimedItem);
                                    }

                                    bool matched = matchRegex.IsMatch(nestedToken.Symbol);

                                    if (matched)
                                    {
                                        nestedToken.Symbol = matchRegex.Replace(nestedToken.Symbol, quotedValue);

                                        replacedSymbols.Add(token.Symbol);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            this.Script.Owner = null;

            this.Script.Name.Symbol = this.TargetInterpreter.GetQuotedString(this.DbObject.Name);
        }

        private bool IsNestedToken(TokenInfo token)
        {
            if (string.IsNullOrEmpty(token.Symbol))
            {
                return false;
            }

            if (token.Tokens.Count > 0)
            {
                return true;
            }

            if (token is TableName tableName)
            {
                if (tableName.Name != null && tableName.Symbol != null && tableName.Name.Length < tableName.Length)
                {
                    return true;
                }
            }

            if (token is ColumnName columnName)
            {
                if ((columnName.Name != null && columnName.Symbol != null && columnName.Name.Length < columnName.Length)
                    || (columnName.TableName != null && columnName.TableName.Symbol != null && columnName.TableName.Length < columnName.Length))
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<TokenInfo> GetNestedTokenChildren(TokenInfo token)
        {
            List<TokenInfo> tokens = new List<TokenInfo>();

            if (token is TableName tableName)
            {
                if (tableName.Name?.Symbol != null)
                {
                    tokens.Add(tableName.Name);
                }
            }
            else if (token is ColumnName columnName)
            {
                if (columnName.Name != null)
                {
                    tokens.Add(columnName.Name);
                }

                if (columnName.TableName != null)
                {
                    tokens.Add(columnName.TableName);
                }
            }

            if (token.Tokens.Count > 0)
            {
                tokens.AddRange(token.Tokens.Where(item => item.Symbol != null));
            }

            return tokens;
        }

        private string GetQuotedName(string name, TokenType tokenType)
        {
            if (!this.nameWithQuotation)
            {
                return name;
            }

            name = this.GetQuotedString(name);

            List<string> items = name.Split('.').ToList();

            for (int i = 0; i < items.Count - 1; i++)
            {
                if ((tokenType == TokenType.TableName || tokenType == TokenType.RoutineName))
                {
                    if (i == items.Count - 2)
                    {
                        items[i] = this.TargetDbOwner;
                    }
                    else if (i < items.Count - 2)
                    {
                        items[i] = "";
                    }
                }
            }

            string lastItem = items[items.Count - 1];

            if (nameRegex.IsMatch(lastItem))
            {
                items[items.Count - 1] = this.TargetInterpreter.GetQuotedString(items[items.Count - 1].Trim(this.TrimChars));
            }

            return string.Join(".", items.Where(item => !string.IsNullOrEmpty(item)));
        }

        private string GetQuotedString(string value)
        {
            if (!this.nameWithQuotation)
            {
                return value;
            }

            var matches = identifierRegex.Matches(value);

            foreach (Match match in matches)
            {
                value = this.ReplaceValue(value, match.Value, this.TargetInterpreter.GetQuotedString(match.Value.Trim(this.TrimChars)));
            }

            return value;
        }

        private string GetTrimedName(string name, DbInterpreter dbInterpreter)
        {
            return name.Trim(dbInterpreter.QuotationLeftChar, dbInterpreter.QuotationRightChar);
        }

        private string GetOriginalValue(TokenInfo token)
        {
            return this.DbObject.Definition.Substring(token.StartIndex.Value, token.Length);
        }

        private void ReplaceTokens(IEnumerable<TokenInfo> tokens)
        {
            Action<TokenInfo> changeValue = (token) =>
             {
                 token.Symbol = this.DbObject.Definition.Substring(token.StartIndex.Value, token.Length);

                 if (this.TargetInterpreter.DatabaseType != DatabaseType.SqlServer && this.TargetDbOwner != "dbo" && token.Symbol.ToLower().Contains("dbo."))
                 {
                     token.Symbol = this.ReplaceValue(token.Symbol, "dbo.", "");
                 }
             };

            this.Script.Functions.ForEach(item => { if (item != null) { changeValue(item); } });

            this.TranslateFunctions();

            foreach (var token in tokens)
            {
                if (token.Symbol == null)
                {
                    continue;
                }

                if (token.StartIndex != null || token.StopIndex != null)
                {
                    changeValue(token);

                    foreach (TokenInfo function in this.Script.Functions)
                    {
                        if (function.StartIndex >= token.StartIndex && function.StopIndex <= token.StopIndex)
                        {
                            function.Symbol = this.GetQuotedString(function.Symbol);

                            string oldValue = this.GetOriginalValue(function);

                            token.Symbol = this.ReplaceValue(token.Symbol, oldValue, function.Symbol);
                        }
                    }
                }
            }
        }

        private string ReplaceValue(string source, string oldValue, string newValue)
        {
            return Regex.Replace(source, Regex.Escape(oldValue), newValue, RegexOptions.IgnoreCase);
        }

        private void TranslateFunctions()
        {
            FunctionTranslator functionTranslator = new FunctionTranslator(this.SourceInterpreter, this.TargetInterpreter, this.Script.Functions);

            functionTranslator.Translate();
        }

        private TableColumn CreateTableColumn(string dataType)
        {
            TableColumn column = new TableColumn();

            if (dataType.IndexOf("(") > 0)
            {
                DataTypeInfo dataTypeInfo = DataTypeHelper.GetDataTypeInfo(this.SourceInterpreter, dataType);
                column.DataType = dataTypeInfo.DataType;

                bool isChar = DataTypeHelper.IsCharType(dataType);

                string dataLength = dataTypeInfo.Args;

                string[] lengthItems = dataLength.Split(',');

                int length;

                if (!int.TryParse(lengthItems[0], out length))
                {
                    length = -1;
                }

                if (isChar || DataTypeHelper.IsBinaryType(dataType))
                {
                    column.MaxLength = length;

                    if (isChar && length != -1)
                    {
                        if (DataTypeHelper.StartWithN(dataType))
                        {
                            column.MaxLength = length * 2;
                        }
                    }
                }
                else if (length > 0)
                {
                    column.Precision = length;

                    if (lengthItems.Length > 1)
                    {
                        column.Scale = int.Parse(lengthItems[1]);
                    }
                }
            }
            else
            {
                UserDefinedType userDefinedType = this.UserDefinedTypes.FirstOrDefault(item => item.Name.Trim(this.TrimChars).ToUpper() == dataType.Trim(this.TrimChars).ToUpper());

                if (userDefinedType != null)
                {
                    column.DataType = userDefinedType.Type;
                    column.MaxLength = userDefinedType.MaxLength;
                }
                else
                {
                    column.DataType = dataType;
                }
            }

            return column;
        }
    }
}
