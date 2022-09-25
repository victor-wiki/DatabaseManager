using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class ScriptTokenProcessor
    {
        private ColumnTranslator columnTranslator;
        private bool removeDbSchema => this.TargetInterpreter.DatabaseType != DatabaseType.SqlServer;
        public CommonScript Script { get; set; }
        public ScriptDbObject DbObject { get; set; }
        public DbInterpreter SourceInterpreter { get; set; }
        public DbInterpreter TargetInterpreter { get; set; }
        public List<UserDefinedType> UserDefinedTypes { get; set; } = new List<UserDefinedType>();
        public string TargetDbSchema { get; set; }

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
            IEnumerable<string> keywords = KeywordManager.GetKeywords(this.TargetInterpreter.DatabaseType);
            IEnumerable<string> functions = FunctionManager.GetFunctionSpecifications(this.TargetInterpreter.DatabaseType).Select(item => item.Name);

            this.columnTranslator = new ColumnTranslator(this.SourceInterpreter, this.TargetInterpreter, null);
            columnTranslator.LoadMappings();

            Dictionary<string, string> dictChangedValues = new Dictionary<string, string>();

            DatabaseType targeDbType = this.TargetInterpreter.DatabaseType;

            Action<TokenInfo> changeParaVarName = (pv) =>
            {
                this.RestoreValue(pv);

                string name = pv.Symbol;

                if (targeDbType != DatabaseType.SqlServer)
                {
                    if (keywords.Contains(name.ToUpper()) || functions.Contains(name.ToUpper()))
                    {
                        pv.Symbol = name + "_";
                    }
                    else
                    {
                        pv.Symbol = name.Replace("@", "");

                        if (pv.Parent != null)
                        {
                            pv.Parent.Symbol = pv.Parent.Symbol.Replace("@", "");
                        }
                    }
                }
                else
                {
                    pv.Symbol = "@" + pv.Symbol;
                }

                this.AddChangedValue(dictChangedValues, name, pv.Symbol);
            };

            Action<TokenInfo> changeDataType = (token) =>
            {
                if (token.Symbol != null)
                {
                    TableColumn tableColumn = this.CreateTableColumn(token.Symbol);

                    columnTranslator.ConvertDataType(tableColumn);

                    token.Symbol = this.TargetInterpreter.ParseDataType(tableColumn);
                }                
            };

            if (this.Script is RoutineScript routineScript)
            {
                var parameters = routineScript.Parameters;

                foreach (var parameter in parameters)
                {
                    changeParaVarName(parameter.Name);
                    changeDataType(parameter.DataType);
                }

                if (routineScript.ReturnDataType != null)
                {
                    changeDataType(routineScript.ReturnDataType);
                }
            }

            foreach (Statement statement in this.Script.Statements)
            {
                ScriptTokenExtracter tokenExtracter = new ScriptTokenExtracter(statement);
                IEnumerable<TokenInfo> tokens = tokenExtracter.Extract();

                this.ReplaceTokens(tokens);

                foreach (TokenInfo token in tokens)
                {
                    var tokenType = token.Type;

                    if (tokenType == TokenType.RoutineName
                        || tokenType == TokenType.TableName
                        || tokenType == TokenType.ColumnName
                        || tokenType == TokenType.Alias)
                    {
                        string oldSymbol = token.Symbol;

                        this.RemoveQuotationChar(token);

                        if (oldSymbol != token.Symbol && !string.IsNullOrEmpty(oldSymbol))
                        {
                            this.AddChangedValue(dictChangedValues, oldSymbol, token.Symbol);
                        }
                    }
                    else if (tokenType == TokenType.VariableName)
                    {
                        changeParaVarName(token);
                    }
                    else if (token.Type == TokenType.DataType)
                    {
                        changeDataType(token);
                    }
                    else
                    {
                        this.ReplaceTokenSymbol(dictChangedValues, token);

                        this.RemoveQuotationChar(token);
                    }
                }
            }
         
            this.Script.Name.Symbol = this.TargetInterpreter.GetQuotedString(this.DbObject.Name);
        }

        private bool IsNameContainsWihtespace(string name)
        {
            return name.Contains(" ");
        }

        private void RemoveQuotationChar(TokenInfo token)
        {
            string symbol = token.Symbol;

            if (symbol == null)
            {
                return;
            }

            if (!this.IsNameContainsWihtespace(symbol) && !symbol.Contains("."))
            {
                token.Symbol = this.GetTrimedName(token.Symbol);
            }
            else
            {
                if (this.SourceInterpreter.DatabaseType == DatabaseType.SqlServer)
                {
                    token.Symbol = token.Symbol.Replace("[", this.TargetInterpreter.QuotationLeftChar.ToString())
                                               .Replace("]", this.TargetInterpreter.QuotationRightChar.ToString())
                                               .Replace("\"", this.TargetInterpreter.QuotationLeftChar.ToString());

                }
                else if (this.TargetInterpreter.DatabaseType == DatabaseType.SqlServer)
                {
                    token.Symbol = token.Symbol.Replace(this.SourceInterpreter.QuotationLeftChar.ToString(), "\"")
                                               .Replace(this.SourceInterpreter.QuotationRightChar.ToString(), "\"");
                }
                else
                {
                    token.Symbol = token.Symbol.Replace(this.SourceInterpreter.QuotationLeftChar, this.TargetInterpreter.QuotationLeftChar)
                                               .Replace(this.SourceInterpreter.QuotationRightChar, this.TargetInterpreter.QuotationRightChar);
                }
            }
        }

        private void AddChangedValue(Dictionary<string, string> dict, string key, string value)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, value);
            }
        }

        private void ReplaceTokenSymbol(Dictionary<string, string> dict, TokenInfo tokenInfo)
        {
            foreach (var pv in dict)
            {
                tokenInfo.Symbol = tokenInfo.Symbol.Replace(pv.Key, pv.Value);
            }
        }


        private string GetTrimedName(string name)
        {
            return name?.Trim(this.SourceInterpreter.QuotationLeftChar, this.SourceInterpreter.QuotationRightChar);
        }

        private string GetOriginalValue(TokenInfo token)
        {
            return this.DbObject.Definition.Substring(token.StartIndex.Value, token.Length);
        }

        private void RestoreValue(TokenInfo token)
        {
            token.Symbol = this.DbObject.Definition.Substring(token.StartIndex.Value, token.Length);
        }

        private void ReplaceTokens(IEnumerable<TokenInfo> tokens)
        {
            Action<TokenInfo> changeValue = (token) =>
             {
                 this.RestoreValue(token);

                 if (this.TargetInterpreter.DatabaseType != DatabaseType.SqlServer && this.TargetDbSchema != "dbo" && token.Symbol.ToLower().Contains("dbo."))
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
                            function.Symbol = function.Symbol;

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
