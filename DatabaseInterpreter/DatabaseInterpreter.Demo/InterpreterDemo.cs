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
        public async Task<SchemaInfo> GetSchemaInfoAsync(SchemaInfoFilter filter)
        {
            return await this.interpreter.GetSchemaInfoAsync(filter);
        } 
        #endregion

        #region Schema Scripts
        public async Task<string> GenerateSchemaScriptsAsync(SchemaInfo schemaInfo)
        {
            SchemaInfoFilter filter = new SchemaInfoFilter()
            {
                TableNames = schemaInfo.Tables.Select(item => item.Name).ToArray(),
                ViewNames = schemaInfo.Views.Select(item => item.Name).ToArray()               
            };

            return this.interpreter.GenerateSchemaScripts(await this.Interpreter.GetSchemaInfoAsync(filter));
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
