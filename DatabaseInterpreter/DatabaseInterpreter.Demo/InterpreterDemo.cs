using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Demo
{
    public class InterpreterDemo
    {
        private DbInterpreter interpreter;
        public DbInterpreter Interpreter => interpreter;

        public InterpreterDemo(DbInterpreter dbInterpreter)
        {
            this.interpreter = dbInterpreter;
        }

        #region SchemaInfo
        public async Task<SchemaInfo> GetSchemaInfoAsync(SelectionInfo selectionInfo)
        {
            return await this.interpreter.GetSchemaInfoAsync(selectionInfo);
        } 
        #endregion

        #region Schema Scripts
        public async Task<string> GenerateSchemaScriptsAsync(SchemaInfo schemaInfo)
        {
            SelectionInfo selectionInfo = new SelectionInfo()
            {
                TableNames = schemaInfo.Tables.Select(item => item.Name).ToArray(),
                ViewNames = schemaInfo.Views.Select(item => item.Name).ToArray(),
                TriggerNames = schemaInfo.Triggers.Select(item => item.Name).ToArray()
            };

            return this.interpreter.GenerateSchemaScripts(await this.Interpreter.GetSchemaInfoAsync(selectionInfo));
        }
        #endregion

        #region Data Scripts
        public async Task<string> GenerateDataScriptsAsync(SchemaInfo schemaInfo)
        {
            return await this.interpreter.GenerateDataScriptsAsync(schemaInfo);
        }
        #endregion
    }
}
