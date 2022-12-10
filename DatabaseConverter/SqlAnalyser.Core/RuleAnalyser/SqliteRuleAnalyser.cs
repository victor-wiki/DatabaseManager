using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using static SqliteParser;

namespace SqlAnalyser.Core
{
    public class SqliteRuleAnalyser : SqlRuleAnalyser
    {
        public override IEnumerable<Type> ParseTableTypes => new List<Type>() { typeof(Table_nameContext), typeof(Qualified_table_nameContext) };

        public override IEnumerable<Type> ParseColumnTypes => new List<Type>() { typeof(Column_nameContext) };

        public override IEnumerable<Type> ParseTableAliasTypes => new List<Type> { typeof(Table_aliasContext) };

        public override IEnumerable<Type> ParseColumnAliasTypes => new List<Type> { typeof(Column_aliasContext) };

        public SqliteRuleAnalyser(string content) : base(content)
        {
        }

        protected override Lexer GetLexer()
        {
            return new SqliteLexer(this.GetCharStreamFromString());
        }

        protected override Parser GetParser(CommonTokenStream tokenStream)
        {
            return new SqliteParser(tokenStream);
        }

        private Sql_stmt_listContext GetRootContext(out SqlSyntaxError error)
        {
            error = null;

            SqliteParser parser = this.GetParser() as SqliteParser;

            SqlSyntaxErrorListener errorListener = this.AddParserErrorListener(parser);

            var context = parser.parse().sql_stmt_list().FirstOrDefault();

            error = errorListener.Error;

            return context;
        }

        private Sql_stmtContext GetStmtContext(out SqlSyntaxError error)
        {
            return this.GetRootContext(out error)?.sql_stmt()?.FirstOrDefault();
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

            SqliteParser parser = this.GetParser() as SqliteParser;

            SqlSyntaxErrorListener errorListener = this.AddParserErrorListener(parser);

            Sql_stmt_listContext[] stmts = parser.parse().sql_stmt_list();

            error = errorListener.Error;

            AnalyseResult result = new AnalyseResult() { Error = error };

            CommonScript script = null;

            Action checkScript = () =>
            {
                if (script == null)
                {
                    script = new CommonScript();
                }
            };

            foreach (Sql_stmt_listContext stmt in stmts)
            {
                foreach (Sql_stmtContext sql_stmt in stmt.sql_stmt())
                {
                    if (sql_stmt.children == null)
                    {
                        continue;
                    }

                    foreach (var child in sql_stmt.children)
                    {
                        if (child is Select_stmtContext select_Stmt)
                        {
                            checkScript();

                            script.Statements.Add(this.ParseSelectStmt(select_Stmt));
                        }
                        else if (child is Insert_stmtContext insert_Stmt)
                        {
                            checkScript();

                            script.Statements.Add(this.ParseInsertStmt(insert_Stmt));
                        }
                        else if (child is Update_stmtContext update_Stmt)
                        {
                            checkScript();

                            script.Statements.Add(this.ParseUpdateStmt(update_Stmt));
                        }
                        else if (child is Delete_stmtContext delete_Stmt)
                        {
                            checkScript();

                            script.Statements.Add(this.ParseDeleteStmt(delete_Stmt));
                        }
                        else if (child is Create_table_stmtContext create_Table_Stmt)
                        {
                            checkScript();

                            script.Statements.Add(this.ParseCreateTableStmt(create_Table_Stmt));
                        }
                        else if (child is Drop_stmtContext drop)
                        {
                            checkScript();

                            script.Statements.Add(this.ParseDropStmt(drop));
                        }
                        else if (child is Create_view_stmtContext view)
                        {
                            script = new ViewScript();

                            this.SetViewScript(script as ViewScript, view);
                        }
                        else if (child is Create_trigger_stmtContext trigger)
                        {
                            script = new TriggerScript();

                            this.SetTriggerScript(script as TriggerScript, trigger);
                        }
                    }
                }

                this.ExtractFunctions(script, stmt);
            }


            result.Script = script;

            return result;
        }

        public override AnalyseResult AnalyseView()
        {
            SqlSyntaxError error = null;

            Sql_stmtContext stmt = this.GetStmtContext(out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && stmt != null)
            {
                ViewScript script = new ViewScript();

                Create_view_stmtContext view = stmt.create_view_stmt();

                if (view != null)
                {
                    this.SetViewScript(script, view);
                }

                this.ExtractFunctions(script, stmt);

                result.Script = script;
            }

            return result;
        }

