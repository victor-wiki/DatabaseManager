using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using static PostgreSqlParser;

namespace SqlAnalyser.Core
{
    public class PostgreSqlRuleAnalyser : SqlRuleAnalyser
    {
        public PostgreSqlRuleAnalyser(string content) : base(content)
        {
        }

        public override IEnumerable<Type> ParseTableTypes => new List<Type>() { typeof(Qualified_nameContext) };

        public override IEnumerable<Type> ParseColumnTypes => new List<Type>() { typeof(ColumnrefContext) };
        public override IEnumerable<Type> ParseTableAliasTypes => new List<Type>() { typeof(Alias_clauseContext) };
        public override IEnumerable<Type> ParseColumnAliasTypes => new List<Type>() { };

        protected override Lexer GetLexer()
        {
            return new PostgreSqlLexer(this.GetCharStreamFromString());
        }

        protected override Parser GetParser(CommonTokenStream tokenStream)
        {
            return new PostgreSqlParser(tokenStream);
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

            RootContext rootContext = this.GetRootContext(out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && rootContext != null)
            {
                var stmts = rootContext.stmtblock()?.stmtmulti()?.stmt();

                if (stmts != null)
                {
                    CommonScript script = new CommonScript();

                    foreach (var stmt in stmts)
                    {
                        script.Statements.AddRange(this.ParseStmt(stmt));
                    }

                    this.ExtractFunctions(script, rootContext);

                    result.Script = script;
                }
            }

            return result;
        }

        public override AnalyseResult AnalyseProcedure()
        {
            SqlSyntaxError error = null;

            StmtContext statement = this.GetStmtContext(out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && statement != null)
            {
                RoutineScript script = new RoutineScript() { Type = RoutineType.PROCEDURE };

                CreatefunctionstmtContext proc = statement.createfunctionstmt();

                if (proc != null)
                {
                    #region Name
                    Func_nameContext name = proc.func_name();

                    if (name.indirection() != null)
                    {
                        script.Schema = name.colid().GetText();
                        script.Name = new TokenInfo(this.GetIndirectionAttrName(name.indirection()));
                    }
                    else
                    {
                        script.Name = new TokenInfo(name.type_function_name());
                    }
                    #endregion

                    #region Parameters   
                    this.SetRoutineParameters(script, proc.func_args_with_defaults().func_args_with_defaults_list()?.func_arg_with_default());
                    #endregion

                    //#region Declare
                    //handle declare
                    //#endregion

                    //The ANTLR can't parse the body, parse the body independently using the "AnalyseCommon" method.   
                    #region Body
                    //handle body
                    #endregion
                }

                //extract functions                             

                result.Script = script;
            }

            return result;
        }

        public override AnalyseResult AnalyseFunction()
        {
            SqlSyntaxError error = null;

            StmtContext statement = this.GetStmtContext(out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && statement != null)
            {
                RoutineScript script = new RoutineScript() { Type = RoutineType.FUNCTION };

                CreatefunctionstmtContext func = statement.createfunctionstmt();

                if (func != null)
                {
                    #region Name
                    Func_nameContext name = func.func_name();

                    if (name.indirection() != null)
                    {
                        script.Schema = name.colid().GetText();
                        script.Name = new TokenInfo(this.GetIndirectionAttrName(name.indirection()));
                    }
                    else
                    {
                        script.Name = new TokenInfo(name.type_function_name());
                    }
                    #endregion

                    #region Parameters   
                    this.SetRoutineParameters(script, func.func_args_with_defaults()?.func_args_with_defaults_list()?.func_arg_with_default());
                    #endregion

                    //#region Declare
                    //handle declare
                    //#endregion

                    #region Body
                    //handle body
                    #endregion
                }

                //extract functions

                result.Script = script;
            }

            return result;
        }

