using System;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public class CustomDataGridView: DataGridView
    {
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);
            }
            catch (Exception ex)
            {
                this.Invalidate();
            }
        }
    }
}
