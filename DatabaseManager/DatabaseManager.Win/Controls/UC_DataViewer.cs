using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseInterpreter.Model;
using DatabaseManager.Model;
using DatabaseInterpreter.Core;
using DatabaseManager.Core;

namespace DatabaseManager.Controls
{
    public partial class UC_DataViewer : UserControl, IDbObjContentDisplayer
    {
        private DatabaseObjectDisplayInfo displayInfo;

        public UC_DataViewer()
        {
            InitializeComponent();

            this.pagination.PageSize = 50;

            this.dgvData.AutoGenerateColumns = true;
        }

        public void Show(DatabaseObjectDisplayInfo displayInfo)
        {
            this.LoadData(displayInfo);
        }

        private async void LoadData(DatabaseObjectDisplayInfo displayInfo, long pageNum=1)
        {
            this.displayInfo = displayInfo;

            Table table = displayInfo.DatabaseObject as Table;

            int pageSize = this.pagination.PageSize;

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(displayInfo.DatabaseType, displayInfo.ConnectionInfo, new DbInterpreterOption());

            (long Total, DataTable Data) result = await dbInterpreter.GetPagedDataTableAsync(table, "", pageSize, pageNum);

            this.pagination.TotalCount = result.Total;

            this.dgvData.DataSource = result.Data;          
        }

        private void pagination_OnPageNumberChanged(long pageNum)
        {
            this.LoadData(this.displayInfo, pageNum);
        }

        public void Save(string filePath)
        {
            DataTableHelper.WriteToFile(this.dgvData.DataSource as DataTable, filePath);
        }
    }
}
