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
        private Regex identifierRegex = new Regex($@"([`""\[][ _0-9a-zA-Z]+[`""\]])");
        private Regex nameRegex = new Regex(@"\b(^[_a-zA-Z][ _0-9a-zA-Z]+$)\b");
        private ColumnTranslator columnTranslator;
        public CommonScript Script { get; set; }
        public ScriptDbObject DbObject { get; set; }
        public DbInterpreter SourceInterpreter { get; set; }
        public DbInterpreter TargetInterpreter { get; set; }
        public Dictionary<string, string> ReplacedValues { get; private set; } = new Dictionary<string, string>();

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

            this.columnTranslator = new ColumnTranslator(this.SourceInterpreter, this.TargetInterpreter, null);
            columnTranslator.LoadMappings();

            Func<TokenInfo, bool> changeValue = (token) =>
            {
                string oldSymbol = token.Symbol;

                bool hasChanged = false;

                if (this.TargetInterpreter.DatabaseType == DatabaseType.SqlServer)
                {
                    token.Symbol = token.Symbol.TrimStart('@') + "@";
                    hasChanged = true;

                    if (!this.ReplacedValues.ContainsKey(token.Symbol))
                    {
                        this.ReplacedValues.Add(oldSymbol, token.Symbol);
                    }
                }
                else if (this.SourceInterpreter.DatabaseType == DatabaseType.SqlServer)
                {
                    token.Symbol = token.Symbol.TrimStart('@');

                    if (keywords.Contains(token.Symbol.ToUpper()))
                    {
                        token.Symbol = "_" + token.Symbol;
                    }

                    hasChanged = true;

                    if (!this.ReplacedValues.ContainsKey(token.Symbol))
                    {
                        this.ReplacedValues.Add(oldSymbol, token.Symbol);
                    }
                }

                return hasChanged;
            };

            this.ReplaceTokens(tokens);

            foreach (TokenInfo token in tokens)
            {
                if (token.Symbol == null)
                {
                    continue;
                }

                bool hasChanged = false;

                if (token.Type == TokenType.ParameterName)
                {
                    hasChanged = changeValue(token);
                }
                else if (token.Type == TokenType.DataType)
                {
                    TableColumn tableColumn = this.CreateTableColumn(token.Symbol);

                    columnTranslator.ConvertDataType(tableColumn);

                    token.Symbol = this.TargetInterpreter.ParseDataType(tableColumn).Trim(this.TrimChars);
                }
                else if (token.Type == TokenType.VariableName)
                {
                    hasChanged = changeValue(token);
                }
                else if (token.Type == TokenType.TableName)
                {
                    token.Symbol = this.GetQuotedName(token.Symbol);
                }
                else if (token.Type == TokenType.ColumnName)
                {
                    ColumnInfo column = token.Tag as ColumnInfo;

                    if (column == null)
                    {
                        token.Symbol = this.GetQuotedName(token.Symbol);
                    }
                    else
                    {
                        string columnContent = "";

                        if (column.Expression != null)
                        {
                            columnContent = $"{ this.GetQuotedName(column.Expression.ToString())}";
                        }

                        if (column.Alias != null && !string.IsNullOrEmpty(column.Alias.Symbol))
                        {
                            columnContent += $" AS {this.GetQuotedName(column.Alias.ToString())}";
                        }

                        token.Symbol = columnContent;
                    }
                }
                else if(token.Type == TokenType.Condition)
                {
                    token.Symbol = this.GetQuotedString(token.Symbol);
                }

                if (!hasChanged && this.ReplacedValues.ContainsKey(token.Symbol))
                {
                    token.Symbol = this.ReplacedValues[token.Symbol];
                }
            }

            this.Script.Owner = null;

            this.Script.Name.Symbol = this.TargetInterpreter.GetQuotedString(this.DbObject.Name);
        }

        private string GetQuotedName(string name)
        {
            name = this.GetQuotedString(name);

            List<string> items = name.Split('.').ToList();

            string lastItem = items[items.Count - 1];

            if (nameRegex.IsMatch(lastItem))
            {
                items[items.Count - 1] = this.TargetInterpreter.GetQuotedString(items[items.Count - 1].Trim(this.TrimChars));
            }

            for (int i = 0; i < items.Count - 1; i++)
            {
                if (this.TargetInterpreter.DatabaseType != DatabaseType.SqlServer && this.GetTrimedName(items[i], this.SourceInterpreter) == "dbo")
                {
                    items[i] = "";
                }
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
                column.DataType = dataType;
            }

            return column;
        }
    }
}