        private void SetViewScript(ViewScript script, Create_view_stmtContext view)
        {
            #region Name
            script.Name = new TokenInfo(view.view_name()) { Type = TokenType.ViewName };
            #endregion

            #region Statement

            foreach (var child in view.children)
            {
                if (child is Select_stmtContext select)
                {
                    script.Statements.Add(this.ParseSelectStmt(select));
                }
            }

            #endregion
        }

        public override AnalyseResult AnalyseTrigger()
        {
            SqlSyntaxError error = null;

            Sql_stmtContext stmt = this.GetStmtContext(out error);

            AnalyseResult result = new AnalyseResult() { Error = error };

            if (!result.HasError && stmt != null)
            {
                TriggerScript script = new TriggerScript();

                Create_trigger_stmtContext trigger = stmt.create_trigger_stmt();

                if (trigger != null)
                {
                    this.SetTriggerScript(script, trigger);
                }

                this.ExtractFunctions(script, stmt);

                result.Script = script;
            }

            return result;
        }

        private void SetTriggerScript(TriggerScript script, Create_trigger_stmtContext trigger)
        {
            #region Name                 

            script.Name = new TokenInfo(trigger.trigger_name()) { Type = TokenType.TriggerName };

            #endregion

            script.TableName = new TableName(trigger.table_name());
            script.ColumnNames = new List<ColumnName>();

            script.ColumnNames.AddRange(trigger.column_name().Select(item => new ColumnName(item)));

            foreach (var child in trigger.children)
            {
                if (child is TerminalNodeImpl tni)
                {
                    switch (tni.GetText().ToUpper())
                    {
                        case "BEDORE":
                            script.Time = TriggerTime.BEFORE;
                            break;
                        case "AFTER":
                            script.Time = TriggerTime.AFTER;
                            break;
                        case "INSERT":
                            script.Events.Add(TriggerEvent.INSERT);
                            break;
                        case "UPDATE":
                            script.Events.Add(TriggerEvent.UPDATE);
                            break;
                        case "DELETE":
                            script.Events.Add(TriggerEvent.DELETE);
                            break;
                    }
                }
                else if (child is Insert_stmtContext insert)
                {
                    script.Statements.Add(this.ParseInsertStmt(insert));
                }
                else if (child is Update_stmtContext update)
                {
                    script.Statements.Add(this.ParseUpdateStmt(update));
                }
                else if (child is Delete_stmtContext delete)
                {
                    script.Statements.Add(this.ParseDeleteStmt(delete));
                }
                else if (child is Select_stmtContext select)
                {
                    script.Statements.Add(this.ParseSelectStmt(select));
                }
            }
        }

        public override AnalyseResult AnalyseFunction()
        {
            throw new NotSupportedException();
        }

        public override AnalyseResult AnalyseProcedure()
        {
            throw new NotSupportedException();
        }

