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
        private Regex nameRegex = new Regex(@"\b(^[_a-zA-Z][ _0-9a-zA-Z]+$)\b");
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
            List<TokenInfo> tokens = tokenExtracter.Extract();

            IEnumerable<string> keywords = KeywordManager.GetKeywords(this.TargetInterpreter.DatabaseType);
            IEnumerable<string> functions = FunctionManager.GetFunctionSpecifications(this.TargetInterpreter.DatabaseType).Select(item=>item.Name);

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
                    token.Symbol = $"{ this.GetQuotedName(tableName.Name.ToString(), token.Type)}" + (tableName.Alias == null ? "" : " " + tableName.Alias);
                }                
                else if (token is ColumnName columnName)
                {
                    string columnContent = "";

                    if (columnName.Name != null)
                    {
                        columnContent = $"{ this.GetQuotedName(columnName.Name.ToString().Trim('.').Trim(this.TrimChars), token.Type)}";
                        columnName.Name.Symbol = columnContent;
                    }

                    if (columnName.Alias != null && !string.IsNullOrEmpty(columnName.Alias.Symbol))
                    {
                        columnContent += $" AS {this.GetQuotedName(columnName.Alias.ToString(), token.Type)}";
                    }

                    token.Symbol = columnContent;
                }
                else if(token.Type == TokenType.TableName || token.Type == TokenType.ColumnName)
                {
                    token.Symbol = this.GetQuotedName(token.Symbol.Trim('.'), token.Type);
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
                    token.Symbol = this.GetQuotedString(token.Symbol);
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

                            string pattern = "";

                            bool strictMatch = true;

                            if (prefix.Length == 0)
                            {
                                strictMatch = false;
                                pattern = $@"[^`^""^\[]\b({kp.Key})\b[^`^""^\]]";
                            }
                            else
                            {
                                pattern = $@"([{prefix}]\b({kp.Key.Substring(prefix.Length)})\b)";
                            }

                            foreach (Match match in Regex.Matches(token.Symbol, pattern))
                            {
                                string matchValue = match.Value;

                                string replaceValue = strictMatch ? kp.Value : matchValue.First() + kp.Value + matchValue.Last();

                                token.Symbol = Regex.Replace(token.Symbol, pattern, replaceValue);
                            }
                        }
                    }
                }
                #endregion
            }

            #region Nested token handle
            if(this.nameWithQuotation)
            {
                var nestedTokens = tokens.Where(item => item.Symbol != null && item.Tokens.Count > 0);
               
                foreach (TokenInfo nestedToken in nestedTokens)
                {
                    List<string> replacedSymbols = new List<string>();

                    foreach (var token in nestedToken.Tokens)
                    {
                        string trimedSymbol = token.Symbol.Trim(this.TrimChars);

                        if(replacedSymbols.Contains(trimedSymbol))
                        {
                            continue;
                        }

                        Regex matchRegex = new Regex($@"[{this.SourceInterpreter.QuotationLeftChar}]?\b({trimedSymbol})\b[{this.SourceInterpreter.QuotationRightChar}]?");
                        string quotedValue = $"{this.TargetInterpreter.QuotationLeftChar}{trimedSymbol}{this.TargetInterpreter.QuotationRightChar}";

                        if (matchRegex.IsMatch(nestedToken.Symbol) && !nestedToken.Symbol.Contains(quotedValue))
                        {
                            nestedToken.Symbol = matchRegex.Replace(nestedToken.Symbol, token.Symbol);

                            replacedSymbols.Add(trimedSymbol);
                        }
                    }
                }
            }           
            #endregion

            this.Script.Owner = null;

            this.Script.Name.Symbol = this.TargetInterpreter.GetQuotedString(this.DbObject.Name);
        }

        private string GetQuotedName(string name, TokenType tokenType)
        {
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

        private void ReplaceTokens(List<TokenInfo> tokens)
        {
            Action<TokenInfo> changeValue = (token) =>
             {
                 token.Symbol = this.DbObject.Definition.Substring(token.StartIndex.Value, token.Length);

                 if(this.TargetInterpreter.DatabaseType!= DatabaseType.SqlServer && this.TargetDbOwner!= "dbo" && token.Symbol.ToLower().Contains("dbo."))
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
                column.DataType = this.columnTranslator.GetTrimedDataType(dataType);

                bool isChar = DataTypeHelper.IsCharType(dataType);

                int startIndex = dataType.IndexOf("(");
                int endIndex = dataType.IndexOf(")");

                string dataLength = dataType.Substring(startIndex + 1, endIndex - startIndex - 1);

                string[] lengthItems = dataLength.Split(',');

                int length;

                if (!int.TryParse(lengthItems[0], out length))
                {
                    length = -1;
                }

                if (isChar)
                {
                    column.MaxLength = length;

                    if (length != -1)
                    {
                        if (dataType.ToLower().StartsWith("n"))
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
