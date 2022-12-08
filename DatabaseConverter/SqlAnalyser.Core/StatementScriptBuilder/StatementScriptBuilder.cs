using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class StatementScriptBuilder:IDisposable
    {
        private StringBuilder builder = new StringBuilder();
        internal int Level = 0;
        internal string Indent => " ".PadLeft((this.Level + 1) * 2);
        internal int LoopCount = 0;
        public StringBuilder Script => this.builder;

        public RoutineType RoutineType { get; set; }

        public List<DeclareVariableStatement> DeclareVariableStatements { get; internal set; } = new List<DeclareVariableStatement>();
        public List<DeclareCursorStatement> DeclareCursorStatements { get; internal set; } = new List<DeclareCursorStatement>();
        public List<Statement> OtherDeclareStatements { get; internal set; } = new List<Statement>();

        public List<Statement> SpecialStatements { get; internal set; } = new List<Statement>();
        public Dictionary<string, string> Replacements { get; internal set; } = new Dictionary<string, string>();
        public List<string> TemporaryTableNames { get; set; } = new List<string>();

        public StatementScriptBuilderOption Option { get; set; } = new StatementScriptBuilderOption();

        internal int Length
        {
            get
            {
                return this.builder.Length;
            }
        }

        public List<Statement> GetDeclareStatements()
        {
            List<Statement> statements = new List<Statement>();

            statements.AddRange(this.DeclareVariableStatements);
            statements.AddRange(this.DeclareCursorStatements);
            statements.AddRange(this.OtherDeclareStatements);

            return statements;
        }

        protected void Append(string value, bool appendIndent = true)
        {
            builder.Append($"{(appendIndent ? this.Indent : "")}{value}");
        }

        protected void AppendLine(string value = "", bool appendIndent = true)
        {
            this.Append(value, appendIndent);
            this.builder.Append(Environment.NewLine);
        }

        protected virtual void PreHandleStatements(List<Statement> statements) { }

        protected void AppendChildStatements(IEnumerable<Statement> statements, bool needSeparator = true)
        {
            List<Statement> statementList = new List<Statement>();

            statementList.AddRange(statements);

            this.PreHandleStatements(statementList);

            int childCount = statementList.Count();

            if (childCount > 0)
            {
                this.IncreaseLevel();
            }

            foreach (Statement statement in statementList)
            {
                this.Build(statement, needSeparator);
            }

            if (childCount > 0)
            {
                this.DecreaseLevel();
            }
        }

        public virtual StatementScriptBuilder Build(Statement statement, bool appendSeparator = true)
        {
            return this;
        }

        internal void IncreaseLevel()
        {
            this.Level++;
        }

        internal void DecreaseLevel()
        {
            this.Level--;
        }

        public void TrimEnd(params char[] characters)
        {
            if (characters != null && characters.Length > 0)
            {
                while (this.builder.Length > 0 && characters.Contains(this.builder[this.builder.Length - 1]))
                {
                    this.builder.Remove(this.builder.Length - 1, 1);
                }
            }
        }

        public void TrimSeparator()
        {
            this.TrimEnd(';', '\r', '\n', ' ');
        }

        public override string ToString()
        {
            return this.builder.ToString();
        }

        public void Clear()
        {
            this.builder.Clear();
        }

        protected virtual void BuildSelectStatement(SelectStatement select, bool appendSeparator = true) { }

        protected void BuildSelectStatementFromItems(SelectStatement selectStatement)
        {
            this.BuildFromItems(selectStatement.FromItems, selectStatement);
        }

        protected void BuildFromItems(List<FromItem> fromItems, SelectStatement selectStatement = null, bool isDeleteFromItem = false)
        {
            int count = fromItems.Count;
            int i = 0;

            bool hasJoins = false;

            foreach (FromItem fromItem in fromItems)
            {
                NameToken fromTableName = fromItem.TableName;

                if (fromTableName == null && selectStatement?.TableName != null)
                {
                    fromTableName = selectStatement.TableName;
                }

                TokenInfo alias = fromItem.Alias;

                bool isInvalidTableName = this.IsInvalidTableName(fromTableName?.Symbol);

                if (i == 0)
                {
                    if (!isInvalidTableName)
                    {
                        this.Append("FROM ");
                    }
                }

                hasJoins = fromItem.HasJoinItems;

                if (i > 0 && !hasJoins)
                {
                    this.Append(",", false);
                }

                string nameWithAlias = this.GetNameWithAlias(fromTableName);

                if (!isInvalidTableName)
                {
                    if (nameWithAlias?.Trim() != alias?.Symbol?.Trim())
                    {
                        this.Append($"{nameWithAlias}{(hasJoins ? Environment.NewLine : "")}", false);
                    }
                }

                bool hasSubSelect = false;

                if (fromItem.SubSelectStatement != null)
                {
                    hasSubSelect = true;

                    this.AppendLine("(");
                    this.BuildSelectStatement(fromItem.SubSelectStatement, false);
                    this.Append(")");

                    if (alias != null)
                    {
                        this.Append($"{alias}", false);
                    }
                }

                if (fromItem.JoinItems.Count > 0)
                {
                    if (hasSubSelect)
                    {
                        this.AppendLine("");
                    }

                    int j = 0;

                    foreach (JoinItem joinItem in fromItem.JoinItems)
                    {
                        if (joinItem.Type == JoinType.PIVOT || joinItem.Type == JoinType.UNPIVOT)
                        {
                            if (joinItem.PivotItem != null)
                            {
                                this.BuildPivotItem(joinItem.PivotItem);
                            }
                            else if (joinItem.UnPivotItem != null)
                            {
                                this.BuildUnPivotItem(joinItem.UnPivotItem);
                            }

                            if (joinItem.Alias != null)
                            {
                                this.AppendLine(joinItem.Alias.Symbol);
                            }
                            else
                            {
                                this.AppendLine(joinItem.Type.ToString() + "_");
                            }
                        }
                        else
                        {
                            bool isPostgresDeleteFromJoin = isDeleteFromItem && this is PostgreSqlStatementScriptBuilder;

                            string condition = joinItem.Condition == null ? "" : $" {(isPostgresDeleteFromJoin && j == 0 ? "WHERE" : "ON")} {joinItem.Condition}";

                            string joinKeyword = (isPostgresDeleteFromJoin && j == 0) ? "USING " : $"{joinItem.Type} JOIN ";

                            TableName tableName = joinItem.TableName;

                            string joinTableName = null;

                            bool isSubquery = AnalyserHelper.IsSubquery(tableName.Symbol);

                            if (!isSubquery)
                            {
                                joinTableName = this.GetNameWithAlias(tableName);
                            }
                            else
                            {
                                joinTableName = $"{StringHelper.GetParenthesisedString(tableName.Symbol)}{(tableName.Alias == null ? "" : $" {tableName.Alias}")}";
                            }

                            this.AppendLine($"{joinKeyword}{joinTableName}{condition}");
                        }

                        j++;
                    }
                }

                i++;
            }

            if (!hasJoins)
            {
                this.AppendLine("", false);
            }
        }

        protected void BuildIfCondition(IfStatementItem item)
        {
            if (item.Condition != null)
            {
                this.Append(item.Condition.Symbol);
            }
            else if (item.CondtionStatement != null)
            {
                if (item.ConditionType == IfConditionType.NotExists)
                {
                    this.Append("NOT EXISTS");
                }
                else if (item.ConditionType == IfConditionType.Exists)
                {
                    this.Append("EXISTS");
                }

                this.AppendSubquery(item.CondtionStatement);
            }
        }

        protected void BuildUpdateSetValue(NameValueItem item)
        {
            if (item.Value != null)
            {
                this.Append(item.Value.Symbol);
            }
            else if (item.ValueStatement != null)
            {
                this.AppendSubquery(item.ValueStatement);
            }
        }

        public void AppendSubquery(SelectStatement statement)
        {
            this.Append("(");
            this.BuildSelectStatement(statement, false);
            this.Append(")");
        }

        protected virtual string GetNameWithAlias(NameToken name)
        {
            return name?.NameWithAlias;
        }

        public string GetTrimedQuotationValue(string value)
        {
            return value?.Trim('[', ']', '"', '`');
        }

        protected bool IsInvalidTableName(string tableName)
        {
            if (tableName == null)
            {
                return false;
            }

            tableName = this.GetTrimedQuotationValue(tableName);

            if (tableName.ToUpper() == "DUAL" && !(this is PlSqlStatementScriptBuilder))
            {
                return true;
            }

            return false;
        }

        protected virtual string GetPivotInItem(TokenInfo token)
        {
            return this.GetTrimedQuotationValue(token.Symbol);
        }

        public void BuildPivotItem(PivotItem pivotItem)
        {
            this.AppendLine("PIVOT");
            this.AppendLine("(");
            this.AppendLine($"{pivotItem.AggregationFunctionName}({pivotItem.AggregatedColumnName})");
            this.AppendLine($"FOR {pivotItem.ColumnName} IN ({(string.Join(",", pivotItem.Values.Select(item => this.GetPivotInItem(item))))})");
            this.AppendLine(")");
        }

        public void BuildUnPivotItem(UnPivotItem unpivotItem)
        {
            this.AppendLine("UNPIVOT");
            this.AppendLine("(");
            this.AppendLine($"{unpivotItem.ValueColumnName}");
            this.AppendLine($"FOR {unpivotItem.ForColumnName} IN ({(string.Join(",", unpivotItem.InColumnNames.Select(item => $"{item}")))})");
            this.AppendLine(")");
        }     

        protected bool HasAssignVariableColumn(SelectStatement statement)
        {
            var columns = statement.Columns;

            if (columns.Any(item => AnalyserHelper.IsAssignNameColumn(item)))
            {
                return true;
            }

            return false;
        }

        protected string GetNextLoopLabel(string prefix, bool withColon = true)
        {
            return $"{prefix}{++this.LoopCount}{(withColon ? ":" : "")}";
        }

        protected string GetCurrentLoopLabel(string prefix)
        {
            return $"{prefix}{this.LoopCount}";
        }

        protected string GetConstriants(List<ConstraintInfo> constaints, bool isForColumn = false)
        {
            if (constaints == null || constaints.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            int i = 0;

            foreach (ConstraintInfo constraint in constaints)
            {
                string name = string.IsNullOrEmpty(constraint.Name?.Symbol) ? "" : $" {constraint.Name.Symbol}";

                ConstraintType constraintType = constraint.Type;

                string definition = "";

                switch (constraintType)
                {
                    case ConstraintType.PrimaryKey:
                        definition = "PRIMARY KEY";

                        if (!isForColumn)
                        {
                            definition += $" ({string.Join(",", constraint.ColumnNames)})";
                        }

                        break;
                    case ConstraintType.UniqueIndex:
                        definition = "UNIQUE";
                        
                        if(!isForColumn)
                        {
                            definition += $"({string.Join(",", constraint.ColumnNames)})";
                        }

                        break;
                    case ConstraintType.Check:
                        definition = $"CHECK {StringHelper.GetParenthesisedString(constraint?.Definition?.Symbol)}";
                        break;
                    case ConstraintType.ForeignKey:
                        ForeignKeyInfo fki = constraint.ForeignKey;

                        if (fki != null)
                        {
                            if (!isForColumn)
                            {
                                definition = $" FOREIGN KEY ({string.Join(",", fki.ColumnNames)})";
                            }

                            definition += $" REFERENCES {fki.RefTableName}({(string.Join(",", fki.RefColumNames))})";

                            if (fki.UpdateCascade)
                            {
                                definition += " UPDATE CASCADE";
                            }

                            if (fki.DeleteCascade)
                            {
                                definition += " DELETE CASCADE";
                            }
                        }

                        break;

                }

                if(this is MySqlStatementScriptBuilder && isForColumn)
                {
                    sb.Append($" {definition}");
                }
                else
                {
                    bool hasName = !string.IsNullOrEmpty(name);

                    if (hasName && isForColumn)
                    {
                        sb.Append($" {definition}");
                    }  
                    else
                    {
                        sb.Append($"{(hasName? "CONSTRAINT":"")} {(!hasName?"":$"{name} ")}{definition}".Trim());
                    }
                }                

                if (i < constaints.Count - 1)
                {
                    sb.AppendLine(",");
                }

                i++;
            }

            return sb.ToString();
        }       

        public void Dispose()
        {
            this.DeclareVariableStatements.Clear();
            this.DeclareCursorStatements.Clear();
            this.OtherDeclareStatements.Clear();
            this.SpecialStatements.Clear();
            this.Replacements.Clear();
            this.TemporaryTableNames.Clear();
        }
    }
}
