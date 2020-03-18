using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Demo;
using System;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Test
{
    class Program
    {
        static ConnectionInfo sqlServerConn = new ConnectionInfo() { Server = @".\sql2019", Database = "Northwind", IntegratedSecurity = true };
        static ConnectionInfo mySqlConn = new ConnectionInfo() { Server = "localhost", Database = "northwind", UserId = "sa", Password = "1234" };
        static ConnectionInfo oracleConn = new ConnectionInfo() { Server = "127.0.0.1/orcl", Database = "Northwind", UserId = "C##northwind", Password = "TEST" };

        static DbInterpreterOption option = new DbInterpreterOption()
        {
            ScriptOutputMode = GenerateScriptOutputMode.WriteToString | GenerateScriptOutputMode.WriteToFile,
            ScriptOutputFolder = "output",
            GetAllObjectsIfNotSpecified = true
        };

        static SqlServerInterpreter sqlServerInterpreter = new SqlServerInterpreter(sqlServerConn, option);
        static MySqlInterpreter mySqlInterpreter = new MySqlInterpreter(mySqlConn, option);
        static OracleInterpreter oracleInterpreter = new OracleInterpreter(oracleConn, option);

        static void Main(string[] args)
        {
            RunDemo();

            Console.ReadLine();
        }

        static async void RunDemo()
        {
            await InterpreterDemoRuner.Run(new InterpreterDemo(sqlServerInterpreter), new SelectionInfo() { });
            //await InterpreterDemoRuner.Run(new InterpreterDemo(mySqlInterpreter), new SelectionInfo() { });
            //await InterpreterDemoRuner.Run(new InterpreterDemo(oracleInterpreter), new SelectionInfo() { });

            Console.WriteLine("OK");
        }
    }
}
