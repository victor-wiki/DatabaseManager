using DatabaseInterpreter.Geometry;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Microsoft.SqlServer.Types;
using MySqlConnector;
using NetTopologySuite.Geometries;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgGeom = NetTopologySuite.Geometries;

namespace DatabaseInterpreter.Core
{
    public abstract class DbScriptGenerator
    {
        protected DbInterpreter dbInterpreter;
        protected DbInterpreterOption option;
        protected DatabaseType databaseType;
        protected string scriptsDelimiter;

        public DbScriptGenerator(DbInterpreter dbInterpreter)
        {
            this.dbInterpreter = dbInterpreter;
            this.option = dbInterpreter.Option;
            this.databaseType = dbInterpreter.DatabaseType;
            this.scriptsDelimiter = dbInterpreter.ScriptsDelimiter;
        }

        #region Schema Scripts
        public abstract ScriptBuilder GenerateSchemaScripts(SchemaInfo schemaInfo);

        protected virtual List<Script> GenerateScriptDbObjectScripts<T>(List<T> dbObjects)
            where T : ScriptDbObject
        {
            List<Script> scripts = new List<Script>();

            foreach (T dbObject in dbObjects)
            {
                this.dbInterpreter.FeedbackInfo(OperationState.Begin, dbObject);

                bool hasNewLine = this.scriptsDelimiter.Contains(Environment.NewLine);

                string definition = dbObject.Definition.Trim();

                scripts.Add(new CreateDbObjectScript<T>(definition));

                if (!hasNewLine)
                {
                    if (!definition.EndsWith(this.scriptsDelimiter))
                    {
                        scripts.Add(new SpliterScript(this.scriptsDelimiter));
                    }
                }
                else
                {
                    scripts.Add(new NewLineSript());
                    scripts.Add(new SpliterScript(this.scriptsDelimiter));
                }

                scripts.Add(new NewLineSript());

                this.dbInterpreter.FeedbackInfo(OperationState.End, dbObject);
            }

            return scripts;
        }

        #endregion

