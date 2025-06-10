using DatabaseInterpreter.Utility;
using DatabaseManager.Forms;
using DatabaseManager.Helper;
using System;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_QueryResultGrid : UserControl
    {
        private frmFindBox findBox = null;
        private string searchWord = null;
        private bool matchCase = false;
        private bool matchWholeWord = false;
        private bool isHighlighting = false;

        public UC_QueryResultGrid()
        {
            InitializeComponent();

            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, this.dgvData, new object[] { true });
        }

        public void LoadData(DataTable dataTable)
        {
            this.ResetSearch();

            this.dgvData.DataSource = DataGridViewHelper.ConvertDataTable(dataTable);
        }

        public void ClearData()
        {
            this.dgvData.DataSource = null;
        }

        private void ResetSearch()
        {
            this.searchWord = null;
            this.matchCase = false;
            this.matchWholeWord = false;
            this.isHighlighting = false;
        }

        private void tsmiSave_Click(object sender, EventArgs e)
        {
            this.Save();
        }

        private void Save()
        {
            if (this.dlgSave == null)
            {
                this.dlgSave = new SaveFileDialog();
            }

            this.dlgSave.FileName = "";

            DialogResult result = this.dlgSave.ShowDialog();

            if (result == DialogResult.OK)
            {
                DataTableHelper.WriteToFile(this.dgvData.DataSource as DataTable, this.dlgSave.FileName);
            }
        }

        private void dgvData_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Action action = () =>
                {
                    this.SetContextMenuItemVisible();

                    this.contextMenuStrip1.Show(this.dgvData, e.Location);
                };

                if (this.InvokeRequired)
                {
                    this.Invoke(action);
                }
                else
                {
                    action();
                }

                this.Invalidate();
            }
        }

        private void tsmiCopy_Click(object sender, EventArgs e)
        {
            this.Copy(DataGridViewClipboardCopyMode.EnableWithoutHeaderText);
        }

        private void tsmiCopyWithHeader_Click(object sender, EventArgs e)
        {
            this.Copy(DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText);
        }

        private void Copy(DataGridViewClipboardCopyMode mode)
        {
            this.dgvData.ClipboardCopyMode = mode;

            Clipboard.SetDataObject(this.dgvData.GetClipboardContent());
        }

        private void dgvData_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvData.ClearSelection();
        }

        private void dgvData_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void dgvData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewHelper.FormatCell(this.dgvData, e);
        }

        private void SetContextMenuItemVisible()
        {
            int selectedCount = this.dgvData.GetCellCount(DataGridViewElementStates.Selected);

            this.tsmiSave.Visible = this.dgvData.Rows.Count > 0;
            this.tsmiCopy.Visible = selectedCount > 1;
            this.tsmiCopyWithHeader.Visible = selectedCount > 1;
            this.tsmiViewGeometry.Visible = selectedCount == 1 && DataGridViewHelper.IsGeometryValue(this.dgvData);
            this.tsmiCopyContent.Visible = selectedCount == 1;
            this.tsmiShowContent.Visible = selectedCount == 1;
            this.tsmiSetColumnWidthByDefault.Visible = this.dgvData.AutoSizeColumnsMode != DataGridViewAutoSizeColumnsMode.None;
            this.tsmiAutoColumnWidth.Visible = this.dgvData.AutoSizeColumnsMode != DataGridViewAutoSizeColumnsMode.AllCells;
            this.tsmiFindText.Visible = this.dgvData.Rows.Count > 0;
            this.tsmiClearHighlighting.Visible = this.isHighlighting;
        }

        private void tsmiViewGeometry_Click(object sender, EventArgs e)
        {
            DataGridViewHelper.ShowGeometryViewer(this.dgvData);
        }

        private void tsmiCopyContent_Click(object sender, EventArgs e)
        {
            var value = DataGridViewHelper.GetCurrentCellValue(this.dgvData);

            if (!string.IsNullOrEmpty(value))
            {
                Clipboard.SetDataObject(value);
            }
        }

        private void tsmiShowContent_Click(object sender, EventArgs e)
        {
            DataGridViewHelper.ShowCellContent(this.dgvData);
        }

        private void tsmiAutoColumnWidth_Click(object sender, EventArgs e)
        {
            this.SetAutoCoumnWidthMode(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void tsmiSetColumnWidthByDefault_Click(object sender, EventArgs e)
        {
            this.SetAutoCoumnWidthMode(DataGridViewAutoSizeColumnsMode.None);
        }

        private void SetAutoCoumnWidthMode(DataGridViewAutoSizeColumnsMode mode)
        {
            this.dgvData.AutoSizeColumnsMode = mode;                      
        }

        private void dgvData_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                this.FindText();
            }
        }

        private Control FindParentByType(Control control, Type type)
        {
            if (control != null)
            {
                if (control.GetType() == type || (type.Name == nameof(Form) && control is Form))
                {
                    return control;
                }
                else
                {
                    return FindParentByType(control.Parent, type);
                }
            }

            return null;
        }

        private void FindText()
        {
            if (this.findBox == null || this.findBox.IsDisposed)
            {
                this.findBox = new frmFindBox(true);              
                this.findBox.MinimizeBox = true;
                this.findBox.OnFind += this.FindBox_OnFind;               
            }
            else
            {
                this.findBox.WindowState = FormWindowState.Normal;
            }            

            Control topLevelControl = this.TopLevelControl;
            Control parent = this.FindParentByType(this.Parent, typeof(SplitterPanel));

            if (topLevelControl == null)
            {
                topLevelControl = this.dgvData;
            }
            else
            {
                this.findBox.Owner =(Form) topLevelControl;
            }

            if (parent == null)
            {
                parent = this.dgvData;
            }

            Point gridLocation = parent.Location;

            int x = topLevelControl.Location.X + topLevelControl.Width - this.findBox.Width - 30;
            int y = parent.Top + 118;

            this.findBox.StartPosition = FormStartPosition.Manual;
            this.findBox.Location = new System.Drawing.Point(x, y);

            this.findBox.Show();
        }

        private void FindBox_OnFind()
        {
            this.ResetSearch();
            this.dgvData.Invalidate();

            string word = this.findBox.FindWord;

            this.searchWord = word;
            this.matchCase = this.findBox.MatchCase;
            this.matchWholeWord = this.findBox.MatchWholeWord;

            bool found = false;

            if (!string.IsNullOrEmpty(word))
            {
                foreach (DataGridViewRow row in this.dgvData.Rows)
                {
                    if (row.Index >= 0)
                    {
                        foreach (DataGridViewColumn column in this.dgvData.Columns)
                        {
                            if (column is DataGridViewTextBoxColumn c)
                            {
                                if (c.Index >= 0 && c.Visible)
                                {
                                    var cell = row.Cells[c.Index];
                                    string value = cell.Value?.ToString();

                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        var matches = this.MatchWord(value, word);

                                        if (matches != null && matches.Count > 0)
                                        {
                                            this.dgvData.InvalidateCell(cell);
                                            
                                            if(!found)
                                            {
                                                this.dgvData.FirstDisplayedScrollingRowIndex = cell.RowIndex;
                                            }

                                            found = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (!found)
            {
                MessageBox.Show("Not found.");
            }
        }

        private void dgvData_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            int columnIndex = e.ColumnIndex;
            int rowIndex = e.RowIndex;

            if (rowIndex >= 0 && columnIndex >= 0)
            {
                var column = this.dgvData.Columns[columnIndex];

                if (column.Visible && column is DataGridViewTextBoxColumn c)
                {
                    if (!string.IsNullOrEmpty(this.searchWord))
                    {
                        if (e.FormattedValue == null)
                        {
                            return;
                        }

                        string value = e.FormattedValue.ToString();

                        var matches = this.MatchWord(value, this.searchWord);

                        if (matches != null)
                        {
                            e.Handled = true;
                            e.PaintBackground(e.CellBounds, true);

                            foreach (Match match in matches)
                            {
                                int index = match.Index;
                                int length = match.Length;

                                if (index >= 0)
                                {
                                    Rectangle rect = new Rectangle();
                                    rect.Y = e.CellBounds.Y + 2;
                                    rect.Height = e.CellBounds.Height - 5;

                                    string beforeSearchword = value.Substring(0, index);

                                    string searchWord = value.Substring(index, length);

                                    Size s1 = TextRenderer.MeasureText(e.Graphics, beforeSearchword, e.CellStyle.Font, e.CellBounds.Size);
                                    Size s2 = TextRenderer.MeasureText(e.Graphics, searchWord, e.CellStyle.Font, e.CellBounds.Size);

                                    int cellPosX = e.CellBounds.X;

                                    if (s1.Width > 5)
                                    {
                                        rect.X = cellPosX + s1.Width - 5;
                                        rect.Width = s2.Width - 6;
                                    }
                                    else
                                    {
                                        rect.X = cellPosX + 2;
                                        rect.Width = s2.Width - 6;
                                    }

                                    if(rect.X + rect.Width> (cellPosX + column.Width))
                                    {
                                        continue;
                                    }

                                    SolidBrush brush = new SolidBrush(Color.LightPink);

                                    e.Graphics.FillRectangle(brush, rect);

                                    brush.Dispose();

                                    this.isHighlighting = true;
                                }
                            }

                            e.PaintContent(e.CellBounds);
                        }
                    }
                }
            }
        }

        private MatchCollection MatchWord(string value, string word)
        {
            string pattern = word;

            if (this.matchWholeWord)
            {
                pattern = $@"\b({word})\b";
            }

            string regex = pattern;

            RegexOptions option = RegexOptions.Multiline;

            if (!matchCase)
            {
                option = option | RegexOptions.IgnoreCase;
            }

            regex = regex.Replace("[", "\\[").Replace("]", "\\]");

            try
            {
                MatchCollection matches = Regex.Matches(value, regex, option);

                return matches;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void tsmiClearHighlighting_Click(object sender, EventArgs e)
        {
            this.ResetSearch();

            this.dgvData.Invalidate();
        }

        private void tsmiFindText_Click(object sender, EventArgs e)
        {
            this.FindText();
        }
    }
}
