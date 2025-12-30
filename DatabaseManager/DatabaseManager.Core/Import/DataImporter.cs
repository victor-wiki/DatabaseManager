using DatabaseConverter.Core;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core.Model;
using DatabaseManager.FileUtility;
using DatabaseManager.FileUtility.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Table = DatabaseInterpreter.Model.Table;

namespace DatabaseManager.Core
{
    public class DataImporter
    {
        private IObserver<FeedbackInfo> observer;

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public async Task<(bool Success, DataValidateResult ValidateResult)> Import(DbInterpreter dbInterpreter, Table table, SourceFileInfo info, List<DataImportColumnMapping> columnMappings, CancellationToken cancellationToken)
        {
            dbInterpreter.Option.ScriptOutputMode = GenerateScriptOutputMode.WriteToString;
            dbInterpreter.Option.ThrowExceptionWhenErrorOccurs = true;

            string tableName = table.Name;

            try
            {
                using (DbConnection connection = dbInterpreter.CreateConnection())
                {
                    string filePath = info.FilePath;

                    string fileExtension = Path.GetExtension(filePath).ToLower();

                    DataReadResult result = null;

                    this.Feedback("Begin to read file data...");

                    if (fileExtension == ".csv")
                    {
                        result = this.ReadFromCsv(info, tableName);
                    }
                    else if (fileExtension == ".xlsx" || fileExtension == ".xls")
                    {
                        result = this.ReadFromExcel(info, tableName);
                    }

                    this.Feedback("End read file data.");

                    string[] headerNames = result.HeaderColumns;

                    SchemaInfoFilter filter = new SchemaInfoFilter() { TableNames = [tableName] };

                    var columns = await dbInterpreter.GetTableColumnsAsync(connection, filter);

                    List<TableColumn> sortedColumns = new List<TableColumn>();

                    if (headerNames != null && headerNames.Length > 0)
                    {
                        int order = 1;

                        foreach (var headerName in headerNames)
                        {
                            TableColumn column = null;

                            if (!info.FirstRowIsColumnName)
                            {
                                column = columns.FirstOrDefault(item => item.Name.ToLower() == headerName.ToLower());
                            }
                            else
                            {
                                var mapping = columnMappings == null ? null : columnMappings.FirstOrDefault(item => item.FileColumnName == headerName);

                                if (mapping != null)
                                {
                                    column = columns.FirstOrDefault(item => item.Name.ToLower() == mapping.TableColumName.ToLower());
                                }
                                else
                                {
                                    column = columns.FirstOrDefault(item => item.Name.ToLower() == headerName.ToLower());
                                }
                            }

                            if (column != null)
                            {
                                column.Order = order;

                                sortedColumns.Add(column);

                                order++;
                            }
                        }
                    }
                    else
                    {
                        sortedColumns.AddRange(columns);
                    }

                    Dictionary<int, Dictionary<int, Dictionary<string, object>>> pagedData = this.GetPagedData(result, sortedColumns);

                    this.Feedback("Begin to validate data...");

                    DataValidateResult validateResult = await this.ValidateData(dbInterpreter, connection, table, sortedColumns, filter, pagedData, columnMappings);

                    this.Feedback("End validate data.");

                    validateResult.Columns = sortedColumns;

                    if (validateResult.IsValid == false)
                    {
                        return (false, validateResult);
                    }

                    await connection.OpenAsync();

                    var trans = await connection.BeginTransactionAsync();

                    DbScriptGenerator targetDbScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(dbInterpreter);

                    this.Feedback("Begin to insert data...");

                    int total = result.Data.Count;
                    int count = 0;

                    foreach (var data in pagedData)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            trans.Rollback();
                            break;
                        }

                        count += data.Value.Count;

                        this.Feedback($"Inserted data: {count}/{total}.");

                        Dictionary<int, Dictionary<string, object>> rows = data.Value;

                        await DbConverter.InsertData(dbInterpreter, connection, targetDbScriptGenerator, table, sortedColumns, rows.Values.ToList(), cancellationToken, trans);
                    }

                    this.Feedback("End insert data.");

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await trans.CommitAsync();
                    }

