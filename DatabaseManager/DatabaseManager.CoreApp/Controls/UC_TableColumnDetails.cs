using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_TableColumnDetails : UserControl
    {
        private IEnumerable<TableColumn> tableColumns;

        public UC_TableColumnDetails()
        {
            InitializeComponent();
        }

        public void LoadData(Table table, IEnumerable<TableColumn> columns, TablePrimaryKey primaryKey, IEnumerable<TableForeignKey> foreignKeys)
        {
            this.tableColumns = columns;

            foreach (TableColumn column in columns)
            {
                bool isPrimaryKey = primaryKey != null && primaryKey.TableName == table.Name && primaryKey.Columns.Any(item => item.ColumnName == column.Name);
                TableForeignKey foreignKey = foreignKeys?.FirstOrDefault(item => item.TableName == column.TableName && item.Columns.Any(t => t.ColumnName == column.Name));
                bool isForeignKey = foreignKey != null;

                ListViewItem item = new ListViewItem(" ");
                item.ImageIndex = isPrimaryKey ? 0 : (isForeignKey ? 1 : -1);
                item.Name = column.Name;

                item.SubItems.Add(column.Name);
                item.Tag = foreignKey;

                this.lvTableColumns.Items.Add(item);
            }
        }

        public void HighlightForeignKeyColumns(TableForeignKey foreignKey)
        {
            foreach (ListViewItem item in this.lvTableColumns.Items)
            {
                TableForeignKey fk = item.Tag as TableForeignKey;

                if (fk != null)
                {
                    bool isMatched = false;

                    if (fk.Name == foreignKey.Name && fk.Name != null)
                    {
                        isMatched = true;
                    }
                    else if (SchemaInfoHelper.IsForeignKeyColumnsEquals(fk.Columns, foreignKey.Columns))
                    {
                        isMatched = true;
                    }

                    if (isMatched)
                    {
                        item.BackColor = Color.LightGray;
                    }
                }
            }
        }

        private void lvTableColumns_MouseMove(object sender, EventArgs e)
        {
            Point pt = this.lvTableColumns.PointToClient(Control.MousePosition);
            ListViewHitTestInfo hitTest = this.lvTableColumns.HitTest(pt);

            if (hitTest.Item != null)
            {
                string columnName = hitTest.Item.Name;

                var column = this.tableColumns.FirstOrDefault(item => item.Name == columnName);

                this.toolTip1.Show(column?.Comment, this.lvTableColumns, new Point(pt.X + 20, pt.Y + 10));
            }
            else
            {
                this.toolTip1.Hide(this.lvTableColumns);
            }
        }

        private void lvTableColumns_MouseLeave(object sender, EventArgs e)
        {
            this.toolTip1.Hide(this.lvTableColumns);
        }
    }
}