        public override AnalyseResult AnalyseTrigger()
        {
            SqlSyntaxError error = null;

            StmtContext statement = this.GetStmtContext(out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && statement != null)
            {
                TriggerScript script = new TriggerScript();

                CreatetrigstmtContext trigger = statement.createtrigstmt();

                if (trigger != null)
                {
                    #region Name
                    NameContext name = trigger.name();

                    script.Name = new TokenInfo(name);
                    #endregion

                    TriggeractiontimeContext actionTime = trigger.triggeractiontime();

                    switch (actionTime.GetText().ToUpper())
                    {
                        case nameof(PostgreSqlParser.BEFORE):
                            script.Time = TriggerTime.BEFORE;
                            break;
                        case nameof(PostgreSqlParser.AFTER):
                            script.Time = TriggerTime.AFTER;
                            break;
                        case nameof(PostgreSqlParser.INSTEAD):
                            script.Time = TriggerTime.INSTEAD_OF;
                            break;
                    }

                    var events = trigger.triggerevents();

                    foreach (var evt in events.triggeroneevent())
                    {
                        TriggerEvent triggerEvent = (TriggerEvent)Enum.Parse(typeof(TriggerEvent), evt.GetText().ToUpper());

                        script.Events.Add(triggerEvent);
                    }

                    script.TableName = this.ParseTableName(trigger.qualified_name());

                    Func_nameContext funcName = trigger.func_name();

                    if (funcName.indirection() != null)
                    {
                        script.FunctionName = new NameToken(this.GetIndirectionAttrName(funcName.indirection()));
                        script.Schema = funcName.colid().GetText();
                    }
                    else
                    {
                        script.FunctionName = new NameToken(funcName.colid());
                    }
                }

                this.ExtractFunctions(script, statement);

                result.Script = script;
            }

            return result;
        }

        public override AnalyseResult AnalyseView()
        {
            SqlSyntaxError error = null;

            StmtContext statement = this.GetStmtContext(out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && statement != null)
            {
                ViewScript script = new ViewScript();

                ViewstmtContext view = statement.viewstmt();

                if (view != null)
                {
                    #region Name
                    Qualified_nameContext name = view.qualified_name();

                    if (name.indirection() != null)
                    {
                        script.Schema = name.colid().GetText();
                        script.Name = new TokenInfo(this.GetIndirectionAttrName(name.indirection()));
                    }
                    else
                    {
                        script.Name = new TokenInfo(name.colid());
                    }
                    #endregion

                    #region Statement

                    script.Statements.Add(this.ParseSelectStatement(view.selectstmt()));

                    #endregion
                }

                this.ExtractFunctions(script, statement);

                result.Script = script;
            }

            return result;
        }

        private RootContext GetRootContext(out SqlSyntaxError error)
        {
            error = null;

            PostgreSqlParser parser = this.GetParser() as PostgreSqlParser;

            SqlSyntaxErrorListener errorListener = new SqlSyntaxErrorListener();

            parser.AddErrorListener(errorListener);

            RootContext context = parser.root();

            error = errorListener.Error;

            return context;
        }

        private StmtContext GetStmtContext(out SqlSyntaxError error)
        {
            error = null;

            RootContext rootContext = this.GetRootContext(out error);

            return rootContext?.stmtblock()?.stmtmulti()?.stmt()?.FirstOrDefault();
        }

        private void SetRoutineParameters(RoutineScript script, Func_arg_with_defaultContext[] parameters)
        {
            if (parameters != null)
            {
                foreach (Func_arg_with_defaultContext parameter in parameters)
                {
                    Parameter parameterInfo = new Parameter();

                    Param_nameContext paraName = parameter.func_arg().param_name();

                    parameterInfo.Name = new TokenInfo(paraName) { Type = TokenType.ParameterName };

                    parameterInfo.DataType = new TokenInfo(parameter.func_arg().func_type().GetText()) { Type = TokenType.DataType };

                    bool hasDefault = false;

                    foreach (var child in parameter.children)
                    {
                        if (child is TerminalNodeImpl impl)
                        {
                            if (impl.GetText().ToUpper() == "DEFAULT")
                            {
                                hasDefault = true;
                            }
                        }
                        else if (child is A_exprContext exprContext)
                        {
                            if (hasDefault)
                            {
                                A_exprContext defaultValue = exprContext;

                                if (defaultValue != null)
                                {
                                    parameterInfo.DefaultValue = new TokenInfo(defaultValue);
                                }

                                break;
                            }
                        }
                    }

                    this.SetParameterType(parameterInfo, parameter.children);

                    script.Parameters.Add(parameterInfo);
                }
            }
        }

