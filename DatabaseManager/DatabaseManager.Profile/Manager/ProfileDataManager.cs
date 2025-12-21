using DatabaseConverter.Core;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseManager.Profile.Manager
{
    internal class ProfileDataManager
    {

        internal static async Task<bool> KeepUserData(string sourceDbFilePath, string targetDbFilePath)
        {
            try
            {
                using (var connection = ProfileBaseManager.CreateDbConnection(targetDbFilePath) as SqliteConnection)
                {
                    connection.Open();

                    string sql = "DELETE FROM PersonalSetting";

                    var cmd = connection.CreateCommand();

                    cmd.CommandText = sql;

                    await cmd.ExecuteNonQueryAsync();
                }

                DbInterpreterOption sourceScriptOption = new DbInterpreterOption()
                {
                    ScriptOutputMode = GenerateScriptOutputMode.None,
                    SortObjectsByReference = true,
                    GetTableAllObjects = true,
                    ThrowExceptionWhenErrorOccurs = true
                };

                DbInterpreterOption targetScriptOption = new DbInterpreterOption()
                {
                    ScriptOutputMode = GenerateScriptOutputMode.WriteToString,
                    ThrowExceptionWhenErrorOccurs = true
                };

                DbConveterInfo source = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(DatabaseType.Sqlite, new ConnectionInfo() { Database = sourceDbFilePath }, sourceScriptOption) };
                DbConveterInfo target = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(DatabaseType.Sqlite, new ConnectionInfo() { Database = targetDbFilePath }, targetScriptOption) };

                using (DbConverter  dbConverter= new DbConverter(source, target))
                {
                    var option = dbConverter.Option;

                    option.GenerateScriptMode = GenerateScriptMode.Data;                 
                    option.ExecuteScriptOnTargetServer = true;
                    option.UseTransaction = true;                
                    option.SplitScriptsToExecute = true;

                    SchemaInfo schemaInfo = new SchemaInfo();

                    schemaInfo.Tables = new List<Table>()
                    {
                        new Table(){ Name = "Account" },
                        new Table(){ Name = "Connection" },
                        new Table(){ Name = "FileConnection" },
                        new Table(){ Name = "DatabaseVisibility" },
                        new Table(){ Name = "PersonalSetting" }
                    };

                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                    var token = cancellationTokenSource.Token;

                    DbConvertResult result = await dbConverter.Convert(token, schemaInfo);              
                    
                    if(result.InfoType == DbConvertResultInfoType.Error)
                    {
                        throw new Exception(result.Message);
                    }                  
                }
            }            
            catch (Exception ex)
            {
                LogHelper.LogError(ExceptionHelper.GetExceptionDetails(ex));

                throw ex;
            }
            finally
            {
                GC.Collect();
                SqliteConnection.ClearAllPools();
            }

            return true;
        }       
    }
}
