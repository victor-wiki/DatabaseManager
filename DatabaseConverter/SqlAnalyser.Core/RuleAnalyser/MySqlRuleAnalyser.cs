using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using static MySqlParser;

namespace SqlAnalyser.Core
{
    public class MySqlRuleAnalyser : SqlRuleAnalyser
    {
        public override IEnumerable<Type> ParseTableTypes => new List<Type>() { typeof(SingleTableContext), typeof(TableRefContext) };

        public override IEnumerable<Type> ParseColumnTypes => new List<Type>() { typeof(SelectItemContext), typeof(ColumnRefContext) };

        public override Lexer GetLexer(string content)
        {
            return new MySqlLexer(this.GetCharStreamFromString(content));
        }

        public override Parser GetParser(CommonTokenStream tokenStream)
        {
            return new MySqlParser(tokenStream);
        }

        public SimpleStatementContext GetRootContext(string content, out SqlSyntaxError error)
        {
            error = null;

            MySqlParser parser = this.GetParser(content) as MySqlParser;

            SqlSyntaxErrorListener errorListener = this.AddParserErrorListener(parser);

            SimpleStatementContext context = parser.query().simpleStatement();

            error = errorListener.Error;

            return context;
        }

        public CreateStatementContext GetCreateStatementContext(string content, out SqlSyntaxError error)
        {
            error = null;

            SimpleStatementContext rootContext = this.GetRootContext(content, out error);

            return rootContext.createStatement();
        }

        public override AnalyseResult AnalyseProcedure(string content)
        {
            SqlSyntaxError error = null;

            CreateStatementContext createStatement = this.GetCreateStatementContext(content, out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && createStatement != null)
            {
                RoutineScript script = new RoutineScript() { Type = RoutineType.PROCEDURE };

                CreateProcedureContext proc = createStatement.createProcedure();

                if (proc != null)
                {
                    #region Name                    
                    script.Name = new TokenInfo(proc.procedureName().qualifiedIdentifier());
                    #endregion

                    #region Parameters
                    ProcedureParameterContext[] parameters = proc.procedureParameter();

                    if (parameters != null)
                    {
                        foreach (ProcedureParameterContext parameter in parameters)
                        {
                            Parameter parameterInfo = new Parameter();

                            var funPara = parameter.functionParameter();

                            parameterInfo.Name = new TokenInfo(funPara.parameterName()) { Type = TokenType.ParameterName };

                            parameterInfo.DataType = new TokenInfo(funPara.typeWithOptCollate().GetText()) { Type = TokenType.DataType };

                            this.SetParameterType(parameterInfo, parameter.children);

                            script.Parameters.Add(parameterInfo);
                        }
                    }
                    #endregion

                    #region Body

                    this.SetScriptBody(script, proc.compoundStatement());

                    #endregion
                }

                this.ExtractFunctions(script, createStatement);

                result.Script = script;
            }

            return result;
        }

        public void SetScriptBody(CommonScript script, CompoundStatementContext body)
        {
            script.Statements.AddRange(this.ParseRoutineBody(body));
        }

        public List<Statement> ParseRoutineBody(CompoundStatementContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var child in node.unlabeledBlock().beginEndBlock().children)
            {
                if (child is SpDeclarationsContext spDeclarations)
                {
                    foreach (var declaration in spDeclarations.spDeclaration())
                    {
                        foreach (var c in declaration.children)
                        {
                            if (c is VariableDeclarationContext variable)
                            {
                                statements.Add(this.ParseDeclareStatement(variable));
                            }
                            else if (c is HandlerDeclarationContext handlerDeclaration)
                            {
                                statements.Add(this.ParseDeclareHandler(handlerDeclaration));
                            }
                            else if (c is CursorDeclarationContext cursorDeclaration)
                            {
                                statements.Add(this.ParseDeclareCursor(cursorDeclaration));
                            }
                        }
                    }
                }
                else if (child is CompoundStatementListContext compoundStatementList)
                {
                    statements.AddRange(this.ParseCompoundStatementList(compoundStatementList));
                }
            }

            return statements;
        }

