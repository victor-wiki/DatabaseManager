using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.FileUtility;
using DatabaseManager.FileUtility.Model;
using DatabaseManager.Model;
using NPOI.SS.Formula.Functions;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
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

        public async Task<(bool Success, DataValidateResult ValidateResult)> Import(DbInterpreter dbInterpreter, Table table, ImportDataInfo info)
        {
            dbInterpreter.Option.ScriptOutputMode = GenerateScriptOutputMode.WriteToString;

            string tableName = table.Name;

            try
            {
                using (DbConnection connection = dbInterpreter.CreateConnection())
                {
                    string sql = $"select * from {dbInterpreter.GetQuotedString(tableName)}";

                    DataTable dataTable = await dbInterpreter.GetDataTableAsync(connection, sql, true);

                    string filePath = info.FilePath;

                    string fileExtension = Path.GetExtension(filePath).ToLower();

                    DataReadResult result = null;

                    if (fileExtension == ".csv")
                    {
                        result = this.ReadFromCsv(info, tableName);
                    }
                    else if (fileExtension == ".xlsx" || fileExtension == ".xls")
                    {
                        result = this.ReadFromExcel(info, tableName);
                    }

                    string[]? columnNames = result.HeaderColumns;

                    SchemaInfoFilter filter = new SchemaInfoFilter() { TableNames = [tableName] };

                    var columns = await dbInterpreter.GetTableColumnsAsync(connection, filter);

                    List<TableColumn> sortedColumns = new List<TableColumn>();
                    List<string> headerColumnNameList = result.HeaderColumns == null ? new List<string>() : result.HeaderColumns.Select(item => item.ToLower()).ToList();

                    if (columnNames != null && columnNames.Length > 0)
                    {
                        int order = 1;

                        foreach (var columnName in columnNames)
                        {
                            TableColumn column = columns.FirstOrDefault(item => item.Name.ToLower() == columnName.ToLower());

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

                    DataValidateResult validateResult = await this.ValidateData(dbInterpreter, connection, table, sortedColumns, filter, pagedData);

                    validateResult.Columns = sortedColumns;

                    if (validateResult.IsValid == false)
                    {
                        return (false, validateResult);
                    }

                    await connection.OpenAsync();

                    var trans = await connection.BeginTransactionAsync();

                    DbScriptGenerator targetDbScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(dbInterpreter);

                    foreach (var data in pagedData)
                    {
                        int pageNumber = data.Key;
                        Dictionary<int, Dictionary<string, object>> rows = data.Value;

                        StringBuilder sb = new StringBuilder();

                        Dictionary<string, object> parameters = targetDbScriptGenerator.AppendDataScripts(sb, table, sortedColumns, new Dictionary<long, List<Dictionary<string, object>>>() { { 1, rows.Values.ToList() } });

                        string script = sb.ToString().Trim().Trim(';');

                        string delimiter = ");" + Environment.NewLine;

                        if (!script.Contains(delimiter))
                        {
                            await dbInterpreter.ExecuteNonQueryAsync(connection, this.GetCommandInfo(script, parameters, trans));
                        }
                        else
                        {
                            var items = script.Split(delimiter);

                            List<string> insertItems = new List<string>();

                            foreach (var item in items)
                            {
                                if (item.Trim().ToUpper().StartsWith("INSERT INTO "))
                                {
                                    insertItems.Add(item);
                                }
                                else
                                {
                                    if (insertItems.Any())
                                    {
                                        insertItems[insertItems.Count - 1] += (delimiter + item);
                                    }
                                }
                            }

                            int count = 0;

                            foreach (var item in insertItems)
                            {
                                count++;

                                var cmd = count < insertItems.Count ? (item + delimiter).Trim().Trim(';') : item;

                                await dbInterpreter.ExecuteNonQueryAsync(connection, this.GetCommandInfo(cmd, parameters, trans));
                            }
                        }
                    }

                    await trans.CommitAsync();

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
                var pagedData = data.Skip(i - 1).Take(batchCount);

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

        private async Task<DataValidateResult> ValidateData(DbInterpreter dbInterpreter, DbConnection connection, Table table, List<TableColumn> columns, SchemaInfoFilter filter, Dictionary<int, Dictionary<int, Dictionary<string, object>>> pagedData)
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

                            if(resultRow.InvalidMessages == null)
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

        public string WriteValidateResultToExcel(DataValidateResult result, Table table, bool firstRowIsColumnName)
        {
            GridData gridData = new GridData();

            gridData.Columns = result.Columns;

            foreach (DataValidateResultRow resultRow in result.Rows)
            {
                GridRow row = new GridRow();
                row.RowIndex = resultRow.RowIndex;

                if(resultRow.InvalidMessages!=null)
                {
                    row.Comments = new List<GridRowComment>();

                    foreach(var invalidMessage in resultRow.InvalidMessages)
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
                            cell.NeedHighlight = true;
                        }
                        else if(row.Comments!=null && row.Comments.Any(item=>item.ColumnIndexes.Contains(cellIndex)))
                        {
                            cell.NeedHighlight = true;
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

        public DataReadResult ReadFromCsv(ImportDataInfo info, string tableName)
        {
            CsvReader reader = new CsvReader(info);

            return reader.Read();
        }

        public DataReadResult ReadFromExcel(ImportDataInfo info, string tableName)
        {
            ExcelReader reader = new ExcelReader(info);

            return reader.Read();
        }

        private void HandleError(Exception ex)
        {
            string errMsg = ExceptionHelper.GetExceptionDetails(ex);
            this.Feedback(this, errMsg, FeedbackInfoType.Error, true);
        }

        public void Feedback(object owner, string content, FeedbackInfoType infoType = FeedbackInfoType.Info, bool enableLog = true, bool suppressError = false)
        {
            FeedbackInfo info = new FeedbackInfo() { InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(content), Owner = owner };

            FeedbackHelper.Feedback(suppressError ? null : this.observer, info, enableLog);
        }
    }
}
