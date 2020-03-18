using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseManager.Model;
using DatabaseManager.Properties;
using System.IO;

namespace DatabaseManager.Controls
{
    public partial class UC_DbObjectContent : UserControl
    {
        private Dictionary<int, Rectangle> dictCloseButtonRectangle = new Dictionary<int, Rectangle>();

        public UC_DbObjectContent()
        {
            InitializeComponent();
        }

        public void ShowContent(DatabaseObjectDisplayInfo content)
        {
            TabPage page = this.FindTabPage(content);

            string title = $" {content.Name}  ";
            if (page == null)
            {
                page = new TabPage(title);
                page.Tag = content;

                RichTextBox textbox = new RichTextBox();
                textbox.Dock = DockStyle.Fill;
                textbox.Text = content.Content;

                page.Controls.Add(textbox);

                this.tabControl1.TabPages.Insert(0, page);

                this.tabControl1.SelectedTab = page;
            }
            else
            {
                this.GetRichTextBox(page).Text = title;
                this.tabControl1.SelectedTab = page;
            }

            page.BackColor = Color.Transparent;
        }

        private RichTextBox GetRichTextBox(TabPage tabPage)
        {
            foreach (Control control in tabPage.Controls)
            {
                if (control is RichTextBox)
                {
                    return control as RichTextBox;
                }
            }
            return null;
        }

        public TabPage FindTabPage(DatabaseObjectDisplayInfo content)
        {
            foreach (TabPage page in this.tabControl1.TabPages)
            {
                DatabaseObjectDisplayInfo data = page.Tag as DatabaseObjectDisplayInfo;

                if (page.Text == content.Name && content.DatabaseType == data.DatabaseType && content.DisplayType == data.DisplayType)
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
        }

        private void tsmiCloseAll_Click(object sender, EventArgs e)
        {
            this.tabControl1.TabPages.Clear();
            this.dictCloseButtonRectangle.Clear();
        }

        private void tsmiSaveScript_Click(object sender, EventArgs e)
        {
            if (this.dlgSaveScripts == null)
            {
                this.dlgSaveScripts = new SaveFileDialog();
            }

            TabPage tabPage = this.tabControl1.SelectedTab;

            this.dlgSaveScripts.FileName = tabPage.Text.Trim();

            DialogResult result = this.dlgSaveScripts.ShowDialog();
            if (result == DialogResult.OK)
            {
                File.WriteAllText(this.dlgSaveScripts.FileName, this.GetRichTextBox(tabPage).Text);
            }
        }
    }
}
