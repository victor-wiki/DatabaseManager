using DatabaseInterpreter.Model;
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
                    this.AppendLine(string.Join("," + Environment.NewLine + Indent, update.SetItems.Select(item => $"{item.Name}={item.Value}")));
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
                                this.Append($"FROM {fromItem.TableName.NameWithAlias}");
                            }
                            else
                            {
                                this.Append($",{fromItem.TableName.NameWithAlias}");
                            }

                            if (i == fromItemsCount - 1)
                            {
                                this.AppendLine();
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
                    if (item.Type == IfStatementType.IF)
                    {
                        this.AppendLine($"{item.Type} {item.Condition}");
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
                this.AppendLine($"WHILE {loop.Condition}");
                this.AppendLine("BEGIN");

                this.AppendChildStatements(loop.Statements, true);

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
            else if (statement is PrintStatement print)
            {
                this.AppendLine($"PRINT {print.Content.Symbol?.Replace("||", "+")};");
            }
            else if (statement is CallStatement execute)
            {
                this.AppendLine($"EXECUTE {execute.Name} {string.Join(",", execute.Parameters)};");
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
                this.AppendLine($"TRUNCATE TABLE {truncate.TableName}");
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

            return this;
        }

        private bool IsRoutineName(TokenInfo token)
        {
            var tokenType = token.Type;

            return tokenType == TokenType.RoutineName || tokenType == TokenType.ProcedureName || tokenType == TokenType.FunctionName;
        }

        protected override void BuildSelectStatement(SelectStatement select, bool appendSeparator = true)
        {
            bool isIntoVariable = select.IntoTableName != null && select.IntoTableName.Symbol.StartsWith("@");
            bool isWith = select.WithStatements != null && select.WithStatements.Count > 0;

            string top = select.TopInfo == null ? "" : $" TOP {select.TopInfo.TopCount}{(select.TopInfo.IsPercent ? " PERCENT " : "")}";
            string intoVariable = isIntoVariable ? (select.IntoTableName.Symbol + "=") : "";

            string selectColumns = $"SELECT {top}{intoVariable}{string.Join(",", select.Columns.Select(item => this.GetNameWithAlias(item)))}";

            if (!isWith)
            {
                this.AppendLine(selectColumns);
            }

            if (select.IntoTableName != null && !isIntoVariable)
            {
                this.AppendLine($"INTO {select.IntoTableName.ToString()}");
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
                //NOTE: "OFFSET X ROWS FETCH NEXT Y ROWS ONLY" only available for SQLServer 2012 and above.
                this.AppendLine($"OFFSET {select.LimitInfo.StartRowIndex} ROWS FETCH NEXT {select.LimitInfo.RowCount} ROWS ONLY");
            }

            if (select.UnionStatements != null)
            {
                foreach (UnionStatement union in select.UnionStatements)
                {
                    this.Build(union, false).TrimSeparator();
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

            string trimedTableName = tableName.Trim('[',']');

            if (table.IsTemporary && !trimedTableName.StartsWith("#"))
            {
                string newTableName = "#" + trimedTableName;

                if (!this.Replacements.ContainsKey(tableName))
                {
                    this.Replacements.Add(trimedTableName, newTableName);
                }               
            }

            sb.AppendLine($"CREATE TABLE {tableName}(");

            int i = 0;

            foreach (var column in table.Columns)
            {
                sb.AppendLine($"{column.Name.FieldName} {column.DataType}{(i == table.Columns.Count - 1 ? "" : ",")}");

                i++;
            }

            sb.AppendLine(");");

            return sb.ToString();
        }
    }
}