        public List<Statement> ParseSimpleStatement(SimpleStatementContext simpleStatementContext)
        {
            List<Statement> statements = new List<Statement>();

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

            foreach (var child in simpleStatementContext.children)
            {
                if (child is SelectStatementContext select)
                {
                    statements.Add(this.ParseSelectStatement(select));
                }
                else if (child is InsertStatementContext insert)
                {
                    InsertStatement statement = this.ParseInsertStatement(insert);

                    statements.Add(statement);
                }
                else if (child is UpdateStatementContext update)
                {
                    statements.Add(this.ParseUpdateStatement(update));
                }
                else if (child is DeleteStatementContext delete)
                {
                    statements.Add(this.ParseDeleteStatement(delete));
                }
                else if (child is SetStatementContext set)
                {
                    statements.AddRange(this.ParseSetStatement(set));
                }
                else if (child is CallStatementContext call)
                {
                    statements.Add(this.ParseCallStatement(call));
                }
                else if(child is DropStatementContext drop)
                {
                    foreach(var c in drop.children)
                    {
                        if (c is DropTableContext drop_table)
                        {
                            addDropStatement(DatabaseObjectType.Table, TokenType.TableName, drop_table.tableRefList().tableRef());
                        }
                        else if (c is DropViewContext drop_view)
                        {
                            addDropStatement(DatabaseObjectType.View, TokenType.ViewName, drop_view.viewRefList().viewRef());
                        }
                        else if (c is DropFunctionContext drop_function)
                        {
                            addDropStatement(DatabaseObjectType.Function, TokenType.FunctionName, new ParserRuleContext[] { drop_function.functionRef() });
                        }
                        else if (c is DropProcedureContext drop_proc)
                        {
                            addDropStatement(DatabaseObjectType.Procedure, TokenType.ProcedureName, new ParserRuleContext[] { drop_proc.procedureRef() });
                        }
                        else if (c is DropTriggerContext drop_trigger)
                        {
                            addDropStatement(DatabaseObjectType.Trigger, TokenType.TriggerName, new ParserRuleContext[] { drop_trigger.triggerRef() });
                        }
                    }                    
                }               
            }

            return statements;
        }

        public void SetParameterType(Parameter parameterInfo, IList<IParseTree> nodes)
        {
            foreach (var child in nodes)
            {
                if (child is TerminalNodeImpl terminalNode)
                {
                    if (terminalNode.Symbol.Type == MySqlParser.IN_SYMBOL)
                    {
                        parameterInfo.ParameterType = ParameterType.IN;
                    }
                    else if (terminalNode.Symbol.Type == MySqlParser.OUT_SYMBOL)
                    {
                        parameterInfo.ParameterType = ParameterType.OUT;
                    }
                    else if (terminalNode.Symbol.Type == MySqlParser.INOUT_SYMBOL)
                    {
                        parameterInfo.ParameterType = ParameterType.IN | ParameterType.OUT;
                    }
                }
            }
        }

        public override AnalyseResult AnalyseFunction(string content)
        {
            SqlSyntaxError error = null;

            CreateStatementContext createStatement = this.GetCreateStatementContext(content, out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && createStatement != null)
            {
                RoutineScript script = new RoutineScript() { Type = RoutineType.FUNCTION };

                CreateFunctionContext func = createStatement.createFunction();

                if (func != null)
                {
                    #region Name                    
                    script.Name = new TokenInfo(func.functionName().qualifiedIdentifier());
                    #endregion

                    #region Parameters
                    FunctionParameterContext[] parameters = func.functionParameter();

                    if (parameters != null)
                    {
                        foreach (FunctionParameterContext parameter in parameters)
                        {
                            Parameter parameterInfo = new Parameter();

                            parameterInfo.Name = new TokenInfo(parameter.parameterName()) { Type = TokenType.ParameterName };

                            parameterInfo.DataType = new TokenInfo(parameter.typeWithOptCollate().GetText()) { Type = TokenType.DataType };

                            this.SetParameterType(parameterInfo, parameter.children);

                            script.Parameters.Add(parameterInfo);
                        }
                    }
                    #endregion

                    script.ReturnDataType = new TokenInfo(func.typeWithOptCollate().GetText()) { Type = TokenType.DataType };

                    #region Body

                    this.SetScriptBody(script, func.compoundStatement());

                    #endregion
                }

                this.ExtractFunctions(script, createStatement);

                result.Script = script;
            }

            return result;
        }

