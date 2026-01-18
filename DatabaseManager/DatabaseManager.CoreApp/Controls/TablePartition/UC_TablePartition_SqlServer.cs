using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_TablePartition_SqlServer : UserControl, ITablePartitionControl
    {
        private SqlServerInterpreter dbInterpreter;
        private Table table;

        public UC_TablePartition_SqlServer()
        {
            InitializeComponent();
        }     

        public async Task LoadData(DbInterpreter dbInterpreter, Table table)
        {
            this.dbInterpreter = dbInterpreter as SqlServerInterpreter;
            this.table = table;

            using(var connection = this.dbInterpreter.CreateConnection())
            {
                var scheme = await this.dbInterpreter.GetPartitionSchemeByTable(connection, table, true);

                this.txtSchemeName.Text = scheme.SchemeName;
                this.txtColumnName.Text = scheme.ColumnName;         

                this.txtFunctionName.Text = scheme.FunctionName;

                this.txtFilegroups.Text = string.Join(Environment.NewLine, scheme.Filegroups);

                var scriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.dbInterpreter) as SqlServerScriptGenerator;

                this.txtSchemeDefinition.Text = scriptGenerator.CreatePartitionScheme(scheme).Content.TrimEnd(';',' ');

                var partitionFunction = (await this.dbInterpreter.GetPartitionFunctions([scheme.FunctionName], true)).FirstOrDefault();

                this.txtFunctionDefinition.Text = scriptGenerator.CreatePartitionFunction(partitionFunction).Content.TrimEnd(';', ' ');
            }        
        }

        public async Task Reload()
        {
            await this.LoadData(this.dbInterpreter, this.table);
        }
    }
}