        private SelectStatement ParseSelectStmt(Select_stmtContext node)
        {
            SelectStatement statement = new SelectStatement();

            var withNode = node.common_table_stmt();

            WithStatement with = null;

            if (withNode != null)
            {
                if (withNode.GetText().ToUpper().StartsWith("WITH"))
                {
                    with = new WithStatement();

                    foreach (var child in withNode.children)
                    {
                        if (child is TerminalNodeImpl tni)
                        {
                            if (tni.GetText().ToUpper() != "WITH")
                            {
                                with.CTE = new TokenInfo(tni);
                            }
                        }
                        else if (child is Common_table_expressionContext cte)
                        {
                            var tableName = cte.table_name();
                            var columns = cte.column_name();
                            var select = cte.select_stmt();

                            if (tableName != null)
                            {
                                with.Name = new TableName(tableName);
                            }

                            if (columns != null)
                            {
                                with.Columns = columns.Select(item => new ColumnName(item)).ToList();
                            }

                            if (select != null)
                            {
                                with.SelectStatements = new List<SelectStatement>();

                                with.SelectStatements.Add(this.ParseSelectStmt(select));
                            }
                        }
                    }
                }
            }

            bool isUnion = false;
            bool isUnionAll = false;

            foreach (var child in node.children)
            {
                if (child is Select_coreContext core)
                {
                    if (!isUnion)
                    {
                        statement = this.ParseSelectCore(core);
                    }
                    else
                    {
                        if (statement.UnionStatements == null)
                        {
                            statement.UnionStatements = new List<UnionStatement>();
                        }

                        UnionStatement union = new UnionStatement() { Type = isUnionAll ? UnionType.UNION_ALL : UnionType.UNION };

                        union.SelectStatement = this.ParseSelectCore(core);

                        statement.UnionStatements.Add(union);
                    }
                }
                else if (child is Compound_operatorContext co)
                {
                    string text = co.GetText().ToUpper();

                    if (text.Contains("UNION"))
                    {
                        isUnion = true;
                    }

                    if (text.Contains("ALL"))
                    {
                        isUnionAll = true;
                    }
                }
            }

            if (with != null)
            {
                statement.WithStatements = new List<WithStatement>() { with };
            }

            var orderBy = node.order_by_stmt();
            var limit = node.limit_stmt();

            if (orderBy != null)
            {
                statement.OrderBy = new List<TokenInfo>();

                statement.OrderBy.AddRange(orderBy.ordering_term().Select(item => this.CreateToken(item.expr(), TokenType.OrderBy)));
            }

            if (limit != null)
            {
                statement.LimitInfo = new SelectLimitInfo();

                bool isOffset = false;

                foreach (var child in limit.children)
                {
                    if (child is TerminalNodeImpl tni)
                    {
                        if (tni.GetText().ToUpper() == "OFFSET")
                        {
                            isOffset = true;
                        }
                    }
                    else if (child is ParserRuleContext pr)
                    {
                        if (!isOffset)
                        {
                            statement.LimitInfo.RowCount = new TokenInfo(pr);
                        }
                        else
                        {
                            statement.LimitInfo.StartRowIndex = new TokenInfo(pr);
                        }
                    }
                }
            }

            return statement;
        }

        private SelectStatement ParseSelectCore(Select_coreContext node)
        {
            SelectStatement statement = new SelectStatement();

            var columns = node.result_column();
            var tableNames = node.table_or_subquery();
            var join = node.join_clause();

            if (columns != null && columns.Length > 0)
            {
                statement.Columns.AddRange(columns.Select(item => this.ParseColumnName(item)));
            }

            statement.FromItems = new List<FromItem>();

            if (tableNames != null && tableNames.Length > 0)
            {
                foreach (var tableName in tableNames)
                {
                    FromItem fromItem = new FromItem();
                    fromItem.TableName = this.ParseTableName(tableName);

                    statement.FromItems.Add(fromItem);
                }
            }
            else if (join != null)
            {
                statement.FromItems.Add(this.ParseJoin(join));
            }

            bool isWhere = false;
            bool isGroupBy = false;
            bool isHaving = false;

            foreach (var child in node.children)
            {
                if (child is TerminalNodeImpl tni)
                {
                    string text = tni.GetText().ToUpper();

                    switch (text)
                    {
                        case "WHERE":
                            isWhere = true;
                            break;
                        case "GROUP":
                            isGroupBy = true;
                            break;
                        case "HAVING":
                            isHaving = true;
                            break;
                    }
                }
                else if (child is ExprContext expr)
                {
                    if (isWhere)
                    {
                        statement.Where = this.ParseCondition(expr);
                    }
                    else if (isGroupBy)
                    {
                        statement.GroupBy.Add(this.CreateToken(expr, TokenType.GroupBy));
                    }
                    else if (isHaving)
                    {
                        statement.Having = this.ParseCondition(expr);
                    }
                }
            }

            if (statement.TableName == null && !statement.HasFromItems && statement.Columns.Count == 0)
            {
                statement.Columns.Add(new ColumnName(node));
            }

            return statement;
        }

        private FromItem ParseJoin(Join_clauseContext node)
        {
            FromItem item = new FromItem();

            int i = 0;

            foreach (var child in node.children)
            {
                if (child is Table_or_subqueryContext table)
                {
                    if (i == 0)
                    {
                        item.TableName = this.ParseTableName(table);
                    }
                    else
                    {
                        item.JoinItems.Last().TableName = this.ParseTableName(table);
                    }
                }
                else if (child is Join_operatorContext opt)
                {
                    JoinItem joinItem = new JoinItem();

                    string text = opt.GetText().ToUpper();

                    if (text.Contains("LEFT"))
                    {
                        joinItem.Type = JoinType.LEFT;
                    }
                    else if (text.Contains("CROSS"))
                    {
                        joinItem.Type = JoinType.CROSS;
                    }
                    else
                    {
                        joinItem.Type = JoinType.INNER;
                    }

                    item.JoinItems.Add(joinItem);
                }
                else if (child is Join_constraintContext jc)
                {
                    item.JoinItems.Last().Condition = this.ParseCondition(jc.expr());
                }

                i++;
            }

            return item;
        }