        #region Data Scripts
        public virtual async Task<string> GenerateDataScriptsAsync(SchemaInfo schemaInfo)
        {
            StringBuilder sb = new StringBuilder();

            if (this.option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
            {
                this.ClearScriptFile(GenerateScriptMode.Data);
            }

            using (DbConnection connection = this.dbInterpreter.CreateConnection())
            {
                int tableCount = schemaInfo.Tables.Count;
                int count = 0;

                foreach (Table table in schemaInfo.Tables)
                {
                    if (this.dbInterpreter.CancelRequested)
                    {
                        break;
                    }

                    count++;

                    string strTableCount = $"({count}/{tableCount})";
                    string tableName = table.Name;

                    List<TableColumn> columns = schemaInfo.TableColumns.Where(item => item.Schema == table.Schema && item.TableName == tableName).OrderBy(item => item.Order).ToList();

                    bool isSelfReference = TableReferenceHelper.IsSelfReference(tableName, schemaInfo.TableForeignKeys);

                    TablePrimaryKey primaryKey = schemaInfo.TablePrimaryKeys.FirstOrDefault(item => item.Schema == table.Schema && item.TableName == tableName);

                    string primaryKeyColumns = primaryKey == null ? "" : string.Join(",", primaryKey.Columns.OrderBy(item => item.Order).Select(item => this.GetQuotedString(item.ColumnName)));

                    long total = await this.dbInterpreter.GetTableRecordCountAsync(connection, table);

                    if (this.option.DataGenerateThreshold.HasValue && total > this.option.DataGenerateThreshold.Value)
                    {
                        this.FeedbackInfo($"Record count of table \"{this.GetQuotedFullTableName(table)}\" exceeds {this.option.DataGenerateThreshold.Value},ignore it.");
                        continue;
                    }

                    int pageSize = this.dbInterpreter.DataBatchSize;

                    this.FeedbackInfo($"{strTableCount}Table \"{this.GetQuotedFullTableName(table)}\":record count is {total}.");

                    Dictionary<long, List<Dictionary<string, object>>> dictPagedData;

                    if (isSelfReference)
                    {
                        var fk = schemaInfo.TableForeignKeys.FirstOrDefault(item =>
                            item.Schema == table.Schema
                            && item.TableName == tableName
                            && item.ReferencedTableName == tableName);

                        var fkc = fk.Columns.FirstOrDefault();

                        string referencedColumnName = this.GetQuotedString(fkc.ReferencedColumnName);
                        string columnName = this.GetQuotedString(fkc.ColumnName);

                        string strWhere = $" WHERE ({columnName} IS NULL OR {columnName}={referencedColumnName})";

                        dictPagedData = await this.GetSortedPageData(connection, table as Table, primaryKeyColumns, fkc.ReferencedColumnName, fkc.ColumnName, columns, total, strWhere);
                    }
                    else
                    {
                        dictPagedData = await this.dbInterpreter.GetPagedDataListAsync(connection, table, columns, primaryKeyColumns, total, total, pageSize);
                    }

                    this.FeedbackInfo($"{strTableCount}Table \"{this.GetQuotedFullTableName(table)}\":data read finished.");

                    GenerateScriptOutputMode scriptOutputMode = this.option.ScriptOutputMode;

                    if (count > 1)
                    {
                        if (scriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToString))
                        {
                            sb.AppendLine();
                        }

                        if (scriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
                        {
                            this.AppendScriptsToFile(Environment.NewLine, GenerateScriptMode.Data);
                        }
                    }

                    if (this.option.BulkCopy && this.dbInterpreter.SupportBulkCopy && !scriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
                    {
                        continue;
                    }
                    else
                    {
                        if (scriptOutputMode != GenerateScriptOutputMode.None)
                        {
                            this.AppendDataScripts(sb, table, columns as IEnumerable<TableColumn>, dictPagedData);
                        }
                    }
                }
            }

            var dataScripts = string.Empty;

            try
            {
                dataScripts = sb.ToString();
            }
            catch (OutOfMemoryException ex)
            {
                this.FeedbackError("Exception occurs:" + ex.Message);
            }
            finally
            {
                sb.Clear();
            }

            return dataScripts;
        }

        private async Task<Dictionary<long, List<Dictionary<string, object>>>> GetSortedPageData(DbConnection connection, Table table, string primaryKeyColumns, string referencedColumnName, string fkColumnName, List<TableColumn> columns, long total, string whereClause = "")
        {
            string quotedTableName = this.GetQuotedDbObjectNameWithSchema(table);

            int pageSize = this.dbInterpreter.DataBatchSize;

            long batchCount = Convert.ToInt64(await this.dbInterpreter.GetScalarAsync(connection, $"SELECT COUNT(1) FROM {quotedTableName} {whereClause}"));

            var dictPagedData = await this.dbInterpreter.GetPagedDataListAsync(connection, table, columns, primaryKeyColumns, total, batchCount, pageSize, whereClause);

            List<object> parentValues = dictPagedData.Values.SelectMany(item => item.Select(t => t[primaryKeyColumns.Trim(this.dbInterpreter.QuotationLeftChar, this.dbInterpreter.QuotationRightChar)])).ToList();

            if (parentValues.Count > 0)
            {
                TableColumn parentColumn = columns.FirstOrDefault(item => item.Schema == table.Schema && item.Name == fkColumnName);

                long parentValuesPageCount = PaginationHelper.GetPageCount(parentValues.Count, this.option.InQueryItemLimitCount);

                for (long parentValuePageNumber = 1; parentValuePageNumber <= parentValuesPageCount; parentValuePageNumber++)
                {
                    IEnumerable<object> pagedParentValues = parentValues.Skip((int)(parentValuePageNumber - 1) * pageSize).Take(this.option.InQueryItemLimitCount);

                    var parsedValues = pagedParentValues.Select(item => this.ParseValue(parentColumn, item, true));

                    string inCondition = this.GetWhereInCondition(parsedValues, fkColumnName);

                    whereClause = $@" WHERE ({inCondition})
                                      AND ({this.GetQuotedString(fkColumnName)}<>{this.GetQuotedString(referencedColumnName)})";

                    batchCount = Convert.ToInt64(await this.dbInterpreter.GetScalarAsync(connection, $"SELECT COUNT(1) FROM {quotedTableName} {whereClause}"));

                    if (batchCount > 0)
                    {
                        Dictionary<long, List<Dictionary<string, object>>> dictChildPagedData = await this.GetSortedPageData(connection, table, primaryKeyColumns, referencedColumnName, fkColumnName, columns, total, whereClause);

                        foreach (var kp in dictChildPagedData)
                        {
                            long pageNumber = dictPagedData.Keys.Max(item => item);
                            dictPagedData.Add(pageNumber + 1, kp.Value);
                        }
                    }
                }
            }

            return dictPagedData;
        }

        private string GetWhereInCondition(IEnumerable<object> values, string columnName)
        {
            int valuesCount = values.Count();
            StringBuilder sb = new StringBuilder();

            int oracleLimitCount = 1000;//oracle where in items count is limit to 1000

            if (valuesCount > oracleLimitCount && this.databaseType == DatabaseType.Oracle)
            {
                var groups = values.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / oracleLimitCount).Select(x => x.Select(v => v.Value));

                int count = 0;

                foreach (var gp in groups)
                {
                    sb.AppendLine($"{(count > 0 ? " OR " : "")}{this.GetQuotedString(columnName)} IN ({string.Join(",", gp)})");

                    count++;
                }
            }
            else
            {
                sb.Append($"{this.GetQuotedString(columnName)} IN ({string.Join(",", values)})");
            }

            return sb.ToString();
        }

        public virtual Dictionary<string, object> AppendDataScripts(StringBuilder sb, Table table, IEnumerable<TableColumn> columns, Dictionary<long, List<Dictionary<string, object>>> dictPagedData)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            bool appendString = this.option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToString);
            bool appendFile = this.option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile);

