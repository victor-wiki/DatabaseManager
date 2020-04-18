using Antlr4.Runtime.Tree;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static MySqlParser;
using Antlr4.Runtime;
using DatabaseInterpreter.Model;

namespace SqlAnalyser.Core
{
    public class MySqlRuleAnalyser : SqlRuleAnalyser
    {
        public override Lexer GetLexer(string content)
        {
            return new MySqlLexer(this.GetCharStreamFromString(content));
        }

        public override Parser GetParser(CommonTokenStream tokenStream)
        {
            return new MySqlParser(tokenStream);
        }

        public RootContext GetRootContext(string content)
        {
            MySqlParser parser = this.GetParser(content) as MySqlParser;

            RootContext context = parser.root();

            return context;
        }

        public DdlStatementContext GetDdlStatementContext(string content)
        {
            RootContext rootContext = this.GetRootContext(content);

            return rootContext?.sqlStatements()?.sqlStatement()?.Select(item => item?.ddlStatement()).FirstOrDefault();
        }

        public override RoutineScript AnalyseProcedure(string content)
        {
            DdlStatementContext ddlStatement = this.GetDdlStatementContext(content);

            RoutineScript script = new RoutineScript();

            if (ddlStatement != null)
            {
                CreateProcedureContext proc = ddlStatement.createProcedure();

                if (proc != null)
                {
                    #region Name
                    this.SetScriptName(script, proc.fullId());
                    #endregion

                    #region Parameters
                    ProcedureParameterContext[] parameters = proc.procedureParameter();

                    if (parameters != null)
                    {
                        foreach (ProcedureParameterContext parameter in parameters)
                        {
                            Parameter parameterInfo = new Parameter();

                            UidContext uid = parameter.uid();

                            parameterInfo.Name = new TokenInfo(uid) { Type = TokenType.ParameterName };

                            parameterInfo.DataType = new TokenInfo(parameter.dataType().GetText()) { Type = TokenType.DataType };

                            this.SetParameterType(parameterInfo, parameter.children);

                            script.Parameters.Add(parameterInfo);
                        }
                    }
                    #endregion

                    #region Body

                    this.SetScriptBody(script, proc.routineBody());

                    #endregion
                }
            }

            this.ExtractFunctions(script, ddlStatement);

            return script;
        }

        public void SetScriptBody(CommonScript script, RoutineBodyContext body)
        {
            script.Statements.AddRange(this.ParseRoutineBody(body));
        }

        public List<Statement> ParseRoutineBody(RoutineBodyContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var child in node.children)
            {
                if (child is BlockStatementContext block)
                {
                    statements.AddRange(this.ParseBlockStatement(block));
                }
                else if (child is SqlStatementContext sqlStatement)
                {
                    statements.AddRange(this.ParseSqlStatement(sqlStatement));
                }
            }

            return statements;
        }

        public List<Statement> ParseBlockStatement(BlockStatementContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var bc in node.children)
            {
                if (bc is DeclareVariableContext declare)
                {
                    statements.Add(this.ParseDeclareStatement(declare));
                }
                else if (bc is ProcedureSqlStatementContext procStatement)
                {
                    statements.AddRange(this.ParseProcedureStatement(procStatement));
                }
                else if (bc is DeclareCursorContext cursor)
                {
                    statements.Add(this.ParseDeclareCursor(cursor));
                }
                else if (bc is DeclareHandlerContext handler)
                {
                    statements.Add(this.ParseDeclareHandler(handler));
                }
            }

