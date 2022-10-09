using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using NCalc;
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

        public List<UserDefinedType> UserDefinedTypes { get; set; } = new List<UserDefinedType>();

        public ColumnTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter, IEnumerable<TableColumn> columns) : base(sourceInterpreter, targetInterpreter)
        {
            this.columns = columns;
            this.sourceDataTypeSpecs = DataTypeManager.GetDataTypeSpecifications(this.sourceDbType);
            this.functionTranslator = new FunctionTranslator(this.sourceDbInterpreter, this.targetDbInterpreter);
            this.dataTypeTranslator = new DataTypeTranslator(this.sourceDbInterpreter, this.targetDbInterpreter);
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

            this.CheckComputeExpression();

            foreach (TableColumn column in this.columns)
            {
                if (!column.IsUserDefined)
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

            IEnumerable<FunctionMapping> funcMappings = this.functionMappings.FirstOrDefault(item => item.Any(t => t.DbType == this.sourceDbType.ToString() && t.Function.Split(',').Any(m => m.Trim().ToLower() == defaultValue.Trim().ToLower())));

            if (funcMappings != null)
            {
                defaultValue = funcMappings.FirstOrDefault(item => item.DbType == this.targetDbType.ToString())?.Function.Split(',')?.FirstOrDefault();
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
                else if (column.IsUserDefined)
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
                //this is to avoid error when datatype like money uses coalesce(exp,0)
                if (computeExp.Contains("coalesce"))
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

            column.ComputeExp = this.functionTranslator.GetMappedFunction(column.ComputeExp);

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