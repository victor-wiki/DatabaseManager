using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Tree;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using RTools_NTS.Util;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using static TSqlParser;

namespace SqlAnalyser.Core
{
    public class TSqlRuleAnalyser : SqlRuleAnalyser
    {
        public override IEnumerable<Type> ParseTableTypes => new List<Type>() { typeof(Full_table_nameContext) };

        public override IEnumerable<Type> ParseColumnTypes => new List<Type>() { typeof(Full_column_nameContext) };

        public override Lexer GetLexer(string content)
        {
            return new TSqlLexer(this.GetCharStreamFromString(content));
        }

        public override Parser GetParser(CommonTokenStream tokenStream)
        {
            return new TSqlParser(tokenStream);
        }

        public Tsql_fileContext GetRootContext(string content, out SqlSyntaxError error)
        {
            error = null;

            TSqlParser parser = this.GetParser(content) as TSqlParser;

            SqlSyntaxErrorListener errorListener = this.AddParserErrorListener(parser);

            Tsql_fileContext context = parser.tsql_file();

            error = errorListener.Error;

            return context;
        }

        public Batch_level_statementContext GetBatchLevelStatementContext(string content, out SqlSyntaxError error)
        {
            error = null;

            Tsql_fileContext rootContext = this.GetRootContext(content, out error);
            var batches = rootContext.batch();

            return rootContext.batch()?.FirstOrDefault()?.batch_level_statement();
        }

        public override AnalyseResult AnalyseProcedure(string content)
        {
            SqlSyntaxError error = null;

            Batch_level_statementContext batchLevelStatement = this.GetBatchLevelStatementContext(content, out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && batchLevelStatement != null)
            {
                RoutineScript script = new RoutineScript() { Type = RoutineType.PROCEDURE };

                Create_or_alter_procedureContext proc = batchLevelStatement.create_or_alter_procedure();

                if (proc != null)
                {
                    #region Name
                    this.SetScriptName(script, proc.func_proc_name_schema().id_());
                    #endregion

                    #region Parameters
                    this.SetRoutineParameters(script, proc.procedure_param());
                    #endregion

                    #region Body

                    this.SetScriptBody(script, proc.sql_clauses());

                    #endregion
                }

                this.ExtractFunctions(script, batchLevelStatement);

                result.Script = script;
            }


            return result;
        }

        public void SetScriptBody(CommonScript script, Sql_clausesContext[] clauses)
        {
            foreach (var clause in clauses)
            {
                script.Statements.AddRange(this.ParseSqlClause(clause));
            }
        }

        public override AnalyseResult AnalyseFunction(string content)
        {
            SqlSyntaxError error = null;

            Batch_level_statementContext batchLevelStatement = this.GetBatchLevelStatementContext(content, out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && batchLevelStatement != null)
            {
                RoutineScript script = new RoutineScript() { Type = RoutineType.FUNCTION };

                Create_or_alter_functionContext func = batchLevelStatement.create_or_alter_function();

                if (func != null)
                {
                    #region Name
                    this.SetScriptName(script, func.func_proc_name_schema().id_());
                    #endregion

                    #region Parameters
                    this.SetRoutineParameters(script, func.procedure_param());
                    #endregion

                    this.SetFunction(script, func);
                }

                this.ExtractFunctions(script, batchLevelStatement);

                result.Script = script;
            }

            return result;
        }

        public void SetFunction(RoutineScript script, Create_or_alter_functionContext func)
        {
            var scalar = func.func_body_returns_scalar();
            var table = func.func_body_returns_table();
            var select = func.func_body_returns_select();

            if (scalar != null)
            {
                script.ReturnDataType = new TokenInfo(scalar.data_type().GetText()) { Type = TokenType.DataType };

                this.SetScriptBody(script, scalar.sql_clauses());

                #region ReturnStatement
                IParseTree t = null;
                for (var i = scalar.children.Count - 1; i >= 0; i--)
                {
                    if (scalar.children[i] is TerminalNodeImpl terminalNode)
                    {
                        if (terminalNode.Symbol.Type == TSqlParser.RETURN)
                        {
                            if (t != null)
                            {
                                ReturnStatement returnStatement = new ReturnStatement();
                                returnStatement.Value = new TokenInfo(t as ParserRuleContext);

                                script.Statements.Add(returnStatement);

                                break;
                            }
                        }
                    }

                    t = scalar.children[i];
                }
                #endregion
            }
            else if (table != null)
            {
                script.ReturnTable = new TemporaryTable();

                foreach (var child in table.children)
                {
                    if (child is TerminalNodeImpl terminalNode)
                    {
                        if (terminalNode.Symbol.Text.StartsWith("@"))
                        {
                            script.ReturnTable.Name = new TokenInfo(terminalNode) { Type = TokenType.VariableName };
                        }
                    }
                    else if (child is Table_type_definitionContext type)
                    {
                        script.ReturnTable.Columns = type.column_def_table_constraints().column_def_table_constraint()
                            .Select(item => new ColumnInfo() { Name = this.ParseColumnName(item) }).ToList();
                    }
                }

                script.Statements.AddRange(table.sql_clauses().SelectMany(item => this.ParseSqlClause(item)));
            }
            else if (select != null)
            {
                script.Statements.AddRange(this.ParseSelectStatement(select.select_statement_standalone()?.select_statement()));
            }
        }

        public override AnalyseResult AnalyseView(string content)
        {
            SqlSyntaxError error = null;

            Batch_level_statementContext batchLevelStatement = this.GetBatchLevelStatementContext(content, out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && batchLevelStatement != null)
            {
                ViewScript script = new ViewScript();

                Create_viewContext view = batchLevelStatement.create_view();

                if (view != null)
                {
                    #region Name
                    this.SetScriptName(script, view.simple_name().id_());
                    #endregion

                    #region Statement

                    foreach (var child in view.children)
                    {
                        if (child is Select_statementContext select)
                        {
                            script.Statements.AddRange(this.ParseSelectStatement(select));
                        }
                        else if (child is Select_statement_standaloneContext standalone)
                        {
                            script.Statements.AddRange(this.ParseSelectStandaloneContext(standalone));
                        }
                    }

                    #endregion
                }

                this.ExtractFunctions(script, batchLevelStatement);

                result.Script = script;
            }

            return result;
        }