        private void SetParameterType(Parameter parameterInfo, IList<IParseTree> nodes)
        {
            foreach (var child in nodes)
            {
                if (child is TerminalNodeImpl terminalNode)
                {
                    if (terminalNode.Symbol.Type == PostgreSqlParser.IN_P)
                    {
                        parameterInfo.ParameterType = ParameterType.IN;
                    }
                    else if (terminalNode.Symbol.Type == PostgreSqlParser.OUT_P)
                    {
                        parameterInfo.ParameterType = ParameterType.OUT;
                    }
                    else if (terminalNode.Symbol.Type == PostgreSqlParser.INOUT)
                    {
                        parameterInfo.ParameterType = ParameterType.IN | ParameterType.OUT;
                    }
                }
            }
        }

        private SelectStatement ParseSelectStatement(SelectstmtContext node)
        {
            SelectStatement statement = new SelectStatement();

            var select = node.select_no_parens();
            var withClause = select.with_clause();
            var selectClause = select.select_clause();

            statement = this.ParseSelectClause(selectClause);

            if (withClause != null)
            {
                var cteList = withClause.cte_list().common_table_expr();

                statement.WithStatements = new List<WithStatement>();

                foreach (var cte in cteList)
                {
                    WithStatement withStatement = new WithStatement();
                    withStatement.Name = new TableName(cte.name());

                    statement.WithStatements.Add(withStatement);
                }
            }

            var limit = select?.select_limit()?.limit_clause();

            if (limit != null)
            {
                statement.LimitInfo = new SelectLimitInfo()
                {
                    StartRowIndex = new TokenInfo(limit.select_offset_value()),
                    RowCount = new TokenInfo(limit.select_limit_value())
                };
            }

            return statement;
        }

        private List<Statement> ParseStmt(StmtContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var child in node.children)
            {
                if (child is SelectstmtContext select)
                {
                    statements.Add(this.ParseSelectStatement(select));
                }
                else if (child is InsertstmtContext insert)
                {
                    statements.Add(this.ParseInsertStatement(insert));
                }
                else if (child is UpdatestmtContext update)
                {
                    statements.Add(this.ParseUpdateStatement(update));
                }
                else if (child is DeletestmtContext delete)
                {
                    statements.Add(this.ParseDeleteStatement(delete));
                }
                else if (child is CreatestmtContext create)
                {
                    var statement = this.ParseCreateStatement(create);

                    if (statement != null)
                    {
                        statements.Add(statement);
                    }
                }
                else if (child is TruncatestmtContext truncate)
                {
                    var statement = this.ParseTuncateStatement(truncate);

                    if (statement != null)
                    {
                        statements.Add(statement);
                    }
                }
                else if (child is DropstmtContext drop)
                {
                    statements.Add(this.ParseDropStatetment(drop));
                }
            }

            return statements;
        }

        private SelectStatement ParseSelectClause(Select_clauseContext node)
        {
            SelectStatement statement = new SelectStatement();

            foreach (var child in node.children)
            {
                if (child is Simple_selectContext simple)
                {
                    statement = this.ParseSimpleSelect(simple);
                }
            }

            return statement;
        }

