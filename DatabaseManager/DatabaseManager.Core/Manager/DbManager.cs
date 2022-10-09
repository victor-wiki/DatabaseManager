using DatabaseConverter.Core;
using DatabaseConverter.Model;
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
                    dbConnection.Open();

                    await this.SetConstrainsEnabled(dbConnection, false);                   

                    transaction = dbConnection.BeginTransaction();

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

            if(dbConnection == null)
            {
                needDispose = true;
                dbConnection = this.dbInterpreter.CreateConnection();
            }           

            IEnumerable<Script> scripts = this.scriptGenerator.SetConstrainsEnabled(enabled);

            foreach (Script script in scripts)
            {
                await this.dbInterpreter.ExecuteNonQueryAsync(dbConnection, script.Content);
            }

            if(needDispose)
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
                    await this.DropDbObjects(connection, schemaInfo.Procedures);
                    await this.DropDbObjects(connection, schemaInfo.Views);
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
                    await this.DropDbObject(obj, connection);

                    names.Add(obj.Name);
                }
            }
        }

        private async Task DropDbObject(DatabaseObject dbObject, DbConnection connection = null)
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

                if (connection != null)
                {
                    await this.dbInterpreter.ExecuteNonQueryAsync(connection, sql);
                }
                else
                {
                    await this.dbInterpreter.ExecuteNonQueryAsync(sql);
                }
            }
        }

        public async Task DropDbObject(DatabaseObject dbObject)
        {
            try
            {
                await this.DropDbObject(dbObject, null);
            }
            catch (Exception ex)
            {
                this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex));
            }
        }

        public async Task Translate(DatabaseType sourceDbType, DatabaseType targetDbType, DatabaseObject dbObject, ConnectionInfo connectionInfo, TranslateHandler translateHandler = null)
        {
            DbInterpreterOption sourceScriptOption = new DbInterpreterOption() { ScriptOutputMode = GenerateScriptOutputMode.None };
            DbInterpreterOption targetScriptOption = new DbInterpreterOption() { ScriptOutputMode = GenerateScriptOutputMode.WriteToString };

            DbConveterInfo source = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(sourceDbType, connectionInfo, sourceScriptOption) };
            DbConveterInfo target = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(targetDbType, new ConnectionInfo(), sourceScriptOption) };

            using (DbConverter dbConverter = new DbConverter(source, target))
            {
                dbConverter.Option.OnlyForTranslate = true;
                dbConverter.Option.GenerateScriptMode = GenerateScriptMode.Schema;
                dbConverter.Option.ExecuteScriptOnTargetServer = false;
                dbConverter.Option.ConvertComputeColumnExpression = true;

                dbConverter.Subscribe(this.observer);

                if (translateHandler != null)
                {
                    dbConverter.OnTranslated += translateHandler;
                }                  

                await dbConverter.Translate(dbObject);
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

        public async Task<DiagnoseResult> Diagnose(DatabaseType databaseType, ConnectionInfo connectionInfo, DiagnoseType diagnoseType)
        {
            DbDiagnosis dbDiagnosis = DbDiagnosis.GetInstance(databaseType, connectionInfo);

            dbDiagnosis.OnFeedback += this.Feedback;

            DiagnoseResult result = await dbDiagnosis.Diagnose(diagnoseType);

            return result;
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
            this.Feedback(FeedbackInfoType.Error, message);
        }
    }
}
