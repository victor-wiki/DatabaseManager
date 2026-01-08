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

namespace DatabaseManager.Forms
{
    public partial class frmTableColumnRelation : Form
    {
        private TableForeignKey foreignKey;

        public frmTableColumnRelation(TableForeignKey foreignKey)
        {
            InitializeComponent();

            this.foreignKey = foreignKey;
        }

        private void frmTableColumnsRelation_Load(object sender, EventArgs e)
        {

        }

        public void LoadTableColumns(bool isSource, Table table, IEnumerable<TableColumn> columns, TablePrimaryKey primaryKey, IEnumerable<TableForeignKey> foreignKeys)
        {
            if (isSource)
            {
                this.lblFKTableName.Text = table.Name;
                this.ucFKTableColumns.LoadData(table, columns, primaryKey, foreignKeys);
            }
            else
            {
                this.lblPKTableName.Text = table.Name;
                this.ucPKTableColumns.LoadData(table, columns, primaryKey, foreignKeys);

                if (this.foreignKey != null)
                {
                    this.ucFKTableColumns.HighlightForeignKeyColumns(foreignKey);
                }
            }
        }
    }
}
