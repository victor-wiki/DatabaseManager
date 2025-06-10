using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Core;
using DatabaseManager.Data;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using SqlAnalyser.Model;
using SqlCodeEditor;
using SqlCodeEditor.Document;
using SqlCodeEditor.Gui.CompletionWindow;
using SqlCodeEditor.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public delegate void RunScriptsHandler();
    public delegate void QueryEditorInfoMessageHandler(string information);

    public partial class UC_QueryEditor : UserControl
    {
        private string namePattern = @"\b([_a-zA-Z][_0-9a-zA-Z]*)\b";
        private string nameWithSpacePattern = @"\b([_a-zA-Z][ _0-9a-zA-Z]*)\b";
        private SchemaInfo schemaInfo;
        private IEnumerable<string> keywords;
        private IEnumerable<FunctionSpecification> builtinFunctions;
        private List<SqlWord> allWords;
        private bool intellisenseSetuped;
        private bool enableIntellisense;       
        private List<string> dbSchemas;
        private CodeCompletionWindow codeCompletionWindow;
        private string commentString => this.DbInterpreter?.CommentString;

        public RunScriptsHandler OnRunScripts;
        public DatabaseType DatabaseType { get; set; }
        public DbInterpreter DbInterpreter { get; set; }
        public event EventHandler SetupIntellisenseRequired;
        public QueryEditorInfoMessageHandler OnQueryEditorInfoMessage;
        public IDocument Document => this.txtEditor.Document;
        public TextEditorControlEx Editor => this.txtEditor;
        public TextArea TextArea => this.txtEditor.ActiveTextAreaControl.TextArea;
        public SelectionManager SelectionManager => this.TextArea.SelectionManager;
        public string SelectedText => this.SelectionManager.SelectedText;
        public ContextMenuStrip ContextMenu => this.txtEditor.ActiveTextAreaControl.ContextMenuStrip;

        private int CurrentCharIndex
        {
            get
            {
                return this.TextArea.Caret.Offset;
            }
        }

        private int SelectionStart { get { return this.CurrentCharIndex; } }

        public UC_QueryEditor()
        {
            InitializeComponent();
        }

        public void Init()
        {
            TextEditorControlBase.CheckForIllegalCrossThreadCalls = false;
            (this.Document.TextBufferStrategy as GapTextBufferStrategy).CheckTread = false;

            TextEditorHelper.ApplySetting(this.txtEditor, SettingManager.Setting.TextEditorOption);

            this.keywords = KeywordManager.GetKeywords(this.DatabaseType);
            this.builtinFunctions = FunctionManager.GetFunctionSpecifications(this.DatabaseType);

            this.txtEditor.FindForm.Shown += this.FindForm_Shown;
            this.TextArea.KeyDown += this.txtEditor_KeyDown;
            this.TextArea.KeyUp += this.txtEditor_KeyUp;
            this.TextArea.MouseDown += this.txtEditor_MouseDown;
            this.TextArea.MouseClick += this.txtEditor_MouseClick;

            ToolStripMenuItem tsmiDisableIntellisense = new ToolStripMenuItem("Disable Intellisense") { Name = "tsmiDisableIntellisense" };
            tsmiDisableIntellisense.Click += this.tsmiDisableIntellisense_Click;
            this.ContextMenu.Items.Add(tsmiDisableIntellisense);

            ToolStripMenuItem tsmiUpdateIntellisense = new ToolStripMenuItem("Update Intellisense") { Name = "tsmiUpdateIntellisense" };
            tsmiUpdateIntellisense.Click += this.tsmiUpdateIntellisense_Click;
            this.ContextMenu.Items.Add(tsmiUpdateIntellisense);

            ToolStripMenuItem tsmiValidateScripts = new ToolStripMenuItem("Validate Scripts") { Name = "tsmiValidateScripts" };
            tsmiValidateScripts.Click += this.tsmiValidateScripts_Click;
            this.ContextMenu.Items.Add(tsmiValidateScripts);

            this.ContextMenu.Opening += this.editorContexMenu_Opening;            

            this.txtEditor.SyntaxHighlighting = TextEditorHelper.GetHighlightingType(this.DatabaseType);           
        }

        private void FindForm_Shown(object sender, EventArgs e)
        {
            Control topLevelControl = this.TopLevelControl;

            if (topLevelControl != null)
            {
                this.txtEditor.FindForm.Location = new Point(topLevelControl.Left + topLevelControl.Width - this.txtEditor.FindForm.Width - 10, topLevelControl.Top + 120);
            }
            else
            {
                this.txtEditor.FindForm.Location = new Point(950, 112);
            }
        }
      

        public void SetupIntellisence()
        {
            this.intellisenseSetuped = true;
            this.enableIntellisense = true;
            this.schemaInfo = DataStore.GetSchemaInfo(this.DatabaseType);
            this.allWords = SqlWordFinder.FindWords(this.DatabaseType, "");
            this.dbSchemas = this.allWords.Where(item => item.Type == SqlWordTokenType.Schema).Select(item => item.Text).ToList();
        }

        private void txtEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                if (this.OnRunScripts != null)
                {
                    this.OnRunScripts();
                }
            }            
        }

        private void txtEditor_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                this.ShowCurrentPosition();

                this.HandleKeyUpForIntellisense(e);
            }
            catch (Exception ex)
            {
            }
        }

        private void HandleKeyUpForIntellisense(KeyEventArgs e)
        {
            if ((e.KeyValue >= 112 && e.KeyValue <= 123) || e.KeyData == Keys.Space || e.KeyData == Keys.Enter || e.KeyData == Keys.Escape)
            {
                this.CloseCodeCompletionWindow();
                return;
            }

            if (e.KeyData == Keys.Up || e.KeyData == Keys.Down)
            {
                if (this.IsCodeCompletionWindowVisible())
                {
                    return;
                }
            }

            SqlWordToken token = this.GetLastWordToken();

            if (token == null || token.Text == null || token.Type != SqlWordTokenType.None)
            {
                this.CloseCodeCompletionWindow();
            }

            if (this.enableIntellisense && token != null && !string.IsNullOrEmpty(token.Text))
            {
                bool needHandle = true;

                if(token.Type == SqlWordTokenType.Keyword)
                {
                    needHandle = false;
                }
                else
                {
                    string text = token.Text;

                    bool isKeywordStart = this.keywords.Any(item => item.ToUpper().StartsWith(text.ToUpper()));
                    bool isAllWordsStart = this.allWords.Any(item => item.Text.ToUpper().StartsWith(text.ToUpper()));

                    if (isKeywordStart && !isAllWordsStart)
                    {
                        needHandle = false;
                    }
                }              

                if (!needHandle)
                {
                    if(this.IsCodeCompletionWindowVisible())
                    {
                        this.CloseCodeCompletionWindow();
                    }
                   
                    return;
                }
            }

            if (this.enableIntellisense && this.IsCodeCompletionWindowVisible())
            {
                if (this.codeCompletionWindow.Tag is SqlWord word)
                {
                    if (word.Type == SqlWordTokenType.Table)
                    {
                        string columnName = null;

                        int index = this.SelectionStart;
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
                    if (token == null || token.Type == SqlWordTokenType.String)
                    {
                        return;
                    }

                    SqlWord word = this.FindWord(token.Text);

                    if (word.Type == SqlWordTokenType.Table)
                    {
                        this.ShowTableColumns(word.Text);
                        this.SetCodeCompletionWindowTag(word);
                    }
                    else if (word.Type == SqlWordTokenType.Schema)
                    {
                        this.ShowDbObjects(null, word.Text);
                        this.SetCodeCompletionWindowTag(word);
                    }
                }
            }
            else if (e.KeyCode == Keys.Back)
            {
                if (this.enableIntellisense && this.IsCodeCompletionWindowVisible())
                {
                    if (!this.IsMatchWord(token.Text) || this.txtEditor.Text.Length == 0)
                    {
                        this.CloseCodeCompletionWindow();
                    }
                    else
                    {
                        this.ShowWordListByToken(token);
                    }
                }
            }
            else if ((e.KeyValue < 48 && e.KeyValue!=16) || (e.KeyValue >= 58 && e.KeyValue <= 64) || (e.KeyValue >= 91 && e.KeyValue <= 96) || e.KeyValue > 122 )
            {
                this.CloseCodeCompletionWindow();
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

        private void SetCodeCompletionWindowTag(SqlWord word)
        {
            if (this.codeCompletionWindow != null)
            {
                this.codeCompletionWindow.Tag = word;
            }
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

        private bool IsWordInQuotationChar(SqlWordToken token)
        {
            if (token == null)
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

        private void ShowWordListByToken(SqlWordToken token)
        {
            if (token == null)
            {
                return;
            }

            if(token.Type == SqlWordTokenType.Number || token.Type == SqlWordTokenType.Comment)
            {
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
                SqlCompletionDataProvider provider = new SqlCompletionDataProvider();
                provider.InsertSelected += this.InsertSelectedWord;

                SqlWordToken[] tokens = words.Select(item => new SqlWordToken() { Type = item.Type, Text = item.Text }).ToArray();

                this.codeCompletionWindow = CodeCompletionWindow.ShowCompletionWindow(this.ParentForm, this.txtEditor, tokens, provider, ' ', false, false);
            }
        }

        private bool IsCodeCompletionWindowVisible()
        {
            return this.codeCompletionWindow != null && this.codeCompletionWindow.Visible;
        }

        private void CloseCodeCompletionWindow()
        {
            if (this.codeCompletionWindow != null)
            {
                this.codeCompletionWindow.Close();
                this.codeCompletionWindow.Dispose();
                this.codeCompletionWindow = null;
            }
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

        private SqlWordToken GetLastWordToken(bool noAction = false, bool isInsert = false)
        {
            SqlWordToken token = null;

            int currentIndex = this.CurrentCharIndex;
            int lineIndex = this.TextArea.Caret.Line;
            int lineFirstCharIndex = TextEditorHelper.GetFirstCharIndexOfLine(this.Editor, lineIndex);

            int index = currentIndex - 1;

            if (index < 0 || index > this.txtEditor.Text.Length - 1)
            {
                return token;
            }

            token = new SqlWordToken();

            if (this.txtEditor.Text[index] == '.')
            {
                if (isInsert)
                {
                    token.StartIndex = token.StopIndex = this.SelectionStart;
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

                string delimiterPattern = @"[ ,\.\r\n=]";

                int i = -1;

                bool existed = false;
                for (i = index; i >= 0; i--)
                {
                    char c = this.txtEditor.Text[i];

                    if (!Regex.IsMatch(c.ToString(), delimiterPattern))
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

                            if (Regex.IsMatch(c.ToString(), delimiterPattern))
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
                }
                else if (this.builtinFunctions.Any(item => item.Name.ToUpper() == trimedWord.ToUpper()))
                {
                    token.Type = SqlWordTokenType.BuiltinFunction;
                }
                else if (isComment)
                {
                    token.Type = SqlWordTokenType.Comment;
                }
                else if (long.TryParse(word, out _))
                {
                    token.Type = SqlWordTokenType.Number;
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

            char? quotationLeftChar = this.DbInterpreter.QuotationLeftChar;
            char? quotationRightChar = this.DbInterpreter.QuotationRightChar;

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
                if(this.DbInterpreter.QuotationLeftChar.HasValue)
                {
                    return value.Trim(this.DbInterpreter.QuotationLeftChar.Value, this.DbInterpreter.QuotationRightChar.Value, '"');
                }
                else
                {
                    return value.Trim('"');
                }
            }

            return value;
        }

        private void InsertSelectedWord(ICompletionData data, int offset, char key)
        {
            try
            {
                SqlWordToken token = this.GetLastWordToken(true, true);

                string selectedWord = data.Text;

                int length = token.StopIndex - token.StartIndex + 1;

                string quotationValue = selectedWord;               

                if (!this.builtinFunctions.Any(item=>item.Name.ToUpper() == data.Text.ToUpper()))
                {
                    quotationValue = this.DbInterpreter.GetQuotedString(selectedWord);
                }
              
                if (length > 0 && token.Text != ".")
                {
                    this.txtEditor.SelectText(token.StartIndex, length);
                }

                this.TextArea.InsertString(quotationValue);

                this.TextArea.Focus();
            }
            catch (Exception ex)
            {
            }
        }

        private void ShowCurrentPosition()
        {
            string message = "";

            int lineIndex = this.TextArea.Caret.Line;
            int column = this.TextArea.Caret.Column;
            int index = this.CurrentCharIndex;

            message = $"Line:{lineIndex + 1}  Column:{column + 1}  Index:{index}";

            if (this.OnQueryEditorInfoMessage != null)
            {
                this.OnQueryEditorInfoMessage(message);
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
            this.CloseCodeCompletionWindow();

            if (!this.enableIntellisense)
            {
                return;
            }

            this.txtToolTip.Visible = false;
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

                TextEditorHelper.HighlightingError(this.txtEditor, error);
            }
            else
            {
                TextEditorHelper.ClearMarkers(this.txtEditor);

                if (showMessageBox)
                {
                    MessageBox.Show("The scripts is valid.");
                }
            }
        }       

        private void ClearSelection()
        {
            this.SelectionManager.ClearSelection();
        }

        private void editorContexMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.SetContextMenuItemDisplay();
        }

        private void SetContextMenuItemDisplay()
        {
            foreach (ToolStripItem item in this.ContextMenu.Items)
            {
                if (item.Name == "tsmiValidateScripts")
                {
                    bool hasText = this.txtEditor.Text.Trim().Length > 0;
                    item.Visible = hasText;
                }
                else if (item.Name == "tsmiDisableIntellisense")
                {
                    item.Text = $"{(this.enableIntellisense ? "Disable" : "Enable")} Intellisense";
                }
                else if (item.Name == "tsmiUpdateIntellisense")
                {
                    item.Visible = this.enableIntellisense;
                }
            }
        }

        internal void DisposeResources()
        {
            this.CloseCodeCompletionWindow();
        }
    }
}