        public override AnalyseResult AnalyseView(string content)
        {
            SqlSyntaxError error = null;

            CreateStatementContext createStatement = this.GetCreateStatementContext(content, out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && createStatement != null)
            {
                ViewScript script = new ViewScript();

                CreateViewContext view = createStatement.createView();

                if (view != null)
                {
                    #region Name
                    script.Name = new TokenInfo(view.viewName());
                    #endregion                  

                    #region Statement

                    foreach (var child in view.children)
                    {
                        if (child is ViewTailContext tail)
                        {
                            script.Statements.Add(this.ParseViewSelectStatement(tail.viewSelect()));
                        }
                    }

                    #endregion
                }

                this.ExtractFunctions(script, createStatement);

                result.Script = script;
            }

            return result;
        }

        public override AnalyseResult AnalyseTrigger(string content)
        {
            SqlSyntaxError error = null;

            CreateStatementContext createStatement = this.GetCreateStatementContext(content, out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && createStatement != null)
            {
                TriggerScript script = new TriggerScript();

                CreateTriggerContext trigger = createStatement.createTrigger();

                if (trigger != null)
                {
                    #region Name                    
                    script.Name = new TokenInfo(trigger.triggerName());

                    var follows = trigger.triggerFollowsPrecedesClause();
                    if (follows != null)
                    {
                        script.OtherTriggerName = new TokenInfo(follows.textOrIdentifier());

                        script.Behavior = follows.GetText().Replace(script.OtherTriggerName.Symbol, "");
                    }

                    #endregion

                    script.TableName = new TableName(trigger.tableRef().qualifiedIdentifier());

                    foreach (var child in trigger.children)
                    {
                        if (child is TerminalNodeImpl terminalNode)
                        {
                            switch (terminalNode.Symbol.Type)
                            {
                                case MySqlParser.BEFORE_SYMBOL:
                                    script.Time = TriggerTime.BEFORE;
                                    break;
                                case MySqlParser.AFTER_SYMBOL:
                                    script.Time = TriggerTime.AFTER;
                                    break;
                                case MySqlParser.INSERT_SYMBOL:
                                    script.Events.Add(TriggerEvent.INSERT);
                                    break;
                                case MySqlParser.UPDATE_SYMBOL:
                                    script.Events.Add(TriggerEvent.UPDATE);
                                    break;
                                case MySqlParser.DELETE_SYMBOL:
                                    script.Events.Add(TriggerEvent.DELETE);
                                    break;
                                case MySqlParser.PRECEDES_SYMBOL:
                                    script.Behavior = "PRECEDES";
                                    break;
                                case MySqlParser.FOLLOWS_SYMBOL:
                                    script.Behavior = "FOLLOWS";
                                    break;
                            }
                        }
                    }

                    #region Body

                    this.SetScriptBody(script, trigger.compoundStatement());

                    #endregion
                }

                this.ExtractFunctions(script, createStatement);

                result.Script = script;
            }

            return result;
        }

        public DeclareCursorStatement ParseDeclareCursor(CursorDeclarationContext node)
        {
            DeclareCursorStatement statement = new DeclareCursorStatement();

            statement.CursorName = new TokenInfo(node.identifier()) { Type = TokenType.CursorName };
            statement.SelectStatement = this.ParseSelectStatement(node.selectStatement());

            return statement;
        }

        public DeclareCursorHandlerStatement ParseDeclareHandler(HandlerDeclarationContext node)
        {
            DeclareCursorHandlerStatement statement = new DeclareCursorHandlerStatement();

            statement.Conditions.AddRange(node.handlerCondition().Select(item => new TokenInfo(item) { Type = TokenType.Condition }));
            statement.Statements.AddRange(this.ParseCompoundStatement(node.compoundStatement()));

            return statement;
        }

        public OpenCursorStatement ParseOpenCursorSatement(CursorOpenContext node)
        {
            OpenCursorStatement statement = new OpenCursorStatement();

            statement.CursorName = new TokenInfo(node.identifier()) { Type = TokenType.CursorName };

            return statement;
        }

