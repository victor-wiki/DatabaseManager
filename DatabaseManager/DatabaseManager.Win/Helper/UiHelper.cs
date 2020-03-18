using System;
using System.Drawing;
using System.Windows.Forms;

namespace DatabaseManager.Helper
{
    public class UiHelper
    {
        public static void AppendMessage(RichTextBox textBox, string message, bool isError = false)
        {
            int start = textBox.Text.Length;

            if (start > 0)
            {
                textBox.AppendText(Environment.NewLine);
            }

            textBox.AppendText(message);

            textBox.Select(start, textBox.Text.Length - start);
            textBox.SelectionColor = isError ? Color.Red : Color.Black;

            textBox.SelectionStart = textBox.TextLength;
            textBox.ScrollToCaret();
        }
    }
}
