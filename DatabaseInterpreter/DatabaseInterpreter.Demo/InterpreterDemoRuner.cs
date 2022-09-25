using DatabaseInterpreter.Model;
using System;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Demo
{
    public class InterpreterDemoRuner
    {
        public static async Task Run(InterpreterDemo demo, SchemaInfoFilter filter)
        {
            SchemaInfo schemaInfo = await demo.GetSchemaInfoAsync(filter);

            OutputHelper.Output(FormatName(demo, "GetSchemaInfoAsync"), schemaInfo, true);

            string schemaScript = await demo.GenerateSchemaScriptsAsync(schemaInfo);

            Console.WriteLine("Schema scripts:");
            Console.WriteLine(schemaScript);

            Console.WriteLine();
            Console.WriteLine("Data scripts:");           

            string dataScript = await demo.GenerateDataScriptsAsync(schemaInfo);

            Console.WriteLine(dataScript);
        }     
        
        private static string FormatName(InterpreterDemo demo, string name)
        {
            return $"{demo.Interpreter.GetType().Name}_{name}";
        }        
    }
}