        public FetchCursorStatement ParseFetchCursorSatement(CursorFetchContext node)
        {
            FetchCursorStatement statement = new FetchCursorStatement();

            statement.CursorName = new TokenInfo(node.identifier()) { Type = TokenType.CursorName };
            statement.Variables.AddRange(node.identifierList().identifier().Select(item => new TokenInfo(item) { Type = TokenType.VariableName }));

            return statement;
        }

        public CloseCursorStatement ParseCloseCursorSatement(CursorCloseContext node)
        {
            CloseCursorStatement statement = new CloseCursorStatement() { IsEnd = true };

            statement.CursorName = new TokenInfo(node.identifier()) { Type = TokenType.CursorName };

            return statement;
        }

        public CallStatement ParseCallStatement(CallStatementContext node)
        {
            CallStatement statement = new CallStatement();

            statement.Name = new TokenInfo(node.procedureRef()) { Type = TokenType.RoutineName };

            ExprContext[] expressions = node.exprList().expr();

            if (expressions != null && expressions.Length > 0)
            {
                statement.Arguments.AddRange(expressions.Select(item => new TokenInfo(item)));
            }

            return statement;
        }

        public InsertStatement ParseInsertStatement(InsertStatementContext node)
        {
            InsertStatement statement = new InsertStatement();

            statement.TableName = this.ParseTableName(node.tableRef());

            var insertFrom = node.insertFromConstructor();

            foreach (var child in insertFrom.fields().children)
            {
                if (child is InsertIdentifierContext col)
                {
                    TokenInfo tokenInfo = new TokenInfo(col.columnRef()) { Type = TokenType.ColumnName };

                    statement.Columns.Add(this.ParseColumnName(col.columnRef()));
                }
            }

            foreach (var value in insertFrom.insertValues().valueList().values())
            {
                foreach (var v in value.expr())
                {
                    TokenInfo valueInfo = new TokenInfo(v) { Type = TokenType.InsertValue };

                    statement.Values.Add(valueInfo);
                }
            }

            return statement;
        }

        public UpdateStatement ParseUpdateStatement(UpdateStatementContext node)
        {
            UpdateStatement statement = new UpdateStatement();

            var tableRefs = node.tableReferenceList().tableReference();

            statement.FromItems = new List<FromItem>();

            foreach (var tableRef in tableRefs)
            {
                statement.TableNames.Add(this.ParseTableName(tableRef.tableFactor()));

                var joinTables = tableRef.joinedTable();

                FromItem fromItem = new FromItem();

                foreach (var joinTable in joinTables)
                {
                    fromItem.TableName = this.ParseTableName(joinTable.tableReference().tableFactor());

                    JoinItem joinItem = new JoinItem();
                    joinItem.TableName = fromItem.TableName;
                    joinItem.Condition = new TokenInfo(joinTable.expr()) { Type = TokenType.Condition };

                    fromItem.JoinItems.Add(joinItem);
                }

                statement.FromItems.Add(fromItem);
            }

            var setItems = node.updateList().updateElement();

            foreach (var setItem in setItems)
            {
                var valueExp = setItem.expr();

                NameValueItem item = new NameValueItem();

                item.Name = this.ParseColumnName(setItem.columnRef());
                item.Value = new TokenInfo(valueExp);

                this.AddChildTableAndColumnNameToken(valueExp, item.Value);

                statement.SetItems.Add(item);
            }

            var condition = node.whereClause().expr();

            if (condition != null)
            {
                statement.Condition = this.ParseCondition(condition);
            }

            return statement;
        }

        public DeleteStatement ParseDeleteStatement(DeleteStatementContext node)
        {
            List<DeleteStatement> statements = new List<DeleteStatement>();

            DeleteStatement statement = new DeleteStatement();
            statement.TableName = this.ParseTableName(node.tableRef());

            var condition = node.whereClause().expr();

            if (condition != null)
            {
                statement.Condition = this.ParseCondition(condition);
            }

            return statement;
        }

