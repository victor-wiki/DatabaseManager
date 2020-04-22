using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_DataGridView : UserControl
    {
        public UC_DataGridView()
        {
            InitializeComponent();
        }

        public void LoadData(DataTable dataTable)
        {
            this.dgvData.DataSource = dataTable;
        }
    }
}
