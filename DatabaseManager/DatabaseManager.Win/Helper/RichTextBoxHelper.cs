using DatabaseConverter.Core;
using DatabaseInterpreter.Model;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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

        public static void HighlightingKeywords(RichTextBox richTextBox, DatabaseType databaseType)
        {
            var keywords = KeywordManager.GetKeywords(databaseType);

            string keywordsRegex = $@"\b({string.Join("|", keywords)})\b";
            MatchCollection keywordMatches = Regex.Matches(richTextBox.Text, keywordsRegex, RegexOptions.IgnoreCase);

            foreach (Match m in keywordMatches)
            {
                richTextBox.SelectionStart = m.Index;
                richTextBox.SelectionLength = m.Length;
                richTextBox.SelectionColor = Color.Blue;
            }

            richTextBox.SelectionStart = 0;
            richTextBox.SelectionLength = 0;
            richTextBox.Focus();
        }
    }
}
