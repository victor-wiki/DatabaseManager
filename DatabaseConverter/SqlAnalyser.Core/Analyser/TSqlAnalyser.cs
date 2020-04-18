using SqlAnalyser.Model;
using System;
using System.Linq;
using System.Text;
using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SqlAnalyser.Core
{
    public class TSqlAnalyser : SqlAnalyserBase
    {
        private TSqlRuleAnalyser ruleAnalyser = null;

        public override DatabaseType DatabaseType => DatabaseType.SqlServer;

        public TSqlAnalyser()
        {
            this.ruleAnalyser = new TSqlRuleAnalyser();
        }

        public override ViewScript AnalyseView(string content)
        {
            ViewScript view = this.ruleAnalyser.AnalyseView(content);

            return view;
        }

        public override RoutineScript AnalyseProcedure(string content)
        {
            RoutineScript procedure = this.ruleAnalyser.AnalyseProcedure(content);

            procedure.Type = RoutineType.PROCEDURE;

            return procedure;
        }

        public override RoutineScript AnalyseFunction(string content)
        {
            RoutineScript function = this.ruleAnalyser.AnalyseFunction(content);
            function.Type = RoutineType.FUNCTION;

            return function;
        }

        public override TriggerScript AnalyseTrigger(string content)
        {
            TriggerScript trigger = this.ruleAnalyser.AnalyseTrigger(content);

            return trigger;
        }

        public override string GenerateScripts(CommonScript script)
        {
            if (script is RoutineScript routineScript)
            {
                return this.GenerateRoutineScripts(routineScript);
            }
            else if (script is ViewScript viewScript)
            {
                return this.GenearteViewScripts(viewScript);
            }
            else if (script is TriggerScript triggerScript)
            {
                return this.GenearteTriggerScripts(triggerScript);
            }
            else
            {
                throw new NotSupportedException($"Not support generate scripts for type: {script.GetType()}.");
            }
        }

        public string GenerateRoutineScripts(RoutineScript script)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CREATE {script.Type.ToString()} {script.FullName}");

            if (script.Parameters.Count > 0)
            {
                sb.AppendLine("(");

                int i = 0;
                foreach (Parameter parameter in script.Parameters)
                {
                    ParameterType parameterType = parameter.ParameterType;

                    string strParameterType = "";

                    if (parameterType.HasFlag(ParameterType.IN) && parameterType.HasFlag(ParameterType.OUT))
                    {
                        strParameterType = "INOUT";
                    }
                    else if (parameterType != ParameterType.NONE)
                    {
                        strParameterType = parameterType.ToString();
                    }

                    string defaultValue = parameter.DefaultValue == null ? "" : "=" + parameter.DefaultValue;

                    sb.AppendLine($"{strParameterType} {parameter.Name} {parameter.DataType} {defaultValue} {(i == script.Parameters.Count - 1 ? "" : ",")}");

                    i++;
                }

                sb.AppendLine(")");
            }
            else if (script.Type == RoutineType.FUNCTION)
            {
                sb.AppendLine("(");
                sb.AppendLine(")");
            }

            if (script.Type == RoutineType.FUNCTION)
            {
                if (script.ReturnTable == null)
                {
                    sb.AppendLine($"RETURNS {script.ReturnDataType}");
                }
                else
                {
                    sb.AppendLine($"RETURNS {script.ReturnTable.Name}({string.Join(",", script.ReturnTable.Columns.Select(t => $"{t.Name} {t.DataType}")) })");
                }
            }

            sb.AppendLine("AS");

            sb.AppendLine("BEGIN");

            Action<IEnumerable<Statement>> appendStatements = (statements) =>
            {
                foreach (Statement statement in statements)
                {
                    if (statement is WhileStatement @while)
                    {
                        FetchCursorStatement fetchCursorStatement = @while.Statements.FirstOrDefault(item => item is FetchCursorStatement) as FetchCursorStatement;

                        if (fetchCursorStatement != null && !statements.Any(item => item is FetchCursorStatement))
                        {
                            @while.Condition.Symbol = "@@FETCH_STATUS = 0";

                            sb.AppendLine(this.BuildStatement(fetchCursorStatement));
                        }
                    }

                    sb.AppendLine(this.BuildStatement(statement));
                }
            };

            ExceptionStatement exceptionStatement = (ExceptionStatement)script.Statements.FirstOrDefault(item => item is ExceptionStatement);

            if (exceptionStatement != null)
            {
                sb.AppendLine("BEGIN TRY");
                appendStatements(script.Statements.Where(item => !(item is ExceptionStatement)));
                sb.AppendLine("END TRY");

                sb.AppendLine("BEGIN CATCH");

                foreach (ExceptionItem exceptionItem in exceptionStatement.Items)
                {
                    sb.AppendLine($"IF {exceptionItem.Name} = ERROR_PROCEDURE() OR {exceptionItem.Name} = ERROR_NUMBER()");
                    sb.AppendLine("BEGIN");

                    appendStatements(exceptionItem.Statements);

                    sb.AppendLine("END");
                }

                sb.AppendLine("END CATCH");
            }
            else
            {
                appendStatements(script.Statements);
            }

            sb.AppendLine("END");

            return this.FormatScripts(sb.ToString());
        }

        public string GenearteViewScripts(ViewScript script)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CREATE VIEW {script.FullName} AS");

            foreach (Statement statement in script.Statements)
            {
                sb.AppendLine(this.BuildStatement(statement));
            }

            return this.FormatScripts(sb.ToString());
        }

        public string GenearteTriggerScripts(TriggerScript script)
        {
            StringBuilder sb = new StringBuilder();

            string time = script.Time == TriggerTime.BEFORE ? "INSTEAD OF" : script.Time.ToString();
            string events = string.Join(",", script.Events);

            sb.AppendLine($"CREATE TRIGGER {script.FullName} ON {script.TableName}");
            sb.AppendLine($"{time} {events} NOT FOR REPLICATION ");

            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");

            foreach (Statement statement in script.Statements)
            {
                sb.Append(this.BuildStatement(statement));
            }

            sb.AppendLine("END");

            return this.FormatScripts(sb.ToString());
        }

        private string BuildStatement(Statement statement, int level = 0, bool appendSeparator = true)
        {
            StringBuilder sb = new StringBuilder();

            string indent = " ".PadLeft((level + 1) * 2);

            Action<string> append = (value) => { sb.Append($"{indent}{value}"); };

            Action<string> appendLine = (value) => { append(value + Environment.NewLine); };

            Action<IEnumerable<Statement>, bool> appendStatements = (statements, needSeparator) =>
            {
                foreach (Statement st in statements)
                {
                    append(this.BuildStatement(st, level + 1, needSeparator));
                }
            };

            if (statement is SelectStatement select)
            {
                bool isIntoVariable = select.IntoTableName != null && select.IntoTableName.Symbol.StartsWith("@");
                bool isWith = select.WithStatements != null && select.WithStatements.Count > 0;

                string top = select.TopInfo == null ? "" : $" TOP {select.TopInfo.TopCount}{(select.TopInfo.IsPercent ? " PERCENT " : "")}";
                string intoVariable = isIntoVariable ? (select.IntoTableName.Symbol + "=") : "";

                string selectColumns = $"SELECT {top}{intoVariable}{string.Join("," + Environment.NewLine + indent, select.Columns.Select(item => item.ToString()))}";

                if (!isWith)
                {
                    appendLine(selectColumns);
                }

                if (select.IntoTableName != null && !isIntoVariable)
                {
                    appendLine($"INTO {select.IntoTableName.ToString()}");
                }

                Action appendWith = () =>
                  {
                      int i = 0;

                      foreach (WithStatement withStatement in select.WithStatements)
                      {
                          if (i == 0)
                          {
                              appendLine($"WITH {withStatement.Name}");

                              if (withStatement.Columns != null && withStatement.Columns.Count > 0)
                              {
                                  appendLine($"({string.Join(",", withStatement.Columns.Select(item => item))})");
                              }
                          }
                          else
                          {
                              appendLine($",{withStatement.Name}");
                          }

                          appendLine("AS(");

                          appendStatements(withStatement.SelectStatements, false);

                          appendLine(")");

                          i++;
                      }
                  };

                Action appendFrom = () =>
                  {
                      if (select.FromItems != null && select.FromItems.Count > 0)
                      {
                          int i = 0;
                          foreach (FromItem fromItem in select.FromItems)
                          {
                              if (i == 0)
                              {
                                  appendLine($"FROM {fromItem.TableName}");
                              }

                              foreach (JoinItem joinItem in fromItem.JoinItems)
                              {
                                  string condition = joinItem.Condition == null ? "" : $" ON {joinItem.Condition}";

                                  appendLine($"{joinItem.Type} JOIN {joinItem.TableName}{condition}");
                              }

                              i++;
                          }
                      }
                      else if (select.TableName != null)
                      {
                          appendLine($"FROM {select.TableName}");
                      }
                  };

                if (isWith)
                {
                    appendWith();
                    appendLine(selectColumns);
                }

                appendFrom();

                if (select.Where != null)
                {
                    append($"WHERE {select.Where}");
                }

                if (select.GroupBy != null && select.GroupBy.Count > 0)
                {
                    appendLine($"GROUP BY {string.Join(",", select.GroupBy)}");
                }

                if (select.Having != null)
                {
                    appendLine($"HAVING {select.Having}");
                }

                if (select.OrderBy != null && select.OrderBy.Count > 0)
                {
                    appendLine($"ORDER BY {string.Join(",", select.OrderBy)}");
                }

                if (select.LimitInfo != null)
                {
                    //TODO
                }

                if (select.UnionStatements != null)
                {
                    foreach (UnionStatement union in select.UnionStatements)
                    {                       
                        sb.Append(this.BuildStatement(union, level, false).TrimEnd(';'));
                    }
                }

                if (appendSeparator)
                {
                    appendLine(";");
                }
            }
            else if (statement is InsertStatement insert)
            {
                append($"INSERT INTO {insert.TableName}");

                if (insert.Columns.Count > 0)
                {
                    appendLine($"({ string.Join(",", insert.Columns.Select(item => item.ToString()))})");
                }

                if (insert.SelectStatements != null && insert.SelectStatements.Count > 0)
                {
                    appendStatements(insert.SelectStatements, true);
                }
                else
                {
                    appendLine($"VALUES({string.Join(",", insert.Values.Select(item => item))});");
                }
            }
            else if(statement is UnionStatement union)
            {
                appendLine(this.GetUnionTypeName(union.Type));
                sb.AppendLine(this.BuildStatement(union.SelectStatement));
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
                            if (nameValue.Name.Symbol.ToUpper().Trim().StartsWith(alias.ToUpper() + "."))
                            {
                                if (!added)
                                {
                                    tableNames.Add(new TokenInfo(alias));
                                    added = true;
                                }

                                if (nameValue.Value.Symbol?.ToUpper()?.Contains("SELECT") == true)
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

                appendLine($"UPDATE {string.Join(",", tableNames)} SET");

                appendLine(string.Join("," + Environment.NewLine + indent, update.SetItems.Select(item => $"{item.Name}={item.Value}")));

                if (update.FromItems != null && update.FromItems.Count > 0)
                {
                    int i = 0;
                    foreach (FromItem fromItem in update.FromItems)
                    {
                        if (i == 0)
                        {
                            appendLine($"FROM {fromItem.TableName}");
                        }

                        foreach (JoinItem joinItem in fromItem.JoinItems)
                        {
                            string condition = joinItem.Condition == null ? "" : $" ON {joinItem.Condition}";

                            appendLine($"{joinItem.Type} JOIN {joinItem.TableName}{condition}");
                        }

                        i++;
                    }
                }

                if (update.Condition != null && update.Condition.Symbol != null)
                {
                    appendLine($"WHERE {update.Condition}");
                }

                if (update.Option != null)
                {
                    appendLine(update.Option.ToString());
                }

                appendLine(";");
            }
            else if (statement is DeleteStatement delete)
            {
                appendLine($"DELETE FROM {delete.TableName}");

                if (delete.Condition != null)
                {
                    appendLine($"WHERE {delete.Condition}");
                }

                appendLine(";");
            }
            else if (statement is DeclareStatement declare)
            {
                if (declare.Type == DeclareType.Variable)
                {
                    string defaultValue = (declare.DefaultValue == null ? "" : $"= {declare.DefaultValue}");
                    appendLine($"DECLARE {declare.Name} {declare.DataType} {defaultValue};");
                }
                else if (declare.Type == DeclareType.Table)
                {
                    appendLine($"DECLARE {declare.Name} TABLE (");

                    int i = 0;
                    foreach (var column in declare.Table.Columns)
                    {
                        appendLine($"{column.Name} {column.DataType}{(i == declare.Table.Columns.Count - 1 ? "" : ",")}");
                    }

                    appendLine(")");
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

                    appendLine($"SET {set.Key } = {set.Value };");
                }
            }
            else if (statement is IfStatement @if)
            {
                foreach (IfStatementItem item in @if.Items)
                {
                    if (item.Type == IfStatementType.IF)
                    {
                        appendLine($"{item.Type} {item.Condition}");
                    }
                    else
                    {
                        appendLine($"{item.Type}");
                    }

                    appendLine("BEGIN");

                    if (item.Statements.Count > 0)
                    {
                        appendStatements(item.Statements, true);
                    }
                    else
                    {
                        appendLine("PRINT('BLANK!');");
                    }

                    appendLine("END");
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

                append(this.BuildStatement(ifStatement));
            }
            else if (statement is LoopStatement loop)
            {
                appendLine($"WHILE {loop.Condition}");
                appendLine("BEGIN");

                appendStatements(loop.Statements, true);

                appendLine("END");
            }
            else if (statement is WhileStatement @while)
            {
                appendLine($"WHILE { @while.Condition }");
                appendLine("BEGIN");

                appendStatements(@while.Statements, true);

                appendLine("END");
            }
            else if (statement is LoopExitStatement whileExit)
            {
                appendLine($"IF {whileExit.Condition}");
                appendLine("BEGIN");
                appendLine("BREAK");
                appendLine("END");
            }
            else if (statement is TryCatchStatement tryCatch)
            {
                appendLine("BEGIN TRY");
                appendStatements(tryCatch.TryStatements, true);
                appendLine("END TRY");

                appendLine("BEGIN CATCH");
                appendStatements(tryCatch.CatchStatements, true);
                appendLine("END CATCH");

            }
            else if (statement is ReturnStatement @return)
            {
                appendLine($"RETURN {@return.Value};");
            }
            else if (statement is PrintStatement print)
            {
                appendLine($"PRINT {print.Content.Symbol?.Replace("||", "+")};");
            }
            else if (statement is CallStatement execute)
            {
                appendLine($"EXECUTE {execute.Name} {string.Join(",", execute.Arguments)};");
            }
            else if (statement is TransactionStatement transaction)
            {
                TransactionCommandType commandType = transaction.CommandType;

                switch (commandType)
                {
                    case TransactionCommandType.BEGIN:
                        appendLine("BEGIN TRANS");
                        break;
                    case TransactionCommandType.COMMIT:
                        appendLine("COMMIT");
                        break;
                    case TransactionCommandType.ROLLBACK:
                        appendLine("ROLLBACK");
                        break;
                }
            }
            else if (statement is LeaveStatement leave)
            {
                appendLine("RETURN;");
            }
            else if (statement is DeclareCursorStatement declareCursor)
            {
                appendLine($"DECLARE {declareCursor.CursorName} CURSOR FOR");
                append(this.BuildStatement(declareCursor.SelectStatement, level));
            }
            else if (statement is OpenCursorStatement openCursor)
            {
                appendLine($"OPEN {openCursor.CursorName}");
            }
            else if (statement is FetchCursorStatement fetchCursor)
            {
                appendLine($"FETCH NEXT FROM {fetchCursor.CursorName} INTO {string.Join(",", fetchCursor.Variables)}");
            }
            else if (statement is CloseCursorStatement closeCursor)
            {
                appendLine($"CLOSE {closeCursor.CursorName}");

                if (closeCursor.IsEnd)
                {
                    appendLine($"DEALLOCATE {closeCursor.CursorName}");
                }
            }
            else if (statement is DeallocateCursorStatement deallocateCursor)
            {
                appendLine($"DEALLOCATE {deallocateCursor.CursorName}");
            }
            else if (statement is TruncateStatement truncate)
            {
                appendLine($"TRUNCATE TABLE {truncate.TableName}");
            }

            return sb.ToString();
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
            switch(unionType)
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
