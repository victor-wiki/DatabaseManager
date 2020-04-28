using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class StatementScriptBuilder
    {
        private StringBuilder sb = new StringBuilder();
        protected int level = 0;
        protected string indent => " ".PadLeft((this.level + 1) * 2);

        public StringBuilder Script => this.sb;

        protected void Append(string value, bool appendIndent = true)
        {
            sb.Append($"{(appendIndent ? this.indent : "")}{ value }");
        }

        protected void AppendLine(string value, bool appendIndent = true)
        {
            this.Append(value, appendIndent);
            this.sb.Append(Environment.NewLine);
        }

        protected void AppendChildStatements(IEnumerable<Statement> statements, bool needSeparator = true)
        {
            int childCount = statements.Count();

            if(childCount>0)
            {
                this.IncreaseLevel();
            }

            foreach (Statement statement in statements)
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

        private void IncreaseLevel()
        {
            this.level++;
        }

        private void DecreaseLevel()
        {
            this.level--;
        }

        public void TrimEnd(params char[] characters)
        {
            if (characters != null && characters.Length > 0)
            {
                while (this.sb.Length > 0 && characters.Contains(this.sb[this.sb.Length - 1]))
                {
                    this.sb.Remove(this.sb.Length - 1, 1);
                }
            }
        }

        public void TrimSeparator()
        {
            this.TrimEnd(';', '\r', '\n', ' ');
        }

        public override string ToString()
        {
            return this.sb.ToString();
        }

        protected void BuildSelectStatementFromItems(SelectStatement selectStatement)
        {
            int i = 0;

            bool hasJoins = false;

            foreach (FromItem fromItem in selectStatement.FromItems)
            {
                if (i == 0)
                {
                    this.Append("FROM ");
                }

                hasJoins = fromItem.JoinItems.Count > 0;

                if (i > 0 && !hasJoins)
                {
                    this.Append(",", false);
                }

                this.Append($"{fromItem.TableName}{(hasJoins ? Environment.NewLine : "")}", false);

                foreach (JoinItem joinItem in fromItem.JoinItems)
                {
                    string condition = joinItem.Condition == null ? "" : $" ON {joinItem.Condition}";

                    this.AppendLine($"{joinItem.Type} JOIN {joinItem.TableName}{condition}");
                }

                i++;
            }

            if (!hasJoins)
            {
                this.AppendLine("", false);
            }
        }
    }
}
