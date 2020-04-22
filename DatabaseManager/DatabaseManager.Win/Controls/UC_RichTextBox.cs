using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DatabaseManager.Core;
using DatabaseManager.Model;

namespace DatabaseManager.Controls
{
    public partial class UC_RichTextBox : UserControl, IDbObjContentDisplayer
    {
        public UC_RichTextBox()
        {
            InitializeComponent();
        }

        public RichTextBox TextBox => this.richTextBox1;

        public void Show(DatabaseObjectDisplayInfo displayInfo)
        {
            this.richTextBox1.Clear();

            this.richTextBox1.AppendText(displayInfo.Content);
        }

        public void Save(string filePath)
        {
            File.WriteAllText(filePath, this.richTextBox1.Text);
        }

        private void tsmiCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(this.richTextBox1.SelectedText);
        }

        private void richTextBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.tsmiCopy.Enabled = this.richTextBox1.SelectionLength > 0;

                this.contextMenuStrip1.Show(this.richTextBox1, e.Location);
            }
        }
    }
}
