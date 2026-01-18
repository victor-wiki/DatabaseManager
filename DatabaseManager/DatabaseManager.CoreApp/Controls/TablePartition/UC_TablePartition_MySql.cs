using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using SqlCodeEditor.Document;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_TablePartition_MySql : UserControl, ITablePartitionControl
    {
        private MySqlInterpreter dbInterpreter;
        private Table table;

        public UC_TablePartition_MySql()
        {
            InitializeComponent();

            var defaultHighlightingStrategy = (this.txtDefinition.Document.HighlightingStrategy as DefaultHighlightingStrategy);
            var oldColor = defaultHighlightingStrategy.DigitColor;

            defaultHighlightingStrategy.DigitColor = new HighlightColor(nameof(SystemColors.ControlText), oldColor.Bold, oldColor.Italic);
        }

        public async Task LoadData(DbInterpreter dbInterpreter, Table table)
        {
            this.dbInterpreter = dbInterpreter as MySqlInterpreter;
            this.table = table;

            using (var connection = this.dbInterpreter.CreateConnection())
            {
                var summary = await this.dbInterpreter.GetPartitionSummary(connection, table, true);

                if (summary != null)
                {
                    var scriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.dbInterpreter) as MySqlScriptGenerator;

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
