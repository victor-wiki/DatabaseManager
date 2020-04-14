using DatabaseConverter.Core;
using DatabaseInterpreter.Model;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;

namespace DatabaseManager.Helper
{
    public class RichTextBoxHelper
    {
        public static void AppendMessage(RichTextBox richTextBox, string message, bool isError = false)
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
            richTextBox.ScrollToCaret();
        }

        public static void Highlighting(RichTextBox richTextBox, DatabaseType databaseType)
        {
            var keywords = KeywordManager.GetKeywords(databaseType);
            var functions = FunctionManager.GetFunctions(databaseType).Except(keywords);

            string keywordsRegex = $@"\b({string.Join("|", keywords)})\b";
            string functionsRegex = $@"\b({string.Join("|", functions)})\b";
            string stringRegex = $@"(['][^'^(^)]*['])";

            Highlighting(richTextBox, keywordsRegex, RegexOptions.IgnoreCase, Color.Blue);             
            Highlighting(richTextBox, functionsRegex, RegexOptions.IgnoreCase, ColorTranslator.FromHtml("#FF00FF"));
            Highlighting(richTextBox, stringRegex, RegexOptions.IgnoreCase, Color.Red);           

            richTextBox.SelectionStart = 0;
            richTextBox.SelectionLength = 0;
            richTextBox.Focus();
        }

        public static void Highlighting(RichTextBox richTextBox, string regex, RegexOptions option, Color color)
        {
            MatchCollection matches = Regex.Matches(richTextBox.Text, regex, option);

            foreach (Match m in matches)
            {
                richTextBox.SelectionStart = m.Index;
                richTextBox.SelectionLength = m.Length;
                richTextBox.SelectionColor = color;
            }
        }
    }
}
