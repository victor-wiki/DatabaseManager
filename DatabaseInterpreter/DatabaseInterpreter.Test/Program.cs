using DatabaseInterpreter.Core;
using DatabaseInterpreter.Demo;
using DatabaseInterpreter.Model;
using System;

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
            TreatBytesAsNullForReading = true,
            TreatBytesAsNullForExecuting = true
        };

        static SqlServerInterpreter sqlServerInterpreter = new SqlServerInterpreter(sqlServerConn, option);
        static MySqlInterpreter mySqlInterpreter = new MySqlInterpreter(mySqlConn, option);
        static OracleInterpreter oracleInterpreter = new OracleInterpreter(oracleConn, option);

        static void Main(string[] args)
        {
            RunDemo();
;
            Console.ReadLine();
        }

        static async void RunDemo()
        {
            SchemaInfoFilter filter = new SchemaInfoFilter();

            filter.DatabaseObjectType =
                 DatabaseObjectType.UserDefinedType
                 | DatabaseObjectType.Function
                 | DatabaseObjectType.Table
                 | DatabaseObjectType.View
                 | DatabaseObjectType.Procedure
                 | DatabaseObjectType.TableColumn
                 | DatabaseObjectType.TablePrimaryKey
                 | DatabaseObjectType.TableForeignKey
                 | DatabaseObjectType.TableIndex
                 | DatabaseObjectType.TableConstraint
                 | DatabaseObjectType.TableTrigger
                 | DatabaseObjectType.Sequence;

            await InterpreterDemoRuner.Run(new InterpreterDemo(sqlServerInterpreter), filter);
            await InterpreterDemoRuner.Run(new InterpreterDemo(mySqlInterpreter), filter);
            await InterpreterDemoRuner.Run(new InterpreterDemo(oracleInterpreter), filter);

            Console.WriteLine("OK");
        }
    }
}
