using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Core;
using DatabaseManager.Data;
using DatabaseManager.Forms;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace DatabaseManager.Controls
{
    public delegate void QueryEditorInfoMessageHandler(string information);

    public partial class UC_QueryEditor : UserControl
    {
        private Regex nameRegex = new Regex(@"\b(^[_a-zA-Z][ _0-9a-zA-Z]+$)\b");
        private string namePattern = @"\b([_a-zA-Z][_0-9a-zA-Z]+)\b";
        private string nameWithSpacePattern = @"\b([_a-zA-Z][ _0-9a-zA-Z]+)\b";
        private SchemaInfo schemaInfo;
        private IEnumerable<string> keywords;
        private IEnumerable<FunctionSpecification> builtinFunctions;
        private List<SqlWord> allWords;
        private bool intellisenseSetuped;
        private bool enableIntellisense;
        private bool isPasting = false;
        private List<string> dbSchemas;
        private const int WordListMinWidth = 160;
        private string commentString { get { return RichTextBoxHelper.GetCommentString(this.DatabaseType); } }
        private frmFindBox findBox;
        public DatabaseType DatabaseType { get; set; }
        public DbInterpreter DbInterpreter { get; set; }
        public event EventHandler SetupIntellisenseRequired;

        public QueryEditorInfoMessageHandler OnQueryEditorInfoMessage;
        public UC_QueryEditor()
        {
            InitializeComponent();

            this.lvWords.MouseWheel += LvWords_MouseWheel;
            this.panelWords.VerticalScroll.Enabled = true;
            this.panelWords.VerticalScroll.Visible = true;
        }

        public void Init()
        {
            this.keywords = KeywordManager.GetKeywords(this.DatabaseType);
            this.builtinFunctions = FunctionManager.GetFunctionSpecifications(this.DatabaseType);
        }

        private void LvWords_MouseWheel(object sender, MouseEventArgs e)
        {
            if (this.panelWords.Visible && this.txtToolTip.Visible)
            {
                this.txtToolTip.Visible = false;
            }
        }

        public RichTextBox Editor => this.txtEditor;

        public void SetupIntellisence()
        {
            this.intellisenseSetuped = true;
            this.enableIntellisense = true;
            this.schemaInfo = DataStore.GetSchemaInfo(this.DatabaseType);
            this.allWords = SqlWordFinder.FindWords(this.DatabaseType, "");
            this.dbSchemas = this.allWords.Where(item => item.Type == SqlWordTokenType.Schema).Select(item => item.Text).ToList();
        }

        private void tsmiCopy_Click(object sender, EventArgs e)
        {
            this.CopyText();
        }

        private void CopyText()
        {
            Clipboard.SetDataObject(this.txtEditor.SelectedText);
        }

        private void tsmiPaste_Click(object sender, EventArgs e)
        {
            this.txtEditor.Paste();
        }

        private void txtEditor_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.tsmiCopy.Enabled = this.txtEditor.SelectionLength > 0;
                this.tsmiDisableIntellisense.Text = $"{(this.enableIntellisense ? "Disable" : "Enable")} Intellisense";
                this.tsmiUpdateIntellisense.Visible = this.enableIntellisense;
                this.editorContexMenu.Show(this.txtEditor, e.Location);
            }
        }

        private void txtEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                if (FormEventCenter.OnRunScripts != null)
                {
                    FormEventCenter.OnRunScripts();
                }
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                this.isPasting = true;
                return;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                this.ShowFindBox();
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                this.CopyText();
            }

            if (!this.enableIntellisense)
            {
                return;
            }

            if (e.KeyCode == Keys.Down)
            {
                if (this.panelWords.Visible && !this.lvWords.Focused)
                {
                    this.lvWords.Focus();

                    if (this.lvWords.Items.Count > 0)
                    {
                        this.lvWords.Items[0].Selected = true;
                    }

                    e.SuppressKeyPress = true;
                }
            }
        }

        private void ShowFindBox()
        {
            if (this.findBox == null || this.findBox.IsDisposed)
            {
                this.findBox = new frmFindBox(true);

                this.findBox.OnFind += this.FindBox_OnFind;
                this.findBox.OnEndFind += this.FindBox_OnEndFind;
            }

            this.findBox.StartPosition = FormStartPosition.Manual;

            Control topControl = this.GetTopConrol();

            if (topControl != null)
            {
                this.findBox.Location = new Point(topControl.Left + topControl.Width - this.findBox.Width - 40, topControl.Top + 130);
            }
            else
            {
                this.findBox.Location = new Point(1000, 150);
            }

            this.findBox.Show();
        }

        private void FindBox_OnEndFind()
        {
            this.ClearSelection();
        }

        private Control GetTopConrol()
        {
            Control parent = this.Parent;

            while (parent != null)
            {
                if (parent.Parent == null)
                {
                    return parent;
                }

                parent = parent.Parent;
            }

            return null;
        }

        private void FindBox_OnFind()
        {
            RichTextBoxHelper.HighlightingFindWord(this.txtEditor, findBox.FindWord, findBox.MatchCase, findBox.MatchWholeWord);
        }

        private void txtEditor_KeyUp(object sender, KeyEventArgs e)
        {
            this.ShowCurrentPosition();

            if (this.isPasting)
            {
                return;
            }

            try
            {
                this.HandleKeyUpForIntellisense(e);
            }
            catch (Exception ex)
            {
            }
        }

        private void HandleKeyUpForIntellisense(KeyEventArgs e)
        {
            if (e.KeyValue >= 112 && e.KeyValue <= 123)
            {
                this.SetWordListViewVisible(false);
                return;
            }

            if (e.KeyCode == Keys.Space)
            {
                this.ClearStyleForSpace();

                this.SetWordListViewVisible(false);
            }

            SqlWordToken token = this.GetLastWordToken();

            if (token == null || token.Text == null || token.Type != SqlWordTokenType.None)
            {
                this.SetWordListViewVisible(false);

                if (token != null && token.Text != null)
                {
                    if (token.Type != SqlWordTokenType.String && token.Text.Contains("'"))
                    {
                        this.ClearStyle(token);
                    }
                }
            }

            if (this.enableIntellisense && this.panelWords.Visible)
            {
                if (this.lvWords.Tag is SqlWord word)
                {
                    if (word.Type == SqlWordTokenType.Table)
                    {
                        string columnName = null;

                        int index = this.txtEditor.SelectionStart;
                        char c = this.txtEditor.Text[index - 1];

                        if (c != '.')
                        {
                            columnName = token.Text;
                        }

                        this.ShowTableColumns(word.Text, columnName);
                    }
                    else if (word.Type == SqlWordTokenType.Schema)
                    {
                        this.ShowDbObjects(token.Text);
                    }

                    return;
                }
            }

            if (e.KeyData == Keys.OemPeriod)
            {
                if (this.enableIntellisense)
                {
                    if (token.Type == SqlWordTokenType.String)
                    {
                        return;
                    }

                    SqlWord word = this.FindWord(token.Text);

                    if (word.Type == SqlWordTokenType.Table)
                    {
                        this.ShowTableColumns(word.Text);
                        this.lvWords.Tag = word;
                    }
                    else if (word.Type == SqlWordTokenType.Schema)
                    {
                        this.ShowDbObjects(null, word.Text);
                        this.lvWords.Tag = word;
                    }
                }
            }
            else if (e.KeyCode == Keys.Back)
            {
                if (this.enableIntellisense && this.panelWords.Visible)
                {
                    if (!this.IsMatchWord(token.Text) || this.txtEditor.Text.Length == 0)
                    {
                        this.SetWordListViewVisible(false);
                    }
                    else
                    {
                        this.ShowWordListByToken(token);
                    }
                }

                if (token != null && token.Text.Length > 0 && this.commentString.Contains(token.Text.Last()))
                {
                    this.HighlightingWord(token);
                }
            }
            else if (e.KeyValue < 48 || (e.KeyValue >= 58 && e.KeyValue <= 64) || (e.KeyValue >= 91 && e.KeyValue <= 96) || e.KeyValue > 122)
            {
                this.SetWordListViewVisible(false);
            }
            else
            {
                if (this.enableIntellisense)
                {
                    if (!this.IsWordInQuotationChar(token))
                    {
                        this.ShowWordListByToken(token);
                    }
                }
            }
        }

        private bool IsWordInQuotationChar(SqlWordToken token)
        {
            if(token == null)
            {
                return false;
            }

            int startIndex = token.StopIndex;

            if (startIndex == 0)
            {
                return false;
            }

            int singleQotationCharCount = this.txtEditor.Text.Substring(0, startIndex).Count(item => item == '\'');

            return singleQotationCharCount % 2 != 0;
        }

        private void HighlightingWord(SqlWordToken token)
        {
            int start = this.txtEditor.SelectionStart;
            int lineIndex = this.txtEditor.GetLineFromCharIndex(start);
            int stop = this.txtEditor.GetFirstCharIndexFromLine(lineIndex) + this.txtEditor.Lines[lineIndex].Length - 1;

            RichTextBoxHelper.Highlighting(this.txtEditor, this.DatabaseType, true, start, stop); ;
        }

        private void ShowWordListByToken(SqlWordToken token)
        {
            if (token == null || string.IsNullOrEmpty(token.Text) || token.Type == SqlWordTokenType.Number)
            {
                this.SetWordListViewVisible(false);

                return;
            }

            SqlWordTokenType type = this.DetectTypeByWord(token.Text);

            var words = SqlWordFinder.FindWords(this.DatabaseType, token.Text, type);

            this.ShowWordList(words);
        }

        private void ShowTableColumns(string tableName, string columnName = null)
        {
            IEnumerable<SqlWord> columns = SqlWordFinder.FindWords(this.DatabaseType, columnName, SqlWordTokenType.TableColumn, tableName);

            this.ShowWordList(columns);
        }

        private void ShowDbObjects(string search, string owner = null)
        {
            IEnumerable<SqlWord> words = SqlWordFinder.FindWords(this.DatabaseType, search, SqlWordTokenType.Table | SqlWordTokenType.View | SqlWordTokenType.Function, owner);

            if (!string.IsNullOrEmpty(search))
            {
                List<SqlWord> sortedWords = new List<SqlWord>();

                sortedWords.AddRange(words.Where(item => item.Text.StartsWith(search, StringComparison.OrdinalIgnoreCase)));
                sortedWords.AddRange(words.Where(item => !item.Text.StartsWith(search, StringComparison.OrdinalIgnoreCase)));

                this.ShowWordList(sortedWords);
            }
            else
            {
                this.ShowWordList(words);
            }
        }

        private void ShowWordList(IEnumerable<SqlWord> words)
        {
            if (words.Count() > 0)
            {
                this.lvWords.Items.Clear();

                foreach (SqlWord sw in words)
                {
                    ListViewItem item = new ListViewItem();

                    switch (sw.Type)
                    {
                        case SqlWordTokenType.Keyword:
                            item.ImageIndex = 0;
                            break;
                        case SqlWordTokenType.BuiltinFunction:
                        case SqlWordTokenType.Function:
                            item.ImageIndex = 1;
                            break;
                        case SqlWordTokenType.Table:
                            item.ImageIndex = 2;
                            break;
                        case SqlWordTokenType.View:
                            item.ImageIndex = 3;
                            break;
                        case SqlWordTokenType.TableColumn:
                            item.ImageIndex = 4;
                            break;
                        case SqlWordTokenType.Schema:
                            item.ImageIndex = 5;
                            break;
                    }

                    item.SubItems.Add(sw.Text);
                    item.SubItems[1].Tag = sw.Type;
                    item.Tag = sw.Source;

                    this.lvWords.Items.Add(item);
                }

                string longestText = words.OrderByDescending(item => item.Text.Length).FirstOrDefault().Text;

                int width = this.MeasureTextWidth(this.lvWords, longestText);

                this.lvWords.Columns[1].Width = width + 20;

                int totalWidth = this.lvWords.Columns.Cast<ColumnHeader>().Sum(item => item.Width) + 50;

                this.panelWords.Width = totalWidth < WordListMinWidth ? WordListMinWidth : totalWidth;

                this.SetWordListPanelPostition();

                this.SetWordListViewVisible(true);
            }
            else
            {
                this.SetWordListViewVisible(false);
            }
        }

        private void SetWordListPanelPostition()
        {
            Point point = this.txtEditor.GetPositionFromCharIndex(txtEditor.SelectionStart);
            point.Y += (int)Math.Ceiling(this.txtEditor.Font.GetHeight()) + 2;
            point.X += 2;

            this.panelWords.Location = point;
        }

        private void ClearStyle(SqlWordToken token)
        {
            this.txtEditor.Select(token.StartIndex, token.StopIndex - token.StartIndex + 1);
            this.txtEditor.SelectionColor = Color.Black;
            this.txtEditor.SelectionStart = token.StopIndex + 1;
            this.txtEditor.SelectionLength = 0;
        }

        private void ClearStyleForSpace()
        {
            int start = this.txtEditor.SelectionStart;
            this.txtEditor.Select(start - 1, 1);
            this.txtEditor.SelectionColor = Color.Black;
            this.txtEditor.SelectionStart = start;
            this.txtEditor.SelectionLength = 0;
        }

        private SqlWordTokenType DetectTypeByWord(string word)
        {
            switch (word.ToUpper())
            {
                case "FROM":
                    return SqlWordTokenType.Table | SqlWordTokenType.View;
            }

            return SqlWordTokenType.None;
        }

        private bool IsMatchWord(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return false;
            }

            var words = SqlWordFinder.FindWords(this.DatabaseType, word);

            return words.Count > 0;
        }

        private void SetWordListViewVisible(bool visible)
        {
            if (visible)
            {
                this.panelWords.BringToFront();
                this.panelWords.Show();
            }
            else
            {
                this.txtToolTip.Hide();
                this.panelWords.Hide();
                this.lvWords.Tag = null;
            }
        }

        private SqlWordToken GetLastWordToken(bool noAction = false, bool isInsert = false)
        {
            SqlWordToken token = null;

            int currentIndex = this.txtEditor.SelectionStart;
            int lineIndex = this.txtEditor.GetLineFromCharIndex(currentIndex);
            int lineFirstCharIndex = this.txtEditor.GetFirstCharIndexOfCurrentLine();

            int index = currentIndex - 1;

            if (index < 0 || index > this.txtEditor.Text.Length - 1)
            {
                return token;
            }

            token = new SqlWordToken();

            bool isDot = false;

            if (this.txtEditor.Text[index] == '.')
            {
                isDot = true;

                if (isInsert)
                {
                    token.StartIndex = token.StopIndex = this.txtEditor.SelectionStart;
                    token.Text = ".";

                    return token;
                }

                index = index - 1;
            }

            token.StopIndex = index;

            string lineBefore = this.txtEditor.Text.Substring(lineFirstCharIndex, currentIndex - lineFirstCharIndex);

            bool isComment = false;

            if (this.DbInterpreter != null && lineBefore.Contains(this.commentString))
            {
                isComment = true;
            }

            string word = "";

            if (!isComment)
            {
                List<char> chars = new List<char>();

                string delimeterPattern = @"[ ,\.\r\n=]";

                int i = -1;

                bool existed = false;
                for (i = index; i >= 0; i--)
                {
                    char c = this.txtEditor.Text[i];

                    if (!Regex.IsMatch(c.ToString(), delimeterPattern))
                    {
                        chars.Add(c);

                        if (c == '\'')
                        {
                            break;
                        }
                        else if (c == '(')
                        {
                            if (chars.Count > 1)
                            {
                                chars.RemoveAt(chars.Count - 1);
                                i++;
                            }

                            break;
                        }
                    }
                    else
                    {
                        existed = true;
                        break;
                    }
                }

                if (i == -1)
                {
                    i = 0;
                }

                chars.Reverse();

                word = string.Join("", chars);

                token.Text = word;

                token.StartIndex = i + (existed ? 1 : 0);

                if (token.StartIndex == token.StopIndex && isInsert && word.Length > 0)
                {
                    token.StopIndex = token.StartIndex + word.Length;
                }

                if (word.Contains("'"))
                {
                    int singQuotationCount = lineBefore.Count(item => item == '\'');

                    bool isQuotationPaired = singQuotationCount % 2 == 0;

                    if (isQuotationPaired && word.StartsWith("'"))
                    {
                        List<char> afterChars = new List<char>();

                        for (int j = currentIndex; j < this.txtEditor.Text.Length; j++)
                        {
                            char c = this.txtEditor.Text[j];

                            if (Regex.IsMatch(c.ToString(), delimeterPattern))
                            {
                                break;
                            }
                            else
                            {
                                afterChars.Add(c);
                            }
                        }

                        string afterWord = string.Join("", afterChars);

                        if (afterWord.EndsWith("'") || (word == "'" && afterChars.Count == 0))
                        {
                            token.Type = SqlWordTokenType.String;
                        }
                        else
                        {
                            token.StartIndex++;
                            token.Text = token.Text.Substring(1);
                        }
                    }
                    else if (!isQuotationPaired || (isQuotationPaired && word.EndsWith("'")))
                    {
                        token.Type = SqlWordTokenType.String;
                    }

                    if (token.Type == SqlWordTokenType.String)
                    {
                        if (!isDot)
                        {
                            this.SetWordColor(token);
                        }

                        return token;
                    }
                }
            }
            else
            {
                int firstIndexOfComment = lineFirstCharIndex + lineBefore.IndexOf(this.commentString);

                token.StartIndex = firstIndexOfComment;
                token.StopIndex = lineFirstCharIndex + this.txtEditor.Lines[lineIndex].Length - 1;
            }

            string trimedWord = this.TrimQuotationChars(word);

            if (!noAction)
            {
                if (this.enableIntellisense && this.dbSchemas.Any(item => item.ToUpper() == trimedWord.ToUpper()))
                {
                    token.Type = SqlWordTokenType.Schema;
                }
                else if (this.keywords.Any(item => item.ToUpper() == word.ToUpper()))
                {
                    token.Type = SqlWordTokenType.Keyword;

                    this.SetWordColor(token);
                }
                else if (this.builtinFunctions.Any(item => item.Name.ToUpper() == trimedWord.ToUpper()))
                {
                    token.Type = SqlWordTokenType.BuiltinFunction;

                    this.SetWordColor(token);
                }
                else if (isComment)
                {
                    token.Type = SqlWordTokenType.Comment;

                    this.SetWordColor(token, true);
                }
                else if (long.TryParse(word, out _))
                {
                    token.Type = SqlWordTokenType.Number;
                }
                else
                {
                    if (!isDot && !this.IsWordInQuotationChar(token))
                    {
                        this.ClearStyle(token);
                    }
                }
            }

            return token;
        }

        private SqlWord FindWord(string text)
        {
            text = this.TrimQuotationChars(text);

            SqlWord word = null;

            if (this.dbSchemas.Count > 0 && this.dbSchemas.Any(item => text.ToUpper() == item.ToUpper()))
            {
                word = new SqlWord() { Type = SqlWordTokenType.Schema, Text = text };

                return word;
            }

            word = this.allWords.FirstOrDefault(item => item.Text.ToUpper() == text.ToUpper()
                                && (item.Type == SqlWordTokenType.Table || item.Type == SqlWordTokenType.View));

            if (word != null)
            {
                return word;
            }
            else
            {
                word = new SqlWord() { Text = text };
            }

            char quotationLeftChar = this.DbInterpreter.QuotationLeftChar;
            char quotationRightChar = this.DbInterpreter.QuotationRightChar;

            string quotationNamePattern = $@"([{quotationLeftChar}]{nameWithSpacePattern}[{quotationRightChar}])";

            Regex regex = new Regex($@"({namePattern}|{quotationNamePattern})[\s\n\r]+(AS[\s\n\r]+)?\b({text})\b", RegexOptions.IgnoreCase);

            var matches = regex.Matches(this.txtEditor.Text);

            string name = "";
            foreach (Match match in matches)
            {
                if (match.Value.Trim().ToUpper() != text.ToUpper())
                {
                    int lastIndexOfSpace = match.Value.LastIndexOf(' ');

                    string value = Regex.Replace(match.Value.Substring(0, lastIndexOfSpace), @" AS[\s\n\r]?", "", RegexOptions.IgnoreCase).Trim();

                    if (!this.keywords.Any(item => item.ToUpper() == value.ToUpper()))
                    {
                        name = this.TrimQuotationChars(value);
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                name = text;
            }

            if (this.schemaInfo.Tables.Any(item => item.Name.ToUpper() == name.ToUpper()))
            {
                word.Text = name;
                word.Type = SqlWordTokenType.Table;
            }

            return word;
        }

        private string TrimQuotationChars(string value)
        {
            if (this.DbInterpreter != null)
            {
                return value.Trim(this.DbInterpreter.QuotationLeftChar, this.DbInterpreter.QuotationRightChar, '"');
            }

            return value;
        }

        private void SetWordColor(SqlWordToken token, bool keepCurrentPos = false)
        {
            if (!SettingManager.Setting.EnableEditorHighlighting)
            {
                return;
            }

            Color color = Color.Black;

            if (token.Type == SqlWordTokenType.Keyword)
            {
                color = Color.Blue;
            }
            else if (token.Type == SqlWordTokenType.BuiltinFunction)
            {
                color = ColorTranslator.FromHtml("#FF00FF");
            }
            else if (token.Type == SqlWordTokenType.String)
            {
                color = Color.Red;
            }
            else if (token.Type == SqlWordTokenType.Comment)
            {
                color = ColorTranslator.FromHtml("#008000");
            }

            int start = this.txtEditor.SelectionStart;

            this.txtEditor.Select(token.StartIndex, token.StopIndex - token.StartIndex + 1);
            this.txtEditor.SelectionBackColor = this.txtEditor.BackColor;
            this.txtEditor.SelectionColor = color;
            this.txtEditor.SelectionStart = keepCurrentPos ? start : token.StopIndex + 1;
            this.txtEditor.SelectionLength = 0;
        }

        private void InsertSelectedWord()
        {
            try
            {
                SqlWordToken token = this.GetLastWordToken(true, true);

                ListViewItem item = this.lvWords.SelectedItems[0];
                object tag = item.Tag;

                string selectedWord = item.SubItems[1].Text;

                int length = token.StartIndex == token.StopIndex ? 0 : token.StopIndex - token.StartIndex + 1;

                this.txtEditor.Select(token.StartIndex, length);

                string quotationValue = selectedWord;

                if (!(tag is FunctionSpecification))
                {
                    quotationValue = this.DbInterpreter.GetQuotedString(selectedWord);
                }

                this.txtEditor.SelectedText = quotationValue;

                this.SetWordListViewVisible(false);

                this.txtEditor.SelectionStart = this.txtEditor.SelectionStart;
                this.txtEditor.Focus();
            }
            catch (Exception ex)
            {

            }
        }

        private void ShowCurrentPosition()
        {
            string message = "";

            if (this.txtEditor.SelectionStart >= 0)
            {
                int lineIndex = this.txtEditor.GetLineFromCharIndex(this.txtEditor.SelectionStart);
                int column = this.txtEditor.SelectionStart - this.txtEditor.GetFirstCharIndexOfCurrentLine() + 1;

                message = $"Line:{lineIndex + 1}  Column:{column} Index:{this.txtEditor.SelectionStart}";
            }
            else
            {
                message = "";
            }

            if (this.OnQueryEditorInfoMessage != null)
            {
                this.OnQueryEditorInfoMessage(message);
            }
        }

        private void lvWords_DoubleClick(object sender, EventArgs e)
        {
            if (this.lvWords.SelectedItems.Count > 0)
            {
                this.InsertSelectedWord();
            }
        }

        private void lvWords_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.InsertSelectedWord();
            }
            else if (!(e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
            {
                if (this.panelWords.Visible)
                {
                    this.panelWords.Visible = false;
                    this.txtEditor.SelectionStart = this.txtEditor.SelectionStart;
                    this.txtEditor.Focus();
                }
            }
        }

        private void tsmiDisableIntellisense_Click(object sender, EventArgs e)
        {
            if (this.enableIntellisense)
            {
                this.enableIntellisense = false;
                this.intellisenseSetuped = false;
            }
            else
            {
                if (!this.intellisenseSetuped)
                {
                    if (this.SetupIntellisenseRequired != null)
                    {
                        this.SetupIntellisenseRequired(this, null);
                    }
                }
            }
        }

        private void txtEditor_SelectionChanged(object sender, EventArgs e)
        {
            if (this.isPasting)
            {
                this.isPasting = false;

                RichTextBoxHelper.Highlighting(this.txtEditor, this.DatabaseType);
            }
        }

        private void lvWords_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.txtToolTip.Visible = false;

            if (this.lvWords.SelectedItems.Count > 0)
            {
                ListViewItem item = this.lvWords.SelectedItems[0];

                object source = item.Tag;
                string tooltip = null;

                if (source is FunctionSpecification funcSpec)
                {
                    tooltip = $"{funcSpec.Name}({funcSpec.Args})";
                }
                else if (source is TableColumn column)
                {
                    tooltip = $"{column.Name}({this.DbInterpreter.ParseDataType(column)})";
                }

                if (!string.IsNullOrEmpty(tooltip))
                {
                    this.ShowTooltip(tooltip, item);
                }
            }
        }

        private void ShowTooltip(string text, ListViewItem item)
        {
            this.txtToolTip.Text = text;

            this.txtToolTip.Location = new Point(this.panelWords.Location.X + this.panelWords.Width, this.panelWords.Location.Y + item.Position.Y);

            this.txtToolTip.Width = this.MeasureTextWidth(this.txtToolTip, text);

            this.txtToolTip.Visible = true;
        }

        private int MeasureTextWidth(Control control, string text)
        {
            using (Graphics g = this.CreateGraphics())
            {
                return (int)Math.Ceiling(g.MeasureString(text, control.Font).Width);
            }
        }

        private void tsmiUpdateIntellisense_Click(object sender, EventArgs e)
        {
            if (this.SetupIntellisenseRequired != null)
            {
                this.SetupIntellisenseRequired(this, null);
            }
        }

        private void txtEditor_MouseClick(object sender, MouseEventArgs e)
        {
            this.HandleMouseDownClick(e);
        }

        private void txtEditor_MouseDown(object sender, MouseEventArgs e)
        {
            this.HandleMouseDownClick(e);
        }

        private void HandleMouseDownClick(MouseEventArgs e)
        {
            this.ShowCurrentPosition();

            this.isPasting = false;

            if (!this.enableIntellisense)
            {
                return;
            }

            this.txtToolTip.Visible = false;

            if (this.panelWords.Visible && !this.panelWords.Bounds.Contains(e.Location))
            {
                this.panelWords.Visible = false;
                this.lvWords.Items.Clear();
                this.lvWords.Tag = null;
            }
        }

        private void tsmiSelectAll_Click(object sender, EventArgs e)
        {
            this.txtEditor.SelectAll();
        }

        private void tsmiValidateScripts_Click(object sender, EventArgs e)
        {
            this.ClearSelection();

            this.ValidateScripts(true);
        }

        internal async void ValidateScripts(bool showMessageBox = false)
        {
            SqlSyntaxError error = await Task.Run(() => ScriptValidator.ValidateSyntax(this.DatabaseType, this.txtEditor.Text));

            if (error != null && error.HasError)
            {
                if (showMessageBox)
                {
                    frmTextContent msgBox = new frmTextContent("Error Message", error.ToString(), true);
                    msgBox.ShowDialog();
                }

                RichTextBoxHelper.HighlightingError(this.txtEditor, error);
            }
            else
            {
                if (showMessageBox)
                {
                    MessageBox.Show("The scripts is valid.");
                }
            }
        }

        private void ClearSelection()
        {
            int start = this.txtEditor.SelectionStart;

            this.txtEditor.SelectAll();
            this.txtEditor.SelectionBackColor = Color.White;
            this.txtEditor.SelectionStart = start;
            this.txtEditor.SelectionLength = 0;
        }

        private void editorContexMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool hasText = this.txtEditor.Text.Trim().Length > 0;
            this.tsmiValidateScripts.Visible = hasText;
            this.tsmiFindText.Visible = hasText;
        }

        internal void DisposeResources()
        {
            if (this.findBox != null && !this.findBox.IsDisposed)
            {
                this.findBox.Close();
                this.findBox.Dispose();
            }
        }

        private void tsmiFindText_Click(object sender, EventArgs e)
        {
            this.ShowFindBox();
        }
    }
}
