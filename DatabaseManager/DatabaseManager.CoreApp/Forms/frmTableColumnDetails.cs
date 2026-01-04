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
    public partial class frmTableColumnDetails : Form
    {
        public frmTableColumnDetails(Table table, IEnumerable<TableColumn> columns, TablePrimaryKey primaryKey, IEnumerable<TableForeignKey> foreignKeys)
        {
            InitializeComponent();           

            this.Text = table.Name;

            this.uC_TableColumnDetails1.LoadData(table, columns, primaryKey, foreignKeys);
        }
    }
}
