using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core.Model;
using DatabaseManager.FileUtility;
using DatabaseManager.FileUtility.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Table = DatabaseInterpreter.Model.Table;

namespace DatabaseManager.Core
{
    public class DataExporter
    {
        private IObserver<FeedbackInfo> observer;

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public async Task<DataExportResult> Export(DbInterpreter dbInterpreter, DatabaseObject dbObject, List<DataExportColumn> columns, ExportSpecificDataOption option, CancellationToken cancellationToken)
        {
            DataExportResult exportResult = new DataExportResult();

            List<TableColumn> tableColumns = new List<TableColumn>();

            if (columns != null)
            {
                tableColumns.AddRange(columns.Select(item => new TableColumn() { Name = item.ColumnName, DataType = item.DataType }));
            }

            bool isForView = false;

            if (dbObject is View)
            {
                dbObject = ObjectHelper.CloneObject<Table>(dbObject);

                isForView = true;
            }

            try
            {
                DataTable mergedDataTable = new DataTable();

                using (var connection = dbInterpreter.CreateConnection())
                {
                    if (!option.ExportAllThatMeetCondition)
                    {
                        List<long> pageNumbers = option.PageNumbers;

                        foreach (long pageNumber in pageNumbers)
                        {
                            (long Total, DataTable Data) result = await dbInterpreter.GetPagedDataTableAsync(connection, dbObject as Table, option.OrderColumns, option.PageSize, pageNumber, option.ConditionClause, isForView, tableColumns);

                            mergedDataTable.Merge(result.Data);
                        }
                    }
                    else
                    {
                        int batchCount = 500;
                        long count = 0;

                        (long Total, DataTable Data) result = await dbInterpreter.GetPagedDataTableAsync(connection, dbObject as Table, option.OrderColumns, batchCount, 1, option.ConditionClause, isForView, tableColumns);

                        count += result.Data.Rows.Count;

                        mergedDataTable.Merge(result.Data);

                        long total = result.Total;

                        long pageNumber = total % batchCount == 0 ? total / batchCount : total / batchCount + 1;

                        if (pageNumber > 1)
                        {
                            for (int i = 2; i <= pageNumber; i++)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    exportResult.Message = "Task has been canceled.";
                                    break;
                                }

                                count += (i < pageNumber ? batchCount : total - (pageNumber - 1) * batchCount);

                                this.Feedback($"Reading data {count}/{result.Total}...");

                                result = await dbInterpreter.GetPagedDataTableAsync(connection, dbObject as Table, option.OrderColumns, batchCount, i, option.ConditionClause, isForView, tableColumns);

                                mergedDataTable.Merge(result.Data);
                            }
                        }
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        this.Feedback("Writing to file...");

                        if (columns != null)
                        {
                            foreach (DataColumn column in mergedDataTable.Columns)
                            {
                                var col = columns.FirstOrDefault(item => item.ColumnName == column.ColumnName);

                                if (col != null && col.ColumnName != col.DisplayName && !string.IsNullOrEmpty(col.DisplayName))
                                {
                                    column.ColumnName = col.DisplayName;
                                }
                            }
                        }

                        string filePath = this.ExportDataTable(mergedDataTable, dbObject.Name, option);

                        this.Feedback("End write to file.");

                        exportResult.IsOK = true;
                        exportResult.FilePath = filePath;

                        return exportResult;
                    }
                    else
                    {
                        return exportResult;
                    }
                }
            }
            catch (Exception ex)
            {
                this.HandleError(ex);

                exportResult.Message = ex.Message;

                return exportResult;
            }
        }

        public DataExportResult Export(DataTable dataTable, List<DataExportColumn> columns, ExportSpecificDataOption option)
        {
            DataExportResult exportResult = new DataExportResult();

            List<DataColumn> excludeColumns = new List<DataColumn>();

            foreach (DataColumn column in dataTable.Columns)
            {
                string columnName = column.ColumnName;

                var col = columns.FirstOrDefault(item => item.ColumnName == columnName);

                if (col != null)
                {
                    if(!string.IsNullOrEmpty(col.DisplayName))
                    {
                        column.ColumnName = col.DisplayName;
                    }                   
                }
                else
                {
                    excludeColumns.Add(column);
                }
            }

            excludeColumns.ForEach(item => { dataTable.Columns.Remove(item); });

            exportResult.FilePath = this.ExportDataTable(dataTable, dataTable.TableName, option);
            exportResult.IsOK = true;

            return exportResult;
        }

        private string ExportDataTable(DataTable dataTable, string tableName, ExportDataOption option)
        {
            string filePath = null;

            if (option.FileType == ExportFileType.CSV)
            {
                filePath = this.WriteToCsv(dataTable, option, tableName);
            }
            else
            {
                filePath = this.WriteToExcel(dataTable, option, tableName);
            }

            return filePath;
        }

        public static void WriteToCsv(DataTable dataTable, string filePath)
        {
            CsvWriter writer = new CsvWriter(new ExportDataOption() { FilePath = filePath });

            writer.Write(dataTable);
        }

        public string WriteToCsv(DataTable dataTable, ExportDataOption option = null, string tableName = null)
        {
            CsvWriter writer = new CsvWriter(option);

            return writer.Write(dataTable, tableName);
        }

        public string WriteToExcel(DataTable dataTable, ExportDataOption option = null, string tableName = null)
        {
            ExcelWriter writer = new ExcelWriter(option);

            return writer.Write(dataTable, tableName);
        }

        private void HandleError(Exception ex)
        {
            string errMsg = ExceptionHelper.GetExceptionDetails(ex);
            this.Feedback(this, errMsg, FeedbackInfoType.Error, true);
        }

        private void Feedback(string message)
        {
            this.Feedback(this, message);
        }

        private void Feedback(object owner, string content, FeedbackInfoType infoType = FeedbackInfoType.Info, bool enableLog = true, bool suppressError = false)
        {
            FeedbackInfo info = new FeedbackInfo() { InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(content), Owner = owner };

            FeedbackHelper.Feedback(suppressError ? null : this.observer, info, enableLog);
        }
    }
}
