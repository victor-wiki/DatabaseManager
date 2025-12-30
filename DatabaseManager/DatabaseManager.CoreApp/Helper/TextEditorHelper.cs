using DatabaseInterpreter.Model;
using DatabaseManager.Core.Model;
using SqlAnalyser.Model;
using SqlCodeEditor;
using SqlCodeEditor.Document;
using System.Drawing;

namespace DatabaseManager.Helper
{
    public class TextEditorHelper
    {
        public static void ApplySetting(TextEditorControlEx editor, TextEditorOption option)
        {
            editor.ShowLineNumbers = option.ShowLineNumber;

            var oldFont = editor.Document.TextEditorProperties.Font;

            editor.Document.TextEditorProperties.Font = new Font(new FontFamily(option.FontName), option.FontSize, oldFont.Style);
        }

        public static string GetHighlightingType(DatabaseType databaseType)
        {
            string highlightingType;

            switch (databaseType)
            {
                case DatabaseType.MySql:
                    highlightingType = SyntaxModes.MySql;
                    break;
                case DatabaseType.Oracle:
                    highlightingType = SyntaxModes.PlSql;
                    break;
                case DatabaseType.Postgres:
                    highlightingType = SyntaxModes.PostgreSql;
                    break;
                case DatabaseType.Sqlite:
                    highlightingType = SyntaxModes.SqliteSql;
                    break;
                case DatabaseType.SqlServer:
                    highlightingType = SyntaxModes.TSql;
                    break;
                default:
                    highlightingType = string.Empty;
                    break;
            }

            return highlightingType;
        }       

        public static void Highlighting(TextEditorControlEx editor, DatabaseType databaseType)
        {
            editor.SyntaxHighlighting = TextEditorHelper.GetHighlightingType(databaseType);
        }

        public static void ClearHighlighting(TextEditorControlEx editor)
        {
            editor.SyntaxHighlighting = string.Empty;
        }

        public static void HighlightingError(TextEditorControlEx editor, object error)
        {
            if (error is SqlSyntaxError sqlSyntaxError)
            {
                foreach (SqlSyntaxErrorItem item in sqlSyntaxError.Items)
                {
                    AddMarker(editor, item.StartIndex, item.StopIndex - item.StartIndex + 1, Color.Yellow);
                }
            }

            editor.Refresh();
        }

        public static void AddMarker(TextEditorControlEx editor, int startIndex, int length, Color color)
        {
            var marker = new TextMarker(startIndex, length,
                              TextMarkerType.SolidBlock, color, Color.Black);

            editor.Document.MarkerStrategy.AddMarker(marker);
        }

        public static void ClearMarkers(TextEditorControlEx editor)
        {
            editor.Document.MarkerStrategy.RemoveAll(item => true);
            editor.Refresh();
        }

        public static int GetFirstCharIndexOfLine(TextEditorControlEx editor, int line)
        {
            int total = 0;

            for (int i = 0; i < line; i++)
            {
                var segment = editor.Document.GetLineSegment(i);

                total += segment.TotalLength;
            }

            return total;
        }

        public static void ClearText(TextEditorControlEx editor)
        {
            editor.Text = "";
            editor.Refresh();
            editor.Invalidate();
        }
    }
}
