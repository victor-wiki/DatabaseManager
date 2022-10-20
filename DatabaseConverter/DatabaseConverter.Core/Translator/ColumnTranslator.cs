using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using NCalc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace DatabaseConverter.Core
{
    public class ColumnTranslator : DbObjectTokenTranslator
    {
        private IEnumerable<TableColumn> columns;
        private IEnumerable<DataTypeSpecification> sourceDataTypeSpecs;
        private DataTypeTranslator dataTypeTranslator;
        private FunctionTranslator functionTranslator;
        private SequenceTranslator sequenceTranslator;

        public List<UserDefinedType> UserDefinedTypes { get; set; } = new List<UserDefinedType>();

        public ColumnTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter, IEnumerable<TableColumn> columns) : base(sourceInterpreter, targetInterpreter)
        {
            this.columns = columns;
            this.sourceDataTypeSpecs = DataTypeManager.GetDataTypeSpecifications(this.sourceDbType);
            this.functionTranslator = new FunctionTranslator(this.sourceDbInterpreter, this.targetDbInterpreter);
            this.dataTypeTranslator = new DataTypeTranslator(this.sourceDbInterpreter, this.targetDbInterpreter);
            this.sequenceTranslator = new SequenceTranslator(this.sourceDbInterpreter, this.targetDbInterpreter);
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

            this.CheckComputeExpression();

            foreach (TableColumn column in this.columns)
            {
                if (!DataTypeHelper.IsUserDefinedType(column))
                {
                    DataTypeInfo dataTypeInfo = DataTypeHelper.GetDataTypeInfoByTableColumn(column);

                    this.dataTypeTranslator.Translate(dataTypeInfo);

                    DataTypeHelper.SetDataTypeInfoToTableColumn(dataTypeInfo, column);
                }

                if (!string.IsNullOrEmpty(column.DefaultValue))
                {
                    this.ConvertDefaultValue(column);
                }

                if (column.IsComputed)
                {
                    this.ConvertComputeExpression(column);
                }
            }

            this.FeedbackInfo("End translate columns.");
        }

        public void ConvertDefaultValue(TableColumn column)
        {
            string defaultValue = ValueHelper.GetTrimedParenthesisValue(column.DefaultValue);

            if (SequenceTranslator.IsSequenceValueFlag(this.sourceDbType, defaultValue))
            {
                if (this.targetDbType == DatabaseType.MySql)
                {
                    column.DefaultValue = null;
                }
                else
                {
                    column.DefaultValue = this.sequenceTranslator.HandleSequenceValue(defaultValue);
                }

                return;
            }

            bool hasScale = false;
            string scale = null;

            if (defaultValue.EndsWith(')') && !defaultValue.EndsWith("()")) //timestamp(scale)
            {
                int index = defaultValue.IndexOf('(');
                hasScale = true;
                scale = defaultValue.Substring(index + 1).Trim(')').Trim();
                defaultValue = defaultValue.Substring(0, index).Trim();
            }

            IEnumerable<FunctionMapping> funcMappings = this.functionMappings.FirstOrDefault(item => item.Any(t => t.DbType == this.sourceDbType.ToString()
                                                        && t.Function.Split(',').Any(m => m.Trim().ToLower() == defaultValue.Trim().ToLower())));

            if (funcMappings != null)
            {
                defaultValue = funcMappings.FirstOrDefault(item => item.DbType == this.targetDbType.ToString())?.Function.Split(',')?.FirstOrDefault();

                if (this.targetDbType == DatabaseType.MySql || this.targetDbType == DatabaseType.Postgres)
                {
                    if (defaultValue.ToUpper() == "CURRENT_TIMESTAMP" && column.DataType.Contains("timestamp") && column.Scale > 0)
                    {
                        defaultValue += $"({column.Scale})";
                    }
                }

                if (this.targetDbType == DatabaseType.SqlServer)
                {
                    if (defaultValue == "GETDATE()")
                    {
                        if (hasScale && int.TryParse(scale, out _))
                        {
                            if (int.Parse(scale) > 3)
                            {
                                defaultValue = "SYSDATETIME()";
                            }
                        }
                    }
                }
            }
            else
            {
                if (this.sourceDbInterpreter.DatabaseType == DatabaseType.Postgres)
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
                    }

                    if (defaultValue.Trim() == "true" || defaultValue.Trim() == "false")
                    {
                        defaultValue = defaultValue.Replace("true", "1").Replace("false", "0");
                    }
                }

                if (column.DataType == "boolean")
                {
                    if (this.targetDbInterpreter.DatabaseType == DatabaseType.Postgres)
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

                        if (this.targetDbInterpreter.DatabaseType == DatabaseType.Postgres)
                        {
                            if (dataTypeInfo.DataType == "boolean")
                            {
                                defaultValue = defaultValue.Replace("0", "row(false)").Replace("1", "row(true)");
                            }
                        }
                        else if (this.targetDbInterpreter.DatabaseType == DatabaseType.Oracle)
                        {
                            if ((attr.DataType == "bit" || attr.DataType == "boolean") && dataTypeInfo.DataType == "number")
                            {
                                string dataType = this.targetDbInterpreter.GetQuotedString(udt.Name);

                                defaultValue = defaultValue.Replace("0", $"{dataType}(0)").Replace("1", $"{dataType}(1)");
                            }
                        }
                    }
                }
            }

            column.DefaultValue = defaultValue;
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

                        List<FunctionFomular> fomulars = FunctionTranslator.GetFunctionFomulars(exp);

                        if (fomulars.Count > 0 && fomulars.First().Args.Count > 0)
                        {
                            column.ComputeExp = fomulars.First().Args[0];
                        }
                    }
                }
            }

            if (this.targetDbInterpreter.DatabaseType == DatabaseType.Postgres)
            {
                if (column.DataType == "money" && !column.ComputeExp.ToLower().Contains("::money"))
                {
                    column.ComputeExp = TranslateHelper.ConvertNumberToPostgresMoney(column.ComputeExp);
                }
            }

            if (this.sourceDbInterpreter.DatabaseType == DatabaseType.Postgres)
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
                bool hasCharColumn = false;

                string[] items = column.ComputeExp.Split(this.sourceDbInterpreter.STR_CONCAT_CHARS);

                if (items.Any(item => this.columns.Any(c => this.GetTrimedName(c.Name) == this.GetTrimedName(item.Trim('(', ')')) && DataTypeHelper.IsCharType(c.DataType))))
                {
                    hasCharColumn = true;
                }

                column.ComputeExp = TranslateHelper.ConvertConcatChars(column.ComputeExp, this.sourceDbInterpreter.STR_CONCAT_CHARS, this.targetDbInterpreter.STR_CONCAT_CHARS, hasCharColumn);

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
                                            && DataTypeHelper.SpecialDataTypes.Any(t => t.ToLower().Contains(item.DataType.ToLower()))
                                            && Regex.IsMatch(column.ComputeExp, $@"\b({item.Name})\b", RegexOptions.IgnoreCase));

                    if (isReferToSpecialDataType)
                    {
                        setToNull = true;
                    }

                    if (!setToNull && (this.targetDbInterpreter.DatabaseType == DatabaseType.MySql || this.targetDbInterpreter.DatabaseType == DatabaseType.Oracle))
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