        private SelectStatement ParseSimpleSelect(Simple_selectContext node)
        {
            SelectStatement statement = new SelectStatement();

            var columns = node.opt_target_list().target_list();
            var from = node.from_clause();
            var where = node.where_clause();
            var groupBy = node.group_clause();
            var having = node.having_clause();
            var intos = node.into_clause();

            foreach (var col in columns.children)
            {
                ColumnName colName = null;

                if (col is Target_labelContext lable)
                {
                    colName = this.ParseColumnName(lable);
                }
                else if (col is Target_starContext star)
                {
                    colName = this.ParseColumnName(star);
                }

                if (colName != null)
                {
                    statement.Columns.Add(colName);
                }
            }

            if (intos != null && intos.Length > 0)
            {
                statement.Intos = new List<TokenInfo>();

                foreach (var into in intos)
                {
                    var exprs = into.into_target()?.expr_list();

                    if (exprs != null)
                    {
                        foreach (var child in exprs.children)
                        {
                            if ((child is ParserRuleContext pr) && !string.IsNullOrEmpty(child.GetText()))
                            {
                                string text = pr.GetText();

                                bool hasWord = AnalyserHelper.HasWord(this.Content, text, 0, pr.Start.StartIndex);

                                TokenInfo token = new TokenInfo(pr) { Type = hasWord ? TokenType.VariableName : TokenType.TableName };

                                statement.Intos.Add(token);
                            }
                        }
                    }
                    else if (into.children != null)
                    {
                        foreach (var child in into.children)
                        {
                            if (child is OpttempTableNameContext tn)
                            {
                                TokenInfo token = new TokenInfo(tn) { Type = TokenType.TableName };

                                statement.Intos.Add(token);
                                break;
                            }
                        }
                    }
                }
            }

            statement.FromItems = this.ParseFromClause(from);

            if (where != null)
            {
                statement.Where = this.ParseCondition(where.a_expr());
            }

            if (groupBy != null && groupBy.group_by_list() != null)
            {
                statement.GroupBy = new List<TokenInfo>();

                var items = groupBy.group_by_list().group_by_item();

                foreach (var item in items)
                {
                    var gpb = new TokenInfo(item) { Type = TokenType.GroupBy };

                    statement.GroupBy.Add(gpb);

                    if (!AnalyserHelper.IsValidColumnName(gpb))
                    {
                        this.AddChildTableAndColumnNameToken(item, gpb);
                    }
                }

                if (having != null && having.a_expr() != null)
                {
                    statement.Having = this.ParseCondition(having.a_expr());
                }
            }

            return statement;
        }

        private InsertStatement ParseInsertStatement(InsertstmtContext node)
        {
            InsertStatement statement = new InsertStatement();

            var tableName = node.insert_target().qualified_name();
            var columns = node.insert_rest()?.insert_column_list()?.insert_column_item();
            var valueClause = node.insert_rest()?.selectstmt()?.select_no_parens()?.select_clause()?.simple_select();

            statement.TableName = this.ParseTableName(tableName);

            if (columns != null)
            {
                statement.Columns.AddRange(columns.Select(item => new ColumnName(item)));
            }

            if (valueClause != null)
            {
                var values = valueClause.values_clause();

                if (values != null)
                {
                    statement.Values.AddRange(values.expr_list().Select(item => new TokenInfo(item) { Type = TokenType.InsertValue }));
                }
                else
                {
                    statement.SelectStatements = new List<SelectStatement>() { this.ParseSimpleSelect(valueClause) };
                }
            }

            return statement;
        }

        private UpdateStatement ParseUpdateStatement(UpdatestmtContext node)
        {
            UpdateStatement statement = new UpdateStatement();

            var tableName = this.ParseTableName(node.relation_expr_opt_alias());
            var sets = node.set_clause_list()?.set_clause();
            var from = node.from_clause();
            var where = node.where_or_current_clause();

            statement.TableNames.Add(tableName);

            statement.FromItems = new List<FromItem>();

            statement.FromItems.Add(new FromItem() { TableName = tableName });

            if (sets != null)
            {
                foreach (var set in sets)
                {
                    NameValueItem nvi = new NameValueItem();

                    nvi.Name = this.ParseColumnName(set.set_target());

                    var value = set.a_expr();

                    bool isSubquery = AnalyserHelper.IsSubquery(value);

                    if (!isSubquery)
                    {
                        nvi.Value = new TokenInfo(set.a_expr()) { Type = TokenType.UpdateSetValue };
                    }
                    else
                    {
                        Select_clauseContext select = this.FindSelectClause(value);

                        nvi.ValueStatement = this.ParseSelectClause(select);
                    }

                    statement.SetItems.Add(nvi);
                }
            }

            if (from != null)
            {
                statement.FromItems.AddRange(this.ParseFromClause(from));
            }

            if (where != null)
            {
                statement.Condition = this.ParseCondition(where.a_expr());
            }

            return statement;
        }

