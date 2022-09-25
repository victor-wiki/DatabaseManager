using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using static PostgreSqlParser;

namespace SqlAnalyser.Core
{
    public class PostgreSqlRuleAnalyser : SqlRuleAnalyser
    {
        public override Lexer GetLexer(string content)
        {
            return new PostgreSqlLexer(this.GetCharStreamFromString(content));
        }

        public override Parser GetParser(CommonTokenStream tokenStream)
        {
            return new PostgreSqlParser(tokenStream);
        }
        public override AnalyseResult AnalyseFunction(string content)
        {
            SqlSyntaxError error = null;

            StmtContext statement = this.GetStmtContext(content, out error);

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
                    this.SetRoutineParameters(script, func.func_args_with_defaults().func_args_with_defaults_list().func_arg_with_default());
                    #endregion

                    //#region Declare
                    //handle declare
                    //#endregion

                    #region Body
                    //handle body
                    #endregion
                }

                //extract functions

                ////comment above need to implement in future, the ANTLR can't parse the body script.

                result.Script = script;
            }

            return result;
        }

        public override AnalyseResult AnalyseProcedure(string content)
        {
            SqlSyntaxError error = null;

            StmtContext statement = this.GetStmtContext(content, out error);

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
                    this.SetRoutineParameters(script, proc.func_args_with_defaults().func_args_with_defaults_list().func_arg_with_default());
                    #endregion

                    //#region Declare
                    //handle declare
                    //#endregion

                    #region Body
                    //handle body
                    #endregion
                }

                //extract functions

                ////comment above need to implement in future, the ANTLR can't parse the body script.

                result.Script = script;
            }

            return result;
        }

        public override AnalyseResult AnalyseTrigger(string content)
        {
            SqlSyntaxError error = null;

            StmtContext statement = this.GetStmtContext(content, out error);

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

                    switch (actionTime.GetText())
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
                        TriggerEvent triggerEvent = (TriggerEvent)Enum.Parse(typeof(TriggerEvent), evt.GetText());

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

                result.Script = script;
            }

            return result;
        }

        public override AnalyseResult AnalyseView(string content)
        {
            SqlSyntaxError error = null;

            StmtContext statement = this.GetStmtContext(content, out error);

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

                //extract functions

                result.Script = script;
            }

            return result;
        }

        public RootContext GetRootContext(string content, out SqlSyntaxError error)
        {
            error = null;

            PostgreSqlParser parser = this.GetParser(content) as PostgreSqlParser;

            SqlSyntaxErrorListener errorListener = new SqlSyntaxErrorListener();

            parser.AddErrorListener(errorListener);

            RootContext context = parser.root();

            error = errorListener.Error;

            return context;
        }

        public StmtContext GetStmtContext(string content, out SqlSyntaxError error)
        {
            error = null;

            RootContext rootContext = this.GetRootContext(content, out error);

            return rootContext?.stmtblock()?.stmtmulti()?.stmt()?.FirstOrDefault();
        }

        public void SetRoutineParameters(RoutineScript script, Func_arg_with_defaultContext[] parameters)
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
                            if (impl.GetText().ToLower() == "default")
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

        public void SetParameterType(Parameter parameterInfo, IList<IParseTree> nodes)
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

        public SelectStatement ParseSelectStatement(SelectstmtContext node)
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
                    withStatement.Name = new TokenInfo(cte.name());

                    statement.WithStatements.Add(withStatement);
                }
            }

            return statement;
        }

        public SelectStatement ParseSelectClause(Select_clauseContext node)
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

        public SelectStatement ParseSimpleSelect(Simple_selectContext node)
        {
            SelectStatement statement = new SelectStatement();

            var columns = node.opt_target_list().target_list();
            var from = node.from_clause();
            var where = node.where_clause();
            var groupBy = node.group_clause();
            var having = node.having_clause();
            var into = node.into_clause();

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

            statement.FromItems = this.ParseFormClause(from);

            if (where != null)
            {
                statement.Where = this.ParseCondition(where.a_expr());
            }

            if (groupBy != null)
            {
                statement.GroupBy = new List<TokenInfo>();

                var items = groupBy.group_by_list().group_by_item();

                foreach (var item in items)
                {
                    statement.GroupBy.Add(new TokenInfo(item) { Type = TokenType.GroupBy });
                }

                if (having != null)
                {
                    statement.Having = this.ParseCondition(having.a_expr());
                }
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
                    return this.ParseToken(node, TokenType.Condition);
                }
            }

            return null;
        }

        public List<FromItem> ParseFormClause(From_clauseContext node)
        {
            List<FromItem> fromItems = new List<FromItem>();

            Table_refContext[] tables = node.from_list().table_ref();

            foreach (Table_refContext table in tables)
            {
                FromItem fromItem = new FromItem();

                this.ParseTableRef(fromItem, table);

                fromItem.JoinItems.Reverse();

                fromItems.Add(fromItem);
            }

            return fromItems;
        }

        private void ParseTableRef(FromItem fromItem, Table_refContext node)
        {
            var joinType = node.join_type();

            if (joinType.Length == 0)
            {
                foreach (var item in node.table_ref())
                {
                    this.ParseTableRef(fromItem, item);
                }

                return;
            }

            List<JoinItem> joinItems = new List<JoinItem>();

            var tableRefs = node.table_ref();
            var joinTable = tableRefs.Last();
            var condition = node.join_qual();

            JoinItem joinItem = new JoinItem();
            joinItem.Type = this.GetJoinType(joinType.Last());
            joinItem.TableName = this.ParseTableName(joinTable);
            joinItem.Condition = this.ParseCondition(condition.Last().a_expr());

            fromItem.TableName = joinItem.TableName;

            fromItem.JoinItems.Add(joinItem);

            if (tableRefs.First().table_ref().Length > 0)
            {
                this.ParseTableRef(fromItem, tableRefs.First());
            }
            else
            {
                fromItem.TableName = this.ParseTableName(node);
            }
        }

        private JoinType GetJoinType(Join_typeContext joinType)
        {
            string type = joinType.GetText();

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

            return JoinType.INNER;
        }
        public override bool IsFunction(IParseTree node)
        {
            throw new NotImplementedException();
        }

        public override ColumnName ParseColumnName(ParserRuleContext node, bool strict = false)
        {
            ColumnName columnName = null;

            if (node != null)
            {
                if (node is Target_labelContext label)
                {
                    columnName = new ColumnName(label);

                    CollabelContext alias = label.collabel();

                    if (columnName != null && alias != null)
                    {
                        columnName.Alias = new TokenInfo(alias.identifier());
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
            if (node != null && node.children != null)
            {
                foreach (var child in node.children)
                {
                    if (child is ColumnrefContext columnRef)
                    {
                        return columnRef;
                    }
                    else if (!(child is TerminalNodeImpl))
                    {
                        return FindColumnRefByLabel(child as ParserRuleContext);
                    }
                }
            }


            return null;
        }

        public override TableName ParseTableName(ParserRuleContext node, bool strict = false)
        {
            TableName tableName = null;

            Action<Opt_alias_clauseContext> setAlias = (alias) =>
            {
                if (tableName != null && alias != null)
                {
                    tableName.Alias = new TokenInfo(alias);
                }
            };

            if (node != null)
            {
                if (node is Table_refContext tableRef)
                {
                    var expr = tableRef.relation_expr();

                    if (expr != null)
                    {
                        tableName = new TableName(expr);

                        setAlias(tableRef.opt_alias_clause());
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
    }
}