            List<string> excludeColumnNames = new List<string>();

            bool excludeIdentityColumn = false;

            if (this.databaseType == DatabaseType.Oracle && this.dbInterpreter.IsLowDbVersion())
            {
                excludeIdentityColumn = false;
            }
            else
            {
                if (this.option.TableScriptsGenerateOption.GenerateIdentity)
                {
                    excludeIdentityColumn = true;
                }
            }

            excludeColumnNames.AddRange(columns.Where(item => item.IsIdentity && excludeIdentityColumn).Select(item => item.Name));

            if (this.option.ExcludeGeometryForData)
            {
                excludeColumnNames.AddRange(columns.Where(item => DataTypeHelper.IsGeometryType(item.DataType)).Select(item => item.Name));
            }

            excludeColumnNames.AddRange(columns.Where(item => item.IsComputed).Select(item => item.Name));

            bool identityColumnHasBeenExcluded = excludeColumnNames.Any(item => columns.Any(col => col.Name == item && col.IsIdentity));
            bool computeColumnHasBeenExcluded = columns.Any(item => item.IsComputed);

            bool canBatchInsert = true;

            if (this.databaseType == DatabaseType.Oracle)
            {
                if (identityColumnHasBeenExcluded)
                {
                    canBatchInsert = false;
                }
            }

            foreach (var kp in dictPagedData)
            {
                if (kp.Value.Count == 0)
                {
                    continue;
                }

                StringBuilder sbFilePage = new StringBuilder();

                string tableName = this.GetQuotedFullTableName(table);
                string columnNames = this.GetQuotedColumnNames(columns.Where(item => !excludeColumnNames.Contains(item.Name)));
                string insert = !canBatchInsert ? "" : $"{this.GetBatchInsertPrefix()} {tableName}({this.GetQuotedColumnNames(columns.Where(item => !excludeColumnNames.Contains(item.Name)))})VALUES";

                if (appendString)
                {
                    if (kp.Key > 1)
                    {
                        sb.AppendLine();
                    }

                    if (!string.IsNullOrEmpty(insert))
                    {
                        sb.AppendLine(insert);
                    }
                }

                if (appendFile)
                {
                    if (kp.Key > 1)
                    {
                        sbFilePage.AppendLine();
                    }

                    if (!string.IsNullOrEmpty(insert))
                    {
                        sbFilePage.AppendLine(insert);
                    }
                }

                int rowCount = 0;

                foreach (var row in kp.Value)
                {
                    rowCount++;

                    var rowValues = this.GetRowValues(row, rowCount - 1, columns, excludeColumnNames, kp.Key, false, out var insertParameters);

                    string values = $"({string.Join(",", rowValues.Select(item => item == null ? "NULL" : item))})";

                    if (insertParameters != null)
                    {
                        foreach (var para in insertParameters)
                        {
                            parameters.Add(para.Key, para.Value);
                        }
                    }

                    bool isAllEnd = rowCount == kp.Value.Count;

                    string beginChar = canBatchInsert ? this.GetBatchInsertItemBefore(tableName,
                                        (identityColumnHasBeenExcluded || computeColumnHasBeenExcluded) ? columnNames : "",
                                        rowCount == 1) :
                                     $"INSERT INTO {tableName}({columnNames}) VALUES";
                    string endChar = canBatchInsert ? this.GetBatchInsertItemEnd(isAllEnd) : (isAllEnd ? ";" : (canBatchInsert ? "," : ";"));

                    values = $"{beginChar}{values}{endChar}";

                    if (this.option.RemoveEmoji)
                    {
                        values = StringHelper.RemoveEmoji(values);
                    }

                    if (appendString)
                    {
                        sb.AppendLine(values);
                    }

                    if (appendFile)
                    {
                        var fileRowValues = this.GetRowValues(row, rowCount - 1, columns, excludeColumnNames, kp.Key, true, out var _);
                        string fileValues = $"({string.Join(",", fileRowValues.Select(item => item == null ? "NULL" : item))})";

                        sbFilePage.AppendLine($"{beginChar}{fileValues}{endChar}");
                    }
                }

                if (appendFile)
                {
                    this.AppendScriptsToFile(sbFilePage.ToString(), GenerateScriptMode.Data);
                }
            }