        private DeleteStatement ParseDeleteStatement(DeletestmtContext node)
        {
            DeleteStatement statement = new DeleteStatement();

            var tableName = node.relation_expr_opt_alias();
            var usingClause = node.using_clause();
            var where = node.where_or_current_clause();

            TokenInfo condition = null;

            if (where != null)
            {
                condition = this.ParseCondition(where.a_expr());
            }

            statement.TableName = this.ParseTableName(tableName);

            bool conditionUsed = false;

            if (usingClause != null)
            {
                var froms = usingClause.from_list().table_ref();

                statement.FromItems = new List<FromItem>();

                FromItem fromItem = new FromItem() { TableName = statement.TableName };

                int count = froms.Length;

                foreach (var from in froms)
                {
                    JoinItem joinItem = new JoinItem();

                    joinItem.TableName = this.ParseTableName(from);

                    if (count == 1)
                    {
                        joinItem.Condition = condition;
                        conditionUsed = true;
                    }
                    else
                    {
                        joinItem.Condition = new TokenInfo("1=1");
                    }

                    fromItem.JoinItems.Add(joinItem);
                }

                statement.FromItems.Add(fromItem);
            }

            if (!conditionUsed)
            {
                statement.Condition = condition;
            }

            return statement;
        }

        private CreateStatement ParseCreateStatement(CreatestmtContext node)
        {
            CreateStatement statement = null;

            foreach (var child in node.children)
            {
                if (child is TerminalNodeImpl tni)
                {
                    if (tni.GetText().ToUpper() == "TABLE")
                    {
                        statement = this.ParseCreateTable(node);
                    }
                }
            }

            return statement;
        }

        private CreateTableStatement ParseCreateTable(CreatestmtContext node)
        {
            CreateTableStatement statement = new CreateTableStatement();

            TableInfo tableInfo = new TableInfo();

            var tableName = node.qualified_name().FirstOrDefault();
            var columns = node.opttableelementlist().tableelementlist().tableelement();

            tableInfo.Name = new TableName(tableName);

            Action<ConstraintInfo> checkForeginKey = (constraintInfo) =>
            {
                if (constraintInfo.ForeignKey == null)
                {
                    constraintInfo.ForeignKey = new ForeignKeyInfo();
                }
            };

            Action<ConstraintInfo, IParseTree> setConstraintInfo = (constraintInfo, node) =>
            {
                if (node is TerminalNodeImpl tni)
                {
                    ConstraintType constraintType = this.GetConstraintType(tni);

                    if (constraintType != ConstraintType.None)
                    {
                        constraintInfo.Type = constraintType;
                    }
                }
                else if (node is Qualified_nameContext qn)
                {
                    if (constraintInfo.Type == ConstraintType.ForeignKey)
                    {
                        checkForeginKey(constraintInfo);
                        constraintInfo.ForeignKey.RefTableName = new TableName(qn);
                    }
                }
                else if (node is ColumnlistContext cl)
                {
                    if (constraintInfo.Type == ConstraintType.ForeignKey)
                    {
                        checkForeginKey(constraintInfo);

                        constraintInfo.ForeignKey.ColumnNames.AddRange(cl.columnElem().Select(item => new ColumnName(item)));
                    }
                    else
                    {
                        if (constraintInfo.ColumnNames == null)
                        {
                            constraintInfo.ColumnNames = new List<ColumnName>();
                        }

                        constraintInfo.ColumnNames.AddRange(cl.columnElem().Select(item => new ColumnName(item)));
                    }
                }
                else if (node is Opt_column_listContext ocl)
                {
                    if (constraintInfo.Type == ConstraintType.ForeignKey)
                    {
                        checkForeginKey(constraintInfo);

                        constraintInfo.ForeignKey.RefColumNames.AddRange(ocl.columnlist().columnElem().Select(item => new ColumnName(item)));
                    }
                }
            };

            foreach (var column in columns)
            {
                var columnDefinition = column.columnDef();
                var tableConstraint = column.tableconstraint();

                if (columnDefinition != null)
                {
                    var columnName = columnDefinition.colid();

                    //ANTLR can't parse table check constraint correctly(it takes that as a column).
                    if (columnName.GetText().ToUpper() == "CONSTRAINT")
                    {
                        continue;
                    }

                    ColumnInfo columnInfo = new ColumnInfo();

                    var dataType = columnDefinition.typename();
                    var constraints = columnDefinition.colquallist()?.colconstraint();

                    columnInfo.Name = new ColumnName(columnName);
                    columnInfo.DataType = new TokenInfo(dataType) { Type = TokenType.DataType };

                    if (constraints != null)
                    {
                        foreach (var constraint in constraints)
                        {
                            string text = constraint.GetText().ToUpper();

                            if (text.Contains("NOT") && text.Contains("NULL"))
                            {
                                columnInfo.IsNullable = false;
                            }
                            else if (text.Contains("IDENTITY"))
                            {
                                columnInfo.IsIdentity = true;
                            }
                            else
                            {
                                ColconstraintelemContext ce = constraint.colconstraintelem();

                                if (ce != null)
                                {
                                    ConstraintInfo constraintInfo = new ConstraintInfo();
                                    bool ignore = false;

                                    var cn = constraint.name();

                                    if (cn != null)
                                    {
                                        constraintInfo.Name = new NameToken(cn);
                                    }

                                    foreach (var c in ce.children)
                                    {
                                        setConstraintInfo(constraintInfo, c);

                                        if (c is A_exprContext aexpr)
                                        {
                                            if (constraintInfo.Type == ConstraintType.Check)
                                            {
                                                constraintInfo.Definition = new TokenInfo(aexpr);
                                            }
                                        }
                                        else if (c is B_exprContext bexpr)
                                        {
                                            if (constraintInfo.Type == ConstraintType.Default)
                                            {
                                                columnInfo.DefaultValue = new TokenInfo(bexpr);

                                                ignore = true;
                                            }
                                        }
                                    }

                                    if (columnInfo.Constraints == null)
                                    {
                                        columnInfo.Constraints = new List<ConstraintInfo>();
                                    }

                                    if (!ignore)
                                    {
                                        columnInfo.Constraints.Add(constraintInfo);
                                    }
                                }
                            }
                        }
                    }

                    tableInfo.Columns.Add(columnInfo);
                }
                else if (tableConstraint != null)
                {
                    ConstraintInfo constraintInfo = new ConstraintInfo();

                    var cn = tableConstraint.name();

                    if (cn != null)
                    {
                        constraintInfo.Name = new NameToken(cn);
                    }

                    foreach (var child in tableConstraint.constraintelem().children)
                    {
                        setConstraintInfo(constraintInfo, child);
                    }

                    if (tableInfo.Constraints == null)
                    {
                        tableInfo.Constraints = new List<ConstraintInfo>();
                    }

                    tableInfo.Constraints.Add(constraintInfo);
                }
            }

            statement.TableInfo = tableInfo;

            return statement;
        }

