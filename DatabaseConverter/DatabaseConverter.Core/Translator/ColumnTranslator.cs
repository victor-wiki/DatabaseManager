using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class ColumnTranslator : DbObjectTokenTranslator
    {
        private IEnumerable<TableColumn> columns;
        private IEnumerable<DataTypeSpecification> sourceDataTypeSpecs;
        private DataTypeTranslator dataTypeTranslator;
        private FunctionTranslator functionTranslator;
        private SequenceTranslator sequenceTranslator;
        private List<FunctionSpecification> targetFuncSpecs;
        public List<TableColumn> ExistedTableColumns { get; set; }

        public ColumnTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter, IEnumerable<TableColumn> columns) : base(sourceInterpreter, targetInterpreter)
        {
            this.columns = columns;
            this.sourceDataTypeSpecs = DataTypeManager.GetDataTypeSpecifications(this.sourceDbType);
            this.functionTranslator = new FunctionTranslator(this.sourceDbInterpreter, this.targetDbInterpreter);
            this.dataTypeTranslator = new DataTypeTranslator(this.sourceDbInterpreter, this.targetDbInterpreter);
            this.sequenceTranslator = new SequenceTranslator(this.sourceDbInterpreter, this.targetDbInterpreter);
            this.targetFuncSpecs = FunctionManager.GetFunctionSpecifications(this.targetDbType);
        }

        public override void Translate()
        {
            if (this.sourceDbType == this.targetDbType)
            {
                return;
            }

            if (this.hasError)
            {
                return;
            }

            if (!this.columns.Any())
            {
                return;
            }

            this.FeedbackInfo("Begin to translate columns.");

            this.LoadMappings();
            this.functionTranslator.LoadMappings();
            this.functionTranslator.LoadFunctionSpecifications();
            this.sequenceTranslator.Option = this.Option;
            this.dataTypeTranslator.Option = this.Option;

            bool dataModeOnly = this.Option.GenerateScriptMode == GenerateScriptMode.Data;

            if (!dataModeOnly)
            {
                this.CheckComputeExpression();
            }

            foreach (TableColumn column in this.columns)
            {
                TableColumn existedColumn = this.GetExistedColumn(column);

                if (existedColumn != null)
                {
                    column.DataType = existedColumn.DataType;
                }
                else
                {
                    if (!DataTypeHelper.IsUserDefinedType(column))
                    {
                        TranslateHelper.TranslateTableColumnDataType(this.dataTypeTranslator, column);
                    }
                }                

                if (!dataModeOnly)
                {
                    if (!string.IsNullOrEmpty(column.DefaultValue))
                    {
                        this.ConvertDefaultValue(column);
                    }

                    if (column.IsComputed)
                    {
                        this.ConvertComputeExpression(column);
                    }
                }
            }

            this.FeedbackInfo("End translate columns.");
        }

        private TableColumn GetExistedColumn(TableColumn column)
        {
            if (this.ExistedTableColumns == null || this.ExistedTableColumns.Count == 0)
            {
                return null;
            }

            return this.ExistedTableColumns.FirstOrDefault(item => SchemaInfoHelper.IsSameTableColumnIgnoreCase(item, column));
        }

        public void ConvertDefaultValue(TableColumn column)
        {
            string defaultValue = StringHelper.GetBalanceParenthesisTrimedValue(column.DefaultValue);
            bool hasParenthesis = defaultValue != column.DefaultValue;

            if (defaultValue.ToUpper() == "NULL")
            {
                column.DefaultValue = null;
                return;
            }

            Func<string> getTrimedValue = () =>
            {
                return defaultValue.Trim().Trim('\'');
            };

            string trimedValue = getTrimedValue();

            if (SequenceTranslator.IsSequenceValueFlag(this.sourceDbType, defaultValue))
            {
                if (this.targetDbType == DatabaseType.MySql
                    || this.Option.OnlyForTableCopy
                    || column.IsIdentity)
                {
                    column.DefaultValue = null;
                }
                else
                {
                    column.DefaultValue = this.sequenceTranslator.HandleSequenceValue(defaultValue);
                }

                return;
            }

            if (defaultValue == "''")
            {
                if (this.targetDbType == DatabaseType.Oracle)
                {
                    column.IsNullable = true;
                    return;
                }
            }

            bool hasScale = false;
            string scale = null;

            if (DataTypeHelper.IsDateOrTimeType(column.DataType) && defaultValue.Count(item => item == '(') == 1 && defaultValue.EndsWith(')')) //timestamp(scale)
            {
                int index = defaultValue.IndexOf('(');
                hasScale = true;
                scale = defaultValue.Substring(index + 1).Trim(')').Trim();
                defaultValue = defaultValue.Substring(0, index).Trim();
            }

            string functionName = defaultValue;

            List<FunctionFormula> formulas = FunctionTranslator.GetFunctionFormulas(this.sourceDbInterpreter, defaultValue);

            if (formulas.Count > 0)
            {
                functionName = formulas.First().Name;
            }

            IEnumerable<FunctionMapping> funcMappings = this.functionMappings.FirstOrDefault(item => item.Any(t => t.DbType == this.sourceDbType.ToString()
                                                        && t.Function.Split(',').Any(m => m.Trim().ToLower() == functionName.ToLower())));

            if (funcMappings != null)
            {
                functionName = funcMappings.FirstOrDefault(item => item.DbType == this.targetDbType.ToString())?.Function.Split(',')?.FirstOrDefault();

                bool handled = false;

                if (this.targetDbType == DatabaseType.MySql || this.targetDbType == DatabaseType.Postgres)
                {
                    if (functionName.ToUpper() == "CURRENT_TIMESTAMP" && column.DataType.Contains("timestamp") && column.Scale > 0)
                    {
                        defaultValue = $"{functionName}({column.Scale})";
                        handled = true;
                    }
                }

                if (this.targetDbType == DatabaseType.SqlServer)
                {
                    if (functionName == "GETDATE")
                    {
                        if (hasScale && int.TryParse(scale, out _))
                        {
                            if (int.Parse(scale) > 3)
                            {
                                defaultValue = "SYSDATETIME";
                                handled = true;
                            }
                        }
                    }
                }

                if (!handled)
                {
                    defaultValue = this.functionTranslator.GetMappedFunction(defaultValue);
                }
            }
            else
            {
                if (this.sourceDbType == DatabaseType.Postgres)
                {
                    if (defaultValue.Contains("::")) //remove Postgres type reference
                    {
                        string[] items = defaultValue.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        List<string> list = new List<string>();

                        foreach (var item in items)
                        {
                            list.Add(item.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                        }

                        defaultValue = string.Join(" ", list);
                        trimedValue = getTrimedValue();
                    }

                    if (defaultValue.Trim() == "true" || defaultValue.Trim() == "false")
                    {
                        defaultValue = defaultValue.Replace("true", "1").Replace("false", "0");
                    }

                    if (defaultValue.ToUpper().Contains("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"))
                    {
                        if (this.targetDbType == DatabaseType.SqlServer)
                        {
                            defaultValue = "GETUTCDATE()";
                        }
                        else if (this.targetDbType == DatabaseType.MySql)
                        {
                            defaultValue = "UTC_TIMESTAMP()";
                        }
                        else if (this.targetDbType == DatabaseType.Oracle)
                        {
                            defaultValue = "SYS_EXTRACT_UTC(SYSTIMESTAMP)";
                        }
                    }
                }
                else if (this.sourceDbType == DatabaseType.SqlServer)
                {
                    //if it uses defined default value
                    if (defaultValue.Contains("CREATE DEFAULT ") && defaultValue.Contains(" AS "))
                    {
                        int asIndex = defaultValue.LastIndexOf("AS");
                        defaultValue = defaultValue.Substring(asIndex + 3).Trim();
                    }

                    if (trimedValue.ToLower() == "newid()")
                    {
                        column.DefaultValue = null;

                        return;
                    }
                }

                #region handle date/time string
                if (this.sourceDbType == DatabaseType.SqlServer)
                {
                    //target data type is datetime or timestamp, but default value is time ('HH:mm:ss')
                    if (DataTypeHelper.IsDatetimeOrTimestampType(column.DataType))
                    {
                        if (TimeSpan.TryParse(trimedValue, out _))
                        {
                            if (this.targetDbType == DatabaseType.MySql || this.targetDbType == DatabaseType.Postgres)
                            {
                                defaultValue = $"'{DateTime.MinValue.Date.ToLongDateString()} {trimedValue}'";
                            }
                            else if (this.targetDbType == DatabaseType.Oracle)
                            {
                                defaultValue = $"TO_TIMESTAMP('{DateTime.MinValue.Date.ToString("yyyy-MM-dd")} {TimeSpan.Parse(trimedValue).ToString("HH:mm:ss")}','yyyy-MM-dd hh24:mi:ss')";
                            }
                        }
                    }
                }
                else if (this.sourceDbType == DatabaseType.Oracle)
                {
                    if (DataTypeHelper.IsDateOrTimeType(column.DataType) && trimedValue.StartsWith("TO_TIMESTAMP"))
                    {
                        int index = trimedValue.IndexOf('(');

                        defaultValue = defaultValue.Split(',')[0].Substring(index + 1);
                    }
                }
                else if (this.sourceDbType == DatabaseType.MySql)
                {
                    if (trimedValue == "0000-00-00 00:00:00")
                    {
                        column.DefaultValue = null;
                        return;
                    }
                    else
                    {
                        if (this.targetDbType == DatabaseType.SqlServer || this.targetDbType == DatabaseType.Postgres)
                        {
                            defaultValue = $"'{trimedValue}'";
                        }
                    }
                }
                else if (this.sourceDbType == DatabaseType.Postgres)
                {
                    defaultValue = defaultValue.Replace("without time zone", "");
                    trimedValue = getTrimedValue();
                }

                if (DataTypeHelper.IsDateOrTimeType(column.DataType))
                {
                    if (this.targetDbType == DatabaseType.Oracle) //datetime string to TO_TIMESTAMP(value)
                    {
                        if (DateTime.TryParse(trimedValue, out _))
                        {
                            if (trimedValue.Contains(" ")) // date & time
                            {
                                defaultValue = $"TO_TIMESTAMP('{DateTime.Parse(trimedValue).ToString("yyyy-MM-dd HH:mm:ss")}','yyyy-MM-dd hh24:mi:ss')";
                            }
                            else
                            {
                                if (trimedValue.Contains(":")) //time
                                {
                                    defaultValue = $"TO_TIMESTAMP('{DateTime.MinValue.ToString("yyyy-MM-dd")} {DateTime.Parse(trimedValue).ToString("HH:mm:ss")}','yyyy-MM-dd hh24:mi:ss')";
                                }
                                else //date
                                {
                                    defaultValue = $"TO_TIMESTAMP('{DateTime.Parse(trimedValue).ToString("yyyy-MM-dd")}','yyyy-MM-dd')";
                                }
                            }
                        }
                    }
                }
                #endregion

                #region handle binary type
                if (this.sourceDbType == DatabaseType.SqlServer)
                {
                    if (this.targetDbType == DatabaseType.Postgres)
                    {
                        if (column.DataType == "bytea" && column.MaxLength > 0)
                        {
                            long value = 0;

                            if (long.TryParse(defaultValue, out value))
                            {
                                //integer hex string to bytea
                                string hex = value.ToString("X").PadLeft((int)column.MaxLength.Value * 2, '0');

                                defaultValue = $"'\\x{hex}'::bytea";
                            }
                            else if (defaultValue.StartsWith("0x"))
                            {
                                defaultValue = $"'\\x{defaultValue.Substring(2)}'::bytea";
                            }
                        }
                    }
                    else if (this.targetDbType == DatabaseType.Oracle || this.targetDbType == DatabaseType.MySql)
                    {
                        if (DataTypeHelper.IsBinaryType(column.DataType) && column.MaxLength > 0)
                        {
                            long value = 0;

                            if (long.TryParse(defaultValue, out value))
                            {
                                string hex = value.ToString("X").PadLeft((int)column.MaxLength.Value * 2, '0');

                                if (this.targetDbType == DatabaseType.Oracle)
                                {
                                    defaultValue = $"'{hex}'";
                                }
                                else if (this.targetDbType == DatabaseType.MySql)
                                {
                                    defaultValue = $"0x{hex}";
                                }
                            }
                            else if (defaultValue.StartsWith("0x"))
                            {
                                if (this.targetDbType == DatabaseType.Oracle)
                                {
                                    defaultValue = $"'{defaultValue.Substring(2)}'";
                                }
                            }
                        }
                    }
                }
                else if (this.sourceDbType == DatabaseType.Postgres)
                {
                    if (DataTypeHelper.IsBinaryType(column.DataType) && trimedValue.StartsWith("\\x"))
                    {
                        if (this.targetDbType == DatabaseType.SqlServer || this.targetDbType == DatabaseType.MySql)
                        {
                            defaultValue = $"0x{trimedValue.Substring(2)}";
                        }
                        else if (this.targetDbType == DatabaseType.Oracle)
                        {
                            defaultValue = $"'{trimedValue.Substring(2)}'";
                        }
                    }
                }
                else if (this.sourceDbType == DatabaseType.MySql)
                {
                    if (DataTypeHelper.IsBinaryType(column.DataType))
                    {
                        if (trimedValue == "0x")
                        {
                            //when type is binary(10) and default value is 0x00000000000000000001 or b'00000000000000000001',
                            //the column "COLUMN_DEFAULT" of "INFORMATION_SCHEMA.COLUMNS" always is "0x", but use "insert into table values(default)" is correct result.

                            column.DefaultValue = null; //TODO                            
                            return;
                        }
                        else if (trimedValue.StartsWith("0x"))
                        {
                            if (targetDbType == DatabaseType.Postgres)
                            {
                                defaultValue = $"'\\x{trimedValue.Substring(2)}'::bytea";
                            }
                            else if (targetDbType == DatabaseType.Oracle)
                            {
                                defaultValue = $"'{trimedValue.Substring(2)}'";
                            }
                        }
                    }
                }
                else if (this.sourceDbType == DatabaseType.Oracle)
                {
                    if (DataTypeHelper.IsBinaryType(column.DataType))
                    {
                        if (this.targetDbType == DatabaseType.SqlServer || this.targetDbType == DatabaseType.MySql)
                        {
                            defaultValue = $"0x{trimedValue}";
                        }
                        if (this.targetDbType == DatabaseType.Postgres && column.DataType == "bytea")
                        {
                            defaultValue = $"'\\x{trimedValue}'::bytea";
                        }
                    }
                }
                #endregion

                if (column.DataType == "boolean")
                {
                    if (this.targetDbType == DatabaseType.Postgres)
                    {
                        defaultValue = defaultValue.Replace("0", "false").Replace("1", "true");
                    }
                }
                else if (DataTypeHelper.IsUserDefinedType(column))
                {
                    var udt = this.UserDefinedTypes.FirstOrDefault(item => item.Name == column.DataType);

                    if (udt != null)
                    {
                        var attr = udt.Attributes.FirstOrDefault();

                        var dataTypeInfo = new DataTypeInfo() { DataType = attr.DataType };

                        this.dataTypeTranslator.Translate(dataTypeInfo);

                        if (this.targetDbType == DatabaseType.Postgres)
                        {
                            if (dataTypeInfo.DataType == "boolean")
                            {
                                defaultValue = defaultValue.Replace("0", "row(false)").Replace("1", "row(true)");
                            }
                        }
                        else if (this.targetDbType == DatabaseType.Oracle)
                        {
                            if ((attr.DataType == "bit" || attr.DataType == "boolean") && dataTypeInfo.DataType == "number")
                            {
                                string dataType = this.targetDbInterpreter.GetQuotedString(udt.Name);

                                defaultValue = defaultValue.Replace("0", $"{dataType}(0)").Replace("1", $"{dataType}(1)");
                            }
                        }
                    }
                }

                //custom function
                if (defaultValue.Contains(this.sourceDbInterpreter.QuotationLeftChar) && this.sourceDbInterpreter.QuotationLeftChar != this.targetDbInterpreter.QuotationLeftChar)
                {
                    defaultValue = this.ParseDefinition(defaultValue);

                    if (defaultValue.Contains("."))
                    {
                        if (this.targetDbType == DatabaseType.MySql || this.targetDbType == DatabaseType.Oracle)
                        {
                            defaultValue = null;
                        }
                        else
                        {
                            string[] items = defaultValue.Split(".");
                            string schema = items[0].Trim();

                            if (this.Option.SchemaMappings.Any())
                            {
                                string mappedSchema = this.Option.SchemaMappings.FirstOrDefault(item => this.GetTrimedName(item.SourceSchema) == this.GetTrimedName(schema))?.TargetSchema;

                                if (!string.IsNullOrEmpty(mappedSchema))
                                {
                                    defaultValue = $"{this.targetDbInterpreter.GetQuotedString(mappedSchema)}.{string.Join(".", items.Skip(1))}";
                                }
                                else
                                {
                                    defaultValue = string.Join(".", items.Skip(1));
                                }
                            }
                            else
                            {
                                defaultValue = string.Join(".", items.Skip(1));
                            }
                        }
                    }
                }
            }

            column.DefaultValue = (!string.IsNullOrEmpty(defaultValue) && hasParenthesis && !defaultValue.StartsWith("-")) ? $"({defaultValue})" : defaultValue;
        }

        public void ConvertComputeExpression(TableColumn column)
        {
            if (this.sourceDbType == DatabaseType.Oracle)
            {
                column.ComputeExp = column.ComputeExp.Replace("U'", "'");
            }
            else if (this.sourceDbType == DatabaseType.SqlServer)
            {
                column.ComputeExp = column.ComputeExp.Replace("N'", "'");
            }

            column.ComputeExp = this.ParseDefinition(column.ComputeExp);

            string computeExp = column.ComputeExp.ToLower();

            if (this.targetDbType == DatabaseType.Postgres)
            {
                //this is to avoid error when datatype is money uses coalesce(exp,0)
                if (computeExp.Contains("coalesce"))
                {
                    if (column.DataType.ToLower() == "money")
                    {
                        string exp = column.ComputeExp;

                        if (computeExp.StartsWith("("))
                        {
                            exp = exp.Substring(1, computeExp.Length - 1);
                        }

                        List<FunctionFormula> formulas = FunctionTranslator.GetFunctionFormulas(this.sourceDbInterpreter, exp);

                        if (formulas.Count > 0)
                        {
                            var args = formulas.First().GetArgs();

                            if (args.Count > 0)
                            {
                                column.ComputeExp = args[0];
                            }
                        }
                    }
                }
            }

            if (this.targetDbType == DatabaseType.Postgres)
            {
                if (column.DataType == "money" && !column.ComputeExp.ToLower().Contains("::money"))
                {
                    column.ComputeExp = TranslateHelper.ConvertNumberToPostgresMoney(column.ComputeExp);
                }
            }

            if (this.sourceDbType == DatabaseType.Postgres)
            {
                if (column.ComputeExp.Contains("::")) //datatype convert operator
                {
                    column.ComputeExp = TranslateHelper.RemovePostgresDataTypeConvertExpression(column.ComputeExp, sourceDataTypeSpecs, this.targetDbInterpreter.QuotationLeftChar, this.targetDbInterpreter.QuotationRightChar);
                }
            }

            if (computeExp.Contains("concat")) //use "||" instead of "concat"
            {
                if (this.targetDbType == DatabaseType.Postgres || this.targetDbType == DatabaseType.Oracle)
                {
                    column.ComputeExp = column.ComputeExp.Replace("concat", "", StringComparison.OrdinalIgnoreCase).Replace(",", "||");

                    if (this.targetDbType == DatabaseType.Oracle)
                    {
                        column.ComputeExp = this.CheckColumnDataTypeForComputeExpression(column.ComputeExp, this.targetDbInterpreter.STR_CONCAT_CHARS, targetDbType);
                    }
                }
            }

            if (!string.IsNullOrEmpty(this.sourceDbInterpreter.STR_CONCAT_CHARS))
            {
                string[] items = column.ComputeExp.Split(this.sourceDbInterpreter.STR_CONCAT_CHARS);

                var charColumns = this.columns.Where(c => items.Any(item => this.GetTrimedName(c.Name) == this.GetTrimedName(item.Trim('(', ')')) && DataTypeHelper.IsCharType(c.DataType)))
                                  .Select(c => c.Name);

                //if(this.Option.ConvertConcatChar)
                {
                    column.ComputeExp = ConcatCharsHelper.ConvertConcatChars(this.sourceDbInterpreter, this.targetDbInterpreter, column.ComputeExp, charColumns);
                }

                column.ComputeExp = this.CheckColumnDataTypeForComputeExpression(column.ComputeExp, this.targetDbInterpreter.STR_CONCAT_CHARS, targetDbType);
            }
        }

        private string CheckColumnDataTypeForComputeExpression(string computeExp, string concatChars, DatabaseType targetDbType)
        {
            if (this.sourceDbType == DatabaseType.Oracle)
            {
                computeExp = computeExp.Replace("SYS_OP_C2C", "").Replace("TO_CHAR", "");
            }

            //check whether column datatype is char/varchar type
            string[] items = computeExp.Split(concatChars);

            List<string> list = new List<string>();
            bool changed = false;

            foreach (var item in items)
            {
                string trimedItem = item.Trim('(', ')', ' ');

                var col = this.columns.FirstOrDefault(c => this.GetTrimedName(c.Name) == this.GetTrimedName(trimedItem));

                if (col != null && !DataTypeHelper.IsCharType(col.DataType))
                {
                    changed = true;

                    if (targetDbType == DatabaseType.Oracle)
                    {
                        list.Add(item.Replace(trimedItem, $"TO_CHAR({trimedItem})"));
                    }
                    else if (targetDbType == DatabaseType.SqlServer)
                    {
                        list.Add(item.Replace(trimedItem, $"CAST({trimedItem} AS VARCHAR(MAX))"));
                    }
                }
                else
                {
                    list.Add(item);
                }
            }

            if (changed)
            {
                return String.Join(concatChars, list.ToArray());
            }

            return computeExp;
        }

        private async void CheckComputeExpression()
        {
            IEnumerable<Function> customFunctions = this.SourceSchemaInfo?.Functions;

            foreach (TableColumn column in this.columns)
            {
                if (column.IsComputed)
                {
                    if (this.Option != null && !this.Option.ConvertComputeColumnExpression)
                    {
                        if (this.Option.OnlyCommentComputeColumnExpressionInScript)
                        {
                            column.ScriptComment = " AS " + column.ComputeExp;
                        }

                        column.ComputeExp = null;
                        continue;
                    }

                    bool setToNull = false;

                    var tableColumns = this.columns.Where(item => item.TableName == column.TableName);

                    bool isReferToSpecialDataType = tableColumns.Any(item => item.Name != column.Name
                                            && DataTypeHelper.IsSpecialDataType(item.DataType)
                                            && Regex.IsMatch(column.ComputeExp, $@"\b({item.Name})\b", RegexOptions.IgnoreCase));

                    if (isReferToSpecialDataType)
                    {
                        setToNull = true;
                    }

                    if (!setToNull && (this.targetDbType == DatabaseType.MySql || this.targetDbType == DatabaseType.Oracle))
                    {
                        if (customFunctions == null || customFunctions.Count() == 0)
                        {
                            customFunctions = await this.sourceDbInterpreter.GetFunctionsAsync();
                        }

                        if (customFunctions != null)
                        {
                            if (customFunctions.Any(item => column.ComputeExp.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                setToNull = true;
                            }
                        }
                    }

                    if (setToNull)
                    {
                        column.ScriptComment = " AS " + column.ComputeExp;
                        column.ComputeExp = null;
                    }
                }
            }
        }
    }
}