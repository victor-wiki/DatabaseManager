using DatabaseInterpreter.Utility;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SqlAnalyser.Core
{
    public class PlSqlStatementScriptBuilder : StatementScriptBuilder
    {
        public override StatementScriptBuilder Build(Statement statement, bool appendSeparator = true)
        {
            base.Build(statement, appendSeparator);

            int startIndex = this.Length;

            if (statement is SelectStatement select)
            {
                this.BuildSelectStatement(select, appendSeparator);
            }
            else if (statement is UnionStatement union)
            {
                this.AppendLine(this.GetUnionTypeName(union.Type));
                this.Build(union.SelectStatement);
            }
            else if (statement is InsertStatement insert)
            {
                this.AppendLine($"INSERT INTO {insert.TableName}");

                if (insert.Columns.Count > 0)
                {
                    this.AppendLine($"({string.Join(",", insert.Columns.Select(item => item.ToString()))})");
                }

                if (insert.SelectStatements != null && insert.SelectStatements.Count > 0)
                {
                    this.AppendChildStatements(insert.SelectStatements, true);
                }
                else
                {
                    this.AppendLine($"VALUES({string.Join(",", insert.Values.Select(item => item))});");
                }

                this.ReplaceTemporaryTableContent(insert.TableName, startIndex);
            }
            else if (statement is UpdateStatement update)
            {
                int fromItemsCount = update.FromItems == null ? 0 : update.FromItems.Count;

                TableName tableName = StatementScriptBuilderHelper.GetUpdateSetTableName(update);
                bool hasJoin = AnalyserHelper.IsFromItemsHaveJoin(update.FromItems);

                this.AppendLine($"UPDATE");

                List<TableName> tableNames = new List<TableName>();

                string strTableName = null;

                if (tableName != null)
                {
                    strTableName = tableName.Symbol;

                    tableNames.Add(tableName);
                }

                if (fromItemsCount > 0)
                {
                    List<NameValueItem> nameValues = new List<NameValueItem>();

                    NameValueItem nameValueItem = new NameValueItem();

                    nameValueItem.Name = new TokenInfo($"({string.Join(",", update.SetItems.Select(item => item.Name))})");

                    string colNames = string.Join(",", update.SetItems.Select(item => item.Value));

                    StringBuilder sb = new StringBuilder();

                    sb.Append($"(SELECT {colNames} ");

                    if (!hasJoin)
                    {
                        string fromTableNames = string.Join(",", update.FromItems.Select(item => item.TableName.NameWithAlias));

                        sb.AppendLine($"FROM {fromTableNames}");
                    }
                    else
                    {
                        int i = 0;

                        foreach (FromItem fromItem in update.FromItems)
                        {
                            string tn = fromItem.TableName.Symbol;
                            string talias = fromItem.TableName.Alias?.ToString();

                            int j = 0;

                            foreach (var joinItem in fromItem.JoinItems)
                            {
                                if (j == 0)
                                {
                                    sb.Append("FROM");
                                }
                                else
                                {
                                    sb.Append(joinItem.Type.ToString() + " JOIN");
                                }

                                string joinTableName = j == 0 ? $" JOIN {fromItem.TableName.NameWithAlias}" : "";

                                sb.Append($" {this.GetNameWithAlias(joinItem.TableName)}{joinTableName} ON {joinItem.Condition}{Environment.NewLine}");

                                j++;
                            }

                            i++;
                        }
                    }

                    if (update.Condition != null && update.Condition.Symbol != null)
                    {
                        sb.AppendLine($"WHERE {update.Condition}");
                    }

                    sb.Append(")");

                    nameValueItem.Value = new TokenInfo(sb.ToString());

                    nameValues.Add(nameValueItem);

                    update.SetItems = nameValues;
                }

                if (tableNames.Count == 0 && update.TableNames.Count > 0)
                {
                    tableNames.AddRange(update.TableNames);
                }

                this.Append($" {string.Join(",", tableNames.Select(item => item.NameWithAlias))}", false);

                this.AppendLine("SET");

                this.AppendLine(string.Join("," + Environment.NewLine + Indent, update.SetItems.Select(item => $"{item.Name}={item.Value}")));

                if (update.Condition != null && update.Condition.Symbol != null)
                {
                    var children = update.Condition.Children;

                    bool hasOtherTableOrAlias = false;

                    foreach (var child in children)
                    {
                        string symbol = child.Symbol;

                        if (symbol?.Contains(".") == true)
                        {
                            string[] items = symbol.Split('.');

                            string tnOrAlias = items[items.Length - 2].Trim();

                            if (!tableNames.Any(item => item.Symbol.ToLower() == tnOrAlias || (item.Alias != null && item.Alias.Symbol.ToLower() == tnOrAlias)))
                            {
                                hasOtherTableOrAlias = true;
                                break;
                            }
                        }
                    }

                    if (!hasOtherTableOrAlias)
                    {
                        this.AppendLine($"WHERE {update.Condition}");
                    }
                }

                this.AppendLine(";");

                this.ReplaceTemporaryTableContent(tableNames.FirstOrDefault(), startIndex);
            }
            else if (statement is DeleteStatement delete)
            {
                bool hasJoin = AnalyserHelper.IsFromItemsHaveJoin(delete.FromItems);

                if (!hasJoin)
                {
                    this.AppendLine($"DELETE FROM {this.GetNameWithAlias(delete.TableName)}");

                    if (delete.Condition != null)
                    {
                        this.AppendLine($"WHERE {delete.Condition}");
                    }
                }
                else
                {
                    string tableName = delete.TableName.Symbol;

                    this.AppendLine($"DELETE FROM {delete.TableName}");

                    string alias = null;

                    int i = 0;

                    foreach (FromItem fromItem in delete.FromItems)
                    {
                        if (i == 0)
                        {
                            if (fromItem.TableName != null && fromItem.TableName.Alias != null)
                            {
                                alias = fromItem.TableName.Alias.Symbol;
                            }
                            else if (fromItem.Alias != null)
                            {
                                alias = fromItem.Alias.Symbol;
                            }
                        }

                        i++;
                    }

                    //use placeholder, the actual column name should according to the business needs.
                    this.AppendLine($"WHERE $columnName$ IN (SELECT $columnName$");

                    this.BuildFromItems(delete.FromItems, null, true);

                    if (delete.Condition != null)
                    {
                        this.AppendLine($"WHERE {delete.Condition}");
                    }

                    this.AppendLine(")");
                }

                this.AppendLine(";");

                this.ReplaceTemporaryTableContent(delete.TableName, startIndex);
            }
            else if (statement is DeclareVariableStatement declareVar)
            {
                StringBuilder sb = new StringBuilder();

                string defaultValue = (declareVar.DefaultValue == null ? "" : $" :={declareVar.DefaultValue}");

                sb.Append($"DECLARE {declareVar.Name} {declareVar.DataType}{defaultValue};");

                if (!(this.Option != null && this.Option.NotBuildDeclareStatement))
                {
                    this.AppendLine(sb.ToString());
                }
                else
                {
                    if (this.Option.OutputRemindInformation)
                    {
                        this.PrintMessage($"'{StringHelper.HandleSingleQuotationChar(sb.ToString())}'");
                    }
                }

                if (this.Option != null && this.Option.CollectDeclareStatement)
                {
                    this.DeclareStatements.Add(declareVar);
                }
            }
            else if (statement is DeclareTableStatement declareTable)
            {
                string sql = this.BuildTable(declareTable.TableInfo);

                if (this.RoutineType == RoutineType.PROCEDURE)
                {
                    sql = this.GetExecuteImmediateSql(sql);
                }

                this.AppendLine(sql);
            }
            else if (statement is CreateTableStatement createTable)
            {
                string sql = this.BuildTable(createTable.TableInfo);

                if (this.RoutineType == RoutineType.PROCEDURE && createTable.TableInfo.IsTemporary)
                {
                    sql = this.GetExecuteImmediateSql(sql);
                }

                this.AppendLine(sql);
            }
            else if (statement is IfStatement @if)
            {
                foreach (IfStatementItem item in @if.Items)
                {
                    if (item.Type == IfStatementType.IF || item.Type == IfStatementType.ELSEIF)
                    {
                        this.AppendLine($"{item.Type} {item.Condition} THEN");
                    }
                    else
                    {
                        this.AppendLine($"{item.Type}");
                    }

                    this.AppendLine("BEGIN");

                    this.AppendChildStatements(item.Statements, true);

                    this.AppendLine("END;");
                }

                this.AppendLine("END IF;");
            }
            else if (statement is CaseStatement @case)
            {
                this.AppendLine($"CASE {@case.VariableName}");

                foreach (IfStatementItem item in @case.Items)
                {
                    if (item.Type != IfStatementType.ELSE)
                    {
                        this.AppendLine($"WHEN {item.Condition} THEN");
                    }
                    else
                    {
                        this.AppendLine("ELSE");
                    }

                    this.AppendLine("BEGIN");
                    this.AppendChildStatements(item.Statements, true);
                    this.AppendLine("END;");
                }

                this.AppendLine("END CASE;");
            }
            else if (statement is SetStatement set)
            {
                if (set.Key != null && set.Value != null)
                {
                    string value = set.Value.Symbol;

                    value = this.GetSetVariableValue(set.Key.Symbol, set.Value?.Symbol);

                    if (!AnalyserHelper.IsSubquery(value))
                    {
                        this.AppendLine($"{set.Key} := {value};");
                    }
                    else
                    {
                        this.AppendLine(StatementScriptBuilderHelper.ConvertToSelectIntoVariable(set.Key.Symbol, value));
                    }
                }
            }
            else if (statement is LoopStatement loop)
            {
                if (loop.Type == LoopType.LOOP)
                {
                    this.AppendLine("LOOP");
                }
                else
                {
                    this.AppendLine($"{loop.Type.ToString()} {loop.Condition} LOOP");
                }

                this.AppendChildStatements(loop.Statements, true);
                this.AppendLine("END LOOP;");
            }
            else if (statement is LoopExitStatement loopExit)
            {
                this.AppendLine($"EXIT WHEN {loopExit.Condition};");
            }
            else if (statement is WhileStatement @while)
            {
                LoopStatement loopStatement = @while as LoopStatement;

                this.AppendLine($"WHILE {@while.Condition} LOOP");
                this.AppendChildStatements(@while.Statements, true);
                this.AppendLine("END LOOP;");
            }
            else if (statement is ReturnStatement @return)
            {
                this.AppendLine($"RETURN {@return.Value};");
            }
            else if (statement is PrintStatement print)
            {
                this.PrintMessage(print.Content.Symbol?.ToString()?.Replace("+", "||"));
            }
            else if (statement is CallStatement call)
            {
                if (!call.IsExecuteSql)
                {
                    this.AppendLine($"{call.Name}({string.Join(",", call.Parameters.Select(item => item.Value?.Symbol?.Split('=')?.LastOrDefault()))});");
                }
                else
                {
                    string content = call.Parameters.FirstOrDefault()?.Value?.Symbol;

                    if (!string.IsNullOrEmpty(content))
                    {
                        var parameters = call.Parameters.Skip(1);

                        List<CallParameter> usings = new List<CallParameter>();

                        foreach (var parameter in parameters)
                        {
                            var value = parameter.Value?.Symbol;

                            if (!parameter.IsDescription)
                            {
                                usings.Add(parameter);
                            }
                        }

                        string strUsings = usings.Count == 0 ? "" : $" USING {(string.Join(",", usings.Select(item => $"{item.Value}")))}";

                        this.AppendLine($"EXECUTE IMMEDIATE {content}{strUsings};");
                    }
                }

            }
            else if (statement is TransactionStatement transaction)
            {
                TransactionCommandType commandType = transaction.CommandType;

                switch (commandType)
                {
                    case TransactionCommandType.COMMIT:
                        this.AppendLine("COMMIT;");
                        break;
                    case TransactionCommandType.ROLLBACK:
                        this.AppendLine("ROLLBACK;");
                        break;
                }
            }
            else if (statement is LeaveStatement leave)
            {
                this.AppendLine("RETURN;");
            }
            else if (statement is TryCatchStatement tryCatch)
            {
                this.AppendLine("EXCEPTION");
                this.AppendLine("WHEN OTHERS THEN");
                this.AppendLine("BEGIN");

                this.AppendChildStatements(tryCatch.CatchStatements, true);

                this.AppendLine("END;");

                this.AppendChildStatements(tryCatch.TryStatements, true);
            }
            else if (statement is ExceptionStatement exception)
            {
                this.AppendLine("EXCEPTION");

                foreach (ExceptionItem exceptionItem in exception.Items)
                {
                    this.AppendLine($"WHEN {exceptionItem.Name} THEN");
                    this.AppendLine("BEGIN");

                    this.AppendChildStatements(exceptionItem.Statements, true);

                    this.AppendLine("END;");
                }
            }
            else if (statement is DeclareCursorStatement declareCursor)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append($"DECLARE CURSOR {declareCursor.CursorName}{(declareCursor.SelectStatement != null ? " IS" : "")}");

                if (!(this.Option != null && this.Option.NotBuildDeclareStatement))
                {
                    this.AppendLine(sb.ToString());

                    if (declareCursor.SelectStatement != null)
                    {
                        this.Build(declareCursor.SelectStatement);
                    }
                }
                else
                {
                    if (this.Option.OutputRemindInformation)
                    {
                        this.PrintMessage($"'{StringHelper.HandleSingleQuotationChar(sb.ToString())}'");
                    }
                }

                if (this.Option != null && this.Option.CollectDeclareStatement)
                {
                    this.DeclareStatements.Add(declareCursor);
                }
            }
            else if (statement is OpenCursorStatement openCursor)
            {
                this.AppendLine($"OPEN {openCursor.CursorName};");
            }
            else if (statement is FetchCursorStatement fetchCursor)
            {
                if (fetchCursor.Variables.Count > 0)
                {
                    this.AppendLine($"FETCH {fetchCursor.CursorName} INTO {string.Join(",", fetchCursor.Variables)};");
                }
            }
            else if (statement is CloseCursorStatement closeCursor)
            {
                this.AppendLine($"CLOSE {closeCursor.CursorName};");
            }
            else if (statement is TruncateStatement truncate)
            {
                this.AppendLine($"TRUNCATE TABLE {truncate.TableName};");
            }
            else if (statement is DropStatement drop)
            {
                string objectType = drop.ObjectType.ToString().ToUpper();

                this.AppendLine($"DROP {objectType} {drop.ObjectName.NameWithSchema};");
            }
            else if (statement is RaiseErrorStatement error)
            {
                string code = error.ErrorCode == null ? "-20000" : error.ErrorCode.Symbol;

                this.AppendLine($"RAISE_APPLICATION_ERROR({code},{error.Content});");
            }
            else if (statement is GotoStatement gts)
            {
                if (gts.IsLabel)
                {
                    this.AppendLine($"GOTO {gts.Label};");
                }
                else
                {
                    this.AppendLine($"<<{gts.Label}>>");

                    this.AppendChildStatements(gts.Statements);
                }
            }
            else if (statement is PreparedStatement prepared)
            {
                PreparedStatementType type = prepared.Type;

                if (type == PreparedStatementType.Prepare)
                {
                    if (this.Option.CollectSpecialStatementTypes.Contains(prepared.GetType()))
                    {
                        this.SpecialStatements.Add(prepared);
                    }
                }
                else if (type == PreparedStatementType.Execute)
                {
                    var pre = this.SpecialStatements.FirstOrDefault(item => (item is PreparedStatement) && (item as PreparedStatement).Id.Symbol == prepared.Id.Symbol) as PreparedStatement;

                    string variables = prepared.ExecuteVariables.Count > 0 ? $" USING {(string.Join(",", prepared.ExecuteVariables))}" : "";

                    this.AppendLine($"EXECUTE IMMEDIATE {pre?.FromSqlOrVariable}{variables};");
                }
            }

            return this;
        }

        protected override void BuildSelectStatement(SelectStatement select, bool appendSeparator = true)
        {
            int startIndex = this.Length;

            bool isWith = select.WithStatements != null && select.WithStatements.Count > 0;
            bool hasAssignVariableColumn = this.HasAssignVariableColumn(select);
            string selectColumns = $"SELECT {string.Join(",", select.Columns.Select(item => this.GetNameWithAlias(item)))}";

            bool handled = false;

            if (select.NoTableName && hasAssignVariableColumn)
            {
                foreach (var column in select.Columns)
                {
                    string symbol = column.Symbol;

                    if (this.IsIdentifierNameBeforeEqualMark(symbol))
                    {
                        string[] items = symbol.Split('=');

                        string variable = items[0];
                        string value = string.Join("=", items.Skip(1));

                        value = this.GetSetVariableValue(variable, value);

                        if (!AnalyserHelper.IsSubquery(value))
                        {
                            symbol = $"{variable}:={value}";
                        }
                        else
                        {
                            symbol = StatementScriptBuilderHelper.ConvertToSelectIntoVariable(variable, value);
                        }
                    }

                    this.AppendLine($"{symbol};");
                }

                handled = true;
            }
            else if (!select.NoTableName && hasAssignVariableColumn && (this.RoutineType == RoutineType.PROCEDURE || this.RoutineType == RoutineType.FUNCTION || this.RoutineType == RoutineType.TRIGGER))
            {
                //use "select column1, column2 into var1, var2" instead of "select var1=column1, var2=column2"

                List<string> variables = new List<string>();
                List<string> columnNames = new List<string>();

                foreach (var column in select.Columns)
                {
                    if (column.Symbol.Contains("="))
                    {
                        string[] items = column.Symbol.Split('=');
                        string variable = items[0].Trim();
                        string columName = items[1].Trim();

                        variables.Add(variable);
                        columnNames.Add(columName);
                    }
                }

                this.AppendLine($"SELECT {string.Join(",", columnNames)} INTO {string.Join(",", variables)}");
            }
            else if (!isWith)
            {
                this.AppendLine(selectColumns);
            }

            if (!handled)
            {
                if (select.TableName == null && !select.HasFromItems)
                {
                    select.TableName = new TableName("DUAL");
                }
            }

            if (select.IntoTableName != null)
            {
                this.AppendLine($"INTO {select.IntoTableName}");
            }

            Action appendWith = () =>
            {
                int i = 0;

                foreach (WithStatement withStatement in select.WithStatements)
                {
                    if (i == 0)
                    {
                        this.AppendLine($"WITH {withStatement.Name}");
                    }
                    else
                    {
                        this.AppendLine($",{withStatement.Name}");
                    }

                    this.AppendLine("AS(");

                    this.AppendChildStatements(withStatement.SelectStatements, false);

                    this.AppendLine(")");

                    i++;
                }

            };

            Action appendFrom = () =>
            {
                if (select.HasFromItems)
                {
                    this.BuildSelectStatementFromItems(select);
                }
                else if (select.TableName != null)
                {
                    this.AppendLine($"FROM {this.GetNameWithAlias(select.TableName)}");
                }
            };

            if (isWith)
            {
                appendWith();

                this.AppendLine(selectColumns);
            }

            appendFrom();

            if (select.Where != null)
            {
                this.AppendLine($"WHERE {select.Where}");
            }

            if (select.GroupBy != null && select.GroupBy.Count > 0)
            {
                this.AppendLine($"GROUP BY {string.Join(",", select.GroupBy)}");
            }

            if (select.Having != null)
            {
                this.AppendLine($"HAVING {select.Having}");
            }

            if (select.OrderBy != null && select.OrderBy.Count > 0)
            {
                this.AppendLine($"ORDER BY {string.Join(",", select.OrderBy)}");
            }

            if (select.TopInfo != null)
            {
                this.AppendLine($"FETCH NEXT {select.TopInfo.TopCount} ROWS ONLY");
            }

            if (select.LimitInfo != null)
            {
                this.AppendLine($"OFFSET {select.LimitInfo.StartRowIndex} ROWS FETCH NEXT {select.LimitInfo.RowCount} ROWS ONLY");
            }

            if (select.UnionStatements != null)
            {
                foreach (var union in select.UnionStatements)
                {
                    this.Build(union, false).TrimSeparator();
                }
            }

            if (appendSeparator)
            {
                this.AppendLine(";");
            }

            this.ReplaceTemporaryTableContent(select.TableName, startIndex);
        }

        private string GetUnionTypeName(UnionType unionType)
        {
            switch (unionType)
            {
                case UnionType.UNION_ALL:
                    return "UNION ALL";
                case UnionType.EXCEPT:
                    return nameof(UnionType.MINUS);
                default:
                    return unionType.ToString();
            }
        }

        private void PrintMessage(string content)
        {
            this.AppendLine($"DBMS_OUTPUT.PUT_LINE({content});");
        }

        public string BuildTable(TableInfo table)
        {
            StringBuilder sb = new StringBuilder();

            string tableName = table.Name.Symbol;

            if (!table.IsGlobal)
            {
                tableName = this.GetPrivateTemporaryTableName(tableName);
            }

            sb.AppendLine($"CREATE {(table.IsGlobal ? "GLOBAL" : "PRIVATE")} {(table.IsTemporary ? "TEMPORARY" : "")} TABLE {tableName}(");

            int i = 0;

            foreach (var column in table.Columns)
            {
                sb.AppendLine($"{column.Name.FieldName} {column.DataType}{(i == table.Columns.Count - 1 ? "" : ",")}");
                i++;
            }

            sb.AppendLine(")");

            if (!table.IsGlobal)
            {
                sb.Append("ON COMMIT DROP DEFINITION;");
            }

            return sb.ToString();
        }

        private string GetPrivateTemporaryTableName(string tableName)
        {
            string prefix = "ORA$PTT_";

            if (!tableName.ToUpper().StartsWith(prefix))
            {
                string newTableName = prefix + tableName;

                if (!this.Replacements.ContainsKey(tableName))
                {
                    this.Replacements.Add(tableName, newTableName);
                }

                if (!this.TemporaryTableNames.Contains(tableName))
                {
                    this.TemporaryTableNames.Add(tableName);
                }

                return newTableName;
            }

            return tableName;
        }

        private string GetSetVariableValue(string name, string value)
        {
            if (name != null && value != null && ValueHelper.IsStringValue(value))
            {
                bool isTimestampValue = value.Contains(" ");
                string dateFormat = "'yyyy-MM-dd'";
                string datetimeFormat = "'yyyy-MM-dd HH24:mi:ss'";

                string format = isTimestampValue ? datetimeFormat : dateFormat;

                var declareVariable = this.DeclareStatements.FirstOrDefault(item => (item is DeclareVariableStatement) && (item as DeclareVariableStatement).Name.Symbol?.Trim() == name.Trim()) as DeclareVariableStatement;

                if (declareVariable != null)
                {
                    string dataType = declareVariable.DataType?.Symbol?.ToUpper();

                    if (dataType != null)
                    {
                        if (dataType == "DATE" || dataType.Contains("TIMESTAMP"))
                        {
                            value = $"TO_DATE({value}, {format})";
                        }
                    }
                }
            }

            return value;
        }

        private string GetExecuteImmediateSql(string sql)
        {
            return $"EXECUTE IMMEDIATE  '{StringHelper.HandleSingleQuotationChar(sql)}';";
        }

        private bool IsTemporaryTable(TableName tableName)
        {
            if (tableName == null || tableName.Symbol == null)
            {
                return false;
            }

            string strTableName = tableName.Symbol;

            return this.TemporaryTableNames.Contains(strTableName);
        }

        private void ReplaceTemporaryTableContent(TableName tableName, int startIndex)
        {
            if (this.RoutineType == RoutineType.PROCEDURE && this.IsTemporaryTable(tableName))
            {
                int length = this.Length - startIndex;

                if (length > 0)
                {
                    string content = this.Script.ToString().Substring(startIndex, length);

                    this.Script.Replace(content, this.GetExecuteImmediateSql(content), startIndex, length);
                }
            }
        }
    }
}