        private TruncateStatement ParseTuncateStatement(TruncatestmtContext node)
        {
            TruncateStatement statement = null;

            if (node.opt_table() != null)
            {
                statement = new TruncateStatement();

                statement.TableName = this.ParseTableName(node.relation_expr_list().relation_expr()?.FirstOrDefault()?.qualified_name());
            }

            return statement;
        }

        private TokenInfo ParseCondition(ParserRuleContext node)
        {
            if (node != null)
            {
                if (node is A_exprContext ||
                    node is A_expr_qualContext
                    )
                {
                    TokenInfo token = this.CreateToken(node);

                    bool isIfCondition = node.Parent != null && (node.Parent is Stmt_elsifsContext);

                    token.Type = isIfCondition ? TokenType.IfCondition : TokenType.SearchCondition;

                    if (!isIfCondition)
                    {
                        this.AddChildTableAndColumnNameToken(node, token);
                    }

                    return token;
                }
            }

            return null;
        }

        private List<FromItem> ParseFromClause(From_clauseContext node)
        {
            List<FromItem> fromItems = new List<FromItem>();

            Table_refContext[] tableRefs = node.from_list()?.table_ref();

            if (tableRefs != null)
            {
                foreach (Table_refContext tableRef in tableRefs)
                {
                    FromItem fromItem = new FromItem();

                    this.ParseTableRef(fromItem, tableRef);

                    fromItems.Add(fromItem);
                }
            }

            return fromItems;
        }