        public override AnalyseResult AnalyseTrigger(string content)
        {
            SqlSyntaxError error = null;

            Batch_level_statementContext batchLevelStatement = this.GetBatchLevelStatementContext(content, out error);

            AnalyseResult result = new AnalyseResult() { Error = error };
            TriggerScript script = new TriggerScript();

            if (!result.HasError && batchLevelStatement != null)
            {
                Create_or_alter_dml_triggerContext trigger = batchLevelStatement.create_or_alter_trigger().create_or_alter_dml_trigger();

                if (trigger != null)
                {
                    #region Name                 

                    this.SetScriptName(script, trigger.simple_name().id_());

                    #endregion

                    script.TableName = new TableName(trigger.table_name());

                    foreach (var child in trigger.children)
                    {
                        if (child is TerminalNodeImpl terminalNode)
                        {
                            switch (terminalNode.Symbol.Type)
                            {
                                case TSqlParser.BEFORE:
                                    script.Time = TriggerTime.BEFORE;
                                    break;
                                case TSqlParser.INSTEAD:
                                    script.Time = TriggerTime.INSTEAD_OF;
                                    break;
                                case TSqlParser.AFTER:
                                    script.Time = TriggerTime.AFTER;
                                    break;
                            }
                        }
                        else if (child is Dml_trigger_operationContext operation)
                        {
                            script.Events.Add((TriggerEvent)Enum.Parse(typeof(TriggerEvent), operation.GetText()));
                        }
                    }

                    #region Body                

                    this.SetScriptBody(script, trigger.sql_clauses());

                    #endregion
                }

                this.ExtractFunctions(script, batchLevelStatement);

                result.Script = script;
            }

            return result;
        }

        public List<Statement> ParseSqlClause(Sql_clausesContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var bc in node.children)
            {
                if (bc is Another_statementContext another)
                {
                    statements.AddRange(this.ParseAnotherStatement(another));
                }
                else if (bc is Dml_clauseContext dml)
                {
                    statements.AddRange(this.ParseDmlStatement(dml));
                }
                else if (bc is Ddl_clauseContext ddl)
                {
                    statements.AddRange(this.ParseDdlStatement(ddl));
                }
                else if (bc is Cfl_statementContext cfl)
                {
                    statements.AddRange(this.ParseCflStatement(cfl));
                }
            }

            return statements;
        }

        public List<Statement> ParseDmlStatement(Dml_clauseContext node)
        {
            List<Statement> statements = new List<Statement>();

            if (node.children != null)
            {
                foreach (var child in node.children)
                {
                    if (child is Select_statement_standaloneContext selectStandalone)
                    {
                        statements.AddRange(this.ParseSelectStandaloneContext(selectStandalone));
                    }
                    else if (child is Select_statementContext select)
                    {
                        statements.AddRange(this.ParseSelectStatement(select));
                    }

                    if (child is Insert_statementContext insert)
                    {
                        statements.Add(this.ParseInsertStatement(insert));
                    }
                    else if (child is Update_statementContext update)
                    {
                        statements.Add(this.ParseUpdateStatement(update));
                    }
                    else if (child is Delete_statementContext delete)
                    {
                        statements.Add(this.ParseDeleteStatement(delete));
                    }
                }
            }

            return statements;
        }

        public List<Statement> ParseDdlStatement(Ddl_clauseContext node)
        {
            List<Statement> statements = new List<Statement>();

            if (node.children != null)
            {
                Action<DatabaseObjectType, TokenType, ParserRuleContext[]> addDropStatement = (objType, tokenType, objNames) =>
                {
                    if (objNames != null)
                    {
                        foreach (var objName in objNames)
                        {
                            DropStatement dropStatement = new DropStatement();
                            dropStatement.ObjectType = objType;
                            dropStatement.ObjectName = new NameToken(objName) { Type = tokenType };

                            statements.Add(dropStatement);
                        }
                    }
                };

                foreach (var child in node.children)
                {
                    if (child is Truncate_tableContext truncate)
                    {
                        TruncateStatement truncateStatement = new TruncateStatement();

                        truncateStatement.TableName = this.ParseTableName(truncate.table_name());

                        statements.Add(truncateStatement);
                    }
                    else if (child is Drop_tableContext drop_Table)
                    {
                        addDropStatement(DatabaseObjectType.Table, TokenType.TableName, drop_Table.table_name());
                    }
                    else if (child is Drop_viewContext drop_View)
                    {
                        addDropStatement(DatabaseObjectType.View, TokenType.ViewName, drop_View.simple_name());
                    }
                    else if (child is Drop_functionContext drop_Function)
                    {
                        addDropStatement(DatabaseObjectType.Function, TokenType.FunctionName, drop_Function.func_proc_name_schema());
                    }
                    else if (child is Drop_procedureContext drop_Procedure)
                    {
                        addDropStatement(DatabaseObjectType.Procedure, TokenType.ProcedureName, drop_Procedure.func_proc_name_schema());
                    }
                    else if (child is Drop_triggerContext drop_Trigger)
                    {
                        addDropStatement(DatabaseObjectType.Trigger, TokenType.TriggerName, drop_Trigger.drop_dml_trigger()?.simple_name());
                    }
                    else if (child is Drop_typeContext drop_Type)
                    {
                        addDropStatement(DatabaseObjectType.Type, TokenType.TypeName, new ParserRuleContext[] { drop_Type.simple_name() });
                    }
                    else if (child is Drop_sequenceContext drop_Sequence)
                    {
                        addDropStatement(DatabaseObjectType.Sequence, TokenType.SequenceName, new ParserRuleContext[] { drop_Sequence.sequence_name });
                    }
                }
            }

            return statements;
        }

        public InsertStatement ParseInsertStatement(Insert_statementContext node)
        {
            InsertStatement statement = new InsertStatement();

            foreach (var child in node.children)
            {
                if (child is Ddl_objectContext table)
                {
                    statement.TableName = this.ParseTableName(table);
                }
                else if (child is Column_name_listContext columns)
                {
                    statement.Columns = columns.id_().Select(item => this.ParseColumnName(item)).ToList();
                }
                else if (child is Insert_column_name_listContext insertColumns)
                {
                    statement.Columns = insertColumns.insert_column_id().Select(item => this.ParseColumnName(item)).ToList();
                }
                else if (child is Insert_statement_valueContext values)
                {
                    var tableValues = values.table_value_constructor();
                    var derivedTable = values.derived_table();

                    if (tableValues != null)
                    {
                        statement.Values = tableValues.expression_list().SelectMany(item => item.expression().Select(t => new TokenInfo(t) { Type = TokenType.InsertValue })).ToList();
                    }
                    else if (derivedTable != null)
                    {
                        statement.SelectStatements = new List<SelectStatement>();

                        statement.SelectStatements.AddRange(this.ParseSelectStatement(derivedTable.subquery().FirstOrDefault()?.select_statement()));
                    }
                }
            }

            return statement;
        }

