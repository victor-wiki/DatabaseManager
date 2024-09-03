using DatabaseInterpreter.Model;
using DatabaseManager.Model;
using SqlAnalyser.Model;
using SqlCodeEditor;
using SqlCodeEditor.Actions;
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
                    var marker = new TextMarker(item.StartIndex, item.StopIndex - item.StartIndex + 1,
                               TextMarkerType.SolidBlock, Color.Yellow, Color.Black);

                    editor.Document.MarkerStrategy.AddMarker(marker);
                }
            }

            editor.Refresh();
        }

        public static void ClearMarkers(TextEditorControlEx editor)
        {
            editor.Document.MarkerStrategy.RemoveAll(item => true);
            editor.Refresh();
        }

        public static void Paste(TextEditorControlEx editor)
        {
            new Paste().Execute(editor.ActiveTextAreaControl.TextArea);
            editor.ActiveTextAreaControl.Focus();
            editor.ActiveTextAreaControl.TextArea.Focus();
        }        
    }
}