        private void ParseTableRef(FromItem fromItem, Table_refContext node)
        {
            var relationExp = node.relation_expr();
            var selectWithParens = node.select_with_parens();
            var alias = node.opt_alias_clause();
            var joinEq = node.join_qual();
            var tableRefs = node.table_ref();

            if (relationExp != null)
            {
                fromItem.TableName = this.ParseTableName(node);
            }
            else if (selectWithParens != null)
            {
                fromItem.TableName = this.ParseTableName(selectWithParens);
            }

            if (alias != null)
            {
                var aliasToken = new TokenInfo(alias);
                fromItem.Alias = aliasToken;
                fromItem.TableName.Alias = aliasToken;
            }

            if (joinEq != null && joinEq.Length > 0)
            {
                var joinType = node.join_type();

                JoinItem joinItem = new JoinItem();
                joinItem.Type = this.GetJoinType(joinType.LastOrDefault());
                joinItem.Condition = this.ParseCondition(joinEq.LastOrDefault()?.a_expr());

                if (tableRefs.Length == 1)
                {
                    var childTableRefs = tableRefs[0].table_ref();

                    if (relationExp != null)
                    {
                        fromItem.TableName = this.ParseTableName(node);
                    }

                    if (childTableRefs.Length == 0)
                    {
                        joinItem.TableName = this.ParseTableName(tableRefs[0]);
                    }
                    else
                    {
                        this.ParseTableRef(fromItem, tableRefs[0]);
                    }
                }
                else if (tableRefs.Length == 2)
                {
                    var firstTableRef = tableRefs[0];
                    var lastTableRef = tableRefs[1];

                    if (firstTableRef.table_ref().Length == 0)
                    {
                        //never appear
                    }
                    else
                    {
                        this.ParseTableRef(fromItem, firstTableRef);
                    }

                    if (lastTableRef.table_ref().Length == 0)
                    {
                        joinItem.TableName = this.ParseTableName(lastTableRef);
                    }
                    else
                    {
                        this.ParseTableRef(fromItem, lastTableRef);
                    }
                }

                fromItem.JoinItems.Add(joinItem);
            }
            else
            {
                if (tableRefs.Length > 0)
                {
                    foreach (var tableRef in tableRefs)
                    {
                        this.ParseTableRef(fromItem, tableRef);
                    }
                }
            }
        }

        private JoinType GetJoinType(Join_typeContext joinType)
        {
            if (joinType != null)
            {
                string type = joinType.GetText().ToUpper();

                switch (type)
                {
                    case nameof(PostgreSqlParser.LEFT):
                        return JoinType.LEFT;
                    case nameof(PostgreSqlParser.RIGHT):
                        return JoinType.RIGHT;
                    case nameof(PostgreSqlParser.FULL):
                        return JoinType.FULL;
                    case nameof(PostgreSqlParser.CROSS):
                        return JoinType.CROSS;
                }
            }

            return JoinType.INNER;
        }

        private DropStatement ParseDropStatetment(DropstmtContext node)
        {
            DropStatement statement = new DropStatement();

            var type = node.object_type_any_name().GetText().ToUpper();

            string typeName = Enum.GetNames(typeof(DatabaseObjectType)).FirstOrDefault(item => item.ToUpper() == type);

            statement.ObjectType = (DatabaseObjectType)Enum.Parse(typeof(DatabaseObjectType), typeName);

            TokenType tokenType = (TokenType)Enum.Parse(typeof(TokenType), typeName + "Name");

            statement.ObjectName = new NameToken(node.any_name_list().any_name().FirstOrDefault()) { Type = tokenType };

            return statement;
        }

        protected override bool IsFunction(IParseTree node)
        {
            if (node is Func_exprContext || node is Func_applicationContext)
            {
                return true;
            }

            return false;
        }

        protected override TokenInfo ParseFunction(ParserRuleContext node)
        {
            TokenInfo token = base.ParseFunction(node);

            if (node is Func_exprContext exp)
            {
                var funName = exp?.func_application()?.func_name();

                if (funName != null && funName.GetText().ToUpper() == "NEXTVAL")
                {
                    var arg = exp.func_application().func_arg_list().GetText();
                    var ids = arg.Trim('\'').Split('.');

                    NameToken seqName;

                    if (ids.Length == 2)
                    {
                        seqName = new NameToken(ids[1]) { Type = TokenType.SequenceName };
                        seqName.StartIndex = exp.Start.StartIndex + ids[0].Length + 1;
                        seqName.StopIndex = seqName.StartIndex + ids[1].Length + 1;
                        seqName.Schema = ids[0];
                    }
                    else
                    {
                        seqName = new NameToken(ids[0]) { Type = TokenType.SequenceName };
                        seqName.StartIndex = exp.Start.StartIndex + 1;
                        seqName.StopIndex = seqName.StartIndex;
                    }

                    token.AddChild(seqName);
                }
            }

            return token;
        }