            return parameters;
        }

        protected virtual string GetBatchInsertPrefix()
        {
            return "INSERT INTO";
        }

        protected virtual string GetBatchInsertItemBefore(string tableName, string columnNames, bool isFirstRow)
        {
            return "";
        }

        protected virtual string GetBatchInsertItemEnd(bool isAllEnd)
        {
            return (isAllEnd ? ";" : ",");
        }

        private List<object> GetRowValues(Dictionary<string, object> row, int rowIndex, IEnumerable<TableColumn> columns, List<string> excludeColumnNames, long pageNumber, bool isAppendToFile, out Dictionary<string, object> parameters)
        {
            parameters = new Dictionary<string, object>();

            List<object> values = new List<object>();

            foreach (TableColumn column in columns)
            {
                string columnName = column.Name;

                if (!row.ContainsKey(columnName))
                {
                    continue;
                }

                if (!excludeColumnNames.Contains(column.Name))
                {
                    object value = row[columnName];
                    object parsedValue = this.ParseValue(column, value);
                    bool isBitArray = row[column.Name]?.GetType() == typeof(BitArray);
                    bool isBytes = ValueHelper.IsBytes(parsedValue) || isBitArray;
                    bool isNullValue = value == DBNull.Value || parsedValue?.ToString() == "NULL";

                    if (!isNullValue)
                    {
                        if (!isAppendToFile)
                        {
                            bool needInsertParameter = this.NeedInsertParameter(column, parsedValue);

                            if ((isBytes && !this.option.TreatBytesAsNullForExecuting) || needInsertParameter)
                            {
                                string parameterName = $"P{pageNumber}_{rowIndex}_{column.Name}";

                                string parameterPlaceholder = this.dbInterpreter.CommandParameterChar + parameterName;

                                if (this.databaseType != DatabaseType.Postgres && isBitArray)
                                {
                                    var bitArray = parsedValue as BitArray;
                                    byte[] bytes = new byte[bitArray.Length];
                                    bitArray.CopyTo(bytes, 0);

                                    parsedValue = bytes;
                                }

                                parameters.Add(parameterPlaceholder, parsedValue);

                                parsedValue = parameterPlaceholder;
                            }
                            else if (isBytes && this.option.TreatBytesAsNullForExecuting)
                            {
                                parsedValue = null;
                            }
                        }
                        else
                        {
                            if (isBytes)
                            {
                                if (this.option.TreatBytesAsHexStringForFile)
                                {
                                    parsedValue = this.GetBytesConvertHexString(parsedValue, column.DataType);
                                }
                                else
                                {
                                    parsedValue = null;
                                }
                            }
                        }
                    }

                    if (DataTypeHelper.IsUserDefinedType(column))
                    {
                        if (this.databaseType == DatabaseType.Postgres)
                        {
                            parsedValue = $"row({parsedValue})";
                        }
                        else if (this.databaseType == DatabaseType.Oracle)
                        {
                            parsedValue = $"{this.GetQuotedString(column.DataType)}({parsedValue})";
                        }
                    }

                    values.Add(parsedValue);
                }
            }

            return values;
        }

        protected virtual bool NeedInsertParameter(TableColumn column, object value)
        {
            return false;
        }

        protected virtual string GetBytesConvertHexString(object value, string dataType)
        {
            return null;
        }

        private object ParseValue(TableColumn column, object value, bool bytesAsString = false)
        {
            if (value != null)
            {
                Type type = value.GetType();
                bool needQuotated = false;
                string strValue = "";

                if (type == typeof(DBNull))
                {
                    return "NULL";
                }
                else if (value is SqlGeography sgg && sgg.IsNull)
                {
                    return "NULL";
                }
                else if (value is SqlGeometry sgm && sgm.IsNull)
                {
                    return "NULL";
                }
                else if (type == typeof(Byte[]))
                {
                    if (((Byte[])value).Length == 16) //GUID
                    {
                        string str = ValueHelper.ConvertGuidBytesToString((Byte[])value, this.databaseType, column.DataType, column.MaxLength, bytesAsString);

                        if (!string.IsNullOrEmpty(str))
                        {
                            needQuotated = true;
                            strValue = str;
                        }
                        else
                        {
                            return value;
                        }
                    }
                    else
                    {
                        return value;
                    }
                }

                bool oracleSemicolon = false;
                string dataType = column.DataType.ToLower();

                switch (type.Name)
                {
                    case nameof(Guid):

                        needQuotated = true;
                        if (this.databaseType == DatabaseType.Oracle && dataType == "raw" && column.MaxLength == 16)
                        {
                            strValue = StringHelper.GuidToRaw(value.ToString());
                        }
                        else
                        {
                            strValue = value.ToString();
                        }
                        break;

                    case nameof(String):

                        needQuotated = true;
                        strValue = value.ToString();

                        if (this.databaseType == DatabaseType.Oracle)
                        {
                            if (strValue.Contains(";"))
                            {
                                oracleSemicolon = true;
                            }
                            else if (DataTypeHelper.IsGeometryType(dataType))
                            {
                                needQuotated = false;
                                strValue = this.GetOracleGeometryInsertValue(column, value);
                            }
                        }
                        break;

                    case nameof(DateTime):
                    case nameof(DateTimeOffset):
                    case nameof(MySqlDateTime):

                        if (this.databaseType == DatabaseType.Oracle)
                        {
                            if (type.Name == nameof(MySqlDateTime))
                            {
                                DateTime dateTime = ((MySqlDateTime)value).GetDateTime();

                                strValue = this.GetOracleDatetimeConvertString(dateTime);
                            }
                            else if (type.Name == nameof(DateTime))
                            {
                                DateTime dateTime = Convert.ToDateTime(value);

                                strValue = this.GetOracleDatetimeConvertString(dateTime);
                            }
                            else if (type.Name == nameof(DateTimeOffset))
                            {
                                DateTimeOffset dtOffset = DateTimeOffset.Parse(value.ToString());
                                int millisecondLength = dtOffset.Millisecond.ToString().Length;
                                string strMillisecond = millisecondLength == 0 ? "" : $".{"f".PadLeft(millisecondLength, 'f')}";
                                string format = $"yyyy-MM-dd HH:mm:ss{strMillisecond}";

                                string strDtOffset = dtOffset.ToString(format) + $"{dtOffset.Offset.Hours}:{dtOffset.Offset.Minutes}";

                                strValue = $@"TO_TIMESTAMP_TZ('{strDtOffset}','yyyy-MM-dd HH24:MI:ssxff TZH:TZM')";
                            }
                        }
                        else if (this.databaseType == DatabaseType.MySql)
                        {
                            if (type.Name == nameof(DateTime))
                            {
                                DateTime dt = (DateTime)value;

                                if (dt > MySqlInterpreter.Timestamp_Max_Value.ToLocalTime())
                                {
                                    value = MySqlInterpreter.Timestamp_Max_Value.ToLocalTime();
                                }

                            }
                            else if (type.Name == nameof(DateTimeOffset))
                            {
                                DateTimeOffset dtOffset = DateTimeOffset.Parse(value.ToString());

                                if (dtOffset > MySqlInterpreter.Timestamp_Max_Value.ToLocalTime())
                                {
                                    dtOffset = MySqlInterpreter.Timestamp_Max_Value.ToLocalTime();
                                }

                                strValue = $"'{dtOffset.DateTime.Add(dtOffset.Offset).ToString("yyyy-MM-dd HH:mm:ss.ffffff")}'";
                            }
                        }

                        if (string.IsNullOrEmpty(strValue))
                        {
                            needQuotated = true;
                            strValue = value.ToString();
                        }
                        break;

                    case nameof(Boolean):

                        if (this.databaseType == DatabaseType.Postgres)
                        {
                            strValue = value.ToString().ToLower();
                        }
                        else
                        {
                            strValue = value.ToString() == "True" ? "1" : "0";
                        }
                        break;
                    case nameof(TimeSpan):

                        if (this.databaseType == DatabaseType.Oracle)
                        {
                            return value;
                        }
                        else
                        {
                            needQuotated = true;

                            if (dataType.Contains("datetime")
                                || dataType.Contains("timestamp")
                                )
                            {
                                DateTime dateTime = this.dbInterpreter.MinDateTime.AddSeconds(TimeSpan.Parse(value.ToString()).TotalSeconds);

                                strValue = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                strValue = value.ToString();
                            }
                        }
                        break;
                    case nameof(SqlGeography):
                    case nameof(SqlGeometry):
                    case nameof(Point):
                    case nameof(LineString):
                    case nameof(Polygon):
                    case nameof(MultiPoint):
                    case nameof(MultiLineString):
                    case nameof(MultiPolygon):
                    case nameof(GeometryCollection):
                        int srid = 0;

                        if (value is SqlGeography sgg) srid = sgg.STSrid.Value;
                        else if (value is SqlGeometry sgm) srid = sgm.STSrid.Value;
                        else if (value is PgGeom.Geometry g) srid = g.SRID;

                        if (this.databaseType == DatabaseType.MySql)
                        {
                            strValue = $"ST_GeomFromText('{this.GetCorrectGeometryText(value, dataType)}',{srid})";
                        }
                        else if (this.databaseType == DatabaseType.Oracle)
                        {
                            strValue = this.GetOracleGeometryInsertValue(column, value, srid);
                        }
                        else
                        {
                            needQuotated = true;
                            strValue = value.ToString();
                        }

                        break;
                    case nameof(SqlHierarchyId):
                    case nameof(NpgsqlLine):
                    case nameof(NpgsqlBox):
                    case nameof(NpgsqlCircle):
                    case nameof(NpgsqlPath):
                    case nameof(NpgsqlLSeg):
                    case nameof(NpgsqlTsVector):
                        needQuotated = true;
                        strValue = value.ToString();

                        break;

                    case nameof(SdoGeometry):
                    case nameof(StGeometry):
                        strValue = this.GetOracleGeometryInsertValue(column, value);

                        break;

                    default:
                        if (string.IsNullOrEmpty(strValue))
                        {
                            strValue = value.ToString();
                        }
                        break;
                }

                if (needQuotated)
                {
                    strValue = $"{this.dbInterpreter.UnicodeLeadingFlag}'{ValueHelper.TransferSingleQuotation(strValue)}'";

                    if (oracleSemicolon)
                    {
                        strValue = strValue.Replace(";", $"'{this.dbInterpreter.STR_CONCAT_CHARS}{OracleInterpreter.SEMICOLON_FUNC}{this.dbInterpreter.STR_CONCAT_CHARS}'");
                    }

                    return strValue;
                }
                else
                {
                    return strValue;
                }
            }
            else
            {
                return null;
            }
        }

        private string GetOracleGeometryInsertValue(TableColumn column, object value, int? srid = null)
        {
            string str = this.GetCorrectGeometryText(value, column.DataType.ToLower());

            string strValue = "";

            if (str.Length > 4000) //oracle allow max char length
            {
                strValue = "NULL";
            }
            else
            {
                string dataType = column.DataType.ToUpper();
                string dataTypeSchema = column.DataTypeSchema?.ToUpper();

                string strSrid = srid.HasValue ? $",{srid.Value}" : "";

                if (dataType == "SDO_GEOMETRY")
                {
                    strValue = $"SDO_GEOMETRY('{str}'{strSrid})";
                }
                else if (dataType == "ST_GEOMETRY")
                {
                    if (string.IsNullOrEmpty(dataTypeSchema) || dataTypeSchema == "MDSYS" || dataTypeSchema == "PUBLIC") //PUBLIC is synonyms of MDSYS
                    {
                        strValue = $"MDSYS.ST_GEOMETRY(SDO_GEOMETRY('{str}'{strSrid}))";
                    }
                    else if (dataTypeSchema == "SDE")
                    {
                        strValue = $"SDE.ST_GEOMETRY('{str}'{strSrid})";
                    }
                }
            }

            return strValue;
        }

        private string GetOracleDatetimeConvertString(DateTime dateTime)
        {
            int millisecondLength = dateTime.Millisecond.ToString().Length;
            string strMillisecond = millisecondLength == 0 ? "" : $".{"f".PadLeft(millisecondLength, 'f')}";
            string format = $"yyyy-MM-dd HH:mm:ss{strMillisecond}";

            return $"TO_TIMESTAMP('{dateTime.ToString(format)}','yyyy-MM-dd hh24:mi:ssxff')";
        }

        private string GetCorrectGeometryText(object value, string dataType)
        {
            if (value is SqlGeography sg)
            {
                if (this.databaseType != DatabaseType.SqlServer && dataType != "geography")
                {
                    return SqlGeographyHelper.ToPostgresGeometry(sg).AsText();
                }
            }
            else if (value is PgGeom.Geometry pg)
            {
                if (pg.UserData != null && pg.UserData is PostgresGeometryCustomInfo pgi)
                {
                    if (pgi.IsGeography)
                    {
                        if (this.databaseType != DatabaseType.Postgres && dataType != "geography")
                        {
                            PostgresGeometryHelper.ReverseCoordinates(pg);

                            return pg.ToString();
                        }
                    }
                }
            }

            return value.ToString();
        }
        #endregion

        #region Append Scripts
        public string GetScriptOutputFilePath(GenerateScriptMode generateScriptMode)
        {
            string database = this.dbInterpreter.ConnectionInfo.Database;
            string databaseName = !string.IsNullOrEmpty(database) && File.Exists(database) ? Path.GetFileNameWithoutExtension(database) : database;

            string fileName = $"{databaseName}_{this.databaseType}_{DateTime.Today.ToString("yyyyMMdd")}_{generateScriptMode.ToString()}.sql";
            string filePath = Path.Combine(this.option.ScriptOutputFolder, fileName);
            return filePath;
        }

        public virtual void AppendScriptsToFile(string content, GenerateScriptMode generateScriptMode, bool overwrite = false)
        {
            if (generateScriptMode == GenerateScriptMode.Schema)
            {
                content = StringHelper.ToSingleEmptyLine(content);
            }

            string filePath = this.GetScriptOutputFilePath(generateScriptMode);

            string directoryName = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (!overwrite)
            {
                File.AppendAllText(filePath, content, Encoding.UTF8);
            }
            else
            {
                File.WriteAllText(filePath, content, Encoding.UTF8);
            }
        }

        public void ClearScriptFile(GenerateScriptMode generateScriptMode)
        {
            string filePath = this.GetScriptOutputFilePath(generateScriptMode);

            if (File.Exists(filePath))
            {
                File.WriteAllText(filePath, "", Encoding.UTF8);
            }
        }
        #endregion

        #region Alter Table
        public abstract Script RenameTable(Table table, string newName);

        public abstract Script SetTableComment(Table table, bool isNew = true);

        public abstract Script AddTableColumn(Table table, TableColumn column);

        public abstract Script RenameTableColumn(Table table, TableColumn column, string newName);

        public abstract Script AlterTableColumn(Table table, TableColumn newColumn, TableColumn oldColumn);

        public abstract Script SetTableColumnComment(Table table, TableColumn column, bool isNew = true);

        public abstract Script DropTableColumn(TableColumn column);

        public abstract Script DropPrimaryKey(TablePrimaryKey primaryKey);

        public abstract Script DropForeignKey(TableForeignKey foreignKey);

        public abstract Script AddPrimaryKey(TablePrimaryKey primaryKey);

        public abstract Script AddForeignKey(TableForeignKey foreignKey);

        public abstract Script AddIndex(TableIndex index);

        public abstract Script DropIndex(TableIndex index);

        public abstract Script AddCheckConstraint(TableConstraint constraint);

        public abstract Script DropCheckConstraint(TableConstraint constraint);

        public abstract Script SetIdentityEnabled(TableColumn column, bool enabled);
        #endregion

        #region Database Operation  
        public abstract Script CreateSchema(DatabaseSchema schema);
        public abstract Script CreateUserDefinedType(UserDefinedType userDefinedType);
        public abstract Script CreateSequence(Sequence sequence);
        public abstract ScriptBuilder CreateTable(Table table, IEnumerable<TableColumn> columns,
           TablePrimaryKey primaryKey,
           IEnumerable<TableForeignKey> foreignKeys,
           IEnumerable<TableIndex> indexes,
           IEnumerable<TableConstraint> constraints);
        public abstract Script DropUserDefinedType(UserDefinedType userDefinedType);
        public abstract Script DropSequence(Sequence sequence);
        public abstract Script DropTable(Table table);
        public abstract Script DropView(View view);
        public abstract Script DropTrigger(TableTrigger trigger);
        public abstract Script DropFunction(Function function);
        public abstract Script DropProcedure(Procedure procedure);
        public abstract IEnumerable<Script> SetConstrainsEnabled(bool enabled);

        public virtual Script Create(DatabaseObject dbObject)
        {
            if (dbObject is TableColumn column)
            {
                return this.AddTableColumn(new Table() { Schema = column.Schema, Name = column.TableName }, column);
            }
            else if (dbObject is TablePrimaryKey primaryKey)
            {
                return this.AddPrimaryKey(primaryKey);
            }
            else if (dbObject is TableForeignKey foreignKey)
            {
                return this.AddForeignKey(foreignKey);
            }
            else if (dbObject is TableIndex index)
            {
                return this.AddIndex(index);
            }
            else if (dbObject is TableConstraint constraint)
            {
                return this.AddCheckConstraint(constraint);
            }
            else if (dbObject is UserDefinedType userDefinedType)
            {
                return this.CreateUserDefinedType(userDefinedType);
            }
            else if (dbObject is ScriptDbObject scriptDbObject)
            {
                return new CreateDbObjectScript<ScriptDbObject>(scriptDbObject.Definition);
            }

            throw new NotSupportedException($"Not support to add {dbObject.GetType().Name} using this method.");
        }

        public virtual Script Drop(DatabaseObject dbObject)
        {
            if (dbObject is TableColumn column)
            {
                return this.DropTableColumn(column);
            }
            else if (dbObject is TablePrimaryKey primaryKey)
            {
                return this.DropPrimaryKey(primaryKey);
            }
            else if (dbObject is TableForeignKey foreignKey)
            {
                return this.DropForeignKey(foreignKey);
            }
            else if (dbObject is TableIndex index)
            {
                return this.DropIndex(index);
            }
            else if (dbObject is TableConstraint constraint)
            {
                return this.DropCheckConstraint(constraint);
            }
            else if (dbObject is TableTrigger trigger)
            {
                return this.DropTrigger(trigger);
            }
            else if (dbObject is View view)
            {
                return this.DropView(view);
            }
            else if (dbObject is Function function)
            {
                return this.DropFunction(function);
            }
            else if (dbObject is Procedure procedure)
            {
                return this.DropProcedure(procedure);
            }
            else if (dbObject is Table table)
            {
                return this.DropTable(table);
            }
            else if (dbObject is UserDefinedType userDefinedType)
            {
                return this.DropUserDefinedType(userDefinedType);
            }
            else if (dbObject is Sequence sequence)
            {
                return this.DropSequence(sequence);
            }

            throw new NotSupportedException($"Not support to drop {dbObject.GetType().Name}.");
        }
        #endregion

        #region Common Method
        public string GetQuotedString(string str)
        {
            return this.dbInterpreter.GetQuotedString(str);
        }

        public string GetQuotedColumnNames(IEnumerable<TableColumn> columns)
        {
            return this.dbInterpreter.GetQuotedColumnNames(columns);
        }

        public string GetQuotedDbObjectNameWithSchema(DatabaseObject dbObject)
        {
            return this.dbInterpreter.GetQuotedDbObjectNameWithSchema(dbObject);
        }

        public string GetQuotedDbObjectNameWithSchema(string schema, string dbObjName)
        {
            return this.dbInterpreter.GetQuotedDbObjectNameWithSchema(schema, dbObjName);
        }

        public string GetQuotedFullTableName(Table table)
        {
            return this.GetQuotedDbObjectNameWithSchema(table);
        }

        public string GetQuotedFullTableName(TableChild tableChild)
        {
            if (string.IsNullOrEmpty(tableChild.Schema))
            {
                return this.GetQuotedString(tableChild.TableName);
            }
            else
            {
                return $"{this.GetQuotedString(tableChild.Schema)}.{this.GetQuotedString(tableChild.TableName)}";
            }
        }

        public string GetQuotedFullTableChildName(TableChild tableChild)
        {
            string fullTableName = this.GetQuotedFullTableName(tableChild);
            return $"{fullTableName}.{this.GetQuotedString(tableChild.Name)}";
        }

        public string TransferSingleQuotationString(string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return comment;
            }

            return ValueHelper.TransferSingleQuotation(comment);
        }

        protected string GetCreateTableOption()
        {
            CreateTableOption option = CreateTableOptionManager.GetCreateTableOption(this.databaseType);

            if (option == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            Action<string> appendValue = (value) =>
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }

                    sb.Append(value);
                }
            };

            foreach(var item in option.Items)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    string[] items = item.Split(CreateTableOptionManager.OptionValueItemsSeperator);

                    foreach (var subItem in items)
                    {
                        appendValue(subItem);
                    }
                }
            }       

            return sb.ToString();
        }
        #endregion

        #region Feedback
        public void FeedbackInfo(OperationState state, DatabaseObject dbObject)
        {
            this.dbInterpreter.FeedbackInfo(state, dbObject);
        }

        public void FeedbackInfo(string message)
        {
            this.dbInterpreter.FeedbackInfo(message);
        }

        public void FeedbackError(string message, bool skipError = false)
        {
            this.dbInterpreter.FeedbackError(message, skipError);
        }
        #endregion
    }
}
