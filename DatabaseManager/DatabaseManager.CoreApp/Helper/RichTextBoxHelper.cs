using DatabaseConverter.Core;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DatabaseManager.Helper
{
    public class RichTextBoxHelper
    {
        public static void AppendMessage(RichTextBox richTextBox, string message, bool isError = false, bool scrollToCaret = true)
        {
            int start = richTextBox.Text.Length;

            if (start > 0)
            {
                richTextBox.AppendText(Environment.NewLine);
            }

            richTextBox.AppendText(message);

            richTextBox.Select(start, richTextBox.Text.Length - start);
            richTextBox.SelectionColor = isError ? Color.Red : Color.Black;

            richTextBox.SelectionStart = richTextBox.TextLength;

            if (scrollToCaret)
            {
                richTextBox.ScrollToCaret();
            }
        }

        public static void Highlighting(RichTextBox richTextBox, DatabaseType databaseType, bool keepPosition = true, int? startIndex = null, int? stopIndex = null)
        {
            int start = richTextBox.SelectionStart;

            var dataTypes = DataTypeManager.GetDataTypeSpecifications(databaseType).Select(item => item.Name);
            var keywords = KeywordManager.GetKeywords(databaseType);
            var functions = FunctionManager.GetFunctionSpecifications(databaseType).Select(item => item.Name).Except(keywords);

            string dataTypesRegex = $@"\b({string.Join("|", dataTypes)})\b";
            string keywordsRegex = $@"\b({string.Join("|", keywords)})\b";
            string functionsRegex = $@"\b({string.Join("|", functions)})\b";
            string stringRegex = $@"(['][^'^(^)]*['])";

            Highlighting(richTextBox, dataTypesRegex, RegexOptions.IgnoreCase, Color.Blue);
            Highlighting(richTextBox, keywordsRegex, RegexOptions.IgnoreCase, Color.Blue);
            Highlighting(richTextBox, functionsRegex, RegexOptions.IgnoreCase, ColorTranslator.FromHtml("#FF00FF"));
            Highlighting(richTextBox, stringRegex, RegexOptions.IgnoreCase, Color.Red);

            string commentString = databaseType == DatabaseType.MySql ? "#" : "--";
            string commentRegex = $@"({commentString}).*[\n]?";
            Highlighting(richTextBox, commentRegex, RegexOptions.IgnoreCase, Color.Green);

            richTextBox.SelectionStart = keepPosition ? start : 0;
            richTextBox.SelectionLength = 0;
            richTextBox.Focus();
        }

        public static void Highlighting(RichTextBox richTextBox, string regex, RegexOptions option, Color color, int? startIndex = null, int? stopIndex = null)
        {            
            string text = richTextBox.Text;

            if (startIndex.HasValue && stopIndex.HasValue)
            {
                text = text.Substring(startIndex.Value, stopIndex.Value - startIndex.Value + 1);
            }

            try
            {
                MatchCollection matches = Regex.Matches(text, regex, option);

                foreach (Match m in matches)
                {
                    int index = m.Index;

                    string leftChar = index > 0 ? text[index - 1].ToString() : "";
                    string rightChar = index + m.Value.Length < text.Length ? text[index + m.Value.Length].ToString() : "";

                    if (!m.Value.Contains("\n"))
                    {
                        string quotationValuePattern = $@"[""\[`]({m.Value})[""\]`]";

                        if (leftChar.Length > 0 && rightChar.Length > 0 && Regex.IsMatch($"{leftChar}{m.Value}{rightChar}", quotationValuePattern))
                        {
                            continue;
                        }
                    }

                    richTextBox.SelectionStart = m.Index + (startIndex.HasValue ? startIndex.Value : 0);
                    richTextBox.SelectionLength = m.Length;
                    richTextBox.SelectionFont = richTextBox.Font;
                    richTextBox.SelectionColor = color;
                }
            }
            catch(Exception ex)
            {

            }            
        }

        public static void HighlightingError(RichTextBox richTextBox, object error)
        {
            if (error is SqlSyntaxError sqlSyntaxError)
            {
                foreach (SqlSyntaxErrorItem item in sqlSyntaxError.Items)
                {
                    int rowIndex = item.Line - 1;

                    int startIndex = richTextBox.GetFirstCharIndexFromLine(rowIndex) + item.Column;

                    richTextBox.SelectionStart = startIndex;
                    richTextBox.SelectionLength = item.Text.Length;

                    richTextBox.SelectionColor = Color.Red;
                    richTextBox.SelectionBackColor = Color.Yellow;
                }
            }

            richTextBox.SelectionStart = 0;
            richTextBox.SelectionLength = 0;
            richTextBox.Focus();
        }
    }
}
