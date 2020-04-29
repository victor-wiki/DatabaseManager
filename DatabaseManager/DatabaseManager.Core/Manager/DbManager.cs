using DatabaseConverter.Core;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace DatabaseManager.Core
{
    public class DbManager
    {
        private IObserver<FeedbackInfo> observer;
        private DbInterpreter dbInterpreter;

        public DbManager()
        {

        }

        public DbManager(DbInterpreter dbInterpreter)
        {
            this.dbInterpreter = dbInterpreter;
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

                using (DbConnection dbConnection = this.dbInterpreter.CreateConnection())
                {
                    dbConnection.Open();

                    await this.dbInterpreter.SetConstrainsEnabled(dbConnection, false);

                    transaction = dbConnection.BeginTransaction();

                    foreach (Table table in tables)
                    {
                        string sql = $"DELETE FROM {this.dbInterpreter.GetQuotedObjectName(table)}";

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

                    await this.dbInterpreter.SetConstrainsEnabled(dbConnection, true);
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
                    }
                }
            }
            finally
            {
                if (failed)
                {
                    this.FeedbackInfo("Enable constrains.");

                    await this.dbInterpreter.SetConstrainsEnabled(true);
                }
            }

            this.FeedbackInfo("End clear data.");
        }

        public async Task EmptyDatabase(DatabaseObjectType databaseObjectType)
        {
            bool sortObjectsByReference = this.dbInterpreter.Option.SortObjectsByReference;
            DatabaseObjectFetchMode fetchMode = this.dbInterpreter.Option.ObjectFetchMode;

            this.dbInterpreter.Option.SortObjectsByReference = true;
            this.dbInterpreter.Option.ObjectFetchMode = DatabaseObjectFetchMode.Details;

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
                    this.FeedbackInfo($"Drop {obj.GetType().Name} \"{obj.Name}\".");

                    await this.dbInterpreter.Drop(connection, obj);

                    names.Add(obj.Name);
                }
            }
        }

        public async Task Translate(DatabaseType sourceDbType, DatabaseType targetDbType, DatabaseObject dbObject, ConnectionInfo connectionInfo, TranslateHandler translateHandler = null)
        {
            DbInterpreterOption sourceScriptOption = new DbInterpreterOption() { ScriptOutputMode = GenerateScriptOutputMode.None };
            DbInterpreterOption targetScriptOption = new DbInterpreterOption() { ScriptOutputMode = GenerateScriptOutputMode.WriteToString };

            targetScriptOption.TableScriptsGenerateOption.GenerateIdentity = true;

            DbConveterInfo source = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(sourceDbType, connectionInfo, sourceScriptOption) };
            DbConveterInfo target = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(targetDbType, new ConnectionInfo(), sourceScriptOption) };

            using (DbConverter dbConverter = new DbConverter(source, target))
            {
                dbConverter.Option.OnlyForTranslate = true;
                dbConverter.Option.GenerateScriptMode = GenerateScriptMode.Schema;
                dbConverter.Option.ExecuteScriptOnTargetServer = false;

                dbConverter.Subscribe(this.observer);

                if (translateHandler != null)
                {
                    dbConverter.OnTranslated += translateHandler;
                }

                if (targetDbType == DatabaseType.SqlServer)
                {
                    target.DbOwner = "dbo";
                }

                SchemaInfo schemaInfo = new SchemaInfo();

                if (dbObject is Table)
                {
                    schemaInfo.Tables.Add(dbObject as Table);
                }
                else if (dbObject is View)
                {
                    schemaInfo.Views.Add(dbObject as DatabaseInterpreter.Model.View);
                }
                else if (dbObject is Function)
                {
                    schemaInfo.Functions.Add(dbObject as Function);
                }
                else if (dbObject is Procedure)
                {
                    schemaInfo.Procedures.Add(dbObject as Procedure);
                }
                else if (dbObject is TableTrigger)
                {
                    schemaInfo.TableTriggers.Add(dbObject as TableTrigger);
                }

                await dbConverter.Convert(schemaInfo);
            }
        }

        public void Feedback(FeedbackInfoType infoType, string message)
        {
            FeedbackInfo info = new FeedbackInfo() { Owner = this, InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(message) };

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
