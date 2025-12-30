using DatabaseManager.FileUtility.Model;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace DatabaseManager.FileUtility
{
    public class ExcelWriter : BaseWriter
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

        public string Write(DataTable dataTable, string tableName = null)
        {
            string filePath = option.FilePath;

            if (string.IsNullOrEmpty(filePath))
            {
                string folder = this.option.IsTemporary ? base.TemporaryFolder : base.DefaultSaveFolder;

                base.CheckFolder(folder);

                filePath = Path.Combine(base.AssemblyFolder, folder, $"{(tableName == null ? "" : $"{tableName}_")}{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
            }

            var columns = dataTable.Columns;
            long total = dataTable.Rows.Count;

            IWorkbook workbook = new XSSFWorkbook();
            long maxRows = 1048576;

            long sheetCount = total % maxRows == 0 ? total / maxRows : total / maxRows + 1;

            for (long i = 1; i <= sheetCount; i++)
            {
                var dataSheet = workbook.CreateSheet($"Sheet{i}");

                int rowIndex = 0;

                if (option.ShowColumnNames)
                {
                    var columnIndex = 0;

                    ICellStyle headerCellStyle = workbook.CreateCellStyle();
                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    headerCellStyle.SetFont(font);

                    var headerRow = dataSheet.CreateRow(rowIndex);

                    foreach (DataColumn col in columns)
                    {
                        var headerCell = headerRow.CreateCell(columnIndex);

                        headerCell.CellStyle = headerCellStyle;

                        headerCell.SetCellValue(col.ColumnName);

                        columnIndex++;
                    }

                    rowIndex++;
                }

                foreach (DataRow row in dataTable.Rows)
                {
                    int columnIndex = 0;

                    var dataRow = dataSheet.CreateRow(rowIndex);

                    foreach (DataColumn col in columns)
                    {
                        var dataCell = dataRow.CreateCell(columnIndex);
                        object cellValue = row[col.ColumnName];

                        this.SetCellValue(dataCell, cellValue);

                        columnIndex++;
                    }

                    rowIndex++;
                }
            }

            FileMode fileMode = File.Exists(filePath) ? FileMode.Truncate : FileMode.OpenOrCreate;

            using (FileStream stream = new FileStream(filePath, fileMode, FileAccess.Write))
            {
                workbook.Write(stream);
            }

            return filePath;
        }

        public string Write(GridData gridData, string tableName = null)
        {
            if (gridData.Rows == null)
            {
                return null;
            }

            string filePath = option.FilePath;

            if (string.IsNullOrEmpty(filePath))
            {
                string folder = this.option.IsTemporary ? base.TemporaryFolder : base.DefaultSaveFolder;

                base.CheckFolder(folder);

                filePath = Path.Combine(base.AssemblyFolder, folder, $"{(tableName == null ? "" : $"{tableName}_")}{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
            }

            var columns = gridData.Columns;
            long total = gridData.Rows.Count;

            IWorkbook workbook = new XSSFWorkbook();
            long maxRows = 1048576;

            long sheetCount = total % maxRows == 0 ? total / maxRows : total / maxRows + 1;

            for (long i = 1; i <= sheetCount; i++)
            {
                var dataSheet = workbook.CreateSheet($"Sheet{i}");

                if (option.ShowColumnNames)
                {
                    ICellStyle headerCellStyle = workbook.CreateCellStyle();
                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    headerCellStyle.SetFont(font);

                    var headerRow = dataSheet.CreateRow(0);

                    var columnIndex = 0;

                    foreach (GridColumn col in columns)
                    {
                        var headerCell = headerRow.CreateCell(columnIndex);

                        headerCell.CellStyle = headerCellStyle;

                        headerCell.SetCellValue(col.Name);

                        columnIndex++;
                    }
                }

                ICellStyle highlightCellBackgroundStyle = workbook.CreateCellStyle();

                highlightCellBackgroundStyle.FillForegroundColor = IndexedColors.Red.Index;
                highlightCellBackgroundStyle.FillPattern = FillPattern.SolidForeground;

                ICellStyle highlightCellForegroundStyle = workbook.CreateCellStyle();

                IFont highlightCellForegroundFont = workbook.CreateFont();
                highlightCellForegroundFont.IsBold = true;
                highlightCellForegroundStyle.SetFont(highlightCellForegroundFont);

                int rowIndex = option.ShowColumnNames ? 1 : 0;

                foreach (GridRow row in gridData.Rows)
                {
                    var dataRow = dataSheet.CreateRow(rowIndex);

                    if (row.Cells != null)
                    {
                        int columnIndex = 0;

                        List<int> commentCellIndexes = new List<int>();

                        foreach (var cell in row.Cells)
                        {
                            var dataCell = dataRow.CreateCell(columnIndex);
                            object cellValue = cell.Content;

                            this.SetCellValue(dataCell, cellValue);

                            if (cell.Comment != null)
                            {
                                this.AddComment(workbook, dataSheet, dataCell, cell.Comment);

                                commentCellIndexes.Add(columnIndex);
                            }

                            if (cell.HighlightMode == GridCellHighlightMode.Background)
                            {
                                dataCell.CellStyle = highlightCellBackgroundStyle;
                            }
                            else if (cell.HighlightMode == GridCellHighlightMode.Foreground)
                            {
                                dataCell.CellStyle = highlightCellForegroundStyle;
                            }

                            columnIndex++;
                        }

                        if (row.Comments != null)
                        {
                            foreach (var comment in row.Comments)
                            {
                                foreach (var colIndex in comment.ColumnIndexes)
                                {
                                    if (!commentCellIndexes.Contains(colIndex))
                                    {
                                        var dataCell = dataRow.GetCell(colIndex);

                                        if (dataCell != null)
                                        {
                                            this.AddComment(workbook, dataSheet, dataCell, comment.Comment);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    rowIndex++;
                }
            }

            FileMode fileMode = File.Exists(filePath) ? FileMode.Truncate : FileMode.OpenOrCreate;

            using (FileStream stream = new FileStream(filePath, fileMode, FileAccess.Write))
            {
                workbook.Write(stream);
            }

            return filePath;
        }

        private void SetCellValue(ICell cell, object value)
        {
            if (value is int)
            {
                cell.SetCellValue(Convert.ToInt32(value));
            }
            else if (value is long)
            {
                cell.SetCellValue(Convert.ToInt64(value));
            }
            else if (value is float || value is double || value is decimal)
            {
                cell.SetCellValue(Convert.ToDouble(value));
            }
            else if (value is DateTime dt)
            {
                cell.SetCellValue(dt.ToUniversalTime().ToString());
            }
            else
            {
                cell.SetCellValue(value?.ToString() ?? "");
            }
        }

        private void AddComment(IWorkbook workbook, ISheet sheet, ICell cell, string text)
        {
            IDrawing drawing = sheet.CreateDrawingPatriarch();
            IClientAnchor anchor = workbook.GetCreationHelper().CreateClientAnchor();

            anchor.Row1 = cell.RowIndex;
            anchor.Col1 = cell.ColumnIndex;
            anchor.Row2 = cell.RowIndex + 3;
            anchor.Col2 = cell.ColumnIndex + 3;

            var comment = drawing.CreateCellComment(anchor);
            comment.String = workbook.GetCreationHelper().CreateRichTextString(text);
            cell.CellComment = comment;
        }
    }
}
