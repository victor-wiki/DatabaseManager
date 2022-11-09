using DatabaseConverter.Core.Model;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class ScriptTokenProcessor : IDisposable
    {       
        private DataTypeTranslator dataTypeTranslator;
        private Dictionary<string, string> dictChangedValues = new Dictionary<string, string>();
        private List<IEnumerable<VariableMapping>> triggerVariableMappings;
        public CommonScript Script { get; set; }
        public ScriptDbObject DbObject { get; set; }
        public DbInterpreter SourceDbInterpreter { get; set; }
        public DbInterpreter TargetDbInterpreter { get; set; }
        public List<UserDefinedType> UserDefinedTypes { get; set; } = new List<UserDefinedType>();
        public DbConverterOption Option { get; set; }

        public char[] TrimChars
        {
            get
            {
                return new char[] { '"', this.SourceDbInterpreter.QuotationLeftChar, this.SourceDbInterpreter.QuotationRightChar, this.TargetDbInterpreter.QuotationLeftChar, this.TargetDbInterpreter.QuotationRightChar };
            }
        }

        public ScriptTokenProcessor(CommonScript script, ScriptDbObject dbObject, DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter)
        {
            this.triggerVariableMappings = TriggerVariableMappingManager.GetVariableMappings();
            this.Script = script;
            this.DbObject = dbObject;
            this.SourceDbInterpreter = sourceInterpreter;
            this.TargetDbInterpreter = targetInterpreter;
        }

        public void Process()
        {
            this.ProcessTokens();
        }

        private void ProcessTokens()
        {
            IEnumerable<string> keywords = KeywordManager.GetKeywords(this.TargetDbInterpreter.DatabaseType);
            IEnumerable<string> functions = FunctionManager.GetFunctionSpecifications(this.TargetDbInterpreter.DatabaseType).Select(item => item.Name);

            this.dataTypeTranslator = new DataTypeTranslator(this.SourceDbInterpreter, this.TargetDbInterpreter);
            dataTypeTranslator.LoadMappings();

            DatabaseType targeDbType = this.TargetDbInterpreter.DatabaseType;

            Action<TokenInfo> changeParaVarName = (pv) =>
            {
                this.RestoreValue(pv);

                string name = pv.Symbol;

                if (targeDbType != DatabaseType.SqlServer)
                {
                    if (keywords.Contains(name.ToUpper()) || functions.Contains(name.ToUpper()))
                    {
                        pv.Symbol = "V_" + name;
                    }
                    else
                    {
                        pv.Symbol = name.Replace("@", "V_");

                        if (pv.Parent != null)
                        {
                            pv.Parent.Symbol = pv.Parent.Symbol.Replace(name, pv.Symbol);
                        }
                    }
                }
                else
                {
                    pv.Symbol = "@" + pv.Symbol;
                }

                this.AddChangedValue(this.dictChangedValues, name, pv.Symbol);
            };

            Action<TokenInfo> changeDataType = (token) =>
            {
                if (token.Symbol != null)
                {
                    TableColumn column = TranslateHelper.SimulateTableColumn(this.SourceDbInterpreter, token.Symbol, this.Option, this.UserDefinedTypes, this.TrimChars);

                    TranslateHelper.TranslateTableColumnDataType(this.dataTypeTranslator, column);

                    token.Symbol = this.TargetDbInterpreter.ParseDataType(column);
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

            this.ProcessFunctions();
            this.ProcessSequences();

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
                        || tokenType == TokenType.ViewName
                        || tokenType == TokenType.TypeName
                        || tokenType == TokenType.SequenceName
                        || tokenType == TokenType.FunctionName
                        || tokenType == TokenType.ProcedureName
                        || tokenType == TokenType.TriggerName
                        || tokenType == TokenType.ColumnName
                        || tokenType == TokenType.Alias)
                    {
                        if (tokenType == TokenType.ColumnName)
                        {
                            if (statement is SelectStatement select)
                            {
                                if (select.TableName == null && select.FromItems == null && this.dictChangedValues.ContainsKey(token.Symbol))
                                {
                                    changeParaVarName(token);
                                }
                            }
                        }

                        this.HandleQuotationChar(statement, token, tokens);
                    }
                    else if (tokenType == TokenType.VariableName 
                        || (tokenType == TokenType.CursorName && token.Symbol?.StartsWith("@") == true )
                      )
                    {
                        if (token.Symbol?.StartsWith("@@") == false)
                        {
                            changeParaVarName(token);
                        }
                    }
                    else if (token.Type == TokenType.DataType)
                    {
                        changeDataType(token);
                    }
                    else
                    {
                        this.ReplaceTokenSymbol(this.dictChangedValues, token);

                        this.HandleQuotationChar(statement, token, tokens);
                    }

                    this.HandleConcatChars(token);
                }
            }            

            this.Script.Schema = this.GetNewQuotedString(this.DbObject.Schema);
            this.Script.Name.Symbol = this.GetNewQuotedString(this.DbObject.Name);

            #region Handle Trigger
            if (this.Script is TriggerScript ts)
            {
                this.RestoreValue(ts.TableName);
                ts.TableName.Symbol = this.GetQuotedDbObjectNameWithSchema(ts.TableName.Symbol);

                if (ts.OtherTriggerName != null)
                {
                    this.RestoreValue(ts.OtherTriggerName);
                    ts.OtherTriggerName.Symbol = this.GetQuotedDbObjectNameWithSchema(ts.OtherTriggerName?.Symbol);
                }

                if (ts.FunctionName != null)
                {
                    this.RestoreValue(ts.FunctionName);
                    ts.FunctionName.Symbol = this.GetQuotedDbObjectNameWithSchema(ts.FunctionName?.Symbol);
                }
            }
            #endregion
        }

        private void HandleConcatChars(TokenInfo token)
        {
            token.Symbol = TranslateHelper.ConvertConcatChars(token.Symbol, this.SourceDbInterpreter.STR_CONCAT_CHARS, this.TargetDbInterpreter.STR_CONCAT_CHARS);
        }

        private void HandleQuotationChar(Statement statement, TokenInfo token, IEnumerable<TokenInfo> tokens)
        {
            this.ReplaceQuotationChar(statement, token, tokens);
        }

        private void ReplaceQuotationChar(Statement statement, TokenInfo token, IEnumerable<TokenInfo> tokens)
        {
            string symbol = token.Symbol;

            if (symbol == null)
            {
                return;
            }

            bool canQuote = this.CanQuote(statement, token);

            if (!canQuote)
            {
                return;
            }

            bool isFunction = this.IsFunctionToken(token);

            if (isFunction && token.Children.Count == 0)
            {
                return;
            }

            if (token.Children == null || token.Children.Count == 0)
            {
                if (token.Type == TokenType.SearchCondition)
                {
                    return;
                }

                if (!symbol.Contains("."))
                {
                    if(token.Type == TokenType.UpdateSetValue)
                    {
                        return;
                    }                 

                    bool isAlias = this.IsAlias(token.Symbol, tokens);

                    if (!isAlias && !isFunction)
                    {
                        if (!this.IsTriggerInteralTable(token))
                        {
                            bool handled = false;

                            if (statement is SelectStatement select)
                            {
                                if (select.IntoTableName?.Symbol == token.Symbol)
                                {
                                    if (this.dictChangedValues.Keys.Contains(token.Symbol))
                                    {
                                        token.Symbol = this.dictChangedValues[token.Symbol];
                                        handled = true;
                                    }
                                }
                            }

                            if (!handled)
                            {
                                token.Symbol = this.GetNewQuotedString(token.Symbol);
                            }
                        }
                    }
                }
                else
                {
                    Func<string[], string> getNewSymbol = (items) =>
                    {
                        string schema = null, tableName = null, columnName = null;

                        if (token.Type == TokenType.ColumnName || token.Type == TokenType.OrderBy || token.Type == TokenType.GroupBy || token.Type == TokenType.SearchCondition)
                        {
                            if (items.Length == 2)
                            {
                                tableName = items[0];
                                columnName = items[1];
                            }
                            else if (items.Length == 3)
                            {
                                schema = items[0];
                                tableName = items[1];
                                columnName = items[2];
                            }

                            bool isAlias = this.IsAlias(tableName, tokens);

                            if (schema != null)
                            {
                                schema = this.GetMappedSchema(schema);
                            }

                            return this.GetQuotedFullColumnName(schema, tableName, columnName, isAlias);
                        }
                        else
                        {
                            if (items.Length == 1)
                            {
                                tableName = items[0];
                            }
                            else if (items.Length == 2)
                            {
                                if (this.IsTriggerVariable(token))
                                {
                                    return $"{items[0]}.{this.GetNewQuotedString(items[1])}";
                                }
                                else
                                {
                                    schema = items[0];
                                    tableName = items[1];
                                }
                            }

                            bool isAlias = this.IsAlias(tableName, tokens);

                            if (schema != null)
                            {
                                schema = this.GetMappedSchema(schema);
                            }

                            return this.GetQuotedFullTableName(schema, tableName, isAlias);
                        }
                    };

                    string[] items = token.Symbol.Split('.');

                    if (items.Length <= 3)
                    {
                        string newSymbol = getNewSymbol(items);

                        if (!string.IsNullOrEmpty(newSymbol))
                        {
                            token.Symbol = this.RemoveRepeatedQuotationChars(newSymbol);
                        }
                    }
                }
            }
            else
            {
                foreach (var child in token.Children)
                {
                    string oldSymbol = child.Symbol;

                    if (this.HasBeenQuoted(oldSymbol))
                    {
                        continue;
                    }

                    string oldAliasSymbol = null;
                    NameToken nt = null;

                    if (child is NameToken)
                    {
                        nt = child as NameToken;
                        oldAliasSymbol = nt.Alias?.Symbol;
                    }

                    this.ReplaceQuotationChar(statement, child, tokens);

                    child.Parent.Symbol = this.ReplaceSymbol(child.Parent.Symbol, oldSymbol, child.Symbol);

                    if (nt != null)
                    {
                        if (oldAliasSymbol != nt.Alias?.Symbol)
                        {
                            child.Parent.Symbol = this.ReplaceSymbol(child.Parent.Symbol, oldAliasSymbol, nt.Alias.Symbol);
                        }
                    }

                    child.Parent.Symbol = this.RemoveRepeatedQuotationChars(child.Parent.Symbol);
                }
            }

            if (token is NameToken)
            {
                var t = token as NameToken;

                this.ChangeAliasQuotationChar(t);
            }
        }

        private bool ChangeAliasQuotationChar(NameToken token)
        {
            if (token.Alias != null)
            {
                if (token.Alias.Symbol.StartsWith(this.SourceDbInterpreter.QuotationLeftChar))
                {
                    string oldAliasSymbol = token.Alias.Symbol;

                    token.Alias.Symbol = this.GetAppropriateAlias(token.Alias.Symbol);

                    return true;
                }
            }

            return false;
        }

        private bool IsTriggerInteralTable(TokenInfo token)
        {
            if (this.Script is TriggerScript)
            {
                return this.triggerVariableMappings.Any(item => item.Any(t => t.DbType == this.SourceDbInterpreter.DatabaseType.ToString() && t.Variable.ToUpper() == token.Symbol));
            }

            return false;
        }

        private bool IsTriggerVariable(TokenInfo token)
        {
            return this.Script is TriggerScript && (token.Type == TokenType.UpdateSetValue || token.Type == TokenType.InsertValue);
        }

        private string ReplaceSymbol(string value, string oldSymbol, string newSymbol)
        {
            if(string.IsNullOrEmpty(oldSymbol))
            {
                return value;
            }

            string pattern = null;

            if (Regex.IsMatch(oldSymbol, TranslateHelper.NameRegexPattern))
            {
                pattern = $"\\b{oldSymbol}\\b";
            }
            else
            {
                pattern = $"({oldSymbol})";
            }

            if (this.SourceDbInterpreter.DatabaseType == DatabaseType.SqlServer)
            {
                pattern = pattern.Replace("[", "\\[").Replace("]", "\\]");
            }

            string result = Regex.Replace(value, pattern, newSymbol, RegexOptions.Multiline);

            if (value.Contains("@" + oldSymbol)) //restore variable name
            {
                result = result.Replace("@" + newSymbol, "@" + oldSymbol);
            }

            return this.ValidateValue(result);
        }

        private bool CanQuote(Statement statement, TokenInfo token)
        {
            if (token.Type == TokenType.General || token.Type == TokenType.VariableName ||
               token.Type == TokenType.ParameterName || token.Type == TokenType.CursorName)
            {
                return false;
            }

            if (token.Type == TokenType.InsertValue)
            {
                return false;
            }

            if (token.Type == TokenType.IfCondition || token.Type == TokenType.ExitCondition)
            {
                return false;
            }

            if (token.Type == TokenType.ColumnName && (token.IsConst))
            {
                return false;
            }

            if (this.dictChangedValues.ContainsValue(token.Symbol))
            {
                return false;
            }

            if (token.Type == TokenType.ColumnName && statement is SelectStatement select)
            {
                if (select.TableName == null && select.FromItems == null && select.Where == null)
                {
                    return false;
                }
            }

            if (token.Type == TokenType.UpdateSetValue)
            {
                if (token.Symbol.EndsWith('\'') || decimal.TryParse(token.Symbol, out _))
                {
                    return false;
                }
            }

            if (this.HasBeenQuoted(token.Symbol))
            {
                return false;
            }

            if(token.Symbol.Trim().ToUpper() =="NULL")
            {
                return false; 
            }

            return true;
        }

        private string RemoveRepeatedQuotationChars(string symbol)
        {
            string repeatedQuotationLeftChars = this.TargetDbInterpreter.QuotationLeftChar.ToString().PadLeft(2, this.TargetDbInterpreter.QuotationLeftChar);
            string repeatedQuotationRightChars = this.TargetDbInterpreter.QuotationRightChar.ToString().PadLeft(2, this.TargetDbInterpreter.QuotationRightChar);

            return symbol.Replace(repeatedQuotationLeftChars, this.TargetDbInterpreter.QuotationLeftChar.ToString())
                         .Replace(repeatedQuotationRightChars, this.TargetDbInterpreter.QuotationRightChar.ToString());
        }

        private bool IsAlias(string symbol, IEnumerable<TokenInfo> tokens)
        {
            return tokens.Any(item => item.Type == TokenType.Alias && item.Symbol == symbol);
        }

        private string GetNewQuotedString(string value)
        {
            if (value == "*" || value?.EndsWith("'") == true)
            {
                return value;
            }

            if (this.HasBeenQuoted(value))
            {
                return value;
            }

            return this.GetQuotedString(this.GetTrimedName(value));
        }

        private bool HasBeenQuoted(string symbol)
        {
            if (symbol?.StartsWith(this.TargetDbInterpreter.QuotationLeftChar) == true && symbol?.EndsWith(this.TargetDbInterpreter.QuotationRightChar) == true)
            {
                return true;
            }

            return false;
        }

        private string GetQuotedString(string value)
        {
            return this.TargetDbInterpreter.GetQuotedString(value);
        }

        private string GetQuotedDbObjectNameWithSchema(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Contains("."))
                {
                    string[] items = value.Split('.');
                    string schema = this.GetTrimedName(items[0]).Trim();

                    string mappedSchema = this.GetMappedSchema(schema);

                    if (!string.IsNullOrEmpty(mappedSchema))
                    {
                        items[0] = mappedSchema;

                        return string.Join(".", items.Select(item => this.GetNewQuotedString(item)));
                    }
                    else
                    {
                        return string.Join(".", items.Skip(1).Select(item => this.GetNewQuotedString(item)));
                    }
                }
                else
                {
                    return this.GetQuotedString(value.Trim());
                }
            }

            return string.Empty;
        }

        private string GetQuotedFullTableName(string schema, string tableName, bool isAlias = false)
        {
            string quotedTableName = isAlias ? this.GetAppropriateAlias(tableName) : this.GetNewQuotedString(tableName);

            if (!string.IsNullOrEmpty(schema))
            {
                return $"{this.GetNewQuotedString(schema)}.{quotedTableName}";
            }
            else
            {
                return quotedTableName;
            }
        }

        private string GetQuotedFullColumnName(string schema, string tableName, string columnName, bool isAlias = false)
        {
            string quotedTableName = isAlias ? this.GetAppropriateAlias(tableName) : this.GetNewQuotedString(tableName);

            if (!string.IsNullOrEmpty(schema))
            {
                return $"{this.GetNewQuotedString(schema)}.{quotedTableName}.{this.GetNewQuotedString(columnName)}";
            }
            else
            {
                return $"{quotedTableName}.{this.GetNewQuotedString(columnName)}";
            }
        }

        private void AddChangedValue(Dictionary<string, string> dict, string key, string value)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, value);
            }
        }

        private string GetAppropriateAlias(string alias)
        {
            if (alias.StartsWith(this.SourceDbInterpreter.QuotationLeftChar))
            {
                return this.GetNewQuotedString(alias);
            }

            return alias;
        }

        private void ReplaceTokenSymbol(Dictionary<string, string> dict, TokenInfo token)
        {
            if(token.Symbol == null)
            {
                return;
            }

            foreach (var pv in dict)
            {
                if (dict.Keys.Contains(token.Symbol))
                {
                    token.Symbol = token.Symbol.Replace(pv.Key, pv.Value);
                }
                else if (token.Symbol.Contains(pv.Key))
                {
                    if (!token.Symbol.Contains($"'{pv.Key}'"))
                    {
                        string pattern = pv.Key.StartsWith("@") ? $"({pv.Key})" : $"\\b{pv.Key}\\b";

                        token.Symbol = Regex.Replace(token.Symbol, pattern, pv.Value, RegexOptions.Multiline);
                    }
                }
            }
        }

        private string GetTrimedName(string name)
        {
            return name?.Trim(this.SourceDbInterpreter.QuotationLeftChar, this.SourceDbInterpreter.QuotationRightChar, '"');
        }

        private string GetOriginalValue(TokenInfo token)
        {
            return this.DbObject.Definition.Substring(token.StartIndex.Value, token.Length);
        }

        private void RestoreValue(TokenInfo token)
        {
            TranslateHelper.RestoreTokenValue(this.DbObject.Definition, token);            
        }       

        private void ReplaceTokens(IEnumerable<TokenInfo> tokens)
        {
            Action<TokenInfo> changeValue = (token) =>
             {
                 this.RestoreValue(token);
             };

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
                            string oldValue = this.GetOriginalValue(function);

                            token.Symbol = this.ReplaceValue(token.Symbol, oldValue, function.Symbol);
                        }
                    }
                }
            }
        }        

        private bool IsFunctionToken(TokenInfo token)
        {
            return this.Script.Functions.Any(item => item.StartIndex == token.StartIndex);
        }

        private string ReplaceValue(string source, string oldValue, string newValue)
        {
            return this.ValidateValue(Regex.Replace(source, Regex.Escape(oldValue), newValue, RegexOptions.IgnoreCase));
        }

        private string ValidateValue(string value)
        {
            if (value != null)
            {
                while (value.Contains("()()"))
                {
                    value = value.Replace("()()", "()");
                }
            }

            return value;
        }

        private void ProcessFunctions()
        {
            this.Script.Functions.ForEach(item =>
            {
                if (item != null)
                {
                    this.RestoreValue(item);

                    if (item.Children != null)
                    {
                        item.Children.ForEach(t => this.RestoreValue(t));
                    }
                }
            });

            var functions = this.Script.Functions.Where(item => !item.Children.Any(t => t.Type == TokenType.SequenceName));

            this.TranslateFunctions(functions);
        }

        private void ProcessSequences()
        {
            var sequences = this.Script.Functions.Where(item => item.Children.Any(t => t.Type == TokenType.SequenceName));

            this.TranslateSequences(sequences);
        }

        private void TranslateFunctions(IEnumerable<TokenInfo> tokens)
        {
            FunctionTranslator functionTranslator = new FunctionTranslator(this.SourceDbInterpreter, this.TargetDbInterpreter, tokens);
            functionTranslator.UserDefinedTypes = this.UserDefinedTypes;

            if(this.Script is RoutineScript rs)
            {
                functionTranslator.RoutineType = rs.Type;
            }
           

            functionTranslator.Translate();
        }

        private void TranslateSequences(IEnumerable<TokenInfo> tokens)
        {
            foreach (TokenInfo token in tokens)
            {
                NameToken seqNameToken = token.Children.FirstOrDefault(item => item.Type == TokenType.SequenceName) as NameToken;

                if (seqNameToken != null)
                {
                    string schema = seqNameToken.Schema;
                    string seqName = seqNameToken.Symbol;

                    string mappedSchema = this.GetMappedSchema(schema);

                    DatabaseType targetDbType = this.TargetDbInterpreter.DatabaseType;

                    string trimedSeqName = this.GetTrimedName(seqName);

                    token.Symbol = SequenceTranslator.ConvertSequenceValue(this.TargetDbInterpreter, mappedSchema, trimedSeqName);
                }
            }
        }

        private string GetMappedSchema(string schema)
        {
            string mappedSchema = SchemaInfoHelper.GetMappedSchema(this.GetTrimedName(schema), this.Option.SchemaMappings);

            if (mappedSchema == null)
            {
                mappedSchema = this.TargetDbInterpreter.DefaultSchema;
            }

            return mappedSchema;
        }



        public void Dispose()
        {
            this.dictChangedValues = null;
        }
    }
}