        public UpdateStatement ParseUpdateStatement(Update_statementContext node)
        {
            UpdateStatement statement = new UpdateStatement();

            Ddl_objectContext name = node.ddl_object();

            statement.TableNames.Add(this.ParseTableName(name));

            foreach (var ele in node.update_elem())
            {
                var valExp = ele.expression();
                var value = this.CreateToken(valExp, TokenType.UpdateSetValue);

                statement.SetItems.Add(new NameValueItem()
                {
                    Name = new TokenInfo(ele.full_column_name()) { Type = TokenType.ColumnName },
                    Value = value
                });

                if (value.Symbol?.StartsWith("@") == false)
                {
                    this.AddChildTableAndColumnNameToken(valExp, value);
                }
            }

            Table_sourcesContext fromTable = node.table_sources();

            if (fromTable != null)
            {
                statement.FromItems = this.ParseTableScources(fromTable);
            }

            statement.Condition = this.ParseCondition(node.search_condition());

            return statement;
        }

        public List<FromItem> ParseTableScources(Table_sourcesContext node)
        {
            List<FromItem> fromItems = new List<FromItem>();

            foreach (var child in node.children)
            {
                if (child is Table_sourceContext ts)
                {
                    FromItem fromItem = new FromItem();

                    Table_source_item_joinedContext tsi = ts.table_source_item_joined();

                    Table_source_itemContext fromTable = tsi.table_source_item();

                    List<Join_partContext> joinParts = new List<Join_partContext>();

                    bool asWhole = false;

                    if (fromTable != null)
                    {
                        As_table_aliasContext alias = fromTable.as_table_alias();

                        if (alias != null)
                        {
                            fromItem.Alias = new TokenInfo(alias.table_alias()) { Type = TokenType.Alias };
                        }

                        Derived_tableContext derivedTable = fromTable.derived_table();

                        if (derivedTable != null)
                        {
                            fromItem.SubSelectStatement = this.ParseDerivedTable(derivedTable);
                        }
                        else
                        {
                            fromItem.TableName = this.ParseTableName(fromTable);
                        }
                    }
                    else
                    {
                        asWhole = true;

                        Table_source_item_joinedContext joined = tsi.table_source_item_joined();

                        fromItem.TableName = new TableName(joined);

                        this.AddChildTableAndColumnNameToken(joined, fromItem.TableName);
                    }

                    Join_partContext[] joins = tsi.join_part();

                    if (joins != null)
                    {
                        joinParts.AddRange(joins);
                    }

                    foreach (Join_partContext join in joinParts)
                    {
                        var tsij = join.join_on()?.table_source()?.table_source_item_joined();

                        if (!asWhole && (tsij.table_source_item_joined() != null || tsij.table_source_item()?.derived_table() != null))
                        {
                            asWhole = true;
                        }

                        List<JoinItem> joinItems = this.ParseJoin(join, asWhole);

                        if (joinItems.Count > 1)
                        {
                            for (int i = joinItems.Count - 1; i > 0; i--)
                            {
                                JoinItem currentJoinItem = joinItems[i];

                                if (i - 1 > 0)
                                {
                                    JoinItem previousJoinItem = joinItems[i - 1];

                                    TableName previousJoinTableName = new TableName(previousJoinItem.TableName.Symbol);
                                    ObjectHelper.CopyProperties(previousJoinItem.TableName, previousJoinTableName);

                                    TableName currentJoinTableName = new TableName(currentJoinItem.TableName.Symbol);
                                    ObjectHelper.CopyProperties(currentJoinItem.TableName, currentJoinTableName);

                                    joinItems[i - 1].TableName = currentJoinTableName;
                                    joinItems[i].TableName = previousJoinTableName;
                                }
                            }
                        }

                        fromItem.JoinItems.AddRange(joinItems);
                    }

                    fromItems.Add(fromItem);
                }
            }

            return fromItems;
        }

        public List<JoinItem> ParseJoin(Join_partContext node, bool asWhole = false)
        {
            List<JoinItem> joinItems = new List<JoinItem>();

            JoinItem joinItem = new JoinItem();

            Join_onContext joinOn = node.join_on();

            if (joinOn != null)
            {
                foreach (var child in joinOn.children)
                {
                    if (child is TerminalNodeImpl terminalNode)
                    {
                        int type = terminalNode.Symbol.Type;

                        switch (type)
                        {
                            case TSqlParser.INNER:
                                joinItem.Type = JoinType.INNER;
                                break;
                            case TSqlParser.LEFT:
                                joinItem.Type = JoinType.LEFT;
                                break;
                            case TSqlParser.RIGHT:
                                joinItem.Type = JoinType.RIGHT;
                                break;
                            case TSqlParser.FULL:
                                joinItem.Type = JoinType.FULL;
                                break;
                            case TSqlParser.CROSS:
                                joinItem.Type = JoinType.CROSS;
                                break;
                            case TSqlParser.PIVOT:
                                joinItem.Type = JoinType.PIVOT;
                                break;
                            case TSqlParser.UNPIVOT:
                                joinItem.Type = JoinType.UNPIVOT;
                                break;
                        }
                    }
                }
            }

            Table_sourceContext tableSoure = joinOn?.table_source();
            Pivot_clauseContext pivot = node.pivot()?.pivot_clause();
            Unpivot_clauseContext unpivot = node.unpivot()?.unpivot_clause();

            As_table_aliasContext alias = tableSoure?.table_source_item_joined()?.table_source_item()?.as_table_alias();

            if (alias != null)
            {
                joinItem.Alias = new TokenInfo(alias.table_alias());
            }

            joinItems.Add(joinItem);

            if (tableSoure != null)
            {
                joinItem.TableName = asWhole ? new TableName(tableSoure) : this.ParseTableName(tableSoure);
                joinItem.Condition = this.ParseCondition(joinOn.search_condition());

                if (!asWhole)
                {
                    Table_source_item_joinedContext join = tableSoure.table_source_item_joined();

                    if (join != null)
                    {
                        Join_partContext[] joinParts = join.join_part();

                        List<JoinItem> childJoinItems = joinParts.SelectMany(item => this.ParseJoin(item)).ToList();

                        joinItems.AddRange(childJoinItems);
                    }
                }
                else
                {
                    #region handle alias
                    var ts = tableSoure.table_source_item_joined()?.table_source_item();
                    var derivedTable = ts?.derived_table();
                    var derivedTableAlias = ts?.as_table_alias();

                    if (derivedTable != null && derivedTableAlias != null)
                    {
                        joinItem.TableName = new TableName(derivedTable) { StartIndex = derivedTable.Start.StartIndex - 1, StopIndex = derivedTable.Stop.StopIndex + 1 };
                        joinItem.TableName.Alias = new TokenInfo(derivedTableAlias);
                    }
                    #endregion

                    this.AddChildTableAndColumnNameToken(tableSoure, joinItem.TableName);
                }
            }
            else if (pivot != null)
            {
                joinItem.PivotItem = this.ParsePivot(pivot);
            }
            else if (unpivot != null)
            {
                joinItem.UnPivotItem = this.ParseUnPivot(unpivot);
            }

            return joinItems;
        }