        protected override TableName ParseTableName(ParserRuleContext node, bool strict = false)
        {
            TableName tableName = null;

            Action<Opt_alias_clauseContext> setAlias = (alias) =>
            {
                if (tableName != null && alias != null && !string.IsNullOrEmpty(alias.GetText()))
                {
                    tableName.Alias = new TokenInfo(alias);
                }
            };

            if (node != null)
            {
                if (node is Table_refContext tableRef)
                {
                    var expr = tableRef.relation_expr();
                    var tbRefs = tableRef.table_ref();

                    if (expr != null)
                    {
                        tableName = new TableName(expr);

                        var alias = tableRef.opt_alias_clause();

                        setAlias(alias);
                    }
                    else if (tbRefs != null && tbRefs.Length > 0)
                    {
                        tableName = this.ParseTableName(tbRefs.First());
                    }
                }
                else if (node is Select_with_parensContext swp)
                {
                    tableName = new TableName(swp);

                    if (AnalyserHelper.IsSubquery(swp))
                    {
                        this.AddChildTableAndColumnNameToken(swp, tableName);
                    }
                }
                else if (node is Relation_expr_opt_aliasContext reo)
                {
                    tableName = new TableName(reo.relation_expr());

                    var alias = reo.colid();

                    if (alias != null)
                    {
                        tableName.Alias = new TokenInfo(alias);
                    }
                }
                else if (node is Qualified_nameContext qualified)
                {
                    if (qualified.indirection() != null)
                    {
                        tableName = new TableName(this.GetIndirectionAttrName(qualified.indirection()));
                        tableName.Schema = qualified.colid().GetText();
                    }
                    else
                    {
                        tableName = new TableName(qualified.colid());
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
                if (node is ColumnrefContext colRef)
                {
                    columnName = new ColumnName(colRef);
                    var indirection = colRef.indirection();

                    if (indirection != null)
                    {
                        columnName.TableName = new TableName(colRef.colid());
                    }
                }
                else if (node is Target_labelContext label)
                {
                    var exp = label.a_expr();

                    columnName = new ColumnName(exp);

                    CollabelContext alias = label.collabel();

                    if (columnName != null && alias != null)
                    {
                        columnName.HasAs = label.children.Any(t => t is TerminalNodeImpl && t.GetText().ToUpper() == "AS");
                        columnName.Alias = new TokenInfo(alias.identifier());
                    }

                    this.AddChildColumnNameToken(exp, columnName);
                }
                else if (node is Set_targetContext st)
                {
                    columnName = new ColumnName(st);

                    var cid = st.colid();
                    var indirection = st.opt_indirection();

                    if (indirection != null)
                    {
                        columnName.TableName = new TableName(cid);
                    }
                }
                else if (node is Target_starContext star)
                {
                    columnName = new ColumnName(star);
                }

                if (!strict && columnName == null)
                {
                    columnName = new ColumnName(node);
                }
            }

            return columnName;
        }

        private Attr_nameContext GetIndirectionAttrName(IndirectionContext indirection)
        {
            return indirection.indirection_el().FirstOrDefault().attr_name();
        }

        private ColumnrefContext FindColumnRefByLabel(ParserRuleContext node)
        {
            return this.FindContext(node, typeof(ColumnrefContext)) as ColumnrefContext;
        }

        private Select_clauseContext FindSelectClause(ParserRuleContext node)
        {
            return this.FindContext(node, typeof(Select_clauseContext)) as Select_clauseContext;
        }

        private ParserRuleContext FindContext(ParserRuleContext node, Type type)
        {
            if (node != null && node.children != null)
            {
                foreach (var child in node.children)
                {
                    if (child.GetType() == type)
                    {
                        return child as ParserRuleContext;
                    }
                    else if (!(child is TerminalNodeImpl))
                    {
                        return FindSelectClause(child as ParserRuleContext);
                    }
                }
            }

            return null;
        }

        protected override TokenInfo ParseTableAlias(ParserRuleContext node)
        {
            if (node != null)
            {
                if (node is Alias_clauseContext alias)
                {
                    return new TokenInfo(alias.colid()) { Type = TokenType.TableAlias };
                }
            }

            return null;
        }

        protected override TokenInfo ParseColumnAlias(ParserRuleContext node)
        {
            return null;
        }
    }
}
