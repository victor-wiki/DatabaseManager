using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_TablePartition_Oracle : UserControl, ITablePartitionControl
    {
        private OracleInterpreter dbInterpreter;
        private Table table;

        public UC_TablePartition_Oracle()
        {
            InitializeComponent();
        }

        public async Task LoadData(DbInterpreter dbInterpreter, Table table)
        {
            this.dbInterpreter = dbInterpreter as OracleInterpreter;
            this.table = table;

            using (var connection = this.dbInterpreter.CreateConnection())
            {
                var summary = await this.dbInterpreter.GetPartitionSummary(connection, table, true);

                if (summary != null)
                {
                    var scriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.dbInterpreter) as OracleScriptGenerator;

                    this.txtDefinition.Text = scriptGenerator.CreateTablePartition(summary).Content.TrimEnd(';', ' ', '\r', '\n');
                }
            }
        }

        public async Task Reload()
        {
            await this.LoadData(this.dbInterpreter, this.table);
        }
    }
}
