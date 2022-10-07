using DatabaseConverter.Core.Model;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using PoorMansTSqlFormatterRedux;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public abstract class DbObjectTranslator
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

        public TranslateHandler OnTranslated;

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
            dataType = dataType.Trim();

            DatabaseType sourceDbType = this.sourceDbInterpreter.DatabaseType;
            DatabaseType targetDbType = this.targetDbInterpreter.DatabaseType;

            string cleanDataType = dataType.Split('(')[0].Trim(this.sourceDbInterpreter.QuotationLeftChar, this.sourceDbInterpreter.QuotationRightChar);
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
                DataTypeMappingTarget targetDataType = mapping.Target;
                newDataType = targetDataType.Type;

                if (usedForFunction)
                {
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
                        else if (DataTypeHelper.IsCharType(newDataType))
                        {
                            newDataType = "CHAR";
                        }
                    }
                    else if (targetDbType == DatabaseType.Oracle)
                    {
                        if (DataTypeHelper.IsCharType(newDataType) && this.GetType() == typeof(FunctionTranslator))
                        {
                            return newDataType;
                        }
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
                if (usedForFunction)
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
            }

            return newDataType;
        }

        public static string ReplaceValue(string source, string oldValue, string newValue, RegexOptions option = RegexOptions.IgnoreCase)
        {
            return Regex.Replace(source, Regex.Escape(oldValue), newValue, option);
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

                if (sourceVariable != null && !string.IsNullOrEmpty(sourceVariable.Variable) && targetVariable.Variable != null && !string.IsNullOrEmpty(targetVariable.Variable))
                {
                    script = ReplaceValue(script, sourceVariable.Variable, targetVariable.Variable);
                }
            }

            return script;
        }

        public string ParseFomular(List<FunctionSpecification> sourceFuncSpecs, List<FunctionSpecification> targetFuncSpecs,
            FunctionFomular fomular, MappingFunctionInfo targetFunctionInfo, out Dictionary<string, string> dictDataType)
        {
            dictDataType = new Dictionary<string, string>();

            string name = fomular.Name;

            if (!string.IsNullOrEmpty(targetFunctionInfo.Args) && !targetFunctionInfo.Args.Contains("EXP"))
            {
                return $"{targetFunctionInfo.Name}({targetFunctionInfo.Args})";
            }           

            FunctionSpecification sourceFuncSpec = sourceFuncSpecs.FirstOrDefault(item => item.Name.ToUpper() == name.ToUpper());
            FunctionSpecification targetFuncSpec = targetFuncSpecs.FirstOrDefault(item => item.Name.ToUpper() == targetFunctionInfo.Name.ToUpper());

            string newExpression = fomular.Expression;

            if (sourceFuncSpec != null && targetFuncSpec != null)
            {
                string delimiter = sourceFuncSpec.Delimiter == "," ? "," : $" {sourceFuncSpec.Delimiter} ";
                fomular.Delimiter = delimiter;

                List<string> fomularArgs = fomular.Args;

                int fetchCount = string.IsNullOrEmpty(targetFunctionInfo.Args) ? fomularArgs.Count : -1;

                Dictionary<int, string> sourceTokens = GetFunctionArgumentTokens(sourceFuncSpec, null, fetchCount);

                int targetArgsLength = targetFuncSpec.Args.Split(new string[] { targetFuncSpec.Delimiter }, StringSplitOptions.RemoveEmptyEntries).Length;

                Dictionary<int, string> targetTokens = GetFunctionArgumentTokens(targetFuncSpec, targetFunctionInfo.Args, fetchCount < targetArgsLength ? targetArgsLength : fetchCount);

                bool ignore = false;

                if (fomularArgs.Count > 0 && (targetTokens.Count == 0 || sourceTokens.Count == 0))
                {
                    ignore = true;
                }

                if (!ignore)
                {
                    List<string> args = new List<string>();

                    foreach (var kp in targetTokens)
                    {
                        int targetIndex = kp.Key;
                        string token = kp.Value;

                        if (sourceTokens.ContainsValue(token))
                        {
                            int sourceIndex = sourceTokens.FirstOrDefault(item => item.Value == token).Key;

                            if (fomularArgs.Count > sourceIndex)
                            {
                                string oldArg = fomular.Args[sourceIndex];
                                string newArg = oldArg;

                                switch (token.ToUpper())
                                {
                                    case "TYPE":

                                        if (!dictDataType.ContainsKey(oldArg))
                                        {
                                            newArg = this.GetNewDataType(this.dataTypeMappings, oldArg);

                                            dictDataType.Add(oldArg, newArg.Trim());
                                        }
                                        else
                                        {
                                            newArg = dictDataType[oldArg];
                                        }
                                        break;
                                }

                                args.Add(newArg);
                            }
                        }
                        else if (!string.IsNullOrEmpty(targetFunctionInfo.Args))
                        {
                            args.Add(token);
                        }
                        else if (token == "STR")
                        {
                            args.Add("''");
                        }
                        else if(token == "START")
                        {
                            args.Add("0");
                        }
                    }

                    string targetDelimiter = targetFuncSpec.Delimiter == "," ? "," : $" {targetFuncSpec.Delimiter} ";

                    string strArgs = string.Join(targetDelimiter, args);
                    string targetFunctionName = targetFunctionInfo.Name;

                    if (this.sourceDbInterpreter.DatabaseType == DatabaseType.Postgres)
                    {
                        if (name == "TRIM" && fomularArgs.Count>1)
                        {
                            switch(fomularArgs[0])
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

                    newExpression = $"{targetFunctionName}{(targetFuncSpec.NoParenthesess ? "" : $"({strArgs})")}";
                }
            }

            return newExpression;
        }

        internal static Dictionary<int, string> GetFunctionArgumentTokens(FunctionSpecification spec, string functionArgs, int fetchCount = -1)
        {
            Dictionary<int, string> dictTokenIndex = new Dictionary<int, string>();

            string specArgs = string.IsNullOrEmpty(functionArgs) ? spec.Args : functionArgs;

            if (!specArgs.EndsWith("..."))
            {
                string str = Regex.Replace(specArgs, @"[\[\]]", "");

                string[] args = str.Split(new string[] { spec.Delimiter, " " }, StringSplitOptions.RemoveEmptyEntries);

                int num = (fetchCount > 0 && args.Length > fetchCount) ? fetchCount : args.Length;

                for (int i = 0; i < num; i++)
                {
                    string arg = args[i];

                    dictTokenIndex.Add(i, arg.Trim());
                }
            }

            return dictTokenIndex;
        }

        public MappingFunctionInfo GetMappingFunctionInfo(string name, out bool useBrackets)
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

            IEnumerable<FunctionMapping> funcMappings = this.functionMappings.FirstOrDefault(item => item.Any(t =>
             (t.Direction == FunctionMappingDirection.OUT || t.Direction == FunctionMappingDirection.INOUT)
              && t.DbType == sourceDbInterpreter.DatabaseType.ToString() && t.Function.Split(',').Any(m => m.ToLower() == text.ToLower())));

            if (funcMappings != null)
            {
                FunctionMapping mapping = funcMappings.FirstOrDefault(item =>
                        (item.Direction == FunctionMappingDirection.IN || item.Direction == FunctionMappingDirection.INOUT)
                        && item.DbType == targetDbInterpreter.DatabaseType.ToString());

                if (mapping != null)
                {
                    functionInfo.Name = mapping.Function.Split(',')?.FirstOrDefault();
                    functionInfo.Args = mapping.Args;
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
    }
}
