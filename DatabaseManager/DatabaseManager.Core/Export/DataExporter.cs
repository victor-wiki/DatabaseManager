using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.FileUtility;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace DatabaseManager.Export
{
    public class DataExporter
    {
        private IObserver<FeedbackInfo> observer;

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public async Task<bool> Export(DbInterpreter dbInterpreter, string tableName, ExportDataOption option)
        {
            try
            {
                using (DbConnection connection = dbInterpreter.CreateConnection())
                {
                    string sql = $"select * from {dbInterpreter.GetQuotedString(tableName)}";

                    DataTable dataTable = await dbInterpreter.GetDataTableAsync(connection, sql, true);

                    if (option.FileType == ExportFileType.CSV)
                    {
                        this.WriteToCsv(dataTable, option, tableName);
                    }
                    else
                    {
                        this.WriteToExcel(dataTable, option, tableName);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                this.HandleError(ex);

                return false;
            }
        }

        public static void WriteToCsv(DataTable dataTable, string filePath)
        {
            CsvWriter writer = new CsvWriter(new ExportDataOption() { FilePath = filePath });

            writer.Write(dataTable);
        }

        public void WriteToCsv(DataTable dataTable, ExportDataOption option = null, string tableName = null)
        {
            CsvWriter writer = new CsvWriter(option);
            writer.Write(dataTable, tableName);
        }

        public void WriteToExcel(DataTable dataTable, ExportDataOption option = null, string tableName = null)
        {
            ExcelWriter writer = new ExcelWriter(option);

            writer.Write(dataTable, tableName);
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
