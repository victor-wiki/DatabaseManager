using Antlr4.Runtime.Tree;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static TSqlParser;
using Antlr4.Runtime;

namespace SqlAnalyser.Core
{
    public class TSqlRuleAnalyser : SqlRuleAnalyser
    {
        public override Lexer GetLexer(string content)
        {
            return new TSqlLexer(this.GetCharStreamFromString(content));
        }

        public override Parser GetParser(CommonTokenStream tokenStream)
        {
            return new TSqlParser(tokenStream);
        }

        public Tsql_fileContext GetRootContext(string content)
        {
            TSqlParser parser = this.GetParser(content) as TSqlParser;

            Tsql_fileContext context = parser.tsql_file();

            return context;
        }

        private Ddl_clauseContext GetDdlStatementContext(string content)
        {
            Tsql_fileContext rootContext = this.GetRootContext(content);

            return rootContext?.batch().FirstOrDefault()?.sql_clauses()?.sql_clause().Select(item => item?.ddl_clause()).FirstOrDefault();
        }

        public override RoutineScript AnalyseProcedure(string content)
        {
            Ddl_clauseContext ddlStatement = this.GetDdlStatementContext(content);

            RoutineScript script = new RoutineScript();

            if (ddlStatement != null)
            {
                Create_or_alter_procedureContext proc = ddlStatement.create_or_alter_procedure();

                if (proc != null)
                {
                    #region Name
                    this.SetScriptName(script, proc.func_proc_name_schema().id());
                    #endregion

                    #region Parameters
                    this.SetRoutineParameters(script, proc.procedure_param());
                    #endregion

                    #region Body

                    this.SetScriptBody(script, proc.sql_clauses().sql_clause());

                    #endregion
                }
            }

            this.ExtractFunctions(script, ddlStatement);

            return script;
        }

        private void SetScriptBody(CommonScript script, Sql_clauseContext[] clauses)
        {
            foreach (var clause in clauses)
            {
                script.Statements.AddRange(this.ParseSqlClause(clause));
            }
        }

        private List<Statement> ParseSqlClause(Sql_clauseContext node)
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
                else if (bc is Cfl_statementContext cfl)
                {
                    statements.AddRange(this.ParseCflStatement(cfl));
                }
            }