        public SelectStatement ParseViewSelectStatement(ViewSelectContext node)
        {
            SelectStatement statement = new SelectStatement();

            foreach (var child in node.children)
            {
                if (child is QueryExpressionOrParensContext exp)
                {
                    statement = this.ParseQueryExpression(exp.queryExpression());
                }
            }

            return statement;
        }

        public SelectStatement ParseSelectStatement(SelectStatementContext node)
        {
            SelectStatement statement = new SelectStatement();

            foreach (var child in node.children)
            {
                if (child is QueryExpressionContext exp)
                {
                    statement = this.ParseQueryExpression(exp);
                }
            }

            return statement;
        }

        public SelectStatement ParseQueryExpression(QueryExpressionContext node)
        {
            SelectStatement statement = null;

            QueryExpressionBodyContext body = node.queryExpressionBody();

            bool isUnion = false;
            UnionType unionType = UnionType.UNION;

            foreach (var c in body.children)
            {
                if (c is QueryPrimaryContext queryPrimary)
                {
                    if (!isUnion)
                    {
                        statement = this.ParseQuerySpecification(queryPrimary.querySpecification());
                    }
                    else
                    {
                        UnionStatement unionStatement = new UnionStatement();
                        unionStatement.Type = unionType;
                        unionStatement.SelectStatement = this.ParseQuerySpecification(queryPrimary.querySpecification());

                        statement.UnionStatements.Add(unionStatement);

                        isUnion = false;
                    }
                }
                else if (c is TerminalNodeImpl terminalNode)
                {
                    if (terminalNode.Symbol.Type == MySqlParser.UNION_SYMBOL)
                    {
                        isUnion = true;
                        statement.UnionStatements = new List<UnionStatement>();
                    }
                }
                else if (c is UnionOptionContext unionOption)
                {
                    switch (unionOption.GetText())
                    {
                        case nameof(TSqlParser.ALL):
                            unionType = UnionType.UNION_ALL;
                            break;
                        case nameof(TSqlParser.INTERSECT):
                            unionType = UnionType.INTERSECT;
                            break;
                        case nameof(TSqlParser.EXCEPT):
                            unionType = UnionType.EXCEPT;
                            break;
                    }
                }
            }

            return statement;
        }

        public SelectStatement ParseQuerySpecification(QuerySpecificationContext node)
        {
            SelectStatement statement = new SelectStatement();

            var columns = node.selectItemList().selectItem();

            if (columns.Length == 0)
            {
                statement.Columns.Add(new ColumnName("*"));
            }
            else
            {
                foreach (var col in columns)
                {
                    statement.Columns.Add(this.ParseColumnName(col));
                }
            }

            var fromTables = node.fromClause()?.tableReferenceList()?.tableReference();

            if (fromTables != null)
            {
                statement.FromItems = new List<FromItem>();

                foreach (var fromTable in fromTables)
                {
                    FromItem fromItem = new FromItem();

                    var tableFactor = fromTable.tableFactor();

                    if (tableFactor.tableReferenceListParens() == null)
                    {
                        fromItem.TableName = this.ParseTableName(tableFactor.singleTable());
                    }
                    else
                    {
                        var tableRefListParents = tableFactor.tableReferenceListParens();

                        fromItem.TableName = new TableName(tableRefListParents);

                        this.AddChildTableAndColumnNameToken(tableRefListParents, fromItem.TableName);
                    }

                    if (statement.TableName == null)
                    {
                        statement.TableName = fromItem.TableName;
                    }

                    statement.FromItems.Add(fromItem);
                }

                var condition = node.whereClause();
                var groupBy = node.groupByClause();
                var having = node.havingClause();

                if (condition != null)
                {
                    statement.Where = this.ParseCondition(condition.expr());
                }

                if (groupBy != null)
                {
                    statement.GroupBy = new List<TokenInfo>();

                    foreach (var col in groupBy.orderList().orderExpression())
                    {
                        statement.GroupBy.Add(this.CreateToken(col, TokenType.GroupBy));
                    }
                }

                if (having != null)
                {
                    statement.Having = this.ParseCondition(having.expr());
                }
            }

            return statement;
        }

