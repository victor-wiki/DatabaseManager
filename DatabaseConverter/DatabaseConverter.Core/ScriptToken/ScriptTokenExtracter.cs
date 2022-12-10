using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DatabaseConverter.Core
{
    public class ScriptTokenExtracter
    {
        public Statement Statement { get; set; }

        private List<TokenInfo> tokens = new List<TokenInfo>();

        public ScriptTokenExtracter(Statement statement)
        {
            this.Statement = statement;
        }

        public IEnumerable<TokenInfo> Extract()
        {
            this.tokens.Clear();

            this.ExtractTokens(this.Statement);

            return this.tokens;
        }

        private void ExtractTokens(dynamic obj, bool isFirst = true)
        {
            if(obj == null)
            {
                return;
            }

            Type type = obj.GetType();

            Action readProperties = () =>
            {
                var properties = type.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    if (property.Name == nameof(TokenInfo.Parent))
                    {
                        continue;
                    }                   

                    dynamic value = property.GetValue(obj);

                    if (value == null)
                    {
                        continue;
                    }

                    if (value is TokenInfo)
                    {
                        if (!value.Equals(obj))
                        {
                            this.ExtractTokens(value, false);
                        }
                    }
                    else if (value.GetType().IsClass && property.PropertyType.IsGenericType && !(property.DeclaringType == typeof(CommonScript) && property.Name == nameof(CommonScript.Functions)))
                    {
                        foreach (var v in value)
                        {
                            this.ExtractTokens(v, false);
                        }
                    }
                    else if (value is Statement || value is StatementItem || value is SelectTopInfo
                    || value is TableInfo || value is ColumnInfo || value is ConstraintInfo || value is ForeignKeyInfo )
                    {
                        this.ExtractTokens(value, false);
                    }
                }
            };

            if (obj is TokenInfo token)
            {
                this.AddToken(token);

                readProperties();

                return;
            }

            readProperties();
        }

        private void AddToken(TokenInfo token)
        {
            if (token == null)
            {
                return;
            }

            this.tokens.Add(token);
        }
    }
}
