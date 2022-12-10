using Antlr4.Runtime.Dfa;
using DatabaseConverter.Core;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Model;
using SqlAnalyser.Core;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace DatabaseManager.Core
{
    public abstract class DbDiagnosis
    {
        public abstract DatabaseType DatabaseType { get; }
        protected ConnectionInfo connectionInfo;
        public string Schema { get; set; }

        public FeedbackHandler OnFeedback;

        public DbDiagnosis(ConnectionInfo connectionInfo)
        {
            this.connectionInfo = connectionInfo;
        }

        #region Diagnose Table
        public virtual Task<TableDiagnoseResult> DiagnoseTable(TableDiagnoseType diagnoseType)
        {
            if (diagnoseType == TableDiagnoseType.SelfReferenceSame)
            {
                return this.DiagnoseSelfReferenceSameForTable();
            }
            else if (diagnoseType == TableDiagnoseType.NotNullWithEmpty)
            {
                return this.DiagnoseNotNullWithEmptyForTable();
            }
            else if (diagnoseType == TableDiagnoseType.WithLeadingOrTrailingWhitespace)
            {
                return this.DiagnoseWithLeadingOrTrailingWhitespaceForTable();
            }

            throw new NotSupportedException($"Not support diagnose for {diagnoseType}");
        }

        public virtual async Task<TableDiagnoseResult> DiagnoseNotNullWithEmptyForTable()
        {
            this.Feedback("Begin to diagnose not null fields with empty value...");

            TableDiagnoseResult result = await this.DiagnoseTableColumn(TableDiagnoseType.NotNullWithEmpty);

            this.Feedback("End diagnose not null fields with empty value.");

            return result;
        }

        public virtual async Task<TableDiagnoseResult> DiagnoseWithLeadingOrTrailingWhitespaceForTable()
        {
            this.Feedback("Begin to diagnose character fields with leading or trailing whitespace...");

            TableDiagnoseResult result = await this.DiagnoseTableColumn(TableDiagnoseType.WithLeadingOrTrailingWhitespace);

            this.Feedback("End diagnose character fields with leading or trailing whitespace.");

            return result;
        }

        private async Task<TableDiagnoseResult> DiagnoseTableColumn(TableDiagnoseType diagnoseType)
        {
            TableDiagnoseResult result = new TableDiagnoseResult();

            DbInterpreterOption option = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple };

            DbInterpreter interpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, this.connectionInfo, option);

            SchemaInfoFilter filter = new SchemaInfoFilter() { Schema = this.Schema };

            this.Feedback("Begin to get table columns...");

            List<TableColumn> columns = await interpreter.GetTableColumnsAsync(filter);

            this.Feedback("End get table columns.");

            dynamic groups = null;

            if (diagnoseType == TableDiagnoseType.NotNullWithEmpty)
            {
                groups = columns.Where(item => DataTypeHelper.IsCharType(item.DataType) && !item.DataType.EndsWith("[]") && !item.IsNullable)
                         .GroupBy(item => new { item.Schema, item.TableName });
            }
            else if (diagnoseType == TableDiagnoseType.WithLeadingOrTrailingWhitespace)
            {
                groups = columns.Where(item => DataTypeHelper.IsCharType(item.DataType) && !item.DataType.EndsWith("[]"))
                         .GroupBy(item => new { item.Schema, item.TableName });
            }

            using (DbConnection dbConnection = interpreter.CreateConnection())
            {
                foreach (var group in groups)
                {
                    foreach (TableColumn column in group)
                    {
                        string countSql = "";

                        if (diagnoseType == TableDiagnoseType.NotNullWithEmpty)
                        {
                            countSql = this.GetTableColumnWithEmptyValueSql(interpreter, column, true);
                        }
                        else
                        {
                            countSql = this.GetTableColumnWithLeadingOrTrailingWhitespaceSql(interpreter, column, true);
                        }

                        this.Feedback($@"Begin to get invalid record count for column ""{column.Name}"" of table ""{column.TableName}""...");

                        int count = Convert.ToInt32(await interpreter.GetScalarAsync(dbConnection, countSql));

                        this.Feedback($@"End get invalid record count for column ""{column.Name}"" of table ""{column.TableName}"", the count is {count}.");

                        if (count > 0)
                        {
                            string sql = "";

                            if (diagnoseType == TableDiagnoseType.NotNullWithEmpty)
                            {
                                sql = this.GetTableColumnWithEmptyValueSql(interpreter, column, false);
                            }
                            else
                            {
                                sql = this.GetTableColumnWithLeadingOrTrailingWhitespaceSql(interpreter, column, false);
                            }

                            result.Details.Add(new TableDiagnoseResultDetail()
                            {
                                DatabaseObject = column,
                                RecordCount = count,
                                Sql = sql
                            });
                        }
                    }
                }
            }

            return result;
        }

        public virtual async Task<TableDiagnoseResult> DiagnoseSelfReferenceSameForTable()
        {
            this.Feedback("Begin to diagnose self reference with same value...");

            TableDiagnoseResult result = new TableDiagnoseResult();

            DbInterpreterOption option = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Details };

            DbInterpreter interpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, this.connectionInfo, option);

            this.Feedback("Begin to get foreign keys...");

            SchemaInfoFilter filter = new SchemaInfoFilter() { Schema = this.Schema };

            List<TableForeignKey> foreignKeys = await interpreter.GetTableForeignKeysAsync(filter);

            this.Feedback("End get foreign keys.");

            var groups = foreignKeys.Where(item => item.ReferencedTableName == item.TableName)
                         .GroupBy(item => new { item.Schema, item.TableName });

            using (DbConnection dbConnection = interpreter.CreateConnection())
            {
                foreach (var group in groups)
                {
                    foreach (TableForeignKey foreignKey in group)
                    {
                        string countSql = this.GetTableColumnReferenceSql(interpreter, foreignKey, true);

                        this.Feedback($@"Begin to get invalid record count for foreign key ""{foreignKey.Name}"" of table ""{foreignKey.TableName}""...");

                        int count = Convert.ToInt32(await interpreter.GetScalarAsync(dbConnection, countSql));

                        this.Feedback($@"End get invalid record count for column ""{foreignKey.Name}"" of table ""{foreignKey.TableName}"", the count is {count}.");

                        if (count > 0)
                        {
                            result.Details.Add(new TableDiagnoseResultDetail()
                            {
                                DatabaseObject = foreignKey,
                                RecordCount = count,
                                Sql = this.GetTableColumnReferenceSql(interpreter, foreignKey, false)
                            });
                        }
                    }
                }
            }

            this.Feedback("End diagnose self reference with same value.");

            return result;
        }

        public abstract string GetStringLengthFunction();
        public abstract string GetStringNullFunction();

        public static DbDiagnosis GetInstance(DatabaseType databaseType, ConnectionInfo connectionInfo)
        {
            if (databaseType == DatabaseType.SqlServer)
            {
                return new SqlServerDiagnosis(connectionInfo);
            }
            else if (databaseType == DatabaseType.MySql)
            {
                return new MySqlDiagnosis(connectionInfo);
            }
            else if (databaseType == DatabaseType.Oracle)
            {
                return new OracleDiagnosis(connectionInfo);
            }
            else if (databaseType == DatabaseType.Postgres)
            {
                return new PostgresDiagnosis(connectionInfo);
            }
            else if(databaseType == DatabaseType.Sqlite)
            {
                return new SqliteDiagnosis(connectionInfo);
            }

            throw new NotImplementedException($"Not implemente diagnosis for {databaseType}.");
        }

        protected virtual string GetTableColumnWithEmptyValueSql(DbInterpreter interpreter, TableColumn column, bool isCount)
        {
            string tableName = $"{column.Schema}.{interpreter.GetQuotedString(column.TableName)}";
            string selectColumn = isCount ? $"{this.GetStringNullFunction()}(COUNT(1),0) AS {interpreter.GetQuotedString("Count")}" : "*";

            string sql = $"SELECT {selectColumn} FROM {tableName} WHERE {this.GetStringLengthFunction()}({interpreter.GetQuotedString(column.Name)})=0";

            return sql;
        }

        protected virtual string GetTableColumnWithLeadingOrTrailingWhitespaceSql(DbInterpreter interpreter, TableColumn column, bool isCount)
        {
            string tableName = $"{column.Schema}.{interpreter.GetQuotedString(column.TableName)}";
            string selectColumn = isCount ? $"{this.GetStringNullFunction()}(COUNT(1),0) AS {interpreter.GetQuotedString("Count")}" : "*";
            string columnName = interpreter.GetQuotedString(column.Name);
            string lengthFunName = this.GetStringLengthFunction();

            string sql = $"SELECT {selectColumn} FROM {tableName} WHERE {lengthFunName}(TRIM({columnName}))<{lengthFunName}({columnName})";

            if (interpreter.DatabaseType == DatabaseType.Postgres)
            {
                if (column.DataType == "character")
                {
                    sql += $" OR {lengthFunName}({columnName})<>{column.MaxLength}";
                }
            }

            return sql;
        }

        protected virtual string GetTableColumnReferenceSql(DbInterpreter interpreter, TableForeignKey foreignKey, bool isCount)
        {
            string tableName = $"{foreignKey.Schema}.{interpreter.GetQuotedString(foreignKey.TableName)}";
            string selectColumn = isCount ? $"{this.GetStringNullFunction()}(COUNT(1),0) AS {interpreter.GetQuotedString("Count")}" : "*";
            string whereClause = string.Join(" AND ", foreignKey.Columns.Select(item => $"{interpreter.GetQuotedString(item.ColumnName)}={interpreter.GetQuotedString(item.ReferencedColumnName)}"));

            string sql = $"SELECT {selectColumn} FROM {tableName} WHERE ({whereClause})";

            return sql;
        }
        #endregion

        #region Diagnose Scripts
        public virtual Task<List<ScriptDiagnoseResult>> DiagnoseScript(ScriptDiagnoseType diagnoseType)
        {
            if (diagnoseType == ScriptDiagnoseType.ViewColumnAliasWithoutQuotationChar)
            {
                return this.DiagnoseViewColumnAliasForScript();
            }
            else if (diagnoseType == ScriptDiagnoseType.NameNotMatch)
            {
                return this.DiagnoseNameNotMatchForScript();
            }

            throw new NotSupportedException($"Not support diagnose for {diagnoseType}.");
        }

        public virtual async Task<List<ScriptDiagnoseResult>> DiagnoseViewColumnAliasForScript()
        {
            this.Feedback("Begin to diagnose column alias has no quotation char for view...");

            List<ScriptDiagnoseResult> results = await this.DiagnoseViewColumnAlias();

            this.Feedback("End diagnosecolumn alias has no quotation char for view.");

            return results;
        }

        public virtual async Task<List<ScriptDiagnoseResult>> DiagnoseNameNotMatchForScript()
        {
            this.Feedback("Begin to diagnose name not match in script...");

            List<ScriptDiagnoseResult> results = await this.DiagnoseNameNotMatch();

            this.Feedback("End diagnose name not match in script.");

            return results;
        }

        private async Task<List<ScriptDiagnoseResult>> DiagnoseViewColumnAlias()
        {

            DbInterpreterOption option = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Details };

            DbInterpreter interpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, this.connectionInfo, option);

            SchemaInfoFilter filter = new SchemaInfoFilter() { Schema = this.Schema };

            this.Feedback("Begin to get views...");

            List<View> views = await interpreter.GetViewsAsync(filter);

            this.Feedback("End get views.");

            List<ScriptDiagnoseResult> results = await Task.Run(() =>
            {
                return this.HandleColumnAliasWithoutQuotationChar(interpreter, views);
            });

            return results;
        }

        private List<ScriptDiagnoseResult> HandleColumnAliasWithoutQuotationChar(DbInterpreter interpreter, List<View> views)
        {
            List<ScriptDiagnoseResult> results = new List<ScriptDiagnoseResult>();           

            this.Feedback("Begin to analyse column alias...");

            foreach (var view in views)
            {
                string definition = view.Definition;

                string wrappedDefinition = this.GetWrappedDefinition(definition);

                SqlAnalyserBase sqlAnalyser = TranslateHelper.GetSqlAnalyser(this.DatabaseType, wrappedDefinition);

                sqlAnalyser.RuleAnalyser.Option.ParseTokenChildren = false;
                sqlAnalyser.RuleAnalyser.Option.ExtractFunctions = false;
                sqlAnalyser.RuleAnalyser.Option.ExtractFunctionChildren = false;

                var analyseResult = sqlAnalyser.Analyse<View>();

                if (analyseResult != null && !analyseResult.HasError)
                {
                    var selectStatement = analyseResult.Script.Statements.FirstOrDefault(item => item is SelectStatement) as SelectStatement;

                    if (selectStatement != null)
                    {
                        ScriptDiagnoseResult result = new ScriptDiagnoseResult() { DbObject = view };

                        result.Details.AddRange(this.ParseSelectStatementColumns(interpreter, selectStatement));

                        if (selectStatement.UnionStatements != null)
                        {
                            foreach (var union in selectStatement.UnionStatements)
                            {
                                result.Details.AddRange(this.ParseSelectStatementColumns(interpreter, union.SelectStatement));
                            }
                        }

                        if (result.Details.Count > 0)
                        {
                            results.Add(result);
                        }
                    }
                }
            }

            this.Feedback("End analyse column alias.");

            return results;
        }

        private List<ScriptDiagnoseResultDetail> ParseSelectStatementColumns(DbInterpreter interpreter, SelectStatement statement)
        {
            List<ScriptDiagnoseResultDetail> details = new List<ScriptDiagnoseResultDetail>();

            var columns = statement.Columns;

            foreach (var col in columns)
            {
                if (col.Alias != null && !col.Alias.Symbol.StartsWith(interpreter.QuotationLeftChar))
                {
                    ScriptDiagnoseResultDetail detail = new ScriptDiagnoseResultDetail();

                    detail.ObjectType = DatabaseObjectType.Column;
                    detail.InvalidName = col.FieldName;
                    detail.Name = interpreter.GetQuotedString(col.FieldName);
                    detail.Index = col.Alias.StartIndex.Value;

                    details.Add(detail);
                }
            }

            return details;
        }

        private async Task<List<ScriptDiagnoseResult>> DiagnoseNameNotMatch()
        {
            List<ScriptDiagnoseResult> results = new List<ScriptDiagnoseResult>();

            DbInterpreterOption option = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Details };

            DbInterpreter interpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, this.connectionInfo, option);

            SchemaInfoFilter filter = new SchemaInfoFilter() { Schema = this.Schema };

            List<View> views = null;
            List<Function> functions = null;
            List<Procedure> procedures = null;
            List<ViewColumnUsage> viewColumnUsages = null;
            List<RoutineScriptUsage> functionUsages = null;
            List<RoutineScriptUsage> procedureUsages = null;

            using (DbConnection connection = interpreter.CreateConnection())
            {
                await interpreter.OpenConnectionAsync(connection);

                interpreter.Option.ObjectFetchMode = DatabaseObjectFetchMode.Simple;

                var tables = await interpreter.GetTablesAsync();

                interpreter.Option.ObjectFetchMode = DatabaseObjectFetchMode.Details;

                #region View
                this.Feedback("Begin to get views...");

                views = await interpreter.GetViewsAsync(connection, filter);

                this.Feedback("End get views.");

                var viewNamesFilter = new SchemaInfoFilter() { Schema = this.Schema, ViewNames = views.Select(item => item.Name).ToArray() };

                if (this.DatabaseType == DatabaseType.SqlServer)
                {
                    viewColumnUsages = await interpreter.GetViewColumnUsages(connection, viewNamesFilter);
                }
                #endregion

                #region Function
                this.Feedback("Begin to get functions...");

                functions = await interpreter.GetFunctionsAsync(connection, filter);

                this.Feedback("End get functions.");

                var functionNamesFilter = new SchemaInfoFilter() { DatabaseObjectType = DatabaseObjectType.Function, FunctionNames = functions.Select(item => item.Name).ToArray() };

                if (this.DatabaseType == DatabaseType.SqlServer)
                {
                    functionUsages = await interpreter.GetRoutineScriptUsages(connection, functionNamesFilter);

                    await this.HandleRoutineScriptUsagesAsync(functionUsages, interpreter, connection, tables, views, functions);
                }
                else if (this.DatabaseType == DatabaseType.MySql)
                {
                    var usages = await this.GetRoutineScriptUsages(interpreter, connection, functions, tables, views, functions);

                    if (usages.Count > 0)
                    {
                        functionUsages = usages;
                    }
                }
                #endregion

                #region Procedure
                this.Feedback("Begin to get procedures...");

                procedures = await interpreter.GetProceduresAsync(connection, filter);

                this.Feedback("End get procedures.");

                var procedureNamesFilter = new SchemaInfoFilter() { DatabaseObjectType = DatabaseObjectType.Procedure, ProcedureNames = procedures.Select(item => item.Name).ToArray() };

                if (this.DatabaseType == DatabaseType.SqlServer)
                {
                    procedureUsages = await interpreter.GetRoutineScriptUsages(connection, procedureNamesFilter);

                    await this.HandleRoutineScriptUsagesAsync(procedureUsages, interpreter, connection, tables, views, functions, procedures);
                }
                else if (this.DatabaseType == DatabaseType.MySql)
                {
                    var usages = await this.GetRoutineScriptUsages(interpreter, connection, procedures, tables, views, functions, procedures);

                    if (usages.Count > 0)
                    {
                        procedureUsages = usages;
                    }
                }
                #endregion
            }

            await Task.Run(() =>
            {
                if (views != null && viewColumnUsages != null)
                {
                    results.AddRange(this.DiagnoseNameNotMatchForViews(views, viewColumnUsages, interpreter.CommentString));
                }

                if (functions != null && functionUsages != null)
                {
                    results.AddRange(this.DiagnoseNameNotMatchForRoutineScripts(functions, functionUsages, interpreter.CommentString));
                }

                if (procedures != null && procedureUsages != null)
                {
                    results.AddRange(this.DiagnoseNameNotMatchForRoutineScripts(procedures, procedureUsages, interpreter.CommentString));
                }
            });

            return results;
        }

        private async Task<List<RoutineScriptUsage>> GetRoutineScriptUsages(DbInterpreter interpreter, DbConnection connection, IEnumerable<ScriptDbObject> scriptDbObjects, List<Table> tables, List<View> views,
              List<Function> functions = null, List<Procedure> procedures = null)
        {
            var tableNames = tables.Select(item => item.Name);
            var viewNames = views.Select(item => item.Name);
            var functionNames = functions == null ? null : functions.Select(item => item.Name);
            var procedureNames = procedures == null ? null : procedures.Select(item => item.Name);

            List<RoutineScriptUsage> usages = new List<RoutineScriptUsage>();

            foreach (var sdb in scriptDbObjects)
            {
                usages.AddRange(this.GetRoutineScriptUsages(sdb, tables, tableNames));
                usages.AddRange(this.GetRoutineScriptUsages(sdb, views, viewNames));

                if (functions != null)
                {
                    usages.AddRange(this.GetRoutineScriptUsages(sdb, functions, functionNames));
                }

                if (procedures != null)
                {
                    usages.AddRange(this.GetRoutineScriptUsages(sdb, procedures, procedureNames));
                }
            }

            if (usages.Count > 0)
            {
                await this.HandleRoutineScriptUsagesAsync(usages, interpreter, connection, tables, views, functions, procedures);
            }

            return usages;
        }

        private List<RoutineScriptUsage> GetRoutineScriptUsages(ScriptDbObject scriptDbObject, IEnumerable<DatabaseObject> dbObjects, IEnumerable<string> dbObjectNames)
        {
            List<RoutineScriptUsage> usages = new List<RoutineScriptUsage>();

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

        private async Task HandleRoutineScriptUsagesAsync(IEnumerable<RoutineScriptUsage> usages, DbInterpreter interpreter, DbConnection connection,
             List<Table> tables = null, List<View> views = null, List<Function> functions = null, List<Procedure> procedures = null
            )
        {
            var usageTables = usages.Where(item => item.RefObjectType == "Table");

            if (tables != null)
            {
                //correct table name to defined if it's not same as defined
                this.CorrectRefObjectName(usageTables, tables);
            }

            var tableNames = usageTables.Select(item => item.RefObjectName).ToArray();

            var tableNamesFilter = new SchemaInfoFilter() { TableNames = tableNames };

            var tableColumns = await interpreter.GetTableColumnsAsync(connection, tableNamesFilter);

            foreach (var usage in usages)
            {
                if (usage.RefObjectType == "Table")
                {
                    usage.ColumnNames = tableColumns.Where(item => item.Schema == usage.RefObjectSchema && item.TableName == usage.RefObjectName).Select(item => item.Name).ToList();
                }
            }

            if (views != null)
            {
                this.CorrectRefObjectName(usages.Where(item => item.RefObjectType == "View"), views);
            }

            if (functions != null)
            {
                this.CorrectRefObjectName(usages.Where(item => item.RefObjectType == "Function"), functions);
            }

            if (procedures != null)
            {
                this.CorrectRefObjectName(usages.Where(item => item.RefObjectType == "Procedure"), procedures);
            }
        }

        private void CorrectRefObjectName(IEnumerable<RoutineScriptUsage> usages, IEnumerable<DatabaseObject> dbObjects)
        {
            if (usages == null || dbObjects == null)
            {
                return;
            }

            foreach (var usage in usages)
            {
                string refObjectName = usage.RefObjectName;

                var dbObject = dbObjects.FirstOrDefault(item => item.Schema == usage.RefObjectSchema && item.Name.ToLower() == refObjectName.ToLower());

                if (dbObject != null && dbObject.Name != refObjectName)
                {
                    usage.RefObjectName = dbObject.Name;
                }
            }
        }

        private List<ScriptDiagnoseResult> DiagnoseNameNotMatchForViews(IEnumerable<ScriptDbObject> views, List<ViewColumnUsage> columnUsages, string commentString)
        {
            List<RoutineScriptUsage> usages = columnUsages.GroupBy(item => new { item.ObjectCatalog, item.ObjectSchema, item.ObjectName, item.RefObjectCatalog, item.RefObjectSchema, item.RefObjectName })
                .Select(item => new RoutineScriptUsage()
                {
                    ObjectCatalog = item.Key.ObjectCatalog,
                    ObjectSchema = item.Key.ObjectSchema,
                    ObjectName = item.Key.ObjectName,
                    ObjectType = "View",
                    RefObjectType = "Table",
                    RefObjectCatalog = item.Key.RefObjectCatalog,
                    RefObjectSchema = item.Key.RefObjectSchema,
                    RefObjectName = item.Key.RefObjectName,
                    ColumnNames = item.Select(t => t.ColumnName).ToList()
                }).ToList();

            return this.DiagnoseNameNotMatchForRoutineScripts(views, usages, commentString);
        }

        private List<ScriptDiagnoseResult> DiagnoseNameNotMatchForRoutineScripts(IEnumerable<ScriptDbObject> routineScripts, List<RoutineScriptUsage> usages, string commentString)
        {
            List<ScriptDiagnoseResult> results = new List<ScriptDiagnoseResult>();

            if (usages.Count == 0)
            {
                return results;
            }

            foreach (var rs in routineScripts)
            {
                ScriptDiagnoseResult result = new ScriptDiagnoseResult();

                result.DbObject = rs;

                string definition = rs.Definition;

                List<ScriptDiagnoseResultDetail> details = new List<ScriptDiagnoseResultDetail>();

                var rsUsages = usages.Where(item => item.ObjectSchema == rs.Schema && item.ObjectName == rs.Name);

                var usageRefObjectNames = rsUsages.Select(item => item.RefObjectName).Distinct().ToList();
                var usageColumnNames = rsUsages.SelectMany(item => item.ColumnNames).ToList();

                details.AddRange(this.MatchUsageNames(rs.Definition, DatabaseObjectType.Table, usageRefObjectNames, commentString));
                details.AddRange(this.MatchUsageNames(rs.Definition, DatabaseObjectType.Column, usageColumnNames, commentString));

                if (details.Count > 0)
                {
                    result.Details = details;
                    results.Add(result);
                }
            }

            return results;
        }

        private string GetWrappedDefinition(string definition)
        {
            return definition.Replace(Environment.NewLine, "\n");
        }

        private List<ScriptDiagnoseResultDetail> MatchUsageNames(string definition, DatabaseObjectType databaseObjectType, List<string> names, string commentString)
        {
            List<ScriptDiagnoseResultDetail> details = new List<ScriptDiagnoseResultDetail>();

            if (names.Count == 0)
            {
                return details;
            }

            string pattern = $@"\b({string.Join("|", names)})\b";

            string wrappedDefinition = this.GetWrappedDefinition(definition);

            ScriptContentInfo textContentInfo = ScriptParser.GetContentInfo(wrappedDefinition, "\n", commentString);
            var commentLines = textContentInfo.Lines.Where(item => item.Type == TextLineType.Comment);

            var matches = Regex.Matches(wrappedDefinition, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            int beginAsIndex = ScriptParser.GetBeginAsIndex(wrappedDefinition);

            foreach (Match match in matches)
            {
                string value = match.Value;

                if (beginAsIndex > 0 && match.Index < beginAsIndex)
                {
                    continue;
                }

                //check whether the value is in comment line
                if (commentLines.Any(item => match.Index > item.FirstCharIndex && match.Index < item.FirstCharIndex + item.Length))
                {
                    continue;
                }

                if (names.Any(item => item.ToLower() == value.ToLower()) && !names.Any(item => item == value))
                {
                    ScriptDiagnoseResultDetail detail = new ScriptDiagnoseResultDetail() { ObjectType = databaseObjectType, Index = match.Index, InvalidName = value };

                    string name = names.FirstOrDefault(item => item.ToLower() == value.ToLower());

                    detail.Name = name;

                    details.Add(detail);
                }
            }

            return details;
        }

        #endregion

        protected void Feedback(string message)
        {
            if (this.OnFeedback != null)
            {
                this.OnFeedback(new FeedbackInfo() { InfoType = FeedbackInfoType.Info, Message = message, Owner = this });
            }
        }
    }
}
