using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
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
                bool isIntoVariable = select.IntoTableName != null && select.IntoTableName.Symbol.StartsWith("@");
                bool isWith = select.WithStatements != null && select.WithStatements.Count > 0;

                string top = select.TopInfo == null ? "" : $" TOP {select.TopInfo.TopCount}{(select.TopInfo.IsPercent ? " PERCENT " : "")}";
                string intoVariable = isIntoVariable ? (select.IntoTableName.Symbol + "=") : "";

                string selectColumns = $"SELECT {top}{intoVariable}{string.Join("," + Environment.NewLine + indent, select.Columns.Select(item => item.ToString()))}";

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
                    if (select.FromItems != null && select.FromItems.Count > 0)
                    {
                        this.BuildSelectStatementFromItems(select);
                    }
                    else if (select.TableName != null)
                    {
                        this.AppendLine($"FROM {select.TableName}");
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

                if (select.LimitInfo != null)
                {
                    //NOTE: "OFFSET X ROWS FETCH NEXT Y ROWS ONLY" ony available for SQLServer 2012 and above.
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
            else if (statement is InsertStatement insert)
            {
                this.Append($"INSERT INTO {insert.TableName}", false);

                if (insert.Columns.Count > 0)
                {
                    this.AppendLine($"({ string.Join(",", insert.Columns.Select(item => item.ToString()))})");
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
                List<TokenInfo> tableNames = new List<TokenInfo>();

                #region Handle for mysql and plsql update join
                if (update.TableNames.Count == 1)
                {
                    string[] items = update.TableNames[0].Symbol.Split(' ');

                    if (items.Length > 1)
                    {
                        string alias = items[1];

                        bool added = false;

                        foreach (NameValueItem nameValue in update.SetItems)
                        {
                            if (nameValue.Name.Symbol.Trim().StartsWith(alias + ".", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!added)
                                {
                                    tableNames.Add(new TokenInfo(alias));
                                    added = true;
                                }

                                if (nameValue.Value.Symbol?.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase) > 0)
                                {
                                    string from = nameof(TSqlParser.FROM);
                                    string where = nameof(TSqlParser.WHERE); ;

                                    string oldValue = nameValue.Value.Symbol;
                                    int fromIndex = oldValue.ToUpper().IndexOf(from);

                                    nameValue.Value.Symbol = Regex.Replace(oldValue.Substring(0, fromIndex), "SELECT ", "", RegexOptions.IgnoreCase).Trim(' ', '(');

                                    if (update.FromItems == null || update.FromItems.Count == 0)
                                    {
                                        update.FromItems = new List<FromItem>();

                                        FromItem fromItem = new FromItem() { TableName = update.TableNames[0] };

                                        int whereIndex = oldValue.ToUpper().IndexOf(where);

                                        string tableName = oldValue.Substring(fromIndex + from.Length, whereIndex - (fromIndex + from.Length) - 1);
                                        string condition = oldValue.Substring(whereIndex + where.Length).Trim(')');

                                        fromItem.JoinItems.Add(new JoinItem() { TableName = new TableName(tableName), Condition = new TokenInfo(condition) });

                                        update.FromItems.Add(fromItem);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                if (tableNames.Count == 0)
                {
                    tableNames.AddRange(update.TableNames);
                }

                this.AppendLine($"UPDATE {string.Join(",", tableNames)} SET");

                this.AppendLine(string.Join("," + Environment.NewLine + indent, update.SetItems.Select(item => $"{item.Name}={item.Value}")));

                if (update.FromItems != null && update.FromItems.Count > 0)
                {
                    int i = 0;
                    foreach (FromItem fromItem in update.FromItems)
                    {
                        if (i == 0)
                        {
                            this.AppendLine($"FROM {fromItem.TableName}");
                        }

                        foreach (JoinItem joinItem in fromItem.JoinItems)
                        {
                            string condition = joinItem.Condition == null ? "" : $" ON {joinItem.Condition}";

                            this.AppendLine($"{joinItem.Type} JOIN {joinItem.TableName}{condition}");
                        }

                        i++;
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
                this.AppendLine($"DELETE FROM {delete.TableName}");

                if (delete.Condition != null)
                {
                    this.AppendLine($"WHERE {delete.Condition}");
                }

                this.AppendLine(";");
            }
            else if (statement is DeclareStatement declare)
            {
                if (declare.Type == DeclareType.Variable)
                {
                    string defaultValue = (declare.DefaultValue == null ? "" : $" = {declare.DefaultValue}");
                    this.AppendLine($"DECLARE {declare.Name} {declare.DataType}{defaultValue};");
                }
                else if (declare.Type == DeclareType.Table)
                {
                    this.AppendLine($"DECLARE {declare.Name} TABLE (");

                    int i = 0;
                    foreach (var column in declare.Table.Columns)
                    {
                        this.AppendLine($"{column.Name} {column.DataType}{(i == declare.Table.Columns.Count - 1 ? "" : ",")}");
                    }

                    this.AppendLine(")");
                }
            }
            else if (statement is SetStatement set)
            {
                if (set.Key != null && set.Value != null)
                {
                    TokenInfo valueToken = set.Value;

                    if (valueToken != null)
                    {
                        if (valueToken.Type == TokenType.RoutineName)
                        {
                            this.MakeupRoutineName(valueToken);
                        }
                        else
                        {
                            TokenInfo child = valueToken.Tokens.FirstOrDefault(item => item.Type == TokenType.RoutineName);

                            if (child != null)
                            {
                                this.MakeupRoutineName(valueToken);
                            }
                        }
                    }

                    this.AppendLine($"SET {set.Key } = {set.Value };");
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
                        ifItem.Condition = new TokenInfo($"{variableName}={item.Condition}") { Type = TokenType.Condition };
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
                this.AppendLine($"WHILE { @while.Condition }");
                this.AppendLine("BEGIN");

                this.AppendChildStatements(@while.Statements, true);

                this.AppendLine("END");
            }
            else if (statement is LoopExitStatement whileExit)
            {
                this.AppendLine($"IF {whileExit.Condition}");
                this.AppendLine("BEGIN");
                this.AppendLine("BREAK");
                this.AppendLine("END");
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
                this.AppendLine($"EXECUTE {execute.Name} {string.Join(",", execute.Arguments)};");
            }
            else if (statement is TransactionStatement transaction)
            {
                TransactionCommandType commandType = transaction.CommandType;

                switch (commandType)
                {
                    case TransactionCommandType.BEGIN:
                        this.AppendLine("BEGIN TRANS");
                        break;
                    case TransactionCommandType.COMMIT:
                        this.AppendLine("COMMIT");
                        break;
                    case TransactionCommandType.ROLLBACK:
                        this.AppendLine("ROLLBACK");
                        break;
                }
            }
            else if (statement is LeaveStatement leave)
            {
                this.AppendLine("RETURN;");
            }
            else if (statement is DeclareCursorStatement declareCursor)
            {
                this.AppendLine($"DECLARE {declareCursor.CursorName} CURSOR FOR");
                this.Build(declareCursor.SelectStatement);
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

            return this;
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
    }
}
