using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;


namespace DatabaseManager.Core
{
    public class DbManager
    {
        private IObserver<FeedbackInfo> observer;
        private DbInterpreter dbInterpreter;
        private DbScriptGenerator scriptGenerator;

        public DbManager()
        {

        }

        public DbManager(DbInterpreter dbInterpreter)
        {
            this.dbInterpreter = dbInterpreter;
            this.scriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(dbInterpreter);
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public async Task ClearData(List<Table> tables = null)
        {
            this.FeedbackInfo("Begin to clear data...");

            if (tables == null)
            {
                tables = await this.dbInterpreter.GetTablesAsync();
            }

            bool failed = false;

            DbTransaction transaction = null;

            try
            {
                this.FeedbackInfo("Disable constrains.");

                DbScriptGenerator scriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.dbInterpreter);

                using (DbConnection dbConnection = this.dbInterpreter.CreateConnection())
                {
                    await dbConnection.OpenAsync();

                    await this.SetConstrainsEnabled(dbConnection, false);

                    transaction = await dbConnection.BeginTransactionAsync();

                    foreach (Table table in tables)
                    {
                        string sql = $"DELETE FROM {this.dbInterpreter.GetQuotedDbObjectNameWithSchema(table)}";

                        this.FeedbackInfo(sql);

                        CommandInfo commandInfo = new CommandInfo()
                        {
                            CommandType = CommandType.Text,
                            CommandText = sql,
                            Transaction = transaction
                        };

                        await this.dbInterpreter.ExecuteNonQueryAsync(dbConnection, commandInfo);
                    }

                    if (!this.dbInterpreter.HasError)
                    {
                        transaction.Commit();
                    }

                    await this.SetConstrainsEnabled(dbConnection, true);
                }
            }
            catch (Exception ex)
            {
                failed = true;
                this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex));

                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception iex)
                    {
                        LogHelper.LogError(iex.Message);
                    }
                }
            }
            finally
            {
                if (failed)
                {
                    this.FeedbackInfo("Enable constrains.");

                    await this.SetConstrainsEnabled(null, true);
                }
            }

            this.FeedbackInfo("End clear data.");
        }

        private async Task SetConstrainsEnabled(DbConnection dbConnection, bool enabled)
        {
            bool needDispose = false;

            if (dbConnection == null)
            {
                needDispose = true;
                dbConnection = this.dbInterpreter.CreateConnection();
            }

            IEnumerable<Script> scripts = this.scriptGenerator.SetConstrainsEnabled(enabled);

            foreach (Script script in scripts)
            {
                await this.dbInterpreter.ExecuteNonQueryAsync(dbConnection, script.Content);
            }

            if (needDispose)
            {
                using (dbConnection) { };
            }
        }

        public async Task EmptyDatabase(DatabaseObjectType databaseObjectType)
        {
            bool sortObjectsByReference = this.dbInterpreter.Option.SortObjectsByReference;
            DatabaseObjectFetchMode fetchMode = this.dbInterpreter.Option.ObjectFetchMode;

            this.dbInterpreter.Option.SortObjectsByReference = true;
            this.dbInterpreter.Option.ObjectFetchMode = DatabaseObjectFetchMode.Simple;

            this.FeedbackInfo("Begin to empty database...");

            SchemaInfo schemaInfo = await this.dbInterpreter.GetSchemaInfoAsync(new SchemaInfoFilter() { DatabaseObjectType = databaseObjectType });

            try
            {
                using (DbConnection connection = this.dbInterpreter.CreateConnection())
                {
                    await this.DropDbObjects(connection, schemaInfo.TableTriggers);
                    await this.DropDbObjects(connection, schemaInfo.Procedures);
                    await this.DropDbObjects(connection, schemaInfo.Views);
                    await this.DropDbObjects(connection, schemaInfo.TableTriggers);
                    await this.DropDbObjects(connection, schemaInfo.TableForeignKeys);
                    await this.DropDbObjects(connection, schemaInfo.Tables);
                    await this.DropDbObjects(connection, schemaInfo.Functions);
                    await this.DropDbObjects(connection, schemaInfo.UserDefinedTypes);
                    await this.DropDbObjects(connection, schemaInfo.Sequences);
                }
            }
            catch (Exception ex)
            {
                this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex));
            }
            finally
            {
                this.dbInterpreter.Option.SortObjectsByReference = sortObjectsByReference;
                this.dbInterpreter.Option.ObjectFetchMode = fetchMode;
            }

            this.FeedbackInfo("End empty database.");
        }

        private async Task DropDbObjects<T>(DbConnection connection, List<T> dbObjects) where T : DatabaseObject
        {
            List<string> names = new List<string>();

            foreach (T obj in dbObjects)
            {
                if (!names.Contains(obj.Name))
                {
                    try
                    {
                        await this.DropDbObject(obj, connection, true);

                        names.Add(obj.Name);
                    }
                    catch (Exception ex)
                    {
                        this.FeedbackError($@"Error occurs when drop ""{obj.Name}"":{ex.Message}", true);

                        continue;
                    }
                }
            }
        }

        private async Task<bool> DropDbObject(DatabaseObject dbObject, DbConnection connection = null, bool continueWhenErrorOccurs = false)
        {
            string typeName = dbObject.GetType().Name;

            this.FeedbackInfo($"Drop {typeName} \"{dbObject.Name}\".");

            Script script = this.scriptGenerator.Drop(dbObject);

            if (script != null && !string.IsNullOrEmpty(script.Content))
            {
                string sql = script.Content;

                if (this.dbInterpreter.ScriptsDelimiter.Length == 1)
                {
                    sql = sql.TrimEnd(this.dbInterpreter.ScriptsDelimiter.ToCharArray());
                }

                var commandInfo = new CommandInfo() { CommandText = sql, ContinueWhenErrorOccurs = continueWhenErrorOccurs };

                if (connection != null)
                {
                    await this.dbInterpreter.ExecuteNonQueryAsync(connection, commandInfo);
                }
                else
                {
                    await this.dbInterpreter.ExecuteNonQueryAsync(commandInfo);
                }

                return true;
            }

            return false;
        }

        public async Task<bool> DropDbObject(DatabaseObject dbObject)
        {
            try
            {
                return await this.DropDbObject(dbObject, null);
            }
            catch (Exception ex)
            {
                this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex));

                return false;
            }
        }

        public bool Backup(BackupSetting setting, ConnectionInfo connectionInfo)
        {
            try
            {
                DbBackup backup = DbBackup.GetInstance(ManagerUtil.GetDatabaseType(setting.DatabaseType));
                backup.Setting = setting;
                backup.ConnectionInfo = connectionInfo;

                this.FeedbackInfo("Begin to backup...");

                string saveFilePath = backup.Backup();

                if (File.Exists(saveFilePath))
                {
                    this.FeedbackInfo($"Database has been backuped to {saveFilePath}.");
                }
                else
                {
                    this.FeedbackInfo($"Database has been backuped.");
                }

                return true;
            }
            catch (Exception ex)
            {
                this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex));
            }

            return false;
        }

        public async Task<TableDiagnoseResult> DiagnoseTable(DatabaseType databaseType, ConnectionInfo connectionInfo, string schema, TableDiagnoseType diagnoseType)
        {
            DbDiagnosis dbDiagnosis = DbDiagnosis.GetInstance(databaseType, connectionInfo);
            dbDiagnosis.Schema = schema;

            dbDiagnosis.OnFeedback += this.Feedback;

            TableDiagnoseResult result = await dbDiagnosis.DiagnoseTable(diagnoseType);

            return result;
        }

        public async Task<List<ScriptDiagnoseResult>> DiagnoseScript(DatabaseType databaseType, ConnectionInfo connectionInfo, string schema, ScriptDiagnoseType diagnoseType)
        {
            DbDiagnosis dbDiagnosis = DbDiagnosis.GetInstance(databaseType, connectionInfo);
            dbDiagnosis.Schema = schema;

            dbDiagnosis.OnFeedback += this.Feedback;

            List<ScriptDiagnoseResult> results = await dbDiagnosis.DiagnoseScript(diagnoseType);

            return results;
        }

        public void Feedback(FeedbackInfoType infoType, string message)
        {
            FeedbackInfo info = new FeedbackInfo() { Owner = this, InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(message) };

            this.Feedback(info);
        }

        public void Feedback(FeedbackInfo info)
        {
            if (this.observer != null)
            {
                FeedbackHelper.Feedback(this.observer, info);
            }
        }

        public void FeedbackInfo(string message)
        {
            this.Feedback(FeedbackInfoType.Info, message);
        }

        public void FeedbackError(string message, bool skipError = false)
        {
            this.Feedback(new FeedbackInfo() { InfoType = FeedbackInfoType.Error, Message = message, IgnoreError = skipError });
        }
    }
}