                    return (true, null);
                }
            }
            catch (Exception ex)
            {
                this.HandleError(ex);

                return (false, null);
            }
        }

        private Dictionary<int, Dictionary<int, Dictionary<string, object>>> GetPagedData(DataReadResult result, List<TableColumn> columns)
        {
            Dictionary<int, Dictionary<int, Dictionary<string, object>>> dict = new Dictionary<int, Dictionary<int, Dictionary<string, object>>>();
            Dictionary<int, Dictionary<int, object>> data = result.Data;

            int batchCount = 500;
            int pageNumber = data.Count % batchCount == 0 ? data.Count / batchCount : data.Count / batchCount + 1;

            for (int i = 1; i <= pageNumber; i++)
            {
                var pagedData = data.Skip((i - 1) * batchCount).Take(batchCount);

                Dictionary<int, Dictionary<string, object>> dataList = new Dictionary<int, Dictionary<string, object>>();

                foreach (var row in pagedData)
                {
                    int rowIndex = row.Key;

                    Dictionary<int, object> rowData = row.Value;

                    Dictionary<string, object> dictRow = new Dictionary<string, object>();

                    foreach (var cell in rowData)
                    {
                        int columnIndex = cell.Key;
                        object cellValue = cell.Value;

                        string columnName = columns[columnIndex].Name;

                        dictRow.Add(columnName, cellValue);
                    }

                    dataList.Add(rowIndex, dictRow);
                }

                dict.Add(i, dataList);
            }

            return dict;
        }

        private CommandInfo GetCommandInfo(string commandText, Dictionary<string, object> parameters = null, DbTransaction transaction = null)
        {
            CommandInfo commandInfo = new CommandInfo()
            {
                CommandType = CommandType.Text,
                CommandText = commandText,
                Parameters = parameters,
                Transaction = transaction
            };

            return commandInfo;
        }

        private async Task<DataValidateResult> ValidateData(DbInterpreter dbInterpreter, DbConnection connection, Table table, List<TableColumn> columns, SchemaInfoFilter filter, Dictionary<int, Dictionary<int, Dictionary<string, object>>> pagedData, List<DataImportColumnMapping> columnMappings = null)
        {
            DataValidateResult validateResult = new DataValidateResult();

            var primaryKey = (await dbInterpreter.GetTablePrimaryKeysAsync(connection, filter)).FirstOrDefault();

            var uniqueIndexes = (await dbInterpreter.GetTableIndexesAsync(connection, filter)).Where(item => item.IsPrimary || item.IsUnique);

            List<TableChild> tableChildren = new List<TableChild>();

            if (primaryKey != null)
            {
                tableChildren.Add(primaryKey);
            }

            tableChildren.AddRange(uniqueIndexes);

            Dictionary<int, Dictionary<string, object>> rows = pagedData.SelectMany(item => item.Value).ToDictionary();

            Dictionary<string, List<string>> dictUnique = new Dictionary<string, List<string>>();
            Dictionary<string, TablePrimaryKey> dictReferenceTablePrimaryKey = new Dictionary<string, TablePrimaryKey>();
            Dictionary<string, IEnumerable<TableIndex>> dictReferenceTableUniqueIndexes = new Dictionary<string, IEnumerable<TableIndex>>();

            foreach (var row in rows)
            {
                int rowIndex = row.Key;
                Dictionary<string, object> rowData = row.Value;

                DataValidateResultRow resultRow = new DataValidateResultRow() { RowIndex = rowIndex };

                List<DataValidateResultCell> cells = new List<DataValidateResultCell>();

                int i = 0;

                foreach (TableColumn column in columns)
                {
                    string columnName = column.Name;

                    if (rowData.ContainsKey(columnName))
                    {
                        object value = rowData[columnName];

                        bool isValid = true;
                        string invalidMessage = null;

                        if (value == null || value?.ToString() == string.Empty)
                        {
                            if (column.IsRequired)
                            {
                                isValid = false;
                                invalidMessage = $"{columnName} is required.";
                            }
                        }
                        else
                        {
                            var mapping = columnMappings == null ? null : columnMappings.FirstOrDefault(item => item.TableColumName == columnName);

                            if (mapping != null)
                            {
                                string referencedTableName = mapping.ReferencedTableName;
                                string referencedTableColumnName = mapping.ReferencedTableColumnName;

                                if(!string.IsNullOrEmpty(referencedTableName) && !string.IsNullOrEmpty(referencedTableColumnName))
                                {
                                    var referencedTableFilter = new SchemaInfoFilter() { TableNames = [referencedTableName] };

                                    TablePrimaryKey referencedTablePrimaryKey = null;

                                    if (dictReferenceTablePrimaryKey.ContainsKey(referencedTableName))
                                    {
                                        referencedTablePrimaryKey = dictReferenceTablePrimaryKey[referencedTableName];
                                    }
                                    else
                                    {
                                        referencedTablePrimaryKey = (await dbInterpreter.GetTablePrimaryKeysAsync(connection, referencedTableFilter)).FirstOrDefault();

                                        dictReferenceTablePrimaryKey.Add(referencedTableName, referencedTablePrimaryKey);
                                    }

                                    if (referencedTablePrimaryKey == null)
                                    {
                                        isValid = false;
                                        invalidMessage = $@"Referenced table ""{referencedTableName}"" has no primary key.";
                                    }
                                    else
                                    {
                                        IEnumerable<TableIndex> referencedTableUniqueIndexes = null;

                                        if (dictReferenceTableUniqueIndexes.ContainsKey(referencedTableName))
                                        {
                                            referencedTableUniqueIndexes = dictReferenceTableUniqueIndexes[referencedTableName];
                                        }
                                        else
                                        {
                                            referencedTableUniqueIndexes = (await dbInterpreter.GetTableIndexesAsync(connection, referencedTableFilter)).Where(item => item.IsUnique);

                                            dictReferenceTableUniqueIndexes.Add(referencedTableName, referencedTableUniqueIndexes);
                                        }

                                        if (!referencedTableUniqueIndexes.Any(item => item.Columns.Any(t => t.ColumnName == referencedTableColumnName)))
                                        {
                                            isValid = false;
                                            invalidMessage = $@"Referenced table ""{referencedTableName}""'s unique columns doesn't contains column ""{referencedTableColumnName}"".";
                                        }
                                        else
                                        {
                                            string dataType = mapping.ReferencedTableColumnDataType;

                                            bool isCharType = DataTypeHelper.IsCharType(dataType, dbInterpreter.DatabaseType == DatabaseType.Sqlite);

                                            string strValue = $"{(isCharType ? "'" : "")}{value.ToString()?.Replace("'", "''")}{(isCharType ? "'" : "")}";

                                            string sql = $"SELECT {dbInterpreter.GetQuotedString(referencedTablePrimaryKey.Columns.First().ColumnName)} FROM {dbInterpreter.GetQuotedString(referencedTableName)} where {dbInterpreter.GetQuotedString(referencedTableColumnName)}={strValue}";

                                            object referencedKeyValue = await dbInterpreter.GetScalarAsync(connection, sql);

                                            if (referencedKeyValue == null || referencedKeyValue?.ToString() == string.Empty)
                                            {
                                                isValid = false;
                                                invalidMessage = $@"{value} doesn't exists in table ""{referencedTableName}"".";
                                            }
                                            else
                                            {
                                                rowData[columnName] = referencedKeyValue;
                                            }
                                        }
                                    }
                                }                           
                            }
                        }

                        cells.Add(new DataValidateResultCell() { ColumnIndex = i, IsValid = isValid, Content = value?.ToString(), InvalidMessage = invalidMessage });
                    }

                    i++;
                }

                foreach (var child in tableChildren)
                {
                    IEnumerable<IndexColumn> indexColumns = null;

                    if (child is TablePrimaryKey pk)
                    {
                        indexColumns = pk.Columns;
                    }
                    else if (child is TableIndex index)
                    {
                        indexColumns = index.Columns;
                    }

                    if (indexColumns == null)
                    {
                        continue;
                    }

                    string indexColumnNames = string.Join("_", indexColumns.Select(item => item.ColumnName));

                    List<string> values = new List<string>();

                    foreach (var column in indexColumns)
                    {
                        string columnName = column.ColumnName;

                        if (rowData.ContainsKey(columnName))
                        {
                            object value = rowData[columnName];

                            values.Add(value?.ToString());
                        }
                    }

                    string indexColumnValues = string.Join("_", values);

                    if (!dictUnique.ContainsKey(indexColumnNames))
                    {
                        dictUnique.Add(indexColumnNames, new List<string>() { indexColumnValues });
                    }
                    else
                    {
                        var existingValues = dictUnique[indexColumnNames];

                        if (existingValues.Contains(indexColumnValues))
                        {
                            resultRow.IsValid = false;

                            if (resultRow.InvalidMessages == null)
                            {
                                resultRow.InvalidMessages = new List<DataValidateResultRowInvalidMessage>();
                            }

                            DataValidateResultRowInvalidMessage invalidMessage = new DataValidateResultRowInvalidMessage();
                            invalidMessage.Message = $"{indexColumnNames} is duplicate.";
                            invalidMessage.ColumnIndexes = this.FindColumnIndexes(columns, indexColumns);

                            resultRow.InvalidMessages.Add(invalidMessage);
                        }
                        else
                        {
                            dictUnique[indexColumnNames].Add(indexColumnValues);
                        }
                    }
                }

                resultRow.Cells = cells;

                if (validateResult.Rows == null)
                {
                    validateResult.Rows = new List<DataValidateResultRow>();
                }

                validateResult.Rows.Add(resultRow);
            }

            return validateResult;
        }

        private List<int> FindColumnIndexes(List<TableColumn> columns, IEnumerable<IndexColumn> indexColumns)
        {
            List<int> columnIndexes = new List<int>();

            foreach (var indexColumn in indexColumns)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    if (columns[i].Name == indexColumn.ColumnName)
                    {
                        columnIndexes.Add(i);
                        break;
                    }
                }
            }

            return columnIndexes;
        }

        public string WriteValidateResultToExcel(DataValidateResult result, Table table, bool firstRowIsColumnName, List<DataImportColumnMapping> columnMappings = null)
        {
            GridData gridData = new GridData();

            gridData.Columns = new List<GridColumn>();

            foreach (var column in result.Columns)
            {
                string columnName = null;

                if (columnMappings != null && columnMappings.Count > 0)
                {
                    var mapping = columnMappings.FirstOrDefault(item => item.TableColumName == column.Name);

                    if (mapping != null)
                    {
                        columnName = mapping.FileColumnName;
                    }
                }

                if (string.IsNullOrEmpty(columnName))
                {
                    columnName = column.Name;
                }

                gridData.Columns.Add(new GridColumn() { Name = columnName });
            }          

            foreach (DataValidateResultRow resultRow in result.Rows)
            {
                GridRow row = new GridRow();              

                if (resultRow.InvalidMessages != null)
                {
                    row.Comments = new List<GridRowComment>();

                    foreach (var invalidMessage in resultRow.InvalidMessages)
                    {
                        GridRowComment comment = new GridRowComment();
                        comment.Comment = invalidMessage.Message;
                        comment.ColumnIndexes = invalidMessage.ColumnIndexes;

                        row.Comments.Add(comment);
                    }
                }

                if (gridData.Rows == null)
                {
                    gridData.Rows = new List<GridRow>();
                }

                gridData.Rows.Add(row);

                if (resultRow.Cells != null)
                {
                    int cellIndex = 0;

                    foreach (var resultCell in resultRow.Cells)
                    {
                        GridCell cell = new GridCell();

                        cell.Content = resultCell.Content;
                        cell.Comment = resultCell.InvalidMessage;

                        if (resultCell.IsValid == false)
                        {
                            cell.HighlightMode =  GridCellHighlightMode.Background;
                        }
                        else if (row.Comments != null && row.Comments.Any(item => item.ColumnIndexes.Contains(cellIndex)))
                        {
                            cell.HighlightMode = GridCellHighlightMode.Background;
                        }

                        if (row.Cells == null)
                        {
                            row.Cells = new List<GridCell>();
                        }

                        row.Cells.Add(cell);

                        cellIndex++;
                    }
                }
            }

            try
            {
                ExcelWriter writer = new ExcelWriter(new ExportDataOption() { ShowColumnNames = firstRowIsColumnName, IsTemporary = true });

                string filePath = writer.Write(gridData, table.Name);

                return filePath;
            }
            catch (Exception ex)
            {
                this.HandleError(ex);

                return null;
            }
        }

        public DataReadResult ReadFromCsv(SourceFileInfo info, string tableName)
        {
            CsvReader reader = new CsvReader(info);

            return reader.Read();
        }

        public DataReadResult ReadFromExcel(SourceFileInfo info, string tableName)
        {
            ExcelReader reader = new ExcelReader(info);

            return reader.Read();
        }

        private void HandleError(Exception ex)
        {
            string errMsg = ExceptionHelper.GetExceptionDetails(ex);

            this.Feedback(this, errMsg, FeedbackInfoType.Error, true);
        }

        private void Feedback(string content)
        {
            this.Feedback(this, content);
        }

        private void Feedback(object owner, string content, FeedbackInfoType infoType = FeedbackInfoType.Info, bool enableLog = true, bool suppressError = false)
        {
            FeedbackInfo info = new FeedbackInfo() { InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(content), Owner = owner };

            FeedbackHelper.Feedback(suppressError ? null : this.observer, info, enableLog);
        }
    }
}
