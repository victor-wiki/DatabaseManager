using DatabaseConverter.Core.Model;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class ScriptTokenProcessor : IDisposable
    {
        private DatabaseType sourceDbType;
        private DatabaseType targetDbType;
        private DataTypeTranslator dataTypeTranslator;
        private Dictionary<string, string> dictChangedValues = new Dictionary<string, string>();
        private List<IEnumerable<VariableMapping>> triggerVariableMappings;
        private List<string> charTokenSymbols = new List<string>();
        private List<TokenInfo> tokensWithConcatChars = new List<TokenInfo>();
        private IEnumerable<string> sourceFunctions;
        private IEnumerable<string> targetFunctions;
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
            this.sourceDbType = sourceInterpreter.DatabaseType;
            this.targetDbType = targetInterpreter.DatabaseType;

            this.sourceFunctions = FunctionManager.GetFunctionSpecifications(this.sourceDbType).Select(item => item.Name);
            this.targetFunctions = FunctionManager.GetFunctionSpecifications(this.targetDbType).Select(item => item.Name);
        }

        public void Process()
        {
            this.ProcessTokens();
        }

        private void ProcessTokens()
        {
            IEnumerable<string> keywords = KeywordManager.GetKeywords(this.targetDbType);

            this.dataTypeTranslator = new DataTypeTranslator(this.SourceDbInterpreter, this.TargetDbInterpreter);
            dataTypeTranslator.LoadMappings();

            Action<TokenInfo> changeParaVarName = (pv) =>
            {
                this.RestoreValue(pv);

                string name = pv.Symbol;

                if (this.targetDbType != DatabaseType.SqlServer)
                {
                    if (keywords.Contains(name.ToUpper()) || this.targetFunctions.Contains(name.ToUpper()))
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
                    if (!pv.Symbol.StartsWith("@"))
                    {
                        pv.Symbol = "@" + pv.Symbol;
                    }
                }

                this.AddChangedValue(this.dictChangedValues, name, pv.Symbol);
            };

            Action<Statement, TokenInfo> changeDataType = (statement, token) =>
            {
                if (token.Symbol != null)
                {
                    if (this.sourceDbType == DatabaseType.SqlServer)
                    {
                        if (token.Symbol.ToUpper() == "DOUBLEPRECISION")
                        {
                            token.Symbol = "FLOAT";
                        }
                    }

                    TableColumn column = TranslateHelper.SimulateTableColumn(this.SourceDbInterpreter, token.Symbol, this.Option, this.UserDefinedTypes, this.TrimChars);

                    TranslateHelper.TranslateTableColumnDataType(this.dataTypeTranslator, column);

                    if (this.targetDbType == DatabaseType.SqlServer)
                    {
                        if (DataTypeHelper.IsTextType(column.DataType) && statement is DeclareVariableStatement)
                        {
                            column.DataType = "NVARCHAR";
                        }
                    }

                    token.Symbol = this.TargetDbInterpreter.ParseDataType(column);

                    if ((statement is DeclareVariableStatement) && DataTypeHelper.IsCharType(column.DataType))
                    {
                        this.charTokenSymbols.Add((statement as DeclareVariableStatement).Name.Symbol);
                    }
                }
            };

            if (this.Script is RoutineScript routineScript)
            {
                var parameters = routineScript.Parameters;

                foreach (var parameter in parameters)
                {
                    changeParaVarName(parameter.Name);
                    changeDataType(null, parameter.DataType);
                }

                if (routineScript.ReturnDataType != null)
                {
                    changeDataType(null, routineScript.ReturnDataType);
                }
                else if (routineScript.ReturnTable != null)
                {
                    if (routineScript.ReturnTable.Name != null && routineScript.ReturnTable.Name.Type == TokenType.VariableName)
                    {
                        changeParaVarName(routineScript.ReturnTable.Name);
                    }
                }
            }

            this.ProcessFunctions();
            this.ProcessSequences();

            foreach (Statement statement in this.Script.Statements)
            {
                ScriptTokenExtracter tokenExtracter = new ScriptTokenExtracter(statement);
                List<TokenInfo> tokens = tokenExtracter.Extract().ToList();

                this.ReplaceTokens(tokens);

                foreach (TokenInfo token in tokens)
                {
                    if (string.IsNullOrEmpty(token.Symbol))
                    {
                        continue;
                    }

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
                        || tokenType == TokenType.ConstraintName
                        || tokenType == TokenType.TableAlias
                        || tokenType == TokenType.ColumnAlias)
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
                            else if (token.Symbol?.StartsWith("@") == true && token.Children.Count == 0)
                            {
                                if (this.dictChangedValues.ContainsKey(token.Symbol))
                                {
                                    changeParaVarName(token);

                                    continue;
                                }
                            }
                        }
                        else if (tokenType == TokenType.TableName)
                        {
                            if (token.Symbol?.StartsWith("@") == true)
                            {
                                changeParaVarName(token);

                                continue;
                            }
                        }

                        this.HandleQuotationChar(statement, token, tokens);
                    }
                    else if (tokenType == TokenType.VariableName || tokenType == TokenType.UserVariableName
                        || (tokenType == TokenType.CursorName && token.Symbol?.StartsWith("@") == true)
                      )
                    {
                        if (token.Symbol?.StartsWith("@@") == false)
                        {
                            changeParaVarName(token);
                        }
                    }
                    else if (token.Type == TokenType.DataType)
                    {
                        if (statement is DeclareVariableStatement && this.IsCopyingType(token.Symbol)
                            && (this.targetDbType == DatabaseType.Oracle || this.targetDbType == DatabaseType.Postgres))
                        {
                            continue;
                        }

                        changeDataType(statement, token);
                    }
                    else
                    {
                        this.ReplaceTokenSymbol(this.dictChangedValues, token);

                        this.HandleQuotationChar(statement, token, tokens);
                    }

                    if (token.Symbol != null && !string.IsNullOrWhiteSpace(this.SourceDbInterpreter.STR_CONCAT_CHARS)
                        && token.Symbol.Contains(this.SourceDbInterpreter.STR_CONCAT_CHARS))
                    {
                        this.tokensWithConcatChars.Add(token);
                    }
                }
            }

            this.tokensWithConcatChars.ForEach(item => this.HandleConcatChars(item));

            this.Script.Schema = this.GetNewQuotedString(this.DbObject.Schema);

            if (this.Script.Name != null)
            {
                this.Script.Name.Symbol = this.GetNewQuotedString(this.DbObject.Name);
            }

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
            if (this.Option.ConvertConcatChar)
            {
                token.Symbol = ConcatCharsHelper.ConvertConcatChars(this.SourceDbInterpreter, this.TargetDbInterpreter, token.Symbol, this.charTokenSymbols);
            }
        }

        private void HandleQuotationChar(Statement statement, TokenInfo token, List<TokenInfo> tokens)
        {
            this.ReplaceQuotationChar(statement, token, tokens);
        }

        private void ReplaceQuotationChar(Statement statement, TokenInfo token, List<TokenInfo> tokens)
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

            if (isFunction)
            {
                string functionName = TranslateHelper.ExtractNameFromParenthesis(symbol);

                if (token.Children.Count == 0)
                {
                    if (this.sourceFunctions.Contains(functionName.ToUpper()))
                    {
                        return;
                    }
                }
            }

            Action procToken = () =>
            {
                if (token.Type == TokenType.SearchCondition)
                {
                    return;
                }

                if (!symbol.Contains("."))
                {
                    if (token.Type == TokenType.UpdateSetValue)
                    {
                        return;
                    }

                    bool isTableAlias = this.IsAlias(token, tokens, TokenType.TableAlias);

                    if (!isTableAlias && !isFunction)
                    {
                        if (!this.IsTriggerInteralTable(token))
                        {
                            bool handled = false;

                            if (statement is SelectStatement select)
                            {
                                TokenInfo intoTableName = this.GetIntoTableName(select);

                                if (intoTableName?.Symbol == token.Symbol)
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
                    Func<TokenSymbolNameInfo, string> getNewSymbol = (nameInfo) =>
                    {
                        string schema = null;

                        if (nameInfo.Type == TokenSymbolNameType.SchemaTableColumn)
                        {
                            bool isTableNameAlias = this.IsAlias(token, tokens, TokenType.TableAlias, nameInfo);
                            bool isColumnNameAlias = this.IsAlias(token, tokens, TokenType.ColumnAlias, nameInfo);

                            if (nameInfo.Schema != null)
                            {
                                schema = this.GetMappedSchema(schema);
                            }

                            return this.GetQuotedFullColumnName(schema, nameInfo.TableName, nameInfo.ColumnName, isTableNameAlias, isColumnNameAlias);
                        }
                        else if (nameInfo.Type == TokenSymbolNameType.SchemaTable)
                        {
                            if (nameInfo.TableName != null)
                            {
                                if (this.IsTriggerVariable(token))
                                {
                                    return $"{nameInfo.Schema}.{this.GetNewQuotedString(nameInfo.TableName)}";
                                }
                            }

                            bool isAlias = this.IsAlias(token, tokens, TokenType.TableAlias, nameInfo);

                            if (schema != null)
                            {
                                schema = this.GetMappedSchema(schema);
                            }

                            return this.GetQuotedFullTableName(schema, nameInfo.TableName, isAlias);
                        }

                        return null;
                    };

                    string[] items = token.Symbol.Split('.');

                    if (items.Length <= 3)
                    {
                        TokenSymbolNameInfo nameInfo = this.GetTokenSymbolNameInfo(token);

                        string newSymbol = nameInfo == null ? null : getNewSymbol(nameInfo);

                        if (!string.IsNullOrEmpty(newSymbol))
                        {
                            token.Symbol = this.RemoveRepeatedQuotationChars(newSymbol);
                        }
                    }
                }
            };

            if (!this.HasChildren(token))
            {
                procToken();
            }
            else
            {
                string parentOldSymbol = token.Symbol;

                int aliasCount = 0;

                //replace with child whose symbol has '.' first
                var children = token.Children.Where(item => item.Symbol?.Contains(".") == true).Union(token.Children.Where(item => item.Symbol?.Contains(".") == false));

                foreach (var child in children)
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
                        // this.ChangeNameTokenAlias(child);

                        if (oldAliasSymbol != nt.Alias?.Symbol)
                        {
                            child.Parent.Symbol = this.ReplaceSymbol(child.Parent.Symbol, oldAliasSymbol, nt.Alias.Symbol);
                        }
                    }

                    child.Parent.Symbol = this.RemoveRepeatedQuotationChars(child.Parent.Symbol);

                    if (child.Type == TokenType.TableAlias || child.Type == TokenType.ColumnAlias)
                    {
                        aliasCount++;
                    }
                }

                if (parentOldSymbol == token.Symbol)
                {
                    if (aliasCount == token.Children.Count)
                    {
                        string[] items = parentOldSymbol.Split('.');

                        if (items.Length <= 3 && items.All(item => Regex.IsMatch(this.GetTrimedName(item), RegexHelper.NameRegexPattern)))
                        {
                            procToken();
                        }
                    }
                }
            }

            this.ChangeNameTokenAlias(token);
        }

        private TokenInfo GetIntoTableName(SelectStatement statement)
        {
            if (statement.Intos == null || statement.Intos.Count == 0)
            {
                return null;
            }

            return statement.Intos.FirstOrDefault(item => item.Type == TokenType.TableName);
        }

        private void ChangeNameTokenAlias(TokenInfo token)
        {
            if (token is NameToken)
            {
                var t = token as NameToken;

                this.ChangeAliasQuotationChar(t);
            }
        }

        private bool HasChildren(TokenInfo token)
        {
            if (token == null || token.Children == null || token.Children.Count == 0)
            {
                return false;
            }

            if (token.Children.Count == 1)
            {
                var child = token.Children[0];

                if (child.StartIndex == token.StartIndex && child.StopIndex == token.StopIndex)
                {
                    return false;
                }
            }

            return token.Children.Count > 0;
        }

        private TokenSymbolNameInfo GetTokenSymbolNameInfo(TokenInfo token)
        {
            string symbol = token.Symbol;
            string[] items = symbol.Split('.');

            if (items.Any(item => !TranslateHelper.IsValidName(item, this.TrimChars)))
            {
                return null;
            }

            string schema = null, tableName = null, columnName = null;
            TokenSymbolNameType type = TokenSymbolNameType.Unknown;

            if (token.Type == TokenType.ColumnName
                || token.Type == TokenType.OrderBy
                || token.Type == TokenType.GroupBy
                || token.Type == TokenType.SearchCondition
                || token.Type == TokenType.UpdateSetValue)
            {
                type = TokenSymbolNameType.SchemaTableColumn;

                if (items.Length == 2)
                {
                    tableName = items[0].Trim();
                    columnName = items[1].Trim();
                }
                else if (items.Length == 3)
                {
                    schema = items[0].Trim();
                    tableName = items[1].Trim();
                    columnName = items[2].Trim();
                }
            }
            else
            {
                type = TokenSymbolNameType.SchemaTable;

                if (items.Length == 1)
                {
                    tableName = items[0].Trim();
                }
                else if (items.Length == 2)
                {
                    schema = items[0].Trim();
                    tableName = items[1].Trim();
                }
            }

            return new TokenSymbolNameInfo() { Schema = schema, TableName = tableName, ColumnName = columnName, Type = type };
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
                else if (token is ColumnName)
                {
                    if (!token.Alias.Symbol.StartsWith(this.TargetDbInterpreter.QuotationLeftChar))
                    {
                        token.Alias.Symbol = this.GetNewQuotedString(token.Alias.Symbol);

                        return true;
                    }
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
            if (string.IsNullOrEmpty(oldSymbol))
            {
                return value;
            }

            string pattern = null;
            string oldSymbolPattern = oldSymbol;

            oldSymbolPattern = Regex.Escape(oldSymbolPattern);

            if (Regex.IsMatch(oldSymbol, RegexHelper.NameRegexPattern))
            {
                pattern = $"\\b{oldSymbolPattern}\\b";
            }
            else
            {
                if (oldSymbol.EndsWith(this.SourceDbInterpreter.QuotationRightChar))
                {
                    pattern = $"({oldSymbolPattern})";
                }
                else
                {
                    pattern = $"({oldSymbolPattern})\\b";
                }
            }

            string result = Regex.Replace(value, pattern, RegexHelper.CheckReplacement(newSymbol), RegexOptions.Multiline);

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

            if (ValueHelper.IsStringValue(token.Symbol) && token.Children.Count == 0)
            {
                if (token is ColumnName && (token as ColumnName).Alias != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (token.Type == TokenType.ColumnName && (token.IsConst))
            {
                if (token is ColumnName && (token as ColumnName).Alias != null)
                {
                    return true;
                }

                return false;
            }

            if (this.dictChangedValues.ContainsValue(token.Symbol))
            {
                return false;
            }

            if (token.Type == TokenType.ColumnName && statement is SelectStatement select)
            {
                if (select.TableName == null && select.FromItems == null && select.Where == null && token.Children.Count == 0)
                {
                    return false;
                }
            }

            if (token.Type == TokenType.UpdateSetValue)
            {
                if (ValueHelper.IsStringValue(token.Symbol) || decimal.TryParse(token.Symbol, out _))
                {
                    return false;
                }
            }

            if (this.HasBeenQuoted(token.Symbol))
            {
                return false;
            }

            string upperCase = token.Symbol.Trim().ToUpper();

            if (upperCase == "NULL" 
                || (upperCase.EndsWith(")") && token.Children.Count ==0 ) 
                || (token.Symbol.StartsWith("@") && token.Children.Count == 0))
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

        private bool IsAlias(TokenInfo token, IEnumerable<TokenInfo> tokens, TokenType tokenType, TokenSymbolNameInfo nameInfo = null)
        {
            string symbol = token.Symbol;
            bool isAlias = false;

            if (nameInfo == null)
            {
                nameInfo = this.GetTokenSymbolNameInfo(token);
            }

            if (nameInfo != null)
            {
                if (tokenType == TokenType.TableAlias)
                {
                    isAlias = tokens.Any(item => item.Type == tokenType && item.Symbol == nameInfo.TableName);
                }
                else if (tokenType == TokenType.ColumnAlias)
                {
                    isAlias = tokens.Any(item => item.Type == tokenType && item.Symbol == nameInfo.ColumnName);
                }
            }

            if (isAlias && tokenType == TokenType.ColumnAlias)
            {
                if (token.Parent is ColumnName cn)
                {
                    if (cn != null && cn.Alias?.Symbol == symbol)
                    {
                        return false; //column alias can't be as same as its parent's alias
                    }
                }
            }

            return isAlias;
        }

        private bool IsQuoted(TokenInfo token)
        {
            if (token != null && token.Symbol?.StartsWith(this.SourceDbInterpreter.QuotationLeftChar) == true)
            {
                return true;
            }

            return false;
        }

        private bool IsQuotationCharSame()
        {
            return this.SourceDbInterpreter.QuotationLeftChar == this.TargetDbInterpreter.QuotationLeftChar;
        }

        private bool IsCopyingType(string symbol)
        {
            return symbol.EndsWith("%TYPE");
        }

        private string GetNewQuotedString(string value)
        {
            if (value == null)
            {
                return String.Empty;
            }

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

        private string GetQuotedFullColumnName(string schema, string tableName, string columnName, bool isTableNameAlias = false, bool isColumnNameAlias = false)
        {
            string quotedTableName = isTableNameAlias ? this.GetAppropriateAlias(tableName) : this.GetNewQuotedString(tableName);
            //string quotedColumnName = isColumnNameAlias ? this.GetAppropriateAlias(columnName) : this.GetNewQuotedString(columnName);
            string quotedColumnName = this.GetNewQuotedString(columnName);

            if (!string.IsNullOrEmpty(schema))
            {
                return $"{this.GetNewQuotedString(schema)}.{quotedTableName}.{quotedColumnName}";
            }
            else
            {
                return $"{quotedTableName}.{quotedColumnName}";
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
            if (token.Symbol == null)
            {
                return;
            }

            foreach (var pv in dict)
            {
                string key = this.FindKeyIgnoreCase(dict, token.Symbol);

                if (!string.IsNullOrEmpty(key))
                {
                    token.Symbol = token.Symbol.Replace(key, pv.Value, StringComparison.OrdinalIgnoreCase);
                }
                else if (token.Symbol.Contains(pv.Key, StringComparison.OrdinalIgnoreCase))
                {
                    if (!token.Symbol.Contains($"'{pv.Key}'", StringComparison.OrdinalIgnoreCase))
                    {
                        string pattern = pv.Key.StartsWith("@") ? $"({pv.Key})" : $"\\b{pv.Key}\\b";

                        token.Symbol = Regex.Replace(token.Symbol, pattern, pv.Value, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    }
                }
            }
        }

        private string FindKeyIgnoreCase(Dictionary<string, string> dict, string symbol)
        {
            var keys = dict.Keys;

            var key = keys.FirstOrDefault(item => item.ToUpper() == symbol.ToLower());

            return key;
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
            return this.ValidateValue(RegexHelper.Replace(source, oldValue, newValue, RegexOptions.IgnoreCase | RegexOptions.Multiline));
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

            var builtInFunctions = functions.Where(item => this.IsBuiltinFunction(item)).ToList();
            var userDefinedFunction = functions.Except(builtInFunctions).ToList();

            this.TranslateFunctions(builtInFunctions);

            foreach (var udf in userDefinedFunction)
            {
                if (!udf.Symbol.Contains("."))
                {
                    if (this.targetDbType == DatabaseType.SqlServer && udf.Symbol.Contains("("))
                    {
                        udf.Symbol = $"dbo.{this.ReplaceRoutineQuotationChar(udf.Symbol)}";
                    }
                    else
                    {
                        udf.Symbol = this.ReplaceRoutineQuotationChar(udf.Symbol);
                    }
                }
            }
        }

        private bool IsBuiltinFunction(TokenInfo token)
        {
            string symbol = token.Symbol;

            string name = TranslateHelper.ExtractNameFromParenthesis(symbol);

            bool isBuiltin = this.sourceFunctions.Any(item => item.ToUpper() == this.GetTrimedName(name.Trim()).ToUpper());

            return isBuiltin;
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

            if (this.Script is RoutineScript rs)
            {
                functionTranslator.RoutineType = rs.Type;
            }
            else if (this.Script is TriggerScript ts)
            {
                functionTranslator.RoutineType = RoutineType.TRIGGER;
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

        private string ReplaceRoutineQuotationChar(string symbol)
        {
            int index = symbol.IndexOf("(");

            if (index > 0)
            {
                string name = TranslateHelper.ExtractNameFromParenthesis(symbol);

                return $"{this.GetNewQuotedString(name)}{symbol.Substring(index)}";
            }
            else
            {
                return this.GetNewQuotedString(symbol.Trim());
            }            
        }

        public void Dispose()
        {
            this.dictChangedValues = null;
            this.charTokenSymbols = null;
            this.tokensWithConcatChars = null;
        }
    }
}
