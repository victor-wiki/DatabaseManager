using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using Microsoft.Msagl.Drawing;
using SqlCodeEditor.Document;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_TablePartition_Postgres : UserControl, ITablePartitionControl
    {
        private PostgresInterpreter dbInterpreter;
        private Table table;

        public UC_TablePartition_Postgres()
        {
            InitializeComponent();

            var defaultHighlightingStrategy = (this.txtInheritedTables.Document.HighlightingStrategy as DefaultHighlightingStrategy);
            var oldColor = defaultHighlightingStrategy.DigitColor;

            defaultHighlightingStrategy.DigitColor = new HighlightColor(nameof(SystemColors.ControlText), oldColor.Bold, oldColor.Italic);
        }

        public async Task LoadData(DbInterpreter dbInterpreter, Table table)
        {
            this.dbInterpreter = dbInterpreter as PostgresInterpreter;
            this.table = table;

            using (var connection = this.dbInterpreter.CreateConnection())
            {
                var scriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.dbInterpreter) as PostgresScriptGenerator;

                var summary = await this.dbInterpreter.GetPartitionSummary(connection, table, true);

                if (summary != null)
                {
                    this.txtDefinition.Text = scriptGenerator.CreateTablePartition(summary).Content.TrimEnd(';', ' ');
                }

                var partitions = await this.dbInterpreter.GetPartitionInfos(table);

                StringBuilder sb = new StringBuilder();

                foreach(var partition in partitions)
                {
                    sb.AppendLine(scriptGenerator.CreateInheritedTable(partition).Content);
                    sb.AppendLine();
                }

                this.txtInheritedTables.Text = sb.ToString().TrimEnd();
            }
        }

        public async Task Reload()
        {
            await this.LoadData(this.dbInterpreter, this.table);
        }
    }
}
