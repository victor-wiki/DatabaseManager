using DatabaseManager.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class DraggableDataGridView : DataGridView
    {
        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown;
        private int rowIndexOfItemUnderMouseToDrop;

        public DraggableDataGridView()
        {
            InitializeComponent();
        }

        private void dgvData_DragDrop(object sender, DragEventArgs e)
        {
            Point clientPoint = this.PointToClient(new Point(e.X, e.Y));

            this.rowIndexOfItemUnderMouseToDrop = this.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            if (this.rowIndexOfItemUnderMouseToDrop == -1)
            {
                return;
            }

            if (this.rowIndexFromMouseDown >= 0 && this.rowIndexOfItemUnderMouseToDrop < this.Rows.Count)
            {
                if (this.Rows[this.rowIndexOfItemUnderMouseToDrop].IsNewRow)
                {
                    return;
                }
            }

            if (e.Effect == DragDropEffects.Move)
            {
                DataGridViewRow rowToMove = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;

                if (rowToMove.Index >= 0)
                {
                    this.Rows.RemoveAt(this.rowIndexFromMouseDown);
                    this.Rows.Insert(this.rowIndexOfItemUnderMouseToDrop, rowToMove);                 
                }
            }
        }

        private void dgvData_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void dgvData_MouseDown(object sender, MouseEventArgs e)
        {
            var hit = this.HitTest(e.X, e.Y);
            this.rowIndexFromMouseDown = hit.RowIndex;

            if (hit.Type == DataGridViewHitTestType.RowHeader && this.rowIndexFromMouseDown != -1)
            {
                Size dragSize = SystemInformation.DragSize;

                this.dragBoxFromMouseDown = new Rectangle(
                          new Point(
                            e.X - (dragSize.Width / 2),
                            e.Y - (dragSize.Height / 2)),
                      dragSize);
            }
            else
            {
                this.dragBoxFromMouseDown = Rectangle.Empty;
            }
        }

        private void dgvData_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if (this.dragBoxFromMouseDown != Rectangle.Empty &&
                !this.dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    DragDropEffects dropEffect = this.DoDragDrop(
                          this.Rows[this.rowIndexFromMouseDown],
                          DragDropEffects.Move);
                }
            }
        }
    }
}