        private InsertStatement ParseInsertStmt(Insert_stmtContext node)
        {
            InsertStatement statement = new InsertStatement();

            var tableName = node.table_name();
            var columns = node.column_name();
            var values = node.expr();
            var select = node.select_stmt();

            statement.TableName = this.ParseTableName(tableName);

            if (columns != null && columns.Length > 0)
            {
                statement.Columns.AddRange(columns.Select(item => this.ParseColumnName(item)));
            }

            if (values != null && values.Length > 0)
            {
                statement.Values.AddRange(values.Select(item => new TokenInfo(item) { Type = TokenType.InsertValue }));
            }
            else if (select != null)
            {
                statement.SelectStatements.Add(this.ParseSelectStmt(select));
            }

            return statement;
        }

        private UpdateStatement ParseUpdateStmt(Update_stmtContext node)
        {
            UpdateStatement statement = new UpdateStatement();

            var tableName = node.qualified_table_name();
            var froms = node.table_or_subquery();
            var joinClause = node.join_clause();

            if (tableName != null)
            {
                statement.TableNames.Add(this.ParseTableName(tableName));
            }

            var columns = node.column_name();
            var exprs = node.expr();

            for (int i = 0; i < columns.Length; i++)
            {
                NameValueItem item = new NameValueItem();

                item.Name = this.ParseColumnName(columns[i]);

                var expr = exprs[i];

                var select = expr.select_stmt();

                if (select == null)
                {
                    item.Value = new TokenInfo(expr);
                }
                else
                {
                    item.ValueStatement = this.ParseSelectStmt(select);
                }

                statement.SetItems.Add(item);
            }

            if (froms != null)
            {
                statement.FromItems = new List<FromItem>();

                foreach (var from in froms)
                {
                    FromItem fromItem = new FromItem();

                    var select = from.select_stmt();

                    if (select == null)
                    {
                        fromItem.TableName = this.ParseTableName(from);
                    }
                    else
                    {
                        var alias = from.table_alias();

                        fromItem.SubSelectStatement = this.ParseSelectStmt(select);

                        if (alias != null)
                        {
                            fromItem.Alias = new TokenInfo(alias);
                        }
                    }

                    statement.FromItems.Add(fromItem);
                }
            }

            if (joinClause != null)
            {
                FromItem fromItem = this.ParseJoin(joinClause);

                statement.FromItems.Add(fromItem);
            }

            if (exprs.Length > columns.Length)
            {
                statement.Condition = this.ParseCondition(exprs.Last());
            }

            return statement;
        }

        private DeleteStatement ParseDeleteStmt(Delete_stmtContext node)
        {
            DeleteStatement statement = new DeleteStatement();

            var tableName = node.qualified_table_name();
            var condition = node.expr();

            statement.TableName = this.ParseTableName(tableName);

            if (condition != null)
            {
                statement.Condition = this.ParseCondition(condition);
            }

            return statement;
        }

