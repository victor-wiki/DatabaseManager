using DatabaseConverter.Core.Functions;
using DatabaseConverter.Core.Model;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using PoorMansTSqlFormatterRedux;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public abstract class DbObjectTranslator:IDisposable
    {
        private IObserver<FeedbackInfo> observer;
        protected string sourceSchemaName;
        protected DbInterpreter sourceDbInterpreter;
        protected DbInterpreter targetDbInterpreter;
        protected DatabaseType sourceDbType;
        protected DatabaseType targetDbType;
        protected List<DataTypeMapping> dataTypeMappings = null;
        protected List<IEnumerable<FunctionMapping>> functionMappings = null;
        protected List<IEnumerable<VariableMapping>> variableMappings = null;
        protected bool hasError = false;

        public bool ContinueWhenErrorOccurs { get; set; }
        public bool HasError => this.hasError;
        public SchemaInfo SourceSchemaInfo { get; set; }
        public DbConverterOption Option { get; set; }
        public List<UserDefinedType> UserDefinedTypes { get; set; } = new List<UserDefinedType>();

        public List<TranslateResult> TranslateResults { get; internal set; } = new List<TranslateResult>();

        public DbObjectTranslator(DbInterpreter source, DbInterpreter target)
        {
            this.sourceDbInterpreter = source;
            this.targetDbInterpreter = target;
            this.sourceDbType = source.DatabaseType;
            this.targetDbType = target.DatabaseType;
        }

        public void LoadMappings()
        {
            if (this.sourceDbInterpreter.DatabaseType != this.targetDbInterpreter.DatabaseType)
            {
                this.functionMappings = FunctionMappingManager.FunctionMappings;
                this.variableMappings = VariableMappingManager.VariableMappings;
                this.dataTypeMappings = DataTypeMappingManager.GetDataTypeMappings(this.sourceDbInterpreter.DatabaseType, this.targetDbInterpreter.DatabaseType);
            }
        }

        public abstract void Translate();

        public DataTypeMapping GetDataTypeMapping(List<DataTypeMapping> mappings, string dataType)
        {
            return mappings.FirstOrDefault(item => item.Source.Type?.ToLower() == dataType?.ToLower());
        }

        internal string GetNewDataType(List<DataTypeMapping> mappings, string dataType, bool usedForFunction = true)
        {
            dataType = this.GetTrimedValue(dataType.Trim());
            DataTypeInfo dataTypeInfo = this.sourceDbInterpreter.GetDataTypeInfo(dataType);

            string upperDataTypeName = dataTypeInfo.DataType.ToUpper();

            if (this.sourceDbType == DatabaseType.MySql)
            {
                if (upperDataTypeName == "SIGNED")
                {
                    if (this.targetDbType == DatabaseType.SqlServer)
                    {
                        return "DECIMAL";
                    }
                    else if (this.targetDbType == DatabaseType.Postgres)
                    {
                        return "NUMERIC";
                    }
                    else if (this.targetDbType == DatabaseType.Oracle)
                    {
                        return "NUMBER";
                    }
                }
            }

            if (this.targetDbType == DatabaseType.Oracle)
            {
                if (usedForFunction && this.GetType() == typeof(FunctionTranslator) && dataTypeInfo.Args?.ToLower() == "max")
                {
                    var mappedDataType = this.GetDataTypeMapping(mappings, dataTypeInfo.DataType)?.Target?.Type;

                    bool isChar = DataTypeHelper.IsCharType(mappedDataType);
                    bool isBinary = DataTypeHelper.IsBinaryType(mappedDataType);

                    if (isChar || isBinary)
                    {
                        var dataTypeSpec = this.targetDbInterpreter.GetDataTypeSpecification(mappedDataType);

                        if (dataTypeSpec != null)
                        {
                            ArgumentRange? range = DataTypeManager.GetArgumentRange(dataTypeSpec, "length");

                            if (range.HasValue)
                            {
                                return $"{mappedDataType}({range.Value.Max})";
                            }
                        }
                    }
                }
            }

            var trimChars = TranslateHelper.GetTrimChars(this.sourceDbInterpreter, this.targetDbInterpreter).ToArray();

            TableColumn column = TranslateHelper.SimulateTableColumn(this.sourceDbInterpreter, dataType, this.Option, this.UserDefinedTypes, trimChars);

            DataTypeTranslator dataTypeTranslator = new DataTypeTranslator(this.sourceDbInterpreter, this.targetDbInterpreter);
            dataTypeTranslator.Option = this.Option;

            TranslateHelper.TranslateTableColumnDataType(dataTypeTranslator, column);

            string newDataTypeName = column.DataType;
            string newDataType = null;

            if (usedForFunction)
            {
                if (this.targetDbType == DatabaseType.MySql)
                {
                    var ndt = this.GetMySqlNewDataType(newDataTypeName);

                    if (ndt != newDataTypeName)
                    {
                        newDataType = ndt;
                    }
                }
                else if (targetDbType == DatabaseType.Postgres)
                {
                    if (DataTypeHelper.IsBinaryType(newDataTypeName))
                    {
                        return newDataTypeName;
                    }
                }
            }

            if (string.IsNullOrEmpty(newDataType))
            {
                newDataType = this.targetDbInterpreter.ParseDataType(column);
            }

            return newDataType;
        }

        private string GetMySqlNewDataType(string dataTypeName)
        {
            string upperTypeName = dataTypeName.ToUpper();

            if (upperTypeName.Contains("INT") || upperTypeName == "BIT")
            {
                return "SIGNED";
            }
            else if (upperTypeName == "NUMBER")
            {
                return "DOUBLE";
            }            
            else if (DataTypeHelper.IsCharType(dataTypeName) || upperTypeName.Contains("TEXT"))
            {
                return "CHAR";
            }
            else if (DataTypeHelper.IsDateOrTimeType(dataTypeName))
            {
                return "DATETIME";
            }
            else if (DataTypeHelper.IsBinaryType(dataTypeName))
            {
                return "BINARY";
            }

            return dataTypeName;
        }

        private string GetTrimedValue(string value)
        {
            return value?.Trim(this.sourceDbInterpreter.QuotationLeftChar, this.sourceDbInterpreter.QuotationRightChar);
        }

        public static string ReplaceValue(string source, string oldValue, string newValue, RegexOptions option = RegexOptions.IgnoreCase)
        {
            return RegexHelper.Replace(source, oldValue, newValue, option);
        }

        public string ExchangeFunctionArgs(string functionName, string args1, string args2)
        {
            if (functionName.ToUpper() == "CONVERT" && this.targetDbInterpreter.DatabaseType == DatabaseType.MySql && args1.ToUpper().Contains("DATE"))
            {
                if (args2.Contains(','))
                {
                    args2 = args2.Split(',')[0];
                }
            }

            string newExpression = $"{functionName}({args2},{args1})";

            return newExpression;
        }

        public string ReplaceVariables(string script, List<IEnumerable<VariableMapping>> mappings)
        {
            foreach (IEnumerable<VariableMapping> mapping in mappings)
            {
                VariableMapping sourceVariable = mapping.FirstOrDefault(item => item.DbType == this.sourceDbInterpreter.DatabaseType.ToString());
                VariableMapping targetVariable = mapping.FirstOrDefault(item => item.DbType == this.targetDbInterpreter.DatabaseType.ToString());

                if (sourceVariable != null && !string.IsNullOrEmpty(sourceVariable.Variable) 
                    && targetVariable!= null && targetVariable.Variable != null && !string.IsNullOrEmpty(targetVariable.Variable)
                   )
                {
                    script = ReplaceValue(script, sourceVariable.Variable, targetVariable.Variable);
                }
            }

            return script;
        }

        public string ParseFormula(List<FunctionSpecification> sourceFuncSpecs, List<FunctionSpecification> targetFuncSpecs,
            FunctionFormula formula, MappingFunctionInfo targetFunctionInfo, out Dictionary<string, string> dictDataType, RoutineType routineType = RoutineType.UNKNOWN)
        {
            dictDataType = new Dictionary<string, string>();

            string name = formula.Name;

            if (!string.IsNullOrEmpty(targetFunctionInfo.Args) && targetFunctionInfo.IsFixedArgs)
            {
                return $"{targetFunctionInfo.Name}({targetFunctionInfo.Args})";
            }

            FunctionSpecification sourceFuncSpec = sourceFuncSpecs.FirstOrDefault(item => item.Name.ToUpper() == name.ToUpper());
            FunctionSpecification targetFuncSpec = targetFuncSpecs.FirstOrDefault(item => item.Name.ToUpper() == targetFunctionInfo.Name.ToUpper());

            string newExpression = formula.Expression;

            if (sourceFuncSpec != null && !string.IsNullOrEmpty(targetFunctionInfo.Translator))
            {
                Type type = Type.GetType($"{typeof(SpecificFunctionTranslatorBase).Namespace}.{targetFunctionInfo.Translator}");

                SpecificFunctionTranslatorBase translator = (SpecificFunctionTranslatorBase)Activator.CreateInstance(type, new object[] { sourceFuncSpec, targetFuncSpec });

                translator.SourceDbType = this.sourceDbType;
                translator.TargetDbType = this.targetDbType;

                newExpression = translator.Translate(formula);

                return newExpression;
            }

            Dictionary<string, string> dataTypeDict = new Dictionary<string, string>();

            if (sourceFuncSpec != null)
            {
                string delimiter = sourceFuncSpec.Delimiter.Length == 1 ? sourceFuncSpec.Delimiter : $" {sourceFuncSpec.Delimiter} ";

                List<string> formulaArgs = formula.GetArgs(delimiter);

                List<FunctionArgumentItemInfo> sourceArgItems = GetFunctionArgumentTokens(sourceFuncSpec, null);
                List<FunctionArgumentItemInfo> targetArgItems = targetFuncSpec == null ? null : GetFunctionArgumentTokens(targetFuncSpec, targetFunctionInfo.Args);

                bool ignore = false;

                if (targetArgItems == null || (formulaArgs.Count > 0 && (targetArgItems == null || targetArgItems.Count == 0 || sourceArgItems.Count == 0)))
                {
                    ignore = true;
                }

                Func<FunctionArgumentItemInfo, string, string> getSourceArg = (source, content) =>
                {
                    int sourceIndex = source.Index;

                    if (formulaArgs.Count > sourceIndex)
                    {
                        string oldArg = formulaArgs[sourceIndex];
                        string newArg = oldArg;

                        switch (content.ToUpper())
                        {
                            case "TYPE":

                                if (!dataTypeDict.ContainsKey(oldArg))
                                {
                                    newArg = this.GetNewDataType(this.dataTypeMappings, oldArg, true);

                                    dataTypeDict.Add(oldArg, newArg.Trim());
                                }
                                else
                                {
                                    newArg = dataTypeDict[oldArg];
                                }

                                break;

                            case "DATE":
                            case "DATE1":
                            case "DATE2":

                                newArg = DatetimeHelper.DecorateDatetimeString(this.targetDbType, newArg);

                                break;
                            case "UNIT":
                            case "'UNIT'":

                                newArg = DatetimeHelper.GetMappedUnit(sourceDbType, targetDbType, oldArg);                                

                                break;
                        }

                        return this.GetFunctionValue(targetFuncSpecs, newArg);
                    }

                    return string.Empty;
                };

                Func<string, string> getTrimedContent = (content) =>
                {
                    return content.Trim('\'');
                };

                Func<string, bool> isQuoted = (content) =>
                {
                    return content.StartsWith('\'');
                };

                Dictionary<string, string> defaults = this.GetFunctionDefaults(targetFunctionInfo);
                string targetFunctionName = targetFunctionInfo.Name;

                if (this.sourceDbInterpreter.DatabaseType == DatabaseType.Postgres)
                {
                    if (name == "TRIM" && formulaArgs.Count > 1)
                    {
                        switch (formulaArgs[0])
                        {
                            case "LEADING":
                                targetFunctionName = "LTRIM";
                                break;
                            case "TRAILING":
                                targetFunctionName = "RTRIM";
                                break;
                        }
                    }
                }

                if (!ignore)
                {
                    StringBuilder sbArgs = new StringBuilder();

                    foreach (FunctionArgumentItemInfo tai in targetArgItems)
                    {
                        if (tai.Index > 0)
                        {
                            sbArgs.Append(targetFuncSpec.Delimiter == "," ? "," : $" {targetFuncSpec.Delimiter} ");
                        }

                        string content = tai.Content;
                        string trimedContent = getTrimedContent(content);
                   
                        var sourceItem = sourceArgItems.FirstOrDefault(item => getTrimedContent(item.Content) == trimedContent);

                        if (sourceItem != null)
                        {
                            string value = getSourceArg(sourceItem, content);

                            if (!string.IsNullOrEmpty(value))
                            {
                                if (isQuoted(sourceItem.Content) && !isQuoted(content))
                                {
                                    value = getTrimedContent(value);
                                }

                                if(content.StartsWith('\''))
                                {
                                    sbArgs.Append('\'');
                                }

                                sbArgs.Append(value);

                                if (content.EndsWith('\''))
                                {
                                    sbArgs.Append('\'');
                                }                                
                            }
                            else
                            {
                                string defaultValue = this.GetFunctionDictionaryValue(defaults, content);

                                if (!string.IsNullOrEmpty(defaultValue))
                                {
                                    sbArgs.Append(defaultValue);
                                }
                            }
                        }
                        else if (sourceArgItems.Any(item => item.Details.Any(t => getTrimedContent(t.Content) == trimedContent)))
                        {
                            var sd = sourceArgItems.FirstOrDefault(item => item.Details.Any(t => getTrimedContent(t.Content) == trimedContent));

                            var details = sd.Details;

                            if (formulaArgs.Count > sd.Index)
                            {
                                var args = formulaArgs[sd.Index].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                                if (details.Where(item => item.Type != FunctionArgumentItemDetailType.Whitespace).Count() == args.Length)
                                {
                                    int i = 0;
                                    foreach (var detail in details)
                                    {
                                        if (detail.Type != FunctionArgumentItemDetailType.Whitespace)
                                        {
                                            if (getTrimedContent(detail.Content) == trimedContent)
                                            {
                                                sbArgs.Append(args[i]);
                                                break;
                                            }

                                            i++;
                                        }
                                    }
                                }
                            }
                        }
                        else if (content == "START")
                        {
                            sbArgs.Append("0");
                        }                        
                        else if (tai.Details.Count > 0)
                        {
                            foreach (FunctionArgumentItemDetailInfo detail in tai.Details)
                            {
                                string dc = detail.Content;
                                string trimedDc = getTrimedContent(dc);                   

                                var si = sourceArgItems.FirstOrDefault(item => getTrimedContent(item.Content) == trimedDc);

                                if (si != null)
                                {
                                    string value = getSourceArg(si, detail.Content);

                                    if (isQuoted(si.Content) && !isQuoted(dc))
                                    {
                                        value = getTrimedContent(value);
                                    }

                                    if (dc.StartsWith('\''))
                                    {
                                        sbArgs.Append('\'');
                                    }

                                    sbArgs.Append(value);

                                    if (dc.EndsWith('\''))
                                    {
                                        sbArgs.Append('\'');
                                    }
                                }
                                else
                                {
                                    sbArgs.Append(detail.Content);
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(targetFunctionInfo.Args))
                        {
                            sbArgs.Append(content);
                        }
                        else if (defaults.ContainsKey(content))
                        {
                            sbArgs.Append(defaults[content]);
                        }
                        else
                        {
                            sbArgs.Append(content);
                        }
                    }

                    #region Oracle: use TO_CHAR instead of CAST(xxx as varchar2(n)) 
                    if (this.targetDbType == DatabaseType.Oracle)
                    {
                        if (targetFunctionName == "CAST")
                        {
                            string[] items = sbArgs.ToString().Split("AS");
                            string dataType = items.LastOrDefault().Trim();

                            if (DataTypeHelper.IsCharType(dataType))
                            {
                                if (routineType == RoutineType.PROCEDURE || routineType == RoutineType.FUNCTION || routineType == RoutineType.TRIGGER)
                                {
                                    targetFunctionName = "TO_CHAR";
                                    sbArgs.Clear();
                                    sbArgs.Append(items[0].Trim());
                                }
                            }
                        }
                    }
                    #endregion

                    newExpression = $"{targetFunctionName}{(targetFuncSpec.NoParenthesess ? "" : $"({sbArgs.ToString()})")}";
                }
                else
                {
                    if (!string.IsNullOrEmpty(targetFunctionInfo.Expression))
                    {
                        string expression = targetFunctionInfo.Expression;

                        foreach (FunctionArgumentItemInfo sourceItem in sourceArgItems)
                        {
                            string value = getSourceArg(sourceItem, sourceItem.Content);

                            if (string.IsNullOrEmpty(value))
                            {
                                string defaultValue = this.GetFunctionDictionaryValue(defaults, sourceItem.Content);

                                if (!string.IsNullOrEmpty(defaultValue))
                                {
                                    value = defaultValue;
                                }
                            }

                            expression = expression.Replace(sourceItem.Content, value);                            
                        }                       

                        newExpression = expression;
                    }
                }

                var replacements = this.GetFunctionStringDictionary(targetFunctionInfo.Replacements);

                foreach (var replacement in replacements)
                {
                    newExpression = newExpression.Replace(replacement.Key, replacement.Value);
                }
            }

            dictDataType = dataTypeDict;

            return newExpression;
        }

        private string GetFunctionDictionaryValue(Dictionary<string, string> values, string arg)
        {
            if (values.ContainsKey(arg))
            {
                return values[arg];
            }

            return null;
        }

        private Dictionary<string, string> GetFunctionDefaults(MappingFunctionInfo targetFunctionInfo)
        {
            return this.GetFunctionStringDictionary(targetFunctionInfo.Defaults);
        }

        private Dictionary<string, string> GetFunctionReplacements(MappingFunctionInfo targetFunctionInfo)
        {
            return this.GetFunctionStringDictionary(targetFunctionInfo.Replacements);
        }

        private Dictionary<string, string> GetFunctionStringDictionary(string content)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();            

            if (!string.IsNullOrEmpty(content))
            {
                string[] items = content.Split(';');

                foreach (string item in items)
                {
                    string[] subItems = item.Split(':');

                    if (subItems.Length == 2)
                    {
                        string key = subItems[0];
                        string value = subItems[1];

                        if (!dict.ContainsKey(key))
                        {
                            dict.Add(key, value);
                        }
                    }
                }
            }

            return dict;
        }

        private string GetFunctionValue(List<FunctionSpecification> targetFuncSpecs, string value)
        {
            FunctionSpecification targetFuncSpec = targetFuncSpecs.FirstOrDefault(item => item.Name.ToUpper() == value.TrimEnd('(', ')').ToUpper());

            if (targetFuncSpec != null)
            {
                if (targetFuncSpec.NoParenthesess && value.EndsWith("()"))
                {
                    value = value.Substring(0, value.Length - 2);
                }
            }

            return value;
        }

        internal static List<FunctionArgumentItemInfo> GetFunctionArgumentTokens(FunctionSpecification spec, string functionArgs)
        {
            List<FunctionArgumentItemInfo> itemInfos = new List<FunctionArgumentItemInfo>();

            string specArgs = string.IsNullOrEmpty(functionArgs) ? spec.Args : functionArgs;

            if (!specArgs.EndsWith("..."))
            {
                string str = Regex.Replace(specArgs, @"[\[\]]", "");

                string[] items = str.Split(new string[] { spec.Delimiter }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < items.Length; i++)
                {
                    string item = items[i].Trim();

                    FunctionArgumentItemInfo itemInfo = new FunctionArgumentItemInfo() { Index = i, Content = item };

                    if (item.Contains(" "))
                    {
                        string[] details = item.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        for (int j = 0; j < details.Length; j++)
                        {
                            if (j > 0)
                            {
                                itemInfo.Details.Add(new FunctionArgumentItemDetailInfo() { Type = FunctionArgumentItemDetailType.Whitespace, Content = " " });
                            }

                            FunctionArgumentItemDetailInfo detail = new FunctionArgumentItemDetailInfo() { Type = FunctionArgumentItemDetailType.Text, Content = details[j] };

                            itemInfo.Details.Add(detail);
                        }
                    }

                    itemInfos.Add(itemInfo);
                }
            }

            return itemInfos;
        }

        public MappingFunctionInfo GetMappingFunctionInfo(string name, string args, out bool useBrackets)
        {
            useBrackets = false;

            string text = name;
            string textWithBrackets = name.ToLower() + "()";

            if (this.functionMappings.Any(item => item.Any(t => t.Function.ToLower() == textWithBrackets)))
            {
                text = textWithBrackets;
                useBrackets = true;
            }

            string targetFunctionName = name;

            MappingFunctionInfo functionInfo = new MappingFunctionInfo() { Name = name };

            IEnumerable<FunctionMapping> funcMappings = this.functionMappings.FirstOrDefault(item =>
            item.Any(t =>
             (t.Direction == FunctionMappingDirection.OUT || t.Direction == FunctionMappingDirection.INOUT)
              && t.DbType == sourceDbInterpreter.DatabaseType.ToString()
              && t.Function.Split(',').Any(m => m.ToLower() == text.ToLower())
             )
            );

            if (funcMappings != null)
            {
                FunctionMapping mapping = funcMappings.FirstOrDefault(item =>
                        (item.Direction == FunctionMappingDirection.IN || item.Direction == FunctionMappingDirection.INOUT)
                        && item.DbType == targetDbInterpreter.DatabaseType.ToString());

                if (mapping != null)
                {
                    bool matched = true;

                    if (!string.IsNullOrEmpty(args) && !string.IsNullOrEmpty(mapping.Args))
                    {
                        if (mapping.IsFixedArgs && args.Trim().ToLower() != mapping.Args.Trim().ToLower())
                        {
                            matched = false;
                        }
                    }

                    if (matched)
                    {
                        functionInfo.Name = mapping.Function.Split(',')?.FirstOrDefault();
                        functionInfo.Args = mapping.Args;
                        functionInfo.IsFixedArgs = mapping.IsFixedArgs;
                        functionInfo.Expression = mapping.Expression;
                        functionInfo.Defaults = mapping.Defaults;
                        functionInfo.Translator = mapping.Translator;
                        functionInfo.Specials = mapping.Specials;
                        functionInfo.Replacements = mapping.Replacements;
                    }
                }
            }

            return functionInfo;
        }

        public string FormatSql(string sql, out bool hasError)
        {
            hasError = false;

            SqlFormattingManager manager = new SqlFormattingManager();

            string formattedSql = manager.Format(sql, ref hasError);

            return formattedSql;
        }

        protected string GetTrimedName(string name)
        {
            return name?.Trim(this.sourceDbInterpreter.QuotationLeftChar, this.sourceDbInterpreter.QuotationRightChar, this.targetDbInterpreter.QuotationLeftChar, this.targetDbInterpreter.QuotationRightChar, '"');
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public void Feedback(FeedbackInfoType infoType, string message, bool skipError = false)
        {
            FeedbackInfo info = new FeedbackInfo() { Owner = this, InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(message), IgnoreError = skipError };

            if (this.observer != null)
            {
                FeedbackHelper.Feedback(this.observer, info);
            }
        }

        public void FeedbackInfo(string message)
        {
            this.Feedback(FeedbackInfoType.Info, message);
        }
        public void FeedbackError(string message, bool skipError = false)
        {
            this.Feedback(FeedbackInfoType.Error, message, skipError);
        }

        public void Dispose()
        {
            this.TranslateResults.Clear();
        }
    }
}
