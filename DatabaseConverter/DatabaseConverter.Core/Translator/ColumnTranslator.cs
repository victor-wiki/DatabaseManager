using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseConverter.Core
{
    public class ColumnTranslator : DbObjectTranslator
    {
        private List<TableColumn> columns;
        private DatabaseType sourceDbType;
        private DatabaseType targetDbType;

        public ColumnTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter, List<TableColumn> columns) : base(sourceInterpreter, targetInterpreter)
        {
            this.columns = columns;
            this.sourceDbType = sourceInterpreter.DatabaseType;
            this.targetDbType = targetInterpreter.DatabaseType;
        }

        public override void Translate()
        {
            if (this.sourceDbType == this.targetDbType)
            {
                return;
            }

            this.LoadMappings();

            foreach (TableColumn column in this.columns)
            {
                this.ConvertDataType(column);

                if (!string.IsNullOrEmpty(column.DefaultValue))
                {
                    string defaultValue = this.GetTrimedDefaultValue(column.DefaultValue);

                    IEnumerable<FunctionMapping> funcMappings = this.functionMappings.FirstOrDefault(item => item.Any(t => t.DbType == this.sourceDbType.ToString() && t.Function.Split(',').Any(m => m.Trim().ToLower() == defaultValue.Trim().ToLower())));

                    if (funcMappings != null)
                    {
                        defaultValue = funcMappings.FirstOrDefault(item => item.DbType == this.targetDbType.ToString())?.Function.Split(',')?.FirstOrDefault();
                    }

                    column.DefaultValue = defaultValue;
                }
            }
        }

        public void ConvertDataType(TableColumn column)
        {
            string sourceDataType = GetTrimedDataType(column.DataType);
            column.DataType = sourceDataType;

            DataTypeMapping dataTypeMapping = this.dataTypeMappings.FirstOrDefault(item => item.Source.Type?.ToLower() == column.DataType?.ToLower());

            if (dataTypeMapping != null)
            {
                string targetDataType = dataTypeMapping.Tareget.Type;

                column.DataType = targetDataType;

                bool isChar = DataTypeHelper.IsCharType(column.DataType);

                if (isChar)
                {
                    if (!string.IsNullOrEmpty(dataTypeMapping.Tareget.Length))
                    {
                        column.MaxLength = int.Parse(dataTypeMapping.Tareget.Length);

                        if (targetDataType.ToLower().StartsWith("n") && !sourceDataType.ToLower().StartsWith("n"))
                        {
                            column.MaxLength *= 2;
                        }
                    }

                    if (dataTypeMapping.Specials != null && dataTypeMapping.Specials.Count > 0)
                    {
                        DataTypeMappingSpecial special = dataTypeMapping.Specials.FirstOrDefault(item => this.IsSpecialMaxLengthMatched(item, column));

                        if (special != null)
                        {
                            column.DataType = special.Type;

                            if (!string.IsNullOrEmpty(special.TargetMaxLength))
                            {
                                column.MaxLength = int.Parse(special.TargetMaxLength);
                            }
                        }
                    }
                }
                else
                {
                    if (dataTypeMapping.Specials != null && dataTypeMapping.Specials.Count > 0)
                    {
                        DataTypeMappingSpecial special = dataTypeMapping.Specials.FirstOrDefault(item => this.IsSpecialMaxLengthMatched(item, column));

                        if (special != null)
                        {
                            column.DataType = special.Type;
                        }
                        else
                        {
                            special = dataTypeMapping.Specials.FirstOrDefault(item => this.IsSpecialPrecisionOrScaleMatched(item, column));

                            if (special != null)
                            {
                                column.DataType = special.Type;
                            }
                        }

                        if (special != null && !string.IsNullOrEmpty(special.TargetMaxLength))
                        {
                            column.MaxLength = int.Parse(special.TargetMaxLength);
                        }
                    }

                    if (!string.IsNullOrEmpty(dataTypeMapping.Tareget.Precision))
                    {
                        column.Precision = int.Parse(dataTypeMapping.Tareget.Precision);
                    }

                    if (!string.IsNullOrEmpty(dataTypeMapping.Tareget.Scale))
                    {
                        column.Scale = int.Parse(dataTypeMapping.Tareget.Scale);
                    }
                }
            }
        }

        private bool IsSpecialMaxLengthMatched(DataTypeMappingSpecial special, TableColumn column)
        {
            if(!string.IsNullOrEmpty(special.SourceMaxLength))
            {
                if (special.SourceMaxLength == column.MaxLength?.ToString())
                {
                    return true;
                }
                else if (special.SourceMaxLength.StartsWith(">") && column.MaxLength.HasValue)
                {
                    Expression exp = new Expression($"{column.MaxLength}{special.SourceMaxLength}");

                    if (!exp.HasErrors())
                    {
                        object result = exp.Evaluate();

                        return result != null && result.GetType() == typeof(Boolean) && (bool)result == true;
                    }
                }
            }            

            return false;
        }

        private bool IsSpecialPrecisionOrScaleMatched(DataTypeMappingSpecial special, TableColumn column)
        {
            string precision = special.SourcePrecision;
            string scale = special.SourceScale;

            if(!string.IsNullOrEmpty(precision) && !string.IsNullOrEmpty(scale))
            {
                return precision == column.Precision.ToString() && scale == column.Scale.ToString(); 
            }
            else if(!string.IsNullOrEmpty(precision) && string.IsNullOrEmpty(scale))
            {
                return precision == column.Precision.ToString();
            }
            else if(string.IsNullOrEmpty(precision) && !string.IsNullOrEmpty(scale))
            {
                return scale == column.Scale.ToString();
            }
            return false;
        }       

        public string GetTrimedDataType(string dataType)
        {
            dataType = dataType.Trim(this.sourceDbInterpreter.QuotationLeftChar, this.sourceDbInterpreter.QuotationRightChar);

            int index = dataType.IndexOf("(");

            if (index > 0)
            {
                return dataType.Substring(0, index);
            }

            return dataType;
        }

        private string GetTrimedDefaultValue(string defaultValue)
        {
            if (!string.IsNullOrEmpty(defaultValue))
            {
                defaultValue = defaultValue.TrimStart('(').TrimEnd(')');

                if (defaultValue.EndsWith("("))
                {
                    defaultValue += ")";
                }

                return defaultValue;
            }

            return defaultValue;
        }
    }
}