        private CreateTableStatement ParseCreateTableStmt(Create_table_stmtContext node)
        {
            CreateTableStatement statement = new CreateTableStatement();

            TableInfo tableInfo = new TableInfo();

            var taleName = node.table_name();
            var columns = node.column_def();
            var constraints = node.table_constraint();
            var select = node.select_stmt();

            tableInfo.Name = new TableName(taleName);

            foreach (var child in node.children)
            {
                if (child is TerminalNodeImpl tni)
                {
                    string text = tni.GetText().ToUpper();

                    if (text == "TEMP" || text == "TEMPORARY")
                    {
                        tableInfo.IsTemporary = true;
                        break;
                    }
                }
            }

            foreach (var column in columns)
            {
                ColumnInfo columnInfo = new ColumnInfo();

                columnInfo.Name = new ColumnName(column.column_name());

                var typeNames = column.type_name().name();

                if (typeNames.Length == 1)
                {
                    columnInfo.DataType = new TokenInfo(typeNames[0]) { Type = TokenType.DataType };
                }
                else if (typeNames.Length == 2)
                {
                    columnInfo.DataType = new TokenInfo(typeNames[0]) { Type = TokenType.DataType };

                    if (typeNames[1].GetText().ToUpper().Trim('[', ']') == "AUTO_INCREMENT")
                    {
                        columnInfo.IsIdentity = true;
                    }
                }

                var columnConstraints = column.column_constraint();

                if (columnConstraints != null)
                {
                    columnInfo.Constraints = new List<ConstraintInfo>();

                    foreach (var constraint in columnConstraints)
                    {
                        var constraintName = constraint.name();

                        ConstraintInfo constraintInfo = new ConstraintInfo();

                        if (constraintName != null)
                        {
                            constraintInfo.Name = new NameToken(constraintName);
                        }

                        bool hasNot = false;
                        bool ignore = false;

                        foreach (var child in constraint.children)
                        {
                            if (child is TerminalNodeImpl tni)
                            {
                                string text = tni.GetText().ToUpper();

                                if (text == "NOT")
                                {
                                    hasNot = true;
                                }
                                else if (text == "NULL")
                                {
                                    if (hasNot)
                                    {
                                        columnInfo.IsNullable = false;
                                    }
                                }
                                else
                                {
                                    ConstraintType constraintType = this.GetConstraintType(tni);

                                    if (constraintType != ConstraintType.None)
                                    {
                                        constraintInfo.Type = constraintType;
                                    }
                                }
                            }
                            else if (child is ExprContext expr)
                            {
                                if (constraintInfo.Type == ConstraintType.Check)
                                {
                                    constraintInfo.Definition = new TokenInfo(expr);
                                }
                                else if (constraintInfo.Type == ConstraintType.Default)
                                {
                                    columnInfo.DefaultValue = new TokenInfo(expr);
                                    ignore = true;
                                }
                            }
                            else if (child is Foreign_key_clauseContext fk)
                            {
                                constraintInfo.ForeignKey = this.ParseForeignKey(fk);
                            }
                        }

                        if (!ignore && constraintInfo.Type != ConstraintType.None)
                        {
                            columnInfo.Constraints.Add(constraintInfo);

                            ignore = false;
                        }
                    }
                }

                tableInfo.Columns.Add(columnInfo);
            }

            if (constraints != null)
            {
                tableInfo.Constraints = new List<ConstraintInfo>();

                foreach (var constraint in constraints)
                {
                    var name = constraint.name();

                    ConstraintInfo constraintInfo = new ConstraintInfo();

                    if (name != null)
                    {
                        constraintInfo.Name = new NameToken(name);
                    }

                    foreach (var child in constraint.children)
                    {
                        if (child is TerminalNodeImpl tni)
                        {
                            ConstraintType constraintType = this.GetConstraintType(tni);

                            if (constraintType != ConstraintType.None)
                            {
                                constraintInfo.Type = constraintType;
                            }
                        }
                        else if (child is Indexed_columnContext ic)
                        {
                            if (constraintInfo.ColumnNames == null)
                            {
                                constraintInfo.ColumnNames = new List<ColumnName>();
                            }

                            constraintInfo.ColumnNames.Add(new ColumnName(ic.column_name()));
                        }
                        else if (child is Column_nameContext cn)
                        {
                            if (constraintInfo.Type == ConstraintType.ForeignKey)
                            {
                                if (constraintInfo.ForeignKey == null)
                                {
                                    constraintInfo.ForeignKey = new ForeignKeyInfo();
                                }

                                constraintInfo.ForeignKey.ColumnNames.Add(new ColumnName(cn));
                            }
                        }
                        else if (child is ExprContext expr)
                        {
                            if (constraintInfo.Type == ConstraintType.Check)
                            {
                                constraintInfo.Definition = new TokenInfo(expr);
                            }                           
                        }
                        else if (child is Foreign_key_clauseContext fk)
                        {
                            if (constraintInfo.ForeignKey != null)
                            {
                                ForeignKeyInfo fki = this.ParseForeignKey(fk);

                                constraintInfo.ForeignKey.RefTableName = fki.RefTableName;
                                constraintInfo.ForeignKey.RefColumNames = fki.RefColumNames;
                            }
                        }
                    }

                    tableInfo.Constraints.Add(constraintInfo);
                }
            }

            if (select != null)
            {
                tableInfo.SelectStatement = this.ParseSelectStmt(select);
            }

            statement.TableInfo = tableInfo;

            return statement;
        }

