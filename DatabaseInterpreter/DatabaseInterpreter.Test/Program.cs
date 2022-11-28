using DatabaseInterpreter.Core;
using DatabaseInterpreter.Demo;
using DatabaseInterpreter.Model;
using System;

namespace DatabaseInterpreter.Test
{
    class Program
    {
        static ConnectionInfo sqlServerConn = new ConnectionInfo() { Server = @"localhost", Database = "Northwind", IntegratedSecurity = true };
        static ConnectionInfo mySqlConn = new ConnectionInfo() { Server = "localhost", Database = "northwind", UserId = "root", Password = "1234456" };
        static ConnectionInfo oracleConn = new ConnectionInfo() { Server = "127.0.0.1/orcl", Database = "test", UserId = "test", Password = "123456" };
        static ConnectionInfo postgresConn = new ConnectionInfo() { Server = "localhost", Database = "test", UserId = "postgres", Password = "123456", Port = "5432" };

        static DbInterpreterOption option = new DbInterpreterOption()
        {
            ScriptOutputMode = GenerateScriptOutputMode.WriteToString | GenerateScriptOutputMode.WriteToFile,
            ScriptOutputFolder = "output",
            TreatBytesAsNullForReading = true,
            TreatBytesAsNullForExecuting = true
        };

        static SqlServerInterpreter sqlServerInterpreter = new SqlServerInterpreter(sqlServerConn, option);
        static MySqlInterpreter mySqlInterpreter = new MySqlInterpreter(mySqlConn, option);
        static OracleInterpreter oracleInterpreter = new OracleInterpreter(oracleConn, option);
        static PostgresInterpreter postgresInterpreter = new PostgresInterpreter(postgresConn, option);

        static void Main(string[] args)
        {
            RunDemo();

            //TestPostgresDependency();
            //TestOracleDependency();
            
            Console.ReadLine();
        }

        static async void RunDemo()
        {
            SchemaInfoFilter filter = new SchemaInfoFilter();

            filter.DatabaseObjectType =
                 DatabaseObjectType.Type
                 | DatabaseObjectType.Function
                 | DatabaseObjectType.Table
                 | DatabaseObjectType.View
                 | DatabaseObjectType.Procedure
                 | DatabaseObjectType.Column
                 | DatabaseObjectType.PrimaryKey
                 | DatabaseObjectType.ForeignKey
                 | DatabaseObjectType.Index
                 | DatabaseObjectType.Constraint
                 | DatabaseObjectType.Trigger
                 | DatabaseObjectType.Sequence;

            await InterpreterDemoRuner.Run(new InterpreterDemo(sqlServerInterpreter), filter);
            await InterpreterDemoRuner.Run(new InterpreterDemo(mySqlInterpreter), filter);
            await InterpreterDemoRuner.Run(new InterpreterDemo(oracleInterpreter), filter);

            Console.WriteLine("OK");
        }

        static async void TestPostgresDependency()
        {
            SchemaInfoFilter filter = new SchemaInfoFilter();
            //filter.ViewNames = new string[] { "" };

            var viewTableUsages = await postgresInterpreter.GetViewTableUsages(filter);

            var viewColumnUsages = await postgresInterpreter.GetViewColumnUsages(filter);           
        }

        static async void TestOracleDependency()
        {
            SchemaInfoFilter filter = new SchemaInfoFilter();
            //filter.ViewNames = new string[] { "" };  

            var viewTableUsages = await oracleInterpreter.GetViewTableUsages(filter, true);

            filter.DatabaseObjectType = DatabaseObjectType.Procedure;
            //filter.ProcedureNames = new string[] { "" };

            var routineScriptUsages = await oracleInterpreter.GetRoutineScriptUsages(filter);
        }
    }
}
