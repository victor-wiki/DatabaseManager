using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using PoorMansTSqlFormatterRedux;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseConverter.Core
{
    public abstract class DbObjectTranslator
    {
        protected string sourceOwnerName;
        protected DbInterpreter sourceDbInterpreter;
        protected DbInterpreter targetDbInterpreter;
        protected List<DataTypeMapping> dataTypeMappings = new List<DataTypeMapping>();
        protected List<IEnumerable<FunctionMapping>> functionMappings = new List<IEnumerable<FunctionMapping>>();

        public DbObjectTranslator(DbInterpreter source, DbInterpreter target)
        {
            this.sourceDbInterpreter = source;
            this.targetDbInterpreter = target;
        }

        public DbObjectTranslator LoadMappings()
        {
            this.functionMappings = FunctionMappingManager.GetFunctionMappings();
            this.dataTypeMappings = DataTypeMappingManager.GetDataTypeMappings(this.sourceDbInterpreter.DatabaseType, this.targetDbInterpreter.DatabaseType);

            return this;
        }

        public abstract void Translate();

        public DataTypeMapping GetDataTypeMapping(List<DataTypeMapping> mappings, string dataType)
        {
            return mappings.FirstOrDefault(item => item.Source.Type?.ToLower() == dataType?.ToLower());
        }

        public string GetNewDataType(List<DataTypeMapping> mappings, string dataType)
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

        

        public string FormatSql(string sql, out bool hasError)
        {
            hasError = false;

            SqlFormattingManager manager = new SqlFormattingManager();
            string formattedSql = manager.Format(sql, ref hasError);
            return formattedSql;
        }
    }
}