            return statements;
        }

        public void SetScriptName(CommonScript script, FullIdContext idContext)
        {
            UidContext[] ids = idContext.uid();

            var name = ids.Last();

            script.Name = new TokenInfo(name);

            if (ids.Length > 1)
            {
                script.Owner = new TokenInfo(ids.First());
            }
        }

        public void SetParameterType(Parameter parameterInfo, IList<IParseTree> nodes)
        {
            foreach (var child in nodes)
            {
                if (child is TerminalNodeImpl terminalNode)
                {
                    if (terminalNode.Symbol.Type == MySqlParser.IN)
                    {
                        parameterInfo.ParameterType = ParameterType.IN;
                    }
                    else if (terminalNode.Symbol.Type == MySqlParser.OUT)
                    {
                        parameterInfo.ParameterType = ParameterType.OUT;
                    }
                    else if (terminalNode.Symbol.Type == MySqlParser.INOUT)
                    {
                        parameterInfo.ParameterType = ParameterType.IN | ParameterType.OUT;
                    }
                }
            }
        }

        public override RoutineScript AnalyseFunction(string content)
        {
            DdlStatementContext ddlStatement = this.GetDdlStatementContext(content);

            RoutineScript script = new RoutineScript();

            if (ddlStatement != null)
            {
                CreateFunctionContext func = ddlStatement.createFunction();

                if (func != null)
                {
                    #region Name
                    this.SetScriptName(script, func.fullId());
                    #endregion

                    #region Parameters
                    FunctionParameterContext[] parameters = func.functionParameter();

                    if (parameters != null)
                    {
                        foreach (FunctionParameterContext parameter in parameters)
                        {
                            Parameter parameterInfo = new Parameter();

                            UidContext uid = parameter.uid();

                            parameterInfo.Name = new TokenInfo(uid) { Type = TokenType.ParameterName };

                            parameterInfo.DataType = new TokenInfo(parameter.dataType().GetText()) { Type = TokenType.DataType };

                            this.SetParameterType(parameterInfo, parameter.children);

                            script.Parameters.Add(parameterInfo);
                        }
                    }
                    #endregion

                    script.ReturnDataType = new TokenInfo(func.dataType().GetText()) { Type = TokenType.DataType };

                    #region Body

                    this.SetScriptBody(script, func.routineBody());

                    #endregion
                }
            }

            this.ExtractFunctions(script, ddlStatement);

            return script;
        }

        public override ViewScript AnalyseView(string content)
        {
            DdlStatementContext ddlStatement = this.GetDdlStatementContext(content);

            ViewScript script = new ViewScript();

            if (ddlStatement != null)
            {
                CreateViewContext view = ddlStatement.createView();

                if (view != null)
                {
                    #region Name
                    this.SetScriptName(script, view.fullId());
                    #endregion                  

                    #region Statement

                    foreach (var child in view.children)
                    {
                        if (child is SimpleSelectContext select)
                        {
                            script.Statements.Add(this.ParseSelectStatement(select));
                        }
                    }

                    #endregion
                }
            }

            this.ExtractFunctions(script, ddlStatement);

            return script;
        }

        public override TriggerScript AnalyseTrigger(string content)
        {
            DdlStatementContext ddlStatement = this.GetDdlStatementContext(content);

            TriggerScript script = new TriggerScript();

            if (ddlStatement != null)
            {
                CreateTriggerContext trigger = ddlStatement.createTrigger();

                if (trigger != null)
                {
                    #region Name

                    FullIdContext[] ids = trigger.fullId();

                    this.SetScriptName(script, ids.First());

                    if (ids.Length > 1)
                    {
                        script.OtherTriggerName = new TokenInfo(ids[1]);
                    }

                    #endregion

                    script.TableName = new TokenInfo(trigger.tableName()) { Type = TokenType.TableName };

                    foreach (var child in trigger.children)
                    {
                        if (child is TerminalNodeImpl terminalNode)
                        {
                            switch (terminalNode.Symbol.Type)
                            {
                                case MySqlParser.BEFORE:
                                    script.Time = TriggerTime.BEFORE;
                                    break;
                                case MySqlParser.AFTER:
                                    script.Time = TriggerTime.AFTER;
                                    break;
                                case MySqlParser.INSERT:
                                    script.Events.Add(TriggerEvent.INSERT);
                                    break;
                                case MySqlParser.UPDATE:
                                    script.Events.Add(TriggerEvent.UPDATE);
                                    break;
                                case MySqlParser.DELETE:
                                    script.Events.Add(TriggerEvent.DELETE);
                                    break;
                                case MySqlParser.PRECEDES:
                                    script.Behavior = nameof(MySqlParser.PRECEDES);
                                    break;
                                case MySqlParser.FOLLOWS:
                                    script.Behavior = nameof(MySqlParser.FOLLOWS);
                                    break;
                            }
                        }
                    }

                    #region Body

                    this.SetScriptBody(script, trigger.routineBody());

                    #endregion
                }
            }

            this.ExtractFunctions(script, ddlStatement);

            return script;
        }

        public List<Statement> ParseProcedureStatement(ProcedureSqlStatementContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var child in node.children)
            {
                if (child is SqlStatementContext sqlStatement)
                {
                    statements.AddRange(this.ParseSqlStatement(sqlStatement));
                }
                else if (child is CompoundStatementContext compoundStatement)
                {
                    statements.AddRange(this.ParseCompoundStatement(compoundStatement));
                }
            }

            return statements;
        }

        public DeclareCursorStatement ParseDeclareCursor(DeclareCursorContext node)
        {
            DeclareCursorStatement statement = new DeclareCursorStatement();

            statement.CursorName = new TokenInfo(node.uid()) { Type = TokenType.CursorName };
            statement.SelectStatement = this.ParseSelectStatement(node.selectStatement());

            return statement;
        }

        public DeclareCursorHandlerStatement ParseDeclareHandler(DeclareHandlerContext node)
        {
            DeclareCursorHandlerStatement statement = new DeclareCursorHandlerStatement();

            statement.Conditions.AddRange(node.handlerConditionValue().Select(item => new TokenInfo(item) { Type = TokenType.Condition }));
            statement.Statements.AddRange(this.ParseRoutineBody(node.routineBody()));

            return statement;
        }

        public OpenCursorStatement ParseOpenCursorSatement(OpenCursorContext node)
        {
            OpenCursorStatement statement = new OpenCursorStatement();

            statement.CursorName = new TokenInfo(node.uid()) { Type = TokenType.CursorName };

            return statement;
        }

        public FetchCursorStatement ParseFetchCursorSatement(FetchCursorContext node)
        {
            FetchCursorStatement statement = new FetchCursorStatement();

            statement.CursorName = new TokenInfo(node.uid()) { Type = TokenType.CursorName };
            statement.Variables.AddRange(node.uidList().uid().Select(item => new TokenInfo(item) { Type = TokenType.VariableName }));

            return statement;
        }

        public CloseCursorStatement ParseCloseCursorSatement(CloseCursorContext node)
        {
            CloseCursorStatement statement = new CloseCursorStatement() { IsEnd = true };

            statement.CursorName = new TokenInfo(node.uid()) { Type = TokenType.CursorName };

            return statement;
        }

        public List<Statement> ParseSqlStatement(SqlStatementContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var child in node.children)
            {
                if (child is AdministrationStatementContext admin)
                {
                    foreach (var adminChild in admin.children)
                    {
                        if (adminChild is SetStatementContext set)
                        {
                            statements.AddRange(this.ParseSetStatement(set));
                        }
                    }
                }
                else if (child is DmlStatementContext dml)
                {
                    statements.AddRange(this.ParseDmlStatement(dml));
                }
                else if (child is TransactionStatementContext transaction)
                {
                    statements.Add(this.ParseTransactionStatement(transaction));
                }               
            }

            return statements;
        }

        public CallStatement ParseCallStatement(CallStatementContext node)
        {
            CallStatement statement = new CallStatement();

            statement.Name = new TokenInfo(node.fullId()) { Type = TokenType.RoutineName };

            ExpressionContext []expressions = node.expressions().expression();

            if(expressions!=null && expressions.Length>0)
            {
                statement.Arguments.AddRange(expressions.Select(item => new TokenInfo(item)));
            }            

            return statement;
        }

        public List<Statement> ParseDmlStatement(DmlStatementContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var child in node.children)
            {
                if (child is InsertStatementContext insert)
                {
                    InsertStatement statement = this.ParseInsertStatement(insert);

                    statements.Add(statement);
                }
                else if (child is DeleteStatementContext delete)
                {
                    statements.AddRange(this.ParseDeleteStatement(delete));
                }
                else if (child is UpdateStatementContext update)
                {
                    statements.Add(this.ParseUpdateStatement(update));
                }
                else if (child is SimpleSelectContext selectContext)
                {
                    SelectStatement statement = this.ParseSelectStatement(selectContext);

                    statements.Add(statement);
                }
                else if (child is CallStatementContext call)
                {
                    statements.Add(this.ParseCallStatement(call));
                }
            }

            return statements;
        }

        public InsertStatement ParseInsertStatement(InsertStatementContext node)
        {
            InsertStatement statement = new InsertStatement();

            foreach (var child in node.children)
            {
                if (child is TableNameContext tableName)
                {
                    statement.TableName = this.ParseTableName(tableName);
                }
                else if (child is UidListContext columns)
                {
                    foreach (var col in columns.children)
                    {
                        if (col is UidContext colId)
                        {
                            TokenInfo tokenInfo = new TokenInfo(colId) { Type = TokenType.ColumnName };

                            statement.Columns.Add(this.ParseColumnName(colId));
                        }
                    }
                }
                else if (child is InsertStatementValueContext values)
                {
                    foreach (var v in values.children)
                    {
                        if (v is ExpressionsWithDefaultsContext exp)
                        {
                            foreach (var expChild in exp.children)
                            {
                                if (expChild is ExpressionOrDefaultContext value)
                                {
                                    TokenInfo valueInfo = new TokenInfo(value);

                                    statement.Values.Add(valueInfo);
                                }
                            }
                        }
                    }
                }
            }

            return statement;
        }

        public UpdateStatement ParseUpdateStatement(UpdateStatementContext node)
        {
            UpdateStatement statement = new UpdateStatement();

            SingleUpdateStatementContext single = node.singleUpdateStatement();
            MultipleUpdateStatementContext multiple = node.multipleUpdateStatement();

            UpdatedElementContext[] updateItems = null;
            ExpressionContext condition = null;

            if (single != null)
            {
                statement.TableNames.Add(this.ParseTableName(single.tableName()));

                updateItems = single.updatedElement();
                condition = single.expression();
            }
            else if (multiple != null)
            {
                updateItems = multiple.updatedElement();
                condition = multiple.expression();

                statement.FromItems = new List<FromItem>();

                TableSourcesContext tableSources = multiple.tableSources();

                statement.FromItems = this.ParseTableSources(tableSources);
                statement.TableNames.AddRange(statement.FromItems.Select(item => item.TableName));                
            }

            if (updateItems != null)
            {
                statement.SetItems.AddRange(updateItems.Select(item =>
                               new NameValueItem()
                               {
                                   Name = new TokenInfo(item.fullColumnName()) { Type = TokenType.ColumnName },
                                   Value = this.ParseToken(item.expression())
                               }));
            }

            statement.Condition = this.ParseCondition(condition);

            return statement;
        }

        public List<FromItem> ParseTableSources(TableSourcesContext node)
        {
            List<FromItem> fromItems = new List<FromItem>();

            TableSourceContext[] tables = node.tableSource();

            foreach (TableSourceContext table in tables)
            {
                if (table is TableSourceBaseContext tb)
                {
                    TableSourceItemContext name = tb.tableSourceItem();

                    TableName tableName = this.ParseTableName(name);

                    FromItem fromItem = new FromItem();
                    fromItem.TableName = tableName;

                    JoinPartContext[] joins = tb.joinPart();
                   
                    if (joins != null && joins.Length > 0)
                    {
                        foreach (JoinPartContext join in joins)
                        {
                            JoinItem joinItem = this.ParseJoin(join);

                            fromItem.JoinItems.Add(joinItem);
                        }                        
                    }                   

                    fromItems.Add(fromItem);
                }
            }

            return fromItems;
        }

        public List<DeleteStatement> ParseDeleteStatement(DeleteStatementContext node)
        {
            List<DeleteStatement> statements = new List<DeleteStatement>();

            SingleDeleteStatementContext single = node.singleDeleteStatement();
            MultipleDeleteStatementContext multiple = node.multipleDeleteStatement();

            if (single != null)
            {
                DeleteStatement statement = new DeleteStatement();
                statement.TableName = this.ParseTableName(single.tableName());
                statement.Condition = this.ParseCondition(single.expression());

                statements.Add(statement);
            }

            return statements;
        }

        public SelectStatement ParseSelectStatement(SelectStatementContext node)
        {
            SelectStatement statement = new SelectStatement();

            foreach (var child in node.children)
            {
                if (child is QuerySpecificationContext query)
                {
                    statement = this.ParseQuerySpecification(query);
                }
                else if (child is UnionSelectContext union)
                {
                    var spec = union.querySpecification();
                    var specNointo = union.querySpecificationNointo();

                    if (spec != null)
                    {
                        statement = this.ParseQuerySpecification(spec);
                    }
                    else if (specNointo != null)
                    {
                        statement = this.ParseQuerySpecification(specNointo);
                    }

                    statement.UnionStatements.AddRange(union.unionStatement().Select(item => this.ParseUnionSatement(item)));
                }
            }

            return statement;
        }

        public SelectStatement ParseQuerySpecification(ParserRuleContext node)
        {
            SelectStatement statement = new SelectStatement();

            foreach (var qc in node.children)
            {
                if (qc is SelectElementsContext elements)
                {
                    foreach (var el in elements.children)
                    {
                        if (!(el is TerminalNodeImpl))
                        {
                            statement.Columns.Add(this.ParseColumnName(el as ParserRuleContext));
                        }
                    }
                }
                else if (qc is SelectIntoVariablesContext into)
                {
                    foreach (var ic in into.children)
                    {
                        if (ic is AssignmentFieldContext field)
                        {
                            statement.IntoTableName = new TokenInfo(field) { Type = TokenType.TableName };
                        }
                    }
                }
                else if (qc is FromClauseContext from)
                {
                    foreach (var fc in from.children)
                    {
                        if (fc is TableSourceContext t)
                        {
                            statement.TableName = this.ParseTableName(t);
                        }
                        else if (fc is TableSourcesContext ts)
                        {             
                            statement.FromItems = this.ParseTableSources(ts);
                        }
                        else if (fc is PredicateExpressionContext exp)
                        {
                            statement.Where = this.ParseCondition(exp);
                        }
                        else if (fc is LogicalExpressionContext logic)
                        {
                            statement.Where = this.ParseCondition(logic);
                        }
                    }
                }
                else if (qc is LimitClauseContext limit)
                {
                    statement.LimitInfo = new SelectLimitInfo();

                    LimitClauseAtomContext[] items = limit.limitClauseAtom();

                    statement.LimitInfo.StartRowIndex = long.Parse(items[0].GetText());
                    statement.LimitInfo.RowCount = long.Parse(items[1].GetText());
                }
            }

            return statement;
        }

        public UnionStatement ParseUnionSatement(UnionStatementContext node)
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
                else if (child is QuerySpecificationContext ||
                         child is QuerySpecificationNointoContext)
                {
                    statement.Type = unionType;
                    statement.SelectStatement = this.ParseQuerySpecification(child as ParserRuleContext);
                }
            }

            return statement;
        }

        public List<SetStatement> ParseSetStatement(SetStatementContext node)
        {
            List<SetStatement> statements = new List<SetStatement>();

            foreach (var child in node.children)
            {
                if (child is VariableClauseContext variable)
                {
                    SetStatement statement = new SetStatement();

                    statement.Key = new TokenInfo(variable);

                    statements.Add(statement);
                }
                else if (child is PredicateExpressionContext exp)
                {
                    statements.Last().Value = this.ParseToken(exp);
                }
            }

            return statements;
        }

        public string ParseExpressionAtom(ExpressionAtomPredicateContext node)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var child in node.children)
            {
                if (child is ConstantExpressionAtomContext constantExp)
                {
                    string text = this.ParseConstExpression(constantExp);

                    sb.Append(text);
                }
                else if (child is MysqlVariableExpressionAtomContext variableExp)
                {
                    string text = variableExp.GetText();

                    sb.Append(text);
                }
                else if (child is FullColumnNameExpressionAtomContext columnNameExp)
                {
                    string text = columnNameExp.GetText();

                    sb.Append(text);
                }
                else if (child is MathExpressionAtomContext mathExp)
                {
                    string text = this.ParseMathExpression(mathExp);

                    sb.Append(text);
                }
                else if (child is NestedExpressionAtomContext nested)
                {
                    string text = nested.GetText();

                    sb.Append(text);
                }
                else if (child is FunctionCallExpressionAtomContext func)
                {
                    string text = func.GetText();

                    sb.Append(text);
                }
            }

            return sb.ToString();
        }

        public string ParseConstExpression(ConstantExpressionAtomContext node)
        {
            string text = node.GetText();
            return text;
        }

        public string ParseMathExpression(MathExpressionAtomContext node)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var child in node.children)
            {
                if (child is MysqlVariableExpressionAtomContext variableExp)
                {
                    string text = variableExp.GetText();
                    sb.Append(text);
                }
                else if (child is MathOperatorContext @operator)
                {
                    string text = @operator.GetText();
                    sb.Append(text);
                }
                else if (child is ConstantExpressionAtomContext constant)
                {
                    string text = this.ParseConstExpression(constant);
                    sb.Append(text);
                }
                else if (child is FunctionCallExpressionAtomContext func)
                {
                    string text = func.GetText();

                    sb.Append(text);
                }
            }

            return sb.ToString();
        }

        public List<Statement> ParseCompoundStatement(CompoundStatementContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var child in node.children)
            {
                if (child is IfStatementContext @if)
                {
                    statements.Add(this.ParseIfStatement(@if));
                }
                else if (child is CaseStatementContext @case)
                {
                    statements.Add(this.ParseCaseStatement(@case));
                }
                else if (child is WhileStatementContext @while)
                {
                    statements.Add(this.ParseWhileStatement(@while));
                }
                else if (child is LoopStatementContext loop)
                {
                    statements.Add(this.ParseLoopStatement(loop));
                }
                else if (child is ReturnStatementContext returnStatement)
                {
                    statements.Add(this.ParseReturnStatement(returnStatement));
                }
                else if (child is BlockStatementContext block)
                {
                    statements.AddRange(this.ParseBlockStatement(block));
                }
                else if (child is LeaveStatementContext leave)
                {
                    statements.Add(this.ParseLeaveStatement(leave));
                }
                else if (child is OpenCursorContext openCursor)
                {
                    statements.Add(this.ParseOpenCursorSatement(openCursor));
                }
                else if (child is FetchCursorContext fetchCursor)
                {
                    statements.Add(this.ParseFetchCursorSatement(fetchCursor));
                }
                else if (child is CloseCursorContext closeCursor)
                {
                    statements.Add(this.ParseCloseCursorSatement(closeCursor));
                }
            }

            return statements;
        }

        public DeclareStatement ParseDeclareStatement(DeclareVariableContext node)
        {
            DeclareStatement statement = new DeclareStatement();

            statement.Name = new TokenInfo(node.uidList().uid().First()) { Type = TokenType.VariableName };
            statement.DataType = new TokenInfo(node.dataType().GetText()) { Type = TokenType.DataType };

            var defaultValue = node.defaultValue();

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
            ifItem.Condition = new TokenInfo(node.expression() as PredicateExpressionContext) { Type = TokenType.Condition };
            ifItem.Statements.AddRange(this.ParseProcedureStatement(node._procedureSqlStatement));
            statement.Items.Add(ifItem);

            foreach (ElifAlternativeContext elseif in node.elifAlternative())
            {
                IfStatementItem elseIfItem = new IfStatementItem() { Type = IfStatementType.ELSEIF };
                elseIfItem.Condition = new TokenInfo(elseif.expression() as PredicateExpressionContext) { Type = TokenType.Condition };
                elseIfItem.Statements.AddRange(elseif.procedureSqlStatement().SelectMany(item => this.ParseProcedureStatement(item)));

                statement.Items.Add(elseIfItem);
            }

            if (node._elseStatements.Count > 0)
            {
                IfStatementItem elseItem = new IfStatementItem() { Type = IfStatementType.ELSE };
                elseItem.Statements.AddRange(node._elseStatements.SelectMany(item => this.ParseProcedureStatement(item)));

                statement.Items.Add(elseItem);
            }

            return statement;
        }

        public CaseStatement ParseCaseStatement(CaseStatementContext node)
        {
            CaseStatement statement = new CaseStatement();

            statement.VariableName = new TokenInfo(node.uid()) { Type = TokenType.VariableName };

            foreach (CaseAlternativeContext when in node.caseAlternative())
            {
                IfStatementItem elseIfItem = new IfStatementItem() { Type = IfStatementType.ELSEIF };
                elseIfItem.Condition = new TokenInfo(when.expression() as PredicateExpressionContext) { Type = TokenType.Condition };
                elseIfItem.Statements.AddRange(when.procedureSqlStatement().SelectMany(item => this.ParseProcedureStatement(item)));

                statement.Items.Add(elseIfItem);
            }

            ProcedureSqlStatementContext[] sqls = node.procedureSqlStatement();

            if (sqls != null && sqls.Length > 0)
            {
                IfStatementItem elseItem = new IfStatementItem() { Type = IfStatementType.ELSE };
                elseItem.Statements.AddRange(sqls.SelectMany(item => this.ParseProcedureStatement(item)));

                statement.Items.Add(elseItem);
            }

            return statement;
        }

        public WhileStatement ParseWhileStatement(WhileStatementContext node)
        {
            WhileStatement statement = new WhileStatement();

            foreach (var child in node.children)
            {
                if (child is ProcedureSqlStatementContext procedure)
                {
                    statement.Statements.AddRange(this.ParseProcedureStatement(procedure));
                }
                else if (child is PredicateExpressionContext exp)
                {
                    statement.Condition = new TokenInfo(exp) { Type = TokenType.Condition };
                }
                else if (child is LogicalExpressionContext logic)
                {
                    statement.Condition = new TokenInfo(logic) { Type = TokenType.Condition };
                }
            }

            return statement;
        }

        public LoopStatement ParseLoopStatement(LoopStatementContext node)
        {
            LoopStatement statement = new LoopStatement();

            statement.Name = new TokenInfo(node.uid().First());

            foreach (var child in node.children)
            {
                if (child is ProcedureSqlStatementContext sql)
                {
                    statement.Statements.AddRange(this.ParseProcedureStatement(sql));
                }
            }

            return statement;
        }

        public ReturnStatement ParseReturnStatement(ReturnStatementContext node)
        {
            ReturnStatement statement = new ReturnStatement();

            foreach (var child in node.children)
            {
                if (child is PredicateExpressionContext predicate)
                {
                    statement.Value = new TokenInfo(predicate);
                }
            }

            return statement;
        }

        public TransactionStatement ParseTransactionStatement(TransactionStatementContext node)
        {
            TransactionStatement statement = new TransactionStatement();
            statement.Content = new TokenInfo(node);

            if (node.startTransaction() != null)
            {
                statement.CommandType = TransactionCommandType.BEGIN;
            }
            else if (node.commitWork() != null)
            {
                statement.CommandType = TransactionCommandType.COMMIT;
            }
            else if (node.rollbackStatement() != null)
            {
                statement.CommandType = TransactionCommandType.ROLLBACK;
            }

            return statement;
        }

        public LeaveStatement ParseLeaveStatement(LeaveStatementContext node)
        {
            LeaveStatement statement = new LeaveStatement();

            statement.Content = new TokenInfo(node);

            return statement;
        }

        public override TableName ParseTableName(ParserRuleContext node)
        {
            TableName tableName = null;

            if (node != null)
            {
                if(node is TableNameContext tn)
                {
                    tableName = new TableName(tn);
                    tableName.Name = new TokenInfo(tn.fullId());
                }
                else if (node is AtomTableItemContext ati)
                {
                    tableName = new TableName(ati);

                    tableName.Name = new TokenInfo(ati.tableName());

                    UidContext alias = ati.uid();

                    if (alias != null)
                    {
                        tableName.Alias = new TokenInfo(alias);
                    }
                }
                else if (node is TableSourceBaseContext tsb)
                {
                    return this.ParseTableName(tsb.tableSourceItem());
                }              
                else if (node is TableSourceContext ts)
                {
                    return this.ParseTableName(ts.children.FirstOrDefault(item => item is TableSourceBaseContext) as ParserRuleContext);
                }
                else if (node is SingleUpdateStatementContext update)
                {
                    tableName = new TableName(update);

                    tableName.Name = new TokenInfo(update.tableName());

                    UidContext alias = update.uid();

                    if (alias != null)
                    {
                        tableName.Alias = new TokenInfo(alias);
                    }
                }

                if (tableName == null)
                {
                    tableName = new TableName(node);
                }
            }

            return tableName;
        }
        public override ColumnName ParseColumnName(ParserRuleContext node)
        {
            ColumnName columnName = null;

            if(node!=null)
            {
                if(node is UidContext uid)
                {
                    columnName = new ColumnName(uid);
                    columnName.Name = new TokenInfo(uid.simpleId());
                }

                if(columnName == null)
                {
                    columnName = new ColumnName(node);
                }
            }

            return columnName;
        }

        public JoinItem ParseJoin(JoinPartContext node)
        {
            JoinItem joinItem = new JoinItem();

            if(node.children.Count>0 && node.children[0] is TerminalNodeImpl terminalNode)
            {
                int type = terminalNode.Symbol.Type;
                switch(type)
                {
                    case MySqlParser.LEFT:
                        joinItem.Type = JoinType.LEFT;
                        break;
                    case MySqlParser.RIGHT:
                        joinItem.Type = JoinType.RIGHT;
                        break;
                    case MySqlParser.FULL:
                        joinItem.Type = JoinType.FULL;
                        break;
                    case MySqlParser.CROSS:
                        joinItem.Type = JoinType.CROSS;
                        break;
                }
            }

            if(node is InnerJoinContext innerJoin)
            {
                joinItem.TableName = this.ParseTableName(innerJoin.tableSourceItem());
                joinItem.Condition = this.ParseCondition(innerJoin.expression());
            }
            else if(node is NaturalJoinContext naturalJoin)
            {
                joinItem.TableName = this.ParseTableName(naturalJoin.tableSourceItem());                
            }
            else if(node is OuterJoinContext outerJoin)
            {
                joinItem.TableName = this.ParseTableName(outerJoin.tableSourceItem());
                joinItem.Condition = this.ParseCondition(outerJoin.expression());
            }

            return joinItem;
        }

        private TokenInfo ParseCondition(ParserRuleContext node)
        {
            if (node != null)
            {
                if (node is ExpressionContext)
                {
                    return this.ParseToken(node, TokenType.Condition);
                }
            }

            return null;
        }

        public override bool IsFunction(IParseTree node)
        {
            if (node is FunctionCallContext)
            {
                return true;
            }

            return false;
        }

        public override bool IsTableName(IParseTree node, out ParserRuleContext parsedNode)
        {
            parsedNode = null;

            if (node is TableNameContext tableName)
            {
                parsedNode = tableName;
                return true;
            }

            return false;
        }

        public override bool IsColumnName(IParseTree node, out ParserRuleContext parsedNode)
        {
            parsedNode = null;

            if (node is FullColumnNameContext columnName)
            {
                UidContext uid = columnName.uid();
                DottedIdContext[] ids = columnName.dottedId();

                if (ids != null && ids.Length > 0)
                {
                    parsedNode = ids[0];
                }
                else if (uid != null)
                {
                    parsedNode = uid;
                }

                return true;
            }

            return false;
        }

        public override bool IsRoutineName(IParseTree node, out ParserRuleContext parsedNode)
        {
            parsedNode = null;

            if (node is UdfFunctionCallContext udf)
            {
                parsedNode = udf.fullId();

                return true;
            }

            return false;
        }
    }
}