        public List<SetStatement> ParseSetStatement(SetStatementContext node)
        {
            List<SetStatement> statements = new List<SetStatement>();

            var sv = node.startOptionValueList()?.optionValueNoOptionType();

            if (sv != null)
            {
                foreach (var child in sv.children)
                {
                    if (child is UserVariableContext variable)
                    {
                        SetStatement statement = new SetStatement();

                        statement.Key = new TokenInfo(variable);

                        statements.Add(statement);
                    }
                    else if (child is InternalVariableNameContext internalVariable)
                    {
                        SetStatement statement = new SetStatement();

                        statement.Key = new TokenInfo(internalVariable);

                        statements.Add(statement);
                    }
                    else if (child is ExprIsContext expr)
                    {
                        statements.Last().Value = this.CreateToken(expr);
                    }
                    else if (child is SetExprOrDefaultContext setExpr)
                    {
                        statements.Last().Value = this.CreateToken(setExpr);
                    }
                }
            }

            return statements;
        }

        public List<Statement> ParseCompoundStatementList(CompoundStatementListContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var child in node.children)
            {
                if (child is CompoundStatementContext compound)
                {
                    statements.AddRange(this.ParseCompoundStatement(compound));
                }
            }

            return statements;
        }

        public List<Statement> ParseCompoundStatement(CompoundStatementContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var child in node.children)
            {
                if (child is SimpleStatementContext simpleStatement)
                {
                    statements.AddRange(this.ParseSimpleStatement(simpleStatement));
                }
                else if (child is IfStatementContext @if)
                {
                    statements.Add(this.ParseIfStatement(@if));
                }
                else if (child is CaseStatementContext @case)
                {
                    statements.Add(this.ParseCaseStatement(@case));
                }
                else if (child is LabeledControlContext labeledControl)
                {
                    statements.Add(this.ParseLabelControlStatement(labeledControl));
                }
                else if (child is UnlabeledControlContext unlabeledControl)
                {
                    statements.Add(this.ParseUnlabeledControlStatement(unlabeledControl));
                }
                else if (child is ReturnStatementContext returnStatement)
                {
                    statements.Add(this.ParseReturnStatement(returnStatement));
                }
                else if (child is LeaveStatementContext leave)
                {
                    statements.Add(this.ParseLeaveStatement(leave));
                }
                else if (child is IterateStatementContext iterate)
                {
                    statements.Add(this.ParseIterateStatement(iterate));
                }
                else if (child is CursorOpenContext openCursor)
                {
                    statements.Add(this.ParseOpenCursorSatement(openCursor));
                }
                else if (child is CursorFetchContext fetchCursor)
                {
                    statements.Add(this.ParseFetchCursorSatement(fetchCursor));
                }
                else if (child is CursorCloseContext closeCursor)
                {
                    statements.Add(this.ParseCloseCursorSatement(closeCursor));
                }
            }

            return statements;
        }

        public DeclareStatement ParseDeclareStatement(VariableDeclarationContext node)
        {
            DeclareStatement statement = new DeclareStatement();

            statement.Name = new TokenInfo(node.identifierList().identifier().First()) { Type = TokenType.VariableName };
            statement.DataType = new TokenInfo(node.dataType().GetText()) { Type = TokenType.DataType };

            var defaultValue = node.expr();

            if (defaultValue != null)
            {
                statement.DefaultValue = new TokenInfo(defaultValue);
            }

            return statement;
        }

        public IfStatement ParseIfStatement(IfStatementContext node)
        {
            IfStatement statement = new IfStatement();

            IfStatementItem ifItem = new IfStatementItem() { Type = IfStatementType.IF };

            bool isFirst = true;
            var ifBody = node.ifBody();

            while (ifBody != null)
            {
                IfStatementItem item = new IfStatementItem() { Type = isFirst ? IfStatementType.IF : IfStatementType.ELSEIF };
                item.Condition = new TokenInfo(ifBody.expr()) { Type = TokenType.Condition };
                item.Statements.AddRange(this.ParseCompoundStatementList(ifBody.thenStatement().compoundStatementList()));

                statement.Items.Add(item);

                ifBody = ifBody.ifBody();

                isFirst = false;

                var elseStatements = ifBody?.compoundStatementList();

                if (elseStatements != null)
                {
                    IfStatementItem elseItem = new IfStatementItem() { Type = IfStatementType.ELSE };
                    elseItem.Statements.AddRange(this.ParseCompoundStatementList(elseStatements));
                }
            }

            return statement;
        }

        public CaseStatement ParseCaseStatement(CaseStatementContext node)
        {
            CaseStatement statement = new CaseStatement();

            statement.VariableName = new TokenInfo(node.expr()) { Type = TokenType.VariableName };

            var whens = node.whenExpression();
            var thens = node.thenStatement();

            for (int i = 0; i < whens.Length; i++)
            {
                IfStatementItem elseIfItem = new IfStatementItem() { Type = i == 0 ? IfStatementType.IF : IfStatementType.ELSEIF };

                elseIfItem.Condition = new TokenInfo(whens[i].expr()) { Type = TokenType.Condition };
                elseIfItem.Statements.AddRange(this.ParseCompoundStatementList(thens[i].compoundStatementList()));

                statement.Items.Add(elseIfItem);
            }

            var elseStatement = node.elseStatement();

            if (elseStatement != null)
            {
                IfStatementItem elseItem = new IfStatementItem() { Type = IfStatementType.ELSE };
                elseItem.Statements.AddRange(this.ParseCompoundStatementList(elseStatement.compoundStatementList()));

                statement.Items.Add(elseItem);
            }

            return statement;
        }

        public WhileStatement ParseWhileStatement(WhileDoBlockContext node)
        {
            WhileStatement statement = new WhileStatement();

            foreach (var child in node.children)
            {
                if (child is CompoundStatementListContext compoundStatementList)
                {
                    statement.Statements.AddRange(this.ParseCompoundStatementList(compoundStatementList));
                }
                else if (child is CompoundStatementContext compound)
                {
                    statement.Statements.AddRange(this.ParseCompoundStatement(compound));
                }
                else if (child is ExprContext condition)
                {
                    statement.Condition = new TokenInfo(condition) { Type = TokenType.Condition };
                }
            }

            return statement;
        }

        public LoopStatement ParseLoopStatement(LoopBlockContext node, LabelContext name)
        {
            LoopStatement statement = new LoopStatement();

            statement.Name = new TokenInfo(name);

            var innerStatements = this.ParseCompoundStatementList(node.compoundStatementList());

            foreach (var innerStatement in innerStatements)
            {
                bool useInnerStatement = true;

                if (innerStatement is IfStatement ifStatement)
                {
                    foreach (var ifItem in ifStatement.Items)
                    {
                        var leaveOrIterate = ifItem.Statements.FirstOrDefault(item => item is LeaveStatement || item is IterateStatement);

                        if (leaveOrIterate != null)
                        {
                            if (ifItem.Condition?.Symbol?.ToUpper() != "DONE")
                            {
                                statement.Condition = ifItem.Condition;
                            }

                            useInnerStatement = false;
                            break;
                        }
                    }
                }


                if (useInnerStatement)
                {
                    statement.Statements.Add(innerStatement);
                }
            }

            return statement;
        }

        public Statement ParseLabelControlStatement(LabeledControlContext node)
        {
            Statement statement = null;

            var name = node.label();

            var content = node.unlabeledControl();

            var loopBlock = content.loopBlock();

            if (loopBlock != null)
            {
                statement = this.ParseLoopStatement(loopBlock, name);
            }

            return statement;
        }

        public Statement ParseUnlabeledControlStatement(UnlabeledControlContext node)
        {
            Statement statement = null;

            foreach (var child in node.children)
            {
                if (child is LoopBlockContext loopBlock)
                {
                    statement = this.ParseLoopStatement(loopBlock, null);
                }
                else if (child is WhileDoBlockContext whileDo)
                {
                    statement = this.ParseWhileStatement(whileDo);
                }
            }

            return statement;
        }

        public ReturnStatement ParseReturnStatement(ReturnStatementContext node)
        {
            ReturnStatement statement = new ReturnStatement();

            foreach (var child in node.children)
            {
                if (child is ExprIsContext expr)
                {
                    statement.Value = new TokenInfo(expr);
                }
            }

            return statement;
        }

        public TransactionStatement ParseTransactionStatement(TransactionStatementContext node)
        {
            TransactionStatement statement = new TransactionStatement();
            statement.Content = new TokenInfo(node);

            foreach (var child in node.children)
            {
                if (child is TerminalNodeImpl terminalNode)
                {
                    switch (terminalNode.Symbol.Type)
                    {
                        case MySqlParser.START_SYMBOL:
                            statement.CommandType = TransactionCommandType.BEGIN;
                            break;
                        case MySqlParser.COMMIT_SYMBOL:
                            statement.CommandType = TransactionCommandType.COMMIT;
                            break;
                        case MySqlParser.ROLLBACK_SYMBOL:
                            statement.CommandType = TransactionCommandType.ROLLBACK;
                            break;
                    }
                }
            }

            return statement;
        }

        public LeaveStatement ParseLeaveStatement(LeaveStatementContext node)
        {
            LeaveStatement statement = new LeaveStatement();

            statement.Content = new TokenInfo(node);

            return statement;
        }

        public IterateStatement ParseIterateStatement(IterateStatementContext node)
        {
            IterateStatement statement = new IterateStatement();

            statement.Content = new TokenInfo(node);

            return statement;
        }

        public override TableName ParseTableName(ParserRuleContext node, bool strict = false)
        {
            TableName tableName = null;

            if (node != null)
            {
                if (node is TableNameContext tn)
                {
                    tableName = new TableName(tn.dotIdentifier());
                }
                else if (node is TableRefContext tableRef)
                {
                    tableName = new TableName(tableRef.qualifiedIdentifier());
                }
                else if (node is SingleTableContext singleTable)
                {
                    tableName = new TableName(singleTable.tableRef().qualifiedIdentifier());

                    var alias = singleTable.tableAlias();

                    if (alias != null)
                    {
                        tableName.HasAs = this.HasAsFlag(alias);
                        tableName.Alias = new TokenInfo(alias);
                    }
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
                if (node is SelectItemContext si)
                {
                    var expr = si.expr();

                    columnName = new ColumnName(expr);               

                    var alias = si.selectAlias();

                    if (alias != null)
                    {
                        columnName.HasAs = this.HasAsFlag(alias);
                        columnName.Alias = new TokenInfo(alias.identifier());
                    }

                    this.AddChildColumnNameToken(si, columnName);
                }
                else if (node is QualifiedIdentifierContext qualifiedIdentifier)
                {
                    columnName = new ColumnName(qualifiedIdentifier.identifier());
                }
                else if (node is ColumnRefContext columnRef)
                {
                    columnName = new ColumnName(columnRef.fieldIdentifier());
                }
                else if (node is ExprIsContext expr)
                {
                    var detail = this.GetColumnDetailContext(expr);

                    if (detail is NumLiteralContext nl)
                    {
                        columnName = new ColumnName(nl) { IsConst = true };
                    }
                }

                if (!strict && columnName == null)
                {
                    columnName = new ColumnName(node);

                    this.AddChildColumnNameToken(node, columnName);
                }
            }

            return columnName;
        }

        private ParserRuleContext GetColumnDetailContext(ParserRuleContext node)
        {
            if (node != null)
            {
                foreach (var child in node.children)
                {
                    if (child is NumLiteralContext nl)
                    {
                        return nl;
                    }
                    else
                    {
                        return this.GetColumnDetailContext(child as ParserRuleContext);
                    }
                }
            }

            return node;
        }

        private TokenInfo ParseCondition(ParserRuleContext node)
        {
            if (node != null)
            {
                if (node is ExprContext || node is ExprIsContext)
                {
                    TokenInfo token = this.CreateToken(node, TokenType.Condition);

                    bool isIfCondition = node.Parent != null && (node.Parent is IfBodyContext || node.Parent is LeaveStatementContext);

                    if (!isIfCondition)
                    {
                        this.AddChildTableAndColumnNameToken(node, token);
                    }

                    return token;
                }
            }

            return null;
        }     

        public override bool IsFunction(IParseTree node)
        {
            if (node is FunctionCallContext || node is RuntimeFunctionCallContext)
            {
                return true;
            }

            return false;
        }
    }
}
