using DatabaseManager.FileUtility.Model;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.XWPF.UserModel;
using System;
using System.IO;
using System.Linq;

namespace DatabaseManager.FileUtility
{
    public class WordWriter
    {
        private WordGenerationOption option;

        public WordWriter(WordGenerationOption option)
        {
            this.option = option;
        }

        public string Write(DocumentBody body)
        {
            XWPFDocument doc = new XWPFDocument();

            var parts = body.Parts;

            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];

                XWPFParagraph paragraph = doc.CreateParagraph();

                paragraph.SpacingAfter = 0;
                paragraph.SpacingBefore = 0;

                var title = paragraph.CreateRun();
                title.IsBold = true;
                title.SetText(part.Title);

                if (!string.IsNullOrEmpty(part.Comment))
                {
                    var comment = paragraph.CreateRun();

                    comment.SetText($"({part.Comment})");
                }

                var gridData = part.Data;

                int rowCount = gridData.Rows.Count + 1;
                int columnCount = gridData.Columns.Count;

                var table = doc.CreateTable(rowCount, columnCount);               

                for (var j = 0; j < rowCount; j++)
                {
                    if (j == 0) //column header
                    {
                        for (int k = 0; k < columnCount; k++)
                        {
                            var cell = table.GetRow(j).GetCell(k);

                            var cellParagraph = cell.Paragraphs.FirstOrDefault();

                            if (cellParagraph == null)
                            {
                                cellParagraph = cell.AddParagraph();
                            }

                            cellParagraph.Alignment = ParagraphAlignment.CENTER;
                            cellParagraph.VerticalAlignment = TextAlignment.CENTER;
                            cellParagraph.SpacingAfter = 0;

                            var cellRun = cellParagraph.CreateRun();
                            cellRun.SetText(gridData.Columns[k].Name);

                            if (option.GridColumnHeaderFontIsBold)
                            {
                                cellRun.IsBold = true;
                            }

                            if (!string.IsNullOrEmpty(option.GridColumnHeaderForegroundColor))
                            {
                                cellRun.SetColor(option.GridColumnHeaderForegroundColor);
                            }

                            if (!string.IsNullOrEmpty(option.GridColumnHeaderBackgroundColor))
                            {
                                var tcPr = cell.GetCTTc().AddNewTcPr();

                                var valign = tcPr.AddNewVAlign();
                                var shd = tcPr.AddNewShd();

                                valign.val = ST_VerticalJc.center;                             
                                shd.val = NPOI.OpenXmlFormats.Wordprocessing.ST_Shd.solid;
                                shd.color = option.GridColumnHeaderBackgroundColor;
                            }
                        }
                    }
                    else
                    {
                        for (int k = 0; k < columnCount; k++)
                        {
                            var cell = table.GetRow(j).GetCell(k);
                            cell.SetText(gridData.Rows[j - 1].Cells[k].Content?.ToString());

                            var tcPr = cell.GetCTTc().AddNewTcPr();
                            var valign = tcPr.AddNewVAlign();

                            valign.val = ST_VerticalJc.center;

                            var cellParagraph = cell.Paragraphs.FirstOrDefault();

                            if (cellParagraph != null)
                            {
                                cellParagraph.SpacingAfter = 0;
                            }
                        }
                    }
                }

                if (i < parts.Count)
                {
                    var blankLineParagraph = doc.CreateParagraph();
                    var blankLineRun = blankLineParagraph.CreateRun();

                    blankLineRun.SetText(Environment.NewLine);
                }
            }

            string filePath = this.option.FilePath;

            using (FileStream fs = File.Create(filePath))
            {
                doc.Write(fs);

                doc.Close();
            }

            return filePath;
        }
    }
}
