using DatabaseManager.Core;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using DatabaseManager.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_DbObjectContent : UserControl
    {
        private Dictionary<int, Rectangle> dictCloseButtonRectangle = new Dictionary<int, Rectangle>();

        public DataFilterHandler OnDataFilter;

        public UC_DbObjectContent()
        {
            InitializeComponent();

            FormEventCenter.OnSave += this.Save;
        }

        public void ShowContent(DatabaseObjectDisplayInfo info)
        {
            this.Visible = true;

            TabPage page = this.FindTabPage(info);           

            string title = $" {info.Name}  ";

            if (page == null)
            {
                page = new TabPage(title) { };               

                this.tabControl1.TabPages.Insert(0, page);

                this.tabControl1.SelectedTab = page;
            }
            else
            {
                this.tabControl1.SelectedTab = page;
            }

            page.Tag = info;
            page.BackColor = Color.White;

            this.SetTabPageContent(info, page);            
        }

        private void SetTabPageContent(DatabaseObjectDisplayInfo info, TabPage tabPage)
        {
            if (info.DisplayType == DatabaseObjectDisplayType.Script)
            {
                UC_RichTextBox ucRichTextBox = this.GetUcControl<UC_RichTextBox>(tabPage);

                if (ucRichTextBox == null)
                {
                    ucRichTextBox = this.AddControlToTabPage<UC_RichTextBox>(tabPage);
                }               

                ucRichTextBox.Show(info);

                if (!string.IsNullOrEmpty(ucRichTextBox.TextBox.Text))
                {
                    RichTextBoxHelper.Highlighting(ucRichTextBox.TextBox, info.DatabaseType);
                }
            }
            else if (info.DisplayType == DatabaseObjectDisplayType.Data)
            {
                UC_DataViewer dataViewer = this.GetUcControl<UC_DataViewer>(tabPage);               

                if (dataViewer == null)
                {
                    dataViewer = this.AddControlToTabPage<UC_DataViewer>(tabPage);
                    dataViewer.OnDataFilter += this.DataFilter;
                }

                dataViewer.Show(info);
            }
        }

        private void DataFilter(object sender)
        {
            if (this.OnDataFilter != null)
            {
                this.OnDataFilter(sender);
            }
        }

        private T AddControlToTabPage<T>(TabPage tabPage) where T : UserControl
        {
            T control = (T)Activator.CreateInstance(typeof(T));
            control.Dock = DockStyle.Fill;
            tabPage.Controls.Add(control);

            return control;
        }       

        private T GetUcControl<T>(TabPage tabPage) where T : UserControl
        {
            foreach (Control control in tabPage.Controls)
            {
                if (control is T)
                {
                    return control as T;
                }
            }

            return null;
        }

        public TabPage FindTabPage(DatabaseObjectDisplayInfo displayInfo)
        {
            foreach (TabPage page in this.tabControl1.TabPages)
            {
                DatabaseObjectDisplayInfo data = page.Tag as DatabaseObjectDisplayInfo;

                if (data.Name == displayInfo.Name && displayInfo.DatabaseType == data.DatabaseType && displayInfo.DisplayType == data.DisplayType)
                {
                    return page;
                }
            }

            return null;
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= this.tabControl1.TabPages.Count)
            {
                return;
            }           

            Bitmap imgClose = Resources.TabClose;

            int headerCloseButtonSize = imgClose.Size.Width;

            bool isSelected = e.Index == this.tabControl1.SelectedIndex;

            SolidBrush backBrush = new SolidBrush(isSelected ? Color.White : ColorTranslator.FromHtml("#DEE1E6"));

            Rectangle headerRect = tabControl1.GetTabRect(e.Index);           

            e.Graphics.FillRectangle(backBrush, headerRect);

            Font font = new Font(this.Font, isSelected ? FontStyle.Bold : FontStyle.Regular);

            Brush fontBrush = new SolidBrush(Color.Black);

            e.Graphics.DrawString(this.tabControl1.TabPages[e.Index].Text.TrimEnd(), font, fontBrush, headerRect.X, headerRect.Y + 2);

            Rectangle closeButtonRect = new Rectangle(headerRect.X + headerRect.Width - headerCloseButtonSize, headerRect.Y + 2, headerCloseButtonSize, headerCloseButtonSize);

            e.Graphics.DrawImage(imgClose, closeButtonRect);

            if (!this.dictCloseButtonRectangle.ContainsKey(e.Index))
            {
                this.dictCloseButtonRectangle.Add(e.Index, closeButtonRect);
            }
            else
            {
                this.dictCloseButtonRectangle[e.Index] = closeButtonRect;
            }

            e.Graphics.Dispose();
        }

        private int FindTabPageIndex(int x, int y)
        {
            foreach (var kp in this.dictCloseButtonRectangle)
            {
                Rectangle rect = kp.Value;

                if (x >= rect.X && x <= rect.X + rect.Width
                 && y >= rect.Y && y <= rect.Y + rect.Height)
                {
                    return kp.Key;
                }
            }

            return -1;
        }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            int tabPageIndex = this.FindTabPageIndex(e.X, e.Y);

            if (tabPageIndex >= 0)
            {
                this.tabControl1.TabPages.RemoveAt(tabPageIndex);

                this.SetControlVisible();
            }
        }

        private void tabControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < this.tabControl1.TabPages.Count; i++)
                {
                    Rectangle r = this.tabControl1.GetTabRect(i);

                    if (r.Contains(e.Location))
                    {
                        this.tabControl1.SelectedIndex = i;

                        this.SetMenuItemsVisible();

                        this.scriptContentMenu.Show(this, e.Location);

                        break;
                    }
                }
            }
        }

        private void SetMenuItemsVisible()
        {
            this.tsmiCloseOthers.Visible = this.tabControl1.TabPages.Count > 1;
            this.tsmiCloseAll.Visible = this.tabControl1.TabPages.Count > 1;
        }

        private void tsmiClose_Click(object sender, EventArgs e)
        {
            if (this.tabControl1.SelectedIndex >= 0)
            {
                this.tabControl1.TabPages.RemoveAt(this.tabControl1.SelectedIndex);
                this.dictCloseButtonRectangle.Remove(this.tabControl1.SelectedIndex);
            }

            this.SetControlVisible();
        }

        private void tsmiCloseOthers_Click(object sender, EventArgs e)
        {
            this.dictCloseButtonRectangle.Clear();

            int index = this.tabControl1.SelectedIndex;

            for (int i = this.tabControl1.TabPages.Count - 1; i >= index + 1; i--)
            {
                this.tabControl1.TabPages.RemoveAt(i);
            }

            while (this.tabControl1.TabPages.Count > 1)
            {
                this.tabControl1.TabPages.RemoveAt(0);
            }

            this.SetControlVisible();
        }

        private void SetControlVisible()
        {
            this.Visible = this.tabControl1.TabPages.Count > 0;
        }

        private void tsmiCloseAll_Click(object sender, EventArgs e)
        {
            this.tabControl1.TabPages.Clear();
            this.dictCloseButtonRectangle.Clear();

            this.SetControlVisible();
        }

        private void tsmiSave_Click(object sender, EventArgs e)
        {
            this.Save();
        }

        private void Save()
        {
            TabPage tabPage = this.tabControl1.SelectedTab;

            if (tabPage == null)
            {
                return;
            }

            DatabaseObjectDisplayInfo displayInfo = tabPage.Tag as DatabaseObjectDisplayInfo;

            if (displayInfo == null)
            {
                return;
            }

            if (this.dlgSave == null)
            {
                this.dlgSave = new SaveFileDialog();
            }

            this.dlgSave.FileName = tabPage.Text.Trim();

            if (displayInfo.DisplayType == DatabaseObjectDisplayType.Script)
            {
                this.dlgSave.Filter = "sql file|*.sql|txt file|*.txt";
            }
            else if (displayInfo.DisplayType == DatabaseObjectDisplayType.Data)
            {
                this.dlgSave.Filter = "csv file|*.csv|txt file|*.txt";
            }

            DialogResult result = this.dlgSave.ShowDialog();
            if (result == DialogResult.OK)
            {
                IDbObjContentDisplayer control = this.GetUcControlInterface(tabPage);

                if (control != null)
                {
                    control.Save(this.dlgSave.FileName);
                }
            }
        }

        private IDbObjContentDisplayer GetUcControlInterface(TabPage tabPage)
        {
            foreach (Control control in tabPage.Controls)
            {
                if (control is IDbObjContentDisplayer)
                {
                    return control as IDbObjContentDisplayer;
                }
            }

            return null;
        }
    }
}