        public PivotItem ParsePivot(Pivot_clauseContext node)
        {
            PivotItem pivotItem = new PivotItem();

            Aggregate_windowed_functionContext function = node.aggregate_windowed_function();

            pivotItem.AggregationFunctionName = new TokenInfo(function.children[0] as TerminalNodeImpl);
            pivotItem.AggregatedColumnName = this.ParseColumnName(function.all_distinct_expression()?.expression());
            pivotItem.ColumnName = this.ParseColumnName(node.full_column_name());
            pivotItem.Values = node.column_alias_list().column_alias().Select(item => new TokenInfo(item)).ToList();

            return pivotItem;
        }

        public UnPivotItem ParseUnPivot(Unpivot_clauseContext node)
        {
            UnPivotItem unpivotItem = new UnPivotItem();
            unpivotItem.ValueColumnName = this.ParseColumnName(node.expression().full_column_name());
            unpivotItem.ForColumnName = this.ParseColumnName(node.full_column_name());
            unpivotItem.InColumnNames = node.full_column_name_list().full_column_name().Select(item => this.ParseColumnName(item)).ToList();

            return unpivotItem;
        }

        public SelectStatement ParseDerivedTable(Derived_tableContext node)
        {
            SelectStatement statement = new SelectStatement();

            foreach (var child in node.children)
            {
                if (child is SubqueryContext subquery)
                {
                    statement = this.ParseSelectStatement(subquery.select_statement()).FirstOrDefault();
                }
            }

            return statement;
        }

        public DeleteStatement ParseDeleteStatement(Delete_statementContext node)
        {
            DeleteStatement statement = new DeleteStatement();

            statement.TableName = this.ParseTableName(node.delete_statement_from().ddl_object());

            Table_sourcesContext fromTable = node.table_sources();

            if (fromTable != null)
            {
                statement.FromItems = this.ParseTableScources(fromTable);

                if (!AnalyserHelper.IsFromItemsHaveJoin(statement.FromItems) && statement.FromItems.Count > 0)
                {
                    statement.TableName = statement.FromItems[0].TableName;
                }
            }

            statement.Condition = this.ParseCondition(node.search_condition());

            return statement;
        }

        public List<Statement> ParseCflStatement(Cfl_statementContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var child in node.children)
            {
                if (child is If_statementContext @if)
                {
                    statements.Add(this.ParseIfStatement(@if));
                }
                if (child is While_statementContext @while)
                {
                    statements.Add(this.ParseWhileStatement(@while));
                }
                else if (child is Block_statementContext block)
                {
                    foreach (var bc in block.children)
                    {
                        if (bc is Sql_clausesContext clauses)
                        {
                            statements.AddRange(this.ParseSqlClause(clauses));
                        }
                    }
                }
                else if (child is Return_statementContext @return)
                {
                    statements.Add(this.ParseReturnStatement(@return));
                }
                else if (child is Try_catch_statementContext trycatch)
                {
                    statements.Add(this.ParseTryCatchStatement(trycatch));
                }
                else if (child is Print_statementContext print)
                {
                    statements.Add(this.ParsePrintStatement(print));
                }
            }

