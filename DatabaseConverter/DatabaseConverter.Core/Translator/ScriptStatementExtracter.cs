using Antlr.Runtime;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DatabaseConverter.Core
{
    public class ScriptStatementExtracter
    {
        public CommonScript Script { get; set; }

        private List<Statement> statements = new List<Statement>();

        public ScriptStatementExtracter(CommonScript script)
        {
            this.Script = script;
        }

        public IEnumerable<Statement> Extract()
        {
            this.statements.Clear();

            this.ExtractStatements(this.Script);

            return this.statements;
        }

        private void ExtractStatements(dynamic obj)
        {
            Type type = obj.GetType();

            Action readProperties = () =>
            {
                var properties = type.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    dynamic value = property.GetValue(obj);

                    if (value == null)
                    {
                        continue;
                    }

                    if (value is Statement)
                    {
                        if (!value.Equals(obj))
                        {
                            this.ExtractStatements(value);
                        }
                    }
                    else if (value.GetType().IsClass && property.PropertyType.IsGenericType )
                    {
                        foreach (var v in value)
                        {
                            this.ExtractStatements(v);
                        }
                    }
                    else if (value is Statement || value is TemporaryTable || value is StatementItem)
                    {
                        this.ExtractStatements(value);
                    }
                }
            };

            if (obj is Statement statement)
            {
                this.AddStatement(statement);

                readProperties();

                return;
            }

            readProperties();
        }

        private void AddStatement(Statement statement)
        {
            if (statement == null)
            {
                return;
            }            

            this.statements.Add(statement);
        }
    }
}
