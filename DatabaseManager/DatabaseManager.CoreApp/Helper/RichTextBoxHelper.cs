using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DatabaseManager.Helper
{
    public class RichTextBoxHelper
    {
        public static string GetCommentString(DatabaseType databaseType)
        {
            return databaseType == DatabaseType.MySql ? "#" : "--";
        }

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


        public static void Highlighting(RichTextBox richTextBox, DatabaseType databaseType, bool keepPosition = true, int? startIndex = null, int? stopIndex = null, bool forceHighlightling = false)
        {
            if (!SettingManager.Setting.EnableEditorHighlighting && !forceHighlightling)
            {
                richTextBox.SelectionStart = 0;
                richTextBox.SelectionLength = 0;
                richTextBox.Focus();
                return;
            }

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

            string commentString = GetCommentString(databaseType);
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
                    richTextBox.SelectionColor = color;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void HighlightingFindWord(RichTextBox richTextBox, string word, bool matchCase, bool matchWholeWord)
        {
            if (string.IsNullOrEmpty(word))
            {
                return;
            }

            string pattern = "";

            if (Regex.IsMatch(word, RegexHelper.NameRegexPattern))
            {
                pattern = $"\\b{word}\\b";
            }
            else
            {
                pattern = $"({word})";
            }

            string regex = matchWholeWord ? pattern : word;

            RegexOptions option = RegexOptions.Multiline;

            if (!matchCase)
            {
                option = option | RegexOptions.IgnoreCase;
            }

            regex = regex.Replace("[", "\\[").Replace("]", "\\]");

            try
            {
                MatchCollection matches = Regex.Matches(richTextBox.Text, regex, option);

                if (matches.Count > 0)
                {
                    HighlightingWord(richTextBox, matches.Select(item => new WordMatchInfo() { Index = item.Index, Length = item.Length }), Color.Orange);
                }
                else
                {
                    MessageBox.Show("No match.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHelper.GetExceptionDetails(ex));
            }
        }

        public static void HighlightingWord(RichTextBox richTextBox, IEnumerable<WordMatchInfo> matches, Color color)
        {
            richTextBox.SelectAll();
            richTextBox.SelectionBackColor = Color.White;
            richTextBox.SelectionStart = 0;
            richTextBox.SelectionLength = 0;

            int fisrtIndex = -1;

            foreach (var m in matches)
            {
                richTextBox.SelectionStart = m.Index;
                richTextBox.SelectionLength = m.Length;
                richTextBox.SelectionBackColor = color;

                if (fisrtIndex == -1)
                {
                    fisrtIndex = m.Index + m.Length;
                }
            }

            if (fisrtIndex >= 0)
            {
                richTextBox.SelectionStart = fisrtIndex;
            }

            richTextBox.SelectionLength = 0;
        }

        public static void HighlightingError(RichTextBox richTextBox, object error)
        {
            if (error is SqlSyntaxError sqlSyntaxError)
            {
                foreach (SqlSyntaxErrorItem item in sqlSyntaxError.Items)
                {
                    richTextBox.SelectionStart = item.StartIndex;
                    richTextBox.SelectionLength = item.StopIndex - item.StartIndex + 1;

                    //richTextBox.SelectionColor = Color.Red;
                    richTextBox.SelectionBackColor = Color.Yellow;
                }
            }

            richTextBox.SelectionStart = 0;
            richTextBox.SelectionLength = 0;
            richTextBox.Focus();
        }
    }
}