            return statements;
        }

        public IfStatement ParseIfStatement(If_statementContext node)
        {
            IfStatement statement = new IfStatement();

            IfStatementItem ifItem = new IfStatementItem() { Type = IfStatementType.IF };
            IfStatementItem elseItem = null;

            bool isElse = false;

            foreach (var child in node.children)
            {
                if (child is TerminalNodeImpl terminalNode)
                {
                    int type = terminalNode.Symbol.Type;

                    if (type == TSqlParser.ELSE)
                    {
                        isElse = true;
                    }
                }
                if (child is Search_conditionContext condition)
                {
                    ifItem.Condition = this.ParseCondition(condition);
                }
                else if (child is Sql_clausesContext clause)
                {
                    List<Statement> statements = this.ParseSqlClause(clause);

                    if (!isElse)
                    {
                        ifItem.Statements.AddRange(statements);
                    }
                    else
                    {
                        if (elseItem == null)
                        {
                            elseItem = new IfStatementItem() { Type = IfStatementType.ELSE };

                            elseItem.Statements.AddRange(statements);
                        }
                    }
                }
            }

            statement.Items.Add(ifItem);

            if (elseItem != null)
            {
                statement.Items.Add(elseItem);
            }

            return statement;
        }

        public WhileStatement ParseWhileStatement(While_statementContext node)
        {
            WhileStatement statement = new WhileStatement();

            foreach (var child in node.children)
            {
                if (child is Search_conditionContext condition)
                {
                    statement.Condition = this.ParseCondition(condition);
                }
                else if (child is Sql_clausesContext clause)
                {
                    statement.Statements.AddRange(this.ParseSqlClause(clause));
                }
            }

            return statement;
        }

        public List<Statement> ParseAnotherStatement(Another_statementContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var child in node.children)
            {
                if (child is Declare_statementContext declare)
                {
                    statements.AddRange(this.ParseDeclareStatement(declare));
                }
                else if (child is Set_statementContext set)
                {
                    statements.Add(this.ParseSetStatement(set));
                }
                else if (child is Execute_statementContext execute)
                {
                    statements.Add(this.ParseExecuteStatement(execute));
                }
                else if (child is Transaction_statementContext transaction)
                {
                    statements.Add(this.ParseTransactionStatment(transaction));
                }
                else if (child is Cursor_statementContext cursor)
                {
                    statements.Add(this.ParseCursorStatement(cursor));
                }
            }

            return statements;
        }

        public List<Statement> ParseDeclareStatement(Declare_statementContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var dc in node.children)
            {
                if (dc is TerminalNodeImpl impl)
                {
                    if (impl.GetText().StartsWith("@"))
                    {
                        DeclareStatement declareStatement = new DeclareStatement();

                        declareStatement.Name = new TokenInfo(impl) { Type = TokenType.VariableName };
                        declareStatement.DataType = new TokenInfo(node.table_name()) { Type = TokenType.DataType };

                        statements.Add(declareStatement);
                    }
                }
                else if (dc is Declare_localContext local)
                {
                    DeclareStatement declareStatement = new DeclareStatement();

                    declareStatement.Name = new TokenInfo(local.LOCAL_ID()) { Type = TokenType.VariableName };
                    declareStatement.DataType = new TokenInfo(local.data_type().GetText()) { Type = TokenType.DataType };

                    var expression = local.expression();

                    if (expression != null)
                    {
                        declareStatement.DefaultValue = new TokenInfo(expression);
                    }

                    statements.Add(declareStatement);
                }
                else if (dc is Declare_cursorContext cursor)
                {
                    statements.Add(this.ParseDeclareCursor(cursor));
                }
                else if (dc is Table_type_definitionContext table)
                {
                    DeclareStatement declareStatement = new DeclareStatement();

                    declareStatement.Type = DeclareType.Table;
                    declareStatement.Name = new TokenInfo(node.LOCAL_ID()) { Type = TokenType.VariableName };
                    declareStatement.Table = new TemporaryTable()
                    {
                        Name = declareStatement.Name,
                        Columns = table.column_def_table_constraints().column_def_table_constraint()
                        .Select(item => new ColumnInfo() { Name = this.ParseColumnName(item) }).ToList()
                    };

                    statements.Add(declareStatement);
                }
            }

            return statements;
        }

        public void SetScriptName(CommonScript script, Id_Context[] ids)
        {
            var name = ids.Last();

            script.Name = new TokenInfo(name);

            if (ids.Length > 1)
            {
                script.Schema = ids.First().GetText();
            }
        }

        public void SetParameterType(Parameter parameterInfo, IList<IParseTree> nodes)
        {
            foreach (var child in nodes)
            {
                if (child is TerminalNodeImpl terminalNode)
                {
                    if (terminalNode.Symbol.Type == TSqlParser.OUT || terminalNode.Symbol.Type == TSqlParser.OUTPUT)
                    {
                        parameterInfo.ParameterType = ParameterType.OUT;
                    }
                }
            }
        }

        public void SetRoutineParameters(RoutineScript script, Procedure_paramContext[] parameters)
        {
            if (parameters != null)
            {
                foreach (Procedure_paramContext parameter in parameters)
                {
                    Parameter parameterInfo = new Parameter();

                    parameterInfo.Name = new TokenInfo(parameter.children[0] as TerminalNodeImpl) { Type = TokenType.ParameterName };

                    parameterInfo.DataType = new TokenInfo(parameter.data_type().GetText()) { Type = TokenType.DataType };

                    var defaultValue = parameter.default_value();

                    if (defaultValue != null)
                    {
                        parameterInfo.DefaultValue = new TokenInfo(defaultValue);
                    }

                    this.SetParameterType(parameterInfo, parameter.children);

                    script.Parameters.Add(parameterInfo);
                }
            }
        }

        public List<SelectStatement> ParseSelectStandaloneContext(Select_statement_standaloneContext node)
        {
            List<SelectStatement> statements = new List<SelectStatement>();

            List<WithStatement> withStatements = null;

            foreach (var child in node.children)
            {
                if (child is Select_statementContext select)
                {
                    statements.AddRange(this.ParseSelectStatement(select));
                }
                else if (child is With_expressionContext with)
                {
                    withStatements = this.ParseWithStatement(with);
                }
            }

            if (statements.Count > 0)
            {
                statements.First().WithStatements = withStatements;
            }

            return statements;
        }

        public List<SelectStatement> ParseSelectStatement(Select_statementContext node)
        {
            List<SelectStatement> statements = new List<SelectStatement>();

            SelectStatement selectStatement = null;

            List<TokenInfo> orderbyList = new List<TokenInfo>();
            TokenInfo option = null;
            SelectLimitInfo selectLimitInfo = null;

            foreach (var child in node.children)
            {
                if (child is Query_expressionContext query)
                {
                    foreach (var qc in query.children)
                    {
                        if (qc is Query_specificationContext specification)
                        {
                            selectStatement = this.ParseQuerySpecification(specification);
                        }
                        else if (qc is Sql_unionContext union)
                        {
                            if (selectStatement.UnionStatements == null)
                            {
                                selectStatement.UnionStatements = new List<UnionStatement>();
                            }

                            selectStatement.UnionStatements.Add(this.ParseUnionSatement(union));
                        }
                        else if (qc is Query_expressionContext exp)
                        {
                            Query_specificationContext querySpec = exp.query_specification();

                            if (querySpec != null)
                            {
                                selectStatement = this.ParseQuerySpecification(querySpec);
                            }
                        }
                    }

                    if (selectStatement != null)
                    {
                        statements.Add(selectStatement);
                    }
                }
                else if (child is Select_order_by_clauseContext order)
                {
                    bool isLimit = false;
                    int limitKeyword = 0;

                    foreach (var oc in order.children)
                    {
                        if (oc is Order_by_clauseContext orderByClause)
                        {
                            var expressions = orderByClause.order_by_expression();

                            if (expressions != null)
                            {
                                orderbyList.AddRange(expressions.Select(item => this.CreateToken(item, TokenType.OrderBy)));
                            }
                        }
                        else if (oc is Order_by_expressionContext orderByExp)
                        {
                            orderbyList.Add(this.CreateToken(orderByExp, TokenType.OrderBy));
                        }
                        else if (oc is TerminalNodeImpl terminalNode)
                        {
                            if ((limitKeyword = terminalNode.Symbol.Type) == TSqlParser.OFFSET)
                            {
                                isLimit = true;
                            }
                        }
                        else if (oc is ExpressionContext exp)
                        {
                            if (isLimit)
                            {
                                if (selectLimitInfo == null)
                                {
                                    selectLimitInfo = new SelectLimitInfo();
                                }

                                if (limitKeyword == TSqlParser.OFFSET)
                                {
                                    selectLimitInfo.StartRowIndex = new TokenInfo(exp);
                                }
                                else if (limitKeyword == TSqlParser.NEXT)
                                {
                                    selectLimitInfo.RowCount = new TokenInfo(exp);
                                }
                            }
                        }
                    }
                }
                else if (child is Option_clauseContext opt)
                {
                    option = new TokenInfo(opt) { Type = TokenType.Option };
                }

                if (selectStatement != null)
                {
                    if (orderbyList.Count > 0)
                    {
                        selectStatement.OrderBy = orderbyList;
                    }

                    if (selectLimitInfo != null)
                    {
                        selectStatement.LimitInfo = selectLimitInfo;
                    }

                    selectStatement.Option = option;
                }
            }

            return statements;
        }

        public List<WithStatement> ParseWithStatement(With_expressionContext node)
        {
            List<WithStatement> statements = new List<WithStatement>();

            var tables = node.common_table_expression();

            if (tables != null)
            {
                foreach (Common_table_expressionContext table in tables)
                {
                    WithStatement statement = new WithStatement();

                    statement.Name = new TableName(table.id_()) { Type = TokenType.General };
                    Column_name_listContext cols = table.column_name_list();

                    if (cols != null)
                    {
                        statement.Columns = cols.id_().Select(item => this.ParseColumnName(item)).ToList();
                    }

                    statement.SelectStatements = this.ParseSelectStatement(table.select_statement());

                    statements.Add(statement);
                }
            }

            return statements;
        }

        public SelectStatement ParseQuerySpecification(Query_specificationContext node)
        {
            SelectStatement statement = new SelectStatement();

            int terminalNodeType = 0;
            foreach (var child in node.children)
            {
                if (child is Select_listContext list)
                {
                    statement.Columns.AddRange(list.select_list_elem().Select(item => this.ParseColumnName(item)));
                }
                else if (child is TerminalNodeImpl terminalNode)
                {
                    terminalNodeType = terminalNode.Symbol.Type;
                    if (terminalNodeType == TSqlParser.INTO)
                    {
                        statement.IntoTableName = new TokenInfo(node.table_name()) { Type = TokenType.TableName };
                    }
                }
                else if (child is Table_sourcesContext table)
                {
                    statement.TableName = this.ParseTableName(table);
                    statement.FromItems = this.ParseTableScources(table);
                }
                else if (child is Search_conditionContext condition)
                {
                    switch (terminalNodeType)
                    {
                        case TSqlParser.WHERE:
                            statement.Where = this.ParseCondition(condition);
                            break;
                        case TSqlParser.HAVING:
                            statement.Having = this.ParseCondition(condition);
                            break;
                    }
                }
                else if (child is Group_by_itemContext groupBy)
                {
                    if (statement.GroupBy == null)
                    {
                        statement.GroupBy = new List<TokenInfo>();
                    }

                    statement.GroupBy.Add(this.CreateToken(groupBy, TokenType.GroupBy));
                }
                else if (child is Top_clauseContext top)
                {
                    var topCount = top.top_count();

                    statement.TopInfo = new SelectTopInfo();
                    statement.TopInfo.TopCount = new TokenInfo(topCount);

                    string text = topCount.GetText();

                    if (text.Contains("@"))
                    {
                        if (text.StartsWith("(") && topCount.expression() != null)
                        {
                            statement.TopInfo.TopCount = new TokenInfo(topCount.expression());
                        }

                        statement.TopInfo.TopCount.Type = TokenType.VariableName;
                    }

                    statement.TopInfo.IsPercent = node.select_list().select_list_elem().Any(item => item.children.Any(t => t?.GetText()?.ToUpper() == "PERCENT"));
                }
            }

            return statement;
        }

        public UnionStatement ParseUnionSatement(Sql_unionContext node)
        {
            UnionStatement statement = new UnionStatement();

            UnionType unionType = UnionType.UNION;

            foreach (var child in node.children)
            {
                if (child is TerminalNodeImpl terminalNode)
                {
                    int type = terminalNode.Symbol.Type;

                    switch (type)
                    {
                        case TSqlParser.ALL:
                            unionType = UnionType.UNION_ALL;
                            break;
                        case TSqlParser.INTERSECT:
                            unionType = UnionType.INTERSECT;
                            break;
                        case TSqlParser.EXCEPT:
                            unionType = UnionType.EXCEPT;
                            break;
                    }
                }
                else if (child is Query_specificationContext spec)
                {
                    statement.Type = unionType;
                    statement.SelectStatement = this.ParseQuerySpecification(spec);
                }
            }

            return statement;
        }

        public SetStatement ParseSetStatement(Set_statementContext node)
        {
            SetStatement statement = new SetStatement();

            foreach (var child in node.children)
            {
                if (child is TerminalNodeImpl terminalNode)
                {
                    int type = terminalNode.Symbol.Type;
                    string text = terminalNode.GetText();

                    if (type != TSqlParser.SET && text != "=" && statement.Key == null)
                    {
                        statement.Key = new TokenInfo(terminalNode);

                        if (text.StartsWith("@"))
                        {
                            statement.Key.Type = TokenType.VariableName;
                        }
                    }
                }
                else if (child is ExpressionContext expression)
                {
                    statement.Value = this.CreateToken(expression);

                    var exps = expression.expression();

                    if (exps != null)
                    {
                        foreach (var exp in exps)
                        {
                            if (exp.GetText().StartsWith("@"))
                            {
                                statement.Value.AddChild(new TokenInfo(exp) { Type = TokenType.VariableName });
                            }
                        }
                    }

                    break;
                }
            }

            return statement;
        }

        public Statement ParseReturnStatement(Return_statementContext node)
        {
            Statement statement = null;

            var expressioin = node.expression();

            if (expressioin != null)
            {
                statement = new ReturnStatement() { Value = new TokenInfo(expressioin) };
            }
            else
            {
                statement = new LeaveStatement() { Content = new TokenInfo(node) };
            }

            return statement;
        }

        public TryCatchStatement ParseTryCatchStatement(Try_catch_statementContext node)
        {
            TryCatchStatement statement = new TryCatchStatement();

            var sqlClauses = node.sql_clauses();

            foreach (var sc in sqlClauses)
            {
                statement.TryStatements.AddRange(this.ParseSqlClause(sc));
            }

            statement.CatchStatements.AddRange(this.ParseSqlClause(node.try_clauses));

            return statement;
        }

        public PrintStatement ParsePrintStatement(Print_statementContext node)
        {
            PrintStatement statement = new PrintStatement();

            statement.Content = new TokenInfo(node.expression());

            return statement;
        }

        public CallStatement ParseExecuteStatement(Execute_statementContext node)
        {
            CallStatement statement = new CallStatement();

            Execute_bodyContext body = node.execute_body();

            statement.Name = new TokenInfo(body.func_proc_name_server_database_schema()) { Type = TokenType.RoutineName };

            Execute_statement_argContext args = body.execute_statement_arg();
            var varStrs = body.execute_var_string();

            if (args != null)
            {
                foreach (Execute_statement_arg_namedContext arg in args.execute_statement_arg_named())
                {
                    statement.Arguments.Add(new TokenInfo(arg));
                }
            }
            else if (varStrs != null && varStrs.Length > 0)
            {
                statement.IsExecuteSql = true;

                statement.Arguments.AddRange(varStrs.Select(item => new TokenInfo(item) { Type = TokenType.VariableName }));
            }

            return statement;
        }

        public TransactionStatement ParseTransactionStatment(Transaction_statementContext node)
        {
            TransactionStatement statement = new TransactionStatement();

            statement.Content = new TokenInfo(node);

            foreach (var child in node.children)
            {
                if (child is TerminalNodeImpl terminalNode)
                {
                    int type = terminalNode.Symbol.Type;

                    if (type == TSqlParser.BEGIN)
                    {
                        statement.CommandType = TransactionCommandType.BEGIN;
                    }
                    else if (type == TSqlParser.COMMIT)
                    {
                        statement.CommandType = TransactionCommandType.COMMIT;
                    }
                    else if (type == TSqlParser.ROLLBACK)
                    {
                        statement.CommandType = TransactionCommandType.ROLLBACK;
                    }
                }
            }

            return statement;
        }

        public Statement ParseCursorStatement(Cursor_statementContext node)
        {
            Statement statement = null;

            bool isOpen = false;
            bool isClose = false;
            bool isDeallocate = false;

            foreach (var child in node.children)
            {
                if (child is Declare_cursorContext declare)
                {
                    statement = this.ParseDeclareCursor(declare);
                }
                else if (child is TerminalNodeImpl terminalNode)
                {
                    int type = terminalNode.Symbol.Type;

                    if (type == TSqlParser.OPEN)
                    {
                        isOpen = true;
                    }
                    else if (type == TSqlParser.CLOSE)
                    {
                        isClose = true;
                    }
                    else if (type == TSqlParser.DEALLOCATE)
                    {
                        isDeallocate = true;
                    }
                }
                else if (child is Cursor_nameContext name)
                {
                    if (isOpen)
                    {
                        OpenCursorStatement openCursorStatement = new OpenCursorStatement();
                        openCursorStatement.CursorName = new TokenInfo(name) { Type = TokenType.CursorName };

                        statement = openCursorStatement;
                    }
                    else if (isClose)
                    {
                        CloseCursorStatement closeCursorStatement = new CloseCursorStatement();
                        closeCursorStatement.CursorName = new TokenInfo(name) { Type = TokenType.CursorName };

                        statement = closeCursorStatement;
                    }
                    else if (isDeallocate)
                    {
                        DeallocateCursorStatement deallocateCursorStatement = new DeallocateCursorStatement();
                        deallocateCursorStatement.CursorName = new TokenInfo(name) { Type = TokenType.CursorName };

                        statement = deallocateCursorStatement;
                    }
                }
                else if (child is Fetch_cursorContext fetch)
                {
                    FetchCursorStatement fetchCursorStatement = new FetchCursorStatement();

                    fetchCursorStatement.CursorName = new TokenInfo(fetch.cursor_name()) { Type = TokenType.CursorName };

                    foreach (var fc in fetch.children)
                    {
                        if (fc is TerminalNodeImpl tn)
                        {
                            string text = tn.GetText();

                            if (text.StartsWith("@"))
                            {
                                fetchCursorStatement.Variables.Add(new TokenInfo(tn) { Type = TokenType.VariableName });
                            }
                        }
                    }

                    statement = fetchCursorStatement;
                }
            }

            return statement;
        }

        public DeclareCursorStatement ParseDeclareCursor(Declare_cursorContext node)
        {
            DeclareCursorStatement statement = new DeclareCursorStatement();
            statement.CursorName = new TokenInfo(node.cursor_name()) { Type = TokenType.CursorName };

            var cursor = node.declare_set_cursor_common();

            Select_statementContext select = null;

            if (cursor != null)
            {
                select = cursor.select_statement_standalone()?.select_statement();
            }
            else
            {
                select = node.select_statement_standalone()?.select_statement();
            }

            if (select != null)
            {
                statement.SelectStatement = this.ParseSelectStatement(select).FirstOrDefault();
            }

            return statement;
        }

        private TokenInfo ParseCondition(ParserRuleContext node)
        {
            if (node != null)
            {
                if (node is Search_conditionContext sc)
                {
                    TokenInfo token = this.CreateToken(node);

                    bool isIfCondition = node.Parent != null && (node.Parent is If_statementContext || node.Parent is While_statementContext);

                    token.Type = isIfCondition ? TokenType.IfCondition : TokenType.SearchCondition;

                    if (!isIfCondition)
                    {
                        this.AddChildTableAndColumnNameToken(sc, token);
                    }

                    return token;
                }
                else if (node is Switch_search_condition_sectionContext)
                {
                    TokenInfo token = this.CreateToken(node, TokenType.SearchCondition);

                    return token;
                }
            }

            return null;
        }

        public override TableName ParseTableName(ParserRuleContext node, bool strict = false)
        {
            TableName tableName = null;

            if (node != null)
            {
                if (node is Table_nameContext tn)
                {
                    tableName = new TableName(tn);
                    tableName.Database = tn.database?.GetText();
                    tableName.Schema = tn.schema?.GetText();
                }
                else if (node is Full_table_nameContext fullName)
                {
                    tableName = new TableName(fullName);
                    tableName.Server = fullName.server?.GetText();
                    tableName.Database = fullName.database?.GetText();
                    tableName.Schema = fullName.schema?.GetText();

                    var parent = fullName.Parent;

                    if (parent != null && parent is Table_source_itemContext ts)
                    {
                        As_table_aliasContext alias = ts.as_table_alias();

                        if (alias != null)
                        {
                            tableName.Alias = new TokenInfo(alias.table_alias());
                        }
                    }
                }
                else if (node is Table_sourceContext ts)
                {
                    tableName = this.ParseTableName(ts.table_source_item_joined());
                }
                else if (node is Table_source_itemContext tsi)
                {
                    tableName = new TableName(tsi.full_table_name());

                    As_table_aliasContext alias = tsi.as_table_alias();

                    if (alias != null)
                    {
                        tableName.Alias = new TokenInfo(alias.table_alias());
                    }
                }
                else if (node is Table_source_item_joinedContext tsij)
                {
                    var tsit = tsij.table_source_item();

                    if (tsit != null)
                    {
                        tableName = this.ParseTableName(tsit);
                    }
                    else
                    {
                        tableName = this.ParseTableName(tsij.table_source_item_joined());
                    }
                }
                else if (node is Table_sourcesContext tss)
                {
                    var joined = tss.table_source().FirstOrDefault()?.table_source_item_joined();
                    var tsis = joined?.table_source_item();

                    if (tsis != null)
                    {
                        tableName = this.ParseTableName(tsis);
                    }
                    else
                    {
                        tableName = this.ParseTableName(joined?.table_source_item_joined());
                    }
                }
                else if (node is Ddl_objectContext ddl)
                {
                    return this.ParseTableName(ddl.full_table_name(), strict);
                }

                if (!strict && tableName == null)
                {
                    tableName = new TableName(node);
                }
            }

            return tableName;
        }

        public override ColumnName ParseColumnName(ParserRuleContext node, bool strict = false)
        {
            ColumnName columnName = null;

            if (node != null)
            {
                if (node is Full_column_nameContext fullName)
                {
                    columnName = new ColumnName(fullName.column_name.GetText(), fullName);
                    columnName.Server = fullName.server?.GetText();
                    columnName.Schema = fullName.schema?.GetText();

                    if (fullName.tablename != null)
                    {
                        columnName.TableName = new TokenInfo(fullName.tablename);
                    }

                    var parent = fullName.Parent;

                    if (parent != null && parent is Column_elemContext celem)
                    {
                        var alias = celem.as_column_alias()?.column_alias();

                        if (alias != null)
                        {
                            columnName.Alias = new TokenInfo(alias);
                        }
                    }
                }
                else if (node is Column_def_table_constraintContext col)
                {
                    columnName = new ColumnName(col.column_definition().id_());
                    columnName.DataType = new TokenInfo(col.column_definition().data_type()) { Type = TokenType.DataType };
                }
                else if (node is Select_list_elemContext elem)
                {
                    string text = elem.GetText();

                    if (text.StartsWith("@"))
                    {
                        columnName = new ColumnName(elem);

                        foreach (var child in elem.children)
                        {
                            if (child is TerminalNodeImpl impl)
                            {
                                columnName.AddChild(new TokenInfo(impl) { Type = TokenType.VariableName });
                                break;
                            }
                            else
                            {
                                columnName.AddChild(new TokenInfo(child as ParserRuleContext) { Type = TokenType.VariableName });
                                break;
                            }
                        }

                        var exp = elem.expression();

                        if (exp != null)
                        {
                            this.AddChildColumnNameToken(exp, columnName);
                        }
                    }
                    else
                    {
                        AsteriskContext asterisk = elem.asterisk();

                        if (asterisk != null)
                        {
                            return this.ParseColumnName(asterisk, strict);
                        }

                        var columnEle = elem.column_elem();
                        var expEle = elem.expression_elem();

                        if (columnEle != null)
                        {
                            columnName = null;

                            var fullColumnName = columnEle.full_column_name();

                            if (fullColumnName != null)
                            {
                                columnName = new ColumnName(fullColumnName);

                                if (fullColumnName.tablename != null)
                                {
                                    columnName.TableName = new TokenInfo(fullColumnName.tablename);
                                }
                            }
                            else
                            {
                                columnName = new ColumnName(columnEle);
                            }

                            var alias = columnEle.as_column_alias()?.column_alias();

                            if (alias != null)
                            {
                                columnName.HasAs = this.HasAsFlag(alias);
                                columnName.Alias = new TokenInfo(alias);

                                this.AddChildColumnNameToken(columnEle, columnName);
                            }
                        }
                        else if (expEle != null)
                        {
                            columnName = this.ParseColumnName(expEle, strict);

                            this.AddChildColumnNameToken(expEle, columnName);
                        }
                    }
                }
                else if (node is AsteriskContext asterisk)
                {
                    columnName = new ColumnName(asterisk);

                    foreach (var ac in asterisk.children)
                    {
                        if (ac is TerminalNodeImpl terminalNode)
                        {
                            if (terminalNode.Symbol.Type == TSqlParser.STAR)
                            {
                                columnName = new ColumnName(terminalNode);
                                break;
                            }
                        }
                    }

                    Table_nameContext tableName = asterisk.table_name();

                    if (columnName != null && tableName != null)
                    {
                        columnName.TableName = new TokenInfo(tableName);
                    }
                }
                else if (node is Expression_elemContext expElem)
                {
                    columnName = new ColumnName(expElem.expression());

                    var constContext = expElem.expression()?.primitive_expression()?.constant();

                    if (constContext != null)
                    {
                        columnName.IsConst = true;
                    }

                    Column_aliasContext alias = expElem.as_column_alias()?.column_alias();

                    if (alias == null)
                    {
                        alias = expElem.column_alias();
                    }

                    if (alias != null)
                    {
                        columnName.HasAs = this.HasAsFlag(alias);
                        columnName.Alias = new TokenInfo(alias);
                    }
                }
                else if (node is ExpressionContext exp)
                {
                    Full_column_nameContext fullColName = exp.full_column_name();

                    if (fullColName != null)
                    {
                        return this.ParseColumnName(fullColName, strict);
                    }
                }
                else if (node is Column_aliasContext colAlias)
                {
                    if (colAlias.Parent != null && colAlias.Parent is Column_alias_listContext)
                    {
                        columnName = new ColumnName(colAlias.id_());

                        Column_alias_listContext parent = colAlias.Parent as Column_alias_listContext;

                        columnName.HasAs = this.HasAsFlag(parent._column_alias);
                        columnName.Alias = new TokenInfo(parent._column_alias);
                    }
                }

                if (!strict && columnName == null)
                {
                    columnName = new ColumnName(node);
                }
            }

            return columnName;
        }

        public override bool IsFunction(IParseTree node)
        {
            if (node is Function_callContext || node is BUILT_IN_FUNCContext)
            {
                return true;
            }

            return false;
        }

        public override TokenInfo ParseFunction(ParserRuleContext node)
        {
            TokenInfo token = base.ParseFunction(node);

            foreach (var child in node.children)
            {
                if (child is NEXT_VALUE_FORContext nextVal)
                {
                    var ids = nextVal.table_name().id_();

                    NameToken seqName;

                    if (ids.Length == 2)
                    {
                        seqName = new NameToken(ids[1]) { Type = TokenType.SequenceName };
                        seqName.Schema = ids[0].GetText();
                    }
                    else
                    {
                        seqName = new NameToken(ids[0]) { Type = TokenType.SequenceName };
                    }

                    token.AddChild(seqName);
                }
            }

            return token;
        }
    }
}
