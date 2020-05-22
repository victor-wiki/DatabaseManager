using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            int i = 0;
            int pickupIndex = -1;

            if (schemaInfo.PickupTable != null)
            {
                foreach (Table table in schemaInfo.Tables)
                {
                    if (table.Owner == schemaInfo.PickupTable.Owner && table.Name == schemaInfo.PickupTable.Name)
                    {
                        pickupIndex = i;
                        break;
                    }
                    i++;
                }
            }

            i = 0;

            using (DbConnection connection = this.dbInterpreter.CreateConnection())
            {
                int tableCount = schemaInfo.Tables.Count - (pickupIndex == -1 ? 0 : pickupIndex);
                int count = 0;

                foreach (Table table in schemaInfo.Tables)
                {
                    if (this.dbInterpreter.CancelRequested)
                    {
                        break;
                    }

                    if (i < pickupIndex)
                    {
                        i++;
                        continue;
                    }

                    count++;

                    string strTableCount = $"({count}/{tableCount})";
                    string tableName = table.Name;

                    List<TableColumn> columns = schemaInfo.TableColumns.Where(item => item.Owner == table.Owner && item.TableName == tableName).OrderBy(item => item.Order).ToList();

                    bool isSelfReference = TableReferenceHelper.IsSelfReference(tableName, schemaInfo.TableForeignKeys);

                    TablePrimaryKey primaryKey = schemaInfo.TablePrimaryKeys.FirstOrDefault(item => item.Owner == table.Owner && item.TableName == tableName);

                    string primaryKeyColumns = primaryKey == null ? "" : string.Join(",", primaryKey.Columns.OrderBy(item => item.Order).Select(item => this.GetQuotedString(item.ColumnName)));

                    long total = await this.dbInterpreter.GetTableRecordCountAsync(connection, table);

                    if (this.option.DataGenerateThreshold.HasValue && total > this.option.DataGenerateThreshold.Value)
                    {
                        continue;
                    }

                    int pageSize = this.dbInterpreter.DataBatchSize;

                    this.FeedbackInfo($"{strTableCount}Table \"{table.Name}\":record count is {total}.");

                    Dictionary<long, List<Dictionary<string, object>>> dictPagedData;

                    if (isSelfReference)
                    {
                        string parentColumnName = schemaInfo.TableForeignKeys.FirstOrDefault(item =>
                            item.Owner == table.Owner
                            && item.TableName == tableName
                            && item.ReferencedTableName == tableName)?.Columns?.FirstOrDefault()?.ColumnName;

                        string strWhere = $" WHERE { this.GetQuotedString(parentColumnName)} IS NULL";

                        dictPagedData = await this.GetSortedPageData(connection, table, primaryKeyColumns, parentColumnName, columns, strWhere);
                    }
                    else
                    {
                        dictPagedData = await this.dbInterpreter.GetPagedDataListAsync(connection, table, columns, primaryKeyColumns, total, pageSize);
                    }

                    this.FeedbackInfo($"{strTableCount}Table \"{table.Name}\":data read finished.");

                    if (count > 1)
                    {
                        if (this.option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToString))
                        {
                            sb.AppendLine();
                        }
                        else if (this.option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
                        {
                            this.AppendScriptsToFile(Environment.NewLine, GenerateScriptMode.Data);
                        }
                    }

                    i++;

                    if (this.option.BulkCopy && this.dbInterpreter.SupportBulkCopy && !this.option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
                    {
                        continue;
                    }
                    else
                    {
                        this.AppendDataScripts(sb, table, columns, dictPagedData);
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

        private async Task<Dictionary<long, List<Dictionary<string, object>>>> GetSortedPageData(DbConnection connection, Table table, string primaryKeyColumns, string parentColumnName, List<TableColumn> columns, string whereClause = "")
        {
            string quotedTableName = this.GetQuotedObjectName(table);

            int pageSize = this.dbInterpreter.DataBatchSize;

            long total = Convert.ToInt64(await this.dbInterpreter.GetScalarAsync(connection, $"SELECT COUNT(1) FROM {quotedTableName} {whereClause}"));

            var dictPagedData = await this.dbInterpreter.GetPagedDataListAsync(connection, table, columns, primaryKeyColumns, total, pageSize, whereClause);

            List<object> parentValues = dictPagedData.Values.SelectMany(item => item.Select(t => t[primaryKeyColumns.Trim(this.dbInterpreter.QuotationLeftChar, this.dbInterpreter.QuotationRightChar)])).ToList();

            if (parentValues.Count > 0)
            {
                TableColumn parentColumn = columns.FirstOrDefault(item => item.Owner == table.Owner && item.Name == parentColumnName);

                long parentValuesPageCount = PaginationHelper.GetPageCount(parentValues.Count, this.option.InQueryItemLimitCount);

                for (long parentValuePageNumber = 1; parentValuePageNumber <= parentValuesPageCount; parentValuePageNumber++)
                {
                    IEnumerable<object> pagedParentValues = parentValues.Skip((int)(parentValuePageNumber - 1) * pageSize).Take(this.option.InQueryItemLimitCount);
                    whereClause = $" WHERE { this.GetQuotedString(parentColumnName)} IN ({string.Join(",", pagedParentValues.Select(item => this.ParseValue(parentColumn, item, true)))})";
                    total = Convert.ToInt64(await this.dbInterpreter.GetScalarAsync(connection, $"SELECT COUNT(1) FROM {quotedTableName} {whereClause}"));

                    if (total > 0)
                    {
                        Dictionary<long, List<Dictionary<string, object>>> dictChildPagedData = await this.GetSortedPageData(connection, table, primaryKeyColumns, parentColumnName, columns, whereClause);

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

        public virtual Dictionary<string, object> AppendDataScripts(StringBuilder sb, Table table, List<TableColumn> columns, Dictionary<long, List<Dictionary<string, object>>> dictPagedData)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            bool appendString = this.option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToString);
            bool appendFile = this.option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile);

            List<string> excludeColumnNames = new List<string>();

            if (this.option.TableScriptsGenerateOption.GenerateIdentity && !this.option.InsertIdentityValue)
            {
                excludeColumnNames.AddRange(columns.Where(item => item.IsIdentity).Select(item => item.Name));
            }

            excludeColumnNames.AddRange(columns.Where(item => item.IsComputed).Select(item => item.Name));

            foreach (var kp in dictPagedData)
            {
                StringBuilder sbFilePage = new StringBuilder();

                string tableName = this.GetQuotedObjectName(table);
                string insert = $"{this.GetBatchInsertPrefix()} {tableName}({this.GetQuotedColumnNames(columns.Where(item => !excludeColumnNames.Contains(item.Name)))})VALUES";

                if (appendString)
                {
                    if (kp.Key > 1)
                    {
                        sb.AppendLine();
                    }

                    sb.AppendLine(insert);
                }

                if (appendFile)
                {
                    if (kp.Key > 1)
                    {
                        sbFilePage.AppendLine();
                    }

                    sbFilePage.AppendLine(insert);
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

                    string beginChar = this.GetBatchInsertItemBefore(tableName, rowCount == 1);
                    string endChar = this.GetBatchInsertItemEnd(rowCount == kp.Value.Count);

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

        protected virtual string GetBatchInsertItemBefore(string tableName, bool isFirstRow)
        {
            return "";
        }

        protected virtual string GetBatchInsertItemEnd(bool isAllEnd)
        {
            return (isAllEnd ? ";" : ",");
        }

        private List<object> GetRowValues(Dictionary<string, object> row, int rowIndex, List<TableColumn> columns, List<string> excludeColumnNames, long pageNumber, bool isAppendToFile, out Dictionary<string, object> parameters)
        {
            parameters = new Dictionary<string, object>();

            List<object> values = new List<object>();

            foreach (TableColumn column in columns)
            {
                if (!excludeColumnNames.Contains(column.Name))
                {
                    object value = this.ParseValue(column, row[column.Name]);
                    bool isBytes = ValueHelper.IsBytes(value);

                    if (!isAppendToFile)
                    {
                        if ((isBytes && !this.option.TreatBytesAsNullForExecuting) || this.NeedInsertParameter(column, value))
                        {
                            string parameterName = $"P{pageNumber}_{rowIndex}_{column.Name}";

                            string parameterPlaceholder = this.dbInterpreter.CommandParameterChar + parameterName;

                            parameters.Add(parameterPlaceholder, value);

                            value = parameterPlaceholder;
                        }
                        else if (isBytes && this.option.TreatBytesAsNullForExecuting)
                        {
                            value = null;
                        }
                    }
                    else
                    {
                        if (isBytes)
                        {
                            if (this.option.TreatBytesAsHexStringForFile)
                            {
                                value = this.GetBytesConvertHexString(value, column.DataType);
                            }
                            else
                            {
                                value = null;
                            }
                        }
                    }

                    values.Add(value);
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

                switch (type.Name)
                {
                    case nameof(Guid):

                        needQuotated = true;
                        if (this.databaseType == DatabaseType.Oracle && column.DataType.ToLower() == "raw" && column.MaxLength == 16)
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
                        }
                        break;

                    case nameof(DateTime):
                    case nameof(DateTimeOffset):
                    case nameof(MySql.Data.Types.MySqlDateTime):

                        if (this.databaseType == DatabaseType.Oracle)
                        {
                            if (type.Name == nameof(MySql.Data.Types.MySqlDateTime))
                            {
                                DateTime dateTime = ((MySql.Data.Types.MySqlDateTime)value).GetDateTime();

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
                            if (type.Name == nameof(DateTimeOffset))
                            {
                                DateTimeOffset dtOffset = DateTimeOffset.Parse(value.ToString());

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

                        strValue = value.ToString() == "True" ? "1" : "0";
                        break;

                    case nameof(TimeSpan):

                        if (this.databaseType == DatabaseType.Oracle)
                        {
                            return value;
                        }
                        else
                        {
                            needQuotated = true;

                            if (column.DataType.IndexOf("datetime", StringComparison.OrdinalIgnoreCase) >= 0)
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

                    case "SqlHierarchyId":
                    case "SqlGeography":
                    case "SqlGeometry":

                        needQuotated = true;
                        strValue = value.ToString();
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
                    strValue = $"{this.dbInterpreter.UnicodeInsertChar}'{ValueHelper.TransferSingleQuotation(strValue)}'";

                    if (oracleSemicolon)
                    {
                        strValue = strValue.Replace(";", $"'{OracleInterpreter.CONNECT_CHAR}{OracleInterpreter.SEMICOLON_FUNC}{OracleInterpreter.CONNECT_CHAR}'");
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

        private string GetOracleDatetimeConvertString(DateTime dateTime)
        {
            int millisecondLength = dateTime.Millisecond.ToString().Length;
            string strMillisecond = millisecondLength == 0 ? "" : $".{"f".PadLeft(millisecondLength, 'f')}";
            string format = $"yyyy-MM-dd HH:mm:ss{strMillisecond}";

            return $"TO_TIMESTAMP('{dateTime.ToString(format)}','yyyy-MM-dd hh24:mi:ssxff')";
        }
        #endregion

        #region Append Scripts
        public string GetScriptOutputFilePath(GenerateScriptMode generateScriptMode)
        {
            string fileName = $"{this.dbInterpreter.ConnectionInfo.Database}_{this.databaseType}_{DateTime.Today.ToString("yyyyMMdd")}_{generateScriptMode.ToString()}.sql";
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

        public abstract Script AlterTableColumn(Table table, TableColumn column);

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
        public abstract Script DropUserDefinedType(UserDefinedType userDefinedType);
        public abstract Script DropTable(Table table);
        public abstract Script DropView(View view);
        public abstract Script DropTrigger(TableTrigger trigger);
        public abstract Script DropFunction(Function function);
        public abstract Script DropProcedure(Procedure procedure);
        public abstract IEnumerable<Script> SetConstrainsEnabled(bool enabled);
        #endregion

        #region Common Method
        public string GetQuotedObjectName(DatabaseObject obj)
        {
            return this.dbInterpreter.GetQuotedObjectName(obj);
        }

        public string GetQuotedString(string str)
        {
            return this.dbInterpreter.GetQuotedString(str);
        }

        public string GetQuotedColumnNames(IEnumerable<TableColumn> columns)
        {
            return this.dbInterpreter.GetQuotedColumnNames(columns);
        }

        public string GetQuotedFullTableName(TableChild tableChild)
        {
            if (string.IsNullOrEmpty(tableChild.Owner))
            {
                return this.GetQuotedString(tableChild.TableName);
            }
            else
            {
                return $"{tableChild.Owner}.{this.GetQuotedString(tableChild.TableName)}";
            }
        }

        public string TransferSingleQuotationString(string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return comment;
            }

            return ValueHelper.TransferSingleQuotation(comment);
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
