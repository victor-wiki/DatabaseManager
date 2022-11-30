using Azure.Core.Diagnostics;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using KellermanSoftware.CompareNetObjects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseManager.Core
{
    public class DepencencyFetcher
    {
        private DbInterpreter dbInterpreter;
        private DatabaseType databaseType;

        public DepencencyFetcher(DbInterpreter dbInterpreter)
        {
            this.dbInterpreter = dbInterpreter;
            this.databaseType = this.dbInterpreter.DatabaseType;
        }

        public async Task<List<DbObjectUsage>> Fetch(DatabaseObject dbObject, bool denpendOnThis = true)
        {
            List<DbObjectUsage> usages = new List<DbObjectUsage>();

            DatabaseObjectType objType = DbObjectHelper.GetDatabaseObjectType(dbObject);

            #region Table Dependencies

            if (objType == DatabaseObjectType.Table)
            {
                SchemaInfoFilter tableForeignKeysFilter = this.GetTableSchemaInfoFilter(dbObject);

                var foreignKeys = await this.dbInterpreter.GetTableForeignKeysAsync(tableForeignKeysFilter, denpendOnThis);

                var fkGroups = foreignKeys.GroupBy(item => new { item.Schema, item.TableName, item.ReferencedSchema, item.ReferencedTableName })
                     .OrderBy(item => item.Key.Schema).ThenBy(item => item.Key.TableName);

                foreach (var fk in fkGroups)
                {
                    DbObjectUsage usage = new DbObjectUsage() { ObjectType = "Table", RefObjectType = "Table" };

                    if (denpendOnThis)
                    {
                        usage.ObjectSchema = fk.Key.Schema;
                        usage.ObjectName = fk.Key.TableName;
                        usage.RefObjectSchema = dbObject.Schema;
                        usage.RefObjectName = dbObject.Name;
                    }
                    else
                    {
                        usage.ObjectSchema = dbObject.Schema;
                        usage.ObjectName = dbObject.Name;
                        usage.RefObjectSchema = fk.Key.ReferencedSchema;
                        usage.RefObjectName = fk.Key.ReferencedTableName;
                    }

                    usages.Add(usage);
                }
            }

            #endregion

            #region View Dependencies

            if (objType == DatabaseObjectType.Table || objType == DatabaseObjectType.View)
            {
                SchemaInfoFilter viewTablesFilter = denpendOnThis ? this.GetTableSchemaInfoFilter(dbObject) : this.GetViewSchemaInfoFilter(dbObject);

                var viewTableUsages = await this.dbInterpreter.GetViewTableUsages(viewTablesFilter, denpendOnThis);

                usages.AddRange(viewTableUsages);
            }

            #endregion

            #region Routine Script Dependencies   

            if (databaseType == DatabaseType.SqlServer || databaseType == DatabaseType.Oracle)
            {
                SchemaInfoFilter routineScriptsFilter = this.GetDbObjectSchemaInfoFilter(dbObject);
                routineScriptsFilter.DatabaseObjectType = objType;

                var routineScriptUsages = await this.dbInterpreter.GetRoutineScriptUsages(routineScriptsFilter, denpendOnThis);

                usages.AddRange(routineScriptUsages);
            }
            else
            {
                using (DbConnection connection = this.dbInterpreter.CreateConnection())
                {
                    if (denpendOnThis)
                    {
                        this.dbInterpreter.Option.ObjectFetchMode = DatabaseObjectFetchMode.Details;

                        var procedures = await this.dbInterpreter.GetProceduresAsync(connection);

                        if (!(dbObject is Procedure))
                        {
                            var functions = await this.dbInterpreter.GetFunctionsAsync(connection);
                            usages.AddRange(this.GetRoutineScriptUsagesForRef(functions, dbObject));
                        }

                        usages.AddRange(this.GetRoutineScriptUsagesForRef(procedures, dbObject));
                    }
                    else
                    {
                        this.dbInterpreter.Option.ObjectFetchMode = DatabaseObjectFetchMode.Details;

                        SchemaInfoFilter dbObjectFilter = this.GetDbObjectSchemaInfoFilter(dbObject);

                        ScriptDbObject sdb = null;

                        if (dbObject is View)
                        {
                            sdb = (await this.dbInterpreter.GetViewsAsync(connection, dbObjectFilter)).FirstOrDefault();
                        }
                        else if (dbObject is Function)
                        {
                            sdb = (await this.dbInterpreter.GetFunctionsAsync(connection, dbObjectFilter)).FirstOrDefault();
                        }
                        else if (dbObject is Procedure)
                        {
                            sdb = (await this.dbInterpreter.GetProceduresAsync(connection, dbObjectFilter)).FirstOrDefault();
                        }

                        this.dbInterpreter.Option.ObjectFetchMode = DatabaseObjectFetchMode.Simple;

                        List<Function> functions = await this.dbInterpreter.GetFunctionsAsync(connection);
                        List<RoutineScriptUsage> routineScriptUsages = new List<RoutineScriptUsage>();

                        if (sdb is View)
                        {
                            routineScriptUsages.AddRange(this.GetRoutineScriptUsages(sdb, functions));
                        }
                        else if (sdb is Function || sdb is Procedure)
                        {
                            List<Table> tables = await this.dbInterpreter.GetTablesAsync(connection);
                            List<View> views = await this.dbInterpreter.GetViewsAsync(connection);

                            routineScriptUsages.AddRange(this.GetRoutineScriptUsages(sdb, tables));
                            routineScriptUsages.AddRange(this.GetRoutineScriptUsages(sdb, views));
                            routineScriptUsages.AddRange(this.GetRoutineScriptUsages(sdb, functions));

                            if (sdb is Procedure)
                            {
                                var procedures = await this.dbInterpreter.GetProceduresAsync(connection);

                                routineScriptUsages.AddRange(this.GetRoutineScriptUsages(sdb, procedures));
                            }
                        }

                        if (routineScriptUsages.Count > 0)
                        {
                            usages.AddRange(routineScriptUsages);
                        }
                    }
                }
            }

            #endregion

            return usages;
        }

        private List<RoutineScriptUsage> GetRoutineScriptUsages(ScriptDbObject scriptDbObject, IEnumerable<DatabaseObject> dbObjects)
        {
            List<RoutineScriptUsage> usages = new List<RoutineScriptUsage>();

            var dbObjectNames = dbObjects.Where(item => !(item.Schema == scriptDbObject.Schema && item.Name == scriptDbObject.Name)).Select(item => item.Name);

            foreach (var name in dbObjectNames)
            {
                string body = ScriptParser.ExtractScriptBody(scriptDbObject.Definition);

                if (Regex.IsMatch(body, $@"\b{name}\b", RegexOptions.Multiline | RegexOptions.IgnoreCase))
                {
                    RoutineScriptUsage usage = new RoutineScriptUsage() { ObjectType = scriptDbObject.GetType().Name, ObjectSchema = scriptDbObject.Schema, ObjectName = scriptDbObject.Name };

                    var dbObj = dbObjects.FirstOrDefault(item => item.Name == name);

                    usage.RefObjectType = dbObj.GetType().Name;
                    usage.RefObjectSchema = dbObj.Schema;
                    usage.RefObjectName = dbObj.Name;

                    usages.Add(usage);
                }
            }

            return usages;
        }

        private List<RoutineScriptUsage> GetRoutineScriptUsagesForRef(IEnumerable<ScriptDbObject> scriptDbObjects, DatabaseObject refDbObject)
        {
            List<RoutineScriptUsage> usages = new List<RoutineScriptUsage>();

            foreach (ScriptDbObject sdb in scriptDbObjects.Where(item => !(item.Schema == refDbObject.Schema && item.Name == refDbObject.Name)))
            {
                if (Regex.IsMatch(sdb.Definition, $@"\b{refDbObject.Name}\b", RegexOptions.Multiline | RegexOptions.IgnoreCase))
                {
                    RoutineScriptUsage usage = new RoutineScriptUsage() { ObjectType = sdb.GetType().Name, ObjectSchema = sdb.Schema, ObjectName = sdb.Name };

                    usage.RefObjectType = refDbObject.GetType().Name;
                    usage.RefObjectSchema = refDbObject.Schema;
                    usage.RefObjectName = refDbObject.Name;

                    usages.Add(usage);
                }
            }

            return usages;
        }

        private SchemaInfoFilter GetSchemaInfoFilter(DatabaseObject dbObject)
        {
            var filter = new SchemaInfoFilter() { Schema = dbObject.Schema };

            return filter;
        }

        private SchemaInfoFilter GetTableSchemaInfoFilter(DatabaseObject dbObject)
        {
            var filter = this.GetSchemaInfoFilter(dbObject);

            filter.TableNames = new string[] { dbObject.Name };

            return filter;
        }

        private SchemaInfoFilter GetViewSchemaInfoFilter(DatabaseObject dbObject)
        {
            var filter = this.GetSchemaInfoFilter(dbObject);

            filter.ViewNames = new string[] { dbObject.Name };

            return filter;
        }

        private SchemaInfoFilter GetDbObjectSchemaInfoFilter(DatabaseObject dbObject)
        {
            var filter = this.GetSchemaInfoFilter(dbObject);

            if (dbObject is Table)
            {
                filter.TableNames = new string[] { dbObject.Name };
            }
            else if (dbObject is View)
            {
                filter.ViewNames = new string[] { dbObject.Name };
            }
            else if (dbObject is Function)
            {
                filter.FunctionNames = new string[] { dbObject.Name };
            }
            else if (dbObject is Procedure)
            {
                filter.ProcedureNames = new string[] { dbObject.Name };
            }

            return filter;
        }
    }
}
