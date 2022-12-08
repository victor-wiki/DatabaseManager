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
        public MySqlRuleAnalyser(string content) : base(content)
        {
        }

        public override IEnumerable<Type> ParseTableTypes => new List<Type>() { typeof(SingleTableContext), typeof(TableRefContext) };

        public override IEnumerable<Type> ParseColumnTypes => new List<Type>() { typeof(SelectItemContext), typeof(ColumnRefContext) };
        public override IEnumerable<Type> ParseTableAliasTypes => new List<Type>() { typeof(TableAliasContext) };
        public override IEnumerable<Type> ParseColumnAliasTypes => new List<Type>() { typeof(SelectAliasContext) };

        protected override Lexer GetLexer()
        {
            return new MySqlLexer(this.GetCharStreamFromString());
        }

        protected override Parser GetParser(CommonTokenStream tokenStream)
        {
            return new MySqlParser(tokenStream);
        }

        private SimpleStatementContext GetRootContext(out SqlSyntaxError error)
        {
            error = null;

            MySqlParser parser = this.GetParser() as MySqlParser;

            SqlSyntaxErrorListener errorListener = this.AddParserErrorListener(parser);

            SimpleStatementContext context = parser.query().simpleStatement();

            error = errorListener.Error;

            return context;
        }

        private CreateStatementContext GetCreateStatementContext(out SqlSyntaxError error)
        {
            error = null;

            SimpleStatementContext rootContext = this.GetRootContext(out error);

            return rootContext.createStatement();
        }

        public override SqlSyntaxError Validate()
        {
            SqlSyntaxError error = null;

            var rootContext = this.GetRootContext(out error);

            return error;
        }

        public override AnalyseResult AnalyseCommon()
        {
            SqlSyntaxError error = null;

            SimpleStatementContext rootContext = this.GetRootContext(out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && rootContext != null)
            {
                CommonScript script = null;

                foreach (var child in rootContext.children)
                {
                    if (child is CreateStatementContext create)
                    {
                        var proc = create.createProcedure();
                        var func = create.createFunction();
                        var trigger = create.createTrigger();
                        var view = create.createView();

                        if (proc != null)
                        {
                            script = new RoutineScript() { Type = RoutineType.PROCEDURE };

                            this.SetProcedureScript(script as RoutineScript, proc);
                        }
                        else if (func != null)
                        {
                            script = new RoutineScript() { Type = RoutineType.FUNCTION };

                            this.SetFunctionScript(script as RoutineScript, func);
                        }
                        else if (trigger != null)
                        {
                            script = new TriggerScript();

                            this.SetTriggerScript(script as TriggerScript, trigger);
                        }
                        else if (view != null)
                        {
                            script = new ViewScript();

                            this.SetViewScript(script as ViewScript, view);
                        }
                    }
                }

                if (script == null)
                {
                    script = new CommonScript();

                    script.Statements.AddRange(this.ParseSimpleStatement(rootContext));
                }

                result.Script = script;

                this.ExtractFunctions(script, rootContext);
            }

            return result;
        }

        public override AnalyseResult AnalyseProcedure()
        {
            SqlSyntaxError error = null;

            CreateStatementContext createStatement = this.GetCreateStatementContext(out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && createStatement != null)
            {
                RoutineScript script = new RoutineScript() { Type = RoutineType.PROCEDURE };

                CreateProcedureContext proc = createStatement.createProcedure();

                if (proc != null)
                {
                    this.SetProcedureScript(script, proc);
                }

                this.ExtractFunctions(script, createStatement);

                result.Script = script;
            }

            return result;
        }

        private void SetScriptName(RoutineScript script, QualifiedIdentifierContext node)
        {
            var id = node.identifier();
            var dotId = node.dotIdentifier();

            if (dotId != null)
            {
                script.Schema = id.GetText();
                script.Name = new TokenInfo(dotId.identifier());
            }
            else
            {
                script.Name = new TokenInfo(id);
            }
        }

        private void SetProcedureScript(RoutineScript script, CreateProcedureContext proc)
        {
            #region Name   
            var qi = proc.procedureName().qualifiedIdentifier();

            this.SetScriptName(script, qi);
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

        private void SetScriptBody(CommonScript script, CompoundStatementContext body)
        {
            script.Statements.AddRange(this.ParseRoutineBody(body));
        }

        private List<Statement> ParseRoutineBody(CompoundStatementContext node)
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

        private List<Statement> ParseSimpleStatement(SimpleStatementContext simpleStatementContext)
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
                else if (child is CreateStatementContext create)
                {
                    var statement = this.ParseCreateStatement(create);

                    if (statement != null)
                    {
                        statements.Add(statement);
                    }
                }
                else if (child is TruncateTableStatementContext truncate)
                {
                    statements.Add(this.ParseTruncateTableStatement(truncate));
                }
                else if (child is DropStatementContext drop)
                {
                    foreach (var c in drop.children)
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
                else if (child is PreparedStatementContext prepared)
                {
                    statements.Add(this.ParsePreparedStatement(prepared));
                }
            }

            return statements;
        }

        private PreparedStatement ParsePreparedStatement(PreparedStatementContext node)
        {
            PreparedStatement statement = new PreparedStatement();

            var id = node.identifier();

            if (id != null)
            {
                statement.Id = new TokenInfo(id);
            }

            foreach (var child in node.children)
            {
                if (child is TerminalNodeImpl tni)
                {
                    string text = child.GetText().ToUpper();

                    if (text == "PREPARE")
                    {
                        statement.Type = PreparedStatementType.Prepare;
                    }
                    else if (text == "DEALLOCAT")
                    {
                        statement.Type = PreparedStatementType.Deallocate;
                    }
                }
                else if (child is TextLiteralContext text)
                {
                    statement.FromSqlOrVariable = new TokenInfo(text);
                }
                else if (child is UserVariableContext uVar)
                {
                    statement.FromSqlOrVariable = new TokenInfo(uVar) { Type = TokenType.UserVariableName };
                }
                else if (child is ExecuteStatementContext es)
                {
                    statement.Type = PreparedStatementType.Execute;

                    statement.Id = new TokenInfo(es.identifier());

                    var variables = es.executeVarList().userVariable();

                    if (variables != null)
                    {
                        statement.ExecuteVariables.AddRange(variables.Select(item => new TokenInfo(item) { Type = TokenType.VariableName }));
                    }
                }
            }

            return statement;
        }

        private void SetParameterType(Parameter parameterInfo, IList<IParseTree> nodes)
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

        public override AnalyseResult AnalyseFunction()
        {
            SqlSyntaxError error = null;

            CreateStatementContext createStatement = this.GetCreateStatementContext(out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && createStatement != null)
            {
                RoutineScript script = new RoutineScript() { Type = RoutineType.FUNCTION };

                CreateFunctionContext func = createStatement.createFunction();

                if (func != null)
                {
                    this.SetFunctionScript(script, func);
                }

                this.ExtractFunctions(script, createStatement);

                result.Script = script;
            }

            return result;
        }

        private void SetFunctionScript(RoutineScript script, CreateFunctionContext func)
        {
            #region Name         
            var qi = func.functionName().qualifiedIdentifier();

            this.SetScriptName(script, qi);
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

        public override AnalyseResult AnalyseView()
        {
            SqlSyntaxError error = null;

            CreateStatementContext createStatement = this.GetCreateStatementContext(out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && createStatement != null)
            {
                ViewScript script = new ViewScript();

                CreateViewContext view = createStatement.createView();

                if (view != null)
                {
                    this.SetViewScript(script, view);
                }

                this.ExtractFunctions(script, createStatement);

                result.Script = script;
            }

            return result;
        }

        private void SetViewScript(ViewScript script, CreateViewContext view)
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

        public override AnalyseResult AnalyseTrigger()
        {
            SqlSyntaxError error = null;

            CreateStatementContext createStatement = this.GetCreateStatementContext(out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && createStatement != null)
            {
                TriggerScript script = new TriggerScript();

                CreateTriggerContext trigger = createStatement.createTrigger();

                if (trigger != null)
                {
                    this.SetTriggerScript(script, trigger);
                }

                this.ExtractFunctions(script, createStatement);

                result.Script = script;
            }

            return result;
        }

        private void SetTriggerScript(TriggerScript script, CreateTriggerContext trigger)
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

        private DeclareCursorStatement ParseDeclareCursor(CursorDeclarationContext node)
        {
            DeclareCursorStatement statement = new DeclareCursorStatement();

            statement.CursorName = new TokenInfo(node.identifier()) { Type = TokenType.CursorName };
            statement.SelectStatement = this.ParseSelectStatement(node.selectStatement());

            return statement;
        }

        private DeclareCursorHandlerStatement ParseDeclareHandler(HandlerDeclarationContext node)
        {
            DeclareCursorHandlerStatement statement = new DeclareCursorHandlerStatement();

            statement.Conditions.AddRange(node.handlerCondition().Select(item => new TokenInfo(item) { Type = TokenType.SearchCondition }));
            statement.Statements.AddRange(this.ParseCompoundStatement(node.compoundStatement()));

            return statement;
        }

        private OpenCursorStatement ParseOpenCursorSatement(CursorOpenContext node)
        {
            OpenCursorStatement statement = new OpenCursorStatement();

            statement.CursorName = new TokenInfo(node.identifier()) { Type = TokenType.CursorName };

            return statement;
        }

        private FetchCursorStatement ParseFetchCursorSatement(CursorFetchContext node)
        {
            FetchCursorStatement statement = new FetchCursorStatement();

            statement.CursorName = new TokenInfo(node.identifier()) { Type = TokenType.CursorName };
            statement.Variables.AddRange(node.identifierList().identifier().Select(item => new TokenInfo(item) { Type = TokenType.VariableName }));

            return statement;
        }

        private CloseCursorStatement ParseCloseCursorSatement(CursorCloseContext node)
        {
            CloseCursorStatement statement = new CloseCursorStatement() { IsEnd = true };

            statement.CursorName = new TokenInfo(node.identifier()) { Type = TokenType.CursorName };

            return statement;
        }

        private CallStatement ParseCallStatement(CallStatementContext node)
        {
            CallStatement statement = new CallStatement();

            statement.Name = new TokenInfo(node.procedureRef()) { Type = TokenType.RoutineName };

            ExprContext[] expressions = node.exprList()?.expr();

            if (expressions != null && expressions.Length > 0)
            {
                statement.Parameters.AddRange(expressions.Select(item => new CallParameter() { Value = new TokenInfo(item) }));
            }

            return statement;
        }

        private InsertStatement ParseInsertStatement(InsertStatementContext node)
        {
            InsertStatement statement = new InsertStatement();

            statement.TableName = this.ParseTableName(node.tableRef());

            var insertFrom = node.insertFromConstructor();
            var fields = insertFrom.fields();

            if (fields != null)
            {
                foreach (var child in fields.children)
                {
                    if (child is InsertIdentifierContext col)
                    {
                        TokenInfo tokenInfo = new TokenInfo(col.columnRef()) { Type = TokenType.ColumnName };

                        statement.Columns.Add(this.ParseColumnName(col.columnRef()));
                    }
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

        private UpdateStatement ParseUpdateStatement(UpdateStatementContext node)
        {
            UpdateStatement statement = new UpdateStatement();

            var tableRefs = node.tableReferenceList().tableReference();

            statement.FromItems = this.ParseFromItems(tableRefs);

            statement.TableNames.AddRange(statement.FromItems.Select(item => item.TableName));

            var setItems = node.updateList().updateElement();

            foreach (var setItem in setItems)
            {
                var valueExp = setItem.expr();

                NameValueItem item = new NameValueItem();

                item.Name = this.ParseColumnName(setItem.columnRef());

                bool isSubquery = AnalyserHelper.IsSubquery(valueExp);

                if (!isSubquery)
                {
                    item.Value = new TokenInfo(valueExp) { Type = TokenType.UpdateSetValue };
                }
                else
                {
                    foreach (var child in valueExp.children)
                    {
                        if (child is PrimaryExprPredicateContext pep)
                        {
                            var subquery = (pep.predicate()?.bitExpr()?.SingleOrDefault().simpleExpr() as SimpleExprSubQueryContext)?.subquery();

                            if (subquery != null)
                            {
                                item.ValueStatement = this.ParseQueryExpression(subquery.queryExpressionParens()?.queryExpression());
                            }

                            break;
                        }
                    }
                }

                if (item.Value != null)
                {
                    this.AddChildTableAndColumnNameToken(valueExp, item.Value);
                }

                statement.SetItems.Add(item);
            }

            var condition = node.whereClause()?.expr();

            if (condition != null)
            {
                statement.Condition = this.ParseCondition(condition);
            }

            return statement;
        }

        private DeleteStatement ParseDeleteStatement(DeleteStatementContext node)
        {
            List<DeleteStatement> statements = new List<DeleteStatement>();

            DeleteStatement statement = new DeleteStatement();

            var tableRef = node.tableRef();

            if (tableRef != null)
            {
                statement.TableName = this.ParseTableName(tableRef);
            }
            else
            {
                var alias = node.tableAliasRefList();
                var tableRefs = node.tableReferenceList();

                if (alias != null)
                {
                    statement.TableName = new TableName(alias);
                }

                if (tableRefs != null)
                {
                    statement.FromItems = this.ParseFromItems(tableRefs.tableReference());

                    if (statement.TableName == null && statement.FromItems.Count > 0 && statement.FromItems[0].TableName != null)
                    {
                        statement.TableName = statement.FromItems[0].TableName;
                    }
                }
            }

            var condition = node.whereClause().expr();

            if (condition != null)
            {
                statement.Condition = this.ParseCondition(condition);
            }

            return statement;
        }

        private SelectStatement ParseViewSelectStatement(ViewSelectContext node)
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

        private SelectStatement ParseSelectStatement(SelectStatementContext node)
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

        private SelectStatement ParseQueryExpression(QueryExpressionContext node)
        {
            SelectStatement statement = null;

            QueryExpressionBodyContext body = node.queryExpressionBody();
            var limit = node.limitClause();

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

            if (limit != null)
            {
                var options = limit.limitOptions().limitOption();

                SelectLimitInfo limitInfo = new SelectLimitInfo();

                if (options.Length > 0)
                {
                    limitInfo.StartRowIndex = new TokenInfo(options[0]);
                }

                if (options.Length > 1)
                {
                    limitInfo.RowCount = new TokenInfo(options[1]);
                }

                statement.LimitInfo = limitInfo;
            }

            return statement;
        }

        private SelectStatement ParseQuerySpecification(QuerySpecificationContext node)
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

            var into = node.intoClause();

            if (into != null)
            {
                statement.Intos = new List<TokenInfo>();

                var userVars = into.userVariable();

                if (userVars != null && userVars.Length > 0)
                {
                    statement.Intos.AddRange(userVars.Select(item => new TokenInfo(item) { Type = TokenType.UserVariableName }));
                }
                else
                {
                    var textString = into.textStringLiteral();

                    if (textString != null)
                    {
                        statement.Intos.Add(new TokenInfo(textString) { Type = TokenType.StringLiteral });
                    }
                    else
                    {
                        var tis = into.textOrIdentifier();

                        if (tis != null)
                        {
                            statement.Intos.AddRange(tis.Select(item => new TokenInfo(item.identifier()) { Type = TokenType.VariableName }));
                        }
                    }
                }
            }

            var fromTables = node.fromClause()?.tableReferenceList()?.tableReference();

            if (fromTables != null)
            {
                statement.FromItems = this.ParseFromItems(fromTables);

                if (statement.TableName == null && statement.FromItems.Count > 0 && statement.FromItems[0].TableName != null)
                {
                    statement.TableName = statement.FromItems[0].TableName;
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
                        var gpb = this.CreateToken(col, TokenType.GroupBy);

                        statement.GroupBy.Add(gpb);

                        if (!AnalyserHelper.IsValidColumnName(gpb))
                        {
                            this.AddChildTableAndColumnNameToken(col, gpb);
                        }
                    }
                }

                if (having != null)
                {
                    statement.Having = this.ParseCondition(having.expr());
                }
            }

            return statement;
        }

        private List<FromItem> ParseFromItems(TableReferenceContext[] tableRefs)
        {
            List<FromItem> fromItems = new List<FromItem>();

            foreach (var tableRef in tableRefs)
            {
                FromItem fromItem = new FromItem();

                var tableFactor = tableRef.tableFactor();
                var joinTables = tableRef.joinedTable();

                var subquery = tableFactor?.derivedTable();

                if (subquery == null)
                {
                    fromItem.TableName = this.ParseTableName(tableFactor);
                }
                else
                {
                    var alias = subquery.tableAlias();

                    fromItem.SubSelectStatement = this.ParseDerivedTable(subquery);

                    if (alias != null)
                    {
                        fromItem.Alias = new TokenInfo(alias);
                    }
                }

                if (joinTables != null && joinTables.Length > 0)
                {
                    foreach (var joinTable in joinTables)
                    {
                        JoinItem joinItem = new JoinItem();

                        var joinType = joinTable.innerJoinType().GetText().ToUpper();

                        if (joinType.StartsWith(nameof(JoinType.INNER)))
                        {
                            joinItem.Type = JoinType.INNER;
                        }
                        else if (joinType.StartsWith(nameof(JoinType.LEFT)))
                        {
                            joinItem.Type = JoinType.LEFT;
                        }
                        else if (joinType.StartsWith(nameof(JoinType.RIGHT)))
                        {
                            joinItem.Type = JoinType.RIGHT;
                        }
                        else if (joinType.StartsWith(nameof(JoinType.CROSS)))
                        {
                            joinItem.Type = JoinType.CROSS;
                        }
                        else if (joinType.StartsWith(nameof(JoinType.FULL)))
                        {
                            joinItem.Type = JoinType.FULL;
                        }

                        var trf = joinTable.tableReference();
                        var derivedTable = trf?.tableFactor()?.derivedTable();

                        if (derivedTable == null)
                        {
                            joinItem.TableName = this.ParseTableName(joinTable.tableReference());
                        }
                        else
                        {
                            joinItem.TableName = this.ParseTableName(derivedTable.subquery());

                            var alias = derivedTable.tableAlias();

                            if (alias != null)
                            {
                                joinItem.TableName.Alias = new TokenInfo(alias);
                            }

                            this.AddChildTableAndColumnNameToken(derivedTable, joinItem.TableName);
                        }

                        joinItem.Condition = this.ParseCondition(joinTable.expr());

                        fromItem.JoinItems.Add(joinItem);
                    }
                }
                else
                {
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
                }

                fromItems.Add(fromItem);
            }

            return fromItems;
        }       

        private List<SetStatement> ParseSetStatement(SetStatementContext node)
        {
            List<SetStatement> statements = new List<SetStatement>();

            var sv = node.startOptionValueList()?.optionValueNoOptionType();

            Action<SetStatement> checkUserVaribleDataType = (statement) =>
            {
                if (statement.IsSetUserVariable)
                {
                    statement.UserVariableDataType = MySqlAnalyserHelper.DetectUserVariableDataType(statement.Value.Symbol);
                }
            };

            if (sv != null)
            {
                foreach (var child in sv.children)
                {
                    if (child is UserVariableContext variable)
                    {
                        SetStatement statement = new SetStatement();

                        statement.Key = new TokenInfo(variable) { Type = TokenType.UserVariableName };

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
                        SetStatement statement = statements.Last();

                        statement.Value = this.CreateToken(expr);

                        checkUserVaribleDataType(statement);
                    }
                    else if (child is SetExprOrDefaultContext setExpr)
                    {
                        bool isSubquery = AnalyserHelper.IsSubquery(setExpr);

                        SetStatement statement = statements.Last();

                        statement.Value = this.CreateToken(setExpr);

                        if (isSubquery)
                        {
                            statement.Value.Type = TokenType.Subquery;

                            this.AddChildTableAndColumnNameToken(setExpr, statements.Last().Value);
                        }
                        else
                        {
                            checkUserVaribleDataType(statement);
                        }
                    }
                }
            }

            return statements;
        }

        private SelectStatement ParseDerivedTable(DerivedTableContext node)
        {
            SelectStatement statement = new SelectStatement();

            foreach(var child in node.children)
            {
                if(child is SubqueryContext subquery)
                {
                    statement = this.ParseSubquery(subquery);
                }
            }

            return statement;
        }

        private SelectStatement ParseSubquery(SubqueryContext node)
        {
            SelectStatement statement = new SelectStatement();

            foreach (var child in node.children)
            {
                if (child is QueryExpressionParensContext qe)
                {
                    statement = this.ParseQueryExpression(qe.queryExpression());
                }
            }

            return statement;
        }

        private List<Statement> ParseCompoundStatementList(CompoundStatementListContext node)
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

        private List<Statement> ParseCompoundStatement(CompoundStatementContext node)
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
                    statements.Add(this.ParseLabelControl(labeledControl));
                }
                else if (child is UnlabeledControlContext unlabeledControl)
                {
                    statements.Add(this.ParseUnlabeledControl(unlabeledControl));
                }
                else if (child is UnlabeledBlockContext unlabeledBlock)
                {
                    statements.AddRange(this.ParseUnlabeledBlock(unlabeledBlock));
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

        private DeclareVariableStatement ParseDeclareStatement(VariableDeclarationContext node)
        {
            DeclareVariableStatement statement = new DeclareVariableStatement();

            statement.Name = new TokenInfo(node.identifierList().identifier().First()) { Type = TokenType.VariableName };
            statement.DataType = new TokenInfo(node.dataType());

            var defaultValue = node.expr();

            if (defaultValue != null)
            {
                statement.DefaultValue = new TokenInfo(defaultValue);
            }

            return statement;
        }

        private IfStatement ParseIfStatement(IfStatementContext node)
        {
            IfStatement statement = new IfStatement();

            bool isFirst = true;
            var ifBody = node.ifBody();

            while (ifBody != null)
            {
                IfStatementItem item = new IfStatementItem() { Type = isFirst ? IfStatementType.IF : IfStatementType.ELSEIF };

                var condition = ifBody.expr();

                SimpleExprSubQueryContext subquery = null;

                var children = (condition is ExprNotContext) ? (condition as ExprNotContext).expr().children : condition.children;

                foreach (var child in children)
                {
                    if (child is PrimaryExprPredicateContext pep)
                    {
                        subquery = pep.predicate()?.bitExpr()?.FirstOrDefault()?.simpleExpr() as SimpleExprSubQueryContext;

                        if (subquery != null)
                        {
                            foreach (var c in subquery.children)
                            {
                                if (c is TerminalNodeImpl && c.GetText().ToUpper() == "EXISTS")
                                {
                                    item.ConditionType = IfConditionType.Exists;
                                    break;
                                }
                            }

                            item.CondtionStatement = this.ParseQueryExpression(subquery.subquery().queryExpressionParens()?.queryExpression());
                        }

                        break;
                    }
                }

                if (item.ConditionType == IfConditionType.Exists && (ifBody.expr() is ExprNotContext))
                {
                    item.ConditionType = IfConditionType.NotExists;
                }

                if (subquery == null)
                {
                    item.Condition = this.ParseCondition(condition);
                }

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

        private CaseStatement ParseCaseStatement(CaseStatementContext node)
        {
            CaseStatement statement = new CaseStatement();

            statement.VariableName = new TokenInfo(node.expr()) { Type = TokenType.VariableName };

            var whens = node.whenExpression();
            var thens = node.thenStatement();

            for (int i = 0; i < whens.Length; i++)
            {
                IfStatementItem elseIfItem = new IfStatementItem() { Type = i == 0 ? IfStatementType.IF : IfStatementType.ELSEIF };

                elseIfItem.Condition = new TokenInfo(whens[i].expr()) { Type = TokenType.IfCondition };
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

        private WhileStatement ParseWhileStatement(WhileDoBlockContext node)
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
                    statement.Condition = new TokenInfo(condition) { Type = TokenType.IfCondition };
                }
            }

            return statement;
        }

        private LoopStatement ParseLoopStatement(LoopBlockContext node, LabelContext name)
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

        private Statement ParseLabelControl(LabeledControlContext node)
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

        private Statement ParseUnlabeledControl(UnlabeledControlContext node)
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

        private List<Statement> ParseUnlabeledBlock(UnlabeledBlockContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var child in node.children)
            {
                if (child is BeginEndBlockContext beginEnd)
                {
                    statements.AddRange(this.ParseCompoundStatementList(beginEnd.compoundStatementList()));
                }
            }

            return statements;
        }

        private ReturnStatement ParseReturnStatement(ReturnStatementContext node)
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

        private TransactionStatement ParseTransactionStatement(TransactionStatementContext node)
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

        private LeaveStatement ParseLeaveStatement(LeaveStatementContext node)
        {
            LeaveStatement statement = new LeaveStatement();

            statement.Content = new TokenInfo(node);

            return statement;
        }

        private IterateStatement ParseIterateStatement(IterateStatementContext node)
        {
            IterateStatement statement = new IterateStatement();

            statement.Content = new TokenInfo(node);

            return statement;
        }

        private Statement ParseCreateStatement(CreateStatementContext node)
        {
            Statement statement = null;

            foreach (var child in node.children)
            {
                if (child is CreateTableContext createTable)
                {
                    statement = this.ParseCreateTableStatement(createTable);
                }
            }

            return statement;
        }

        private CreateTableStatement ParseCreateTableStatement(CreateTableContext node)
        {
            CreateTableStatement statement = new CreateTableStatement();

            TableInfo tableInfo = new TableInfo();

            foreach (var child in node.children)
            {
                if (child is TerminalNodeImpl tni)
                {
                    if (tni.GetText().ToUpper() == "TEMPORARY")
                    {
                        tableInfo.IsTemporary = true;
                        break;
                    }
                }
            }

            tableInfo.Name = this.ParseTableName(node.tableName());

            var columns = node.tableElementList().tableElement();

            foreach (var column in columns)
            {
                ColumnInfo columnInfo = new ColumnInfo();

                Action checkConstraints = () =>
                {
                    if (columnInfo.Constraints == null)
                    {
                        columnInfo.Constraints = new List<ConstraintInfo>();
                    }
                };

                var columnDefinition = column.columnDefinition();
                var constraint = column.tableConstraintDef();
                var references = columnDefinition?.checkOrReferences();

                if (columnDefinition != null)
                {
                    var fieldDefiniton = columnDefinition.fieldDefinition();

                    var columnName = columnDefinition.columnName();
                    var dataType = fieldDefiniton.dataType();
                    var attributes = fieldDefiniton.columnAttribute();


                    columnInfo.Name = new ColumnName(columnName);
                    columnInfo.DataType = new TokenInfo(dataType) { Type = TokenType.DataType };

                    foreach (var attr in attributes)
                    {
                        string text = attr.GetText().ToUpper().Trim();

                        if (text.Contains("NOT") && text.Contains("NULL"))
                        {
                            columnInfo.IsNullable = false;
                        }
                        else if (text == "AUTO_INCREMENT")
                        {
                            columnInfo.IsIdentity = true;
                        }
                        else if (text.Contains("UNIQUE"))
                        {
                            checkConstraints();

                            columnInfo.Constraints.Add(new ConstraintInfo() { Type = ConstraintType.UniqueIndex });
                        }
                        else if (text.Contains("KEY"))
                        {
                            checkConstraints();

                            columnInfo.Constraints.Add(new ConstraintInfo() { Type = ConstraintType.PrimaryKey });
                        }
                        else if (text.StartsWith("DEFAULT"))
                        {
                            checkConstraints();

                            columnInfo.DefaultValue = new TokenInfo(attr.GetText().Substring(7));
                        }
                        else if (text.StartsWith("CHECK"))
                        {
                            checkConstraints();

                            columnInfo.Constraints.Add(new ConstraintInfo()
                            {
                                Type = ConstraintType.Check,
                                Definition = new TokenInfo(attr.GetText().Substring(5))
                            });
                        }
                    }

                    tableInfo.Columns.Add(columnInfo);
                }

                if (references != null)
                {
                    ConstraintInfo constraintInfo = new ConstraintInfo() { Type = ConstraintType.ForeignKey };

                    constraintInfo.ForeignKey = this.ParseReferences(references.references());

                    checkConstraints();

                    columnInfo.Constraints.Add(constraintInfo);
                }

                if (constraint != null)
                {
                    ConstraintInfo constraintInfo = null;

                    Action checkConstraintInfo = () =>
                    {
                        if (constraintInfo == null)
                        {
                            constraintInfo = new ConstraintInfo();
                        }
                    };

                    foreach (var child in constraint.children)
                    {
                        if (child is TerminalNodeImpl tni)
                        {
                            ConstraintType constraintType = this.GetConstraintType(tni);

                            if (constraintType != ConstraintType.None)
                            {
                                checkConstraintInfo();

                                constraintInfo.Type = constraintType;
                            }
                        }
                        else if (child is ConstraintNameContext cn)
                        {
                            checkConstraintInfo();

                            constraintInfo.Name = new NameToken(cn.identifier());
                        }
                        else if(child is CheckConstraintContext check)
                        {
                            checkConstraintInfo();

                            constraintInfo.Type = ConstraintType.Check;

                            constraintInfo.Definition = new TokenInfo(check.exprWithParentheses());
                        }
                        else if (child is KeyListVariantsContext klv)
                        {
                            var cols = klv.keyListWithExpression()?.keyPartOrExpression();

                            if (cols != null)
                            {
                                checkConstraintInfo();

                                if (constraintInfo.ColumnNames == null)
                                {
                                    constraintInfo.ColumnNames = new List<ColumnName>();
                                }

                                constraintInfo.ColumnNames.AddRange(cols.Select(item => new ColumnName(item)));
                            }
                        }
                        else if (child is KeyListContext kl)
                        {
                            if (constraintInfo != null && constraintInfo.Type == ConstraintType.ForeignKey)
                            {
                                if (constraintInfo.ForeignKey == null)
                                {
                                    constraintInfo.ForeignKey = new ForeignKeyInfo();
                                }

                                constraintInfo.ForeignKey.ColumnNames.AddRange(kl.keyPart().Select(item => new ColumnName(item)));
                            }
                        }
                        else if (child is ReferencesContext refc)
                        {
                            if (constraintInfo != null && constraintInfo.Type == ConstraintType.ForeignKey)
                            {
                                ForeignKeyInfo fki = this.ParseReferences(refc);

                                constraintInfo.ForeignKey.RefTableName = fki.RefTableName;
                                constraintInfo.ForeignKey.RefColumNames = fki.RefColumNames;
                            }
                        }
                    }

                    if (constraintInfo != null)
                    {
                        if (tableInfo.Constraints == null)
                        {
                            tableInfo.Constraints = new List<ConstraintInfo>();
                        }

                        tableInfo.Constraints.Add(constraintInfo);
                    }
                }
            }

            statement.TableInfo = tableInfo;

            return statement;
        }

        private ForeignKeyInfo ParseReferences(ReferencesContext node)
        {
            ForeignKeyInfo fki = new ForeignKeyInfo();

            fki.RefTableName = new TableName(node.tableRef());
            fki.RefColumNames.AddRange(node.identifierListWithParentheses().identifierList().identifier().Select(item => new ColumnName(item)));

            return fki;
        }

        private TruncateStatement ParseTruncateTableStatement(TruncateTableStatementContext node)
        {
            TruncateStatement statement = new TruncateStatement();

            statement.TableName = this.ParseTableName(node.tableRef());

            return statement;
        }

        protected override TableName ParseTableName(ParserRuleContext node, bool strict = false)
        {
            TableName tableName = null;

            if (node != null)
            {
                if (node is TableNameContext tn)
                {
                    var dotId = tn.dotIdentifier();
                    var qualifiedId = tn.qualifiedIdentifier();

                    if (dotId != null)
                    {
                        tableName = new TableName(dotId);
                    }
                    else if (qualifiedId != null)
                    {
                        tableName = new TableName(qualifiedId);
                    }
                    else
                    {
                        tableName = new TableName(tn);
                    }
                }
                else if (node is TableRefContext tableRef)
                {
                    tableName = new TableName(tableRef.qualifiedIdentifier());
                }
                else if (node is TableFactorContext tableFactor)
                {
                    var singleTable = tableFactor.singleTable();

                    if (singleTable != null)
                    {
                        tableName = this.ParseTableName(singleTable);
                    }
                    else
                    {
                        tableName = new TableName(tableFactor);
                    }
                }
                else if (node is TableReferenceContext tableReference)
                {
                    var tfc = tableReference.tableFactor();

                    if (tfc != null)
                    {
                        tableName = this.ParseTableName(tfc);
                    }
                    else
                    {
                        tableName = new TableName(tableReference);
                    }
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
        protected override ColumnName ParseColumnName(ParserRuleContext node, bool strict = false)
        {
            ColumnName columnName = null;

            if (node != null)
            {
                if (node is SelectItemContext si)
                {
                    var expr = si.expr();

                    if (expr is ExprIsContext)
                    {
                        columnName = this.ParseColumnName(expr);
                    }
                    else
                    {
                        columnName = new ColumnName(expr);

                        var alias = si.selectAlias();

                        if (alias != null)
                        {
                            columnName.HasAs = this.HasAsFlag(alias);
                            columnName.Alias = new TokenInfo(alias.identifier());
                        }

                        this.AddChildTableAndColumnNameToken(si, columnName);
                    }
                }
                else if (node is QualifiedIdentifierContext qualifiedIdentifier)
                {
                    var id = qualifiedIdentifier.identifier();
                    var dotId = qualifiedIdentifier.dotIdentifier();

                    if (dotId == null)
                    {
                        columnName = new ColumnName(id);
                    }
                    else
                    {
                        columnName = new ColumnName(qualifiedIdentifier);
                        columnName.TableName = new TableName(id);
                    }
                }
                else if (node is ColumnRefContext columnRef)
                {
                    var qi = columnRef.fieldIdentifier()?.qualifiedIdentifier();

                    if (qi != null)
                    {
                        columnName = this.ParseColumnName(qi);
                    }
                    else
                    {
                        columnName = new ColumnName(columnRef.fieldIdentifier());
                    }
                }
                else if (node is ExprIsContext expr)
                {
                    var detail = this.GetColumnDetailContext(expr);

                    if (detail is NumLiteralContext nl)
                    {
                        columnName = new ColumnName(nl) { IsConst = true };
                    }
                    else if (detail is FunctionCallContext fc)
                    {
                        columnName = new ColumnName(fc);
                    }
                    else
                    {
                        columnName = new ColumnName(expr);
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
                    else if (child is FunctionCallContext fc)
                    {
                        return fc;
                    }
                    else
                    {
                        return this.GetColumnDetailContext(child as ParserRuleContext);
                    }
                }
            }

            return node;
        }

        protected override TokenInfo ParseTableAlias(ParserRuleContext node)
        {
            if (node != null)
            {
                if (node is TableAliasContext alias)
                {
                    return new TokenInfo(alias.identifier()) { Type = TokenType.TableAlias };
                }
            }

            return null;
        }

        protected override TokenInfo ParseColumnAlias(ParserRuleContext node)
        {
            if (node != null)
            {
                if (node is SelectAliasContext alias)
                {
                    return new TokenInfo(alias.identifier()) { Type = TokenType.ColumnAlias };
                }
            }

            return null;
        }

        private TokenInfo ParseCondition(ParserRuleContext node)
        {
            if (node != null)
            {
                if (node is ExprContext || node is ExprIsContext)
                {
                    TokenInfo token = this.CreateToken(node);

                    bool isIfCondition = node.Parent != null && (node.Parent is IfBodyContext || node.Parent is LeaveStatementContext);

                    bool isSearchConditionHasSubquery = this.IsSearchConditionHasSubquery(node);

                    if (isIfCondition && !isSearchConditionHasSubquery)
                    {
                        token.Type = TokenType.IfCondition;
                    }
                    else
                    {
                        token.Type = TokenType.SearchCondition;
                    }

                    if (token.Type == TokenType.SearchCondition)
                    {
                        this.AddChildTableAndColumnNameToken(node, token);
                    }

                    return token;
                }
            }

            return null;
        }

        protected override bool IsFunction(IParseTree node)
        {
            if (node is FunctionCallContext || node is RuntimeFunctionCallContext
                || node is SimpleExprConvertContext || node is SimpleExprCastContext)
            {
                return true;
            }

            return false;
        }

        private bool IsSearchConditionHasSubquery(ParserRuleContext node)
        {
            foreach (var child in node.children)
            {
                if (child is SubqueryContext)
                {
                    return true;
                }
                else if (child is ParserRuleContext prc)
                {
                    return this.IsSearchConditionHasSubquery(prc);
                }
            }

            return false;
        }
    }
}
