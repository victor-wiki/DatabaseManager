using NPOI.SS.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseManager.FileUtility
{
    public class ExcelReader : BaseReader
    {
        private readonly Regex numericReg = new Regex("^[0]+[.][0]+[_]*");
        public ExcelReader(SourceFileInfo info) : base(info) { }

        public override DataReadResult Read(bool onlyReadHeader = false)
        {
            DataReadResult result = new DataReadResult();           

            string filePath = this.info.FilePath;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = WorkbookFactory.Create(fs);

                int sheetCount = workbook.NumberOfSheets;

                Dictionary<int, Dictionary<int, object>> dict = new Dictionary<int, Dictionary<int, object>>();

                for (int i = 0; i < sheetCount; i++)
                {
                    ISheet sheet = workbook.GetSheetAt(i);

                    int startRowIndex = sheet.FirstRowNum;
                    int endRowIndex = sheet.LastRowNum;

                    for (int j = startRowIndex; j <= endRowIndex; j++)
                    {
                        var row = sheet.GetRow(j);

                        int k = 0;
                        int cellCount = row.LastCellNum;

                        Dictionary<int, object> dictRow = new Dictionary<int, object>();

                        for (int m = 0; m < cellCount; m++)
                        {
                            var cell = row.GetCell(m);

                            if (j == startRowIndex && info.FirstRowIsColumnName)
                            {
                                if (result.HeaderColumns == null)
                                {
                                    result.HeaderColumns = new string[cellCount];
                                }

                                result.HeaderColumns[k] = cell.StringCellValue;                                
                            }
                            else if(!onlyReadHeader)
                            {
                                dictRow.Add(k, this.GetCellValue(cell));
                            }

                            k++;
                        }

                        if (onlyReadHeader && j == startRowIndex)
                        {
                            break;
                        }

                        if (dictRow.Any())
                        {
                            dict.Add(j, dictRow);
                        }
                    }

                    if(onlyReadHeader)
                    {
                        break;
                    }
                }

                result.Data = dict;
            }          

            return result;
        }

        public object GetCellValue(ICell cell)
        {
            if (cell == null)
            {
                return null;
            }

            switch (cell.CellType)
            {
                case CellType.Boolean:
                    return cell.BooleanCellValue;
                case CellType.Numeric:
                    return cell.NumericCellValue;
                case CellType.String:
                    return cell.StringCellValue;
                case CellType.Error:
                case CellType.Blank:
                case CellType.Formula:
                    return null;
                default:
                    return cell.StringCellValue;
            }           
        }
    }
}