            return statements;
        }

        private List<Statement> ParseDmlStatement(Dml_clauseContext node)
        {
            List<Statement> statements = new List<Statement>();

            foreach (var dc in node.children)
            {
                if (dc is Select_statementContext select)
                {
                    statements.AddRange(this.ParseSelectStatement(select));
                }
                if (dc is Insert_statementContext insert)
                {
                    statements.Add(this.ParseInsertStatement(insert));
                }
                else if (dc is Update_statementContext update)
                {
                    statements.Add(this.ParseUpdateStatement(update));
                }
                else if (dc is Delete_statementContext delete)
                {
                    statements.Add(this.ParseDeleteStatement(delete));
                }
            }

            return statements;
        }

        private InsertStatement ParseInsertStatement(Insert_statementContext node)
        {
            InsertStatement statement = new InsertStatement();

            foreach (var child in node.children)
            {
                if (child is Ddl_objectContext table)
                {
                    statement.TableName = new TokenInfo(table);
                }
                else if (child is Column_name_listContext columns)
                {
                    statement.Columns = columns.id().Select(item => this.ParseColumn(item)).ToList();
                }
                else if (child is Insert_statement_valueContext values)
                {
                    var tableValues = values.table_value_constructor();
                    var derivedTable = values.derived_table();

                    if (tableValues != null)
                    {
                        statement.Values = tableValues.expression_list().SelectMany(item => item.expression().Select(t => new TokenInfo(t))).ToList();
                    }
                    else if (derivedTable != null)
                    {
                        statement.SelectStatements = new List<SelectStatement>();

                        statement.SelectStatements.AddRange(this.ParseSelectStatement(derivedTable.subquery().select_statement()));
                    }
                }
            }

            return statement;
        }

        private UpdateStatement ParseUpdateStatement(Update_statementContext node)
        {
            UpdateStatement statement = new UpdateStatement();

            statement.TableName = new TokenInfo(node.ddl_object());
            foreach (var ele in node.update_elem())
            {
                statement.Items.Add(new NameValueItem() { Name = new TokenInfo(ele.full_column_name()), Value = new TokenInfo(ele.expression()) });
            }

            statement.Condition = new TokenInfo(node.search_condition_list()) { Type = TokenType.Condition};

            return statement;
        }

        private DeleteStatement ParseDeleteStatement(Delete_statementContext node)
        {
            DeleteStatement statement = new DeleteStatement();

            statement.TableName = new TokenInfo(node.delete_statement_from()) { Type = TokenType.TableName };
            statement.Condition = new TokenInfo(node.search_condition()) { Type = TokenType.Condition };

            return statement;
        }

        private List<Statement> ParseCflStatement(Cfl_statementContext node)
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
                            statements.AddRange(clauses.sql_clause().SelectMany(item => this.ParseSqlClause(item)));
                        }
                    }
                }
                else if (child is Return_statementContext @return)
                {
                    statements.Add(this.ParseReturnStatement(@return));
                }
            }

            return statements;
        }

        private IfStatement ParseIfStatement(If_statementContext node)
        {
            IfStatement statement = new IfStatement();

            IfStatementItem item = new IfStatementItem();

            foreach (var child in node.children)
            {
                if (child is Search_conditionContext condition)
                {
                    item.Condition = new TokenInfo(condition) { Type = TokenType.Condition };
                }
                else if (child is Sql_clauseContext clause)
                {
                    item.Statements.AddRange(this.ParseSqlClause(clause));
                }
            }

            statement.Items.Add(item);

            return statement;
        }

        private WhileStatement ParseWhileStatement(While_statementContext node)
        {
            WhileStatement statement = new WhileStatement();

            foreach (var child in node.children)
            {
                if (child is Search_conditionContext condition)
                {
                    statement.Condition = new TokenInfo(condition) { Type = TokenType.Condition };
                }
                else if (child is Sql_clauseContext clause)
                {
                    statement.Statements.AddRange(this.ParseSqlClause(clause));
                }
            }

            return statement;
        }

        private List<Statement> ParseAnotherStatement(Another_statementContext node)
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
            }

            return statements;
        }

        private List<DeclareStatement> ParseDeclareStatement(Declare_statementContext node)
        {
            List<DeclareStatement> statements = new List<DeclareStatement>();

            foreach (var dc in node.children)
            {
                if (dc is Declare_localContext local)
                {
                    DeclareStatement declareStatement = new DeclareStatement();

                    declareStatement.Name = new TokenInfo(local.LOCAL_ID()) { Type = TokenType.VariableName };
                    declareStatement.DataType = new TokenInfo(local.data_type().GetText()) { Type = TokenType.DataType };

                    statements.Add(declareStatement);
                }
                else if (dc is Declare_cursorContext cursor)
                {

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
                        .Select(item => this.ParseTableColumn(item)).ToList()
                    };

                    statements.Add(declareStatement);
                }
            }

            return statements;
        }

        private void SetScriptName(CommonScript script, IdContext[] ids)
        {
            var name = ids.Last();

            script.Name = new TokenInfo(name);

            if (ids.Length > 1)
            {
                script.Owner = new TokenInfo(ids.First());
            }
        }

        private void SetParameterType(Parameter parameterInfo, IList<IParseTree> nodes)
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

        private void SetRoutineParameters(RoutineScript script, Procedure_paramContext[] parameters)
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

        public override RoutineScript AnalyseFunction(string content)
        {
            Ddl_clauseContext ddlStatement = this.GetDdlStatementContext(content);

            RoutineScript script = new RoutineScript();

            if (ddlStatement != null)
            {
                Create_or_alter_functionContext func = ddlStatement.create_or_alter_function();

                if (func != null)
                {
                    #region Name
                    this.SetScriptName(script, func.func_proc_name_schema().id());
                    #endregion

                    #region Parameters
                    this.SetRoutineParameters(script, func.procedure_param());
                    #endregion

                    this.SetFunction(script, func);
                }
            }

            this.ExtractFunctions(script, ddlStatement);

            return script;
        }

        private void SetFunction(RoutineScript script, Create_or_alter_functionContext func)
        {
            var scalar = func.func_body_returns_scalar();
            var table = func.func_body_returns_table();
            var select = func.func_body_returns_select();

            if (scalar != null)
            {
                script.ReturnDataType = new TokenInfo(scalar.data_type().GetText()) { Type = TokenType.DataType };

                this.SetScriptBody(script, scalar.sql_clause());

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
                    if(child is TerminalNodeImpl terminalNode)
                    {                      
                        if(terminalNode.Symbol.Text.StartsWith("@"))
                        {
                            script.ReturnTable.Name = new TokenInfo(terminalNode) { Type= TokenType.VariableName };
                        }                       
                    }
                    else if (child is Table_type_definitionContext type)
                    {
                        script.ReturnTable.Columns = type.column_def_table_constraints().column_def_table_constraint()
                            .Select(item => this.ParseTableColumn(item)).ToList();
                    }
                }

                script.Statements.AddRange(table.sql_clause().SelectMany(item=>this.ParseSqlClause(item)));
            }
            else if (select != null)
            {              
                script.Statements.AddRange(this.ParseSelectStatement(select.select_statement()));
            }
        }

        public override ViewScript AnalyseView(string content)
        {
            Ddl_clauseContext ddlStatement = this.GetDdlStatementContext(content);

            ViewScript script = new ViewScript();

            if (ddlStatement != null)
            {
                Create_viewContext view = ddlStatement.create_view();

                if (view != null)
                {
                    #region Name
                    this.SetScriptName(script, view.simple_name().id());
                    #endregion                  

                    #region Statement

                    foreach (var child in view.children)
                    {
                        if (child is Select_statementContext select)
                        {
                            script.Statements.AddRange(this.ParseSelectStatement(select));
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
            Ddl_clauseContext ddlStatement = this.GetDdlStatementContext(content);

            TriggerScript script = new TriggerScript();

            if (ddlStatement != null)
            {
                Create_or_alter_dml_triggerContext trigger = ddlStatement.create_or_alter_trigger().create_or_alter_dml_trigger();

                if (trigger != null)
                {
                    #region Name                 

                    this.SetScriptName(script, trigger.simple_name().id());

                    #endregion

                    script.TableName = new TokenInfo(trigger.table_name());

                    foreach (var child in trigger.children)
                    {
                        if (child is TerminalNodeImpl terminalNode)
                        {
                            switch (terminalNode.Symbol.Type)
                            {
                                case TSqlParser.BEFORE:
                                    script.Time = TriggerTime.BEFORE;
                                    break;
                                case TSqlParser.AFTER:
                                    script.Time = TriggerTime.AFTER;
                                    break;
                            }
                        }
                        else if (child is Dml_trigger_operationContext operation)
                        {
                            script.Event = (TriggerEvent)Enum.Parse(typeof(TriggerEvent), operation.GetText());
                        }
                    }

                    #region Body

                    this.SetScriptBody(script, trigger.sql_clauses().sql_clause());

                    #endregion
                }
            }

            this.ExtractFunctions(script, ddlStatement);

            return script;
        }

        private List<SelectStatement> ParseSelectStatement(Select_statementContext node)
        {
            List<SelectStatement> statements = new List<SelectStatement>();

            foreach (var child in node.children)
            {
                if (child is Query_expressionContext query)
                {
                    SelectStatement selectStatement = null;

                    foreach (var qc in query.children)
                    {
                        if (qc is Query_specificationContext specification)
                        {
                            selectStatement = this.ParseQuerySpecification(specification);
                        }
                        else if (qc is Sql_unionContext union)
                        {
                            var spec = union.query_specification();

                            if (spec != null && selectStatement != null)
                            {
                                selectStatement.UnionStatements = new List<SelectStatement>() { this.ParseQuerySpecification(spec) };
                            }
                        }
                    }

                    if (selectStatement != null)
                    {
                        statements.Add(selectStatement);
                    }
                }
            }

            return statements;
        }

        private SelectStatement ParseQuerySpecification(Query_specificationContext node)
        {
            SelectStatement statement = new SelectStatement();

            foreach (var child in node.children)
            {
                if (child is Select_listContext list)
                {
                    statement.Columns.AddRange(list.select_list_elem().Select(item => this.ParseColumn(item)));
                }
                else if (child is Table_sourcesContext table)
                {
                    statement.TableName = new TokenInfo(table) { Type = TokenType.TableName };
                }
                else if (child is Search_conditionContext condition)
                {
                    statement.Condition = new TokenInfo(condition) { Type = TokenType.Condition };
                }
            }

            return statement;
        }

        public TokenInfo ParseColumn(ParserRuleContext node)
        {
            TokenInfo tokenInfo = new TokenInfo(node) { Type = TokenType.ColumnName };

            if (node is Select_list_elemContext elem)
            {
                var expEle = elem.expression_elem();

                if (expEle != null)
                {
                    ColumnInfo column = new ColumnInfo();
                    column.Alias = new TokenInfo(expEle.as_column_alias()?.column_alias());
                    column.Expression = new TokenInfo(expEle.expression());

                    tokenInfo.Tag = column;
                }
            }

            return tokenInfo;
        }

        public ColumnInfo ParseTableColumn(ParserRuleContext node)
        {
            ColumnInfo column = new ColumnInfo();

            if (node is Column_def_table_constraintContext col)
            {
                column.Name = new TokenInfo(col.column_definition().id().First()) { Type = TokenType.ColumnName };
                column.DataType = new TokenInfo(col.column_definition().data_type()) { Type = TokenType.DataType };
            }

            return column;
        }

        public SetStatement ParseSetStatement(Set_statementContext node)
        {
            SetStatement statement = new SetStatement();

            foreach (var child in node.children)
            {
                if (child is TerminalNodeImpl terminalNode)
                {
                    int type = terminalNode.Symbol.Type;

                    if (type != TSqlParser.SET && terminalNode.GetText() != "=" && statement.Key == null)
                    {
                        statement.Key = new TokenInfo(terminalNode);
                    }
                }
                else if (child is ExpressionContext exp)
                {
                    statement.Value = new TokenInfo(exp);

                    break;
                }
            }

            return statement;
        }

        public string ParseExpression(ExpressionContext node)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var child in node.children)
            {
                if (child is Full_column_nameContext columnName)
                {
                    string text = columnName.id().GetText();

                    sb.Append(text);
                }
            }

            return sb.ToString();
        }

        public ReturnStatement ParseReturnStatement(Return_statementContext node)
        {
            ReturnStatement statement = new ReturnStatement();

            foreach (var child in node.children)
            {
                if (child is ExpressionContext predicate)
                {
                    statement.Value = new TokenInfo(predicate);
                }
            }

            return statement;
        }

        public override void ExtractFunctions(CommonScript script, ParserRuleContext node)
        {
            this.ExtractFunctions<Function_callContext>(script, node);
        }
    }
}