        private ForeignKeyInfo ParseForeignKey(Foreign_key_clauseContext node)
        {
            ForeignKeyInfo fki = new ForeignKeyInfo();

            var refTableName = node.foreign_table();
            var refColumnNames = node.column_name();

            fki.RefTableName = new TableName(refTableName);
            fki.RefColumNames.AddRange(refColumnNames.Select(item => new ColumnName(item)));

            bool isUpdate = false;
            bool isDelete = false;
            bool isCascade = false;

            foreach (var chilid in node.children)
            {
                if (chilid is TerminalNodeImpl fktni)
                {
                    string text = fktni.GetText().ToUpper();

                    if (text == "UPDATE")
                    {
                        isUpdate = true;
                    }
                    else if (text == "DELETE")
                    {
                        isDelete = true;
                    }
                    else if (text == "CASCADE")
                    {
                        isCascade = true;
                    }
                }
            }

            if (isCascade)
            {
                fki.UpdateCascade = isUpdate;
                fki.DeleteCascade = isDelete;
            }

            return fki;
        }

        private DropStatement ParseDropStmt(Drop_stmtContext node)
        {
            DropStatement statement = new DropStatement();

            var name = node.any_name();

            statement.ObjectName = new NameToken(name);

            DatabaseObjectType databaseObjectType = DatabaseObjectType.None;

            foreach (var child in node.children)
            {
                if (child is TerminalNodeImpl tni)
                {
                    string text = tni.GetText();

                    switch (text)
                    {
                        case nameof(DatabaseObjectType.Table):
                            databaseObjectType = DatabaseObjectType.Table;
                            break;
                        case nameof(DatabaseObjectType.View):
                            databaseObjectType = DatabaseObjectType.View;
                            break;
                        case nameof(DatabaseObjectType.Trigger):
                            databaseObjectType = DatabaseObjectType.Trigger;
                            break;
                    }
                }
            }

            if (databaseObjectType != DatabaseObjectType.None)
            {
                statement.ObjectType = databaseObjectType;
            }

            return statement;
        }

        private TokenInfo ParseCondition(ParserRuleContext node)
        {
            if (node != null)
            {
                if (node is ExprContext expr)
                {
                    TokenInfo token = this.CreateToken(node, TokenType.SearchCondition);

                    this.AddChildTableAndColumnNameToken(expr, token);

                    return token;
                }
            }

            return null;
        }

        protected override bool IsFunction(IParseTree node)
        {
            if (node is ExprContext expr)
            {
                if (expr.function_name() != null)
                {
                    return true;
                }
            }

            return false;
        }

        protected override TableName ParseTableName(ParserRuleContext node, bool strict = false)
        {
            TableName tableName = null;

            if (node != null)
            {
                if (node is Table_nameContext tn)
                {
                    tableName = new TableName(tn);
                }
                else if (node is Qualified_table_nameContext qt)
                {
                    tableName = new TableName(qt);
                }
                else if (node is Table_or_subqueryContext tos)
                {
                    var name = tos.table_name();
                    var select = tos.select_stmt();
                    var alias = tos.table_alias();

                    if (name != null)
                    {
                        tableName = new TableName(name);
                    }
                    else if (select != null)
                    {
                        tableName = new TableName(select);

                        this.AddChildTableAndColumnNameToken(select, tableName);
                    }

                    if (alias != null)
                    {
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
                if (node is Result_columnContext rc)
                {
                    var expr = rc.expr();
                    var alias = rc.column_alias();

                    if (expr != null)
                    {
                        var name = expr.column_name();

                        if (name != null)
                        {
                            columnName = new ColumnName(name);
                        }
                        else
                        {
                            columnName = new ColumnName(expr);
                        }
                    }

                    if (columnName != null && alias != null)
                    {
                        columnName.Alias = new TokenInfo(alias);
                    }
                }

                if (!strict && columnName == null)
                {
                    columnName = new ColumnName(node);
                }
            }

            return columnName;
        }

        protected override TokenInfo ParseTableAlias(ParserRuleContext node)
        {
            if (node != null)
            {
                if (node is Table_aliasContext alias)
                {
                    return new TokenInfo(alias) { Type = TokenType.TableAlias };
                }
            }

            return null;
        }

        protected override TokenInfo ParseColumnAlias(ParserRuleContext node)
        {
            if (node != null)
            {
                if (node is Column_aliasContext alias)
                {
                    return new TokenInfo(alias) { Type = TokenType.ColumnAlias };
                }
            }

            return null;
        }
    }
}
