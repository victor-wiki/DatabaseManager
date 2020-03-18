using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseConverter.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TSQL;
using TSQL.Tokens;
using PoorMansTSqlFormatterRedux;

namespace DatabaseConverter.Core
{
    public class ViewTranslator
    {
        private List<View> views;
        private DbInterpreter sourceDbInterpreter;
        private DbInterpreter targetDbInterpreter;
        private string sourceOwnerName;
        private string targetOwnerName;
        private List<string> dataTypes = new List<string>();
        private List<DataTypeMapping> dataTypeMappings = new List<DataTypeMapping>();
        private List<IEnumerable<FunctionMapping>> functionMappings = new List<IEnumerable<FunctionMapping>>();

        public ViewTranslator(List<View> views, DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, string targetOwnerName = null)
        {
            this.views = views;
            this.sourceDbInterpreter = sourceDbInterpreter;
            this.targetDbInterpreter = targetDbInterpreter;
            this.targetOwnerName = targetOwnerName;
        }

        public List<View> Translate()
        {
            if (sourceDbInterpreter.DatabaseType == targetDbInterpreter.DatabaseType)
            {
                return views;
            }

            string configRootFolder = Path.Combine(PathHelper.GetAssemblyFolder(), "Config");
            string functionMappingFilePath = Path.Combine(configRootFolder, "FunctionMapping.xml");

            #region DataType Mapping
            string dataTypeMappingFilePath = Path.Combine(configRootFolder, $"DataTypeMapping/{sourceDbInterpreter.DatabaseType.ToString()}2{targetDbInterpreter.DatabaseType.ToString()}.xml");
            XDocument dataTypeMappingDoc = XDocument.Load(dataTypeMappingFilePath);

            this.dataTypeMappings = dataTypeMappingDoc.Root.Elements("mapping").Select(item =>
             new DataTypeMapping()
             {
                 Source = new DataTypeMappingSource(item),
                 Tareget = new DataTypeMappingTarget(item)
             })
             .ToList();
            #endregion

            #region Function Mapping
            XDocument functionMappingDoc = XDocument.Load(functionMappingFilePath);
            this.functionMappings = functionMappingDoc.Root.Elements("mapping").Select(item =>
            item.Elements().Select(t => new FunctionMapping() { DbType = t.Name.ToString(), Function = t.Value }))
            .ToList();
            #endregion  

            this.sourceOwnerName = DbInterpreterHelper.GetOwnerName(sourceDbInterpreter);

            if (string.IsNullOrEmpty(targetOwnerName))
            {
                if (targetDbInterpreter is SqlServerInterpreter)
                {
                    targetOwnerName = "dbo";
                }
                else
                {
                    targetOwnerName = DbInterpreterHelper.GetOwnerName(targetDbInterpreter);
                }
            }

            foreach (View view in views)
            {
                try
                {
                    string ownerNameWithQuotation = $"{targetDbInterpreter.QuotationLeftChar}{targetOwnerName}{targetDbInterpreter.QuotationRightChar}";
                    string viewNameWithQuotation = $"{targetDbInterpreter.QuotationLeftChar}{view.Name}{targetDbInterpreter.QuotationRightChar}";

                    string definition = view.Definition;

                    definition = definition
                               .Replace(sourceDbInterpreter.QuotationLeftChar, '"')
                               .Replace(sourceDbInterpreter.QuotationRightChar, '"')
                               .Replace("<>", "!=")
                               .Replace(">", " > ")
                               .Replace("<", " < ")
                               .Replace("!=", "<>");


                    definition = this.ParseDefinition(definition);

                    string createAsClause = $"CREATE VIEW {targetOwnerName}.{viewNameWithQuotation} AS ";

                    if (!definition.Trim().ToLower().StartsWith("create"))
                    {
                        definition = createAsClause + Environment.NewLine + definition;
                    }
                    else
                    {
                        int asIndex = definition.ToLower().IndexOf("as");
                        definition = createAsClause + definition.Substring(asIndex + 2);
                    }

                    view.Definition = definition;
                }
                catch (Exception ex)
                {
                    throw new ViewConvertException(ex)
                    {
                        SourceServer = sourceDbInterpreter.ConnectionInfo.Server,
                        SourceDatabase = sourceDbInterpreter.ConnectionInfo.Database,
                        SourceObject = view.Name,
                        TargetServer = targetDbInterpreter.ConnectionInfo.Server,
                        TargetDatabase = targetDbInterpreter.ConnectionInfo.Database,
                        TargetObject = view.Name
                    };
                }
            }

            return views;
        }

