using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace SqlAnalyser.Core
{
    public class PostgreSqlStatementScriptBuilder : StatementScriptBuilder
    {
        public override StatementScriptBuilder Build(Statement statement, bool appendSeparator = true)
        {
            base.Build(statement, appendSeparator);

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
            }
            else if (statement is UpdateStatement update)
            {
                int fromItemsCount = update.FromItems == null ? 0 : update.FromItems.Count;
                TableName tableName = StatementScriptBuilderHelper.GetUpdateSetTableName(update);
                string tableNameOrAlias = tableName?.Symbol;
                bool isCompositeColumnName = StatementScriptBuilderHelper.IsCompositeUpdateSetColumnName(update);

                this.AppendLine($"UPDATE");

                List<TableName> tableNames = new List<TableName>();

                List<string> joins = new List<string>();
                string otherCondition = null;
                string strTableName = null;
                string alias = null;

                if (tableName?.Alias != null)
                {
                    alias = tableName.Alias.Symbol;
                }

                Func<string, bool, string> getNoAliasString = (str, useOldName) =>
                {
                    if (str != null)
                    {
                        return alias == null ? str : str.Replace($"{alias}.", (useOldName ? $"{strTableName}." : ""));
                    }

                    return str;
                };

                Func<string, string> getCleanColumnName = (name) =>
                {
                    if (name != null)
                    {
                        if (name.Contains("."))
                        {
                            name = name.Split('.').Last();
                        }
                    }

                    return name;
                };

                if (update.HasFromItems)
                {
                    int i = 0;

                    foreach (FromItem fromItem in update.FromItems)
                    {
                        bool hasJoin = fromItem.HasJoinItems;

                        string tn = fromItem.TableName?.Symbol;
                        string talias = fromItem.TableName?.Alias?.ToString();

                        if (fromItem.TableName != null)
                        {
                            bool matched = false;

                            if (alias != null)
                            {
                                if (talias == alias)
                                {
                                    matched = true;
                                }
                            }
                            else if (tn == tableNameOrAlias)
                            {
                                matched = true;
                            }

                            if (matched)
                            {
                                tableNames.Add(fromItem.TableName);
                                strTableName = tn;
                                alias = talias;
                            }
                        }

                        if (hasJoin)
                        {
                            int j = 0;

                            foreach (var joinItem in fromItem.JoinItems)
                            {
                                if (j == 0)
                                {
                                    joins.Add($"FROM {joinItem.TableName.NameWithAlias}");
                                    otherCondition = getNoAliasString(joinItem.Condition.ToString(), true);
                                }
                                else
                                {
                                    joins.Add($"{joinItem.Type} JOIN {joinItem.TableName.NameWithAlias} ON {getNoAliasString(joinItem.Condition.ToString(), false)}");
                                }

                                j++;
                            }
                        }
                        else if (fromItemsCount > 0)
                        {
                            string seperator = (joins.Count < fromItemsCount - 2 && fromItemsCount > 2) ? "," : "";

                            if (tn != null)
                            {
                                if (tn != strTableName || (tn == strTableName && alias != talias))
                                {
                                    joins.Add($"{(joins.Count == 0 ? "FROM" : "")} {fromItem.TableName.NameWithAlias}{seperator}");
                                }
                            }
                            else if (fromItem.SubSelectStatement != null)
                            {
                                PostgreSqlStatementScriptBuilder builder = new PostgreSqlStatementScriptBuilder();

                                if (joins.Count == 0)
                                {
                                    builder.Append("FROM ");
                                }

                                string strAlias = fromItem.Alias == null ? "" : fromItem.Alias.Symbol;

                                builder.AppendLine("(");
                                builder.BuildSelectStatement(fromItem.SubSelectStatement, false);
                                builder.Append($") {strAlias}");

                                builder.Append(seperator);

                                joins.Add(builder.ToString());
                            }
                        }

                        i++;
                    }
                }

                if (tableNames.Count == 0 && update.TableNames.Count > 0)
                {
                    tableNames.AddRange(update.TableNames);

                    if (strTableName == null)
                    {
                        strTableName = update.TableNames.FirstOrDefault()?.Symbol;
                    }
                }

                this.Append($" {string.Join(",", tableNames.Select(item => item.NameWithAlias))}", false);

                this.AppendLine("SET");

                if (!isCompositeColumnName)
                {
                    int k = 0;

                    foreach (var item in update.SetItems)
                    {
                        this.Append($"{item.Name}=");

                        this.BuildUpdateSetValue(item);

                        if (k < update.SetItems.Count - 1)
                        {
                            this.Append(",");
                        }

                        this.AppendLine(this.Indent);

                        k++;
                    }

                    joins.ForEach(item => this.AppendLine(item));
                }
                else
                {
                    this.AppendLine(StatementScriptBuilderHelper.ParseCompositeUpdateSet(this, update));

                    return this;
                }

                bool hasCondition = false;

                if (update.Condition != null && update.Condition.Symbol != null)
                {
                    this.AppendLine($"WHERE {update.Condition}");
                    hasCondition = true;
                }

                if (otherCondition != null)
                {
                    this.AppendLine(hasCondition ? $"AND {otherCondition}" : $"WHERE {otherCondition}");
                }

                this.AppendLine(";");
            }
            else if (statement is DeleteStatement delete)
            {
                bool hasJoin = AnalyserHelper.IsFromItemsHaveJoin(delete.FromItems);

                if (!hasJoin)
                {
                    this.AppendLine($"DELETE FROM {this.GetNameWithAlias(delete.TableName)}");
                }
                else
                {
                    this.Append($"DELETE");

                    this.BuildFromItems(delete.FromItems, null, true);
                }

                if (delete.Condition != null)
                {
                    this.AppendLine($"{(hasJoin ? "AND" : "WHERE")} {delete.Condition}");
                }

                this.AppendLine(";");
            }
            else if (statement is DeclareVariableStatement declareVar)
            {
                StringBuilder sb = new StringBuilder();

                string defaultValue = (declareVar.DefaultValue == null ? "" : $" DEFAULT {declareVar.DefaultValue}");

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
                    this.DeclareVariableStatements.Add(declareVar);
                }
            }
            else if (statement is DeclareTableStatement declareTable)
            {
                this.AppendLine(this.BuildTable(declareTable.TableInfo));
            }
            else if (statement is CreateTableStatement createTable)
            {
                this.AppendLine(this.BuildTable(createTable.TableInfo));
            }
            else if (statement is IfStatement @if)
            {
                foreach (IfStatementItem item in @if.Items)
                {
                    if (item.Type == IfStatementType.IF || item.Type == IfStatementType.ELSEIF)
                    {
                        this.Append($"{item.Type} ");

                        this.BuildIfCondition(item);

                        this.AppendLine(" THEN");
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
                if (set.Key != null)
                {
                    if (set.Value != null)
                    {
                        if (set.IsSetUserVariable)
                        {
                            string dataType = AnalyserHelper.GetUserVariableDataType(DatabaseType.Postgres, set.UserVariableDataType);

                            if (!string.IsNullOrEmpty(dataType))
                            {
                                DeclareVariableStatement declareVariable = new DeclareVariableStatement() { Name = set.Key, DataType = new TokenInfo(dataType) };

                                this.DeclareVariableStatements.Add(declareVariable);
                            }
                        }

                        string value = this.GetSetVariableValue(set.Key.Symbol, set.Value.Symbol);

                        this.AppendLine($"{set.Key} := {value};");
                    }
                    else if (set.IsSetCursorVariable && set.ValueStatement != null)
                    {
                        var declareCursorStatement = this.DeclareCursorStatements.FirstOrDefault(item => item.CursorName.Symbol == set.Key.Symbol);

                        if (declareCursorStatement == null)
                        {
                            this.AppendLine($"SET {set.Key} =");

                            this.BuildSelectStatement(set.ValueStatement);
                        }
                        else
                        {
                            declareCursorStatement.SelectStatement = set.ValueStatement;
                        }
                    }
                }
            }
            else if (statement is LoopStatement loop)
            {
                bool isReverse = false;
                bool isForLoop = false;
                bool isIntegerIterate = false;
                string iteratorName = null;

                if (loop.Type == LoopType.LOOP)
                {
                    this.AppendLine("LOOP");
                }
                else if (loop.Type == LoopType.FOR && loop.LoopCursorInfo != null)
                {
                    isForLoop = true;
                    isReverse = loop.LoopCursorInfo.IsReverse;
                    iteratorName = loop.LoopCursorInfo.IteratorName.Symbol;

                    if(loop.LoopCursorInfo.IsIntegerIterate)
                    {
                        isIntegerIterate = true;

                        DeclareVariableStatement declareVariable = new DeclareVariableStatement();
                        declareVariable.Name = loop.LoopCursorInfo.IteratorName;
                        declareVariable.DataType = new TokenInfo("INTEGER");

                        this.DeclareVariableStatements.Add(declareVariable);

                        if (!isReverse)
                        {
                            this.AppendLine($"{iteratorName}:={loop.LoopCursorInfo.StartValue};");
                            this.AppendLine($"WHILE {iteratorName}<={loop.LoopCursorInfo.StopValue} LOOP");
                        }
                        else
                        {
                            this.AppendLine($"{iteratorName}:={loop.LoopCursorInfo.StopValue};");
                            this.AppendLine($"WHILE {iteratorName}>={loop.LoopCursorInfo.StartValue} LOOP");
                        }
                    }                    
                }
                else
                {
                    this.AppendLine($"{loop.Type.ToString()} {loop.Condition} LOOP");
                }

                this.AppendChildStatements(loop.Statements, true);

                if (isForLoop && isIntegerIterate)
                {
                    this.AppendLine($"{iteratorName}:={iteratorName}{(isReverse ? "-" : "+")}1;");
                }

                this.AppendLine("END LOOP;");
            }
            else if (statement is LoopExitStatement loopExit)
            {
                if (!loopExit.IsCursorLoopExit)
                {
                    this.AppendLine($"EXIT WHEN {loopExit.Condition};");
                }
                else
                {
                    this.AppendLine($"EXIT WHEN NOT FOUND;");
                }
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
                string value = @return.Value?.Symbol;

                if (this.RoutineType != RoutineType.PROCEDURE)
                {
                    this.AppendLine($"RETURN {value};");
                }
                else
                {
                    bool isStringValue = ValueHelper.IsStringValue(value);

                    this.PrintMessage(isStringValue ? StringHelper.HandleSingleQuotationChar(value) : value);

                    this.AppendLine("RETURN;");
                }
            }
            else if (statement is PrintStatement print)
            {
                this.PrintMessage(print.Content.Symbol);
            }
            else if (statement is CallStatement call)
            {
                if (!call.IsExecuteSql)
                {
                    this.AppendLine($"CALL {call.Name}({string.Join(",", call.Parameters.Select(item => item.Value?.Symbol?.Split('=')?.LastOrDefault()))});");
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

                        this.AppendLine($"EXECUTE {content}{strUsings};");
                    }
                }
            }
            else if (statement is TransactionStatement transaction)
            {
                TransactionCommandType commandType = transaction.CommandType;

                switch (commandType)
                {
                    case TransactionCommandType.BEGIN:
                        this.AppendLine("START TRANSACTION;");
                        break;
                    case TransactionCommandType.COMMIT:
                        this.AppendLine("COMMIT;");
                        break;
                    case TransactionCommandType.ROLLBACK:
                        this.AppendLine("ROLLBACK;");
                        break;
                }
            }
            else if (statement is BreakStatement @break)
            {
                this.AppendLine("EXIT;");
            }
            else if (statement is LeaveStatement leave)
            {
                this.AppendLine("RETURN;");
            }
            else if (statement is ExceptionStatement exception)
            {
                this.AppendLine("EXCEPTION");

                foreach (ExceptionItem exceptionItem in exception.Items)
                {
                    string name = exceptionItem.Name?.Symbol;

                    this.AppendLine($"WHEN {name} THEN");

                    this.AppendChildStatements(exceptionItem.Statements, true);
                }
            }
            else if (statement is TryCatchStatement tryCatch)
            {
                this.AppendChildStatements(tryCatch.TryStatements, true);

                this.AppendLine("EXCEPTION WHEN OTHERS THEN");

                this.AppendChildStatements(tryCatch.CatchStatements, true);
            }
            else if (statement is DeclareCursorStatement declareCursor)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append($"DECLARE {declareCursor.CursorName} CURSOR{(declareCursor.SelectStatement != null ? " FOR" : "")}");

                if (!(this.Option != null && this.Option.NotBuildDeclareStatement))
                {
                    if (declareCursor.SelectStatement != null)
                    {
                        this.AppendLine(sb.ToString());

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
                    if (!this.DeclareCursorStatements.Any(item => item.CursorName.Symbol == declareCursor.CursorName.Symbol))
                    {
                        this.DeclareCursorStatements.Add(declareCursor);
                    }
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

                this.AppendLine($"DROP {objectType} IF EXISTS {drop.ObjectName.NameWithSchema};");
            }
            else if (statement is RaiseErrorStatement error)
            {
                if (error.Content != null)
                {
                    this.AppendLine($"RAISE EXCEPTION '%',{error.Content};");
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

                    this.AppendLine($"EXECUTE {pre?.FromSqlOrVariable}{variables};");
                }
            }
            else if (statement is GotoStatement gts)
            {
                if (gts.IsLabel)
                {
                    this.AppendLine($"--GOTO-- {gts.Label};");
                }
                else
                {
                    this.AppendLine($"--GOTO--{gts.Label}");

                    this.AppendChildStatements(gts.Statements);
                }
            }

            return this;
        }

        protected override void BuildSelectStatement(SelectStatement select, bool appendSeparator = true)
        {
            bool isCreateTemporaryTable = false;

            TokenInfo intoTableName = AnalyserHelper.GetIntoTableName(select);

            if (intoTableName != null)
            {
                isCreateTemporaryTable = true;

                this.AppendLine($"CREATE TEMPORARY TABLE IF NOT EXISTS {intoTableName} AS (");
            }

            bool isWith = select.WithStatements != null && select.WithStatements.Count > 0;

            string selectColumns = $"SELECT {string.Join(",", select.Columns.Select(item => this.GetNameWithAlias(item)))}";

            if (select.NoTableName && select.Columns.Any(item => AnalyserHelper.IsAssignNameColumn(item)))
            {
                foreach (var column in select.Columns)
                {
                    string symbol = column.Symbol;

                    if (AnalyserHelper.IsAssignNameColumn(column))
                    {
                        string[] items = symbol.Split('=');

                        var values = items.Skip(1);

                        string strValue = "";

                        if (values.Count() == 1)
                        {
                            strValue = this.GetSetVariableValue(items[0], items[1]);
                        }
                        else
                        {
                            strValue = string.Join("=", items.Skip(1));
                        }

                        symbol = $"{items[0]}:={strValue}";
                    }

                    this.AppendLine($"{symbol};");
                }
            }
            else if (!isWith)
            {
                this.AppendLine(selectColumns);
            }

            if (!isCreateTemporaryTable && select.Intos != null && select.Intos.Count > 0)
            {
                this.Append("INTO ");
                this.AppendLine(String.Join(",", select.Intos));
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
                this.AppendLine($"LIMIT {select.TopInfo.TopCount}");
            }

            if (select.LimitInfo != null)
            {
                this.AppendLine($"LIMIT {select.LimitInfo.RowCount} OFFSET {select.LimitInfo.StartRowIndex?.Symbol ?? "0"}");
            }

            if (select.UnionStatements != null)
            {
                foreach (var union in select.UnionStatements)
                {
                    this.Build(union, false).TrimSeparator();
                    this.AppendLine();
                }
            }

            if (isCreateTemporaryTable)
            {
                this.AppendLine(")");
            }

            if (appendSeparator)
            {
                this.AppendLine(";");
            }
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
            this.AppendLine($"RAISE INFO '%',{content};");
        }

        private string GetSetVariableValue(string name, string value)
        {
            if (name != null && value != null && ValueHelper.IsStringValue(value))
            {
                var declareVariable = this.DeclareVariableStatements.FirstOrDefault(item => item.Name.Symbol?.Trim() == name.Trim());

                if (declareVariable != null)
                {
                    string dataType = declareVariable.DataType?.Symbol?.ToUpper();

                    if (dataType != null)
                    {
                        if (dataType == "DATE")
                        {
                            value = $"{value}::DATE";
                        }
                        else if (dataType.Contains("TIMESTAMP"))
                        {
                            value = $"{value}::TIMESTAMP";
                        }
                    }
                }
            }

            return value;
        }

        public string BuildTable(TableInfo table)
        {
            StringBuilder sb = new StringBuilder();

            var columns = table.Columns;
            var selectStatement = table.SelectStatement;

            sb.AppendLine($"CREATE {(table.IsTemporary ? "TEMPORARY" : "")} TABLE {table.Name}{(columns.Count > 0 ? "(" : "AS")}");

            if (columns.Count > 0)
            {
                bool hasTableConstraints = table.HasTableConstraints;

                int i = 0;

                foreach (var column in columns)
                {
                    string name = column.Name.Symbol;
                    string dataType = column.DataType?.Symbol ?? "";
                    string require = column.IsNullable ? " NULL" : " NOT NULL";
                    string seperator = (i == table.Columns.Count - 1 ? (hasTableConstraints ? "," : "") : ",");

                    if (column.IsComputed)
                    {
                        sb.AppendLine($"{name}{dataType}{require} GENERATED ALWAYS AS ({column.ComputeExp}) STORED{seperator}");
                    }
                    else
                    {
                        string identity = column.IsIdentity ? " GENERATED ALWAYS AS IDENTITY" : "";
                        string defaultValue = string.IsNullOrEmpty(column.DefaultValue?.Symbol) ? "" : $" DEFAULT {StringHelper.GetParenthesisedString(column.DefaultValue.Symbol)}";
                        string constraint = this.GetConstriants(column.Constraints, true);
                        string strConstraint = string.IsNullOrEmpty(constraint) ? "" : $" {constraint}";

                        sb.AppendLine($"{name} {column.DataType}{defaultValue}{identity}{require}{strConstraint}{seperator}");
                    }

                    i++;
                }

                if (hasTableConstraints)
                {
                    sb.AppendLine(this.GetConstriants(table.Constraints));
                }

                sb.AppendLine(")");
            }
            else
            {
                PostgreSqlStatementScriptBuilder builder = new PostgreSqlStatementScriptBuilder();

                builder.BuildSelectStatement(selectStatement, false);

                sb.AppendLine(builder.ToString());
            }

            sb.Append(";");

            return sb.ToString();
        }
    }
}
