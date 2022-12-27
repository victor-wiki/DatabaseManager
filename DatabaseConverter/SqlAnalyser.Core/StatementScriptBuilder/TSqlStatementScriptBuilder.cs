using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using Newtonsoft.Json;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlAnalyser.Core
{
    public class TSqlStatementScriptBuilder : StatementScriptBuilder
    {
        public override StatementScriptBuilder Build(Statement statement, bool appendSeparator = true)
        {
            base.Build(statement, appendSeparator);

            if (statement is SelectStatement select)
            {
                this.BuildSelectStatement(select, appendSeparator);
            }
            else if (statement is InsertStatement insert)
            {
                this.Append($"INSERT INTO {insert.TableName}", false);

                if (insert.Columns.Count > 0)
                {
                    this.AppendLine($"({string.Join(",", insert.Columns.Select(item => item))})");
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
            else if (statement is UnionStatement union)
            {
                this.AppendLine(this.GetUnionTypeName(union.Type));
                this.Build(union.SelectStatement);
            }
            else if (statement is UpdateStatement update)
            {
                int fromItemsCount = update.FromItems == null ? 0 : update.FromItems.Count;
                TableName tableName = StatementScriptBuilderHelper.GetUpdateSetTableName(update);

                if (tableName == null && update.TableNames != null)
                {
                    tableName = update.TableNames.FirstOrDefault();
                }

                bool useAlias = tableName.Alias != null;

                this.AppendLine($"UPDATE {(useAlias ? tableName.Alias.Symbol : tableName.Symbol)}");

                this.Append("SET ");

                if (!StatementScriptBuilderHelper.IsCompositeUpdateSetColumnName(update))
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
                }
                else if (update.SetItems.Count > 0)
                {
                    this.Append(StatementScriptBuilderHelper.ParseCompositeUpdateSet(this, update));

                    return this;
                }

                StringBuilder sb = new StringBuilder();

                if (fromItemsCount > 0)
                {
                    int i = 0;

                    foreach (FromItem fromItem in update.FromItems)
                    {
                        bool hasJoin = fromItem.HasJoinItems;

                        if (hasJoin)
                        {
                            if (i == 0)
                            {
                                this.AppendLine($"FROM {fromItem.TableName.NameWithAlias}");
                            }

                            foreach (JoinItem joinItem in fromItem.JoinItems)
                            {
                                string condition = joinItem.Condition == null ? "" : $" ON {joinItem.Condition}";

                                this.AppendLine($"{joinItem.Type} JOIN {joinItem.TableName.NameWithAlias}{condition}");
                            }
                        }
                        else
                        {
                            if (i == 0)
                            {
                                this.Append($"FROM ");
                            }
                            else
                            {
                                this.Append(",");
                            }

                            if (fromItem.SubSelectStatement != null)
                            {
                                string alias = fromItem.Alias == null ? "" : fromItem.Alias.Symbol;

                                this.AppendLine("(");
                                this.BuildSelectStatement(fromItem.SubSelectStatement, false);
                                this.AppendLine($") {alias}");
                            }
                            else if (fromItem.TableName != null)
                            {
                                this.Append($"{fromItem.TableName.NameWithAlias}");
                            }                           
                        }

                        i++;
                    }
                }
                else
                {
                    if (useAlias)
                    {
                        this.AppendLine($"FROM {tableName.NameWithAlias}");
                    }
                }

                if (update.Condition != null && update.Condition.Symbol != null)
                {
                    this.AppendLine($"WHERE {update.Condition}");
                }

                if (update.Option != null)
                {
                    this.AppendLine(update.Option.ToString());
                }

                this.AppendLine(";");
            }
            else if (statement is DeleteStatement delete)
            {
                bool hasJoin = AnalyserHelper.IsFromItemsHaveJoin(delete.FromItems);

                if (!hasJoin)
                {
                    this.AppendLine($"DELETE FROM {delete.TableName}");
                }
                else
                {
                    this.AppendLine($"DELETE {delete.TableName}");

                    this.BuildFromItems(delete.FromItems);
                }

                if (delete.Condition != null)
                {
                    this.AppendLine($"WHERE {delete.Condition}");
                }

                this.AppendLine(";");
            }
            else if (statement is DeclareVariableStatement declareVar)
            {
                string defaultValue = (declareVar.DefaultValue == null ? "" : $" = {declareVar.DefaultValue}");
                this.AppendLine($"DECLARE {declareVar.Name} {declareVar.DataType}{defaultValue};");
            }
            else if (statement is DeclareTableStatement declareTable)
            {
                TableInfo tableInfo = declareTable.TableInfo;

                this.AppendLine($"DECLARE {tableInfo.Name} TABLE (");

                int i = 0;

                foreach (var column in tableInfo.Columns)
                {
                    this.AppendLine($"{column.Name.FieldName} {column.DataType}{(i == tableInfo.Columns.Count - 1 ? "" : ",")}");
                }

                this.AppendLine(")");
            }
            else if (statement is CreateTableStatement createTable)
            {
                this.AppendLine(this.BuildTable(createTable.TableInfo));
            }
            else if (statement is SetStatement set)
            {
                if (set.Key != null && set.Value != null)
                {
                    if (set.IsSetUserVariable)
                    {
                        string dataType = AnalyserHelper.GetUserVariableDataType(DatabaseType.SqlServer, set.UserVariableDataType);

                        if (!string.IsNullOrEmpty(dataType))
                        {
                            this.AppendLine($"DECLARE {set.Key} {dataType};");
                        }
                    }

                    TokenInfo valueToken = set.Value;

                    if (valueToken != null)
                    {
                        if (this.IsRoutineName(valueToken))
                        {
                            this.MakeupRoutineName(valueToken);
                        }
                        else
                        {
                            TokenInfo child = valueToken.Children.FirstOrDefault(item => this.IsRoutineName(item));

                            if (child != null)
                            {
                                this.MakeupRoutineName(valueToken);
                            }
                        }
                    }

                    this.AppendLine($"SET {set.Key} = {set.Value};");
                }
            }
            if (statement is IfStatement @if)
            {
                foreach (IfStatementItem item in @if.Items)
                {
                    if (item.Type == IfStatementType.IF || item.Type == IfStatementType.ELSEIF)
                    {
                        this.Append($"{item.Type} ");

                        this.BuildIfCondition(item);

                        this.AppendLine();
                    }
                    else
                    {
                        this.AppendLine($"{item.Type}");
                    }

                    this.AppendLine("BEGIN");

                    if (item.Statements.Count > 0)
                    {
                        this.AppendChildStatements(item.Statements, true);
                    }
                    else
                    {
                        this.AppendLine("PRINT('BLANK!');");
                    }

                    this.AppendLine("END");
                    this.AppendLine();
                }
            }
            else if (statement is CaseStatement @case)
            {
                string variableName = @case.VariableName.ToString();

                IfStatement ifStatement = new IfStatement();

                int i = 0;
                foreach (var item in @case.Items)
                {
                    IfStatementItem ifItem = new IfStatementItem();

                    ifItem.Type = i == 0 ? IfStatementType.IF : item.Type;

                    if (item.Type != IfStatementType.ELSE)
                    {
                        ifItem.Condition = new TokenInfo($"{variableName}={item.Condition}") { Type = TokenType.IfCondition };
                    }

                    i++;
                }

                this.Build(ifStatement);
            }
            else if (statement is LoopStatement loop)
            {
                bool isReverse = false;
                bool isForLoop = false;
                bool isIntegerIterate = false;
                string iteratorName = null;

                if(loop.Type != LoopType.FOR)
                {
                    this.AppendLine($"WHILE {loop.Condition}");
                }
                else if(loop.LoopCursorInfo != null)
                {
                    isForLoop = true;
                    isReverse = loop.LoopCursorInfo.IsReverse;

                    iteratorName = "@"+ loop.LoopCursorInfo.IteratorName.Symbol;

                    if(loop.LoopCursorInfo.IsIntegerIterate)
                    {
                        isIntegerIterate = true;
                        this.AppendLine($"DECLARE {iteratorName} INT;");

                        if (!isReverse)
                        {
                            this.AppendLine($"SET {iteratorName}={loop.LoopCursorInfo.StartValue};");
                            this.AppendLine($"WHILE {iteratorName}<={loop.LoopCursorInfo.StopValue}");
                        }
                        else
                        {
                            this.AppendLine($"SET {iteratorName}={loop.LoopCursorInfo.StopValue};");
                            this.AppendLine($"WHILE {iteratorName}>={loop.LoopCursorInfo.StartValue}");
                        }
                    }                   
                }

                this.AppendLine("BEGIN");

                this.AppendChildStatements(loop.Statements, true);

                if(isForLoop && isIntegerIterate)
                {
                    this.AppendLine($"SET {iteratorName}= {iteratorName}{(isReverse? "-":"+")}1;");
                }

                this.AppendLine("END");
            }
            else if (statement is WhileStatement @while)
            {
                this.AppendLine($"WHILE {@while.Condition}");
                this.AppendLine("BEGIN");

                this.AppendChildStatements(@while.Statements, true);

                this.AppendLine("END");
                this.AppendLine();
            }
            else if (statement is LoopExitStatement whileExit)
            {
                if (!whileExit.IsCursorLoopExit)
                {
                    this.AppendLine($"IF {whileExit.Condition}");
                    this.AppendLine("BEGIN");
                    this.AppendLine("BREAK");
                    this.AppendLine("END");
                }
            }
            else if (statement is TryCatchStatement tryCatch)
            {
                this.AppendLine("BEGIN TRY");
                this.AppendChildStatements(tryCatch.TryStatements, true);
                this.AppendLine("END TRY");

                this.AppendLine("BEGIN CATCH");
                this.AppendChildStatements(tryCatch.CatchStatements, true);
                this.AppendLine("END CATCH");

            }
            else if (statement is ReturnStatement @return)
            {
                this.AppendLine($"RETURN {@return.Value};");
            }
            else if (statement is BreakStatement @break)
            {
                this.AppendLine("BREAK;");
            }
            else if (statement is ContinueStatement @continue)
            {
                this.AppendLine("CONTINUE;");
            }
            else if (statement is PrintStatement print)
            {
                this.AppendLine($"PRINT {print.Content.Symbol?.Replace("||", "+")};");
            }
            else if (statement is CallStatement call)
            {
                if (!call.IsExecuteSql)
                {
                    this.AppendLine($"EXECUTE {call.Name} {string.Join(",", call.Parameters.Select(item => item.Value))};");
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

                        string strParameters = usings.Count == 0 ? "" : $",N'', {(string.Join(",", usings.Select(item => $"{item.Value}")))}";

                        this.AppendLine($"EXECUTE SP_EXECUTESQL {content}{strParameters};");
                    }
                }
            }
            else if (statement is TransactionStatement transaction)
            {
                TransactionCommandType commandType = transaction.CommandType;

                string content = transaction.Content == null ? "" : $" {transaction.Content.Symbol}";

                switch (commandType)
                {
                    case TransactionCommandType.BEGIN:
                        this.AppendLine($"BEGIN TRANS{content};");
                        break;
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
            else if (statement is DeclareCursorStatement declareCursor)
            {
                this.AppendLine($"DECLARE {declareCursor.CursorName} CURSOR{(declareCursor.SelectStatement != null ? " FOR" : "")}");

                if (declareCursor.SelectStatement != null)
                {
                    this.Build(declareCursor.SelectStatement);
                }

                this.AppendLine();
            }
            else if (statement is OpenCursorStatement openCursor)
            {
                this.AppendLine($"OPEN {openCursor.CursorName}");
            }
            else if (statement is FetchCursorStatement fetchCursor)
            {
                this.AppendLine($"FETCH NEXT FROM {fetchCursor.CursorName} INTO {string.Join(",", fetchCursor.Variables)}");
            }
            else if (statement is CloseCursorStatement closeCursor)
            {
                this.AppendLine($"CLOSE {closeCursor.CursorName}");

                if (closeCursor.IsEnd)
                {
                    this.AppendLine($"DEALLOCATE {closeCursor.CursorName}");
                }
            }
            else if (statement is DeallocateCursorStatement deallocateCursor)
            {
                this.AppendLine($"DEALLOCATE {deallocateCursor.CursorName}");
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
                string severity = string.IsNullOrEmpty(error.Severity) ? "-1" : error.Severity;
                string state = string.IsNullOrEmpty(error.State) ? "0" : error.State;

                this.AppendLine($"RAISERROR({error.Content},{severity},{state});");
            }
            else if (statement is GotoStatement gts)
            {
                if (gts.IsLabel)
                {
                    this.AppendLine($"GOTO {gts.Label};");
                }
                else
                {
                    this.AppendLine($"{gts.Label}:");

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

                    string variables = prepared.ExecuteVariables.Count > 0 ? $",N'',{(string.Join(",", prepared.ExecuteVariables))}" : "";

                    this.AppendLine($"EXECUTE SP_EXECUTESQL {pre?.FromSqlOrVariable}{variables};");
                }
            }

            return this;
        }

        private bool IsRoutineName(TokenInfo token)
        {
            var tokenType = token.Type;

            return tokenType == TokenType.RoutineName || tokenType == TokenType.ProcedureName || tokenType == TokenType.FunctionName;
        }

        protected override void BuildSelectStatement(SelectStatement select, bool appendSeparator = true)
        {
            TokenInfo intoTableName = AnalyserHelper.GetIntoTableName(select);
            bool isAssignVariable = intoTableName == null && select.Intos != null && select.Intos.Count > 0;

            bool isWith = select.WithStatements != null && select.WithStatements.Count > 0;

            if (select.LimitInfo != null && select.TopInfo == null)
            {
                if (select.LimitInfo.StartRowIndex == null || select.LimitInfo.StartRowIndex.Symbol == "0")
                {
                    select.TopInfo = new SelectTopInfo() { TopCount = select.LimitInfo.RowCount };
                }
            }

            string top = select.TopInfo == null ? "" : $" TOP {select.TopInfo.TopCount}{(select.TopInfo.IsPercent ? " PERCENT " : " ")}";

            string selectColumns = $"SELECT {top}";

            if (!isAssignVariable)
            {
                selectColumns += $"{string.Join(",", select.Columns.Select(item => this.GetNameWithAlias(item)))}";
            }

            if (!isWith)
            {
                this.Append(selectColumns);
            }

            if (intoTableName != null)
            {
                this.AppendLine($"INTO {intoTableName}");
            }
            else if (isAssignVariable && select.Columns.Count == select.Intos.Count)
            {
                List<string> assigns = new List<string>();

                for (int i = 0; i < select.Intos.Count; i++)
                {
                    assigns.Add($"{select.Intos[i]}={select.Columns[i]}");
                }

                this.AppendLine(String.Join(", ", assigns));
            }

            Action appendWith = () =>
            {
                int i = 0;

                foreach (WithStatement withStatement in select.WithStatements)
                {
                    if (i == 0)
                    {
                        this.AppendLine($"WITH {withStatement.Name}");

                        if (withStatement.Columns != null && withStatement.Columns.Count > 0)
                        {
                            this.AppendLine($"({string.Join(",", withStatement.Columns.Select(item => item))})");
                        }
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
                this.Append($"WHERE {select.Where}");
            }

            if (select.GroupBy != null && select.GroupBy.Count > 0)
            {
                this.AppendLine($"GROUP BY {string.Join(",", select.GroupBy.Select(item => item))}");
            }

            if (select.Having != null)
            {
                this.AppendLine($"HAVING {select.Having}");
            }

            if (select.OrderBy != null && select.OrderBy.Count > 0)
            {
                this.AppendLine($"ORDER BY {string.Join(",", select.OrderBy.Select(item => item))}");
            }

            if (select.LimitInfo != null)
            {
                if (select.TopInfo == null)
                {
                    if (select.OrderBy == null)
                    {
                        this.AppendLine("ORDER BY (SELECT 0)");
                    }

                    //NOTE: "OFFSET X ROWS FETCH NEXT Y ROWS ONLY" only available for SQLServer 2012 and above.
                    this.AppendLine($"OFFSET {select.LimitInfo.StartRowIndex?.Symbol ?? "0"} ROWS FETCH NEXT {select.LimitInfo.RowCount} ROWS ONLY");
                }
            }

            if (select.UnionStatements != null)
            {
                foreach (UnionStatement union in select.UnionStatements)
                {
                    this.Build(union, false).TrimSeparator();
                    this.AppendLine();
                }
            }

            if (appendSeparator)
            {
                this.AppendLine(";");
            }
        }

        private void MakeupRoutineName(TokenInfo token)
        {
            string symbol = token.Symbol;
            int index = symbol.IndexOf("(");

            string name = index == -1 ? symbol : symbol.Substring(0, index);

            if (!name.Contains("."))
            {
                token.Symbol = $"dbo." + symbol;
            }
        }

        private string GetUnionTypeName(UnionType unionType)
        {
            switch (unionType)
            {
                case UnionType.UNION_ALL:
                    return "UNION ALL";
                case UnionType.MINUS:
                    return nameof(UnionType.EXCEPT);
                default:
                    return unionType.ToString();
            }
        }

        protected override string GetPivotInItem(TokenInfo token)
        {
            return $"[{this.GetTrimedQuotationValue(token.Symbol)}]";
        }

        public string BuildTable(TableInfo table)
        {
            StringBuilder sb = new StringBuilder();

            string tableName = table.Name.Symbol;

            string trimedTableName = tableName.Trim('[', ']');

            if (table.IsTemporary && !trimedTableName.StartsWith("#"))
            {
                string newTableName = "#" + trimedTableName;

                if (!this.Replacements.ContainsKey(tableName))
                {
                    this.Replacements.Add(trimedTableName, newTableName);
                }
            }

            bool hasColumns = table.Columns.Count > 0;
            bool hasSelect = table.SelectStatement != null;

            if(hasColumns)
            {
                sb.AppendLine($"CREATE TABLE {tableName}(");

                bool hasTableConstraints = table.HasTableConstraints;

                int i = 0;

                foreach (var column in table.Columns)
                {
                    string name = column.Name.Symbol;
                    string dataType = column.DataType?.Symbol ?? "VARCHAR(MAX)";
                    string require = column.IsNullable ? " NULL" : " NOT NULL";
                    string seperator = (i == table.Columns.Count - 1 ? (hasTableConstraints ? "," : "") : ",");

                    bool isComputeExp = column.IsComputed;

                    if(isComputeExp)
                    {
                        sb.Append($"{name} AS ({column.ComputeExp}){seperator}");
                    }
                    else
                    {
                        string identity = column.IsIdentity ? $" IDENTITY({table.IdentitySeed??1},{table.IdentityIncrement??1})" : "";
                        string defaultValue = string.IsNullOrEmpty(column.DefaultValue?.Symbol) ? "" : $" DEFAULT {StringHelper.GetParenthesisedString(column.DefaultValue.Symbol)}";
                        string constraint = this.GetConstriants(column.Constraints, true);
                        string strConstraint = string.IsNullOrEmpty(constraint) ? "" : $" {constraint}";                       

                        sb.AppendLine($"{name} {column.DataType}{identity}{require}{defaultValue}{strConstraint}{seperator}");
                    }

                    i++;
                }

                if (hasTableConstraints)
                {
                    sb.AppendLine(this.GetConstriants(table.Constraints));
                }

                sb.Append(")");
            }
            else if (hasSelect)
            {
                table.SelectStatement.Intos = new List<TokenInfo>();

                table.SelectStatement.Intos.Add(table.Name);

                TSqlStatementScriptBuilder builder = new TSqlStatementScriptBuilder();

                builder.BuildSelectStatement(table.SelectStatement, false);

                sb.AppendLine(builder.ToString());
            }

            sb.AppendLine(";");

            return sb.ToString();
        }
    }
}
