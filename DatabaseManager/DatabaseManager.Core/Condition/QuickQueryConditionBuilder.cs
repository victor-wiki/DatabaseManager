using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core.Model;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager.Core
{
    public class QuickQueryConditionBuilder
    {
        private DbInterpreter dbInterpreter;
        private DatabaseType databaseType;
        private DatabaseObject tableOrView;
       

        public QuickQueryConditionBuilder(DbInterpreter dbInterpreter, DatabaseObject table)
        {
            this.dbInterpreter = dbInterpreter;
            this.databaseType = this.dbInterpreter.DatabaseType;
            this.tableOrView = table;
        }

        public async Task<string> Build(QuickFilterInfo info)
        {
            string content = info.Content;
            FilterMode mode = info.FilterMode;

            StringBuilder sb = new StringBuilder();

            if(mode == FilterMode.Contains || mode == FilterMode.Equals)
            {
                using (DbConnection connection = this.dbInterpreter.CreateConnection())
                {
                    var columns = await this.dbInterpreter.GetTableColumnsAsync(connection, new SchemaInfoFilter() { TableNames = [tableOrView.Name] });

                    List<string> items = new List<string>();

                    foreach(var column in columns)
                    {
                        string dataType = column.DataType;

                        bool isCharType = DataTypeHelper.IsCharType(dataType);
                        bool isBinaryType = DataTypeHelper.IsBinaryType(dataType);
                        bool isDateOrTimeType = DataTypeHelper.IsDateOrTimeType(dataType);
                        bool isGeometryType = DataTypeHelper.IsGeometryType(dataType);
                        bool isSpecialDataType = DataTypeHelper.IsSpecialDataType(dataType);

                        if(!isBinaryType && !isGeometryType && !isSpecialDataType)
                        {
                            string fieldName = $"{this.dbInterpreter.QuotationLeftChar}{column.Name}{this.dbInterpreter.QuotationRightChar}";

                            if(this.databaseType == DatabaseType.SqlServer)
                            {
                                if(dataType == "xml" || dataType == "text")
                                {
                                    fieldName = $"CONVERT(NVARCHAR(MAX),{fieldName})";
                                }
                                else if(!isCharType)
                                {
                                    fieldName = $"CONVERT(VARCHAR(MAX),{fieldName})";
                                }
                            }
                            else if(this.databaseType == DatabaseType.MySql)
                            {
                                if(isDateOrTimeType)
                                {
                                    fieldName = $"CAST({fieldName} AS CHAR)";
                                }
                            }
                            else if (this.databaseType == DatabaseType.Oracle)
                            {
                                if (!isCharType)
                                {
                                    fieldName = $"CAST({fieldName} AS VARCHAR2(4000))";
                                }
                            }
                            else if(this.databaseType == DatabaseType.Postgres)
                            {
                                if(!isCharType)
                                {
                                    fieldName = $"{fieldName}::CHARACTER VARYING";
                                }
                            }

                            string conditon = mode == FilterMode.Contains ? $"like '%{content}%'": $"='{content}'";

                            items.Add($"{fieldName} {conditon}");
                        }
                    }

                    sb.AppendLine(string.Join(" OR ", items)) ;
                }
            }
            else if(mode == FilterMode.SQL)
            {
                sb.AppendLine(content);
            }

            return sb.ToString().Trim() ;
        }
    }
}
