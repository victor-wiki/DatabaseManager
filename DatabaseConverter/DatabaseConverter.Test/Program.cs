using DatabaseConverter.Demo;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System;

namespace DatabaseConverter.Test
{
    class Program
    {
        static ConnectionInfo sqlServerConn = new ConnectionInfo() { Server = @".\sql2019", Database = "Northwind", IntegratedSecurity = true };
        static ConnectionInfo mySqlConn = new ConnectionInfo() { Server = "localhost", Database = "northwind", UserId = "sa", Password = "1234" };
        static ConnectionInfo oracleConn = new ConnectionInfo() { Server = "127.0.0.1/orcl", Database = "Northwind", UserId = "C##northwind", Password = "TEST" };

        static DbInterpreterOption option = new DbInterpreterOption()
        {
            ScriptOutputMode = GenerateScriptOutputMode.WriteToString | GenerateScriptOutputMode.WriteToFile,
            ScriptOutputFolder = "output"
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
            //await ConverterDemoRuner.Run(new ConverterDemo(sqlServerInterpreter, mySqlInterpreter));
            //await ConverterDemoRuner.Run(new ConverterDemo(sqlServerInterpreter, oracleInterpreter));
            await ConverterDemoRuner.Run(new ConverterDemo(mySqlInterpreter, oracleInterpreter));

            Console.WriteLine("OK");
        }
    }
}
