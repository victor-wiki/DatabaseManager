using DatabaseConverter.Core;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core.Model;
using DatabaseManager.FileUtility;
using DatabaseManager.FileUtility.Model;
using SixLabors.Fonts;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseManager.Core
{
    public class DataCompare
    {
        private DbInterpreter sourceDbInterpreter;
        private DbInterpreter targetDbInterpreter;
        private SchemaInfo schemaInfo;
        private DataCompareOption option;
        private IObserver<FeedbackInfo> observer;

        public const string DifferentDataSourceColumnNameSuffix = "__source";
        public const string DifferentDataTargetColumnNameSuffix = "__target";


        public DataCompare(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, SchemaInfo schemaInfo, DataCompareOption option = null)
        {
            this.sourceDbInterpreter = sourceDbInterpreter;
            this.targetDbInterpreter = targetDbInterpreter;
            this.schemaInfo = schemaInfo;

            if (option == null)
            {
                option = new DataCompareOption();
            }

            this.option = option;
        }

        public async Task<DataCompareResult> Compare(CancellationToken cancellationToken)
        {
            DataCompareResult result = new DataCompareResult();

            var sourceSchemaInfoWithAllTables = await this.sourceDbInterpreter.GetSchemaInfoAsync(new SchemaInfoFilter { DatabaseObjectType = DatabaseObjectType.Table | DatabaseObjectType.ForeignKey });

            var allTables = sourceSchemaInfoWithAllTables.Tables;

            var sortedTableNames = TableReferenceHelper.ResortTableNames(allTables.Select(item => item.Name).ToArray(), sourceSchemaInfoWithAllTables.TableForeignKeys);

            var tableNames = this.schemaInfo.Tables.Select(item => item.Name).ToArray();

            SchemaInfoFilter sourceFilter = new SchemaInfoFilter() { DatabaseObjectType = DatabaseObjectType.Column | DatabaseObjectType.PrimaryKey, TableNames = tableNames };
            SchemaInfoFilter targetFilter = new SchemaInfoFilter() { DatabaseObjectType = DatabaseObjectType.Table | DatabaseObjectType.Column | DatabaseObjectType.PrimaryKey, TableNames = tableNames };

            var sourceSchemaInfo = await this.sourceDbInterpreter.GetSchemaInfoAsync(sourceFilter);
            var targetSchemaInfo = await this.targetDbInterpreter.GetSchemaInfoAsync(targetFilter);

            tableNames = sortedTableNames.Where(item => tableNames.Contains(item)).ToArray();

            using (var sourceConnection = this.sourceDbInterpreter.CreateConnection())
            {
                using (var targetConnnection = this.targetDbInterpreter.CreateConnection())
                {
                    int count = 0;
                    int totalTablesCount = tableNames.Count();

                    int tableCount = tableNames.Length;

                    foreach (var tableName in tableNames)
                    {
                        count++;

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        this.FeedbackInfo($@"Begin to process table ""{tableName}""({count}/{tableCount}).");

                        Table targetTable = targetSchemaInfo.Tables.FirstOrDefault(item => item.Name == tableName);

                        if (targetTable != null)
                        {
                            Table sourceTable = allTables.FirstOrDefault(item => item.Name == tableName);

                            long sourceRecordCount = await this.sourceDbInterpreter.GetTableRecordCountAsync(sourceConnection, sourceTable);
                            long targetRecordCount = await this.sourceDbInterpreter.GetTableRecordCountAsync(targetConnnection, targetTable);

                            this.FeedbackInfo($@"Table ""{tableName} has {sourceRecordCount} records.");

                            DataCompareResultDetail resultDetail = new DataCompareResultDetail { Order = count, SourceTable = sourceTable, TargetTable = targetTable, SourceTableRecordCount = sourceRecordCount, TargetTableRecordCount = targetRecordCount };

                            result.Details.Add(resultDetail);

                            var sourcePrimaryKey = sourceSchemaInfo.TablePrimaryKeys.FirstOrDefault(item => item.TableName == tableName && item.Schema == sourceTable.Schema);

                            var targetPrimaryKey = targetSchemaInfo.TablePrimaryKeys.FirstOrDefault(item => item.TableName == tableName);

                            if (sourcePrimaryKey != null && targetPrimaryKey != null && this.IsPrimaryKeyColumnsAreEqual(sourcePrimaryKey.Columns, targetPrimaryKey.Columns))
                            {
                                var primaryKeyColumns = sourcePrimaryKey.Columns;
                                int primaryKeyColumnsCount = primaryKeyColumns.Count;

                                var sourceTableColumns = sourceSchemaInfo.TableColumns.Where(item => item.TableName == tableName && item.Schema == sourceTable.Schema).ToList();
                                var targetTableColumns = targetSchemaInfo.TableColumns.Where(item => item.TableName == tableName && item.Schema == targetTable.Schema).ToList(); ;

                                resultDetail.SourceTableColumns = sourceTableColumns;
                                resultDetail.TargetTableColumns = targetTableColumns;

                                var keyColumns = sourceTableColumns.Where(item => primaryKeyColumns.Any(t => t.ColumnName == item.Name)).ToList();

                                resultDetail.KeyColumns = keyColumns;

                                var sameTableColumns = targetTableColumns.Where(item => sourceTableColumns.Any(s => s.Name == item.Name)).ToList();

                                resultDetail.SameTableColumns = sameTableColumns;

                                string strColumnNames = string.Join(",", this.sourceDbInterpreter.GetQuotedColumnNames(keyColumns));
                                string orderColumns = strColumnNames;

                                string sourcePrimaryKeySql = $"select {strColumnNames} from {this.sourceDbInterpreter.GetQuotedDbObjectNameWithSchema(sourceTable)} order by {orderColumns}";
                                string targetPrimaryKeySql = $"select {strColumnNames} from {this.sourceDbInterpreter.GetQuotedDbObjectNameWithSchema(targetTable)} order by {orderColumns}";

                                DataTable sourcePrimaryKeyDataTable = await this.sourceDbInterpreter.GetDataTableAsync(sourceConnection, sourcePrimaryKeySql);
                                DataTable targetPrimaryKeyDataTable = await this.sourceDbInterpreter.GetDataTableAsync(targetConnnection, targetPrimaryKeySql);

                                var groupedPrimaryKeyRows = this.GetGroupedDataRows(sourcePrimaryKeyDataTable.Rows.Cast<DataRow>().ToList(), targetPrimaryKeyDataTable.Rows.Cast<DataRow>().ToList());

                                DataCompareDisplayMode displayMode = this.option.DisplayMode;

                                if (displayMode.HasFlag(DataCompareDisplayMode.OnlyInSource) && groupedPrimaryKeyRows.ContainsKey(DataCompareDisplayMode.OnlyInSource))
                                {
                                    resultDetail.OnlyInSourceKeyRows = groupedPrimaryKeyRows[DataCompareDisplayMode.OnlyInSource];
                                }

                                if (displayMode.HasFlag(DataCompareDisplayMode.OnlyInTarget) && groupedPrimaryKeyRows.ContainsKey(DataCompareDisplayMode.OnlyInTarget))
                                {
                                    resultDetail.OnlyInTargetKeyRows = groupedPrimaryKeyRows[DataCompareDisplayMode.OnlyInTarget];
                                }

                                if ((displayMode.HasFlag(DataCompareDisplayMode.Different) || displayMode.HasFlag(DataCompareDisplayMode.Indentical)) && groupedPrimaryKeyRows.ContainsKey(DataCompareDisplayMode.Indentical))
                                {
                                    var sameKeyDataRows = groupedPrimaryKeyRows[DataCompareDisplayMode.Indentical];

                                    int total = sameKeyDataRows.Count;
                                    int pageSize = primaryKeyColumnsCount == 1 ? 500 : 100;
                                    int pageCount = total % pageSize == 0 ? total / pageSize : total / pageSize + 1;

                                    long recordsCount = 0;

                                    for (int i = 1; i <= pageCount; i++)
                                    {
                                        if (cancellationToken.IsCancellationRequested)
                                        {
                                            break;
                                        }

                                        var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(i, pageSize);

                                        long starRowNumber = startEndRowNumber.StartRowNumber;
                                        long endRowNumber = startEndRowNumber.EndRowNumber;

                                        if (endRowNumber > total)
                                        {
                                            endRowNumber = total;
                                        }

                                        recordsCount += endRowNumber - starRowNumber + 1;

                                        this.FeedbackInfo($@"({count}/{totalTablesCount})Reading table ""{tableName}"" {recordsCount}/{total} records.");

                                        List<DataRow> keyRows = sameKeyDataRows.Skip((int)(starRowNumber - 1)).Take((int)(endRowNumber - starRowNumber + 1)).ToList();

                                        string whereCondition = GetKeyColumnWhereCondition(this.sourceDbInterpreter, keyRows, keyColumns);

                                        DataTable sourceDataTable = await this.sourceDbInterpreter.GetPagedDataTableAsync(sourceConnection, sourceTable, sameTableColumns, null, pageSize, 1, whereCondition);

                                        DataTable targetDataTable = await this.sourceDbInterpreter.GetPagedDataTableAsync(targetConnnection, targetTable, sameTableColumns, null, pageSize, 1, whereCondition);

                                        (List<DataRow> SameKeyRows, List<DataRow> DifferentKeyRows) res = this.CompareDataTable(sourceDataTable, targetDataTable, keyRows, sameTableColumns, keyColumns);

                                        if (res.SameKeyRows.Count > 0)
                                        {
                                            resultDetail.IdenticalKeyRows.AddRange(res.SameKeyRows);
                                        }

                                        if (res.DifferentKeyRows.Count > 0)
                                        {
                                            resultDetail.DifferentKeyRows.AddRange(res.DifferentKeyRows);
                                        }
                                    }
                                }
                            }
                        }

                        this.FeedbackInfo($@"End process table ""{tableName}""({count}/{tableCount}).");
                    }
                }
            }

            return result;
        }

        private (List<DataRow> SameKeyRows, List<DataRow> DifferentKeyRows) CompareDataTable(DataTable sourceDataTable, DataTable targetDatatable, List<DataRow> keyRows, List<TableColumn> columns, List<TableColumn> keyColumns)
        {
            List<DataRow> sameKeyRows = new List<DataRow>();
            List<DataRow> differentKeyRows = new List<DataRow>();

            List<DataRow> sameRows = sourceDataTable.AsEnumerable().Intersect(targetDatatable.AsEnumerable(), DataRowComparer.Default).ToList();

            foreach (DataRow sameRow in sameRows)
            {
                string keyValue = this.GetKeyValue(sameRow, keyColumns);

                DataRow keyRow = this.FindKeyRow(keyRows, keyValue, keyColumns);

                if (keyRow != null)
                {
                    sameKeyRows.Add(keyRow);

                    keyRows.Remove(keyRow);
                }
            }

            differentKeyRows.AddRange(keyRows);

            return (sameKeyRows, differentKeyRows);
        }

        private DataRow FindKeyRow(List<DataRow> rows, string keyValue, List<TableColumn> keyColumns)
        {
            foreach (var row in rows)
            {
                if (keyValue == this.GetKeyValue(row, keyColumns))
                {
                    return row;
                }
            }

            return null;
        }

        private string GetKeyValue(DataRow row, List<TableColumn> keyColumns)
        {
            List<string> values = new List<string>();

            foreach (var column in keyColumns)
            {
                values.Add(row[column.Name]?.ToString());
            }

            return string.Join("_", values);
        }

        public static string GetKeyColumnWhereCondition(DbInterpreter dbInterpreter, List<DataRow> keyRows, List<TableColumn> columns)
        {
            if (keyRows == null || keyRows.Count == 0)
            {
                return string.Empty;
            }

            DatabaseType databaseType = dbInterpreter.DatabaseType;

            Dictionary<string, bool> dictIsCharType = new Dictionary<string, bool>();

            foreach (DataColumn column in keyRows.First().Table.Columns)
            {
                string columnName = column.ColumnName;

                TableColumn tableColumn = columns.FirstOrDefault(item => item.Name == columnName);

                if (tableColumn != null)
                {
                    string dataType = tableColumn.DataType;

                    bool isCharType = DataTypeHelper.IsCharType(dataType);

                    dictIsCharType.Add(columnName, isCharType);
                }
            }

            int columnCount = columns.Count;

            List<string> singleColumnValues = new List<string>();
            List<Dictionary<string, string>> rowDatas = new List<Dictionary<string, string>>();

            for (int i = 0; i < keyRows.Count; i++)
            {
                DataRow row = keyRows[i];

                if (columnCount == 1)
                {
                    singleColumnValues.Add(row[0]?.ToString());
                }
                else
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>();

                    foreach (DataColumn column in row.Table.Columns)
                    {
                        string columnName = column.ColumnName;

                        dict.Add(columnName, row[columnName]?.ToString());
                    }

                    rowDatas.Add(dict);
                }
            }

            string condition = null;

            if (columnCount == 1)
            {
                string columnName = columns[0].Name;

                bool isCharType = dictIsCharType[columnName];

                string strValues = string.Join(",", singleColumnValues.Select(item => isCharType ? (item == null ? "null" : $"'{item}'") : item));

                string strInOrEqual = keyRows.Count == 1 ? $" = {strValues}" : $" in ({strValues})";

                condition = $"{dbInterpreter.GetQuotedString(columnName)}{strInOrEqual}";
            }
            else
            {
                List<string> rowConditions = new List<string>();

                foreach (Dictionary<string, string> rowData in rowDatas)
                {
                    List<string> columnConditions = new List<string>();

                    foreach (var kp in rowData)
                    {
                        string columName = kp.Key;
                        string value = kp.Value;

                        bool isCharType = dictIsCharType[columName];

                        columnConditions.Add($"{dbInterpreter.GetQuotedString(columName)}{(value == null ? "is" : "=")}{(value == null ? "null" : (isCharType ? $"'{value}'" : value))}");
                    }

                    rowConditions.Add($"({string.Join(" and ", columnConditions)})");
                }

                condition = $"({string.Join(" or ", rowConditions)})";
            }

            return $"where {condition}";
        }

        private bool IsPrimaryKeyColumnsAreEqual(List<IndexColumn> indexColumns1, List<IndexColumn> indexColumns2)
        {
            return indexColumns1.Count == indexColumns2.Count && indexColumns1.All(item => indexColumns2.Any(t => t.ColumnName == item.ColumnName));
        }

        private Dictionary<DataCompareDisplayMode, List<DataRow>> GetGroupedDataRows(List<DataRow> rows1, List<DataRow> rows2)
        {
            Dictionary<DataCompareDisplayMode, List<DataRow>> dict = new Dictionary<DataCompareDisplayMode, List<DataRow>>();

            DataCompareDisplayMode displayMode = this.option.DisplayMode;

            if (displayMode.HasFlag(DataCompareDisplayMode.Different) || displayMode.HasFlag(DataCompareDisplayMode.Indentical))
            {
                List<DataRow> sameRows = rows1.Intersect(rows2, DataRowComparer.Default).ToList();

                if (sameRows.Count > 0)
                {
                    dict.Add(DataCompareDisplayMode.Indentical, sameRows);
                }
            }

            if (displayMode.HasFlag(DataCompareDisplayMode.OnlyInSource))
            {
                List<DataRow> onlyInSourceRows = rows1.Except(rows2, DataRowComparer.Default).ToList();

                if (onlyInSourceRows.Count > 0)
                {
                    dict.Add(DataCompareDisplayMode.OnlyInSource, onlyInSourceRows);
                }
            }

            if (displayMode.HasFlag(DataCompareDisplayMode.OnlyInTarget))
            {
                List<DataRow> onlyInTargetRows = rows2.Except(rows1, DataRowComparer.Default).ToList();

                if (onlyInTargetRows.Count > 0)
                {
                    dict.Add(DataCompareDisplayMode.OnlyInTarget, onlyInTargetRows);
                }
            }

            return dict;
        }

        public static List<DataRow> GetPagedKeyRows(List<DataRow> rows, int pageSize, long pageNumber)
        {
            int total = rows.Count;
            int pageCount = total % pageSize == 0 ? total / pageSize : total / pageSize + 1;

            var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

            long starRowNumber = startEndRowNumber.StartRowNumber;
            long endRowNumber = startEndRowNumber.EndRowNumber;

            if (endRowNumber > total)
            {
                endRowNumber = total;
            }

            List<DataRow> pagedKeyRows = rows.Skip((int)(starRowNumber - 1)).Take((int)(endRowNumber - starRowNumber + 1)).ToList();

            return pagedKeyRows;
        }

        public static async Task<(DataTable Data, Dictionary<int, List<DataCompareValueInfo>> ValueInfos)> GetDifferentData(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, DataCompareResultDetail detail, int pageSize, long pageNumber)
        {
            Dictionary<int, List<DataCompareValueInfo>> dictValueInfos = new Dictionary<int, List<DataCompareValueInfo>>();

            var rows = detail.DifferentKeyRows;

            var pagedKeyRows = GetPagedKeyRows(rows, pageSize, pageNumber);

            string whereCondition = DataCompare.GetKeyColumnWhereCondition(sourceDbInterpreter, pagedKeyRows, detail.KeyColumns);

            string orderColumns = sourceDbInterpreter.GetQuotedColumnNames(detail.KeyColumns);

            DataTable sourceDataTable = await sourceDbInterpreter.GetPagedDataTableAsync(sourceDbInterpreter.CreateConnection(), detail.SourceTable, detail.SameTableColumns, orderColumns, pageSize, 1, whereCondition);
            DataTable targetDataTable = await sourceDbInterpreter.GetPagedDataTableAsync(targetDbInterpreter.CreateConnection(), detail.TargetTable, detail.SameTableColumns, orderColumns, pageSize, 1, whereCondition);

            List<DataCompareDifferentRow> differentRows = new List<DataCompareDifferentRow>();

            DataTable dataTable = new DataTable();

            for (int i = 0; i < sourceDataTable.Rows.Count; i++)
            {
                DataRow sourceRow = sourceDataTable.Rows[i];
                DataRow targetRow = targetDataTable.Rows[i];

                Dictionary<string, (object, object)> dict = new Dictionary<string, (object, object)>();

                foreach (var column in detail.SameTableColumns.Where(item => !detail.KeyColumns.Any(t => t.Name == item.Name)))
                {
                    string columnName = column.Name;

                    object sourceValue = sourceRow[columnName];
                    object targetValue = targetRow[columnName];

                    if (!Equals(sourceValue, targetValue))
                    {
                        dict.Add(columnName, (sourceValue, targetValue));
                    }
                }

                if (dict.Any())
                {
                    differentRows.Add(new DataCompareDifferentRow() { RowIndex = i, KeyRow = pagedKeyRows[i], Details = dict });
                }
            }

            foreach (var column in detail.KeyColumns)
            {
                dataTable.Columns.Add(new DataColumn() { ColumnName = column.Name });
            }

            var diffColumnNames = differentRows.SelectMany(item => item.Details.Select(t => t.Key)).Distinct();

            foreach (var name in diffColumnNames)
            {
                string columnName = name + DifferentDataSourceColumnNameSuffix;

                dataTable.Columns.Add(new DataColumn() { ColumnName = columnName });
                dataTable.Columns.Add(new DataColumn() { ColumnName = name + DifferentDataTargetColumnNameSuffix });
            }

            foreach (var row in differentRows)
            {
                int rowIndex = row.RowIndex;

                List<DataCompareValueInfo> valueInfos = new List<DataCompareValueInfo>();

                DataRow keyRow = row.KeyRow;

                DataRow dataRow = dataTable.NewRow();

                foreach (var keyColumn in detail.KeyColumns)
                {
                    valueInfos.Add(new DataCompareValueInfo() { Value = keyRow[keyColumn.Name] });
                }

                for (int i = detail.KeyColumns.Count; i < dataTable.Columns.Count; i++)
                {
                    string columnName = dataTable.Columns[i].ColumnName;
                    string originalColumnName = columnName;

                    if (columnName.EndsWith(DifferentDataSourceColumnNameSuffix))
                    {
                        originalColumnName = columnName.Replace(DifferentDataSourceColumnNameSuffix, "");
                    }
                    else if (columnName.EndsWith(DifferentDataTargetColumnNameSuffix))
                    {
                        originalColumnName = columnName.Replace(DifferentDataTargetColumnNameSuffix, "");
                    }

                    if (row.Details.ContainsKey(originalColumnName))
                    {
                        var v = row.Details[originalColumnName];

                        if (columnName.EndsWith(DifferentDataSourceColumnNameSuffix))
                        {
                            valueInfos.Add(new DataCompareValueInfo() { Value = v.Item1, IsDifferent = true });
                        }
                        else if (columnName.EndsWith(DifferentDataTargetColumnNameSuffix))
                        {
                            valueInfos.Add(new DataCompareValueInfo() { Value = v.Item2, IsDifferent = true });
                        }
                    }
                    else
                    {
                        valueInfos.Add(new DataCompareValueInfo() { Value = sourceDataTable.Rows[rowIndex][originalColumnName] });
                    }
                }

                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    DataCompareValueInfo valueInfo = valueInfos[i];

                    dataRow[i] = valueInfo.Value;
                }

                dataTable.Rows.Add(dataRow);

                dictValueInfos.Add(rowIndex, valueInfos);
            }

            return (dataTable, dictValueInfos);
        }

        public static string GetDifferentDataColumnDisplayText(string columnName)
        {
            if (columnName.EndsWith(DifferentDataSourceColumnNameSuffix))
            {
                return columnName.Replace(DifferentDataSourceColumnNameSuffix, "") + "→";
            }
            else if (columnName.EndsWith(DifferentDataTargetColumnNameSuffix))
            {
                return "→" + columnName.Replace(DifferentDataTargetColumnNameSuffix, "");
            }

            return columnName;
        }

        public async Task<string> GenerateScripts(List<DataCompareResultDetail> details, CancellationToken cancellationToken)
        {
            StringBuilder sb = new StringBuilder();

            int pageSize = 100;

            int tableCount = 0;
            int tablesTotalCount = details.Count;

            foreach (var detail in details)
            {
                tableCount++;

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (detail.OnlyInTargetCount > 0)
                {
                    sb.AppendLine(this.GetOnlyInTargetDeleteSql(detail, pageSize));
                }

                if (detail.DifferentCount > 0)
                {
                    sb.AppendLine();

                    sb.AppendLine(await this.GetDifferentUpdateSql(detail, pageSize));
                }

                if (detail.OnlyInSourceCount > 0)
                {
                    sb.AppendLine();

                    this.FeedbackInfo($@"({tableCount}/{tablesTotalCount})Begin to generate INSERT sql for table ""{detail.SourceTable.Name}""...");

                    int total = detail.OnlyInSourceCount;
                    long pageCount = PaginationHelper.GetPageCount(total, pageSize);

                    this.sourceDbInterpreter.Option.ScriptOutputMode = GenerateScriptOutputMode.WriteToString;

                    DbScriptGenerator dbScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.sourceDbInterpreter);

                    using (var sourceConnection = this.sourceDbInterpreter.CreateConnection())
                    {
                        int recordCount = 0;

                        for (int i = 1; i <= pageCount; i++)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            recordCount += (int)(i < pageCount ? pageSize : (total - (pageCount - 1) * pageSize));

                            this.FeedbackInfo($@"({tableCount}/{tablesTotalCount})Generating INSERT sql for table ""{detail.SourceTable.Name}"" ({recordCount}/{total})...");

                            var pagedKeyRows = GetPagedKeyRows(detail.OnlyInSourceKeyRows, pageSize, i);

                            string whereCondition = DataCompare.GetKeyColumnWhereCondition(this.sourceDbInterpreter, pagedKeyRows, detail.KeyColumns);

                            DataTable sourceDataTable = await this.sourceDbInterpreter.GetPagedDataTableAsync(sourceConnection, detail.SourceTable, detail.SourceTableColumns, null, pageSize, 1, whereCondition);

                            var rows = this.sourceDbInterpreter.ConvertDataTableToDictionaryList(sourceDataTable, detail.TargetTableColumns);

                            dbScriptGenerator.AppendDataScripts(sb, detail.TargetTable, detail.TargetTableColumns, new Dictionary<long, List<Dictionary<string, object>>>() { { 1, rows } });

                            sb.AppendLine();
                        }
                    }

                    this.FeedbackInfo($@"End generate INSERT sql for table ""{detail.SourceTable.Name}"".");
                }
            }

            return sb.ToString().Trim();
        }

        private string GetOnlyInTargetDeleteSql(DataCompareResultDetail detail, int pageSize)
        {
            StringBuilder sb = new StringBuilder();

            this.FeedbackInfo($@"Begin to generate DELETE sql for table ""{detail.TargetTable.Name}""...");

            int total = detail.OnlyInTargetCount;
            long pageCount = PaginationHelper.GetPageCount(total, pageSize);

            for (int i = 1; i <= pageCount; i++)
            {
                var pagedKeyRows = GetPagedKeyRows(detail.OnlyInTargetKeyRows, pageSize, i);

                string condition = GetKeyColumnWhereCondition(this.targetDbInterpreter, pagedKeyRows, detail.KeyColumns);

                string sql = $"DELETE FROM {this.targetDbInterpreter.GetQuotedString(detail.TargetTable.Name)} {condition};";

                sb.AppendLine(sql);
            }

            this.FeedbackInfo($@"End generate DELETE sql for table ""{detail.TargetTable.Name}"".");

            return sb.ToString().Trim();
        }

        private async Task<string> GetDifferentUpdateSql(DataCompareResultDetail detail, int pageSize)
        {
            StringBuilder sb = new StringBuilder();

            this.FeedbackInfo($@"Begin to generate UPDATE sql for table ""{detail.TargetTable.Name}""...");

            int total = detail.DifferentCount;
            long pageCount = PaginationHelper.GetPageCount(total, pageSize);

            DbScriptGenerator dbScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.targetDbInterpreter);

            using (var sourceConnection = this.sourceDbInterpreter.CreateConnection())
            {
                using (var targetConnection = this.targetDbInterpreter.CreateConnection())
                {
                    for (int pageNumber = 1; pageNumber <= pageCount; pageNumber++)
                    {
                        var pagedKeyRows = GetPagedKeyRows(detail.DifferentKeyRows, pageSize, pageNumber);

                        string whereCondition = DataCompare.GetKeyColumnWhereCondition(this.sourceDbInterpreter, pagedKeyRows, detail.KeyColumns);

                        string orderColumns = this.sourceDbInterpreter.GetQuotedColumnNames(detail.KeyColumns);

                        DataTable sourceDataTable = await this.sourceDbInterpreter.GetPagedDataTableAsync(sourceConnection, detail.SourceTable, detail.SameTableColumns, orderColumns, pageSize, 1, whereCondition);
                        DataTable targetDataTable = await this.sourceDbInterpreter.GetPagedDataTableAsync(targetConnection, detail.TargetTable, detail.SameTableColumns, orderColumns, pageSize, 1, whereCondition);

                        for (int i = 0; i < sourceDataTable.Rows.Count; i++)
                        {
                            DataRow sourceRow = sourceDataTable.Rows[i];
                            DataRow targetRow = targetDataTable.Rows[i];

                            Dictionary<string, object> dict = new Dictionary<string, object>();
                            List<string> updateItems = new List<string>();

                            foreach (var column in detail.SameTableColumns.Where(item => !detail.KeyColumns.Any(t => t.Name == item.Name)))
                            {
                                string columnName = column.Name;

                                object sourceValue = sourceRow[columnName];
                                object targetValue = targetRow[columnName];

                                if (sourceValue != targetValue)
                                {
                                    dict.Add(columnName, sourceValue);
                                }
                            }

                            if (dict.Any())
                            {
                                foreach (var kp in dict)
                                {
                                    object value = kp.Value;
                                    string strValue = null;

                                    if ((value == null || value == DBNull.Value))
                                    {
                                        strValue = "null";
                                    }
                                    else
                                    {
                                        var column = detail.TargetTableColumns.FirstOrDefault(item => item.Name == kp.Key);

                                        strValue = dbScriptGenerator.ParseValue(column, value, true)?.ToString();
                                    }

                                    updateItems.Add($"{kp.Key}={strValue}");
                                }
                            }

                            string keyCondition = DataCompare.GetKeyColumnWhereCondition(this.sourceDbInterpreter, new List<DataRow>() { pagedKeyRows[i] }, detail.KeyColumns);

                            string sql = $"UPDATE {this.targetDbInterpreter.GetQuotedString(detail.TargetTable.Name)} SET {string.Join(",", updateItems)} {keyCondition};";

                            sb.AppendLine(sql);
                        }

                        sb.AppendLine();
                    }
                }
            }

            this.FeedbackInfo($@"End generate UPDATE sql for table ""{detail.TargetTable.Name}"".");

            return sb.ToString().Trim();
        }

        public async Task<DataSynchronizeResult> Synchronize(List<DataCompareResultDetail> details, CancellationToken cancellationToken, List<SchemaMappingInfo> schemaMappings = null)
        {
            DataSynchronizeResult result = new DataSynchronizeResult();

            int pageSize = 100;

            try
            {
                using (var sourceConnection = this.sourceDbInterpreter.CreateConnection())
                {
                    using (var targetConnection = this.targetDbInterpreter.CreateConnection())
                    {
                        await targetConnection.OpenAsync();

                        var transaction = targetConnection.BeginTransaction();

                        foreach (var detail in details)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                await transaction.RollbackAsync();
                                break;
                            }

                            StringBuilder sb = new StringBuilder();

                            if (detail.OnlyInTargetCount > 0)
                            {
                                sb.AppendLine(this.GetOnlyInTargetDeleteSql(detail, pageSize));
                            }

                            if (detail.DifferentCount > 0)
                            {
                                sb.AppendLine(await this.GetDifferentUpdateSql(detail, pageSize));
                            }

                            string sql = sb.ToString().Trim();

                            if (sql.Length > 0)
                            {
                                await this.RunCommand(this.targetDbInterpreter, targetConnection, sql, cancellationToken, transaction);
                            }

                            if (detail.OnlyInSourceCount > 0)
                            {
                                this.FeedbackInfo($@"Begin to copy data to table ""{detail.TargetTable.Name}""...");

                                int total = detail.OnlyInSourceCount;
                                long pageCount = PaginationHelper.GetPageCount(total, pageSize);
                                int count = 0;

                                this.targetDbInterpreter.Option.ScriptOutputMode = GenerateScriptOutputMode.WriteToString;

                                DbScriptGenerator dbScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.targetDbInterpreter);

                                for (int i = 1; i <= pageCount; i++)
                                {
                                    if (cancellationToken.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    var pagedKeyRows = GetPagedKeyRows(detail.OnlyInSourceKeyRows, pageSize, i);

                                    string whereCondition = DataCompare.GetKeyColumnWhereCondition(this.sourceDbInterpreter, pagedKeyRows, detail.KeyColumns);

                                    DataTable sourceDataTable = await this.sourceDbInterpreter.GetPagedDataTableAsync(sourceConnection, detail.SourceTable, detail.SameTableColumns, null, pageSize, 1, whereCondition);

                                    count += sourceDataTable.Rows.Count;

                                    this.FeedbackInfo($@"Copying data to table ""{detail.TargetTable.Name}"" {count}/{total}...");

                                    await DbConverter.CopyData(this.sourceDbInterpreter, this.targetDbInterpreter, targetConnection, dbScriptGenerator, detail.SourceTable, detail.SourceTableColumns,
                                        detail.TargetTable, detail.SameTableColumns, sourceDataTable, true, cancellationToken, schemaMappings, null, transaction);

                                }

                                this.FeedbackInfo($@"End copy data to table ""{detail.TargetTable.Name}"".");
                            }
                        }

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            await transaction.CommitAsync();
                        }

                        result.IsOK = true;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Feedback(this, ex.Message, FeedbackInfoType.Error);

                result.IsOK = false;
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task<ExecuteResult> RunCommand(DbInterpreter dbInterpreter, DbConnection dbConnection, string command, CancellationToken cancellationToken, DbTransaction transaction)
        {
            CommandInfo commandInfo = new CommandInfo()
            {
                CommandType = CommandType.Text,
                CommandText = command,
                Transaction = transaction,
                CancellationToken = cancellationToken
            };

            ExecuteResult result = await dbInterpreter.ExecuteNonQueryAsync(dbConnection, commandInfo);

            return result;
        }

        public static async Task<DataExportResult> ExportDifferentData(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, DataCompareResultDetail detail, int pageSize, long pageNumber, string filePath)
        {
            DataExportResult exportResult = new DataExportResult();

            try
            {
                (DataTable Data, Dictionary<int, List<DataCompareValueInfo>> ValueInfos) result = await DataCompare.GetDifferentData(sourceDbInterpreter, targetDbInterpreter, detail, pageSize, pageNumber);

                Func<int, int, bool> isDifferentValue = (rowIndex, columnIndex) =>
                {
                    if (result.ValueInfos.ContainsKey(rowIndex))
                    {
                        var rowData = result.ValueInfos[rowIndex];

                        if (columnIndex >= 0 && columnIndex < rowData.Count)
                        {
                            var valueInfo = rowData[columnIndex];

                            return valueInfo.IsDifferent;
                        }
                    }

                    return false;
                };

                ExportDataOption option = new ExportDataOption() { FilePath = filePath, ShowColumnNames = true };

                ExcelWriter writer = new ExcelWriter(option);

                DataTable dataTable = result.Data;

                GridData gridData = new GridData() { Columns = new List<GridColumn>(), Rows = new List<GridRow>() };

                foreach (DataColumn column in dataTable.Columns)
                {
                    gridData.Columns.Add(new GridColumn() { Name = GetDifferentDataColumnDisplayText(column.ColumnName) });
                }

                int rowIndex = 0;

                foreach (DataRow row in dataTable.Rows)
                {
                    GridRow gridRow = new GridRow() { Cells = new List<GridCell>() };

                    int columnIndex = 0;

                    foreach (DataColumn column in dataTable.Columns)
                    {
                        object value = row[column.ColumnName];

                        GridCell gridCell = new GridCell();
                        gridCell.Content = ValueHelper.IsNullValue(value) ? "NULL" : value.ToString(); ;

                        if (isDifferentValue(rowIndex, columnIndex))
                        {
                            gridCell.HighlightMode = GridCellHighlightMode.Foreground;
                        }

                        gridRow.Cells.Add(gridCell);

                        columnIndex++;
                    }

                    rowIndex++;

                    gridData.Rows.Add(gridRow);
                }

                filePath = writer.Write(gridData, detail.SourceTable.Name);

                if (File.Exists(filePath))
                {
                    exportResult.IsOK = true;
                    exportResult.FilePath = filePath;
                }
            }
            catch (Exception ex)
            {
                exportResult.Message = ex.Message;
            }

            return exportResult;
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public void FeedbackInfo(string info)
        {
            this.Feedback(this, info);
        }

        public void Feedback(object owner, string content, FeedbackInfoType infoType = FeedbackInfoType.Info, bool enableLog = true, bool suppressError = false)
        {
            FeedbackInfo info = new FeedbackInfo() { InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(content), Owner = owner };

            FeedbackHelper.Feedback(suppressError ? null : this.observer, info, enableLog);
        }
    }
}