        private string ParseDefinition(string definition)
        {
            var tokens = this.ParseTokens(definition);
            bool changed = false;

            definition = this.HandleDefinition(definition, tokens, out changed);



            StringBuilder sb = new StringBuilder();

            bool ignore = false;

            if (changed)
            {
                tokens = this.ParseTokens(definition);
            }

            TSQLTokenType previousType = TSQLTokenType.Whitespace;
            string previousText = "";

            for (int i = 0; i < tokens.Count; i++)
            {
                if (ignore)
                {
                    ignore = false;
                    continue;
                }

                var token = tokens[i];

                var tokenType = token.Type;
                string text = token.Text;

                switch (tokenType)
                {
                    case TSQLTokenType.Identifier:

                        if (dataTypes.Contains(text))
                        {
                            sb.Append(text);
                            continue;
                        }

                        var nextToken = i + 1 < tokens.Count ? tokens[i + 1] : null;

                        //Remove owner name
                        if (nextToken != null && nextToken.Text.Trim() != "(" &&
                            text.Trim('"') == sourceOwnerName && i + 1 < tokens.Count && tokens[i + 1].Text == "."
                            )
                        {
                            ignore = true;
                            continue;
                        }
                        else if (nextToken != null && nextToken.Text.Trim() == "(") //function handle
                        {
                            IEnumerable<FunctionMapping> funcMappings = functionMappings.FirstOrDefault(item => item.Any(t => t.DbType == sourceDbInterpreter.DatabaseType.ToString() && t.Function.Split(',').Any(m => m.ToLower() == text.ToLower())));
                            if (funcMappings != null)
                            {
                                string targetFunction = funcMappings.FirstOrDefault(item => item.DbType == targetDbInterpreter.DatabaseType.ToString())?.Function.Split(',')?.FirstOrDefault();

                                if (!string.IsNullOrEmpty(targetFunction))
                                {
                                    sb.Append(targetFunction);
                                }
                            }
                            else
                            {
                                sb.Append(text);
                            }
                        }
                        else
                        {
                            sb.Append($"{targetDbInterpreter.QuotationLeftChar}{text.Trim('"')}{targetDbInterpreter.QuotationRightChar}");
                        }
                        break;
                    case TSQLTokenType.StringLiteral:
                        if (previousType != TSQLTokenType.Whitespace && previousText.ToLower() == "as")
                        {
                            sb.Append($"{targetDbInterpreter.QuotationLeftChar}{text.Trim('\'', '"')}{targetDbInterpreter.QuotationRightChar}");
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
                        sb.Append(token.Text);
                        break;
                    default:
                        sb.Append(token.Text);
                        break;
                }

                if (!string.IsNullOrWhiteSpace(text))
                {
                    previousText = text;
                    previousType = tokenType;
                }

            }

            definition = sb.ToString();

            #region Handle join cluase for mysql which has no "on", so it needs to make up that.
            try
            {
                if (this.sourceDbInterpreter.GetType() == typeof(MySqlInterpreter))
                {
                    bool hasError = false;
                    string formattedDefinition = this.FormatSql(definition, out hasError);

                    if (!hasError)
                    {
                        string[] lines = formattedDefinition.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        Regex joinRegex = new Regex("\\b(join)\\b", RegexOptions.IgnoreCase);
                        Regex onRegex = new Regex("\\b(on)\\b", RegexOptions.IgnoreCase);
                        Regex wordRegex = new Regex("([a-zA-Z(]+)", RegexOptions.IgnoreCase);

                        sb = new StringBuilder();
                        foreach (string line in lines)
                        {
                            bool hasChanged = false;

                            if (joinRegex.IsMatch(line))
                            {
                                string leftStr = line.Substring(line.ToLower().LastIndexOf("join") + 4);
                                if (!onRegex.IsMatch(line) && !wordRegex.IsMatch(leftStr))
                                {
                                    hasChanged = true;
                                    sb.AppendLine($"{line} ON 1=1 ");
                                }
                            }

                            if (!hasChanged)
                            {
                                sb.AppendLine(line);
                            }
                        }

                        definition = sb.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                FeedbackInfo info = new FeedbackInfo() { InfoType = FeedbackInfoType.Error, Message = ExceptionHelper.GetExceptionDetails(ex), Owner = this };
                FeedbackHelper.Feedback(info);
            } 
            #endregion

            return definition.Trim();
        }

        private string HandleDefinition(string definition, List<TSQLToken> tokens, out bool changed)
        {
            changed = false;

            string newDefinition = definition;

            foreach (TSQLToken token in tokens)
            {
                string text = token.Text;

                int startIndex = -1;
                int endIndex = -1;
                int leftBracketCount = 0;
                int rightBracketCount = 0;
                string dataType = "";
                string newDataType = "";

                switch (token.Type)
                {
                    case TSQLTokenType.SystemIdentifier:

                        switch (text.ToUpper())
                        {
                            case "CONVERT":
                                startIndex = token.BeginPosition;
                                endIndex = definition.Substring(startIndex).ToUpper().IndexOf("AS") + startIndex;

                                string functionBody = definition.Substring(startIndex, endIndex - startIndex);

                                leftBracketCount = functionBody.Length - functionBody.Replace("(", "").Length;
                                rightBracketCount = functionBody.Length - functionBody.Replace(")", "").Length;

                                if (leftBracketCount < rightBracketCount)
                                {
                                    int count = 0;
                                    for (int k = 0; k < functionBody.Length; k++)
                                    {
                                        if (functionBody[k] == ')')
                                        {
                                            count++;
                                            if (count == leftBracketCount)
                                            {
                                                functionBody = functionBody.Substring(0, k + 1);
                                                break;
                                            }
                                        }
                                    }
                                }

                                int firstLeftBracketIndex = functionBody.IndexOf('(');
                                int lastRightBracketIndex = functionBody.LastIndexOf(')');

                                string mainBody = functionBody.Substring(firstLeftBracketIndex + 1, lastRightBracketIndex - firstLeftBracketIndex - 1);

                                int firstCommaIndex = mainBody.IndexOf(',');

                                string[] args = new string[2] { mainBody.Substring(0, firstCommaIndex), mainBody.Substring(firstCommaIndex + 1) };

                                string expression = "";
                                dataType = "";

                                if (sourceDbInterpreter is SqlServerInterpreter)
                                {
                                    dataType = args[0];
                                    expression = args[1];
                                }
                                else if (sourceDbInterpreter is MySqlInterpreter)
                                {
                                    dataType = args[1];
                                    expression = args[0];
                                }

                                newDataType = this.GetNewDataType(dataTypeMappings, dataType);

                                if (!string.IsNullOrEmpty(newDataType) && !dataTypes.Contains(dataType))
                                {
                                    dataTypes.Add(newDataType);
                                }

                                string newFunctionBody = "";

                                if (targetDbInterpreter is OracleInterpreter)
                                {
                                    newFunctionBody = $"CAST({expression} AS {newDataType})";
                                }
                                else if
                                (
                                    (sourceDbInterpreter is SqlServerInterpreter || sourceDbInterpreter is MySqlInterpreter)
                                    &&
                                    (targetDbInterpreter is SqlServerInterpreter || targetDbInterpreter is MySqlInterpreter)
                                )
                                {
                                    newFunctionBody = this.ExchangeFunctionArgs(text, newDataType, expression);
                                }

                                newDefinition = newDefinition.Replace(functionBody, newFunctionBody);

                                changed = true;

                                break;
                        }

                        break;

                    case TSQLTokenType.Identifier:
                        switch (text.ToUpper())
                        {
                            case "CAST":
                                startIndex = token.BeginPosition;
                                int asIndex = startIndex + definition.Substring(startIndex).ToUpper().IndexOf(" AS ");

                                string arg = definition.Substring(token.EndPosition + 1, asIndex - startIndex - 3);

                                int functionEndIndex = -1;
                                for (int i = asIndex + 3; i < definition.Length; i++)
                                {
                                    if (definition[i] == '(')
                                    {
                                        leftBracketCount++;
                                    }
                                    else if (definition[i] == ')')
                                    {
                                        rightBracketCount++;
                                    }

                                    if (rightBracketCount - leftBracketCount == 1)
                                    {
                                        dataType = definition.Substring(asIndex + 4, i - asIndex - 4);
                                        functionEndIndex = i;
                                        break;
                                    }
                                }

                                newDataType = this.GetNewDataType(dataTypeMappings, dataType);

                                if (!string.IsNullOrEmpty(newDataType) && !dataTypes.Contains(dataType))
                                {
                                    dataTypes.Add(newDataType);
                                }

                                newDefinition = newDefinition.Replace(dataType, newDataType);

                                changed = true;

                                break;
                        }

                        break;
                }
            }

            return newDefinition;
        }

        private string ExchangeFunctionArgs(string functionName, string args1, string args2)
        {
            string newFunctionBody = $"{functionName}({args2},{args1})";

            return newFunctionBody;
        }

        private DataTypeMapping GetDataTypeMapping(List<DataTypeMapping> mappings, string dataType)
        {
            return mappings.FirstOrDefault(item => item.Source.Type?.ToLower() == dataType?.ToLower());
        }

        private string GetNewDataType(List<DataTypeMapping> mappings, string dataType)
        {
            DatabaseType sourceDbType = this.sourceDbInterpreter.DatabaseType;
            DatabaseType targetDbType = this.targetDbInterpreter.DatabaseType;

            string cleanDataType = dataType.Split('(')[0];
            string newDataType = cleanDataType;
            bool hasPrecisionScale = false;

            if (cleanDataType != dataType)
            {
                hasPrecisionScale = true;
            }

            string upperTypeName = newDataType.ToUpper();

            DataTypeMapping mapping = this.GetDataTypeMapping(mappings, cleanDataType);
            if (mapping != null)
            {
                DataTypeMappingTarget targetDataType = mapping.Tareget;
                newDataType = targetDataType.Type;

                if (targetDbType == DatabaseType.MySql)
                {
                    if (upperTypeName == "INT")
                    {
                        newDataType = "SIGNED";
                    }
                    else if (upperTypeName == "FLOAT" || upperTypeName == "DOUBLE" || upperTypeName == "NUMBER")
                    {
                        newDataType = "DECIMAL";
                    }
                }

                if (!hasPrecisionScale && !string.IsNullOrEmpty(targetDataType.Precision) && !string.IsNullOrEmpty(targetDataType.Scale))
                {
                    newDataType += $"({targetDataType.Precision},{targetDataType.Scale})";
                }
                else if (hasPrecisionScale)
                {
                    newDataType += "(" + dataType.Split('(')[1];
                }
            }
            else
            {
                if (sourceDbType == DatabaseType.MySql)
                {
                    if (upperTypeName == "SIGNED")
                    {
                        if (targetDbType == DatabaseType.SqlServer)
                        {
                            newDataType = "DECIMAL";
                        }
                        else if (targetDbType == DatabaseType.Oracle)
                        {
                            newDataType = "NUMBER";
                        }
                    }
                }
            }

            return newDataType;
        }

        private List<TSQL.Tokens.TSQLToken> ParseTokens(string sql)
        {
            return TSQLTokenizer.ParseTokens(sql, true, true);
        }

        private string FormatSql(string sql, out bool hasError)
        {
            hasError = false;

            SqlFormattingManager manager = new SqlFormattingManager();            
            string formattedSql = manager.Format(sql, ref hasError);
            return formattedSql;
        }
    }
}
