using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;

namespace DatabaseManager.FileUtility
{
    public class ExcelWriter:BaseWriter
    {
        private ExportDataOption option;

        public ExcelWriter(ExportDataOption option)
        {
            if (option == null)
            {
                option = new ExportDataOption();
            }

            this.option = option;
        }

        public void Write(DataTable dataTable, string tableName = null)
        {
            string filePath = option.FilePath;           

            if(string.IsNullOrEmpty(filePath))
            {
                base.CheckDefaultSaveFolder();

                filePath = $"{this.DefaultSaveFolder}/{(tableName == null ? "" : $"{tableName}_")}{DateTime.Now.ToString("yyyyMMdd")}..xlsx";
            }

            var columns = dataTable.Columns;
            long total = dataTable.Rows.Count;

            IWorkbook workbook = new XSSFWorkbook();
            long maxRows = 1048576;

            long sheetCount = total % maxRows == 0 ? total / maxRows : total / maxRows + 1;

            for (long i = 1; i <= sheetCount; i++)
            {
                var dataSheet = workbook.CreateSheet($"Sheet{i}");

                var columnIndex = 0;
                int rowIndex = 0;

                ICellStyle headerCellStyle = workbook.CreateCellStyle();
                IFont font = workbook.CreateFont();
                font.IsBold = true;
                headerCellStyle.SetFont(font);

                var headerRow = dataSheet.CreateRow(rowIndex);

                if (option.ShowColumnNames)
                {
                    foreach (DataColumn col in columns)
                    {
                        var headerCell = headerRow.CreateCell(columnIndex);

                        headerCell.CellStyle = headerCellStyle;

                        headerCell.SetCellValue(col.ColumnName);

                        columnIndex++;
                    }

                    rowIndex++;
                }

                ICellStyle dataCellStyle = workbook.CreateCellStyle();

                foreach (DataRow row in dataTable.Rows)
                {
                    columnIndex = 0;

                    var dataRow = dataSheet.CreateRow(rowIndex);

                    foreach (DataColumn col in columns)
                    {
                        var dataCell = dataRow.CreateCell(columnIndex);
                        object cellValue = row[col.ColumnName];

                        cellValue = cellValue == null ? "" : cellValue.ToString();

                        if (cellValue is int)
                        {
                            dataCell.SetCellValue(Convert.ToInt32(cellValue));
                        }
                        else if (cellValue is long)
                        {
                            dataCell.SetCellValue(Convert.ToInt64(cellValue));
                        }
                        else if (cellValue is float || cellValue is double || cellValue is decimal)
                        {
                            dataCell.SetCellValue(Convert.ToDouble(cellValue));
                        }
                        else if (cellValue is DateTime dt)
                        {
                            dataCell.SetCellValue(dt.ToUniversalTime().ToString());
                        }
                        else
                        {
                            dataCell.SetCellValue(cellValue?.ToString() ?? "");
                        }

                        columnIndex++;
                    }

                    rowIndex++;
                }
            }

            FileMode fileMode = File.Exists(filePath) ? FileMode.Truncate : FileMode.OpenOrCreate;

            using (FileStream stream = new FileStream(filePath, fileMode,  FileAccess.Write))
            {
                workbook.Write(stream);
            }
        }
    }
}
