using Antlr.Runtime;
using DatabaseConverter.Core.Model;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using Newtonsoft.Json.Linq;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace DatabaseConverter.Core
{
    public class ScriptTokenProcessor
    {
        private const string NameExpression = @"^([a-zA-Z_\s]+)$";
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
                            pv.Parent.Symbol = pv.Parent.Symbol.Replace("@", "V_");
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
                    TableColumn column = this.CreateTableColumn(token.Symbol);

                    DataTypeInfo dataTypeInfo = DataTypeHelper.GetDataTypeInfoByTableColumn(column);

                    dataTypeTranslator.Translate(dataTypeInfo);

                    DataTypeHelper.SetDataTypeInfoToTableColumn(dataTypeInfo, column);

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
                        if (tokenType == TokenType.ColumnName)
                        {
                            if (statement is SelectStatement select)
                            {
                                if (select.TableName == null && select.FromItems == null && dictChangedValues.ContainsKey(token.Symbol))
                                {
                                    changeParaVarName(token);
                                }
                            }
                        }

                        this.HandleQuotationChar(statement, token, tokens);
                    }
                    else if (tokenType == TokenType.VariableName)
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
                        this.ReplaceTokenSymbol(dictChangedValues, token);

                        this.HandleQuotationChar(statement, token, tokens);
                    }
                }
            }

            this.Script.Schema = this.GetQuotedString(this.DbObject.Schema);
            this.Script.Name.Symbol = this.GetQuotedString(this.DbObject.Name);

            #region Handle Trigger
            if (this.Script is TriggerScript ts)
            {
                this.RestoreValue(ts.TableName);
                ts.TableName.Symbol = this.GetQuotedString(ts.TableName.Symbol);

                if (ts.OtherTriggerName != null)
                {
                    this.RestoreValue(ts.OtherTriggerName);
                    ts.OtherTriggerName.Symbol = this.GetQuotedString(ts.OtherTriggerName?.Symbol);
                }

                if (ts.FunctionName != null)
                {
                    this.RestoreValue(ts.FunctionName);
                    ts.FunctionName.Symbol = this.GetQuotedString(ts.FunctionName?.Symbol);
                }
            }
            #endregion
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
                if (!symbol.Contains("."))
                {
                    bool isAlias = this.IsAlias(token.Symbol, tokens);

                    if (!isAlias && !isFunction)
                    {
                        if (!this.IsTriggerInteralTable(token))
                        {
                            token.Symbol = this.GetNewQuotedString(token.Symbol);
                        }
                    }
                }
                else
                {
                    Func<string[], string> getNewSymbol = (items) =>
                    {
                        string schema = null, tableName = null, columnName = null;

                        if (token.Type == TokenType.ColumnName || token.Type == TokenType.OrderBy || token.Type == TokenType.GroupBy || token.Type == TokenType.Condition)
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

                        token.Symbol = this.RemoveRepeatedQuotationChars(newSymbol);
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
            string pattern = null;

            if (Regex.IsMatch(oldSymbol, NameExpression))
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

            return Regex.Replace(value, pattern, newSymbol, RegexOptions.Multiline);
        }

        private bool CanQuote(Statement statement, TokenInfo token)
        {
            if (token.Type == TokenType.General || token.Type == TokenType.VariableName ||
               token.Type == TokenType.ParameterName || token.Type == TokenType.CursorName)
            {
                return false;
            }

            if (token.Type == TokenType.Condition
                && (statement is IfStatement || statement is WhileStatement
                || statement is LoopStatement || statement is LeaveStatement)
                )
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
                if (select.TableName == null && select.FromItems == null)
                {
                    return false;
                }
            }

            if (this.HasBeenQuoted(token.Symbol))
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
            if (value == "*" || value?.StartsWith("'") == true)
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

        private void ReplaceTokenSymbol(Dictionary<string, string> dict, TokenInfo tokenInfo)
        {
            foreach (var pv in dict)
            {
                tokenInfo.Symbol = tokenInfo.Symbol.Replace(pv.Key, pv.Value);
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
            if (token != null && token.StartIndex.HasValue && token.Length > 0)
            {
                token.Symbol = this.DbObject.Definition.Substring(token.StartIndex.Value, token.Length);
            }
        }

        private void ReplaceTokens(IEnumerable<TokenInfo> tokens)
        {
            Action<TokenInfo> changeValue = (token) =>
             {
                 this.RestoreValue(token);
             };

            this.Script.Functions.ForEach(item =>
            {
                if (item != null)
                {
                    changeValue(item);

                    if (item.Children != null)
                    {
                        item.Children.ForEach(t => changeValue(t));
                    }
                }
            });

            var functions = this.Script.Functions.Where(item => !item.Children.Any(t => t.Type == TokenType.SequenceName));

            this.TranslateFunctions(functions);

            var sequences = this.Script.Functions.Where(item => item.Children.Any(t => t.Type == TokenType.SequenceName));

            this.TranslateSequences(sequences);

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
            return Regex.Replace(source, Regex.Escape(oldValue), newValue, RegexOptions.IgnoreCase);
        }

        private void TranslateFunctions(IEnumerable<TokenInfo> tokens)
        {
            FunctionTranslator functionTranslator = new FunctionTranslator(this.SourceDbInterpreter, this.TargetDbInterpreter, tokens);

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

                    if (targetDbType == DatabaseType.SqlServer)
                    {
                        token.Symbol = $"NEXT VALUE FOR {this.TargetDbInterpreter.GetQuotedDbObjectNameWithSchema(mappedSchema, trimedSeqName)}";
                    }
                    else if (targetDbType == DatabaseType.Postgres)
                    {
                        token.Symbol = $"NEXTVAL('{this.TargetDbInterpreter.GetQuotedDbObjectNameWithSchema(mappedSchema, trimedSeqName)}')";
                    }
                    else if (targetDbType == DatabaseType.Oracle)
                    {
                        token.Symbol = $"{this.TargetDbInterpreter.GetQuotedDbObjectNameWithSchema(mappedSchema, trimedSeqName)}.NEXTVAL";
                    }
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

        private TableColumn CreateTableColumn(string dataType)
        {
            TableColumn column = new TableColumn();

            if (dataType.IndexOf("(") > 0)
            {
                DataTypeInfo dataTypeInfo = DataTypeHelper.GetDataTypeInfo(this.SourceDbInterpreter, dataType);
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
                        if (DataTypeHelper.StartsWithN(dataType))
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
                    var attr = userDefinedType.Attributes.First();

                    column.DataType = attr.DataType;
                    column.MaxLength = attr.MaxLength;
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
